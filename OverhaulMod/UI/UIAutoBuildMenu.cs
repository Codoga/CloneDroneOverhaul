using InternalModBot;
using OverhaulMod.Engine;
using OverhaulMod.Utils;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static System.Runtime.CompilerServices.RuntimeHelpers;

namespace OverhaulMod.UI
{
    public class UIAutoBuildMenu : OverhaulUIBehaviour
    {
        [UIElementAction(nameof(Hide))]
        [UIElement("CloseButton")]
        private readonly Button m_exitButton;

        [UIElementAction(nameof(OnCloseUpgradeUIButtonClicked))]
        [UIElement("CloseUpgradeUIButton", false)]
        private readonly Button m_closeUpgradeUIButton;

        [UIElementAction(nameof(OnResetUpgradesButtonClicked))]
        [UIElement("ResetUpgradesButton", false)]
        private readonly Button m_resetUpgradesButton;

        [UIElementAction(nameof(OnClearButtonClicked))]
        [UIElement("ClearButton")]
        private readonly Button m_clearButton;

        [UIElementAction(nameof(OnSelectBuildToUseOnMatchStartButtonClicked))]
        [UIElement("BuildToUseOnMatchStartButton")]
        private readonly Button m_selectBuildToUseOnMatchStartButton;

        [UIElementAction(nameof(OnStopSelectingBuildToUseOnMatchStartButtonClicked))]
        [UIElement("DontSelectBuildToUseOnMatchStartButton", false)]
        private readonly Button m_stopSelectingBuildToUseOnMatchStartButton;

        [UIElementAction(nameof(OnSelectNothingToUseOnMatchStartButtonClicked))]
        [UIElement("SelectNothingToUseOnMatchStartButton", false)]
        private readonly Button m_selectNothingToUseOnMatchStartButton;

        [UIElementAction(nameof(OnSearchBoxChanged))]
        [UIElement("SearchBox")]
        private readonly InputField m_searchBox;

        [KeyBindSetter(KeyCode.U)]
        [UIElementAction(nameof(OnKeyBindChanged))]
        [UIElement("KeyBind")]
        private readonly UIElementKeyBindSetter m_keyBind;

        [UIElement("Panel", true)]
        private readonly GameObject m_panel;

        [UIElement("NewBuildButtonPrefab", false)]
        private readonly Button m_newBuildButtonPrefab;

        [UIElement("BuildDisplayPrefab", false)]
        private readonly ModdedObject m_buildDisplayPrefab;

        [UIElement("Content")]
        private readonly Transform m_buildDisplayContainer;

        [UIElement("BuildToUseOnMatchStartButton")]
        private readonly Button m_buildToUseOnMatchStartButton;

        [UIElement("BuildToUseOnMatchStartText")]
        private readonly Text m_buildToUseOnMatchStartText;

        private int m_upgradeUISiblingIndex;

        private bool m_selectingBuildToUseOnMatchStart;

        private Dictionary<string, GameObject> m_searchEntries;

        private AutoBuildInfo m_editingBuild;

        private GameObject m_instantiatedNewButton;

        public override bool refreshOnlyCursor => true;

        public bool isShowingUpgradeUI
        {
            get;
            set;
        }

        public GameObject objectToShow
        {
            get;
            set;
        }

        protected override void OnInitialized()
        {
            m_searchEntries = new Dictionary<string, GameObject>();
            m_keyBind.key = AutoBuildManager.AutoBuildKeyBind;
        }

        public override void Show()
        {
            base.Show();

            PopulateBuilds();
            OnStopSelectingBuildToUseOnMatchStartButtonClicked();

            m_clearButton.interactable = false;
            m_searchBox.text = string.Empty;
        }

        public override void OnEnable()
        {
            SetUpgradeUISiblingIndex(false);
        }

