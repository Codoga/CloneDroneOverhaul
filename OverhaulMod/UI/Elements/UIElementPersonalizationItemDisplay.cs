using OverhaulMod.Content.Personalization;
using OverhaulMod.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OverhaulMod.UI
{
    public class UIElementPersonalizationItemDisplay : OverhaulUIBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public const string ITEM_UNEQUIPPED_BG_COLOR = "#404040";
        public const string ITEM_EQUIPPED_BG_COLOR = "#305EE0";

        public const string ITEM_UNEQUIPPED_NAME_BG_COLOR = "#272727";
        public const string ITEM_EQUIPPED_NAME_BG_COLOR = "#10204F";

        [UIElement("NameBG")]
        private readonly Image m_nameBg;

        [UIElement("Frame")]
        private readonly Image m_frame;

        [UIElement("ItemName")]
        private readonly Text m_nameText;

        [UIElement("NameHolder")]
        private readonly RectTransform m_nameHolder;

        [UIElement("NewIndicator")]
        private readonly GameObject m_newIndicator;

        [UIElement("FavoriteIndicator")]
        private readonly GameObject m_favoriteIndicator;

        [UIElement("VerifiedIndicator")]
        private readonly GameObject m_wasVerifiedIndicator;

        [UIElement("UpdatedIndicator")]
        private readonly GameObject m_wasUpdatedIndicator;

        private Button m_button;

        private Image m_bg;

        private UIPersonalizationItemBrowser m_browser;

        private RectTransform m_rectTransform;

        public PersonalizationItemInfo ItemInfo;

        private float m_timeForDoubleClick;

        private bool m_refreshNameHolderNextFrame;

        protected override void OnInitialized()
        {
            m_rectTransform = base.GetComponent<RectTransform>();
            m_bg = base.GetComponent<Image>();
            m_button = base.GetComponent<Button>();
            m_button.onClick.AddListener(onClicked);

            GlobalEventManager.Instance.AddEventListener(PersonalizationManager.ITEM_EQUIPPED_OR_UNEQUIPPED_EVENT, RefreshDisplays);
            RefreshDisplays();

            m_timeForDoubleClick = -1f;
            m_refreshNameHolderNextFrame = true;
        }

        public override void Update()
        {
            if (m_refreshNameHolderNextFrame)
            {
                m_refreshNameHolderNextFrame = false;

                RectTransform rectTransform = m_nameHolder;
                Vector2 sizeDelta = rectTransform.sizeDelta;
                sizeDelta.x = Mathf.Min(m_nameText.preferredWidth + 1f, 175f);
                rectTransform.sizeDelta = sizeDelta;
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            GlobalEventManager.Instance.RemoveEventListener(PersonalizationManager.ITEM_EQUIPPED_OR_UNEQUIPPED_EVENT, RefreshDisplays);
        }

        public bool IsDoubleClicked() => m_timeForDoubleClick != -1f && Time.unscaledTime < m_timeForDoubleClick;

        public void SetBrowserUI(UIPersonalizationItemBrowser itemsBrowser)
        {
            m_browser = itemsBrowser;
        }

        public void RefreshDisplays()
        {
            PersonalizationItemInfo itemInfo = ItemInfo;
            if (itemInfo == null)
                return;

            PersonalizationUserInfo personalizationUserInfo = PersonalizationManager.Instance.userInfo;

            bool wasUpdated = personalizationUserInfo.GetItemVersion(itemInfo) != itemInfo.Version;
            bool wasVerified = personalizationUserInfo.IsItemUnverified(itemInfo) && itemInfo.IsVerified;
            bool isDiscovered = personalizationUserInfo.IsItemDiscovered(itemInfo);
            bool isFavorite = personalizationUserInfo.IsItemFavorite(itemInfo);
            bool equipped = itemInfo.IsEquipped();

            Color bgColor = ModParseUtils.TryParseToColor(equipped ? ITEM_EQUIPPED_BG_COLOR : ITEM_UNEQUIPPED_BG_COLOR, Color.white);
            Color nameBgColor = ModParseUtils.TryParseToColor(equipped ? ITEM_EQUIPPED_NAME_BG_COLOR : ITEM_UNEQUIPPED_NAME_BG_COLOR, Color.white);

            m_bg.color = bgColor;
            m_nameBg.color = nameBgColor;
            m_frame.color = nameBgColor;
            m_favoriteIndicator.SetActive(isFavorite);
            m_newIndicator.SetActive(!isDiscovered && !wasVerified && !wasUpdated);
            m_wasVerifiedIndicator.SetActive(wasVerified);
            m_wasUpdatedIndicator.SetActive(wasUpdated && !wasVerified && isDiscovered);
        }

        private void onClicked()
        {
            PersonalizationItemInfo itemInfo = ItemInfo;
            if (itemInfo == null || !itemInfo.IsUnlocked())
                return;

            updateItemUserInfo();

            PersonalizationManager.Instance.EquipItem(itemInfo);
            m_browser.MakeDefaultSkinButtonInteractable();
        }

        private void updateItemUserInfo()
        {
            PersonalizationItemInfo itemInfo = ItemInfo;
            if (itemInfo == null)
                return;

            PersonalizationUserInfo userInfo = PersonalizationManager.Instance?.userInfo;
            if (userInfo == null)
                return;

            if (!userInfo.IsItemDiscovered(itemInfo))
            {
                userInfo.SetIsItemDiscovered(itemInfo);
                m_newIndicator.SetActive(false);
            }

            if (userInfo.IsItemUnverified(itemInfo) && itemInfo.IsVerified)
            {
                userInfo.SetIsItemUnverified(itemInfo, false);
                m_wasVerifiedIndicator.SetActive(false);
            }

            if (userInfo.GetItemVersion(itemInfo) != itemInfo.Version)
            {
                userInfo.SetItemVersion(itemInfo, itemInfo.Version);
                m_wasUpdatedIndicator.SetActive(false);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            PersonalizationItemInfo itemInfo = ItemInfo;
            if (itemInfo == null)
                return;

            if (!itemInfo.IsUnlocked())
            {
                updateItemUserInfo();
            }

            m_browser.ShowDescriptionBox(itemInfo, m_rectTransform);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            m_browser.ShowDescriptionBox(null, null);
        }
    }
}
