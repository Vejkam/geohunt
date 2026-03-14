import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';

export default function Home() {
  const navigate = useNavigate();
  const [loggedIn, setLoggedIn] = useState(true);

  useEffect(() => {
    const checkLogin = async () => {
      try {
        const res = await fetch('/api/User/me', {
          method: 'GET',
          credentials: 'include',
        });
        if (!res.ok) {
          navigate('/login', { replace: true });
        }
      } catch (error) {
        console.error('Error checking login status:', error);
        navigate('/login', { replace: true });
      }
    };
    checkLogin();
  }, [navigate]);

  const handleLogout = async () => {
    try {
      const res = await fetch('/api/Account/logout', {
        method: 'POST',
        credentials: 'include',
      });
      if (res.ok) {
        setLoggedIn(false);
        navigate('/login', { replace: true });
      } else {
        console.error('Logout failed');
        alert('Logout failed. Please try again.');
      }
    } catch (error) {
      console.error('Error during logout:', error);
      alert('An error occurred during logout. Please try again.');
    }
  };

  return (
    <main className="min-h-full text-white flex items-center justify-center relative px-4 py-15">
      <section className="w-full max-w-3xl">
        <div className="bg-linear-to-r from-slate-800 to-blue-900 rounded-2xl p-8 md:p-10 border-2 border-blue-500 shadow-2xl shadow-blue-900/50">
          <header className="text-center mb-8">
            <h1 className="text-4xl md:text-5xl font-extrabold mb-3 tracking-tight">
              Welcome to <span className="text-blue-300">GeoHunt</span> 🌍
            </h1>
            <p className="text-base md:text-lg text-blue-200 max-w-2xl mx-auto">
              Explore the world, make precise guesses, unlock achievements, and
              track your progress in a map-based adventure built for explorers.
            </p>
          </header>

          <div className="flex flex-col sm:flex-row gap-4 justify-center">
            <button
              onClick={() => navigate('/start')}
              className="px-6 py-3 rounded-xl font-semibold shadow-lg shadow-blue-900/40
                         bg-linear-to-r from-blue-500 to-sky-400 text-slate-950
                         hover:from-blue-400 hover:to-sky-300 transition"
            >
              Start Playing
            </button>

            <button
              onClick={() => navigate('/joinroom')}
              className="px-6 py-3 bg-green-600 text-white font-semibold rounded-xl shadow hover:bg-green-700 transition"
            >
              Join Room
            </button>

            <button
              onClick={() => navigate('/about')}
              className="px-6 py-3 rounded-xl font-semibold border border-blue-300/60
                         text-blue-100 bg-slate-900/40
                         hover:bg-slate-800/70 hover:border-blue-200 transition"
            >
              Learn More
            </button>
          </div>

          <div className="text-center mt-6">
            <button
              onClick={handleLogout}
              className="px-4 py-2 text-sm rounded-lg bg-red-600 hover:bg-red-700"
            >
              Logout
            </button>
          </div>
        </div>
      </section>
    </main>
  );
}
