// src/pages/Game.tsx (StreetViewApp)
declare global {
  interface Window {
    google: any;
  }
}

import { useEffect, useRef, useState } from 'react';
import MiniMap from '../components/MiniMap';
import { useNavigate, useLocation } from 'react-router-dom';
import RoundResultMap from '../components/RoundResultMap';
import { AchievementPopup } from '../components/AchievementPopup';
import type { AchievementDisplay, UserAchievementApi } from '../types/achievements';

interface Coordinates {
  lat: number;
  lng: number;
}

interface GeocodingApiResponse {
  id: number;
  modifiedCoordinates: Coordinates;
  panoID: string;
}

type GameStateFromStart = {
  gameId: string;
  totalRounds: number;
  currentRound: number; 
};

type GuessPostResponse = {
  guess: {
    id: string;
    gameId: string;
    locationId: number;
    roundNumber: number;
    guessedLatitude: number;
    guessedLongitude: number;
    distanceKm: number;
    score: number;
  };
  finished: boolean;
  currentRound: number; 
  totalScore: number;
  achievementsUnlocked: UserAchievementApi[];
};

const StreetViewApp = () => {
  const streetViewRef = useRef<HTMLDivElement>(null);
  const panoRef = useRef<any>(null);

  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const isInitializedRef = useRef(false);

  const [selectedCoords, setSelectedCoords] = useState<Coordinates | null>(null);
  const [initialCoords, setInitialCoords] = useState<Coordinates | null>(null);
  const [gameId, setGameId] = useState<string | null>(null);
  const [locationId, setLocationId] = useState<number | null>(null);

  const [round, setRound] = useState<number>(1);
  const [totalRounds, setTotalRounds] = useState<number>(3);
  const [submitting, setSubmitting] = useState(false);
  const [postRoundSummary, setPostRoundSummary] = useState<{
    completedRound: number;
    score: number;
    distanceKm: number;
    actual: Coordinates;
    guess: Coordinates;
    finished: boolean;
    nextRound: number;
    totalScore: number;
  } | null>(null);
  const [pendingNextLocation, setPendingNextLocation] = useState<{
    position: Coordinates;
    locationId: number;
  } | null>(null);

  const navigate = useNavigate();
  const location = useLocation();

  const [achievementQueue, setAchievementQueue] = useState<AchievementDisplay[]>([]);
  const [currentAchievementPopup, setCurrentAchievementPopup] = useState<AchievementDisplay | null>(null);
  const achievementSoundRef = useRef<HTMLAudioElement | null>(null);
  const [finishedGamePending, setFinishedGamePending] = useState(false);

  const handleApiError = async (response: Response, context: string) => {
    if (!response.ok) {
      const contentType = response.headers.get('Content-Type') || '';
      let errorData: any = {};

      if (contentType.includes('application/json')) {
        errorData = await response.json().catch(() => ({}));
      } else {
        errorData.message = await response.text().catch(() => '');
      }

      if (errorData.code === 'MAPS_UNAVAILABLE') {
        throw new Error(`Couldn't ${context}. The map service is temporarily unavailable. Please retry in a moment.`);
      }

      const extraMsg = errorData.message ? `: ${errorData.message}` : '';
      throw new Error(`${context} failed: ${response.status}${extraMsg}`);
    }
  };

  useEffect(() => {
    const s = (location.state || {}) as Partial<GameStateFromStart>;
    if (!s.gameId) {
      navigate('/start', { replace: true });
      return;
    }
    setGameId(s.gameId);
    setRound(s.currentRound ?? 1);
    setTotalRounds(s.totalRounds ?? 3);
  }, [location.state, navigate]);

  const waitGoogleMaps = () =>
    new Promise<void>((resolve, reject) => {
      const start = Date.now();
      const tick = () => {
        if (window.google?.maps) return resolve();
        if (Date.now() - start > 15000)
          return reject(new Error('Google Maps API not loaded'));
        setTimeout(tick, 100);
      };
      tick();
    });

  const applyLocationToPano = (position: Coordinates, locId: number) => {
    if (!panoRef.current) {
      panoRef.current = new window.google.maps.StreetViewPanorama(streetViewRef.current!, {
        position,
        pov: { heading: 0, pitch: 0 },
        zoom: 1,
        addressControl: false,
        fullscreenControl: false,
        imageDateControl: false,
        showRoadLabels: false,
      });
    } else {
      panoRef.current.setPosition(position);
      panoRef.current.setPov({ heading: 0, pitch: 0 });
      panoRef.current.setZoom(1);
    }

    setInitialCoords(position);
    setLocationId(locId);
    setSelectedCoords(null);
  };

  const fetchAndPersistLocation = async () => {
    const coordsRes = await fetch('/api/geocoding/valid_coords');
    await handleApiError(coordsRes, 'load Street View');
    const coordsData: GeocodingApiResponse = await coordsRes.json();

    const lat = parseFloat(String(coordsData.modifiedCoordinates.lat));
    const lng = parseFloat(String(coordsData.modifiedCoordinates.lng));
    if (Number.isNaN(lat) || Number.isNaN(lng)) {
      throw new Error('Invalid coordinates received from API');
    }
    const position: Coordinates = { lat, lng };

    const locRes = await fetch('/api/Locations', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ latitude: lat, longitude: lng, panoId: coordsData.panoID }),
    });
    await handleApiError(locRes, 'save location');
    const locData = await locRes.json();

    return { position, locationId: locData.id };
  };

  const loadRoundLocation = async () => {
    const { position, locationId: locId } = await fetchAndPersistLocation();
    applyLocationToPano(position, locId);
  };

  useEffect(() => {
    if (isInitializedRef.current) return;
    isInitializedRef.current = true;

    (async () => {
      try {
        await waitGoogleMaps();
        if (!streetViewRef.current) throw new Error('Street View container not mounted');
        await loadRoundLocation();
        setLoading(false);
      } catch (err) {
        const msg = err instanceof Error ? err.message : 'Failed to load Street View';
        setError(msg);
        setLoading(false);
        console.error('Street View init error:', err);
      }
    })();
  }, []);

  const handleSubmitGuess = async () => {
    if (!selectedCoords || !initialCoords || !gameId || !locationId) {
      console.error('Missing required data:', { selectedCoords, initialCoords, gameId, locationId });
      return;
    }

    if (!achievementSoundRef.current) {
      achievementSoundRef.current = new Audio("/sounds/achievement-unlocked.mp3");
      achievementSoundRef.current.volume = 0.8;
    }

    try {
      setSubmitting(true);
      const completedRound = round;

      const resultResponse = await fetch('/api/result', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ guessedCoords: selectedCoords, initialCoords }),
      });
      await handleApiError(resultResponse, 'calculate score');
      const resultData: { score: number; distance: number } = await resultResponse.json();

      const guessRes = await fetch('/api/Guess', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          gameId,
          locationId,
          guessedLatitude: selectedCoords.lat,
          guessedLongitude: selectedCoords.lng,
          score: resultData.score,
          distanceKm: resultData.distance,
        }),
      });
      await handleApiError(guessRes, 'save guess');
      const guessData: GuessPostResponse = await guessRes.json();
      console.log("Guess response:", guessData);
      console.log("Achievements unlocked:", guessData.achievementsUnlocked);

      if (guessData.achievementsUnlocked.length > 0) {
        const newDisplayItems: AchievementDisplay[] =
          guessData.achievementsUnlocked.map((a) => ({
            ...a,
            uniqueId: `${a.code}-${a.unlockedAt}-${Math.random().toString(36).slice(2)}`
          }));

        console.log("New achievements for queue:", newDisplayItems);

        setAchievementQueue((prev) => [...prev, ...newDisplayItems]);
      }

      setPostRoundSummary({
        completedRound,
        score: resultData.score,
        distanceKm: resultData.distance,
        actual: initialCoords,
        guess: selectedCoords,
        finished: guessData.finished,
        nextRound: guessData.currentRound,
        totalScore: guessData.totalScore,
      });
        
      if (guessData.finished && guessData.achievementsUnlocked.length > 0) {
        setFinishedGamePending(true);
      }

      setPendingNextLocation(null);
      if (!guessData.finished) {
        fetchAndPersistLocation()
          .then(setPendingNextLocation)
          .catch((err) => {
            console.error('Prefetch next location failed:', err);
            setPendingNextLocation(null);
          });
      }
    } catch (e) {
      console.error(e);
      alert((e as Error).message || 'Failed to submit this round');
    } finally {
      setSubmitting(false);
    }
  };

  const handleContinue = async () => {
    if (!postRoundSummary) return;

    if (postRoundSummary.finished) {
      if (achievementQueue.length > 0) {
        setPostRoundSummary(null);
        return;
      }
      
      navigate(`/results/${gameId}`, {
        state: {
          gameId,
        },
        replace: true,
      });
      return;
    }

    setPostRoundSummary(null);

    if (pendingNextLocation) {
      applyLocationToPano(pendingNextLocation.position, pendingNextLocation.locationId);
      setRound(postRoundSummary.nextRound);
      setPendingNextLocation(null);
      return;
    }

    try {
      setLoading(true);
      setRound(postRoundSummary.nextRound);
      await loadRoundLocation();
    } catch (err) {
      console.error('Failed to load next round:', err);
      alert('Failed to load next round. Please try again.');
    } finally {
      setLoading(false);
    }
  };
  const handleCloseCurrentAchievement = () => {
    setAchievementQueue((prevQueue) => {
      const [, ...rest] = prevQueue;

      if (rest.length === 0) {
        setCurrentAchievementPopup(null);

        if (finishedGamePending && gameId) {
          navigate(`/results/${gameId}`, {
            state: { gameId },
            replace: true,
          });
        }
      } else {
        setCurrentAchievementPopup(rest[0]);
      }

      return rest;
    });
  };

  useEffect(() => {
    if (!currentAchievementPopup && achievementQueue.length > 0) {
      console.log("Showing popup for:", achievementQueue[0]);
      setCurrentAchievementPopup(achievementQueue[0]);
    }
  }, [achievementQueue, currentAchievementPopup]);

  useEffect(() => {
    if (!currentAchievementPopup) return;
    if (!achievementSoundRef.current) return;

    achievementSoundRef.current.currentTime = 0;
    achievementSoundRef.current
      .play()
      .catch((err) => {
        console.warn("Failed to play achievement sound:", err);
      });
  }, [currentAchievementPopup]);

  return (
    <div className="relative h-screen w-screen">
      {/* Round/Score badge */}
      <div className="absolute top-3 left-3 z-50 rounded bg-black/60 text-white px-3 py-2 text-sm">
        Round {round} / {totalRounds}
      </div>

      {/* Street View container */}
      <div ref={streetViewRef} className="h-full w-full" />

      {/* Loading overlay */}
      {loading && !error && (
        <div className="absolute inset-0 flex items-center justify-center bg-black/30 backdrop-blur-sm">
          <div className="text-center">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-white mx-auto mb-4"></div>
            <p className="text-white">Loading Street View...</p>
            <p className="text-white/70 text-sm mt-2">Check console for details</p>
          </div>
        </div>
      )}

      {/* Error overlay */}
      {error && (
        <div className="absolute inset-0 flex items-center justify-center bg-gray-100">
          <div className="bg-white p-8 rounded-lg shadow-lg max-w-md">
            <h2 className="text-red-600 text-xl font-bold mb-2">Error</h2>
            <p className="text-gray-700">{error}</p>
            <button
              onClick={() => window.location.reload()}
              className="mt-4 px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
            >
              Retry
            </button>
          </div>
        </div>
      )}

      {/* Mini map + submit */}
      <div className="absolute bottom-4 right-4 flex flex-col gap-2" style={{ zIndex: 9999 }}>
        <MiniMap
          initialZoom={1}
          onSelect={(coords) => {
            setSelectedCoords(coords);
          }}
          className="overflow-hidden shadow-lg rounded-xl"
          style={{ width: 300, height: 200 }}
        />
        <button
          className="px-3 py-3 bg-slate-900 text-white font-semibold rounded-xl shadow hover:bg-slate-800 transition disabled:opacity-60 disabled:cursor-not-allowed border-slate-700"
          onClick={handleSubmitGuess}
          disabled={!selectedCoords || submitting || !!error || loading || !!postRoundSummary}
        >
          {submitting ? 'Submitting…' : selectedCoords ? 'Submit Guess' : 'Select a location on the map'}
        </button>
      </div>

      {postRoundSummary && (
        <div className="absolute inset-0 bg-black/70 backdrop-blur-sm flex items-center justify-center px-4 z-[12000]">
          <div className="bg-linear-to-r from-slate-800 to-blue-900 rounded-2xl p-8 border-2 border-blue-500 shadow-2xl shadow-blue-900/50 w-full max-w-3xl">
            <div className="flex flex-col gap-1 sm:flex-row sm:items-baseline sm:justify-between">
              <h2 className="text-xl font-semibold text-blue-300">
                Round {postRoundSummary.completedRound} summary
              </h2>
              <div className="text-sm text-blue-100">
                Score: <span className="font-semibold text-green-400">{postRoundSummary.score}</span> · Distance:{' '}
                <span className="font-semibold text-blue-200">{postRoundSummary.distanceKm.toFixed(1)} km</span>
              </div>
            </div>

            <div className="mt-4">
              <RoundResultMap
                actual={postRoundSummary.actual}
                guess={postRoundSummary.guess}
                distanceKm={postRoundSummary.distanceKm}
              />
            </div>

            <div className="mt-4 text-sm text-blue-200">
              <div>
                Actual: {postRoundSummary.actual.lat.toFixed(5)}, {postRoundSummary.actual.lng.toFixed(5)}
              </div>
              <div>
                Your guess: {postRoundSummary.guess.lat.toFixed(5)}, {postRoundSummary.guess.lng.toFixed(5)}
              </div>
            </div>

            <div className="mt-6 flex justify-end gap-3">
              <button
                onClick={handleContinue}
                className="px-6 py-2.5 rounded-xl font-semibold bg-linear-to-r from-blue-500 to-sky-400 text-slate-950 shadow-lg shadow-blue-900/40 hover:from-blue-400 hover:to-sky-300 transition"
              >
                {postRoundSummary.finished
                  ? 'See final results'
                  : `Continue to Round ${postRoundSummary.nextRound}`}
              </button>
            </div>
          </div>
        </div>
      )}
      <AchievementPopup
        achievement={currentAchievementPopup}
        isOpen={!!currentAchievementPopup}
        onClose={handleCloseCurrentAchievement}
        autoCloseMs={3000}
      />
    </div>
  );
};

export default StreetViewApp;
