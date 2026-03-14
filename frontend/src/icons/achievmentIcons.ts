import {
  Target,
  Crosshair,
  Focus,
  Trophy,
  Medal,
  HelpCircle,
  CalendarCheck,
  BicepsFlexed,
  Moon
} from "lucide-react";
import type { LucideIcon } from "lucide-react";

export const achievementIcons: Record<string, LucideIcon> = {
  FIRST_GUESS: Target,
  BULLSEYE_100M: Crosshair,
  NEAR_1KM: Focus,
  SCORE_10K: Trophy,
  CLEAN_SWEEP: Medal,
  STREAK_MASTER: CalendarCheck,
  MARATHONER: BicepsFlexed,
  LATE_NIGHT_PLAYER: Moon
};

export const defaultAchievementIcon: LucideIcon = HelpCircle;