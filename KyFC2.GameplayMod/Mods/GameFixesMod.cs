using BaseMod.Core;
using BaseMod.Core.Extensions;
using Il2Cpp;
using MelonLoader;
using static BaseMod.Core.ModConfig;

namespace KyFC2.GameplayMod.Mods;
internal class GameFixesMod
{
    #region Configuration
    internal static MelonPreferences_Entry<bool> Enabled;
    internal static MelonPreferences_Entry<bool> RequiredUndress;
    internal static MelonPreferences_Entry<bool> ThreesomeAssistUndress;
    #endregion

    #region States
    internal static bool IsModActive => Enabled.Value;
    #endregion

    internal static void Load(ModConfig config)
    {
        try
        {
            Enabled = config.Entry(nameof(GameFixesMod), nameof(Enabled), false,
                "Activates the modification", new AcceptableValueList<bool>([true, false]));
            RequiredUndress = config.Entry(nameof(GameFixesMod), nameof(RequiredUndress), true,
                "Set for all positions requirement to undress", new AcceptableValueList<bool>([true, false]));
            ThreesomeAssistUndress = config.Entry(nameof(GameFixesMod), nameof(ThreesomeAssistUndress), true,
                "Undress assist character in threesome positions", new AcceptableValueList<bool>([true, false]));
        }
        catch (Exception ex)
        {
            GameplayMod.Log.Error(ex.Message);
        }
    }

    internal static void RequiredUndressFix(KYFC kyfc)
    {
        try
        {
            if (!Enabled.Value || !RequiredUndress.Value)
            {
                return;
            }

            foreach (SexMove move in kyfc.EnemySexPositionsSO)
            {
                if (move is null)
                {
                    continue;
                }

                move.RequiresUndresBot = true;
                move.RequiresUndresTop = true;
            }

            foreach (SexMove move in kyfc.BaseEnemySexPositionsSO)
            {
                if (move is null)
                {
                    continue;
                }

                move.RequiresUndresBot = true;
                move.RequiresUndresTop = true;
            }

            foreach (SexMove move in kyfc.TagEnemySexPositionsSO)
            {
                if (move is null)
                {
                    continue;
                }

                move.RequiresUndresBot = true;
                move.RequiresUndresTop = true;
            }

            foreach (SexMove move in kyfc.PlayerSexPositionsSO)
            {
                if (move is null)
                {
                    continue;
                }

                move.RequiresUndresBot = true;
                move.RequiresUndresTop = true;
            }

            foreach (SexMove move in kyfc.BasePlayerSexPositionsSO)
            {
                if (move is null)
                {
                    continue;
                }

                move.RequiresUndresBot = true;
                move.RequiresUndresTop = true;
            }

            foreach (SexMove move in kyfc.TagPlayerSexPositionsSO)
            {
                if (move is null)
                {
                    continue;
                }

                move.RequiresUndresBot = true;
                move.RequiresUndresTop = true;
            }
        }
        catch (Exception ex)
        {
            GameplayMod.Log.Error(ex.Message);
            return;
        }
    }

    internal static void ThreesomeAssistUndressFix(SexSystem sexSystem)
    {
        try
        {
            if (!Enabled.Value || !ThreesomeAssistUndress.Value)
            {
                return;
            }

            if (!sexSystem.IsThreesome || sexSystem.Assist is null || !sexSystem.Assist.TryGetComponentWithCast(out CharacterSex characterSex))
            {
                return;
            }

            characterSex.UndressBySlot(2, 2);
            characterSex.UndressBySlot(3, 2);
            characterSex.UndressBySlot(5, 2);
        }
        catch (Exception ex)
        {
            GameplayMod.Log.Error(ex.Message);
            return;
        }
    }
}
