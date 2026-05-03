using HarmonyLib;
using Il2Cpp;
using KyFC2.GameplayMod.Mods;

namespace KyFC2.GameplayMod.Patches;
internal class KyFCPatch
{
    internal static bool Prepare()
    {
        try
        {
            if (!SexDamageMod.IsModActive)
            {
                return false;
            }

            return true;
        }
        catch (Exception)
        {
            GameplayMod.Log.Warning($"{nameof(KyFCPatch)} not applied due exeption");
            return false;
        }
    }

    [HarmonyPrefix]
    [HarmonyWrapSafe]
    [HarmonyPatch(typeof(KYFC), nameof(KYFC.DamageArousalEnemy))]
    static bool KYFCDamageArousalEnemyPrefix(KYFC __instance, bool __runOriginal, ref int __0)
    {
        __0 = SexDamageMod.ArousalFatigueCheck(__instance, __0, false);
        __0 = SexDamageMod.ArousalStarterCheck(__instance, __0, false);

        if (!__runOriginal)
        {
            return false;
        }

        return true;
    }

    [HarmonyPrefix]
    [HarmonyWrapSafe]
    [HarmonyPatch(typeof(KYFC), nameof(KYFC.DamageArousalPlayer))]
    static bool KYFCDamageArousalPlayerPrefix(KYFC __instance, bool __runOriginal, ref int __0)
    {
        __0 = SexDamageMod.ArousalFatigueCheck(__instance, __0, true);
        __0 = SexDamageMod.ArousalStarterCheck(__instance, __0, true);

        if (!__runOriginal)
        {
            return false;
        }

        return true;
    }
}
