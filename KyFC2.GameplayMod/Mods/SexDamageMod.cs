using System;

using BaseMod.Core.Extensions;

using BepInEx.Configuration;

using KyFC2.GameplayMod.Components;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace KyFC2.GameplayMod.Mods;
internal class SexDamageMod {
    #region Configuration
    internal static ConfigEntry<bool> Enabled;
    internal static ConfigEntry<bool> ArousalFatigue;
    internal static ConfigEntry<bool> ArousalStarter;
    #endregion

    #region States
    internal static bool IsModActive => Enabled.Value;
    #endregion

    internal static void Load(ConfigFile config) {
        try {
            Enabled = config.Bind(nameof(SexDamageMod), nameof(Enabled), false,
                new ConfigDescription("Activates the modification", new AcceptableValueList<bool>([true, false])));
            ArousalFatigue = config.Bind(nameof(SexDamageMod), nameof(ArousalFatigue), true,
                new ConfigDescription("After cum character gain arouse slowly for a while", new AcceptableValueList<bool>([true, false])));
            ArousalStarter = config.Bind(nameof(SexDamageMod), nameof(ArousalStarter), true,
                new ConfigDescription("If character has Sex Starter buff he will gain arousal slowly", new AcceptableValueList<bool>([true, false])));

        } catch (Exception ex) {
            Plugin.Log.Error(ex.Message);
        }
    }

    internal static int ArousalFatigueCheck(KYFC kyfc, int arousalDamage, bool isPlayer) {
        try {
            if (!Enabled.Value || !ArousalStarter.Value || SceneManager.GetActiveScene().buildIndex != 1)
                return arousalDamage;

            if (arousalDamage <= 0)
                return 0;

            GameObject gameObject = isPlayer ? kyfc.sexsystem.Player : kyfc.sexsystem.Enemy;
            if (gameObject is null || !gameObject.TryGetComponentWithCast(out KyFCCharacterModComponent characterModComponent))
                return arousalDamage;

            float deltaTime = Time.time - characterModComponent.LastCumTime;
            float rate = 1f;
            if (characterModComponent.IsCharacterSelfCum) {
                if (deltaTime < 15f) {
                    rate = 0.5f;
                } else if (deltaTime < 30f) {
                    rate = 0.75f;
                }
            } else {
                if (deltaTime < 15f) {
                    rate = 0.75f;
                }
            }

            float newArousal = arousalDamage * rate;
            return Mathf.Max(1, Mathf.RoundToInt(newArousal));
        } catch (Exception ex) {
            Plugin.Log.Error(ex.Message);
            return arousalDamage;
        }
    }

    internal static int ArousalStarterCheck(KYFC kyfc, int arousalDamage, bool isPlayer) {
        try {
            if (!Enabled.Value || !ArousalFatigue.Value || SceneManager.GetActiveScene().buildIndex != 1)
                return arousalDamage;

            if (arousalDamage <= 0)
                return 0;

            var activeBuffs = isPlayer ? kyfc.buffsystem.PlayerActiveBuffs : kyfc.buffsystem.EnemyActiveBuffs;
            bool isStarter = false;
            foreach (var gameObject in activeBuffs) {
                if (gameObject.TryGetComponentWithCast(out Buff buff) && buff.BuffID == 5) {
                    isStarter = true;
                    break;
                }
            }

            float rate = 1f;
            if (isStarter) {
                rate = 0.75f;
            }

            float newArousal = arousalDamage * rate;
            return Mathf.Max(1, Mathf.RoundToInt(newArousal));
        } catch (Exception ex) {
            Plugin.Log.Error(ex.Message);
            return arousalDamage;
        }
    }
}