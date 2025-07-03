using System;

using BaseMod.Core.Extensions;

using BepInEx.Configuration;

namespace KyFC2.GameplayMod.Mods;
internal class GlossEffectMod {
    #region Configuration
    internal static ConfigEntry<bool> Enabled;
    internal static ConfigEntry<float> BaseGloss;
    internal static ConfigEntry<float> MaxGloss;
    #endregion

    #region States
    internal static bool IsModActive => Enabled.Value;
    #endregion

    internal static void Load(ConfigFile config) {
        try {
            Enabled = config.Bind(nameof(GlossEffectMod), nameof(Enabled), false,
                new ConfigDescription("Activates the modification", new AcceptableValueList<bool>([true, false])));
            BaseGloss = config.Bind(nameof(GlossEffectMod), nameof(BaseGloss), 0.2f,
                new ConfigDescription("Base game gloss value", new AcceptableValueRange<float>(0.0f, 1.0f)));
            MaxGloss = config.Bind(nameof(GlossEffectMod), nameof(MaxGloss), 0.9f,
                new ConfigDescription("Maximum gloss value", new AcceptableValueRange<float>(0.0f, 1.0f)));

        } catch (Exception ex) {
            Plugin.Log.Error(ex.Message);
        }
    }

    internal static float? GetGlossValue(int currentValue, int maxValue) {
        try {
            if (!Enabled.Value)
                return null;

            float level = (float)currentValue / maxValue;
            float value = BaseGloss.Value + (level * (MaxGloss.Value - BaseGloss.Value));
            return value;
        } catch (Exception ex) {
            Plugin.Log.Error(ex.Message);
            return null;
        }
    }
}