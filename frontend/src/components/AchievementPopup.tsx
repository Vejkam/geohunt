import React, { useEffect } from "react";
import type { AchievementDisplay } from "../types/achievements";
import {
  achievementIcons,
  defaultAchievementIcon,
} from "../icons/achievmentIcons";

interface AchievementPopupProps {
  achievement: AchievementDisplay | null;
  isOpen: boolean;
  onClose: () => void;
  autoCloseMs?: number;
}

export const AchievementPopup: React.FC<AchievementPopupProps> = ({
  achievement,
  isOpen,
  onClose,
  autoCloseMs = 4000,
}) => {
  useEffect(() => {
    if (!isOpen || !achievement || !autoCloseMs) return;

    const timerId = window.setTimeout(onClose, autoCloseMs);
    return () => window.clearTimeout(timerId);
  }, [isOpen, achievement, autoCloseMs, onClose]);

  if (!isOpen || !achievement) return null;

  const Icon = achievementIcons[achievement.code] ?? defaultAchievementIcon;

  return (
    <div
      aria-live="polite"
      aria-atomic="true"
      className="
        fixed
        bottom-4
        left-1/2
        -translate-x-1/2
        z-[13000]
        pointer-events-none
      "
    >
      <div
        role="status"
        className="
          pointer-events-auto
          flex
          items-start
          gap-3
          min-w-[280px]
          max-w-[420px]
          rounded-xl
          bg-slate-900
          text-slate-50
          shadow-xl
          px-4
          py-3
          font-sans
          border
          border-slate-700
          transform
          transition
          duration-200
          ease-out
        "
      >
        <div
          className="
            flex
            h-10
            w-10
            shrink-0
            items-center
            justify-center
            self-center
            rounded-full
            border-2
            border-yellow-400
          "
        >
          <Icon className="h-5 w-5" />
        </div>

        <div className="flex-1">
          <div
            className="
              text-[11px]
              uppercase
              tracking-widest
              text-slate-300/80
              mb-0.5
            "
          >
            Achievement unlocked
          </div>
          <div className="text-sm font-semibold mb-1">
            {achievement.name}
          </div>
          <div className="text-xs text-slate-100/90">
            {achievement.description}
          </div>
        </div>

        <button
          type="button"
          onClick={onClose}
          aria-label="Dismiss achievement notification"
          className="
            ml-1
            border-none
            bg-transparent
            text-slate-300
            hover:text-slate-100
            transition
            text-base
            leading-none
            p-1
            cursor-pointer
          "
        >
          ✕
        </button>
      </div>
    </div>
  );
};
