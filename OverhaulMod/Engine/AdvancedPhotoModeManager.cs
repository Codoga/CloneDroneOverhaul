using OverhaulMod.UI;
using OverhaulMod.Utils;
using OverhaulMod.Visuals;

namespace OverhaulMod.Engine
{
    public class AdvancedPhotoModeManager : Singleton<AdvancedPhotoModeManager>, IGameLoadListener
    {
        [ModSetting(ModSettingsConstants.ADVANCED_PHOTO_MODE, true)]
        public static bool EnableAdvancedPhotoMode;

        [ModSetting(ModSettingsConstants.REQUIRE_RMB_HOLD_WHEN_UI_IS_HIDDEN, false)]
        public static bool RequireHoldingRMBWhenUIIsHidden;

        private LightingInfo m_nonEditedLightingInfo, m_editedLightingInfo;

        private bool m_isInPhotoMode, m_hasEverEnteredPhotoMode;

        public LevelLightSettings editingLevelLightSettings
        {
            get;
            set;
        }

        private void Start()
        {
            m_nonEditedLightingInfo = new LightingInfo();
            m_editedLightingInfo = new LightingInfo();
        }

        public void OnGameLoaded()
        {
            GlobalEventManager.Instance.AddEventListener("EnteredPhotoMode", onEnteredPhotoMode);
            GlobalEventManager.Instance.AddEventListener("ExitedPhotoMode", onExitedPhotoMode);
        }

        private void OnDestroy()
        {
            GlobalEventManager.Instance.RemoveEventListener("EnteredPhotoMode", onEnteredPhotoMode);
            GlobalEventManager.Instance.RemoveEventListener("ExitedPhotoMode", onExitedPhotoMode);
        }

        public void OnLevelLightSettingsChanged(LevelLightSettings changedLightSettings)
        {
            LevelLightSettings currentLightSettings = editingLevelLightSettings;
            if (currentLightSettings != changedLightSettings)
            {
                LevelLightSettings newLightSettings = LevelEditorLightManager.Instance.GetActiveLightSettings();
                if (newLightSettings != changedLightSettings)
                    return;
            }

            editingLevelLightSettings = changedLightSettings;
            m_nonEditedLightingInfo.SetValues(currentLightSettings);
        }

        public void RefreshLightingWithEditedInfo()
        {
            if (!m_isInPhotoMode)
                return;

            LevelLightSettings currentLevelLightSettings = editingLevelLightSettings;
            if (!currentLevelLightSettings)
                return;

            m_editedLightingInfo.ApplyValues(currentLevelLightSettings);
            LevelEditorLightManager.Instance.RefreshLightInScene();
        }

        public void RefreshLightingWithNormalInfo()
        {
            LevelLightSettings currentLevelLightSettings = editingLevelLightSettings;
            if (!currentLevelLightSettings)
            {
                PostEffectsManager.Instance.RefreshCameraPostEffects();
                return;
            }

            m_nonEditedLightingInfo.ApplyValues(currentLevelLightSettings);
            LevelEditorLightManager.Instance.RefreshLightInScene();
        }

        public void RestoreDefaults()
        {
            PhotoManager.Instance.OverridePausedTimeScale = 0.1f;
            Settings.SetDefaultSettings();

            LevelLightSettings currentLevelLightSettings = editingLevelLightSettings;
            if (!currentLevelLightSettings)
                return;

            m_nonEditedLightingInfo.ApplyValues(currentLevelLightSettings);
            m_editedLightingInfo.SetValues(currentLevelLightSettings);
            LevelEditorLightManager.Instance.RefreshLightInScene();
        }

        public LightingInfo GetNormalLightingInfo()
        {
            return m_nonEditedLightingInfo;
        }

        public LightingInfo GetEditedLightingInfo()
        {
            return m_editedLightingInfo;
        }

        public bool IsInPhotoMode()
        {
            return m_isInPhotoMode;
        }

        private void onEnteredPhotoMode()
        {
            if (!EnableAdvancedPhotoMode)
                return;

            m_isInPhotoMode = true;

            LevelLightSettings levelLightSettings = LevelEditorLightManager.Instance.GetActiveLightSettings();
            if (!levelLightSettings)
            {
                editingLevelLightSettings = null;
                return;
            }
            editingLevelLightSettings = levelLightSettings;

            m_nonEditedLightingInfo.SetValues(levelLightSettings);

            if (!m_hasEverEnteredPhotoMode)
            {
                m_editedLightingInfo.SetValues(levelLightSettings);
                Settings.SetDefaultSettings();

                m_hasEverEnteredPhotoMode = true;
            }

            RefreshLightingWithEditedInfo();
        }

        private void onExitedPhotoMode()
        {
            if (m_isInPhotoMode)
            {
                m_isInPhotoMode = false;

                RealisticLightingManager.Instance.PatchLevelLightSettings();
                RefreshLightingWithNormalInfo();

                UIPhotoModeUIRework photoModeUI = ModUIManager.Instance?.Get<UIPhotoModeUIRework>(AssetBundleConstants.UI, ModUIConstants.UI_PHOTO_MODE_UI_REWORK);
                if (photoModeUI)
                    photoModeUI.ResetEnvironmentFields();
            }
        }

        public static class Settings
        {
            public static bool overrideSettings
            {
                get
                {
                    AdvancedPhotoModeManager advancedPhotoModeManager = Instance;
                    return advancedPhotoModeManager && advancedPhotoModeManager.IsInPhotoMode();
                }
            }

            public static bool EnableVignette, EnableDithering, EnableSSAO;

            public static float VignetteIntensity;

            public static void SetDefaultSettings()
            {
                VignetteIntensity = 0.23f;
                EnableVignette = PostEffectsManager.EnableVignette;
                EnableDithering = PostEffectsManager.EnableDithering;
                EnableSSAO = PostEffectsManager.EnableSSAO;
            }
        }
    }
}
