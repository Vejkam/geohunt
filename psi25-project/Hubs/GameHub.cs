using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using psi25_project.Data;
using psi25_project.Models.Dtos;
using psi25_project.Services.Interfaces;


namespace psi25_project.Hubs
{
    public class GameHub : Hub
    {
        private readonly IMultiplayerGameService _gameService;
        private readonly GeoHuntContext _context;

        public GameHub(IMultiplayerGameService gameService, GeoHuntContext context)
        {
            _gameService = gameService;
            _context = context;
        }

        public async Task JoinGameRoom(string roomCode)
        {
            var room = await _context.Rooms
                .FirstOrDefaultAsync(r => r.RoomCode == roomCode);
            
            if (room == null)
                throw new HubException("Room not found");

            var roomId = room.Id;

            await Groups.AddToGroupAsync(Context.ConnectionId, roomId.ToString());
            
            Console.WriteLine($"Player joined GameHub group: {roomId}");
        }

        public async Task StartGame(string roomCode)
        {
            var room = await _context.Rooms
                .FirstOrDefaultAsync(r => r.RoomCode == roomCode);
            
            if (room == null)
                throw new HubException("Room not found");

            var roomId = room.Id;

            var gameDto = await _gameService.StartGameAsync(roomId);

            Console.WriteLine($"Broadcasting GameStarted to group: {roomId}, gameId: {gameDto.GameId}");

            await Clients.Group(roomId.ToString())
                .SendAsync("GameStarted", gameDto.GameId.ToString());

            await Clients.Group(roomId.ToString())
                .SendAsync("RoundStarted", new
                {
                    gameId = gameDto.GameId.ToString(),
                    currentRound = gameDto.CurrentRound,
                    totalRounds = gameDto.TotalRounds,   
                    roundLatitude = gameDto.RoundLatitude,
                    roundLongitude = gameDto.RoundLongitude
                });

            await Clients.Group(roomId.ToString())
                .SendAsync("GameStateUpdated", gameDto);
        }

        public async Task SubmitGuess(Guid playerId, double latitude, double longitude)
        {
            var result = await _gameService.SubmitGuessAsync(playerId, latitude, longitude);

            var player = await _gameService.GetCurrentGameForPlayerAsync(playerId);
            if (player == null) return;

            var roomId = player.Game.RoomId;

            await Clients.Group(roomId.ToString())
                .SendAsync("RoundResult", result);

            var game = await _gameService.GetCurrentGameForRoomAsync(roomId);
            if (game == null) return;

            var gameDto = new MultiplayerGameDto
            {
                GameId = game.Id,
                RoomId = game.RoomId,
                CurrentRound = game.CurrentRound,
                TotalRounds = game.TotalRounds,
                RoundLatitude = game.RoundLatitude,
                RoundLongitude = game.RoundLongitude,
                Players = game.Players.Select(p => new MultiplayerPlayerDto
                {
                    PlayerId = p.PlayerId,
                    DisplayName = p.Player?.DisplayName ?? "Unknown",
                    Score = p.Score,
                    Finished = p.Finished,
                    LastGuessLatitude = p.LastGuessLatitude,
                    LastGuessLongitude = p.LastGuessLongitude,
                    DistanceMeters = p.DistanceMeters
                }).ToList()
            };

            await Clients.Group(roomId.ToString())
                .SendAsync("GameStateUpdated", gameDto);

            var allFinished = game.Players.All(p => p.Finished);
            
            if (allFinished)
            {
                Console.WriteLine("All players finished the round!");
                
                await Task.Delay(2000);

                if (game.CurrentRound < game.TotalRounds)
                {
                    Console.WriteLine($"Starting next round {game.CurrentRound + 1}");
                    
                    var nextRound = await _gameService.NextRoundAsync(game.Id);
                    
                    await Clients.Group(roomId.ToString())
                        .SendAsync("GameStateUpdated", nextRound);
                    
                    await Clients.Group(roomId.ToString())
                        .SendAsync("RoundStarted", new
                        {
                            gameId = nextRound.GameId.ToString(),
                            currentRound = nextRound.CurrentRound,
                            totalRounds = nextRound.TotalRounds,
                            roundLatitude = nextRound.RoundLatitude,
                            roundLongitude = nextRound.RoundLongitude
                        });
                    
                    Console.WriteLine($"✅ Round {nextRound.CurrentRound} started successfully");
                }
                else
                {
                    Console.WriteLine("All rounds completed - ending game");
                    
                    var finishedGame = await _gameService.EndGameAsync(game.Id);
                    
                    await Clients.Group(roomId.ToString())
                        .SendAsync("GameFinished", finishedGame);
                }
            }
        }
        public async Task GetCurrentGame(Guid roomId)
        {
            var game = await _gameService.GetCurrentGameForRoomAsync(roomId);
            if (game == null) return;

            await Groups.AddToGroupAsync(Context.ConnectionId, roomId.ToString());

            await Clients.Caller.SendAsync("GameStateUpdated", game);
        }

        public async Task JoinGame(string roomCode, string gameId)
        {
            var room = await _context.Rooms
                .FirstOrDefaultAsync(r => r.RoomCode == roomCode);
            
            if (room == null)
                throw new HubException("Room not found");

            var roomId = room.Id;
            
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId.ToString());
            
            Console.WriteLine($"Player joined game: {gameId} in room: {roomCode}");
            
            var game = await _gameService.GetCurrentGameForRoomAsync(roomId);
            if (game != null)
            {
                var gameDto = new MultiplayerGameDto
                {
                    GameId = game.Id,
                    RoomId = game.RoomId,
                    CurrentRound = game.CurrentRound,
                    TotalRounds = game.TotalRounds,
                    RoundLatitude = game.RoundLatitude,
                    RoundLongitude = game.RoundLongitude,
                    Players = game.Players.Select(p => new MultiplayerPlayerDto
                    {
                        PlayerId = p.PlayerId,
                        DisplayName = p.Player?.DisplayName ?? "Unknown",
                        Score = p.Score,
                        Finished = p.Finished,
                        LastGuessLatitude = p.LastGuessLatitude,
                        LastGuessLongitude = p.LastGuessLongitude,
                        DistanceMeters = p.DistanceMeters
                    }).ToList()
                };

                await Clients.Caller.SendAsync("GameStateUpdated", gameDto);
                
                if (game.CurrentRound > 0 && game.RoundLatitude.HasValue && game.RoundLongitude.HasValue)
                {
                    await Clients.Caller.SendAsync("RoundStarted", new
                    {
                        gameId = game.Id.ToString(),
                        currentRound = game.CurrentRound,
                        totalRounds = game.TotalRounds,
                        roundLatitude = game.RoundLatitude.Value,
                        roundLongitude = game.RoundLongitude.Value
                    });
                }
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}