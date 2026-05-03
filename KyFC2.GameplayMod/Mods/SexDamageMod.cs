using BaseMod.Core;
using BaseMod.Core.Extensions;
using Il2Cpp;
using KyFC2.GameplayMod.Components;
using MelonLoader;
using UnityEngine;
using UnityEngine.SceneManagement;
using static BaseMod.Core.ModConfig;

namespace KyFC2.GameplayMod.Mods;
internal class SexDamageMod
{
    #region Configuration
    internal static MelonPreferences_Entry<bool> Enabled;
    internal static MelonPreferences_Entry<bool> ArousalFatigue;
    internal static MelonPreferences_Entry<bool> ArousalMoans;
    internal static MelonPreferences_Entry<bool> ArousalStarter;
    #endregion

    #region States
    internal static bool IsModActive => Enabled.Value;
    #endregion

    internal static void Load(ModConfig config)
    {
        try
        {
            Enabled = config.Entry(nameof(SexDamageMod), nameof(Enabled), false,
                "Activates the modification", new AcceptableValueList<bool>([true, false]));
            ArousalFatigue = config.Entry(nameof(SexDamageMod), nameof(ArousalFatigue), true,
                "After cum character gain arouse slowly for a while", new AcceptableValueList<bool>([true, false]));
            ArousalMoans = config.Entry(nameof(SexDamageMod), nameof(ArousalMoans), true,
                "Moans speed of character will depends on arousal", new AcceptableValueList<bool>([true, false]));
            ArousalStarter = config.Entry(nameof(SexDamageMod), nameof(ArousalStarter), true,
                "If character has Sex Starter buff he will gain arousal slowly", new AcceptableValueList<bool>([true, false]));

        }
        catch (Exception ex)
        {
            GameplayMod.Log.Error(ex.Message);
        }
    }

    internal static int ArousalFatigueCheck(KYFC kyfc, int arousalDamage, bool isPlayer)
    {
        try
        {
            if (!Enabled.Value || !ArousalStarter.Value || SceneManager.GetActiveScene().buildIndex != 1)
            {
                return arousalDamage;
            }

            if (arousalDamage <= 0)
            {
                return 0;
            }

            GameObject gameObject = isPlayer ? kyfc.sexsystem.Player : kyfc.sexsystem.Enemy;
            if (gameObject is null || !gameObject.TryGetComponentWithCast(out KyFCCharacterModComponent characterModComponent))
            {
                return arousalDamage;
            }

            float deltaTime = Time.time - characterModComponent.LastCumTime;
            float rate = 1f;
            if (characterModComponent.IsCharacterSelfCum)
            {
                if (deltaTime < 15f)
                {
                    rate = 0.60f;
                }
                else if (deltaTime < 30f)
                {
                    rate = 0.80f;
                }
            }
            else
            {
                if (deltaTime < 15f)
                {
                    rate = 0.80f;
                }
            }

            float newArousal = arousalDamage * rate;
            Plugin.Log.Info($"{characterModComponent.CharacterSex.CharacterSO.CharacterName}: Arousal fatigue damage: {arousalDamage} -> {newArousal}");
            return Mathf.Max(1, Mathf.RoundToInt(newArousal));
        }
        catch (Exception ex)
        {
            GameplayMod.Log.Error(ex.Message);
            return arousalDamage;
        }
    }

    internal static int ArousalMoansCheck(int arousal, int maxArousal)
    {
        int defaultMoans = PlayerData.Instance.adultSettingsDATA.MoanRatio switch
        {
            0 => 2,
            2 => 9,
            _ => 5
        };

        try
        {
            if (!Enabled.Value || !ArousalMoans.Value || SceneManager.GetActiveScene().buildIndex != 1)
            {
                return defaultMoans;
            }

            if (maxArousal <= 0)
            {
                return defaultMoans;
            }

            int moansSpeed = 2;
            int incValue = 2 * Mathf.Min(4, Mathf.RoundToInt(arousal / (maxArousal * 0.2f)));
            moansSpeed += incValue;

            return moansSpeed;
        }
        catch (Exception ex)
        {
            GameplayMod.Log.Error(ex.Message);
            return defaultMoans;
        }
    }

    internal static int ArousalStarterCheck(KYFC kyfc, int arousalDamage, bool isPlayer)
    {
        try
        {
            if (!Enabled.Value || !ArousalFatigue.Value || SceneManager.GetActiveScene().buildIndex != 1)
            {
                return arousalDamage;
            }

            if (arousalDamage <= 0)
            {
                return 0;
            }

            Il2CppSystem.Collections.Generic.List<GameObject> activeBuffs = isPlayer ? kyfc.buffsystem.PlayerActiveBuffs : kyfc.buffsystem.EnemyActiveBuffs;
            bool isStarter = false;
            foreach (GameObject gameObject in activeBuffs)
            {
                if (gameObject.TryGetComponentWithCast(out Buff buff) && buff.BuffID == 5)
                {
                    isStarter = true;
                    break;
                }
            }

            float rate = 1f;
            if (isStarter)
            {
                rate = 0.85f;
            }

            float newArousal = arousalDamage * rate;
            KCharacter character = isPlayer ? kyfc.PlayerCharacterSO : kyfc.EnemyCharacterSO;
            Plugin.Log.Info($"{character.CharacterName}: Arousal starter damage: {arousalDamage} -> {newArousal}");
            return Mathf.Max(1, Mathf.RoundToInt(newArousal));
        }
        catch (Exception ex)
        {
            GameplayMod.Log.Error(ex.Message);
            return arousalDamage;
        }
    }
}
