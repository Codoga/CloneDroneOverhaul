using OverhaulMod.Content.Personalization;
using UnityEngine;
using UnityEngine.UI;

namespace OverhaulMod.UI
{
    public class UIElementPersonalizationItemDescriptionBox : OverhaulUIBehaviour
    {
        [UIElement("ItemName")]
        private readonly Text m_itemNameText;

        [UIElement("ItemDescription")]
        private readonly Text m_itemDescriptionText;

        [UIElement("EquipButton")]
        private readonly GameObject m_unequippedIndicatorObject;

        [UIElement("EquippedLabel")]
        private readonly GameObject m_equippedIndicatorObject;

        [UIElement("LockedOverlay")]
        private readonly GameObject m_lockedOverlay;

        [UIElement("NonVerifiedOverlay")]
        private readonly GameObject m_nonVerifiedOverlay;

        [UIElement("LockedNonVerifiedOverlay")]
        private readonly GameObject m_lockedNonVerifiedOverlay;

        [UIElement("DescriptionBox")]
        private readonly RectTransform m_boxTransform;

        [UIElementAction(nameof(OnEquipButtonClicked))]
        [UIElement("EquipButton")]
        private readonly Button m_equipButton;

        private PersonalizationItemInfo m_selectedItemInfo;

        private UIElementMouseEventsComponent m_mouseEvents;

        private UIPersonalizationItemBrowser m_browser;

        private Animator m_animator;

        private bool m_refreshBoxNextFrame;

        protected override void OnInitialized()
        {
            m_mouseEvents = base.gameObject.AddComponent<UIElementMouseEventsComponent>();
            m_animator = base.GetComponent<Animator>();
        }

        public override void Update()
        {
            if (Input.GetMouseButtonDown(0) && !m_mouseEvents.isMouseOverElement && !m_browser.IsMouseOverPanel())
            {
                Hide();
            }

            if (m_refreshBoxNextFrame)
            {
                m_refreshBoxNextFrame = false;

                float initialHeight = 75f - (m_lockedOverlay.activeSelf || m_lockedNonVerifiedOverlay.activeSelf || m_nonVerifiedOverlay.activeSelf ? 0f : 20f);
                float preferredTextHeight = m_itemDescriptionText.preferredHeight + 10f;

                RectTransform t = m_boxTransform;
                Vector2 sizeDelta = t.sizeDelta;
                sizeDelta.y = initialHeight + preferredTextHeight;
                t.sizeDelta = sizeDelta;

                RectTransform t2 = m_itemDescriptionText.rectTransform;
                Vector2 sizeDelta2 = t2.sizeDelta;
                sizeDelta2.y = preferredTextHeight;
                t2.sizeDelta = sizeDelta2;
            }
        }

        public void SetBrowserUI(UIPersonalizationItemBrowser personalizationItemsBrowser)
        {
            m_browser = personalizationItemsBrowser;
        }

        public void ShowForItem(PersonalizationItemInfo itemInfo, RectTransform rectTransform)
        {
            if (itemInfo == null || rectTransform == null)
            {
                Hide();
                return;
            }

            Show();
            if (m_selectedItemInfo == itemInfo)
                return;

            m_animator.Play(string.Empty);
            m_selectedItemInfo = itemInfo;

            m_itemNameText.text = itemInfo.Name;
            m_itemDescriptionText.text = itemInfo.Description;

            /*
            bool equipped = itemInfo.IsEquipped();
            m_unequippedIndicatorObject.SetActive(!equipped);
            m_equippedIndicatorObject.SetActive(equipped);*/

            bool isLocked = !itemInfo.IsUnlocked();
            m_lockedOverlay.SetActive(isLocked && itemInfo.IsVerified);
            m_nonVerifiedOverlay.SetActive(!itemInfo.IsVerified && !isLocked);
            m_lockedNonVerifiedOverlay.SetActive(!itemInfo.IsVerified && isLocked);
            m_equipButton.interactable = !isLocked;

            Transform transform = base.transform;
            Vector3 vector = transform.position;
            vector.y = rectTransform.position.y;
            transform.position = vector;

            m_refreshBoxNextFrame = true;
        }

        public void OnEquipButtonClicked()
        {
        }
    }
}
