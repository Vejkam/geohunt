import { useEffect, useState } from "react";
import type { UserAchievementApi } from "../types/achievements";

export function useUserAchievements(userId: string) {
  const [userAchievements, setUserAchievements] = useState<UserAchievementApi[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<unknown | null>(null);

  useEffect(() => {
    if (!userId) return;

    async function load() {
      try {
        setLoading(true);
        setError(null);
        const res = await fetch(`/api/Achievement/achievements/${userId}`);
        if (!res.ok) throw new Error("Failed to fetch user achievements");
        
        const text = await res.text();
        if (!text.trim()) {
          setUserAchievements([]);
          return;
        }

        const data: UserAchievementApi[] = JSON.parse(text);
        setUserAchievements(Array.isArray(data) ? data : []);
      } catch (err) {
        setError(err);
      } finally {
        setLoading(false);
      }
    }

    load();
  }, [userId]);

  const byCode = userAchievements.reduce((acc, a) => {
    acc[a.code] = a;
    return acc;
  }, {} as Record<string, UserAchievementApi>);

  return { userAchievements, byCode, loading, error };
}
