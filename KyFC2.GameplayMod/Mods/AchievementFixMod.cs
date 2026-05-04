using BaseMod.Core;
using Il2Cpp;
using MelonLoader;
using UnityEngine;
using static BaseMod.Core.ModConfig;

namespace KyFC2.GameplayMod.Mods;
internal class AchievementFixMod
{
    #region Configuration
    internal static MelonPreferences_Entry<bool> Enabled;
    #endregion

    #region States
    internal static bool IsModActive => Enabled.Value;
    #endregion

    internal static void Load(ModConfig config)
    {
        try
        {
            Enabled = config.Entry(nameof(AchievementFixMod), nameof(Enabled), true,
                "Activates the modification", new AcceptableValueList<bool>([true, false]));
        }
        catch (Exception ex)
        {
            GameplayMod.Log.Error(ex.Message);
        }
    }

    internal static void ReportProgress(AchievementManager achievementManager, string achievementID, int amount = 1)
    {
        if (!Enabled.Value)
        {
            return;
        }

        List<AchievementData> achievements = [];

        foreach (AchievementData obj in achievementManager.runtimeAchievements)
        {
            if (obj.achievementID.Equals(achievementID) && !obj.isUnlocked)
            {
                achievements.Add(obj);
            }
        }

        if (achievements.Count == 0)
        {
            return;
        }

        achievements.ForEach(achievement =>
        {
            if (achievement.isProgressBased)
            {
                int previous = achievement.currentValue;
                GameplayMod.Log.Msg($"Change value for achievement '{achievement.title}'. {previous} -> {previous + amount}");
                achievement.currentValue = Mathf.Min(achievement.currentValue + amount, achievement.targetValue);

                if (previous < achievement.targetValue && achievement.currentValue >= achievement.targetValue)
                {
                    GameplayMod.Log.Msg($"Unlock achievement '{achievement.title}'");
                    achievementManager.UnlockAchievement(achievement);
                }
            }
            else
            {
                GameplayMod.Log.Msg($"Unlock achievement '{achievement.title}'");
                achievementManager.UnlockAchievement(achievement);
            }
        });
    }
}
