using LevelEditorPatch;
using OverhaulMod.Combat;
using OverhaulMod.Content;
using OverhaulMod.Content.LevelEditor;
using OverhaulMod.Content.Personalization;
using OverhaulMod.Engine;
using OverhaulMod.Patches.Behaviours;
using OverhaulMod.Utils;
using OverhaulMod.Visuals;
using OverhaulMod.Visuals.Environment;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace OverhaulMod
{
    public static class ModLoader
    {
        private static bool s_hasAddedObjects;

        public static void Load()
        {
            if (!HasToLoad())
            {
                ModManagers.Instance.TriggerModLoadedEvent();
                return;
            }

            loadAssemblies();
            loadGameUIThemeData();

            ModBuildInfo.Load();
            ModFeatures.CacheValues();

            QualitySettings.asyncUploadTimeSlice = 4;
            QualitySettings.asyncUploadBufferSize = 16;
            QualitySettings.asyncUploadPersistentBuffer = true;
            QualitySettings.softParticles = true;

            GameObject gameObject = new GameObject("OverhaulManagers", new Type[] { typeof(ModManagers) });
            UnityEngine.Object.DontDestroyOnLoad(gameObject);

            createDirectories();
            addManagers();
            ModManagers.Instance.TriggerModLoadedEvent();

            addLevelEditorObjects();
            addListeners();
        }

        public static void Unload()
        {
            ModManagers modManagers = ModManagers.Instance;
            if (modManagers && modManagers.gameObject)
            {
                UnityEngine.Object.Destroy(modManagers.gameObject);
            }
        }

        private static void addManagers()
        {
            _ = ModManagers.NewSingleton<ModSettingsDataManager>();
            _ = ModManagers.NewSingleton<ModSettingsManager>();
            _ = ModManagers.NewSingleton<ModDataManager>();
            _ = ModManagers.NewSingleton<RepositoryManager>();
            _ = ModManagers.NewSingleton<GoogleDriveManager>();
            _ = ModManagers.NewSingleton<ScheduledActionsManager>();
            _ = ModManagers.NewSingleton<ExclusiveContentManager>();
            _ = ModManagers.NewSingleton<ModLocalizationManager>();
            _ = ModManagers.NewSingleton<ModAudioManager>();
            _ = ModManagers.NewSingleton<ModResources>();
            _ = ModManagers.NewSingleton<ModUIManager>();
            _ = ModManagers.NewSingleton<ModLevelManager>();
            _ = ModManagers.NewSingleton<ModWeaponsManager>();
            _ = ModManagers.NewSingleton<ModUpgradesManager>();
            _ = ModManagers.NewSingleton<DifficultyTierManager>();
            _ = ModManagers.NewSingleton<ModGameModifiersManager>();
            _ = ModManagers.NewSingleton<TitleScreenCustomizationManager>();

            _ = ModManagers.NewSingleton<ModWebhookManager>();
            _ = ModManagers.NewSingleton<ExclusiveContentManager>();
            _ = ModManagers.NewSingleton<ContentManager>();
            _ = ModManagers.NewSingleton<UpdateManager>();
            _ = ModManagers.NewSingleton<NewsManager>();
            _ = ModManagers.NewSingleton<PersonalizationCacheManager>();
            _ = ModManagers.NewSingleton<PersonalizationManager>();
            _ = ModManagers.NewSingleton<PersonalizationEditorManager>();
            _ = ModManagers.NewSingleton<PersonalizationEditorObjectManager>();
            _ = ModManagers.NewSingleton<PersonalizationEditorCopyPasteManager>();
            _ = ModManagers.NewSingleton<PersonalizationItemVerificationManager>();
            _ = ModManagers.NewSingleton<PersonalizationEditorGuideManager>();
            _ = ModManagers.NewSingleton<PersonalizationMultiplayerManager>();
            _ = ModManagers.NewSingleton<PersonalizationEditorTemplateManager>();

            _ = ModManagers.NewSingleton<WeatherManager>();
            _ = ModManagers.NewSingleton<FloatingDustManager>();
            _ = ModManagers.NewSingleton<FadingVoxelManager>();

            _ = ModManagers.NewSingleton<PressActionKeyObjectManager>();
            _ = ModManagers.NewSingleton<ArenaRemodelManager>();
            _ = ModManagers.NewSingleton<ArenaAudienceManager>();
            _ = ModManagers.NewSingleton<LightingTransitionManager>();
            _ = ModManagers.NewSingleton<AdvancedPhotoModeManager>();
            _ = ModManagers.NewSingleton<UpgradeModesManager>();
            _ = ModManagers.NewSingleton<TransitionManager>();
            _ = ModManagers.NewSingleton<CameraManager>();
            _ = ModManagers.NewSingleton<RichPresenceManager>();
            _ = ModManagers.NewSingleton<PooledPrefabManager>();
            _ = ModManagers.NewSingleton<RealisticLightingManager>();
            _ = ModManagers.NewSingleton<ParticleManager>();
            _ = ModManagers.NewSingleton<PostEffectsManager>();
            if (ModFeatures.IsEnabled(ModFeatures.FeatureType.QuickReset)) _ = ModManagers.NewSingleton<QuickResetManager>();
            _ = ModManagers.NewSingleton<AutoBuildManager>();

            _ = ModManagers.NewSingleton<ModPhysicsManager>();
        }

        private static void loadAssemblies()
        {
            LevelEditorPatch.Patch.Apply();
            ModIntegrationUtils.Load();
        }

        private static void addLevelEditorObjects()
        {
            if (!s_hasAddedObjects)
            {
                Patch.AddObject("WeatherSettingsOverride", "OverhaulMod", "", GameObject.CreatePrimitive(PrimitiveType.Cylinder).transform, new Type[] { typeof(LevelEditorWeatherSettingsOverride) }, Path.Combine(ModCore.editorTexturesFolder, "WeatherSettingsOverride.png"));

                /*
                if (ModBuildInfo.debug)
                    Patch.AddObject("ArenaAudienceLinePoint", "OverhaulMod", "", GameObject.CreatePrimitive(PrimitiveType.Sphere).transform, new Type[] { typeof(ArenaAudienceLinePoint) }, null);*/

                s_hasAddedObjects = true;
            }
        }

        private static void createDirectories()
        {
            _ = ModIOUtils.CreateDirectoryIfNotExists(ModCore.modDataFolder);
            _ = ModIOUtils.CreateDirectoryIfNotExists(ModCore.contentFolder);
            _ = ModIOUtils.CreateDirectoryIfNotExists(ModCore.savesFolder);
            _ = ModIOUtils.CreateDirectoryIfNotExists(ModCore.addonsFolder);
            _ = ModIOUtils.CreateDirectoryIfNotExists(ModCore.customizationFolder);
            _ = ModIOUtils.CreateDirectoryIfNotExists(ModCore.customizationPersistentFolder);
        }

        private static void addListeners()
        {
            ModSettingsManager modSettingsManager = ModSettingsManager.Instance;
            modSettingsManager.AddSettingValueChangedListener(refreshCameraPostEffects, ModSettingsConstants.ENABLE_TITLE_BAR_OVERHAUL);
            modSettingsManager.AddSettingValueChangedListener(refreshCameraPostEffects, ModSettingsConstants.ENABLE_SSAO);
            modSettingsManager.AddSettingValueChangedListener(refreshCameraPostEffects, ModSettingsConstants.SSAO_INTENSITY);
            modSettingsManager.AddSettingValueChangedListener(refreshCameraPostEffects, ModSettingsConstants.SSAO_SAMPLE_COUNT);
            modSettingsManager.AddSettingValueChangedListener(refreshCameraPostEffects, ModSettingsConstants.ENABLE_CHROMATIC_ABERRATION);
            modSettingsManager.AddSettingValueChangedListener(refreshCameraPostEffects, ModSettingsConstants.CHROMATIC_ABERRATION_INTENSITY);
            modSettingsManager.AddSettingValueChangedListener(refreshCameraPostEffects, ModSettingsConstants.CHROMATIC_ABERRATION_ON_SCREEN_EDGES);
            modSettingsManager.AddSettingValueChangedListener(refreshCameraPostEffects, ModSettingsConstants.COLOR_BLINDNESS_MODE);
            modSettingsManager.AddSettingValueChangedListener(refreshCameraPostEffects, ModSettingsConstants.COLOR_BLINDNESS_AFFECT_UI);
            modSettingsManager.AddSettingValueChangedListener(refreshCameraPostEffects, ModSettingsConstants.ENABLE_DOF);
            modSettingsManager.AddSettingValueChangedListener(refreshCameraPostEffects, ModSettingsConstants.ENABLE_BLOOM);
            modSettingsManager.AddSettingValueChangedListener(refreshCameraPostEffects, ModSettingsConstants.TWEAK_BLOOM);
            modSettingsManager.AddSettingValueChangedListener(refreshCameraPostEffects, ModSettingsConstants.ENABLE_DITHERING);
            modSettingsManager.AddSettingValueChangedListener(refreshCameraPostEffects, ModSettingsConstants.ENABLE_VIGNETTE);
            modSettingsManager.AddSettingValueChangedListener(refreshCameraPostEffects, ModSettingsConstants.ENABLE_SUN_SHAFTS);
            modSettingsManager.AddSettingValueChangedListener(delegate (object obj)
            {
                PressActionKeyObjectManager manager = PressActionKeyObjectManager.Instance;
                if (manager && obj is bool boolVal)
                {
                    if (!boolVal)
                        manager.HideDescription();

                    foreach (LevelEditorUseButtonTrigger trigger in Resources.FindObjectsOfTypeAll<LevelEditorUseButtonTrigger>())
                    {
                        if (boolVal)
                        {
                            if (trigger._keyboardHint)
                                trigger._keyboardHint.Show(string.Empty);

                            PressActionKeyObjectManager.Instance.SetNearestTriggerNull();
                        }
                        else
                        {
                            trigger.destroyKeyboardHint();
                        }
                    }
                }
            }, ModSettingsConstants.ENABLE_PRESS_BUTTON_TRIGGER_DESCRIPTION_REWORK);

            modSettingsManager.AddSettingValueChangedListener(delegate (object obj)
            {
                ModActionUtils.DoInFrame(delegate
                {
                    EnergyUIPatchBehaviour energyUIPatch = GamePatchBehaviour.GetBehaviour<EnergyUIPatchBehaviour>();
                    if (energyUIPatch)
                        energyUIPatch.RefreshPatch();
                });
            }, ModSettingsConstants.ENERGY_UI_REWORK);
        }

        private static void refreshCameraPostEffects(object obj)
        {
            PostEffectsManager.Instance.RefreshCameraPostEffects();
        }

        private static void loadGameUIThemeData()
        {
            if (ModCache.gameUIThemeData)
                return;

            GameUIThemeData gameUIThemeData = null;
            foreach (SelectableUI selectableUi in Resources.FindObjectsOfTypeAll<SelectableUI>())
                if (selectableUi.GameThemeData && selectableUi.GameThemeData.SelectionCornerPrefab)
                {
                    foreach (var graphic in selectableUi.GameThemeData.SelectionCornerPrefab.GetComponentsInChildren<Graphic>(true))
                        graphic.raycastTarget = false;

                    gameUIThemeData = selectableUi.GameThemeData;
                    break;
                }

            if (gameUIThemeData)
            {
                gameUIThemeData.ButtonBackground[0].Color = new Color(0.19f, 0.37f, 0.88f, 1);
                gameUIThemeData.ButtonBackground[1].Color = new Color(0.3f, 0.5f, 1, 1f);
                gameUIThemeData.ButtonTextOutline[0].Color = new Color(0.1f, 0.1f, 0.1f, 0.7f);
                gameUIThemeData.ButtonTextOutline[1].Color = new Color(0.1f, 0.1f, 0.1f, 0.6f);
                ModCache.gameUIThemeData = gameUIThemeData;
            }
        }

        public static bool HasToLoad()
        {
            return !ModManagers.Instance;
        }
    }
}
