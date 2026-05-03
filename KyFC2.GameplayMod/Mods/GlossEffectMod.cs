using BaseMod.Core;
using MelonLoader;
using static BaseMod.Core.ModConfig;

namespace KyFC2.GameplayMod.Mods;
internal class GlossEffectMod
{
    #region Configuration
    internal static MelonPreferences_Entry<bool> Enabled;
    internal static MelonPreferences_Entry<float> BaseGloss;
    internal static MelonPreferences_Entry<float> MaxGloss;
    #endregion

    #region States
    internal static bool IsModActive => Enabled.Value;
    #endregion

    internal static void Load(ModConfig config)
    {
        try
        {
            Enabled = config.Entry(nameof(GlossEffectMod), nameof(Enabled), false,
                "Activates the modification", new AcceptableValueList<bool>([true, false]));
            BaseGloss = config.Entry(nameof(GlossEffectMod), nameof(BaseGloss), 0.2f,
                "Base game gloss value", new AcceptableValueRange<float>(0.0f, 1.0f));
            MaxGloss = config.Entry(nameof(GlossEffectMod), nameof(MaxGloss), 0.9f,
                "Maximum gloss value", new AcceptableValueRange<float>(0.0f, 1.0f));

        }
        catch (Exception ex)
        {
            GameplayMod.Log.Error(ex.Message);
        }
    }

    internal static float? GetGlossValue(int currentValue, int maxValue)
    {
        try
        {
            if (!Enabled.Value)
            {
                return null;
            }

            float level = (float)currentValue / maxValue;
            float value = BaseGloss.Value + level * (MaxGloss.Value - BaseGloss.Value);
            return value;
        }
        catch (Exception ex)
        {
            GameplayMod.Log.Error(ex.Message);
            return null;
        }
    }
}
