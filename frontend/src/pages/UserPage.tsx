import { Calendar, Flame } from "lucide-react";
import { useAuth } from "../hooks/useAuth";
import { useAvailableAchievements } from "../hooks/useAvailableAchievements";
import { useUserAchievements } from "../hooks/useUserAchievements";
import { useUserStats } from "../hooks/useUserStats";
import { useNavigate } from "react-router-dom";

export default function UserPage() {
  const { username, createdAt, userId } = useAuth();
  const navigate = useNavigate();

  const {
    achievements: availableAchievements,
    loading: availableLoading,
    error: availableError,
  } = useAvailableAchievements();

  const {
    userAchievements,
    loading: userLoading,
    error: userError,
  } = useUserAchievements(userId);

  const {
    userStats,
    loading: userStatsLoading,
    error: userStatsError
  } = useUserStats(userId);

  const loading = availableLoading || userLoading || userStatsLoading;
  const hasError = availableError || userError || userStatsError;

  if (loading) {
    return <div>Loading achievements...</div>;
  }

  if (hasError) {
    console.error(availableError || userError || userStatsError);
    return <div>Failed to load achievements</div>;
  }

  const userAchievementsByCode: Record<string, (typeof userAchievements)[number]> =
    {};
  for (const a of userAchievements) {
    userAchievementsByCode[a.code] = a;
  }

  const mergedAchievements = availableAchievements
    .map((a) => {
      const unlocked = userAchievementsByCode[a.code];
      return {
        ...a,
        unlocked: Boolean(unlocked),
        unlockedAt: unlocked?.unlockedAt ?? null,
      };
    })
    .sort((a, b) => {
      if (a.unlocked === b.unlocked) return 0;
      return a.unlocked ? -1 : 1;
    });

    const totalAchievements = availableAchievements.length;
    const unlockedAchievements = userAchievements.length;

    const progress = totalAchievements === 0 ? 0 : Math.round((unlockedAchievements / totalAchievements) * 100);
    const isCompleted = progress === 100;

      const handleLogout = async () => {
    try {
      const res = await fetch('/api/Account/logout', {
        method: 'POST',
        credentials: 'include' 
      });
      if (res.ok) {
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
    <div className="min-h-screen p-6">
        <div className="max-w-7xl mx-auto">
            {/* Profile Header */}
            <div className="bg-linear-to-r from-slate-800 to-blue-900 rounded-2xl p-8 mb-8 border-2 border-blue-500 shadow-2xl">
                <div className="flex flex-col md:flex-row items-center md:items-start gap-6">
                    {/* User Info */}
                    <div className="flex-1 text-center md:text-left">
                        <div className="flex items-center justify-center md:justify-start gap-3 mb-2">
                            <h1 className="text-4xl font-bold text-white">{username}</h1>
                            <span className="bg-blue-600 text-white px-3 py-1 rounded-full text-sm font-bold">
                                Level XX
                            </span>
                        </div>
                        <div className="flex flex-wrap justify-center md:justify-start gap-4 text-blue-200 mb-4">
                            <div className="flex items-center gap-2">
                                <Calendar className="w-4 h-4" />
                                <span className="text-sm">
                                    Joined at {createdAt}
                                </span>
                            </div>
                            <div className="flex items-center gap-2">
                                <Flame className="w-4 h-4 text-orange-400" />
                                <span className="text-sm font-semibold">
                                    {userStats?.currentStreakDays} day streak
                                </span>
                            </div>
                        </div>
                        
                        {/* Stats Grid */}
                        <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                            <div className="bg-slate-900 bg-opacity-50 rounded-lg p-3">
                                <div className="text-2xl font-bold text-white">{userStats?.totalGames}</div>
                                <div className="text-xs text-blue-300">Games Played</div>
                            </div>
                            <div className="bg-slate-900 bg-opacity-50 rounded-lg p-3">
                                <div className="text-2xl font-bold text-green-400">XX%</div>
                                <div className="text-xs text-blue-300">Another statistic</div>
                            </div>
                            <div className="bg-slate-900 bg-opacity-50 rounded-lg p-3">
                                <div className="text-2xl font-bold text-orange-400">{userStats?.longestStreakDays}</div>
                                <div className="text-xs text-blue-300">Longest Streak</div>
                            </div>
                            <div className="bg-slate-900 bg-opacity-50 rounded-lg p-3">
                                <div className="text-2xl font-bold text-purple-400">Xh</div>
                                <div className="text-xs text-blue-300">Playtime</div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            {/* Achievements Grid */}
        <section>
          <h2 className="text-xl font-semibold text-white mb-4">
            Achievements
          </h2>

        {/* Achievements progress bar */}
        <div className="mb-4 w-64">
        <div className="flex justify-between items-center mb-1">
            <span className="text-sm text-blue-100 font-medium">Progress</span>
            <span className="text-xs text-blue-300">
            {unlockedAchievements} / {totalAchievements} ({progress}%)
            </span>
        </div>

        <div className="w-full h-2 rounded-full bg-slate-800 overflow-hidden">
            <div
            className={`h-full transition-all duration-500 ${
                isCompleted
                ? "bg-yellow-400"
                : "bg-linear-to-r from-green-400 to-emerald-500"
            }`}
            style={{ width: `${progress}%` }}
            />
        </div>
        </div>

          <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
            {mergedAchievements.map((achievement) => {
              const Icon = achievement.icon;
              const isUnlocked = achievement.unlocked;

              return (
                <div
                  key={achievement.code}
                  className={`flex items-center gap-3 rounded-lg p-3 bg-slate-900/60 border transition
                    ${
                      isUnlocked
                        ? "border-green-500"
                        : "border-slate-700 opacity-60"
                    }`}
                >
                  {Icon && (
                    <Icon
                      className={`w-6 h-6 ${
                        isUnlocked ? "text-green-400" : "text-slate-500"
                      }`}
                    />
                  )}
                  <div>
                    <div className="font-semibold text-white flex items-center gap-2">
                      {achievement.name}
                      {!isUnlocked && (
                        <span className="text-xs px-2 py-0.5 rounded-full bg-slate-800 text-slate-300">
                          Locked
                        </span>
                      )}
                    </div>
                    <div className="text-sm text-blue-200">
                      {achievement.description}
                    </div>
                    {isUnlocked && achievement.unlockedAt && (
                      <div className="text-xs text-green-400 mt-1">
                        Unlocked:{" "}
                        {new Date(achievement.unlockedAt)
                        .toISOString()
                        .replace("T", ", ")
                        .slice(0, 17)}
                      </div>
                    )}
                  </div>
                </div>
              );
            })}
          </div>
          {/* Logout button */}
          <div className="mt-8 flex justify-center">
            <button
              onClick={handleLogout}
              className="px-4 py-2 rounded-lg font-medium
                        bg-slate-900/70 border border-red-400/40 text-red-300
                        hover:bg-slate-900/90 hover:border-red-300 hover:text-red-200
                        shadow-md shadow-red-900/40 transition w-32"
            >
              Logout
            </button>
          </div>
        </section>
        </div>
      </div>
  );
}
