using System;

using BaseMod.Core.Extensions;
using BaseMod.Core.Utils;

using BepInEx.Configuration;

using UnityEngine.SceneManagement;

namespace KyFC2.GameplayMod.Mods;
internal class ReverseModeMod {
    #region Configuration
    internal static ConfigEntry<bool> Enabled;
    internal static ConfigEntry<bool> AlwaysAtSameRoles;
    internal static ConfigEntry<int> Chance;
    #endregion

    #region States
    internal static bool IsModActive => Enabled.Value;
    #endregion

    internal static void Load(ConfigFile config) {
        try {
            Enabled = config.Bind(nameof(ReverseModeMod), nameof(Enabled), false,
                new ConfigDescription("Activates the modification", new AcceptableValueList<bool>([true, false])));
            AlwaysAtSameRoles = config.Bind(nameof(ReverseModeMod), nameof(AlwaysAtSameRoles), true,
                new ConfigDescription("Always activate when the enemy's role matches the player's role", new AcceptableValueList<bool>([true, false])));
            Chance = config.Bind(nameof(ReverseModeMod), nameof(Chance), 30,
                new ConfigDescription("Chance for animation reversal", new AcceptableValueRange<int>(0, 100)));

        } catch (Exception ex) {
            Plugin.Log.Error(ex.Message);
        }
    }

    internal static void Apply(SexSystem sexSystem) {
        try {
            if (!Enabled.Value || SceneManager.GetActiveScene().buildIndex != 1)
                return;

            if (sexSystem.playerSex.IsBoundHeavyRestraint > 0 || SexSystem.GameOver)
                return;

            if ((AlwaysAtSameRoles.Value && sexSystem.CasterActive == sexSystem.TargetActive) || RandomUtils.Chance(Chance.Value)) {
                SexSystem.ReverseMode = !SexSystem.ReverseMode;
                Plugin.Log.Info("Reverse mod activated");
            }
        } catch (Exception ex) {
            Plugin.Log.Error(ex.Message);
            return;
        }
    }
}