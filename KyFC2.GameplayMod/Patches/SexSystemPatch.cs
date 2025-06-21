using System;

using HarmonyLib;

using KyFC2.GameplayMod.Mods;

namespace KyFC2.GameplayMod.Patches;
internal class SexSystemPatch {
    internal static bool Prepare() {
        try {
            if (!SexMoveChoiceMod.IsModActive && !ReverseModeMod.IsModActive && !GameFixesMod.IsModActive)
                return false;

            return true;
        } catch (Exception) {
            Plugin.Log.LogWarning($"{nameof(SexSystemPatch)} not applied due exeption");
            return false;
        }
    }

    [HarmonyPostfix]
    [HarmonyWrapSafe]
    [HarmonyPatch(typeof(SexSystem), nameof(SexSystem.Setup))]
    static void SexSystemSetupPostfix(SexSystem __instance) {
        if (__instance.IsThreesome)
            GameFixesMod.ThreesomeAssistUndressFix(__instance);
    }

    [HarmonyPrefix]
    [HarmonyWrapSafe]
    [HarmonyPatch(typeof(SexSystem), nameof(SexSystem.SetHeavyBondageAnimation))]
    static bool SexSystemSetHeavyBondageAnimationPrefix(SexSystem __instance, bool __runOriginal) {
        SexMoveChoiceMod.SetSexID(__instance);

        if (!__runOriginal)
            return false;

        return true;
    }

    [HarmonyPrefix]
    [HarmonyWrapSafe]
    [HarmonyPatch(typeof(SexSystem), nameof(SexSystem.SetSexAnimation))]
    static bool SexSystemSetSexAnimationPrefix(SexSystem __instance, bool __runOriginal) {
        SexMoveChoiceMod.SetSexID(__instance);

        if (!__runOriginal)
            return false;

        return true;
    }

    [HarmonyPostfix]
    [HarmonyWrapSafe]
    [HarmonyPatch(typeof(SexSystem), nameof(SexSystem.SetSexAnimation))]
    static void SexSystemSetSexAnimationPostfix(SexSystem __instance) {
        ReverseModeMod.Apply(__instance);
    }

    [HarmonyPrefix]
    [HarmonyWrapSafe]
    [HarmonyPatch(typeof(SexSystem), nameof(SexSystem.SetThreesomeAnimation))]
    static bool SexSystemSetThreesomeAnimationPrefix(SexSystem __instance, bool __runOriginal) {
        SexMoveChoiceMod.SetSexID(__instance);

        if (!__runOriginal)
            return false;

        return true;
    }

    [HarmonyPostfix]
    [HarmonyWrapSafe]
    [HarmonyPatch(typeof(SexSystem), nameof(SexSystem.SetThreesomeAnimation))]
    static void SexSystemSetThreesomeAnimationPostfix(SexSystem __instance) {
        ReverseModeMod.Apply(__instance);
    }

    [HarmonyPostfix]
    [HarmonyWrapSafe]
    [HarmonyPatch(typeof(SexSystem), nameof(SexSystem.Start))]
    static void SexSystemStartPostfix(SexSystem __instance) {
        GameFixesMod.RequiredUndressFix(__instance.kyfc);
    }
}