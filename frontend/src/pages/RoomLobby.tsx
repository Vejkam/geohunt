import { useEffect, useState, useRef, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import * as signalR from '@microsoft/signalr';

interface Player {
  id: string;
  userId: string;
  displayName: string;
  isReady: boolean;
}

export default function RoomLobby() {
  const { roomCode } = useParams<{ roomCode: string }>();
  const navigate = useNavigate();

  const [players, setPlayers] = useState<Player[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [userId, setUserId] = useState('');
  const [myPlayerId, setMyPlayerId] = useState('');
  const [myDisplayName, setMyDisplayName] = useState('');
  const [roomHubConnected, setRoomHubConnected] = useState(false);
  const [gameHubConnected, setGameHubConnected] = useState(false);

  const roomHubRef = useRef<signalR.HubConnection | null>(null);
  const gameHubRef = useRef<signalR.HubConnection | null>(null);
  const isNavigatingRef = useRef(false);

  const navigateToGame = useCallback((gameId: string) => {
    isNavigatingRef.current = true;
    navigate(`/multiplayer/${roomCode}/${gameId}`);
  }, [navigate, roomCode]);

  // Fetch current user
  useEffect(() => {
    const loadUser = async () => {
      try {
        const res = await fetch("/api/User/me", { credentials: "include" });
        if (!res.ok) throw new Error("User not logged in");
        const data = await res.json();
        setUserId(data.id);
      } catch (err) {
        console.error(err);
        setError("Failed to fetch user");
      } finally {
        setLoading(false);
      }
    };
    loadUser();
  }, []);

  // Fetch players
  useEffect(() => {
    if (!userId || !roomCode) return;
    const loadPlayers = async () => {
      try {
        const res = await fetch(`/api/Rooms/${roomCode}/players`, { credentials: "include" });
        if (!res.ok) throw new Error("Failed to load players");
        const list: Player[] = await res.json();
        setPlayers(list);
        const me = list.find((p) => p.userId === userId);
        if (me) {
          setMyPlayerId(me.id);
          setMyDisplayName(me.displayName);
        }
      } catch (err) {
        console.error(err);
        setError("Failed to load players");
      }
    };
    loadPlayers();
  }, [userId, roomCode]);

  useEffect(() => {
    isNavigatingRef.current = false;
    if (userId && roomCode) {
      const reloadPlayers = async () => {
        try {
          const res = await fetch(`/api/Rooms/${roomCode}/players`, { credentials: "include" });
          if (res.ok) setPlayers(await res.json());
        } catch (err) {
          console.error("Failed to reload players:", err);
        }
      };
      reloadPlayers();
    }
  }, [userId, roomCode]);

  useEffect(() => {
    if (!roomCode || !myPlayerId || !myDisplayName) return;
    if (roomHubRef.current) return;

    const hub = new signalR.HubConnectionBuilder()
      .withUrl("http://localhost:5041/roomHub", { withCredentials: true })
      .withAutomaticReconnect()
      .build();

    roomHubRef.current = hub;

    hub.onclose(() => setRoomHubConnected(false));
    hub.onreconnecting(() => setRoomHubConnected(false));
    hub.onreconnected(async () => {
      setRoomHubConnected(true);
      try {
        await hub.invoke("JoinRoom", roomCode, myPlayerId, myDisplayName);
      } catch (err) { console.error(err); }
    });

    hub.on("PlayerListUpdated", setPlayers);
    hub.on("GameStarted", navigateToGame);

    const start = async () => {
      try {
        await hub.start();
        setRoomHubConnected(true);
        await hub.invoke("JoinRoom", roomCode, myPlayerId, myDisplayName);
      } catch (err) {
        console.error("RoomHub connection error:", err);
        setRoomHubConnected(false);
      }
    };
    start();

    return () => {
      if (isNavigatingRef.current) return;
      const cleanup = async () => {
        if (hub.state === signalR.HubConnectionState.Connected) {
          try { await hub.invoke("LeaveRoom", roomCode); } catch {} 
        }
        await hub.stop();
      };
      cleanup();
    };
  }, [roomCode, myPlayerId, myDisplayName, navigateToGame]);

  // Connect to GameHub
  useEffect(() => {
    if (!roomCode || !myPlayerId) return;
    if (gameHubRef.current) return;

    const hub = new signalR.HubConnectionBuilder()
      .withUrl("http://localhost:5041/gameHub", { withCredentials: true })
      .withAutomaticReconnect()
      .build();

    gameHubRef.current = hub;

    hub.onclose(() => setGameHubConnected(false));
    hub.onreconnecting(() => setGameHubConnected(false));
    hub.onreconnected(async () => {
      setGameHubConnected(true);
      if (roomCode) await hub.invoke("JoinGameRoom", roomCode).catch(console.error);
    });
    hub.on("GameStarted", navigateToGame);

    const start = async () => {
      try {
        await hub.start();
        setGameHubConnected(true);
        await hub.invoke("JoinGameRoom", roomCode);
      } catch (err) {
        console.error("GameHub connection error:", err);
        setGameHubConnected(false);
      }
    };
    start();

    return () => {
      if (isNavigatingRef.current) return;
      hub.stop();
    };
  }, [roomCode, myPlayerId, navigateToGame]);

  const handleToggleReady = async () => {
    if (!myPlayerId || !roomHubConnected) return setError("Not connected to server");
    try {
      const res = await fetch(`/api/Players/${myPlayerId}/toggle-ready`, { method: "POST", credentials: "include" });
      if (!res.ok) throw new Error();
      const updated: Player = await res.json();
      setPlayers(prev => prev.map(p => p.id === myPlayerId ? updated : p));
      await roomHubRef.current?.invoke("UpdateReadyState", roomCode, myPlayerId, updated.isReady);
    } catch { setError("Failed to toggle ready"); }
  };

  const handleStartGame = async () => {
    if (!gameHubConnected) return setError("Not connected to game server");
    try { await gameHubRef.current?.invoke("StartGame", roomCode); } 
    catch { setError("Failed to start game"); }
  };

  const handleLeaveRoom = async () => {
    try { await fetch(`/api/Players/${myPlayerId}/leave-room`, { method: "POST", credentials: "include" }); } catch {}
    roomHubRef.current?.stop();
    gameHubRef.current?.stop();
    navigate("/joinroom");
  };

  const allReady = players.length > 0 && players.every(p => p.isReady);
  const myPlayer = players.find(p => p.id === myPlayerId);

  if (loading) return <p className="text-white">Loading room...</p>;
  if (error) return <p className="text-red-400">{error}</p>;

  return (
    <main className="min-h-full text-white flex items-center justify-center relative px-4 py-15">
      <section className="w-full max-w-3xl">
        <div className="bg-linear-to-r from-slate-800 to-blue-900 rounded-2xl p-8 md:p-10 border-2 border-blue-500 shadow-2xl shadow-blue-900/50">
          <header className="text-center mb-8">
            <h1 className="text-4xl md:text-5xl font-extrabold mb-3 tracking-tight">
              Room {roomCode}
            </h1>
            <p className="text-base md:text-lg text-blue-200 max-w-2xl mx-auto">
              {players.length} {players.length === 1 ? "player" : "players"} in the lobby
            </p>
          </header>

          <div className="mb-4 text-center">
            <span className={roomHubConnected ? "text-green-400" : "text-yellow-400"}>
              RoomHub: {roomHubConnected ? "Connected" : "Connecting..."}
            </span>
            {" | "}
            <span className={gameHubConnected ? "text-green-400" : "text-yellow-400"}>
              GameHub: {gameHubConnected ? "Connected" : "Connecting..."}
            </span>
          </div>

          <div className="grid grid-cols-2 sm:grid-cols-4 gap-4 mb-6">
            {players.map((player) => (
              <div key={player.id} className="flex flex-col items-center p-4 bg-white/10 rounded-xl">
                <span className="text-4xl">🙂</span>
                <span className="mt-2">{player.displayName}</span>
                <span className="text-sm mt-1">{player.isReady ? "✅ Ready" : "❌ Not Ready"}</span>
              </div>
            ))}
          </div>

          <div className="flex flex-col sm:flex-row gap-4 justify-center">
            <button
              onClick={handleToggleReady}
              disabled={!roomHubConnected}
              className={`px-6 py-3 rounded-xl font-semibold transition ${
                roomHubConnected ? "bg-green-600 hover:bg-green-700" : "bg-gray-500 cursor-not-allowed"
              }`}
            >
              {myPlayer?.isReady ? "Unready" : "Get Ready"}
            </button>

            <button
              onClick={handleStartGame}
              disabled={!allReady || !gameHubConnected}
              className={`px-6 py-3 rounded-xl font-semibold transition ${
                allReady && gameHubConnected ? "bg-blue-600 hover:bg-blue-700" : "bg-gray-500 cursor-not-allowed"
              }`}
            >
              Start Game
            </button>

            <button
              onClick={handleLeaveRoom}
              className="px-6 py-3 rounded-xl font-semibold bg-red-600 hover:bg-red-700 transition"
            >
              Leave Room
            </button>
          </div>
        </div>
      </section>
    </main>
  );
}
