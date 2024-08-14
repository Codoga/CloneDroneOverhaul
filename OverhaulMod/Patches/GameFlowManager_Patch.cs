using HarmonyLib;
using OverhaulMod.Utils;

namespace OverhaulMod.Patches
{
    [HarmonyPatch(typeof(GameFlowManager))]
    internal static class GameFlowManager_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameFlowManager.StartStoryModeGame))]
        private static void StartStoryModeGame_Prefix(GameFlowManager __instance, bool resetData = false)
        {
            if (!resetData)
            {
                GameDataManager gameDataManager = GameDataManager.Instance;
                if (gameDataManager)
                {
                    string levelId = gameDataManager._storyModeData.CurentLevelID;
                    if (!levelId.IsNullOrEmpty() && levelId.Contains("C5"))
                    {
                        ModGameUtils.overrideCurrentLevelId = levelId;
                        ModGameUtils.overrideActiveSections = gameDataManager._storyModeData.CurrentLevelSectionsVisible;
                    }
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(GameFlowManager.ShowTitleScreen))]
        private static void ShowTitleScreen_Postfix(GameFlowManager __instance)
        {
            if (ModFeatures.IsEnabled(ModFeatures.FeatureType.TitleScreenRework))
                _ = ModUIConstants.ShowTitleScreenRework();

            if (ModManagers.ShowModSetupScreenOnStart)
            {
                ModActionUtils.DoInFrames(delegate
                {
                    _ = ModUIConstants.ShowSettingsMenuRework(true);
                }, 10);
            }
            else
                ModUIUtils.ShowNewUpdateMessageOrChangelog(2f);
        }
    }
}
