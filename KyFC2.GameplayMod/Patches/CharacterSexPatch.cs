using System;

using HarmonyLib;

using KyFC2.GameplayMod.Components;
using KyFC2.GameplayMod.Mods;

namespace KyFC2.GameplayMod.Patches;
internal class CharacterSexPatch {
    internal static bool Prepare() {
        try {
            if (!SexMoveChoiceMod.IsModActive && !GlossEffectMod.IsModActive)
                return false;

            return true;
        } catch (Exception) {
            Plugin.Log.LogWarning($"{nameof(CharacterSexPatch)} not applied due exeption");
            return false;
        }
    }

    [HarmonyPostfix]
    [HarmonyWrapSafe]
    [HarmonyPatch(typeof(CharacterSex), nameof(CharacterSex.Start))]
    static void CharacterSexStartPostfix(CharacterSex __instance) {
        KyFCCharacterModComponent.RegisterClass(__instance);
    }
}