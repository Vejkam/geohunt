import { User, Lock } from "lucide-react";
import { useState } from "react";
import { useNavigate } from "react-router-dom";

export default function Login() {
  const navigate = useNavigate();
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");

  const handleLogin = async () => {
    try {
      setError("");

      const res = await fetch("/api/Account/login", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({ username, password }),
      });

      if (res.ok) {
        navigate("/");
      } else {
        const data = await res.json();
        setError(
          data.message || "Login failed. Please check your credentials."
        );
      }
    } catch (err) {
      console.error("Login error:", err);
      setError("An error occurred. Please try again.");
    }
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    handleLogin();
  };

  return (
    <main className="min-h-full text-white flex items-center justify-center px-4 py-15">
      <section className="w-full max-w-md">
        <div className="bg-linear-to-r from-slate-800 to-blue-900 rounded-2xl p-8 border-2 border-blue-500 shadow-2xl shadow-blue-900/50">
          {/* Branding */}
          <header className="text-center mb-6">
            <h1 className="text-4xl font-extrabold tracking-tight mb-2">
              <span className="text-blue-300">GeoHunt</span>
            </h1>
            <p className="text-sm text-blue-200">
              Log in to continue your journey across the globe 🌍
            </p>
          </header>

          <form onSubmit={handleSubmit} className="flex flex-col items-center">
            {/* Error Message */}
            {error && (
              <div className="mb-4 w-full p-3 rounded-lg border border-red-500/70 bg-red-900/60 text-red-100 text-sm">
                {error}
              </div>
            )}

            {/* Username */}
            <div className="relative mb-4 w-full">
              <User className="absolute left-3 top-1/2 -translate-y-1/2 text-blue-200/80" size={20} />
              <input
                type="text"
                placeholder="Username"
                className="w-full pl-10 pr-4 py-2 rounded-lg border border-slate-700 bg-slate-900/70 text-blue-50 placeholder-blue-200/40 
                           focus:outline-none focus:border-blue-400 focus:ring-2 focus:ring-blue-500/40 transition"
                value={username}
                onChange={(e) => setUsername(e.target.value)}
                required
              />
            </div>

            {/* Password */}
            <div className="relative mb-6 w-full">
              <Lock className="absolute left-3 top-1/2 -translate-y-1/2 text-blue-200/80" size={20} />
              <input
                type="password"
                placeholder="Password"
                className="w-full pl-10 pr-4 py-2 rounded-lg border border-slate-700 bg-slate-900/70 text-blue-50 placeholder-blue-200/40 
                           focus:outline-none focus:border-blue-400 focus:ring-2 focus:ring-blue-500/40 transition"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
              />
            </div>

            {/* Login Button */}
            <button
              type="submit"
              className="w-full mb-3 px-6 py-2.5 rounded-xl font-semibold
                         bg-linear-to-r from-blue-500 to-sky-400 text-slate-950
                         shadow-lg shadow-blue-900/40
                         hover:from-blue-400 hover:to-sky-300 transition"
            >
              Log In
            </button>

            {/* Signup Link */}
            <p className="pt-2 text-sm text-blue-100">
              Don&apos;t have an account?{" "}
              <button
                type="button"
                className="text-blue-300 hover:text-blue-200 underline underline-offset-2"
                onClick={() => navigate("/signup")}
              >
                Sign up
              </button>
            </p>
          </form>
        </div>
      </section>
    </main>
  );
}
