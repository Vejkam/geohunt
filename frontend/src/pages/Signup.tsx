import { User, Lock, Mail } from "lucide-react";
import { useState } from "react";
import { useNavigate } from "react-router-dom";

export default function Signup() {
  const navigate = useNavigate();

  const [username, setUsername] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [enableTwoFactor, setEnableTwoFactor] = useState(false);
  const [twoFactorProvider, setTwoFactorProvider] = useState("Email");
  const [phoneNumber, setPhoneNumber] = useState("");

  const [error, setError] = useState("");
  const [successMessage, setSuccessMessage] = useState("");
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
        body: JSON.stringify({
          username,
          password,
          email,
          enableTwoFactor,
          twoFactorProvider,
          phoneNumber,
        }),
      });

      const data = await res.json().catch(() => null);

      if (res.ok) {
        if (data?.twoFactorToken) {
          setSuccessMessage(
            `Account created. Your two-factor code is: ${data.twoFactorToken}. Please use it to log in.`
          );
          setError("");
          return;
        }

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

            {/* Success Message */}
            {successMessage && (
              <div className="mb-4 w-full p-3 rounded-lg border border-emerald-500/70 bg-emerald-900/60 text-emerald-100 text-sm">
                {successMessage}
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

            {/* Two-Factor Authentication */}
            <div className="mb-4 w-full flex flex-col gap-3">
              <label className="inline-flex items-center gap-2 text-sm text-blue-100">
                <input
                  type="checkbox"
                  checked={enableTwoFactor}
                  onChange={(e) => setEnableTwoFactor(e.target.checked)}
                  className="h-4 w-4 rounded border-slate-700 bg-slate-900 text-blue-500"
                />
                Enable two-factor authentication
              </label>

              {enableTwoFactor && (
                <div className="flex flex-col gap-3">
                  <label className="text-sm text-blue-100">
                    Provider
                    <select
                      value={twoFactorProvider}
                      onChange={(e) => setTwoFactorProvider(e.target.value)}
                      className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-900/70 px-3 py-2 text-blue-50"
                    >
                      <option value="Email">Email</option>
                      <option value="Phone">Phone</option>
                    </select>
                  </label>

                  {twoFactorProvider === "Phone" && (
                    <input
                      type="tel"
                      placeholder="Phone number"
                      value={phoneNumber}
                      onChange={(e) => setPhoneNumber(e.target.value)}
                      className="w-full pl-3 pr-4 py-2 rounded-lg border border-slate-700 bg-slate-900/70 text-blue-50 placeholder-blue-200/40 focus:outline-none focus:border-blue-400 focus:ring-2 focus:ring-blue-500/40 transition"
                    />
                  )}
                </div>
              )}
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
