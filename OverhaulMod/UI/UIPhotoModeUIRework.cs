using OverhaulMod.Engine;
using OverhaulMod.Utils;
using OverhaulMod.Visuals;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OverhaulMod.UI
{
    public class UIPhotoModeUIRework : OverhaulUIBehaviour
    {
        [UIElement("ExpandButton", typeof(UIElementExpandButton))]
        private readonly UIElementExpandButton m_expandButton;

        [UIElement("LightingPanel")]
        private readonly RectTransform m_lightingPanel;

        [UIElementAction(nameof(OnSaveRLightInfoButtonClicked))]
        [UIElement("SaveRLightInfoButton")]
        private readonly Button m_saveRLightInfoButton;

        [UIElementAction(nameof(OnRestoreDefaultsButtonClicked))]
        [UIElement("RestoreDefaultsButton")]
        private readonly Button m_restoreDefaultsButton;

        [UIElementCallback(true)]
        [UIElementAction(nameof(OnTimeScaleChanged))]
        [UIElement("TimeScaleSlider")]
        private readonly Slider m_timeScaleSlider;


        [UIElementAction(nameof(OnShowPlayerToggled))]
        [UIElement("ShowPlayerToggle")]
        private readonly Toggle m_showPlayerToggle;

        [UIElementAction(nameof(OnShowEnemiesToggled))]
        [UIElement("ShowEnemiesToggle")]
        private readonly Toggle m_showEnemiesToggle;

        [UIElementAction(nameof(OnShowGarbageToggled))]
        [UIElement("ShowGarbageToggle")]
        private readonly Toggle m_showGarbageToggle;

        [UIElementAction(nameof(OnHUDToggled))]
        [UIElement("ShowHUDToggle")]
        private readonly Toggle m_showHUDToggle;


        [UIElementAction(nameof(OnCinematicBordersToggled))]
        [UIElement("CinematicBordersToggle")]
        private readonly Toggle m_cinematicBordersToggle;

        [UIElementAction(nameof(OnCinematicBordersHeightChanged))]
        [UIElement("CinematicBordersHeightSlider")]
        private readonly Slider m_cinematicBordersHeightSlider;

        [UIElementAction(nameof(OnSSAOToggled))]
        [UIElement("SSAOToggle")]
        private readonly Toggle m_ambientOcclusionToggle;

        [UIElementAction(nameof(OnVignetteToggled))]
        [UIElement("VignetteToggle")]
        private readonly Toggle m_vignetteToggle;

        [UIElementAction(nameof(OnVignetteIntensityChanged))]
        [UIElement("VignetteIntensitySlider")]
        private readonly Slider m_vignetteIntensitySlider;

        [UIElementAction(nameof(OnDitheringToggled))]
        [UIElement("DitheringToggle")]
        private readonly Toggle m_ditheringToggle;


        [UIElementAction(nameof(OnFogToggled))]
        [UIElement("FogToggle")]
        private readonly Toggle m_fogToggle;

        [ColorPicker(false)]
        [UIElementAction(nameof(OnFogColored))]
        [UIElement("FogColor")]
        private readonly UIElementColorPickerButton m_fogColor;

        [UIElementAction(nameof(OnFogStartChanged))]
        [UIElement("FogStartSlider")]
        private readonly Slider m_fogStartSlider;

        [UIElementAction(nameof(OnFogEndChanged))]
        [UIElement("FogEndSlider")]
        private readonly Slider m_fogEndSlider;


        [UIElementAction(nameof(OnDirectionalLightToggled))]
        [UIElement("DirectionalLightToggle")]
        private readonly Toggle m_directionalLightToggle;

        [ColorPicker(false)]
        [UIElementAction(nameof(OnDirectionalLightColored))]
        [UIElement("DirectionalLightColor")]
        private readonly UIElementColorPickerButton m_directionalLightColor;

        [UIElementAction(nameof(OnDirectionalLightXChanged))]
        [UIElement("DirectionalLightX")]
        private readonly Slider m_directionalLightXSlider;

        [UIElementAction(nameof(OnDirectionalLightYChanged))]
        [UIElement("DirectionalLightY")]
        private readonly Slider m_directionalLightYSlider;

        [UIElementAction(nameof(OnDirectionalLightIntensityChanged))]
        [UIElement("DirectionalLightIntensity")]
        private readonly Slider m_directionalLightIntensitySlider;

        [UIElementAction(nameof(OnDirectionalLightShadowsChanged))]
        [UIElement("DirectionalLightShadows")]
        private readonly Slider m_directionalLightShadowsSlider;


        [UIElementAction(nameof(OnSkyBoxIndexChanged))]
        [UIElement("SkyboxSlider")]
        private readonly Slider m_skyBoxSlider;

        [UIElementAction(nameof(OnUseRealisticSkyBoxesToggled))]
        [UIElement("RealisticSkyboxToggle")]
        private readonly Toggle m_realisticSkyBoxToggle;

        [UIElementAction(nameof(OnRealisticSkyBoxIndexChanged))]
        [UIElement("RealisticSkyboxSlider")]
        private readonly Slider m_realisticSkyBoxSlider;

        private LightingInfo m_lightingInfo;

        private bool m_disallowCallbacks;

        private List<GarbageTarget> m_garbageTargets;

        public override bool closeOnEscapeButtonPress => false;

        protected override void OnInitialized()
        {
            RectTransform lightingPanel = m_lightingPanel;
            lightingPanel.sizeDelta = new Vector2(225f, 300f);

            UIElementExpandButton expandButton = m_expandButton;
            expandButton.rectTransform = lightingPanel;
            expandButton.collapsedSize = lightingPanel.sizeDelta;
            expandButton.expandedSize = new Vector2(375f, 300f);

            m_saveRLightInfoButton.gameObject.SetActive(ModUserInfo.isDeveloper);
        }

        public override void Show()
        {
            base.Show();
            ModActionUtils.DoInFrame(setFieldsValues);
        }

        public override void Hide()
        {
            base.Hide();
        }

        public void ResetEnvironmentFields()
        {
            m_showEnemiesToggle.isOn = true;
            m_showPlayerToggle.isOn = true;
            m_showHUDToggle.isOn = true;
            m_showGarbageToggle.isOn = true;
        }

        private void setFieldsValues()
        {
            LightingInfo lightingInfo = AdvancedPhotoModeManager.Instance.GetEditedLightingInfo();
            if (lightingInfo == null)
            {
                m_lightingInfo = default;
                return;
            }
            m_lightingInfo = lightingInfo;

            m_disallowCallbacks = true;

            UICinematicEffects cinematicEffects = UICinematicEffects.instance;
            if (!cinematicEffects)
            {
                m_cinematicBordersHeightSlider.value = 100f;
                m_cinematicBordersToggle.isOn = false;
            }
            else
            {
                m_cinematicBordersHeightSlider.value = cinematicEffects.bordersHeight;
                m_cinematicBordersToggle.isOn = cinematicEffects.borders;
            }

            m_timeScaleSlider.value = Mathf.Round(Mathf.Clamp01(PhotoManager.Instance.OverridePausedTimeScale) * 10f);

            m_vignetteToggle.isOn = AdvancedPhotoModeManager.Settings.EnableVignette;
            m_ditheringToggle.isOn = AdvancedPhotoModeManager.Settings.EnableDithering;
            m_ambientOcclusionToggle.isOn = AdvancedPhotoModeManager.Settings.EnableSSAO;
            m_vignetteIntensitySlider.value = AdvancedPhotoModeManager.Settings.VignetteIntensity;

            m_fogToggle.isOn = lightingInfo.FogEnabled;
            m_fogColor.color = lightingInfo.FogColor;
            m_fogStartSlider.value = lightingInfo.FogStartDistance;
            m_fogEndSlider.value = lightingInfo.FogEndDistance;

            m_directionalLightToggle.isOn = lightingInfo.EnableDirectionalLight;
            m_directionalLightColor.color = lightingInfo.DirectionalColor;
            m_directionalLightXSlider.value = lightingInfo.DirectionalRotationX;
            m_directionalLightYSlider.value = lightingInfo.DirectionalRotationY;
            m_directionalLightIntensitySlider.value = lightingInfo.DirectionalIntensity;
            m_directionalLightShadowsSlider.value = lightingInfo.DirectionalShadowStrength;

            m_skyBoxSlider.value = lightingInfo.SkyboxIndex;

            RealisticLightingInfo realisticLightingInfo = RealisticLightingManager.Instance.GetCurrentRealisticLightingInfo();
            if (realisticLightingInfo == null)
            {
                m_realisticSkyBoxToggle.isOn = false;
                m_realisticSkyBoxSlider.value = -1;
            }
            else
            {
                m_realisticSkyBoxToggle.isOn = realisticLightingInfo.SkyboxIndex != -1;
                m_realisticSkyBoxSlider.value = realisticLightingInfo.SkyboxIndex;
            }

            m_disallowCallbacks = false;
        }

        private void toggleRobot(FirstPersonMover firstPersonMover, bool value)
        {
            if (!firstPersonMover)
                return;

            if (value)
                firstPersonMover.ShowTemporarilyHiddenWeaponModels();
            else
                firstPersonMover.TemporarilyHideWeaponModels();

            CharacterModel characterModel = firstPersonMover.GetCharacterModel();
            if (characterModel)
            {
                if (value)
                    characterModel.ShowAllHiddenBodyPartsAndArmor();
                else
                    characterModel.HideAllBodyPartsandArmor();
            }
        }

        public void OnSaveRLightInfoButtonClicked()
        {
            RealisticLightingManager.Instance.SaveCurrentLightingInfo(Mathf.RoundToInt(m_realisticSkyBoxSlider.value));
        }

        public void OnRestoreDefaultsButtonClicked()
        {
            AdvancedPhotoModeManager.Instance.RestoreDefaults();
            UICinematicEffects cinematicEffects = UICinematicEffects.instance;
            if (cinematicEffects)
            {
                cinematicEffects.borders = false;
                cinematicEffects.bordersHeight = 100f;
            }
            setFieldsValues();
        }

        public void OnTimeScaleChanged(float value)
        {
            if (m_disallowCallbacks)
                return;

            PhotoManager.Instance.OverridePausedTimeScale = Mathf.Clamp(value / 10f, 0.1f, 1f);
        }

        public void OnShowPlayerToggled(bool value)
        {
            toggleRobot(CharacterTracker.Instance.GetPlayerRobot(), value);
        }

        public void OnShowEnemiesToggled(bool value)
        {
            foreach (Character character in CharacterTracker.Instance.GetAllLivingCharacters())
            {
                if (!character || character.IsMainPlayer() || !(character is FirstPersonMover firstPersonMover))
                    continue;

                toggleRobot(firstPersonMover, value);
            }
        }

        public void OnShowGarbageToggled(bool value)
        {
            List<GarbageTarget> list = value ? m_garbageTargets : GarbageManager.Instance.GetAllGarbageReadyForCollection();
            if (list.IsNullOrEmpty())
                return;

            if (!value)
            {
                m_garbageTargets = list;

                foreach (GarbageTarget t in list)
                    if (t)
                        t.gameObject.SetActive(false);
            }
            else
            {
                foreach (GarbageTarget t in list)
                    if (t)
                        t.gameObject.SetActive(true);
            }
        }

        public void OnHUDToggled(bool value)
        {
            GameUIRoot.Instance.SetPlayerHUDVisible(value && !CutSceneManager.Instance.IsInCutscene());
        }

        public void OnCinematicBordersToggled(bool value)
        {
            m_cinematicBordersHeightSlider.gameObject.SetActive(value);
            if (m_disallowCallbacks)
                return;

            UICinematicEffects cinematicEffects = UICinematicEffects.instance;
            if (cinematicEffects)
                cinematicEffects.borders = value;
        }

        public void OnCinematicBordersHeightChanged(float value)
        {
            if (m_disallowCallbacks)
                return;

            UICinematicEffects cinematicEffects = UICinematicEffects.instance;
            if (cinematicEffects)
                cinematicEffects.bordersHeight = value;
        }

        public void OnSSAOToggled(bool value)
        {
            if (m_disallowCallbacks)
                return;

            AdvancedPhotoModeManager.Settings.EnableSSAO = value;
            PostEffectsManager.Instance.RefreshCameraPostEffects();
        }

        public void OnVignetteToggled(bool value)
        {
            m_vignetteIntensitySlider.gameObject.SetActive(value);
            if (m_disallowCallbacks)
                return;

            AdvancedPhotoModeManager.Settings.EnableVignette = value;
            PostEffectsManager.Instance.RefreshCameraPostEffects();
        }

        public void OnVignetteIntensityChanged(float value)
        {
            if (m_disallowCallbacks)
                return;

            AdvancedPhotoModeManager.Settings.VignetteIntensity = value;
            PostEffectsManager.Instance.RefreshCameraPostEffects();
        }

        public void OnDitheringToggled(bool value)
        {
            if (m_disallowCallbacks)
                return;

            AdvancedPhotoModeManager.Settings.EnableDithering = value;
            PostEffectsManager.Instance.RefreshCameraPostEffects();
        }

        public void OnFogToggled(bool value)
        {
            if (m_disallowCallbacks)
                return;

            m_lightingInfo.FogEnabled = value;
            AdvancedPhotoModeManager.Instance.RefreshLightingWithEditedInfo();
        }

        public void OnFogColored(Color value)
        {
            if (m_disallowCallbacks)
                return;

            m_lightingInfo.FogColor = value;
            AdvancedPhotoModeManager.Instance.RefreshLightingWithEditedInfo();
        }

        public void OnFogStartChanged(float value)
        {
            if (m_disallowCallbacks)
                return;

            m_lightingInfo.FogStartDistance = value;
            AdvancedPhotoModeManager.Instance.RefreshLightingWithEditedInfo();
        }

        public void OnFogEndChanged(float value)
        {
            if (m_disallowCallbacks)
                return;

            m_lightingInfo.FogEndDistance = value;
            AdvancedPhotoModeManager.Instance.RefreshLightingWithEditedInfo();
        }

        public void OnDirectionalLightToggled(bool value)
        {
            if (m_disallowCallbacks)
                return;

            m_lightingInfo.EnableDirectionalLight = value;
            AdvancedPhotoModeManager.Instance.RefreshLightingWithEditedInfo();
        }

        public void OnDirectionalLightColored(Color value)
        {
            if (m_disallowCallbacks)
                return;

            m_lightingInfo.DirectionalColor = value;
            AdvancedPhotoModeManager.Instance.RefreshLightingWithEditedInfo();
        }

        public void OnDirectionalLightXChanged(float value)
        {
            if (m_disallowCallbacks)
                return;

            m_lightingInfo.DirectionalRotationX = value;
            AdvancedPhotoModeManager.Instance.RefreshLightingWithEditedInfo();
        }

        public void OnDirectionalLightYChanged(float value)
        {
            if (m_disallowCallbacks)
                return;

            m_lightingInfo.DirectionalRotationY = value;
            AdvancedPhotoModeManager.Instance.RefreshLightingWithEditedInfo();
        }

        public void OnSkyBoxIndexChanged(float value)
        {
            if (m_disallowCallbacks)
                return;

            m_lightingInfo.SkyboxIndex = Mathf.RoundToInt(value);
            AdvancedPhotoModeManager.Instance.RefreshLightingWithEditedInfo();
        }

        public void OnUseRealisticSkyBoxesToggled(bool value)
        {
            if (m_disallowCallbacks)
                return;

            m_lightingInfo.RealisticSkyboxIndex = value ? Mathf.RoundToInt(m_realisticSkyBoxSlider.value) : -1;
            AdvancedPhotoModeManager.Instance.RefreshLightingWithEditedInfo();
        }

        public void OnRealisticSkyBoxIndexChanged(float value)
        {
            if (m_disallowCallbacks)
                return;

            m_lightingInfo.RealisticSkyboxIndex = m_realisticSkyBoxToggle.isOn ? Mathf.RoundToInt(value) : -1;
            AdvancedPhotoModeManager.Instance.RefreshLightingWithEditedInfo();
        }

        public void OnDirectionalLightIntensityChanged(float value)
        {
            if (m_disallowCallbacks)
                return;

            m_lightingInfo.DirectionalIntensity = value;
            AdvancedPhotoModeManager.Instance.RefreshLightingWithEditedInfo();
        }

        public void OnDirectionalLightShadowsChanged(float value)
        {
            if (m_disallowCallbacks)
                return;

            m_lightingInfo.DirectionalShadowStrength = value;
            AdvancedPhotoModeManager.Instance.RefreshLightingWithEditedInfo();
        }
    }
}
