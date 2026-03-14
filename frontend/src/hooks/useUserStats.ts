import { useState, useEffect } from "react";
import type { UserStatsApi } from "../types/achievements";

export function useUserStats(userId: string) {
    const [userStats, setUserStats] = useState<UserStatsApi>();
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<unknown | null>(null);
    
    useEffect(() => {
        if (!userId) return;

        async function load() {
            try {
            setLoading(true);
            setError(null);
            const res = await fetch(`/api/Achievement/stats/${userId}`);
            if (!res.ok) throw new Error("Failed to fetch user stats");

            const data: UserStatsApi = await res.json();
            setUserStats(data);
            } catch (err) {
            setError(err);
            } finally {
            setLoading(false);
            }
        }

        load();
    }, [userId]);

    return { userStats, loading, error }
}