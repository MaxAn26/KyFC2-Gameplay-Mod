using System;

using BaseMod.Core.Extensions;
using BaseMod.Core.Utils;

using BepInEx.Configuration;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace KyFC2.GameplayMod.Mods;
internal class RandomFemaleGenitalsMod {
    #region Configuration
    internal static ConfigEntry<bool> Enabled;
    internal static ConfigEntry<int> ChanceForFuta;
    internal static ConfigEntry<int> ChanceForFullFuta;
    #endregion

    #region States
    internal static bool IsModActive => Enabled.Value;
    #endregion

    internal static void Load(ConfigFile config) {
        try {
            Enabled = config.Bind(nameof(RandomFemaleGenitalsMod), nameof(Enabled), false,
                new ConfigDescription("Activates the modification", new AcceptableValueList<bool>([true, false])));
            ChanceForFuta = config.Bind(nameof(RandomFemaleGenitalsMod), nameof(ChanceForFuta), 35,
                new ConfigDescription("Chance for female character with active or mixed role become futanari", new AcceptableValueRange<int>(0, 100)));
            ChanceForFullFuta = config.Bind(nameof(RandomFemaleGenitalsMod), nameof(ChanceForFullFuta), 50,
                new ConfigDescription("Chance for female futa character get full futa (dick + balls)", new AcceptableValueRange<int>(0, 100)));

        } catch (Exception ex) {
            Plugin.Log.Error(ex.Message);
        }
    }

    internal static void Apply(Wardrobe2 wardrobe) {
        try {
            if (!Enabled.Value || SceneManager.GetActiveScene().buildIndex != 1)
                return;

            if (wardrobe.isPlayer || wardrobe.CharacterSO.IsMale)
                return;

            if (wardrobe.CharacterSO.PreferredRole >= 1) {
                if (RandomUtils.Chance(ChanceForFuta.Value)) {
                    Plugin.Log.Info($"{wardrobe.CharacterSO.CharacterName} will use a dick");

                    wardrobe.SkinDick.sharedMesh = RandomUtils.Chance(ChanceForFullFuta.Value) ? wardrobe.DickMesh : wardrobe.DickHalfMesh;
                    Material material = UnityEngine.Object.Instantiate(wardrobe.DickMatF);
                    wardrobe.SkinDick.material = material;
                    var color = wardrobe.SkinCharacter.material.GetColor("_Albedo_Tint");
                    material.SetColor("_Albedo_Tint", color);
                } else {
                    Plugin.Log.Info($"{wardrobe.CharacterSO.CharacterName} will use strapon");

                    wardrobe.SkinDick.sharedMesh = wardrobe.StrapMesh;
                    Material material = UnityEngine.Object.Instantiate(wardrobe.StrapMat);
                    wardrobe.SkinDick.material = material;
                    var color = wardrobe.CharacterSO.StrapOnColor;
                    material.SetColor("_Albedo_Tint", color);
                }
            }
        } catch (Exception ex) {
            Plugin.Log.Error(ex.Message);
            return;
        }
    }
}