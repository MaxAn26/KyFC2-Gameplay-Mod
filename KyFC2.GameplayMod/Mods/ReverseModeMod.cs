using BaseMod.Core;
using BaseMod.Core.Utils;
using Il2Cpp;
using MelonLoader;
using UnityEngine.SceneManagement;
using static BaseMod.Core.ModConfig;

namespace KyFC2.GameplayMod.Mods;
internal class ReverseModeMod
{
    #region Configuration
    internal static MelonPreferences_Entry<bool> Enabled;
    internal static MelonPreferences_Entry<bool> AlwaysAtSameRoles;
    internal static MelonPreferences_Entry<int> Chance;
    #endregion

    #region States
    internal static bool IsModActive => Enabled.Value;
    #endregion

    internal static void Load(ModConfig config)
    {
        try
        {
            Enabled = config.Entry(nameof(ReverseModeMod), nameof(Enabled), false,
                "Activates the modification", new AcceptableValueList<bool>([true, false]));
            AlwaysAtSameRoles = config.Entry(nameof(ReverseModeMod), nameof(AlwaysAtSameRoles), true,
                "Always activate when the enemy's role matches the player's role", new AcceptableValueList<bool>([true, false]));
            Chance = config.Entry(nameof(ReverseModeMod), nameof(Chance), 30,
                "Chance for animation reversal", new AcceptableValueRange<int>(0, 100));
        }
        catch (Exception ex)
        {
            GameplayMod.Log.Error(ex.Message);
        }
    }

    internal static void Apply(SexSystem sexSystem)
    {
        try
        {
            if (!Enabled.Value || SceneManager.GetActiveScene().buildIndex != 1)
            {
                return;
            }

            if (sexSystem.playerSex.IsBoundHeavyRestraint > 0 || SexSystem.GameOver)
            {
                return;
            }

            if (AlwaysAtSameRoles.Value && sexSystem.CasterActive == sexSystem.TargetActive || RandomUtils.Chance(Chance.Value))
            {
                SexSystem.ReverseMode = !SexSystem.ReverseMode;
                GameplayMod.Log.Msg("Reverse mod activated");
            }
        }
        catch (Exception ex)
        {
            GameplayMod.Log.Error(ex.Message);
            return;
        }
    }
}