        public override void OnDisable()
        {
            AutoBuildManager autoBuildManager = AutoBuildManager.Instance;
            autoBuildManager.isInAutoBuildConfigurationMode = false;
            autoBuildManager.ResetUpgrades();
            autoBuildManager.SaveBuildsInfo();

            GameObject gameObject = objectToShow;
            if (gameObject)
                gameObject.SetActive(true);

            objectToShow = null;
            SetUpgradeUISiblingIndex(true);

            ModSettingsDataManager.Instance.Save();
        }

        public void PopulateBuilds()
        {
            m_searchEntries.Clear();
            m_instantiatedNewButton = null;
            if (m_buildDisplayContainer.childCount != 0)
                TransformUtils.DestroyAllChildren(m_buildDisplayContainer);

            int i = -1;
            UpgradeManager upgradeManager = UpgradeManager.Instance;
            AutoBuildManager autoBuildManager = AutoBuildManager.Instance;
            var builds = autoBuildManager.buildList.Builds;
            foreach (AutoBuildInfo build in builds)
            {
                i++;
                int index = i;

                ModdedObject moddedObject = Instantiate(m_buildDisplayPrefab, m_buildDisplayContainer);
                moddedObject.gameObject.SetActive(true);

                string searchEntryKey = build.Name.ToLower();
                while (m_searchEntries.ContainsKey(searchEntryKey))
                    searchEntryKey += "_1";

                m_searchEntries.Add(searchEntryKey, moddedObject.gameObject);

                Text buildNameText = moddedObject.GetObject<Text>(0);
                buildNameText.text = AutoBuildManager.GetBuildDisplayName(build.Name);

                Image upgradeIconPrefab = moddedObject.GetObject<Image>(1);
                upgradeIconPrefab.gameObject.SetActive(true);
                Transform upgradeIconContainer = moddedObject.GetObject<Transform>(2);
                foreach (UpgradeTypeAndLevel upgrade in build.Upgrades)
                {
                    Sprite sprite = null;

                    UpgradeDescription upgradeDescription = upgradeManager.GetUpgrade(upgrade.UpgradeType, upgrade.Level);
                    if (upgradeDescription && upgradeDescription.Icon)
                    {
                        sprite = upgradeDescription.Icon;
                    }
                    else
                    {
                        sprite = ModResources.Sprite(AssetBundleConstants.UI, "NA-HQ-128x128");
                    }

                    Image upgradeIcon = Instantiate(upgradeIconPrefab, upgradeIconContainer);
                    upgradeIcon.sprite = sprite;
                }
                upgradeIconPrefab.gameObject.SetActive(false);

                Button editButton = moddedObject.GetObject<Button>(3);
                editButton.onClick.AddListener(delegate
                {
                    if (m_selectingBuildToUseOnMatchStart)
                    {
                        ModSettingsManager.SetIntValue(ModSettingsConstants.AUTO_BUILD_INDEX_TO_USE_ON_MATCH_START, builds.IndexOf(build));
                        RefreshBuildToUseOnStartButton();
                        OnStopSelectingBuildToUseOnMatchStartButtonClicked();
                        return;
                    }
                    ConfigureBuild(build);
                });

                Button deleteButton = moddedObject.GetObject<Button>(4);
                deleteButton.onClick.AddListener(delegate
                {
                    ModUIUtils.MessagePopup(true, $"{LocalizationManager.Instance.GetTranslatedString("auto_build_delete")} \"{AutoBuildManager.GetBuildDisplayName(build.Name)}\"?", LocalizationManager.Instance.GetTranslatedString("action_cannot_be_undone"), 125f, MessageMenu.ButtonLayout.EnableDisableButtons, "Ok", "Yes", "No", null, delegate
                    {
                        AutoBuildInfo autoBuildInfo;
                        int oldIndex = AutoBuildManager.AutoBuildIndexToUseOnMatchStart;
                        if (oldIndex >= 0 && builds.Count > oldIndex)
                        {
                            autoBuildInfo = builds[AutoBuildManager.AutoBuildIndexToUseOnMatchStart];
                        }
                        else
                        {
                            autoBuildInfo = null;
                        }

                        _ = autoBuildManager.buildList.Builds.Remove(build);
                        if(autoBuildInfo != null)
                        {
                            ModSettingsManager.SetIntValue(ModSettingsConstants.AUTO_BUILD_INDEX_TO_USE_ON_MATCH_START, builds.IndexOf(autoBuildInfo));
                        }

                        Destroy(moddedObject.gameObject);
                        RefreshBuildToUseOnStartButton();
                        RefreshNewBuildButton();
                    });
                });

                Button renameButton = moddedObject.GetObject<Button>(5);
                renameButton.onClick.AddListener(delegate
                {
                    ModUIUtils.InputFieldWindow($"{LocalizationManager.Instance.GetTranslatedString("auto_build_rename")} \"{build.Name}\"?", string.Empty, build.Name, 20, 125f, delegate (string name)
                    {
                        build.Name = name;
                        buildNameText.text = AutoBuildManager.GetBuildDisplayName(build.Name);
                        RefreshBuildToUseOnStartButton();
                    });
                });
            }

            RefreshBuildToUseOnStartButton();
        }

