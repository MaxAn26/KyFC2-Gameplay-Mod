using System;
using System.Collections.Generic;

using BaseMod.Core.Extensions;

using BepInEx.Configuration;

using UnityEngine;

namespace KyFC2.GameplayMod.Mods;
internal class AchievementFixMod {
    #region Configuration
    internal static ConfigEntry<bool> Enabled;
    #endregion

    #region States
    internal static bool IsModActive => Enabled.Value;
    #endregion

    internal static void Load(ConfigFile config) {
        try {
            Enabled = config.Bind(nameof(AchievementFixMod), nameof(Enabled), true,
                new ConfigDescription("Activates the modification", new AcceptableValueList<bool>([true, false])));

        } catch (Exception ex) {
            Plugin.Log.Error(ex.Message);
        }
    }

    internal static void ReportProgress( AchievementManager achievementManager, string achievementID, int amount = 1) {
        List<AchievementData> achievements = [];
        
        foreach(var obj in achievementManager.runtimeAchievements) {
            if (obj.achievementID.Equals(achievementID) && !obj.isUnlocked)
                achievements.Add(obj);
        }
        
        if (achievements.Count == 0)
            return;

        achievements.ForEach(achievement => {
            if (achievement.isProgressBased) {
                int previous = achievement.currentValue;
                Plugin.Log.Info($"Change value for achievement '{achievement.title}'. {previous} -> {previous + amount}");
                achievement.currentValue = Mathf.Min(achievement.currentValue + amount, achievement.targetValue);

                if (previous < achievement.targetValue && achievement.currentValue >= achievement.targetValue) {
                    Plugin.Log.Info($"Unlock achievement '{achievement.title}'");
                    achievementManager.UnlockAchievement(achievement);
                }
            } else {
                Plugin.Log.Info($"Unlock achievement '{achievement.title}'");
                achievementManager.UnlockAchievement(achievement);
            }
        });
    }
}