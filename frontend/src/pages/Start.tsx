import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";

type CreateGameDto = {
  userId: string;
  totalRounds: number;
};

type GameResponseDto = {
  id: string;
  userId: string;
  totalScore: number;
  startedAt: string;
  finishedAt: string | null;
  currentRound: number;
  totalRounds: number;
};

export default function Start() {
  const [rounds, setRounds] = useState(3);
  const [loading, setLoading] = useState(false);
  const [err, setErr] = useState<string | null>(null);
  const [userId, setUserId] = useState<string>("");
  const navigate = useNavigate();

  useEffect(() => {
    getUser();
  }, []);

  const getUser = async () => {
    try {
      const res = await fetch("/api/User/me", {
        method: "GET",
        credentials: "include",
      });
      if (res.ok) {
        const data = await res.json();
        setUserId(data.id);
      } else {
        navigate("/login", { replace: true });
      }
    } catch (error) {
      console.error("Error checking login status:", error);
      navigate("/login", { replace: true });
    }
  };

  const handleStart = async () => {
    setErr(null);
    setLoading(true);
    getUser();

    try {
      const payload: CreateGameDto = {
        userId,
        totalRounds: rounds,
      };

      const res = await fetch("/api/Game", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(payload),
      });

      if (!res.ok) {
        const text = await res.text();
        throw new Error(text || `Failed to start game (HTTP ${res.status})`);
      }

      const game: GameResponseDto = await res.json();

      navigate("/game", {
        state: {
          gameId: game.id,
          totalRounds: game.totalRounds,
          currentRound: game.currentRound,
        },
        replace: true,
      });
    } catch (e) {
      setErr((e as Error).message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <main className="min-h-full text-white flex items-center justify-center px-4 py-15">
      <section className="w-full max-w-md">
        <div className="bg-linear-to-r from-slate-800 to-blue-900 p-8 rounded-2xl border-2 border-blue-500 shadow-2xl shadow-blue-900/50">
          <h1 className="text-3xl font-extrabold text-center mb-6 tracking-tight">
            <span className="text-blue-300">Start a Game</span>
          </h1>

          {/* ROUND SELECT */}
          <label className="block mb-2 text-blue-200 font-medium">
            Number of Rounds
          </label>
          <select
            value={rounds}
            onChange={(e) => setRounds(Number(e.target.value))}
            disabled={loading}
            className="w-full cursor-pointer px-4 py-2 mb-6 rounded-lg bg-slate-900/70 border border-slate-700 text-blue-50 
                       focus:outline-none focus:ring-2 focus:ring-blue-500/40 focus:border-blue-400 transition"
          >
            {[1, 2, 3, 4, 5].map((n) => (
              <option key={n} value={n} className="text-black">
                {n}
              </option>
            ))}
          </select>

          {/* ACTIONS */}
          <div className="flex flex-col gap-3">
            <button
              onClick={handleStart}
              disabled={loading}
              className={`w-full px-6 py-3 rounded-xl font-semibold
                          bg-linear-to-r from-blue-500 to-sky-400 text-slate-950
                          shadow-lg shadow-blue-900/40
                          hover:from-blue-400 hover:to-sky-300 transition
                          disabled:opacity-60 disabled:cursor-not-allowed`}
            >
              {loading ? "Starting…" : "Start Game"}
            </button>

            {err && (
              <div className="text-sm text-red-400 text-center bg-red-900/40 border border-red-500/40 py-2 px-3 rounded-lg">
                {err}
              </div>
            )}
          </div>
        </div>
      </section>
    </main>
  );
}