        public void RefreshNewBuildButton()
        {
            if (!m_instantiatedNewButton)
            {
                Button newBuildButton = Instantiate(m_newBuildButtonPrefab, m_buildDisplayContainer);
                newBuildButton.onClick.AddListener(OnNewButtonClicked);
                m_instantiatedNewButton = newBuildButton.gameObject;
            }
            m_instantiatedNewButton.gameObject.SetActive(m_searchBox.text.IsNullOrEmpty() && AutoBuildManager.Instance.buildList.Builds.Count < 10);
        }

        public void RefreshBuildToUseOnStartButton()
        {
            int index = AutoBuildManager.AutoBuildIndexToUseOnMatchStart;

            AutoBuildManager autoBuildManager = AutoBuildManager.Instance;
            List<AutoBuildInfo> builds = autoBuildManager.buildList.Builds;
            if (builds.IsNullOrEmpty() || index < 0 || index >= builds.Count)
            {
                m_buildToUseOnMatchStartText.text = "-";
                return;
            }
            m_buildToUseOnMatchStartText.text = AutoBuildManager.GetBuildDisplayName(builds[index].Name);
        }

        public void SetUpgradeUISiblingIndex(bool initial)
        {
            if (initial)
            {
                ModCache.gameUIRoot.UpgradeUI.transform.SetSiblingIndex(m_upgradeUISiblingIndex);
            }
            else
            {
                Transform transform = ModCache.gameUIRoot.UpgradeUI.transform;
                m_upgradeUISiblingIndex = transform.GetSiblingIndex();
                transform.SetSiblingIndex(ModUIManager.Instance.GetSiblingIndex(ModUIManager.UILayer.AfterTitleScreen) + 3);
            }
        }

        public void ConfigureBuild(AutoBuildInfo autoBuildInfo)
        {
            m_editingBuild = autoBuildInfo;

            AutoBuildManager autoBuildManager = AutoBuildManager.Instance;
            autoBuildManager.isInAutoBuildConfigurationMode = true;
            autoBuildManager.ResetUpgrades(autoBuildInfo.GetUpgradesFromData(), autoBuildInfo.SkillPoints);

            UpgradePagesManager._currentPageIndex = 0;
            UpgradeUI upgradeUI = ModCache.gameUIRoot.UpgradeUI;
            upgradeUI.Show(false, false, false);
            upgradeUI.ExitButton.SetActive(false);

            isShowingUpgradeUI = true;

            m_panel.SetActive(false);
            m_closeUpgradeUIButton.gameObject.SetActive(true);
            m_resetUpgradesButton.gameObject.SetActive(true);
        }

