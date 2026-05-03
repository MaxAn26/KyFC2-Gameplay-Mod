using BaseMod.Core;
using BaseMod.Core.Utils;
using Il2Cpp;
using MelonLoader;
using UnityEngine;
using UnityEngine.SceneManagement;
using static BaseMod.Core.ModConfig;

namespace KyFC2.GameplayMod.Mods;
internal class RandomFemaleGenitalsMod
{
    #region Configuration
    internal static MelonPreferences_Entry<bool> Enabled;
    internal static MelonPreferences_Entry<int> ChanceForFuta;
    internal static MelonPreferences_Entry<int> ChanceForFullFuta;
    #endregion

    #region States
    internal static bool IsModActive => Enabled.Value;
    #endregion

    internal static void Load(ModConfig config)
    {
        try
        {
            Enabled = config.Entry(nameof(RandomFemaleGenitalsMod), nameof(Enabled), false,
                "Activates the modification", new AcceptableValueList<bool>([true, false]));
            ChanceForFuta = config.Entry(nameof(RandomFemaleGenitalsMod), nameof(ChanceForFuta), 35,
                "Chance for female character with active or mixed role become futanari", new AcceptableValueRange<int>(0, 100));
            ChanceForFullFuta = config.Entry(nameof(RandomFemaleGenitalsMod), nameof(ChanceForFullFuta), 50,
                "Chance for female futa character get full futa (dick + balls)", new AcceptableValueRange<int>(0, 100));

        }
        catch (Exception ex)
        {
            GameplayMod.Log.Error(ex.Message);
        }
    }

    internal static void Apply(Wardrobe2 wardrobe)
    {
        try
        {
            if (!Enabled.Value || SceneManager.GetActiveScene().buildIndex != 1)
            {
                return;
            }

            if (wardrobe.isPlayer || wardrobe.CharacterSO.IsMale)
            {
                return;
            }

            if (wardrobe.CharacterSO.PreferredRole >= 1)
            {
                if (RandomUtils.Chance(ChanceForFuta.Value))
                {
                    GameplayMod.Log.Msg($"{wardrobe.CharacterSO.CharacterName} will use a dick");

                    wardrobe.SkinDick.sharedMesh = RandomUtils.Chance(ChanceForFullFuta.Value) ? wardrobe.DickMesh : wardrobe.DickHalfMesh;
                    Material material = UnityEngine.Object.Instantiate(wardrobe.DickMatF);
                    wardrobe.SkinDick.material = material;
                    Color color = wardrobe.SkinCharacter.material.GetColor("_Albedo_Tint");
                    material.SetColor("_Albedo_Tint", color);
                }
                else
                {
                    GameplayMod.Log.Msg($"{wardrobe.CharacterSO.CharacterName} will use strapon");

                    wardrobe.SkinDick.sharedMesh = wardrobe.StrapMesh;
                    Material material = UnityEngine.Object.Instantiate(wardrobe.StrapMat);
                    wardrobe.SkinDick.material = material;
                    Color color = wardrobe.CharacterSO.StrapOnColor;
                    material.SetColor("_Albedo_Tint", color);
                }
            }
        }
        catch (Exception ex)
        {
            GameplayMod.Log.Error(ex.Message);
            return;
        }
    }
}
