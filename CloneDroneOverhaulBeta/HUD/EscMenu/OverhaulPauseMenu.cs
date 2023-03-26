﻿using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace CDOverhaul.HUD
{
    public class OverhaulPauseMenu : OverhaulUI
    {
        [OverhaulSetting("Game interface.Gameplay.New pause menu", true, !OverhaulVersion.TechDemo2Enabled, "The full redesign with new features implemented")]
        public static bool UseThisMenu;

        public static bool ForceUseOldMenu;

        #region Open/Close menu

        private static OverhaulPauseMenu m_Instance;

        private float m_TargetFoV = 60f;
        private bool m_IsAnimatingCamera;

        public static void ToggleMenu()
        {
            if (!m_Instance.AllowToggleMenu)
            {
                return;
            }

            if (m_Instance.gameObject.activeSelf)
            {
                m_Instance.Hide();
                return;
            }
            m_Instance.Show();
        }

        private Animator m_CameraAnimator;
        private Camera m_Camera;

        private float m_TimeMenuChangedItsState;
        public bool AllowToggleMenu => Time.unscaledTime >= m_TimeMenuChangedItsState + 0.45f;

        #endregion

        private Button m_PersonalizationButton;
        private Transform m_PersonalizationPanel;
        private Button m_PersonalizationSkinsButton;

        private Button m_ExitButton;
        private Transform m_ExitSelectPanel;
        private Button m_ExitSelectToMainMenuButton;
        private Button m_ExitSelectToDesktopButton;

        private Button m_AdvancementsButton;
        private Text m_AdvCompletedText;
        private Image m_AdvFillImage;

        private Button m_SettingsButton;
        private Transform m_SettingsSelectPanel;
        private Button m_GameSettingsButton;
        private Button m_ModSettingsButton;

        private OverhaulParametersMenu m_Parameters;

        public override void Initialize()
        {
            m_Instance = this;

            m_PersonalizationButton = MyModdedObject.GetObject<Button>(0);
            m_PersonalizationButton.onClick.AddListener(OnPersonalizationButtonClicked);
            m_PersonalizationPanel = MyModdedObject.GetObject<Transform>(1);
            m_PersonalizationSkinsButton = MyModdedObject.GetObject<Button>(2);
            m_PersonalizationSkinsButton.onClick.AddListener(OnSkinsButtonClicked);

            m_ExitButton = MyModdedObject.GetObject<Button>(4);
            m_ExitButton.onClick.AddListener(OnExitClicked);
            m_ExitSelectPanel = MyModdedObject.GetObject<Transform>(5);
            m_ExitSelectToMainMenuButton = MyModdedObject.GetObject<Button>(6);
            m_ExitSelectToMainMenuButton.onClick.AddListener(OnMainMenuClicked);
            m_ExitSelectToDesktopButton = MyModdedObject.GetObject<Button>(7);
            m_ExitSelectToDesktopButton.onClick.AddListener(OnDesktopClicked);

            m_AdvancementsButton = MyModdedObject.GetObject<Button>(8);
            m_AdvancementsButton.onClick.AddListener(OnAdvClicked);
            m_AdvFillImage = MyModdedObject.GetObject<Image>(9);
            m_AdvCompletedText = MyModdedObject.GetObject<Text>(10);

            m_SettingsButton = MyModdedObject.GetObject<Button>(11);
            m_SettingsButton.onClick.AddListener(OnSettingsClicked);
            m_SettingsSelectPanel = MyModdedObject.GetObject<Transform>(12);
            m_GameSettingsButton = MyModdedObject.GetObject<Button>(13);
            m_GameSettingsButton.onClick.AddListener(OnGameSettingsClicked);
            m_ModSettingsButton = MyModdedObject.GetObject<Button>(14);
            m_ModSettingsButton.onClick.AddListener(OnModSettingsClicked);

            MyModdedObject.GetObject<Button>(16).onClick.AddListener(OnContinueClicked);
            MyModdedObject.GetObject<Button>(15).onClick.AddListener(delegate
            {
                Transform t = TransformUtils.FindChildRecursive(GameUIRoot.Instance.EscMenu.transform, "SettingsButton(Clone)");
                if (t != null)
                {
                    Button b = t.GetComponent<Button>();
                    if (b != null)
                    {
                        b.onClick.Invoke();
                    }
                }
            });

            Hide();
        }

        protected override void OnDisposed()
        {
            base.OnDisposed();
        }

        public void AlignTransformY(Transform targetTransform, Transform transformToUse)
        {
            targetTransform.position = new Vector3(targetTransform.position.x, transformToUse.position.y, targetTransform.position.z);
        }

        public void SetPanelActive(Transform t, Transform caller, bool value)
        {
            if (value)
            {
                AlignTransformY(t, caller.transform);
            }
            t.gameObject.SetActive(value);
        }

        #region Personalization

        public void OnPersonalizationButtonClicked()
        {
            SetPanelActive(m_PersonalizationPanel, m_PersonalizationButton.transform, !m_PersonalizationPanel.gameObject.activeSelf);
        }

        public void OnSkinsButtonClicked()
        {
            WeaponSkinsMenu menu = GetController<WeaponSkinsMenu>();
            if (menu == null)
            {
                return;
            }

            // Todo: Notify player about unpausing the game OR make it possible to change skins while under arena
            Hide();

            menu.SetMenuActive(true);
        }

        #endregion

        #region Exit

        public void OnExitClicked()
        {
            SetPanelActive(m_ExitSelectPanel, m_ExitButton.transform, !m_ExitSelectPanel.gameObject.activeSelf);
        }

        public void OnMainMenuClicked()
        {
            SceneTransitionManager.Instance.DisconnectAndExitToMainMenu();
        }

        public void OnDesktopClicked()
        {
            Application.Quit();
        }

        #endregion

        #region Settings

        public void OnSettingsClicked()
        {
            SetPanelActive(m_SettingsSelectPanel, m_SettingsButton.transform, !m_SettingsSelectPanel.gameObject.activeSelf);
        }

        public void OnGameSettingsClicked()
        {
            SetPanelActive(m_SettingsSelectPanel, null, false);
            GameUIRoot.Instance.SettingsMenu.Show();
            _ = StaticCoroutineRunner.StartStaticCoroutine(settingsCoroutine());
            HideMenu(true);
        }

        private IEnumerator settingsCoroutine()
        {
            yield return new WaitUntil(() => !GameUIRoot.Instance.SettingsMenu.gameObject.activeSelf);
            Show();
            yield break;
        }

        public void OnModSettingsClicked()
        {
            if(m_Parameters == null)
            {
                m_Parameters = GetController<OverhaulParametersMenu>();
                if(m_Parameters == null || m_Parameters.IsDisposedOrDestroyed() || m_Parameters.HadBadStart)
                {
                    return;
                }
            }

            SetPanelActive(m_SettingsSelectPanel, null, false);
            HideMenu(true);
            m_Parameters.Show();
            _ = StaticCoroutineRunner.StartStaticCoroutine(modSettingsCoroutine());
        }

        private IEnumerator modSettingsCoroutine()
        {
            yield return new WaitUntil(() => !m_Parameters.gameObject.activeSelf);
            Show();
            yield break;
        }

        #endregion

        #region Advancements

        public void RefreshAdvancements()
        {
            int completed = 0;
            GameplayAchievementManager manager = GameplayAchievementManager.Instance;
            int all = manager.Achievements.Length;
            int i = 0;
            do
            {
                completed += manager.Achievements[i].IsComplete() ? 1 : 0;
                i++;

            } while (i < all);

            m_AdvFillImage.fillAmount = completed / all;
            m_AdvCompletedText.text = "Completed:  " + completed + " of " + all;
        }

        public void OnAdvClicked()
        {
            HideMenu(true);
            GameUIRoot.Instance.AchievementProgressUI.Show();
            _ = StaticCoroutineRunner.StartStaticCoroutine(advCoroutine());
        }

        private IEnumerator advCoroutine()
        {
            yield return new WaitUntil(() => !GameUIRoot.Instance.AchievementProgressUI.gameObject.activeSelf);
            Show();
            yield break;
        }

        #endregion

        public void OnContinueClicked()
        {
            if (!AllowToggleMenu)
            {
                return;
            }

            Hide();
        }

        public void Show()
        {
            m_TimeMenuChangedItsState = Time.unscaledTime;
            base.gameObject.SetActive(true);
            animateCamera();

            TimeManager.Instance.OnGamePaused();

            RefreshAdvancements();

            m_PersonalizationButton.interactable = !GameModeManager.IsInLevelEditor();

            ShowCursor = true;
        }

        public void HideMenu(bool dontUnpause = false)
        {
            if(!dontUnpause) TimeManager.Instance.OnGameUnPaused();
            m_TimeMenuChangedItsState = Time.unscaledTime;
            base.gameObject.SetActive(false);

            SetPanelActive(m_PersonalizationPanel, null, false);
            SetPanelActive(m_ExitSelectPanel, null, false);
            SetPanelActive(m_SettingsSelectPanel, null, false);

            if (!m_IsAnimatingCamera && m_CameraAnimator != null)
            {
                _ = StaticCoroutineRunner.StartStaticCoroutine(animateCameraCoroutine(m_Camera, m_CameraAnimator, true));
            }

            if (!dontUnpause) ShowCursor = false;
        }

        public void Hide()
        {
            HideMenu(false);
        }

        private void Update()
        {
            if (AllowToggleMenu && Input.GetKeyDown(KeyCode.Alpha0))
            {
                ForceUseOldMenu = true;
                Hide();
                GameUIRoot.Instance.EscMenu.Show();
                GameUIRoot.Instance.RefreshCursorEnabled();
                ForceUseOldMenu = false;
            }
        }

        #region Camera Animation

        private void animateCamera()
        {
            if (CharacterTracker.Instance == null)
            {
                return;
            }

            Character player = CharacterTracker.Instance.GetPlayer();
            if(player == null)
            {
                return;
            }

            m_Camera = player.GetPlayerCamera();
            if(m_Camera == null)
            {
                return;
            }

            m_CameraAnimator = m_Camera.GetComponentInParent<Animator>();
            if (m_CameraAnimator == null)
            {
                return;
            }

            if (m_IsAnimatingCamera)
            {
                return;
            }

            m_TargetFoV = m_Camera.fieldOfView;
            _ = StaticCoroutineRunner.StartStaticCoroutine(animateCameraCoroutine(m_Camera, m_CameraAnimator, false));
        }

        private IEnumerator animateCameraCoroutine(Camera camera, Animator animator, bool animatorState)
        {
            m_IsAnimatingCamera = true;
            int iterations = 20;
            if (!animatorState)
            {
                if (animator != null) animator.enabled = false;
                while (iterations > -1)
                {
                    iterations--;
                    if (camera != null) camera.fieldOfView += (40 - camera.fieldOfView) * 0.5f * (Time.unscaledDeltaTime * 22);
                    yield return null;
                }
            }
            else
            {
                while (iterations > -1)
                {
                    iterations--;
                    if (camera != null) camera.fieldOfView += (m_TargetFoV - camera.fieldOfView) * 0.5f * (Time.unscaledDeltaTime * 22);
                    yield return null;
                }
                if(animator != null) animator.enabled = true;
            }
            m_IsAnimatingCamera = false;
            yield break;
        }

        #endregion
    }
}