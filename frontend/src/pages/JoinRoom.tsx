import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';

export default function JoinRoom() {
  const navigate = useNavigate();
  const [userId, setUserId] = useState<string | null>(null);
  const [displayName, setDisplayName] = useState('');
  const [error, setError] = useState('');
  const [joinRoomCode, setJoinRoomCode] = useState('');
  const [activeTab, setActiveTab] = useState<'join' | 'create'>('join');

  useEffect(() => {
    const fetchUser = async () => {
      try {
        const res = await fetch('/api/User/me', { credentials: 'include' });
        if (!res.ok) throw new Error('Not logged in');
        const data = await res.json();
        setUserId(data.id);
      } catch (err) {
        console.error(err);
        navigate('/login', { replace: true });
      }
    };
    fetchUser();
  }, [navigate]);

  const handleJoinRoom = async () => {
    if (!joinRoomCode || !displayName || !userId) {
      setError('Please fill all fields');
      return;
    }
    try {
      const res = await fetch('/api/Rooms/join', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        credentials: 'include',
        body: JSON.stringify({
          roomCode: joinRoomCode,
          userId,
          displayName
        })
      });
      if (!res.ok) throw new Error('Failed to join room');
      navigate(`/room/${joinRoomCode}`);
    } catch (err) {
      console.error(err);
      setError('Could not join room. Check the code.');
    }
  };

  const handleCreateRoom = async () => {
    if (!displayName || !userId) {
      setError('Please enter your name');
      return;
    }
    try {
      const res = await fetch('/api/Rooms/create', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        credentials: 'include',
        body: JSON.stringify({ totalRounds: 1 })
      });
      if (!res.ok) throw new Error('Failed to create room');
      const room = await res.json();

      const joinRes = await fetch('/api/Rooms/join', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        credentials: 'include',
        body: JSON.stringify({
          roomCode: room.roomCode,
          userId,
          displayName
        })
      });
      if (!joinRes.ok) throw new Error('Failed to join created room');

      navigate(`/room/${room.roomCode}`);
    } catch (err) {
      console.error(err);
      setError('Could not create room');
    }
  };

  return (
    <main className="min-h-full text-white flex items-center justify-center relative px-4 py-15">
      <section className="w-full max-w-3xl">
        <div className="bg-linear-to-r from-slate-800 to-blue-900 rounded-2xl p-8 md:p-10 border-2 border-blue-500 shadow-2xl shadow-blue-900/50">
          <header className="text-center mb-8">
            <h1 className="text-4xl md:text-5xl font-extrabold mb-3 tracking-tight">
              {activeTab === 'join' ? 'Join a Room' : 'Create a Room'}
            </h1>
            <p className="text-base md:text-lg text-blue-200 max-w-2xl mx-auto">
              {activeTab === 'join'
                ? 'Enter a room code and your name to join an existing game.'
                : 'Create a new room and start your adventure with GeoHunt.'}
            </p>
          </header>

          <div className="flex flex-col sm:flex-row gap-4 justify-center mb-6">
            <button
              onClick={() => setActiveTab('join')}
              className={`px-6 py-3 rounded-xl font-semibold shadow-lg ${
                activeTab === 'join'
                  ? 'bg-blue-500 shadow-blue-900/40'
                  : 'bg-white/10 shadow-none hover:bg-white/20'
              } transition`}
            >
              Join Room
            </button>

            <button
              onClick={() => setActiveTab('create')}
              className={`px-6 py-3 rounded-xl font-semibold shadow-lg ${
                activeTab === 'create'
                  ? 'bg-green-500 shadow-blue-900/40'
                  : 'bg-white/10 shadow-none hover:bg-white/20'
              } transition`}
            >
              Create Room
            </button>
          </div>

          <div className="flex flex-col gap-4 w-full max-w-sm mx-auto">
            {activeTab === 'join' && (
              <>
                <input
                  type="text"
                  value={joinRoomCode}
                  onChange={(e) => setJoinRoomCode(e.target.value)}
                  placeholder="Enter Room Code"
                  className="px-4 py-3 rounded-xl bg-white/10 border border-white focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
                <input
                  type="text"
                  value={displayName}
                  onChange={(e) => setDisplayName(e.target.value)}
                  placeholder="Your Name"
                  className="px-4 py-3 rounded-xl bg-white/10 border border-white focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
                <button
                  onClick={handleJoinRoom}
                  className="px-6 py-3 bg-blue-600 text-white font-semibold rounded-xl shadow hover:bg-blue-700 transition"
                >
                  Join Room
                </button>
              </>
            )}

            {activeTab === 'create' && (
              <>
                <input
                  type="text"
                  value={displayName}
                  onChange={(e) => setDisplayName(e.target.value)}
                  placeholder="Your Name"
                  className="px-4 py-3 rounded-xl bg-white/10 border border-white focus:outline-none focus:ring-2 focus:ring-green-500"
                />
                <button
                  onClick={handleCreateRoom}
                  className="px-6 py-3 bg-green-600 text-white font-semibold rounded-xl shadow hover:bg-green-700 transition"
                >
                  Create Room
                </button>
              </>
            )}

            {error && <p className="text-red-500 text-sm text-center">{error}</p>}

            <button
              onClick={() => navigate('/')}
              className="mt-4 px-6 py-3 bg-red-600 text-white font-semibold rounded-xl shadow hover:bg-red-700 transition"
            >
              Back
            </button>
          </div>
        </div>
      </section>
    </main>
  );
}
