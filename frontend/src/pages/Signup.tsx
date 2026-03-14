import { User, Lock, Mail } from "lucide-react";
import { useState } from "react";
import { useNavigate } from "react-router-dom";

export default function Signup() {
  const navigate = useNavigate();

  const [username, setUsername] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");

  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  const handleSignup = async () => {
    try {
      setError("");
      setLoading(true);

      if (password !== confirmPassword) {
        setError("Passwords do not match.");
        setLoading(false);
        return;
      }

      const res = await fetch("/api/Account/register", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        // adjust payload to match your backend
        body: JSON.stringify({ username, password, email }),
      });

      if (res.ok) {
        navigate("/login");
      } else {
        const data = await res.json().catch(() => null);
        setError(
          data?.message || "Sign up failed. Please check your details and try again."
        );
      }
    } catch (err) {
      console.error("Signup error:", err);
      setError("An error occurred. Please try again.");
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!loading) handleSignup();
  };

  return (
    <main className="min-h-full bg-slate-950 text-white flex items-center justify-center px-4">
      <section className="w-full max-w-md">
        <div className="bg-linear-to-r from-slate-800 to-blue-900 rounded-2xl p-8 border-2 border-blue-500 shadow-2xl shadow-blue-900/50">
          {/* Branding */}
          <header className="text-center mb-6">
            <h1 className="text-4xl font-extrabold tracking-tight mb-2">
              <span className="text-blue-300">GeoHunt</span>
            </h1>
            <p className="text-sm text-blue-200">
              Create your account and start exploring the world 🌍
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

            {/* Email (optional, remove if you don't use email) */}
            <div className="relative mb-4 w-full">
              <Mail className="absolute left-3 top-1/2 -translate-y-1/2 text-blue-200/80" size={20} />
              <input
                type="email"
                placeholder="Email"
                className="w-full pl-10 pr-4 py-2 rounded-lg border border-slate-700 bg-slate-900/70 text-blue-50 placeholder-blue-200/40 
                           focus:outline-none focus:border-blue-400 focus:ring-2 focus:ring-blue-500/40 transition"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
              />
            </div>

            {/* Password */}
            <div className="relative mb-4 w-full">
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

            {/* Confirm Password */}
            <div className="relative mb-6 w-full">
              <Lock className="absolute left-3 top-1/2 -translate-y-1/2 text-blue-200/80" size={20} />
              <input
                type="password"
                placeholder="Confirm Password"
                className="w-full pl-10 pr-4 py-2 rounded-lg border border-slate-700 bg-slate-900/70 text-blue-50 placeholder-blue-200/40 
                           focus:outline-none focus:border-blue-400 focus:ring-2 focus:ring-blue-500/40 transition"
                value={confirmPassword}
                onChange={(e) => setConfirmPassword(e.target.value)}
                required
              />
            </div>

            {/* Signup Button */}
            <button
              type="submit"
              disabled={loading}
              className={`w-full mb-3 px-6 py-2.5 rounded-xl font-semibold
                         bg-linear-to-r from-blue-500 to-sky-400 text-slate-950
                         shadow-lg shadow-blue-900/40
                         hover:from-blue-400 hover:to-sky-300 transition
                         disabled:opacity-60 disabled:cursor-not-allowed`}
            >
              {loading ? "Creating account..." : "Sign Up"}
            </button>

            {/* Login Link */}
            <p className="pt-2 text-sm text-blue-100">
              Already have an account?{" "}
              <button
                type="button"
                className="text-blue-300 hover:text-blue-200 underline underline-offset-2"
                onClick={() => navigate("/login")}
              >
                Log in
              </button>
            </p>
          </form>
        </div>
      </section>
    </main>
  );
}
