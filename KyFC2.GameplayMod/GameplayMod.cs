using BaseMod.Core;
using KyFC2.GameplayMod;
using KyFC2.GameplayMod.Mods;
using KyFC2.GameplayMod.Patches;
using MelonLoader;
using MelonLoader.Utils;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

[assembly: MelonInfo(typeof(GameplayMod), ModInfo.MOD_NAME, ModInfo.MOD_VERSION, ModInfo.MOD_DEVELOPER, ModInfo.MOD_URL)]
[assembly: MelonGame(ModInfo.GAME_DEVELOPER, ModInfo.GAME_NAME)]

namespace KyFC2.GameplayMod;
public class GameplayMod : MelonMod
{
    internal static MelonLogger.Instance Log;
    internal static ModConfig ModConfig;
    internal static string ConfigPath;
    internal static string PluginAssets;
    internal static string PluginConfigs;
    internal static string PluginResources;

    public override void OnInitializeMelon()
    {
        // GameplayMod startup logic
        Log = LoggerInstance;
        ConfigPath = MelonEnvironment.UserDataDirectory;
        PluginAssets = Path.Combine(ConfigPath, ModInfo.MOD_GUID, "Assets");
        PluginConfigs = Path.Combine(ConfigPath, ModInfo.MOD_GUID, "Configs");
        PluginResources = Path.Combine(ConfigPath, ModInfo.MOD_GUID, "Resources");

        ModConfig = new($"{ModInfo.MOD_GUID}.cfg");

        GameFixesMod.Load(ModConfig);
        GlossEffectMod.Load(ModConfig);
        RandomFemaleGenitalsMod.Load(ModConfig);
        ReverseModeMod.Load(ModConfig);
        SexDamageMod.Load(ModConfig);
        SexMoveChoiceMod.Load(ModConfig);

        HarmonyInstance.PatchAll(typeof(CharacterSexPatch));
        HarmonyInstance.PatchAll(typeof(KyFCPatch));
        HarmonyInstance.PatchAll(typeof(SexSystemPatch));
        HarmonyInstance.PatchAll(typeof(WardrobePatch));


        SceneManager.sceneLoaded += (UnityAction<Scene, LoadSceneMode>)OnSceneLoaded;
        Log.Msg($"Mod {ModInfo.MOD_GUID} is loaded!");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Log.Msg($"Scene loaded: Name: {scene.name}, BuildIndex: {scene.buildIndex}");
        if (scene.buildIndex == 1)
        {
            SexMoveChoiceMod.Prepare();
        }
    }
}
