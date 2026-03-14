// src/pages/Results.tsx
import { Link, useLocation } from "react-router-dom";
import { useEffect, useState } from "react";
import RoundResultMap from "../components/RoundResultMap";

type ResultsState = {
  gameId: string;
};

type GameResult = {
  id: number;
  gameId: string;
  locationId: number;
  roundNumber: number;
  guessedLatitude: number;
  guessedLongitude: number;
  distanceKm: number;
  score: number;
  actualLatitude: number;
  actualLongitude: number;
};

export default function Results() {
  const location = useLocation();
  const state = location.state as ResultsState | undefined;

  const [results, setResults] = useState<GameResult[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  if (!state) {
    return (
      <main className="min-h-full text-white flex items-center justify-center px-4">
        <section className="w-full max-w-xl">
          <div className="bg-linear-to-r from-slate-800 to-blue-900 rounded-2xl p-8 border-2 border-blue-500 shadow-2xl shadow-blue-900/50">
            <h1 className="text-3xl font-extrabold mb-4 text-center tracking-tight">
              <span className="text-blue-300">Results</span>
            </h1>
            <p className="mb-6 text-sm text-blue-100 text-center">
              There&apos;s no results data to display. This page is meant to be
              opened at the end of a game.
            </p>
            <div className="flex flex-col sm:flex-row gap-3 justify-center">
              <Link
                to="/start"
                className="px-6 py-2.5 rounded-xl font-semibold
                           bg-linear-to-r from-blue-500 to-sky-400 text-slate-950
                           shadow-lg shadow-blue-900/40
                           hover:from-blue-400 hover:to-sky-300 transition text-center"
              >
                Play
              </Link>
              <Link
                to="/"
                className="px-6 py-2.5 rounded-xl font-semibold border border-blue-300/60
                           text-blue-100 bg-slate-900/40
                           hover:bg-slate-800/70 hover:border-blue-200 transition text-center"
              >
                Home
              </Link>
            </div>
          </div>
        </section>
      </main>
    );
  }

  const { gameId } = state;

  useEffect(() => {
    const fetchGameResults = async () => {
      try {
        setLoading(true);
        const res = await fetch(`/api/Guess/${gameId}`, {
          method: "GET",
          headers: { "Content-Type": "application/json" },
        });
        if (!res.ok) throw new Error("Failed to fetch game results");
        const data = await res.json();
        setResults(data);
      } catch (err) {
        setError(
          err instanceof Error ? err.message : "An error occurred while loading results"
        );
      } finally {
        setLoading(false);
      }
    };

    fetchGameResults();
  }, [gameId]);

  if (loading) {
    return (
      <main className="min-h-full text-white flex items-center justify-center px-4">
        <section className="w-full max-w-md">
          <div className="bg-linear-to-r from-slate-800 to-blue-900 rounded-2xl p-8 border-2 border-blue-500 shadow-2xl shadow-blue-900/50 text-center">
            <h1 className="text-2xl font-extrabold mb-3 tracking-tight">
              <span className="text-blue-300">Loading Results…</span>
            </h1>
            <p className="text-sm text-blue-100">
              Fetching your performance data for this game.
            </p>
          </div>
        </section>
      </main>
    );
  }

  if (error) {
    return (
      <main className="min-h-full text-white flex items-center justify-center px-4">
        <section className="w-full max-w-md">
          <div className="bg-linear-to-r from-slate-800 to-blue-900 rounded-2xl p-8 border-2 border-blue-500 shadow-2xl shadow-blue-900/50 text-center">
            <h1 className="text-3xl font-extrabold mb-3 tracking-tight text-red-200">
              Something went wrong
            </h1>
            <p className="text-sm text-red-200 mb-4">{error}</p>
            <div className="flex flex-col sm:flex-row gap-3 justify-center">
              <Link
                to="/start"
                className="px-6 py-2.5 rounded-xl font-semibold
                           bg-linear-to-r from-blue-500 to-sky-400 text-slate-950
                           shadow-lg shadow-blue-900/40
                           hover:from-blue-400 hover:to-sky-300 transition text-center"
              >
                Try Again
              </Link>
              <Link
                to="/"
                className="px-6 py-2.5 rounded-xl font-semibold border border-blue-300/60
                           text-blue-100 bg-slate-900/40
                           hover:bg-slate-800/70 hover:border-blue-200 transition text-center"
              >
                Home
              </Link>
            </div>
          </div>
        </section>
      </main>
    );
  }

  const totalScore = results.reduce((sum, result) => sum + result.score, 0);

  return (
    <main className="min-h-full text-white flex items-center justify-center px-4 py-6">
      <section className="w-full max-w-3xl">
        <div className="bg-linear-to-r from-slate-800 to-blue-900 rounded-2xl p-8 border-2 border-blue-500 shadow-2xl shadow-blue-900/50">
          {/* Header + summary */}
          <header className="mb-6">
            <h1 className="text-3xl font-extrabold mb-2 tracking-tight">
              <span className="text-blue-300">Game Results</span>
            </h1>
            <p className="text-sm text-blue-100">
              Great job! Here&apos;s how you performed this game.
            </p>
          </header>

          <div className="mb-6 grid gap-4 sm:grid-cols-2">
            <div className="rounded-xl bg-slate-900/70 border border-slate-700 px-4 py-3">
              <div className="text-xs uppercase tracking-wide text-blue-300/70 mb-1">
                Game ID
              </div>
              <div className="font-mono text-sm text-blue-100 break-all">
                {gameId}
              </div>
            </div>

            <div className="rounded-xl bg-slate-900/70 border border-slate-700 px-4 py-3">
              <div className="text-xs uppercase tracking-wide text-blue-300/70 mb-1">
                Total Score
              </div>
              <div className="text-xl font-bold text-green-400">
                {totalScore.toLocaleString()} pts
              </div>
            </div>
          </div>

          {/* Rounds List */}
          <h2 className="text-lg font-semibold text-blue-100 mb-3">
            Round Breakdown
          </h2>

          <div className="space-y-3 max-h-[420px] overflow-y-auto pr-1">
            {results.map((result, index) => (
              <div
                key={result.id ?? index}
                className="rounded-xl bg-slate-900/60 border border-slate-700 px-4 py-3"
              >
                <div className="flex gap-4">
                  {/* Left side - Text data */}
                  <div className="flex-shrink-0 flex-1">
                    <div className="flex justify-between items-center mb-2">
                      <h3 className="text-sm font-semibold text-blue-100">
                        Round {index + 1}
                      </h3>
                      <span className="text-xs bg-slate-800/80 px-2 py-1 rounded-full text-blue-200">
                        {result.distanceKm.toFixed(2)} km away
                      </span>
                    </div>
                    <div className="text-sm text-blue-100 space-y-1">
                      <div>
                        Score:{" "}
                        <span className="font-semibold text-green-400">
                          {result.score}
                        </span>{" "}
                        pts
                      </div>
                      <div className="text-xs text-blue-200 mt-1">
                        <div>
                          Actual:{" "}
                          <span className="font-mono">
                            {result.actualLatitude.toFixed(5)},{" "}
                            {result.actualLongitude.toFixed(5)}
                          </span>
                        </div>
                        <div>
                          Your guess:{" "}
                          <span className="font-mono">
                            {result.guessedLatitude.toFixed(5)},{" "}
                            {result.guessedLongitude.toFixed(5)}
                          </span>
                        </div>
                      </div>
                    </div>
                  </div>

                  {/* Right side - Map */}
                  <div className="flex-1">
                    <RoundResultMap
                      actual={{ lat: result.actualLatitude, lng: result.actualLongitude }}
                      guess={{ lat: result.guessedLatitude, lng: result.guessedLongitude }}
                      distanceKm={result.distanceKm}
                    />
                  </div>
                </div>
              </div>
            ))}
          </div>

          {/* Actions */}
          <div className="mt-6 flex flex-col sm:flex-row gap-3 justify-end">
            <Link
              to="/start"
              className="px-6 py-2.5 rounded-xl font-semibold
                         bg-linear-to-r from-blue-500 to-sky-400 text-slate-950
                         shadow-lg shadow-blue-900/40
                         hover:from-blue-400 hover:to-sky-300 transition text-center"
            >
              Play Again
            </Link>
            <Link
              to="/"
              className="px-6 py-2.5 rounded-xl font-semibold border border-blue-300/60
                         text-blue-100 bg-slate-900/40
                         hover:bg-slate-800/70 hover:border-blue-200 transition text-center"
            >
              Home
            </Link>
          </div>
        </div>
      </section>
    </main>
  );
}
