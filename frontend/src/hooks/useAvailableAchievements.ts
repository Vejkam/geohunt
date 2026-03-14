import { useEffect, useState } from "react";
import type { AchievementApi, Achievement } from "../types/achievements";
import { achievementIcons, defaultAchievementIcon } from "../icons/achievmentIcons";

export function useAvailableAchievements() {
  const [achievements, setAchievements] = useState<Achievement[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<unknown>(null);

  useEffect(() => {
    async function load() {
      try {
        setLoading(true);
        setError(null);
        const res = await fetch("/api/Achievement/available-achievements");
        if (!res.ok) {
          throw new Error("Failed to fetch achievements");
        }

        const data: AchievementApi[] = await res.json();

        const withIcons: Achievement[] = data.map((a) => ({
          ...a,
          icon: achievementIcons[a.code] ?? defaultAchievementIcon,
        }));

        setAchievements(withIcons);
      } catch (err) {
        setError(err);
      } finally {
        setLoading(false);
      }
    }

    load();
  }, []);

  return { achievements, loading, error };
}
