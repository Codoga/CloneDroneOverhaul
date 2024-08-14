using OverhaulMod.Engine;
using OverhaulMod.UI;
using OverhaulMod.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OverhaulMod
{
    public class ModUIManager : Singleton<ModUIManager>, IGameLoadListener
    {
        [ModSetting(ModSettingsConstants.SHOW_CHAPTER_SELECTION_MENU_REWORK, true, ModSetting.Tag.UISetting)]
        public static bool ShowChapterSelectionMenuRework;

        [ModSetting(ModSettingsConstants.SHOW_ENDLESS_MODE_MENU, true, ModSetting.Tag.UISetting)]
        public static bool ShowEndlessModeMenu;

        [ModSetting(ModSettingsConstants.SHOW_CHALLENGES_MENU_REWORK, true, ModSetting.Tag.UISetting)]
        public static bool ShowChallengesMenuRework;

        [ModSetting(ModSettingsConstants.SHOW_WORKSHOP_BROWSER_REWORK, true, ModSetting.Tag.UISetting)]
        public static bool ShowWorkshopBrowserRework;

        [ModSetting(ModSettingsConstants.SHOW_ADVANCEMENTS_MENU_REWORK, true, ModSetting.Tag.UISetting)]
        public static bool ShowAdvancementsMenuRework;

        [ModSetting(ModSettingsConstants.SHOW_SETTINGS_MENU_REWORK, true, ModSetting.Tag.UISetting)]
        public static bool ShowSettingsMenuRework;

        [ModSetting(ModSettingsConstants.SHOW_TITLE_SCREEN_REWORK, true, ModSetting.Tag.UISetting)]
        public static bool ShowTitleScreenRework;

        [ModSetting(ModSettingsConstants.SHOW_DUEL_INVITE_MENU_REWORK, true, ModSetting.Tag.UISetting)]
        public static bool ShowDuelInviteMenuRework;

        private Dictionary<string, GameObject> m_instantiatedUIs;

        private List<OverhaulUIBehaviour> m_shownUIs;

        private Transform m_gameUIRootTransform;
        public Transform GameUIRootTransform
        {
            get
            {
                if (!m_gameUIRootTransform)
                {
                    m_gameUIRootTransform = ModCache.gameUIRoot.transform;
                }
                return m_gameUIRootTransform;
            }
        }

        public bool skipHidingCustomUIs
        {
            get;
            private set;
        }

        public Action actionToInvoke
        {
            get;
            private set;
        }

        public WindowManager windowManager
        {
            get;
            private set;
        }

        public override void Awake()
        {
            base.Awake();
            windowManager = base.gameObject.AddComponent<WindowManager>();
            m_instantiatedUIs = new Dictionary<string, GameObject>();
            m_shownUIs = new List<OverhaulUIBehaviour>();
        }

        private void OnDestroy()
        {
            Dictionary<string, GameObject> dictionary = m_instantiatedUIs;
            if (!dictionary.IsNullOrEmpty())
            {
                foreach (GameObject panel in dictionary.Values)
                    if (panel)
                        Destroy(panel);
            }
            dictionary.Clear();
        }

        public void OnGameLoaded()
        {
            onGameInitialized();
        }

        private void onGameInitialized()
        {
            List<string> keysToRemove = new List<string>();
            foreach (KeyValuePair<string, GameObject> keyValue in m_instantiatedUIs)
            {
                if (!keyValue.Value)
                    keysToRemove.Add(keyValue.Key);
            }
            foreach (string key in keysToRemove)
                _ = m_instantiatedUIs.Remove(key);

            _ = ModUIConstants.ShowVersionLabel();
            _ = ModUIConstants.ShowCinematicEffects();
            //_ = ModUIConstants.ShowImageEffects();

            if (ModFeatures.IsEnabled(ModFeatures.FeatureType.Tooltips))
                _ = ModUIConstants.ShowTooltips();
            if (ModFeatures.IsEnabled(ModFeatures.FeatureType.SubtitleTextFieldRework))
                _ = ModUIConstants.ShowSubtitleTextFieldRework();
        }

        public bool HasInstantiatedUI(string assetKey)
        {
            return m_instantiatedUIs.ContainsKey(assetKey);
        }

        public bool IsUIVisible(string assetBundle, string assetKey)
        {
            string fullName = $"{assetBundle}.{assetKey}";
            return HasInstantiatedUI(fullName) && m_instantiatedUIs[fullName].activeInHierarchy;
        }

        public int GetSiblingIndex(UILayer layer)
        {
            switch (layer)
            {
                case UILayer.Last:
                    return GameUIRootTransform.childCount;
                case UILayer.BeforeTitleScreen:
                    return ModCache.titleScreenUI.transform.GetSiblingIndex();
                case UILayer.AfterTitleScreen:
                    return ModCache.titleScreenUI.transform.GetSiblingIndex() + 1;
                case UILayer.BeforeEnergyUI:
                    return ModCache.gameUIRoot.EnergyUI.transform.GetSiblingIndex();
                case UILayer.AfterEnergyUI:
                    return ModCache.gameUIRoot.EnergyUI.transform.GetSiblingIndex() + 1;
                case UILayer.BeforeUpgradeUI:
                    return ModCache.gameUIRoot.UpgradeUI.transform.GetSiblingIndex();
                case UILayer.AfterUpgradeUI:
                    return ModCache.gameUIRoot.UpgradeUI.transform.GetSiblingIndex() + 1;
                case UILayer.BeforeEscMenu:
                    return ModCache.gameUIRoot.EscMenu.transform.GetSiblingIndex();
                case UILayer.AfterEscMenu:
                    return ModCache.gameUIRoot.EscMenu.transform.GetSiblingIndex() + 1;
                case UILayer.BeforeCrashScreen:
                    return ModCache.gameUIRoot.ErrorWindow.transform.GetSiblingIndex();
                case UILayer.AfterCrashScreen:
                    return ModCache.gameUIRoot.ErrorWindow.transform.GetSiblingIndex() + 1;
                case UILayer.BeforeMultiplayerConnectScreen:
                    return ModCache.gameUIRoot.MultiplayerConnectingScreen.transform.GetSiblingIndex();
                case UILayer.AfterMultiplayerConnectScreen:
                    return ModCache.gameUIRoot.MultiplayerConnectingScreen.transform.GetSiblingIndex() + 1;
            }
            return 0;
        }

        public T Show<T>(string assetBundle, string assetKey, UILayer layer = UILayer.Last, int siblingIndexOffset = 0) where T : OverhaulUIBehaviour
        {
            string fullName = $"{assetBundle}.{assetKey}";
            if (!HasInstantiatedUI(fullName))
            {
                GameObject prefab = ModResources.Prefab(assetBundle, assetKey);
                GameObject gameObject = Instantiate(prefab, GameUIRootTransform);
                gameObject.SetActive(true);
                m_instantiatedUIs.Add(fullName, gameObject);
                RectTransform transform = gameObject.transform as RectTransform;
                transform.SetSiblingIndex(GetSiblingIndex(layer) + siblingIndexOffset);
                transform.anchorMin = Vector2.zero;
                transform.anchorMax = Vector2.one;
                transform.sizeDelta = Vector2.zero;
                transform.anchoredPosition = Vector2.zero;
                transform.localScale = Vector3.one;

                T result1 = gameObject.AddComponent<T>();
                result1.Name = fullName;

#if DEBUG
                System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
                result1.InitializeUI();
                stopwatch.Stop();
                ModDebug.Log($"Initialized an UI in {stopwatch.ElapsedMilliseconds} ms, {stopwatch.ElapsedTicks} ticks");
#else
                result1.InitializeUI();
#endif
                result1.Show();

                if (result1.closeOnEscapeButtonPress)
                    m_shownUIs.Add(result1);

                return result1;
            }

            T result = m_instantiatedUIs[fullName].GetComponent<T>();
            result.Show();

            if (result.closeOnEscapeButtonPress)
                m_shownUIs.Add(result);

            return result;
        }

        public T Show<T>(string assetBundle, string assetKey, Transform parent) where T : OverhaulUIBehaviour
        {
            T result = Show<T>(assetBundle, assetKey, UILayer.Last);
            if (parent)
            {
                result.transform.SetParent(parent);
            }
            result.transform.SetAsLastSibling();
            return result;
        }

        public T Get<T>(string assetBundle, string assetKey) where T : OverhaulUIBehaviour
        {
            string fullName = $"{assetBundle}.{assetKey}";
            return m_instantiatedUIs.TryGetValue(fullName, out GameObject gameObject) ? (gameObject?.GetComponent<T>()) : null;
        }

        public bool Hide(string assetBundle, string assetKey)
        {
            OverhaulUIBehaviour overhaulUIBehaviour = Get<OverhaulUIBehaviour>(assetBundle, assetKey);
            if (overhaulUIBehaviour && overhaulUIBehaviour.visible && !overhaulUIBehaviour.forceCancelHide)
            {
                overhaulUIBehaviour.Hide();
                return true;
            }
            return false;
        }

        public OverhaulUIBehaviour GetLastShownUI()
        {
            if (m_shownUIs.Count == 0)
                return null;

            return m_shownUIs[m_shownUIs.Count - 1];
        }

        public void RemoveUIFromLastShown(OverhaulUIBehaviour behaviour)
        {
            _ = m_shownUIs.Remove(behaviour);
        }

        public void RefreshUI(bool refreshOnlyCursor)
        {
            ModCache.gameUIRoot.RefreshCursorEnabled();
            if (!refreshOnlyCursor)
            {
                ModCache.gameUIRoot.SetUIOverLogoModeEnabled(ShouldEnableUIOverLogoMode());
                ModCache.titleScreenUI.setLogoAndRootButtonsVisible(GameModeManager.IsOnTitleScreen() && !ShouldHideTitleScreen());
            }
        }

        public void HideLegacyMenuInsteadOfCustom(GameObject objectToTrack)
        {
            skipHidingCustomUIs = true;
            _ = ModActionUtils.RunCoroutine(letOriginalUIHideNextTime(objectToTrack));
        }

        public void InvokeActionInsteadOfHidingCustomUI(Action action)
        {
            actionToInvoke = action;
        }

        public bool TryInvokeAction()
        {
            if (actionToInvoke != null)
            {
                actionToInvoke.Invoke();
                actionToInvoke = null;
                return true;
            }
            return false;
        }

        private IEnumerator letOriginalUIHideNextTime(GameObject objectToTrack)
        {
            while (objectToTrack && objectToTrack.activeInHierarchy)
                yield return null;

            skipHidingCustomUIs = false;
            yield break;
        }

        public bool ShouldEnableCursor()
        {
            foreach (GameObject gameObject in m_instantiatedUIs.Values)
            {
                if (!gameObject || !gameObject.activeInHierarchy)
                    continue;

                OverhaulUIBehaviour behaviour = gameObject.GetComponent<OverhaulUIBehaviour>();
                if (behaviour && !behaviour.IsElement && behaviour.enableCursor)
                    return true;
            }
            return false;
        }

        public bool ShouldEnableUIOverLogoMode()
        {
            foreach (GameObject gameObject in m_instantiatedUIs.Values)
            {
                if (!gameObject || !gameObject.activeInHierarchy)
                    continue;

                OverhaulUIBehaviour behaviour = gameObject.GetComponent<OverhaulUIBehaviour>();
                if (behaviour && !behaviour.IsElement && behaviour.enableUIOverLogoMode)
                    return true;
            }
            return false;
        }

        public bool ShouldHideTitleScreen()
        {
            foreach (GameObject gameObject in m_instantiatedUIs.Values)
            {
                if (!gameObject || !gameObject.activeInHierarchy)
                    continue;

                OverhaulUIBehaviour behaviour = gameObject.GetComponent<OverhaulUIBehaviour>();
                if (behaviour && !behaviour.IsElement && behaviour.hideTitleScreen)
                    return true;
            }
            return false;
        }

        internal void RemoveFromList(OverhaulUIBehaviour uIBehaviour)
        {
            _ = m_instantiatedUIs.Remove(uIBehaviour.Name);
        }

        public enum UILayer
        {
            First,

            BeforeTitleScreen,
            AfterTitleScreen,

            BeforeEnergyUI,
            AfterEnergyUI,

            BeforeUpgradeUI,
            AfterUpgradeUI,

            BeforeMultiplayerConnectScreen,
            AfterMultiplayerConnectScreen,

            BeforeEscMenu,
            AfterEscMenu,

            BeforeCrashScreen,
            AfterCrashScreen,

            Last
        }

        public class WindowManager : MonoBehaviour
        {
            private ModdedObject m_windowPrefab;

            private Dictionary<string, WindowBehaviour> m_windows;

            private void Awake()
            {
                m_windows = new Dictionary<string, WindowBehaviour>();
                m_windowPrefab = ModResources.Prefab(AssetBundleConstants.UI, "WindowPrefab").GetComponent<ModdedObject>();
            }

            public string Window(Transform parent, Transform content, string title, Vector2 size, Vector2 position = default, bool destroyOnClose = false)
            {
                string windowId = Guid.NewGuid().ToString();
                ModdedObject moddedObject = Instantiate(m_windowPrefab, parent);
                moddedObject.gameObject.SetActive(true);
                WindowBehaviour windowBehaviour = moddedObject.gameObject.AddComponent<WindowBehaviour>();
                windowBehaviour.InitializeElement();
                windowBehaviour.SetSize(size, size == Vector2.one * -1f ? content : null);
                windowBehaviour.SetTitle(title);
                windowBehaviour.SetContents(content);
                windowBehaviour.windowId = windowId;
                windowBehaviour.destroyOnClose = destroyOnClose;
                (windowBehaviour.transform as RectTransform).anchoredPosition = position;
                m_windows.Add(windowId, windowBehaviour);
                return windowId;
            }

            public void ShowWindow(string windowId)
            {
                if (m_windows.TryGetValue(windowId, out WindowBehaviour windowBehaviour))
                {
                    if (windowBehaviour)
                        windowBehaviour.Show();
                }
            }

            public void HideWindow(string windowId)
            {
                if (m_windows.TryGetValue(windowId, out WindowBehaviour windowBehaviour))
                {
                    if (windowBehaviour)
                        windowBehaviour.Hide();
                }
            }

            public bool IsWindowShown(string windowId)
            {
                if (m_windows.TryGetValue(windowId, out WindowBehaviour windowBehaviour))
                {
                    if (windowBehaviour)
                        return windowBehaviour.gameObject.activeInHierarchy;
                }
                return false;
            }

            public WindowBehaviour GetWindow(string windowId)
            {
                if (m_windows.TryGetValue(windowId, out WindowBehaviour windowBehaviour))
                {
                    if (windowBehaviour)
                        return windowBehaviour;
                }
                return null;
            }

            public void RemoveWindow(string windowId)
            {
                _ = m_windows.Remove(windowId);
            }
        }

        public class WindowBehaviour : OverhaulUIBehaviour
        {
            [UIElement("TitleBar")]
            private readonly GameObject m_titleBar;

            [UIElement("TitleBarFrame")]
            private readonly GameObject m_titleBarFrame;

            [UIElement("TitleText")]
            private readonly Text m_titleText;

            [UIElementAction(nameof(Close))]
            [UIElement("CloseButton")]
            private readonly Button m_closeButton;

            [UIElementAction(nameof(ToggleMinimized))]
            [UIElement("HideButton")]
            private readonly Button m_hideButton;

            [UIElement("Content")]
            private readonly Transform m_content;

            private DraggablePanel m_draggablePanel;

            private UIElementMouseEventsComponent m_mouseEvents;

            private RectTransform m_rectTransform;

            private float m_width, m_height;

            private bool m_minimized;
            public bool minimized
            {
                get
                {
                    return m_minimized;
                }
                set
                {
                    m_minimized = value;
                    setMinimized(value);
                }
            }

            public string windowId
            {
                get;
                set;
            }

            public bool destroyOnClose
            {
                get;
                set;
            }

            protected override void OnInitialized()
            {
                base.OnInitialized();

                RectTransform rectTransform = base.transform as RectTransform;
                m_rectTransform = rectTransform;

                DraggablePanel draggablePanel = m_titleBar.AddComponent<DraggablePanel>();
                draggablePanel.SetTransform(rectTransform);
                draggablePanel.SetGoToFront(true);
                m_draggablePanel = draggablePanel;

                UIElementMouseEventsComponent mouseEventsComponent = m_titleBar.AddComponent<UIElementMouseEventsComponent>();
                mouseEventsComponent.doubleClickAction = ToggleMinimized;
                m_mouseEvents = mouseEventsComponent;
            }

            public override void OnDestroy()
            {
                base.OnDestroy();
                Instance.windowManager.RemoveWindow(windowId);
            }

            private void setMinimized(bool value)
            {
                Vector2 size = m_rectTransform.sizeDelta;
                size.y = value ? 34f : m_height + 45f;
                m_rectTransform.sizeDelta = size;
                m_titleBarFrame.SetActive(!value);
                m_content.gameObject.SetActive(!value);
            }

            public void SetTitle(string text)
            {
                m_titleText.text = text;
            }

            public void SetSize(Vector2 size, Transform content = null)
            {
                Vector2 sizeToSet;
                if (content && content is RectTransform rectTransform)
                    sizeToSet = new Vector2(rectTransform.sizeDelta.x, rectTransform.sizeDelta.y);
                else
                    sizeToSet = new Vector2(size.x, size.y);

                m_width = sizeToSet.x;
                m_height = sizeToSet.y;
                m_rectTransform.sizeDelta = new Vector2(size.x + 30f, size.y + 45f);
            }

            public void SetContents(Transform transform)
            {
                transform.gameObject.SetActive(true);
                transform.SetParent(m_content);
                transform.localScale = Vector3.one;
                transform.localEulerAngles = Vector3.zero;
                transform.localPosition = Vector3.zero;

                if (transform is RectTransform rectTransform)
                {
                    float widthToSet = m_width;
                    if (widthToSet == -1f)
                        widthToSet = rectTransform.sizeDelta.x;

                    float hightToSet = m_height;
                    if (hightToSet == -1f)
                        hightToSet = rectTransform.sizeDelta.y;

                    rectTransform.anchorMax = Vector2.one;
                    rectTransform.anchorMin = Vector2.zero;
                    rectTransform.pivot = Vector2.one * 0.5f;
                    rectTransform.sizeDelta = Vector2.zero;
                    rectTransform.anchoredPosition = Vector2.zero;

                    SetSize(new Vector2(widthToSet, hightToSet));
                }
            }

            public void ToggleMinimized()
            {
                minimized = !minimized;
            }

            public void Close()
            {
                if (!destroyOnClose)
                {
                    Hide();
                    return;
                }
                Destroy(base.gameObject);
            }
        }
    }
}
