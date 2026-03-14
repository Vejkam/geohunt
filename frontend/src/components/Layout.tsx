import { Link, Outlet } from "react-router-dom";
import { Home, Info, User, Map, Gamepad2 } from "lucide-react";
import { useAuth } from "../hooks/useAuth";

export default function Layout() {
  const { username } = useAuth();

  return (
    <div className="min-h-screen bg-linear-to-br from-slate-900 via-blue-900 to-slate-900 flex flex-col">
      <nav className="bg-slate-900 bg-opacity-90 backdrop-blur-sm border-b-2 border-blue-500 shadow-2xl sticky top-0 z-50">
        <div className="max-w-7xl mx-auto px-6 py-4">
          <div className="flex items-center justify-between">
            {/* Logo Section */}
            <Link
                to="/"
            >
              <div className="flex items-center gap-3">
              <div className="bg-linear-to-br from-blue-500 to-purple-600 p-2 rounded-lg">
                <Map className="w-8 h-8 text-white" />
              </div>
              <span className="text-2xl font-bold text-white hidden sm:block">
                GeoHunt
              </span>
            </div>
            </Link>


            {/* Navigation Links */}
            <div className="flex items-center gap-2">
              <Link
                to="/"
                className="flex items-center gap-2 px-4 py-2 rounded-lg text-blue-200 hover:bg-slate-800 hover:text-white transition-all font-semibold"
              >
                <Home className="w-5 h-5" />
                <span className="hidden sm:inline">Home</span>
              </Link>

              <Link
                to="/start"
                className="flex items-center gap-2 px-4 py-2 rounded-lg text-blue-200 hover:bg-slate-800 hover:text-white transition-all font-semibold"
              >
                <Gamepad2 className="w-5 h-5" />
                <span className="hidden sm:inline">Play</span>
              </Link>

              <Link
                to="/about"
                className="flex items-center gap-2 px-4 py-2 rounded-lg text-blue-200 hover:bg-slate-800 hover:text-white transition-all font-semibold"
              >
                <Info className="w-5 h-5" />
                <span className="hidden sm:inline">About</span>
              </Link>

              <div className="h-8 w-px bg-slate-700 mx-2" />

              <Link
                to="/user"
                className="flex items-center gap-3 px-4 py-2 rounded-lg bg-linear-to-r from-blue-600 to-purple-600 hover:from-blue-500 hover:to-purple-500 text-white transition-all font-semibold shadow-lg hover:shadow-xl hover:scale-105"
              >
                <User className="w-5 h-5" />
                <span className="hidden md:inline">{username}</span>
              </Link>
            </div>
          </div>
        </div>
      </nav>

      {/* Main content fills the rest of the viewport */}
      <main className="flex-1">
        <Outlet />
      </main>
    </div>
  );
}
