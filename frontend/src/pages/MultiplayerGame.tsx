import { useEffect, useRef, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import * as signalR from "@microsoft/signalr";
import MiniMap from "../components/MiniMap";

declare global {
  interface Window {
    google: any;
  }
}

interface Coordinates {
  lat: number;
  lng: number;
}

interface RoundData {
  gameId: string;
  currentRound: number;
  totalRounds: number;
  roundLatitude: number;
  roundLongitude: number;
}

interface PlayerScore {
  playerId: string;
  displayName: string;
  score: number;
  finished: boolean;
}

interface RoundResult {
  playerId: string;
  roundNumber: number;
  score: number;
  distanceMeters: number;
  totalScore: number;
}

interface MultiplayerGameState {
  gameId: string;
  currentRound: number;
  totalRounds: number;
  players: PlayerScore[];
}

export default function MultiplayerGame() {
  const { roomCode, gameId } = useParams<{ roomCode: string; gameId: string }>();
  const navigate = useNavigate();

  const streetViewRef = useRef<HTMLDivElement>(null);
  const panoRef = useRef<any>(null);
  const connectionRef = useRef<signalR.HubConnection | null>(null);
  const isInitializedRef = useRef(false);
  const pendingRoundDataRef = useRef<RoundData | null>(null);

  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [userId, setUserId] = useState<string>("");
  const [myPlayerId, setMyPlayerId] = useState<string>("");

  const [roundData, setRoundData] = useState<RoundData | null>(null);
  const [selectedCoords, setSelectedCoords] = useState<Coordinates | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const [hasSubmitted, setHasSubmitted] = useState(false);

  const [gameState, setGameState] = useState<MultiplayerGameState | null>(null);
  const [waitingForOthers, setWaitingForOthers] = useState(false);
  const [gameComplete, setGameComplete] = useState(false);

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
      }
    };
    loadUser();
  }, []);

  useEffect(() => {
    if (!userId || !roomCode) return;

    const loadPlayer = async () => {
      try {
        const res = await fetch(`/api/Rooms/${roomCode}/players`, {
          credentials: "include",
        });
        if (!res.ok) throw new Error("Failed to load player");

        const players = await res.json();
        const me = players.find((p: any) => p.userId === userId);
        if (me) setMyPlayerId(me.id);
      } catch (err) {
        console.error(err);
        setError("Failed to load player info");
      }
    };

    loadPlayer();
  }, [userId, roomCode]);

  const waitGoogleMaps = () =>
    new Promise<void>((resolve, reject) => {
      const start = Date.now();
      const tick = () => {
        if (window.google?.maps) return resolve();
        if (Date.now() - start > 15000)
          return reject(new Error("Google Maps API not loaded"));
        setTimeout(tick, 100);
      };
      tick();
    });

  const updateStreetView = async (lat: number, lng: number) => {
    try {
      await waitGoogleMaps();

      if (!streetViewRef.current) {
        console.error("Street View container not ready");
        throw new Error("Street View container not mounted");
      }
      
      const position = { lat, lng };

      if (!panoRef.current) {
        console.log("Creating new Street View panorama");
        panoRef.current = new window.google.maps.StreetViewPanorama(
          streetViewRef.current,
          {
            position,
            pov: { heading: 0, pitch: 0 },
            zoom: 1,
            addressControl: false,
            fullscreenControl: false,
            imageDateControl: false,
            showRoadLabels: false,
          }
        );
      } else {
        console.log("Updating existing Street View panorama");
        panoRef.current.setPosition(position);
        panoRef.current.setPov({ heading: 0, pitch: 0 });
        panoRef.current.setZoom(1);
      }
    } catch (err) {
      console.error("Failed to update Street View:", err);
      throw err;
    }
  };

  useEffect(() => {
    if (!roomCode || !gameId || !myPlayerId) return;
    if (isInitializedRef.current) return;
    
    isInitializedRef.current = true;

    const connection = new signalR.HubConnectionBuilder()
      .withUrl("http://localhost:5041/gameHub", { withCredentials: true })
      .withAutomaticReconnect()
      .build();

    connectionRef.current = connection;

    connection.onclose(() => {
      console.log("GameHub disconnected");
    });

    connection.onreconnecting(() => {
      console.log("GameHub reconnecting...");
    });

    connection.onreconnected(async () => {
      console.log("GameHub reconnected");
      try {
        await connection.invoke("JoinGame", roomCode, gameId);
        console.log("Rejoined game after reconnection");
      } catch (err) {
        console.error("Failed to rejoin game:", err);
      }
    });

    connection.on("RoundStarted", async (data: RoundData) => {
      console.log("🎮 RoundStarted received:", data);
      console.log("📍 Previous round:", roundData?.currentRound, "→ New round:", data.currentRound);
      
      pendingRoundDataRef.current = data;
      setRoundData(data);
      setSelectedCoords(null);
      setHasSubmitted(false);
      setWaitingForOthers(false);
      setGameComplete(false);
      setLoading(true);
      
      console.log("✅ State reset for new round");
      
      setGameState((prev) => {
        if (!prev) return prev;
        const newState = {
          ...prev,
          currentRound: data.currentRound,
          players: prev.players.map((p) => ({
            ...p,
            finished: false, 
          })),
        };
        console.log("🔄 Updated game state:", newState);
        return newState;
      });
    });

    connection.on("GameStateUpdated", (state: MultiplayerGameState) => {
      console.log("GameStateUpdated:", state);
      console.log("Current roundData:", roundData);
      setGameState(state);
      
      if (state.players.every(p => !p.finished)) {
        console.log("New round detected - all players not finished yet");
        setWaitingForOthers(false);
      }
    });

    connection.on("RoundResult", (result: RoundResult) => {
      console.log("RoundResult:", result);
      
      setGameState((prev) => {
        if (!prev) return prev;
        return {
          ...prev,
          players: prev.players.map((p) =>
            p.playerId === result.playerId
              ? { ...p, score: result.score, finished: true }
              : p
          ),
        };
      });
    });

    connection.on("GameFinished", (finalState: MultiplayerGameState) => {
      console.log("GameFinished:", finalState);
      
      setGameState(finalState);
      setWaitingForOthers(false);
      setGameComplete(true);
      setLoading(false);
      
      setTimeout(async () => {
        if (connectionRef.current) {
          await connectionRef.current.stop();
          connectionRef.current = null;
        }
        navigate(`/room/${roomCode}`);
      }, 3000);
    });

    const startConnection = async () => {
      try {
        await connection.start();
        console.log("Connected to GameHub for game");

        await connection.invoke("JoinGame", roomCode, gameId);
        console.log("Joined game successfully");
      } catch (err) {
        console.error("GameHub connection error:", err);
        setError("Failed to connect to game server");
        setLoading(false);
      }
    };

    startConnection();

    return () => {
      console.log("Cleaning up GameHub connection");
      connection.stop();
    };
  }, [roomCode, gameId, myPlayerId, navigate]);

  useEffect(() => {
    if (!roundData) return;
    
    const timer = setTimeout(async () => {
      if (!streetViewRef.current) {
        console.error("Street View container still not ready after delay");
        setError("Failed to initialize Street View");
        setLoading(false);
        return;
      }
      
      try {
        console.log("Initializing Street View with coordinates:", {
          lat: roundData.roundLatitude,
          lng: roundData.roundLongitude
        });
        await updateStreetView(roundData.roundLatitude, roundData.roundLongitude);
        setLoading(false);
        pendingRoundDataRef.current = null;
      } catch (err) {
        console.error("Failed to load Street View:", err);
        setError("Failed to load Street View location");
        setLoading(false);
      }
    }, 100);

    return () => clearTimeout(timer);
  }, [roundData]);

  const handleSubmitGuess = async () => {
    if (!selectedCoords || !roundData || !connectionRef.current || !myPlayerId) {
      console.error("Missing required data for guess submission");
      return;
    }

    try {
      setSubmitting(true);

      await connectionRef.current.invoke(
        "SubmitGuess",
        myPlayerId,
        selectedCoords.lat,
        selectedCoords.lng
      );

      console.log("Guess submitted successfully");
      setHasSubmitted(true);
      setSelectedCoords(null);
      
      setWaitingForOthers(true);
    } catch (err) {
      console.error("Failed to submit guess:", err);
      setError("Failed to submit guess");
    } finally {
      setSubmitting(false);
    }
  };

  if (error) {
    return (
      <div className="flex items-center justify-center h-screen bg-gray-900">
        <div className="bg-white p-8 rounded-lg shadow-lg max-w-md">
          <h2 className="text-red-600 text-xl font-bold mb-2">Error</h2>
          <p className="text-gray-700">{error}</p>
          <button
            onClick={() => navigate(`/room/${roomCode}`)}
            className="mt-4 px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
          >
            Back to Lobby
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="relative h-screen w-screen">
      {/* Street View container - ALWAYS render this */}
      <div ref={streetViewRef} className="h-full w-full" />

      {/* Loading overlay */}
      {loading && (
        <div className="absolute inset-0 flex items-center justify-center bg-gray-900 z-50">
          <div className="text-center">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-white mx-auto mb-4"></div>
            <p className="text-white">Loading round...</p>
          </div>
        </div>
      )}

      {/* Only show UI elements when we have data */}
      {roundData && gameState && (
        <>
          {/* Round indicator */}
          <div className="absolute top-3 left-3 z-50 rounded bg-black/60 text-white px-3 py-2 text-sm">
            Round {roundData.currentRound} / {roundData.totalRounds}
          </div>

          {/* Player scores */}
          <div className="absolute top-3 right-3 z-50 rounded bg-black/60 text-white px-3 py-2 text-sm max-w-xs">
            <div className="font-bold mb-1">Players:</div>
            {gameState.players.map((player) => (
              <div key={player.playerId} className="flex justify-between gap-3">
                <span className={player.playerId === myPlayerId ? "text-yellow-400" : ""}>
                  {player.displayName}
                </span>
                <span>
                  {player.score} pts {player.finished && "✅"}
                </span>
              </div>
            ))}
          </div>

          {/* Game Complete overlay */}
          {gameComplete && (
            <div className="absolute inset-0 flex items-center justify-center bg-black/60 backdrop-blur-md z-50">
              <div className="bg-white/10 border border-white/20 text-white p-8 rounded-2xl shadow-2xl max-w-md w-full text-center">
                <div className="text-6xl mb-4">🎉</div>
                <h2 className="text-3xl font-bold mb-2">Game Complete!</h2>
                <p className="text-white/80 mb-6">Final Scores</p>

                <div className="space-y-2 mb-6">
                  {gameState.players
                    .sort((a, b) => b.score - a.score)
                    .map((player, index) => (
                      <div
                        key={player.playerId}
                        className={`flex items-center justify-between p-3 rounded-xl ${
                          index === 0 ? "bg-yellow-500/20 border border-yellow-400/30" : "bg-white/10 border border-white/10"
                        }`}
                      >
                        <span className="flex items-center gap-2">
                          {index === 0 && "🏆"}
                          <span className={player.playerId === myPlayerId ? "font-bold" : ""}>
                            {player.displayName}
                          </span>
                        </span>
                        <span className="font-bold">{player.score} pts</span>
                      </div>
                    ))}
                </div>

                <p className="text-sm text-white/60">Returning to lobby...</p>
              </div>
            </div>
          )}


          {/* Waiting overlay */}
          {waitingForOthers && (
            <div className="absolute inset-0 flex items-center justify-center bg-black/50 backdrop-blur-md z-40">
              <div className="bg-white/10 border border-white/20 text-white p-6 rounded-2xl shadow-xl text-center max-w-sm w-full">
                
                <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-400 mx-auto mb-4"></div>

                <p className="font-semibold text-lg">
                  {gameState.players.every(p => p.finished)
                    ? "All players finished! Loading next round..."
                    : "Waiting for other players…"}
                </p>

                <p className="text-white/70 text-sm mt-2">
                  {gameState.players.filter(p => p.finished).length} / {gameState.players.length} finished
                </p>

                <div className="mt-4 space-y-1">
                  {gameState.players.map((player) => (
                    <div key={player.playerId} className="text-sm flex items-center justify-between">
                      <span className={player.playerId === myPlayerId ? "font-bold text-blue-300" : ""}>
                        {player.displayName}
                      </span>
                      <span>{player.finished ? "✅" : "⏳"}</span>
                    </div>
                  ))}
                </div>

              </div>
            </div>
          )}


          {/* Mini map + submit */}
          <div className="absolute bottom-4 right-4 flex flex-col gap-2" style={{ zIndex: 9999 }}>
            <MiniMap
              initialZoom={1}
              onSelect={(coords) => setSelectedCoords(coords)}
              className="overflow-hidden shadow-lg rounded-xl"
              style={{ width: 300, height: 200 }}
            />
            <button
              onClick={handleSubmitGuess}
              disabled={!selectedCoords || submitting || hasSubmitted}
              className="px-3 py-3 bg-blue-600 text-white font-semibold rounded-xl shadow hover:bg-blue-700 transition disabled:opacity-60 disabled:cursor-not-allowed"
            >
              {submitting
                ? "Submitting…"
                : hasSubmitted
                ? "Submitted!"
                : selectedCoords
                ? "Submit Guess"
                : "Select a location on the map"}
            </button>
          </div>
        </>
      )}
    </div>
  );
}