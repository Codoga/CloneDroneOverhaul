using OverhaulMod.Content;
using OverhaulMod.Utils;
using Steamworks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OverhaulMod.UI
{
    public class UIWorkshopBrowser : OverhaulUIBehaviour
    {
        public const string ADVENTURE_LEVEL_TYPE_TAB = "Adventure";
        public const string CHALLENGE_LEVEL_TYPE_TAB = "Challenge";
        public const string ENDLESS_LEVEL_TYPE_TAB = "Endless Level";
        public const string LBS_LEVEL_TYPE_TAB = "Last Bot Standing Level";
        public const string COLLECTIONS_TYPE_TAB = "collections";

        public const string YOUR_LEVELS_SOURCE_TYPE = "user";
        public const string SUBSCRIPTIONS_SOURCE_TYPE = "subscriptions";
        public const string ALL_SOURCE_TYPE = "all";

        [UIElementAction(nameof(Hide))]
        [UIElement("CloseButton")]
        private readonly Button m_exitButton;

        [UIElementAction(nameof(OnLegacyUIButtonClicked))]
        [UIElement("OldUIButton")]
        private readonly Button m_legacyUIButton;

        [TabManager(typeof(UIElementTab), null, null, null, nameof(OnSourceTabSelected))]
        private readonly TabManager m_sourceTabs;

        [UIElement("YourLevelsTab")]
        public GameObject m_yourLevelsTab;
        [UIElement("SubscriptionsTab")]
        public GameObject m_subscriptionsTab;
        [UIElementAction(nameof(OnBrowseButtonClicked))]
        [UIElement("BrowseTab")]
        public Button m_browseTab;

        [UIElementAction(nameof(Populate))]
        [UIElement("ReloadButton")]
        public Button m_reloadButton;

        [TabManager(typeof(UIElementTab), null, null, null, nameof(OnQueryTabSelected))]
        private readonly TabManager m_queryTabs;

        [UIElement("TrendingTab")]
        public GameObject m_trendingTab;
        [UIElement("RecentTab")]
        public GameObject m_recentTab;
        [UIElement("MostPopularTab")]
        public GameObject m_mostPopularTab;
        [UIElement("MostSubscribersTab")]
        public GameObject m_mostSubscribersTab;
        [UIElement("ByFollowedUsersTab")]
        public GameObject m_byFollowedTab;
        [UIElement("ByFriendsTab")]
        public GameObject m_byFriendsTab;
        [UIElement("FriendsFavoritesTab")]
        public GameObject m_friendsFavoritesTab;

        [TabManager(typeof(UIElementTab), null, null, null, nameof(OnLevelTypeTabSelected))]
        private readonly TabManager m_levelTypeTabs;

        [UIElement("AdventuresTab")]
        public GameObject m_adventuresTab;
        [UIElement("ChallengesTab")]
        public GameObject m_challengesTab;
        [UIElement("EndlessLevelsTab")]
        public GameObject m_endlessLevelsTab;
        [UIElement("LBSLevelsTab")]
        public GameObject m_lastBotStandingLevelsTab;
        [UIElement("CollectionsTab")]
        public GameObject m_collectionsTab;

        [UIElement("WorkshopItemDisplay", false)]
        public ModdedObject m_workshopItemDisplay;
        [UIElement("WorkshopCollectionDisplay", false)]
        public ModdedObject m_workshopCollectionDisplay;

        [UIElement("ScrollRect")]
        public ScrollRect m_scrollRect;
        [UIElement("Content")]
        public Transform m_gridContainer;
        [UIElement("VerticalContent")]
        public Transform m_verticalContainer;
        [UIElement("Content")]
        public GridLayoutGroup m_containerGridLayoutGroup;

        [UIElement("LoadingIndicator", false)]
        public GameObject m_loadingIndicator;
        [UIElement("NothingToDisplayLabel", false)]
        public GameObject m_nothingToDisplayLabel;

        [UIElement("Tabs")]
        public CanvasGroup m_tabsCanvasGroup;

        [UIElement("BrowseItemsOfTypeDropdown", false)]
        public GameObject m_browseItemsOfTypeDropdownObject;

        [UIElementAction(nameof(OnBrowseLevelsButtonClicked))]
        [UIElement("BrowseLevelsButton")]
        public Button m_browseLevelsButton;
        [UIElement("BrowseLevelsSelectedIndicator", true)]
        public GameObject m_browseLevelsSelectedIndicatorObject;

        [UIElementAction(nameof(OnBrowseCollectionsButtonClicked))]
        [UIElement("BrowseCollectionsButton")]
        public Button m_browseCollectionsButton;
        [UIElement("BrowseCollectionsSelectedIndicator", false)]
        public GameObject m_browseCollectionsSelectedIndicatorObject;

        [UIElementAction(nameof(OnPrevPageButtonClicked))]
        [UIElement("PrevPageButton")]
        private readonly Button m_prevPageButton;
        [UIElementAction(nameof(OnNextPageButtonClicked))]
        [UIElement("NextPageButton")]
        private readonly Button m_nextPageButton;
        [UIElementAction(nameof(OnPageButtonClicked))]
        [UIElement("CurrentPageButton")]
        private readonly Button m_currentPageButton;
        [UIElement("CurrentPageText")]
        private readonly Text m_currentPageText;

        [UIElement("PageDropdown", false)]
        public GameObject m_pageDropdownObject;
        [UIElement("PageButton", false)]
        public ModdedObject m_pageButtonPrefab;
        [UIElement("PageContainer")]
        public Transform m_pageContainer;

        [UIElementAction(nameof(OnTypedSearchText))]
        [UIElement("SearchBox")]
        public InputField m_searchBox;
        [UIElementAction(nameof(OnSearchButtonClicked))]
        [UIElement("SearchButton")]
        public Button m_searchButton;
        [UIElementAction(nameof(OnClearButtonClicked))]
        [UIElement("ClearButton")]
        public Button m_clearButton;
        [UIElementAction(nameof(OnBrowseLevelsButtonClicked))]
        [UIElement("SearchLevelsButton")]
        public Button m_searchLevelsButton;

        [UIElement("SearchLevelsByTitleHolder", false)]
        public GameObject m_searchLevelsByTitleHolderObject;
        [UIElement("SearchLevelsByTitleText")]
        public Text m_searchLevelsByTitleText;

        [UIElement("SearchLevelsByUserHolder", false)]
        public GameObject m_searchLevelsByUserHolderObject;
        [UIElement("SearchLevelsByUserText")]
        public Text m_searchLevelsByUserText;

        [UIElementAction(nameof(OnHelpButtonClicked))]
        [UIElement("HelpButton")]
        public Button m_controlsButton;
        [UIElement("ControlsPanel", false)]
        public GameObject m_controlsPanel;

        [UIElement("ContextMenu", false)]
        private readonly RectTransform m_contextMenu;
        [UIElement("ContextMenu", typeof(UIElementMouseEventsComponent))]
        private readonly UIElementMouseEventsComponent m_contextMenuMouseChecker;

        [UIElementAction(nameof(OnContextMenuSubscribeButtonClicked))]
        [UIElement("ContextMenuSubscribeButton")]
        public Button m_contextMenuSubscribeButton;
        [UIElementAction(nameof(OnContextMenuPlayButtonClicked))]
        [UIElement("ContextMenuPlayButton")]
        public Button m_contextMenuPlayButton;

        [UIElement("QuickPreview", typeof(UIElementWorkshopItemQuickPreview), false)]
        private readonly UIElementWorkshopItemQuickPreview m_quickPreview;

        [UIElementAction(nameof(OnBackButtonClicked))]
        [UIElement("BackButton")]
        public Button m_backButton;

        [UIElement("MainBG")]
        public RectTransform m_mainBG;

        [UIElement("ContentCategoryContainer")]
        public GameObject m_tagsContainerObject;

        private List<UIElementWorkshopItemDisplay> m_selectedItemDisplays;

        private Transform m_container;

        public override bool hideTitleScreen => true;

        private bool m_initializedTabs, m_getWorkshopItemsNextFrame, m_isLoading;

        private float m_timeLeftToPopulate;

        public bool browseCollections
        {
            get;
            set;
        }

        public int sourceType
        {
            get;
            set;
        }

        public string searchLevelType
        {
            get;
            set;
        }

        public CSteamID searchLevelsByUser
        {
            get;
            set;
        }

        public EUserUGCList searchUserList
        {
            get;
            set;
        }

        public EUGCQuery searchQuery
        {
            get;
            set;
        }

        public int page
        {
            get;
            set;
        }

        public string searchText
        {
            get;
            set;
        }

        public PublishedFileId_t browseChildrenOfCollection
        {
            get;
            set;
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            page = 1;
            m_timeLeftToPopulate = -1f;
            m_selectedItemDisplays = new List<UIElementWorkshopItemDisplay>();

            m_controlsButton.gameObject.SetActive(ModFeatures.IsEnabled(ModFeatures.FeatureType.WorkshopBrowserContextMenu));
            browseChildrenOfCollection = default;
        }

        public override void Show()
        {
            base.Show();

            if (!SteamManager.Instance || !SteamManager.Instance.Initialized)
            {
                ModUIUtils.MessagePopupOK("Steam not initialized", "To browse Steam workshop you must have Steam connection established.", true);
                return;
            }

            if (!m_initializedTabs)
            {
                m_levelTypeTabs.AddTab(m_adventuresTab, ADVENTURE_LEVEL_TYPE_TAB);
                m_levelTypeTabs.AddTab(m_challengesTab, CHALLENGE_LEVEL_TYPE_TAB);
                m_levelTypeTabs.AddTab(m_endlessLevelsTab, ENDLESS_LEVEL_TYPE_TAB);
                m_levelTypeTabs.AddTab(m_lastBotStandingLevelsTab, LBS_LEVEL_TYPE_TAB);
                m_levelTypeTabs.SelectTab(ADVENTURE_LEVEL_TYPE_TAB);

                m_queryTabs.AddTab(m_trendingTab, EUGCQuery.k_EUGCQuery_RankedByTrend.ToString());
                m_queryTabs.AddTab(m_recentTab, EUGCQuery.k_EUGCQuery_RankedByPublicationDate.ToString());
                m_queryTabs.AddTab(m_mostPopularTab, EUGCQuery.k_EUGCQuery_RankedByVote.ToString());
                m_queryTabs.AddTab(m_mostSubscribersTab, EUGCQuery.k_EUGCQuery_RankedByTotalUniqueSubscriptions.ToString());
                m_queryTabs.AddTab(m_byFollowedTab, EUGCQuery.k_EUGCQuery_CreatedByFollowedUsersRankedByPublicationDate.ToString());
                m_queryTabs.AddTab(m_byFriendsTab, EUGCQuery.k_EUGCQuery_CreatedByFriendsRankedByPublicationDate.ToString());
                m_queryTabs.AddTab(m_friendsFavoritesTab, EUGCQuery.k_EUGCQuery_FavoritedByFriendsRankedByPublicationDate.ToString());
                m_queryTabs.SelectTab(EUGCQuery.k_EUGCQuery_RankedByTrend.ToString());

                m_sourceTabs.AddTab(m_yourLevelsTab, YOUR_LEVELS_SOURCE_TYPE);
                m_sourceTabs.AddTab(m_subscriptionsTab, SUBSCRIPTIONS_SOURCE_TYPE);
                m_sourceTabs.SelectTab(ALL_SOURCE_TYPE);
                m_initializedTabs = true;
            }

            Populate();
        }

        public override void Hide()
        {
            base.Hide();

            m_timeLeftToPopulate = -1f;
            if (m_gridContainer.childCount != 0)
                TransformUtils.DestroyAllChildren(m_gridContainer);
        }

        public override void Update()
        {
            if (m_getWorkshopItemsNextFrame)
                populate();

            if (m_timeLeftToPopulate >= 0f)
            {
                m_timeLeftToPopulate -= Time.unscaledDeltaTime;
                if (m_timeLeftToPopulate <= 0f)
                {
                    Populate();
                }
            }

            if (Input.GetMouseButtonDown(0) && m_contextMenu.gameObject.activeSelf && !m_contextMenuMouseChecker.isMouseOverElement)
            {
                ShowContextMenu(null);
            }
        }

        public void OnLevelTypeTabSelected(UIElementTab elementTab)
        {
            page = 1;

            UIElementTab oldTab = m_levelTypeTabs.prevSelectedTab;
            UIElementTab newTab = m_levelTypeTabs.selectedTab;
            if (oldTab)
            {
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

            searchLevelType = elementTab.tabId;
            Populate();
        }

        public void OnQueryTabSelected(UIElementTab elementTab)
        {
            page = 1;

            UIElementTab oldTab = m_levelTypeTabs.prevSelectedTab;
            UIElementTab newTab = m_levelTypeTabs.selectedTab;
            if (oldTab)
            {
                RectTransform rt = oldTab.transform as RectTransform;
                Vector2 vector = rt.sizeDelta;
                vector.x = 150f;
                rt.sizeDelta = vector;
            }
            if (newTab)
            {
                RectTransform rt = newTab.transform as RectTransform;
                Vector2 vector = rt.sizeDelta;
                vector.x = 160f;
                rt.sizeDelta = vector;
            }

            if (!Enum.TryParse(elementTab.tabId, out EUGCQuery result))
                result = EUGCQuery.k_EUGCQuery_RankedByTrend;

            searchQuery = result;
            Populate();
        }

        public void OnSourceTabSelected(UIElementTab elementTab)
        {
            browseChildrenOfCollection = default;
            page = 1;
            searchText = null;

            CSteamID steamId = CSteamID.Nil;
            string tabId = elementTab.tabId;
            if (tabId == YOUR_LEVELS_SOURCE_TYPE)
            {
                sourceType = 2;
                searchUserList = EUserUGCList.k_EUserUGCList_Published;
                steamId = SteamUser.GetSteamID();

                m_tabsCanvasGroup.alpha = 0.25f;
                m_tabsCanvasGroup.interactable = false;

                setBrowseItemType(false);
            }
            else if (tabId == SUBSCRIPTIONS_SOURCE_TYPE)
            {
                sourceType = 1;
                searchUserList = EUserUGCList.k_EUserUGCList_Subscribed;
                steamId = SteamUser.GetSteamID();

                m_tabsCanvasGroup.alpha = 0.25f;
                m_tabsCanvasGroup.interactable = false;

                setBrowseItemType(false);
            }
            else
            {
                sourceType = 0;

                m_tabsCanvasGroup.alpha = 1f;
                m_tabsCanvasGroup.interactable = true;
            }
            searchLevelsByUser = steamId;
            Populate();
        }

        public void Populate()
        {
            m_timeLeftToPopulate = -1f;
            m_getWorkshopItemsNextFrame = !m_isLoading;
        }

        private void populate()
        {
            m_getWorkshopItemsNextFrame = false;
            setIsLoading(true);
            refreshTabContainers();
            refreshSearchBox();

            if (m_gridContainer.childCount != 0)
                TransformUtils.DestroyAllChildren(m_gridContainer);

            if (m_verticalContainer.childCount != 0)
                TransformUtils.DestroyAllChildren(m_verticalContainer);

            m_selectedItemDisplays.Clear();

            if (searchLevelType != COLLECTIONS_TYPE_TAB)
            {
                bool collections = browseCollections && browseChildrenOfCollection == default;
                setGridLayout(collections);

                ModSteamUGCUtils.RequestParameters requestParameters = ModSteamUGCUtils.RequestParameters.Create(collections ? EUGCMatchingUGCType.k_EUGCMatchingUGCType_Collections : EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items);
                requestParameters.EnableCaching();
                requestParameters.RequireTags(collections ? null : new List<string>() { searchLevelType });
                requestParameters.ReturnPreviews();
                requestParameters.ReturnLongDescription();

                if (!searchText.IsNullOrEmpty())
                    requestParameters.SearchText(searchText);

                bool success;
                if (collections)
                {
                    success = ModSteamUGCUtils.GetWorkshopItems(new PublishedFileId_t[]
                    {
                        (PublishedFileId_t)3045196841, // Imagine Chapter 6 Competition Winners
                        //(PublishedFileId_t)2783921326, // Beginners Welcome Comp Entries - Clone Drone
                        (PublishedFileId_t)2669983696, // Zombie Adventure Competition Winners 2021
                        (PublishedFileId_t)2670197366, // LAUNCH Level Competition Winners
                        (PublishedFileId_t)2369933278, // Winter Level Editor Competition 2020
                    }, onGotItems, onError, null);
                }
                else
                {
                    if (browseChildrenOfCollection != default)
                    {
                        success = ModSteamUGCUtils.GetWorkshopItem(browseChildrenOfCollection, onGotItem, onError, null);
                    }
                    else
                    {
                        success = sourceType == 0 ? ModSteamUGCUtils.GetAllWorkshopItems(searchQuery, page, requestParameters, onGotItems, onError, null) : ModSteamUGCUtils.GetWorkshopUserItemList(searchLevelsByUser, page, searchUserList, EUserUGCListSortOrder.k_EUserUGCListSortOrder_SubscriptionDateDesc, requestParameters, onGotItems, onError, null);
                    }
                }

                if (!success)
                {
                    onError("Internal error.");
                    setIsLoading(false);
                }
            }
        }

        private void onGotItems(List<WorkshopItem> list)
        {
            bool isEmpty = list.IsNullOrEmpty();
            setIsLoading(false);
            m_nothingToDisplayLabel.SetActive(isEmpty);
            refreshContainer();

            if (isEmpty)
                return;

            bool collections = browseCollections && browseChildrenOfCollection == default;
            foreach (WorkshopItem workshopItem in list)
            {
                ModdedObject moddedObject = Instantiate(collections ? m_workshopCollectionDisplay : m_workshopItemDisplay, m_container);
                moddedObject.gameObject.SetActive(true);
                moddedObject.GetObject<Text>(0).text = workshopItem.Name;
                UIElementWorkshopItemDisplay workshopItemDisplay = moddedObject.gameObject.AddComponent<UIElementWorkshopItemDisplay>();
                workshopItemDisplay.itemPageWindowParentTransform = base.transform;
                workshopItemDisplay.browserUI = this;
                workshopItemDisplay.isCollection = collections;
                workshopItemDisplay.InitializeElement();
                workshopItemDisplay.Populate(workshopItem);
            }
        }

        private void onGotItem(WorkshopItem workshopItem)
        {
            if (!ModSteamUGCUtils.GetWorkshopItems(workshopItem.Children, onGotItems, onError, null))
            {
                onError("Internal error.");
                setIsLoading(false);
            }
        }

        private void onError(string error)
        {
            setIsLoading(false);

            ModUIUtils.MessagePopupOK("Could not get workshop items", error, "ok", Populate, 150f, true);
        }

        private void setGridLayout(bool collections)
        {
            GridLayoutGroup gridLayoutGroup = m_containerGridLayoutGroup;
            if (collections)
            {
                gridLayoutGroup.cellSize = new Vector2((m_gridContainer as RectTransform).rect.width - 24f, 100f);
            }
            else
            {
                gridLayoutGroup.cellSize = new Vector2(150f, 108f);
            }
        }

        private void setPageButtonsActive(bool value)
        {
            m_prevPageButton.interactable = value;
            m_nextPageButton.interactable = value;
            m_currentPageButton.interactable = value;
        }

        private void setIsLoading(bool value)
        {
            m_isLoading = value;

            bool searchByUser = sourceType != 0 && searchUserList == EUserUGCList.k_EUserUGCList_Published;
            m_searchLevelsByUserHolderObject.SetActive(searchByUser);
            m_searchLevelsByUserText.text = searchByUser ? SteamFriends.GetFriendPersonaName(searchLevelsByUser) : "none";

            bool searchByTitle = !searchText.IsNullOrEmpty();
            m_searchLevelsByTitleHolderObject.SetActive(!searchByUser && searchByTitle);
            m_searchLevelsByTitleText.text = searchText;
            m_clearButton.interactable = !value && searchByTitle;

            if (sourceType != 0)
            {
                m_browseCollectionsSelectedIndicatorObject.SetActive(false);
                m_browseLevelsSelectedIndicatorObject.SetActive(false);
            }

            m_backButton.interactable = !value;
            m_reloadButton.interactable = !value;
            m_sourceTabs.interactable = !value;
            m_levelTypeTabs.interactable = !value && !browseCollections;
            m_queryTabs.interactable = !value && !browseCollections;
            m_loadingIndicator.SetActive(value);
            m_nothingToDisplayLabel.SetActive(false);
            m_pageDropdownObject.SetActive(false);
            setPageButtonsActive(!value);
            refreshPagePageButton();
        }

        private void refreshPagePageButton()
        {
            bool notCollections = !browseCollections;

            int p = page;
            m_prevPageButton.gameObject.SetActive(notCollections && p > 1);
            m_nextPageButton.gameObject.SetActive(notCollections && p < ModSteamUGCUtils.pageCount);
            m_currentPageButton.gameObject.SetActive(notCollections);
            m_backButton.gameObject.SetActive(browseChildrenOfCollection != default);
            m_currentPageText.text = p.ToString();
        }

        private void refreshContainer()
        {
            m_container = browseCollections && browseChildrenOfCollection == default ? m_verticalContainer : m_gridContainer;
            m_scrollRect.content = m_container as RectTransform;
        }

        private void refreshTabContainers()
        {
            bool collections = browseCollections;
            m_tagsContainerObject.SetActive(!collections);

            if (collections)
            {
                Vector2 vector = m_mainBG.sizeDelta;
                vector.y = -25f;
                m_mainBG.sizeDelta = vector;
            }
            else
            {
                Vector2 vector = m_mainBG.sizeDelta;
                vector.y = -75f;
                m_mainBG.sizeDelta = vector;
            }
        }

        private void refreshSearchBox()
        {
            InputField inputField = m_searchBox;
            bool makeVisible = !browseCollections && sourceType == 0;

            if (!makeVisible && inputField.IsActive())
                inputField.DeactivateInputField();

            if (!makeVisible)
                inputField.text = string.Empty;

            m_searchBox.gameObject.SetActive(makeVisible);
            m_clearButton.gameObject.SetActive(makeVisible);
            m_searchLevelsButton.gameObject.SetActive(!makeVisible);
        }

        private void setBrowseItemType(bool collections)
        {
            browseCollections = collections;
            m_browseItemsOfTypeDropdownObject.SetActive(false);
            m_browseCollectionsSelectedIndicatorObject.SetActive(collections);
            m_browseLevelsSelectedIndicatorObject.SetActive(!collections);
        }

        public bool HideContextMenuIfShown()
        {
            if (m_contextMenu.gameObject.activeSelf)
            {
                m_contextMenu.gameObject.SetActive(false);
                return true;
            }
            return false;
        }

        public void ShowContextMenu(UIElementWorkshopItemDisplay itemDisplay)
        {
            bool isNull = !itemDisplay;

            m_contextMenu.gameObject.SetActive(!isNull);
            if (!isNull)
                m_contextMenu.position = itemDisplay.transform.position;

            m_contextMenuSubscribeButton.gameObject.SetActive(true);
            m_contextMenuPlayButton.gameObject.SetActive(m_selectedItemDisplays.Count == 1);
        }

        public void SetItemSelected(UIElementWorkshopItemDisplay itemDisplay, bool value)
        {
            if (value)
            {
                if (!m_selectedItemDisplays.Contains(itemDisplay))
                    m_selectedItemDisplays.Add(itemDisplay);
            }
            else
            {
                _ = m_selectedItemDisplays.Remove(itemDisplay);
            }
        }

        public bool IsItemSelected(UIElementWorkshopItemDisplay itemDisplay)
        {
            return m_selectedItemDisplays.Contains(itemDisplay);
        }

        public void QuickPreview(WorkshopItem workshopItem)
        {
            if (workshopItem == null)
            {
                m_quickPreview.gameObject.SetActive(false);
                return;
            }
            m_quickPreview.gameObject.SetActive(true);
            m_quickPreview.Populate(workshopItem);
        }

        public void OnHelpButtonClicked()
        {
            m_controlsPanel.SetActive(!m_controlsPanel.activeSelf);
        }

        public void OnTypedSearchText(string text)
        {
            searchText = text;
            m_timeLeftToPopulate = 1f;
        }

        public void OnSearchButtonClicked()
        {
            searchText = m_searchBox.text;
            Populate();
        }

        public void OnClearButtonClicked()
        {
            m_searchBox.text = string.Empty;
            searchText = null;
            Populate();
        }

        public void OnPageButtonClicked()
        {
            bool active = !m_pageDropdownObject.activeSelf && ModSteamUGCUtils.pageCount > 1;
            m_pageDropdownObject.SetActive(active);

            if (m_pageContainer.childCount != 0)
                TransformUtils.DestroyAllChildren(m_pageContainer);

            if (!active)
                return;

            for (int i = 1; i < ModSteamUGCUtils.pageCount + 1; i++)
            {
                int pageIndex = i;
                ModdedObject pageObject = Instantiate(m_pageButtonPrefab, m_pageContainer);
                pageObject.gameObject.SetActive(true);
                pageObject.GetObject<Text>(0).text = i.ToString();
                pageObject.GetObject<GameObject>(1).SetActive(i == page);
                Button button = pageObject.GetComponent<Button>();
                button.onClick.AddListener(delegate
                {
                    page = pageIndex;
                    Populate();
                });
            }
        }

        public void OnNextPageButtonClicked()
        {
            page++;
            Populate();
        }

        public void OnPrevPageButtonClicked()
        {
            page--;
            Populate();
        }

        public void OnBrowseButtonClicked()
        {
            m_browseItemsOfTypeDropdownObject.SetActive(!m_browseItemsOfTypeDropdownObject.activeSelf);
        }

        public void OnBrowseLevelsButtonClicked()
        {
            if (m_isLoading)
                return;

            setBrowseItemType(false);

            browseChildrenOfCollection = default;
            page = 1;
            sourceType = 0;
            m_tabsCanvasGroup.alpha = 1f;
            m_tabsCanvasGroup.interactable = true;
            m_sourceTabs.DeselectAllTabs();

            Populate();
        }

        public void OnBrowseCollectionsButtonClicked()
        {
            if (m_isLoading)
                return;

            setBrowseItemType(true);

            browseChildrenOfCollection = default;
            page = 1;
            sourceType = 0;
            m_tabsCanvasGroup.alpha = 0.25f;
            m_tabsCanvasGroup.interactable = false;
            m_sourceTabs.DeselectAllTabs();

            Populate();
        }

        public void OnContextMenuSubscribeButtonClicked()
        {

        }

        public void OnContextMenuPlayButtonClicked()
        {

        }

        public void OnBackButtonClicked()
        {
            if (m_isLoading)
                return;

            browseChildrenOfCollection = default;
            Populate();
        }

        public void OnLegacyUIButtonClicked()
        {
            TitleScreenUI titleScreenUI = ModCache.titleScreenUI;
            if (titleScreenUI)
            {
                Hide();
                titleScreenUI.OnWorkshopBrowserButtonClicked();
            }
        }
    }
}