        public void OnCloseUpgradeUIButtonClicked()
        {
            AutoBuildManager autoBuildManager = AutoBuildManager.Instance;
            autoBuildManager.isInAutoBuildConfigurationMode = false;

            m_editingBuild.SetUpgradesFromData(GameDataManager.Instance.GetAvailableSkillPoints());
            m_editingBuild = null;

            autoBuildManager.SaveBuildsInfo();

            UpgradeUI upgradeUI = ModCache.gameUIRoot.UpgradeUI;
            upgradeUI.Hide();

            isShowingUpgradeUI = false;

            m_panel.SetActive(true);
            m_closeUpgradeUIButton.gameObject.SetActive(false);
            m_resetUpgradesButton.gameObject.SetActive(false);
            PopulateBuilds();
            RefreshNewBuildButton();
        }

        public void OnResetUpgradesButtonClicked()
        {
            AutoBuildManager autoBuildManager = AutoBuildManager.Instance;
            autoBuildManager.ResetUpgrades();
        }

        public void OnNewButtonClicked()
        {
            ModUIUtils.InputFieldWindow(LocalizationManager.Instance.GetTranslatedString("auto_build_create"), LocalizationManager.Instance.GetTranslatedString("auto_build_create_desc"), m_searchBox.text.IsNullOrEmpty() ? "Unnamed build" : m_searchBox.text, 20, 125f, delegate (string name)
            {
                AutoBuildInfo autoBuildInfo = new AutoBuildInfo()
                {
                    Name = name,
                    SkillPoints = 4
                };
                autoBuildInfo.FixValues();

                AutoBuildManager autoBuildManager = AutoBuildManager.Instance;
                autoBuildManager.buildList.Builds.Add(autoBuildInfo);
                ConfigureBuild(autoBuildInfo);
            });
        }

        public void OnClearButtonClicked()
        {
            m_searchBox.text = string.Empty;
        }

        public void OnSearchBoxChanged(string value)
        {
            string lowerText = value.ToLower();
            bool forceSetEnabled = value.IsNullOrEmpty();

            m_clearButton.interactable = !forceSetEnabled;

            foreach (KeyValuePair<string, GameObject> keyValue in m_searchEntries)
            {
                if (forceSetEnabled)
                {
                    keyValue.Value.SetActive(true);
                }
                else
                {
                    keyValue.Value.SetActive(keyValue.Key.Contains(lowerText));
                }
            }
        }

        public void OnAutoActivationToggled(bool value)
        {
            ModSettingsManager.SetBoolValue(ModSettingsConstants.AUTO_BUILD_ACTIVATION_ON_MATCH_START, value, true);
        }

        public void OnKeyBindChanged(KeyCode keyCode)
        {
            ModSettingsManager.SetIntValue(ModSettingsConstants.AUTO_BUILD_KEY_BIND, (int)keyCode, true);
        }

        public void OnSelectBuildToUseOnMatchStartButtonClicked()
        {
            m_selectingBuildToUseOnMatchStart = true;
            m_stopSelectingBuildToUseOnMatchStartButton.gameObject.SetActive(true);
            m_selectNothingToUseOnMatchStartButton.gameObject.SetActive(true);

            if (m_instantiatedNewButton)
                m_instantiatedNewButton.SetActive(false);
        }

        public void OnStopSelectingBuildToUseOnMatchStartButtonClicked()
        {
            m_selectingBuildToUseOnMatchStart = false;
            m_stopSelectingBuildToUseOnMatchStartButton.gameObject.SetActive(false);
            m_selectNothingToUseOnMatchStartButton.gameObject.SetActive(false);

            RefreshNewBuildButton();
        }

        public void OnSelectNothingToUseOnMatchStartButtonClicked()
        {
            ModSettingsManager.SetIntValue(ModSettingsConstants.AUTO_BUILD_INDEX_TO_USE_ON_MATCH_START, -1, true);

            m_selectingBuildToUseOnMatchStart = false;
            m_stopSelectingBuildToUseOnMatchStartButton.gameObject.SetActive(false);
            m_selectNothingToUseOnMatchStartButton.gameObject.SetActive(false);

            RefreshBuildToUseOnStartButton();
            RefreshNewBuildButton();
        }
    }
}
