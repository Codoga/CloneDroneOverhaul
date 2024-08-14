using OverhaulMod.Combat;
using OverhaulMod.Content.Personalization;
using OverhaulMod.Engine;
using OverhaulMod.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace OverhaulMod.UI
{
    public class UIPersonalizationItemBrowser : OverhaulUIBehaviour
    {
        public const string ITEM_DISPLAY_DEFAULT_TEXT_COLOR = "#FFFFFF";
        public const string ITEM_DISPLAY_DEFAULT_TEXT_OUTLINE_COLOR = "#000000";
        public const string ITEM_DISPLAY_DEFAULT_TEXT_GLOW_COLOR = "#FFFFFF";

        public const string ITEM_DISPLAY_EXCLUSIVE_TEXT_COLOR = "#FFD058";
        public const string ITEM_DISPLAY_EXCLUSIVE_TEXT_OUTLINE_COLOR = "#A46300";
        public const string ITEM_DISPLAY_EXCLUSIVE_TEXT_GLOW_COLOR = "#FFC44D";

        public const string ITEM_DISPLAY_NONVERIFIED_TEXT_COLOR = "#FFFFFF";
        public const string ITEM_DISPLAY_NONVERIFIED_TEXT_OUTLINE_COLOR = "#00285B";
        public const string ITEM_DISPLAY_NONVERIFIED_TEXT_GLOW_COLOR = "#3E96FF";

        public const string ITEM_DISPLAY_NONVERIFIED_EXCLUSIVE_TEXT_COLOR = "#FFFFFF";
        public const string ITEM_DISPLAY_NONVERIFIED_EXCLUSIVE_TEXT_OUTLINE_COLOR = "#006A0D";
        public const string ITEM_DISPLAY_NONVERIFIED_EXCLUSIVE_TEXT_GLOW_COLOR = "#00F81F";

        [ModSetting(ModSettingsConstants.HAS_EVER_ROTATED_THE_CAMERA, false)]
        public static bool HasEverRotatedTheCamera;

        public static bool IsPreviewing;

        public static bool HasShownAssetsUpdateMenu;

        [UIElementAction(nameof(Hide))]
        [UIElement("CloseButton")]
        private readonly Button m_exitButton;

        [UIElement("Panel")]
        private readonly RectTransform m_panel;

        [UIElement("Panel", typeof(UIElementMouseEventsComponent))]
        private readonly UIElementMouseEventsComponent m_panelMouseEvents;

        [TabManager(typeof(UIElementTab), null, null, null, nameof(OnCategoryTabSelected))]
        private readonly TabManager m_categoryTabs;
        [UIElement("WeaponSkinsTab")]
        private readonly ModdedObject m_weaponSkinsTab;
        [UIElement("AccessoriesTab")]
        private readonly ModdedObject m_accessoriesTab;
        [UIElement("PetsTab")]
        private readonly ModdedObject m_petsTab;

        [TabManager(typeof(UIElementTabWithText), nameof(m_subCategoryTabPrefab), nameof(m_subCategoryTabsContainer), nameof(OnSubcategoryTabCreated), nameof(OnSubcategoryTabSelected))]
        private readonly TabManager m_subcategoryTabs;
        [UIElement("SubcategoryTabPrefab", false)]
        private readonly ModdedObject m_subCategoryTabPrefab;
        [UIElement("SubcategoryTabs")]
        private readonly Transform m_subCategoryTabsContainer;

        [UIElement("ItemDisplay", false)]
        private readonly ModdedObject m_itemDisplay;
        [UIElement("TextDisplay", false)]
        private readonly ModdedObject m_textDisplay;
        [UIElement("MessageDisplay", false)]
        private readonly ModdedObject m_messageDisplay;
        [UIElement("UtilsPanel", false)]
        private readonly ModdedObject m_utilsPanel;
        [UIElement("BottomPanel", false)]
        private readonly ModdedObject m_bottomPanel;
        [UIElement("Content")]
        private readonly Transform m_container;
        [UIElement("Content")]
        private readonly CanvasGroup m_containerCanvasGroup;

        [UIElement("NotImplementedText", false)]
        private readonly GameObject m_notImplementedTextObject;

        [UIElementAction(nameof(OnSettingsButtonClicked))]
        [UIElement("SettingsButton")]
        private readonly Button m_settingsButton;

        [UIElementAction(nameof(OnAllowEnemiesUseWeaponSkinsToggled))]
        [UIElement("EnemiesUseSkinsToggle")]
        private readonly Toggle m_allowEnemiesUseWeaponSkinsToggle;

        [UIElementAction(nameof(OnClearButtonClicked))]
        [UIElement("ClearButton")]
        private readonly Button m_clearButton;

        [UIElement("ScrollRect")]
        private readonly RectTransform m_scrollRectTransform;

        [UIElement("DescriptionBox", typeof(UIElementPersonalizationItemDescriptionBox), false)]
        private readonly UIElementPersonalizationItemDescriptionBox m_descriptionBox;

        [UIElementAction(nameof(OnSearchBoxChanged))]
        [UIElement("SearchBox")]
        private readonly InputField m_searchBox;

        [UIElementAction(nameof(OnSortDropdownChanged))]
        [UIElement("SortDropdown")]
        private readonly Dropdown m_sortDropdown;

        [UIElement("LoadingSawblade")]
        private readonly Image m_loadingSawblade;

        [UIElement("CameraRotationTutorial")]
        private readonly GameObject m_cameraRotationTutorial;

        private Dictionary<string, GameObject> m_cachedDisplays;

        private RectTransform m_rectTransform;

        private bool m_isOpen, m_isPopulating, m_showContents;

        private string m_prevTab;

        private PersonalizationCategory m_selectedCategory;

        private int m_sortType;

        private string m_selectedSubcategory;

        private readonly UnityWebRequest m_webRequest;

        private Button m_defaultSkinButton;

        private bool m_allowUICallbacks;

        private Transform m_cameraHolderTransform;

        private float m_cameraHolderRotationY;

        public override bool enableCursor => true;

        protected override void OnInitialized()
        {
            m_cachedDisplays = new Dictionary<string, GameObject>();
            m_rectTransform = base.GetComponent<RectTransform>();

            m_categoryTabs.AddTab(m_weaponSkinsTab.gameObject, "weapon skins");
            m_categoryTabs.AddTab(m_accessoriesTab.gameObject, "accessories");
            m_categoryTabs.AddTab(m_petsTab.gameObject, "pets");
            m_categoryTabs.SelectTab("weapon skins");
            m_prevTab = "weapon skins";

            m_descriptionBox.SetBrowserUI(this);
            m_sortDropdown.value = 1;
            m_allowEnemiesUseWeaponSkinsToggle.isOn = PersonalizationController.AllowEnemiesUseSkins;

            GlobalEventManager.Instance.AddEventListener(PersonalizationManager.CUSTOMIZATION_ASSETS_FILE_DOWNLOADED_EVENT, onCustomizationAssetsFileDownloaded);
            GlobalEventManager.Instance.AddEventListener(GlobalEvents.PlayerDied, tryHide);
            m_allowUICallbacks = true;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            IsPreviewing = false;
            GlobalEventManager.Instance.RemoveEventListener(PersonalizationManager.CUSTOMIZATION_ASSETS_FILE_DOWNLOADED_EVENT, onCustomizationAssetsFileDownloaded);
            GlobalEventManager.Instance.RemoveEventListener(GlobalEvents.PlayerDied, tryHide);
        }

        public override void Show()
        {
            base.Show();
            IsPreviewing = true;
            m_isOpen = true;
            m_showContents = true;
            m_categoryTabs.interactable = true;

            if (m_categoryTabs.selectedTab && m_prevTab != m_categoryTabs.selectedTab.tabId)
            {
                if (m_container.childCount != 0)
                    TransformUtils.DestroyAllChildren(m_container);

                m_categoryTabs.SelectTab(m_prevTab);
            }

            _ = base.StartCoroutine(waitThenRefreshCameraCoroutine());
            ShowDownloadCustomizationAssetsDownloadMenuIfRequired();

            UIVersionLabel.instance.offsetX = 325f;

            m_cachedDisplays.Clear();
            if (m_container.childCount != 0)
                TransformUtils.DestroyAllChildren(m_container);

            m_loadingSawblade.color = Color.white;

            if (m_selectedCategory == PersonalizationCategory.WeaponSkins)
                selectSubcategoryOfCurrentWeapon();

            Populate();
        }

        public override void Hide()
        {
            base.Hide();
            m_isOpen = false;
            setCameraZoomedIn(false);
            UIVersionLabel.instance.offsetX = 0f;

            PersonalizationManager.Instance.userInfo.SaveIfDirty();
            ModSettingsDataManager.Instance.Save();

            PersonalizationMultiplayerManager.Instance.SendPlayerCustomizationDataEvent(false);
        }

        public override void Update()
        {
            base.Update();

            float d = Time.unscaledDeltaTime * 12.5f * (m_showContents ? 1f : -1f);

            float a = m_containerCanvasGroup.alpha;
            a = Mathf.Clamp(a + d, 0f, 1f);
            m_containerCanvasGroup.alpha = a;

            Color color = m_loadingSawblade.color;
            color.a = 1f - a;
            m_loadingSawblade.color = color;

            Transform holder = m_cameraHolderTransform;
            if (holder)
            {
                bool mouseButtonDown = Input.GetMouseButton(1);
                if (mouseButtonDown && !HasEverRotatedTheCamera)
                {
                    ModSettingsManager.SetBoolValue(ModSettingsConstants.HAS_EVER_ROTATED_THE_CAMERA, true);
                }

                float d2 = Time.deltaTime * 10f;
                m_cameraHolderRotationY = Mathf.Lerp(m_cameraHolderRotationY, mouseButtonDown ? Input.GetAxis("Mouse X") * 1.25f : 0f, d2);

                Vector3 currentEulerAngles = holder.localEulerAngles;
                currentEulerAngles.y += m_cameraHolderRotationY;
                holder.localEulerAngles = currentEulerAngles;
            }

            m_cameraRotationTutorial.SetActive(!HasEverRotatedTheCamera);
        }

        private void LateUpdate()
        {
            refreshCameraRect();
        }

        public override void OnDisable()
        {
            base.OnDisable();

            IsPreviewing = false;
            m_isPopulating = false;
        }

        private void tryHide()
        {
            if (base.gameObject.activeInHierarchy)
            {
                Hide();
            }
        }

        public bool IsMouseOverPanel()
        {
            return m_panelMouseEvents.isMouseOverElement;
        }

        public void ShowDescriptionBox(PersonalizationItemInfo itemInfo, RectTransform rectTransform)
        {
            m_descriptionBox.ShowForItem(itemInfo, rectTransform);
        }

        public void MakeDefaultSkinButtonInteractable()
        {
            Button button = m_defaultSkinButton;
            if (button)
            {
                button.interactable = true;
            }
        }

        public void ShowDownloadCustomizationAssetsDownloadMenuIfRequired()
        {
            if (HasShownAssetsUpdateMenu)
                return;

            if (PersonalizationManager.Instance.GetPersonalizationAssetsState() != PersonalizationAssetsState.Installed)
            {
                _ = ModUIConstants.ShowDownloadPersonalizationAssetsMenu(base.transform);
                HasShownAssetsUpdateMenu = true;
            }
        }

        public void OnCategoryTabSelected(UIElementTab elementTab)
        {
            UIElementTab oldTab = m_categoryTabs.prevSelectedTab;
            UIElementTab newTab = m_categoryTabs.selectedTab;
            if (oldTab)
            {
                m_prevTab = oldTab.tabId;

                RectTransform rt = oldTab.transform as RectTransform;
                Vector2 vector = rt.sizeDelta;
                vector.y = 25f;
                rt.sizeDelta = vector;
            }
            if (newTab)
            {
                RectTransform rt = newTab.transform as RectTransform;
                Vector2 vector = rt.sizeDelta;
                vector.y = 30f;
                rt.sizeDelta = vector;
            }

            m_subcategoryTabs.Clear();
            if (newTab.tabId == "weapon skins")
            {
                m_selectedCategory = PersonalizationCategory.WeaponSkins;
                m_subcategoryTabs.AddTab("Sword");
                m_subcategoryTabs.AddTab("Bow");
                m_subcategoryTabs.AddTab("Hammer");
                m_subcategoryTabs.AddTab("Spear");

                if (ModFeatures.IsEnabled(ModFeatures.FeatureType.ShieldSkins))
                    m_subcategoryTabs.AddTab("Shield");

                if (ModFeatures.IsEnabled(ModFeatures.FeatureType.ScytheSkins) && BoltNetwork.IsServer)
                    m_subcategoryTabs.AddTab(ModWeaponsManager.SCYTHE_TYPE.ToString());

                selectSubcategoryOfCurrentWeapon();
            }
            else if (newTab.tabId == "accessories")
            {
                m_selectedCategory = PersonalizationCategory.Accessories;
            }
            else if (newTab.tabId == "pets")
            {
                m_selectedCategory = PersonalizationCategory.Pets;
            }
            else
            {
                m_selectedCategory = PersonalizationCategory.None;
            }

            Populate();
        }

        public void OnSubcategoryTabSelected(UIElementTab elementTab)
        {
            m_selectedSubcategory = elementTab.tabId;
            Populate();
        }

        public void OnSubcategoryTabCreated(UIElementTab elementTab)
        {
            UIElementTabWithText elementTabWithText = elementTab as UIElementTabWithText;
            elementTabWithText.LocalizationID = $"customization_subtab_{elementTab.tabId.ToLower()}";
        }

        private void selectSubcategoryOfCurrentWeapon()
        {
            FirstPersonMover firstPersonMover = CharacterTracker.Instance.GetPlayerRobot();
            if (firstPersonMover)
            {
                WeaponType weaponType = firstPersonMover.GetEquippedWeaponType();
                string weaponTypeString = weaponType.ToString();

                if (m_subcategoryTabs.HasTab(weaponTypeString))
                    m_subcategoryTabs.SelectTab(weaponTypeString);
                else
                    m_subcategoryTabs.SelectTab("Sword");
            }
            else
            {
                m_subcategoryTabs.SelectTab("Sword");
            }
        }

        public void Populate()
        {
            if (m_isPopulating || !base.enabled || !base.gameObject.activeInHierarchy)
                return;

            m_isPopulating = true;
            _ = base.StartCoroutine(populateCoroutine());
        }

        private IEnumerator populateCoroutine()
        {
            yield return null;

            m_showContents = false;
            m_categoryTabs.interactable = false;
            m_subcategoryTabs.interactable = false;
            m_sortDropdown.interactable = false;

            RectTransform scrollRectTransform = m_scrollRectTransform;
            Vector2 sizeDelta = scrollRectTransform.sizeDelta;
            if (m_selectedCategory != PersonalizationCategory.WeaponSkins)
                sizeDelta.y = -190f;
            else
                sizeDelta.y = -225f;
            scrollRectTransform.sizeDelta = sizeDelta;

            float timeToWait = Time.unscaledTime + 0.25f;
            while (timeToWait > Time.unscaledTime)
                yield return null;

            m_cachedDisplays.Clear();
            if (m_container.childCount != 0)
                TransformUtils.DestroyAllChildren(m_container);

            if (m_selectedCategory != PersonalizationCategory.WeaponSkins && !ModFeatures.IsEnabled(ModFeatures.FeatureType.AccessoriesAndPets))
            {
                m_notImplementedTextObject.SetActive(true);
            }
            else
            {
                m_notImplementedTextObject.SetActive(false);

                if (!Enum.TryParse(m_selectedSubcategory, out WeaponType weaponType))
                    weaponType = WeaponType.Sword;
                else

                    ModGameUtils.WaitForPlayerInputUpdate(delegate (IFPMoveCommandInput input)
                    {
                        switch (weaponType)
                        {
                            case WeaponType.Sword:
                                input.Weapon1 = true;
                                break;
                            case WeaponType.Bow:
                                input.Weapon2 = true;
                                break;
                            case WeaponType.Hammer:
                                input.Weapon3 = true;
                                break;
                            case WeaponType.Spear:
                                input.Weapon4 = true;
                                break;
                            case WeaponType.Shield:
                                input.Weapon4 = true;
                                break;
                            case ModWeaponsManager.SCYTHE_TYPE:
                                FirstPersonMover firstPersonMover = CharacterTracker.Instance.GetPlayerRobot();
                                if (firstPersonMover)
                                {
                                    firstPersonMover.SetEquippedWeaponType(ModWeaponsManager.SCYTHE_TYPE);
                                }
                                break;
                        }
                    });

                if (weaponType == WeaponType.Bow && ModSpecialUtils.IsModEnabled("ee32ba1b-8c92-4f50-bdf4-400a14da829e"))
                {
                    ModdedObject messageDisplay = Instantiate(m_messageDisplay, m_container);
                    messageDisplay.gameObject.SetActive(true);
                    messageDisplay.GetObject<Text>(0).text = LocalizationManager.Instance.GetTranslatedString("bow_skins_not_supported_glock18");
                }
                else
                {
                    ModdedObject utilsPanel = Instantiate(m_utilsPanel, m_container);
                    utilsPanel.gameObject.SetActive(true);
                    Button defaultSkinButton = utilsPanel.GetObject<Button>(0);
                    defaultSkinButton.onClick.AddListener(delegate
                    {
                        defaultSkinButton.interactable = false;
                        PersonalizationController.SetWeaponSkin(weaponType, null);
                        PersonalizationController.DestroyWeaponSkin(weaponType);
                        GlobalEventManager.Instance.Dispatch(PersonalizationManager.ITEM_EQUIPPED_OR_UNEQUIPPED_EVENT);
                    });
                    defaultSkinButton.interactable = !PersonalizationController.GetWeaponSkin(weaponType).IsNullOrEmpty();
                    m_defaultSkinButton = defaultSkinButton;

                    utilsPanel.GetObject<Button>(1).onClick.AddListener(OnUpdateButtonClicked);

                    PersonalizationManager personalizationManager = PersonalizationManager.Instance;
                    if (personalizationManager.GetPersonalizationAssetsState() == PersonalizationAssetsState.NotInstalled)
                    {
                        utilsPanel.GetObject<Text>(2).text = LocalizationManager.Instance.GetTranslatedString("Download");
                    }
                    else
                    {
                        utilsPanel.GetObject<Text>(2).text = LocalizationManager.Instance.GetTranslatedString("customization_button_update");
                    }

                    bool isDeveloper = ModUserInfo.isDeveloper;

                    System.Collections.Generic.List<PersonalizationItemInfo> list = PersonalizationManager.Instance.itemList.GetItems(m_selectedCategory, (PersonalizationItemsSortType)m_sortType);
                    for (int i = 0; i < list.Count; i++)
                    {
                        PersonalizationItemInfo item = list[i];
                        if (item.Weapon != weaponType || (item.HideInBrowser && !isDeveloper))
                            continue;

                        bool isExclusive = item.IsExclusive();
                        bool isVerified = item.IsVerified;
                        bool noSpecificAuthor = false;

                        string authorsString = item.GetAuthorsString(true);
                        string prefix;
                        if (authorsString == "vanilla")
                        {
                            noSpecificAuthor = true;
                            prefix = LocalizationManager.Instance.GetTranslatedString("customization_vanilla");
                        }
                        else
                            prefix = $"{((item.Authors.IsNullOrEmpty() || item.Authors.Count <= 1) ? LocalizationManager.Instance.GetTranslatedString("customization_author") : LocalizationManager.Instance.GetTranslatedString("customization_authors"))} ";

                        string authorsStringToDisplay;
                        if (noSpecificAuthor)
                        {
                            authorsStringToDisplay = prefix;
                        }
                        else
                        {
                            authorsStringToDisplay = $"{prefix}{authorsString.AddColor(Color.white)}";
                        }

                        Color textColor;
                        Color textOutlineColor;
                        Color glowColor;

                        if (isExclusive && !isVerified)
                        {
                            textColor = ModParseUtils.TryParseToColor(ITEM_DISPLAY_NONVERIFIED_EXCLUSIVE_TEXT_COLOR, Color.white);
                            textOutlineColor = ModParseUtils.TryParseToColor(ITEM_DISPLAY_NONVERIFIED_EXCLUSIVE_TEXT_OUTLINE_COLOR, Color.black);
                            glowColor = ModParseUtils.TryParseToColor(ITEM_DISPLAY_NONVERIFIED_EXCLUSIVE_TEXT_GLOW_COLOR, Color.white);
                        }
                        else if (isExclusive)
                        {
                            textColor = ModParseUtils.TryParseToColor(ITEM_DISPLAY_EXCLUSIVE_TEXT_COLOR, Color.white);
                            textOutlineColor = ModParseUtils.TryParseToColor(ITEM_DISPLAY_EXCLUSIVE_TEXT_OUTLINE_COLOR, Color.black);
                            glowColor = ModParseUtils.TryParseToColor(ITEM_DISPLAY_EXCLUSIVE_TEXT_GLOW_COLOR, Color.white);
                        }
                        else if (!isVerified)
                        {
                            textColor = ModParseUtils.TryParseToColor(ITEM_DISPLAY_NONVERIFIED_TEXT_COLOR, Color.white);
                            textOutlineColor = ModParseUtils.TryParseToColor(ITEM_DISPLAY_NONVERIFIED_TEXT_OUTLINE_COLOR, Color.black);
                            glowColor = ModParseUtils.TryParseToColor(ITEM_DISPLAY_NONVERIFIED_TEXT_GLOW_COLOR, Color.white);
                        }
                        else
                        {
                            textColor = ModParseUtils.TryParseToColor(ITEM_DISPLAY_DEFAULT_TEXT_COLOR, Color.white);
                            textOutlineColor = ModParseUtils.TryParseToColor(ITEM_DISPLAY_DEFAULT_TEXT_OUTLINE_COLOR, Color.black);
                            glowColor = ModParseUtils.TryParseToColor(ITEM_DISPLAY_DEFAULT_TEXT_GLOW_COLOR, Color.white);
                        }
                        glowColor.a = !isExclusive && isVerified ? 0.25f : 0.4f;

                        ModdedObject moddedObject = Instantiate(m_itemDisplay, m_container);
                        moddedObject.gameObject.SetActive(true);

                        Text itemNameText = moddedObject.GetObject<Text>(0);
                        itemNameText.text = item.Name.ToUpper();
                        itemNameText.color = textColor;
                        Outline itemNameTextOutline = itemNameText.GetComponent<Outline>();
                        itemNameTextOutline.effectColor = textOutlineColor;

                        Image glowImage = moddedObject.GetObject<Image>(2);
                        glowImage.color = glowColor;

                        moddedObject.GetObject<Text>(1).text = authorsStringToDisplay;

                        UIElementPersonalizationItemDisplay personalizationItemDisplay = moddedObject.gameObject.AddComponent<UIElementPersonalizationItemDisplay>();
                        personalizationItemDisplay.ItemInfo = item;
                        personalizationItemDisplay.SetBrowserUI(this);
                        personalizationItemDisplay.InitializeElement();

                        m_cachedDisplays.Add(item.Name.ToLower(), moddedObject.gameObject);
                    }

                    ModdedObject bottomPanel = Instantiate(m_bottomPanel, m_container);
                    bottomPanel.gameObject.SetActive(true);
                    Button editorButton = bottomPanel.GetObject<Button>(0);
                    editorButton.onClick.AddListener(delegate
                    {
                        ModUIUtils.MessagePopup(true, LocalizationManager.Instance.GetTranslatedString("enter_ceditor_dialog_header"), LocalizationManager.Instance.GetTranslatedString("enter_ceditor_dialog_text"), 150f, MessageMenu.ButtonLayout.EnableDisableButtons, "ok", "Yes", "No", null, delegate
                        {
                            ModCore.EnterCustomizationEditor = true;
                            SceneTransitionManager.Instance.DisconnectAndExitToMainMenu();
                        });
                    });

                    ModdedObject messageDisplay1 = Instantiate(m_messageDisplay, m_container);
                    messageDisplay1.gameObject.SetActive(true);
                    messageDisplay1.GetObject<Text>(0).text = LocalizationManager.Instance.GetTranslatedString("authors_reminder");
                }
            }

            OnSearchBoxChanged(m_searchBox.text);

            m_prevTab = m_categoryTabs.selectedTab?.tabId;
            m_categoryTabs.interactable = true;
            m_subcategoryTabs.interactable = true;
            m_sortDropdown.interactable = true;
            m_showContents = true;
            m_isPopulating = false;
            yield break;
        }

        private IEnumerator waitThenRefreshCameraCoroutine()
        {
            FirstPersonMover firstPersonMover = CharacterTracker.Instance.GetPlayerRobot();
            if (firstPersonMover)
            {
                while (firstPersonMover.IsSwingingMeleeWeapon())
                    yield return null;
            }
            setCameraZoomedIn(true);
            yield break;
        }

        private void onCustomizationAssetsFileDownloaded()
        {
            Populate();
        }

        private void refreshCameraRect()
        {
            CameraManager cameraManager = CameraManager.Instance;
            if (m_isOpen)
            {
                float proportionOfWidthTakenUpBySidebar = m_panel.rect.width / m_rectTransform.rect.width;
                cameraManager.SetCameraRect(new Rect(proportionOfWidthTakenUpBySidebar, 0, 1f - proportionOfWidthTakenUpBySidebar, 1f));
            }
            else
            {
                cameraManager.ResetCameraRect();
            }
        }

        private void setCameraZoomedIn(bool value)
        {
            FirstPersonMover firstPersonMover = CharacterTracker.Instance.GetPlayerRobot();
            CameraManager cameraManager = CameraManager.Instance;
            if (!value)
            {
                refreshCameraRect();
                cameraManager.ResetCameraHolderPosition(firstPersonMover);
                cameraManager.ResetCameraHolderEulerAngles(firstPersonMover);
                cameraManager.enableForceFOVOffset = false;
                cameraManager.enableThirdPerson = false;
                m_cameraHolderTransform = null;
                return;
            }

            refreshCameraRect();
            cameraManager.SetCameraHolderPosition(new Vector3(0f, 0f, 0.75f), firstPersonMover);
            cameraManager.SetCameraHolderEulerAngles(Vector3.up * 220f, firstPersonMover);
            cameraManager.enableForceFOVOffset = true;
            cameraManager.enableThirdPerson = true;
            cameraManager.forceFOVOffset = -5f;

            FirstPersonMover robot = CharacterTracker.Instance.GetPlayerRobot();
            if (robot)
                m_cameraHolderTransform = robot._cameraHolderTransform;

            ModGameUtils.WaitForPlayerInputUpdate(delegate (IFPMoveCommandInput commandInput)
            {
                commandInput.IsResetLookKeyDown = true;
            });
        }

        public void OnSettingsButtonClicked()
        {
            _ = ModUIConstants.ShowPersonalizationSettingsMenu(base.transform);
        }

        public void OnUpdateButtonClicked()
        {
            _ = ModUIConstants.ShowDownloadPersonalizationAssetsMenu(base.transform);
        }

        public void OnSearchBoxChanged(string text)
        {
            m_clearButton.interactable = !text.IsNullOrEmpty();

            _ = text.ToLower();
            bool forceEnableAll = text.IsNullOrEmpty();
            foreach (KeyValuePair<string, GameObject> keyValue in m_cachedDisplays)
            {
                if (forceEnableAll)
                    keyValue.Value.SetActive(true);
                else
                    keyValue.Value.SetActive(keyValue.Key.Contains(text));
            }
        }

        public void OnSortDropdownChanged(int value)
        {
            if (!m_allowUICallbacks)
                return;

            m_sortType = value;
            Populate();
        }

        public void OnClearButtonClicked()
        {
            m_searchBox.text = string.Empty;
        }

        public void OnAllowEnemiesUseWeaponSkinsToggled(bool value)
        {
            if (!m_allowUICallbacks)
                return;

            ModSettingsManager.SetBoolValue(ModSettingsConstants.ALLOW_ENEMIES_USE_WEAPON_SKINS, value, true);
        }
    }
}
