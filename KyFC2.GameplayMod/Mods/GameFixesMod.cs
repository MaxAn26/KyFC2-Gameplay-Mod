using System;

using BaseMod.Core.Extensions;

using BepInEx.Configuration;

namespace KyFC2.GameplayMod.Mods;
internal class GameFixesMod {
    #region Configuration
    internal static ConfigEntry<bool> Enabled;
    internal static ConfigEntry<bool> RequiredUndress;
    internal static ConfigEntry<bool> ThreesomeAssistUndress;
    #endregion

    #region States
    internal static bool IsModActive => Enabled.Value;
    #endregion

    internal static void Load(ConfigFile config) {
        try {
            Enabled = config.Bind(nameof(GameFixesMod), nameof(Enabled), false,
                new ConfigDescription("Activates the modification", new AcceptableValueList<bool>([true, false])));
            RequiredUndress = config.Bind(nameof(GameFixesMod), nameof(RequiredUndress), true,
                new ConfigDescription("Set for all positions requirement to undress", new AcceptableValueList<bool>([true, false])));
            ThreesomeAssistUndress = config.Bind(nameof(GameFixesMod), nameof(ThreesomeAssistUndress), true,
                new ConfigDescription("Undress assist character in threesome positions", new AcceptableValueList<bool>([true, false])));
        } catch (Exception ex) {
            Plugin.Log.Error(ex.Message);
        }
    }

    internal static void RequiredUndressFix(KYFC kyfc) {
        try {
            if (!Enabled.Value || !RequiredUndress.Value)
                return;

            foreach(var move in kyfc.EnemySexPositionsSO) {
                if (move is null)
                    continue;

                move.RequiresUndresBot = true;
                move.RequiresUndresTop = true;
            }

            foreach (var move in kyfc.BaseEnemySexPositionsSO) {
                if (move is null)
                    continue;

                move.RequiresUndresBot = true;
                move.RequiresUndresTop = true;
            }

            foreach (var move in kyfc.TagEnemySexPositionsSO) {
                if (move is null)
                    continue;
                
                move.RequiresUndresBot = true;
                move.RequiresUndresTop = true;
            }

            foreach (var move in kyfc.PlayerSexPositionsSO) {
                if (move is null)
                    continue;
                
                move.RequiresUndresBot = true;
                move.RequiresUndresTop = true;
            }

            foreach (var move in kyfc.BasePlayerSexPositionsSO) {
                if (move is null)
                    continue;
                
                move.RequiresUndresBot = true;
                move.RequiresUndresTop = true;
            }

            foreach (var move in kyfc.TagPlayerSexPositionsSO) {
                if (move is null)
                    continue;
                
                move.RequiresUndresBot = true;
                move.RequiresUndresTop = true;
            }
        } catch (Exception ex) {
            Plugin.Log.Error(ex.Message);
            return;
        }
    }

    internal static void ThreesomeAssistUndressFix(SexSystem sexSystem) {
        try {
            if (!Enabled.Value || !ThreesomeAssistUndress.Value)
                return;

            if (!sexSystem.IsThreesome || sexSystem.Assist is null || !sexSystem.Assist.TryGetComponentWithCast(out CharacterSex characterSex))
                return;

            characterSex.UndressBySlot( 2, 2);
            characterSex.UndressBySlot(3, 2);
            characterSex.UndressBySlot(5, 2);
        } catch (Exception ex) {
            Plugin.Log.Error(ex.Message);
            return;
        }
    }
}