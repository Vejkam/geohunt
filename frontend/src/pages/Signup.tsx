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

  const [error, setError] = useState("");
  const [successMessage, setSuccessMessage] = useState("");
  const [loading, setLoading] = useState(false);
  const [requiresVerification, setRequiresVerification] = useState(false);
  const [verificationCode, setVerificationCode] = useState("");
  const [twoFactorProvider, setTwoFactorProvider] = useState("Email");
  const [twoFactorToken, setTwoFactorToken] = useState<string | null>(null);

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
        }),
      });

      const data = await res.json().catch(() => null);

      if (res.ok) {
        if (data?.twoFactorToken) {
          setRequiresVerification(true);
          setTwoFactorProvider(data.provider ?? "Email");
          setTwoFactorToken(data.twoFactorToken);
          setSuccessMessage(
            `Account created. Your email authentication code is: ${data.twoFactorToken}. Enter it below to complete registration.`
          );
          setError("");
          return;
        }

        navigate("/login");
      } else {
        const parseArray = (items: any[]) =>
          items
            .flatMap((item) => {
              if (typeof item === "string") return [item];
              if (typeof item === "object" && item !== null) {
                return Object.values(item).flatMap((value) =>
                  Array.isArray(value) ? value : [value]
                );
              }
              return [JSON.stringify(item)];
            })
            .map((item) =>
              typeof item === "string"
                ? item
                : item.description || item.code || item.message || JSON.stringify(item)
            )
            .join(" ");

        const parseResponse = (payload: any): string => {
          if (!payload) return "Sign up failed. Please check your details and try again.";
          if (typeof payload === "string") return payload;
          if (Array.isArray(payload)) return parseArray(payload);
          if (payload.errors) {
            if (Array.isArray(payload.errors)) return parseArray(payload.errors);
            if (typeof payload.errors === "object") return parseArray(Object.values(payload.errors));
          }
          if (payload.message) return payload.message;
          if (payload.title) return payload.title;
          if (payload.detail) return payload.detail;
          return JSON.stringify(payload);
        };

        const message = parseResponse(data) || `${res.status} ${res.statusText}`;
        setError(message);
      }
    } catch (err) {
      console.error("Signup error:", err);
      setError("An error occurred. Please try again.");
    } finally {
      setLoading(false);
    }
  };

  const handleVerifyTwoFactor = async () => {
    try {
      setError("");
      setLoading(true);

      const res = await fetch("/api/Account/verify-2fa", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          username,
          provider: twoFactorProvider,
          code: verificationCode,
        }),
      });

      const data = await res.json().catch(() => null);

      if (res.ok) {
        navigate("/");
        return;
      }

      setError(data?.message || "Invalid authentication code.");
    } catch (err) {
      console.error("Verification error:", err);
      setError("An error occurred. Please try again.");
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!loading) {
      if (requiresVerification) {
        handleVerifyTwoFactor();
      } else {
        handleSignup();
      }
    }
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

            <div className="relative mb-4 w-full">
              <Mail className="absolute left-3 top-1/2 -translate-y-1/2 text-blue-200/80" size={20} />
              <input
                type="email"
                placeholder="Email"
                className="w-full pl-10 pr-4 py-2 rounded-lg border border-slate-700 bg-slate-900/70 text-blue-50 placeholder-blue-200/40 
                           focus:outline-none focus:border-blue-400 focus:ring-2 focus:ring-blue-500/40 transition"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
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

            <div className="mb-4 w-full">
              <label className="inline-flex items-center gap-2 text-sm text-blue-100">
                <input
                  type="checkbox"
                  checked={enableTwoFactor}
                  onChange={(e) => setEnableTwoFactor(e.target.checked)}
                  disabled={requiresVerification}
                  className="h-4 w-4 rounded border-slate-700 bg-slate-900 text-blue-500"
                />
                Enable email authentication
              </label>
            </div>

            {requiresVerification && (
              <div className="relative mb-6 w-full">
                <Lock className="absolute left-3 top-1/2 -translate-y-1/2 text-blue-200/80" size={20} />
                <input
                  type="text"
                  placeholder="Enter authentication code"
                  className="w-full pl-10 pr-4 py-2 rounded-lg border border-slate-700 bg-slate-900/70 text-blue-50 placeholder-blue-200/40 
                             focus:outline-none focus:border-blue-400 focus:ring-2 focus:ring-blue-500/40 transition"
                  value={verificationCode}
                  onChange={(e) => setVerificationCode(e.target.value)}
                  required
                />
              </div>
            )}

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
              {loading ? "Processing..." : requiresVerification ? "Verify code" : "Sign Up"}
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
