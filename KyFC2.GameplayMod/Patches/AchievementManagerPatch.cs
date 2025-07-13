using System;

using HarmonyLib;

using KyFC2.GameplayMod.Mods;

namespace KyFC2.GameplayMod.Patches;
internal class AchievementManagerPatch {
    internal static bool Prepare() {
        try {
            if (!AchievementFixMod.IsModActive)
                return false;

            return true;
        } catch (Exception) {
            Plugin.Log.LogWarning($"{nameof(AchievementManagerPatch)} not applied due exeption");
            return false;
        }
    }

    [HarmonyPrefix]
    [HarmonyWrapSafe]
    [HarmonyPatch(typeof(AchievementManager), nameof(AchievementManager.ReportProgress))]
    static bool AchievementManagerReportProgressPrefix(AchievementManager __instance, string __0, int __1, bool __runOriginal) {
        AchievementFixMod.ReportProgress(__instance, __0, __1);

        if (!__runOriginal)
            return false;

        return false;
    }
}