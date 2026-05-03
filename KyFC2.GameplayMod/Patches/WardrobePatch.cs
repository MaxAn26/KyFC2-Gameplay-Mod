using HarmonyLib;
using Il2Cpp;
using KyFC2.GameplayMod.Mods;

namespace KyFC2.GameplayMod.Patches;
internal class WardrobePatch
{
    internal static bool Prepare()
    {
        try
        {
            if (!RandomFemaleGenitalsMod.IsModActive)
            {
                return false;
            }

            return true;
        }
        catch (Exception)
        {
            GameplayMod.Log.Warning($"{nameof(WardrobePatch)} not applied due exeption");
            return false;
        }
    }

    [HarmonyPostfix]
    [HarmonyWrapSafe]
    [HarmonyPatch(typeof(Wardrobe2), nameof(Wardrobe2.LoadStuff))]
    static void Wardrobe2LoadStuffPostfix(Wardrobe2 __instance) => RandomFemaleGenitalsMod.Apply(__instance);
}
