using OverhaulMod.Utils;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace OverhaulMod.UI
{
    public class UIMessagePopup : OverhaulUIBehaviour
    {
        [UIElementAction(nameof(Hide))]
        [UIElement("CloseButton")]
        private readonly Button m_closeButton;

        [UIElement("Header")]
        private readonly Text m_headerText;

        [UIElement("Description")]
        private readonly Text m_descriptionText;

        [UIElementAction(nameof(OnOkButtonClicked))]
        [UIElement("OKButton", false)]
        private readonly Button m_okButton;

        [UIElementAction(nameof(OnYesButtonClicked))]
        [UIElement("YesButton", false)]
        private readonly Button m_yesButton;

        [UIElementAction(nameof(OnNoButtonClicked))]
        [UIElement("NoButton", false)]
        private readonly Button m_noButton;

        [UIElement("OKButtonText")]
        private readonly Text m_okButtonText;

        [UIElement("YesButtonText")]
        private readonly Text m_yesButtonText;

        [UIElement("NoButtonText")]
        private readonly Text m_noButtonText;

        [UIElement("Panel")]
        private readonly CanvasGroup m_panelCanvasGroup;

        [UIElement("Panel")]
        private readonly RectTransform m_panelTransform;

        [UIElementIgnoreIfMissing]
        [UIElement("ScrollRect")]
        private readonly GameObject m_scrollRectObject;

        public bool IsFullscreen;

        private bool m_shouldRefreshText;

        public override bool refreshOnlyCursor => true;

        public Action okButtonAction
        {
            get;
            private set;
        }

        public Action yesButtonAction
        {
            get;
            private set;
        }

        public Action noButtonAction
        {
            get;
            private set;
        }

        public override void Hide()
        {
            base.Hide();
            okButtonAction = null;
            yesButtonAction = null;
            noButtonAction = null;
        }

        public override void Update()
        {
            base.Update();
            if (m_shouldRefreshText)
            {
                m_shouldRefreshText = false;

                Vector2 sizeDelta = m_descriptionText.rectTransform.sizeDelta;
                sizeDelta.y = m_descriptionText.preferredHeight + 30f;
                m_descriptionText.rectTransform.sizeDelta = sizeDelta;
            }
        }

        public void OnOkButtonClicked()
        {
            okButtonAction?.Invoke();
            Hide();
        }

        public void OnYesButtonClicked()
        {
            yesButtonAction?.Invoke();
            Hide();
        }

        public void OnNoButtonClicked()
        {
            noButtonAction?.Invoke();
            Hide();
        }

        public void SetTexts(string header, string description)
        {
            m_headerText.text = header;
            m_descriptionText.text = description;
            refreshTextHeightNextFrame();

            if (m_scrollRectObject)
            {
                m_scrollRectObject.SetActive(!description.IsNullOrEmpty());
            }
        }

        public void SetHeight(float height)
        {
            Vector2 sizeDelta = m_panelTransform.sizeDelta;
            sizeDelta.y = Mathf.Max(100f, height);
            m_panelTransform.sizeDelta = sizeDelta;
        }

        public void SetButtonLayout(MessageMenu.ButtonLayout buttonLayout)
        {
            m_okButton.gameObject.SetActive(buttonLayout == MessageMenu.ButtonLayout.OkButton);
            m_yesButton.gameObject.SetActive(buttonLayout == MessageMenu.ButtonLayout.EnableDisableButtons);
            m_noButton.gameObject.SetActive(buttonLayout == MessageMenu.ButtonLayout.EnableDisableButtons);
        }

        public void SetButtonActions(Action ok, Action yes, Action no)
        {
            okButtonAction = ok;
            yesButtonAction = yes;
            noButtonAction = no;
        }

        public void SetButtonTexts(string okText, string yesText, string noText)
        {
            if (okText == null)
                okText = string.Empty;
            if (yesText == null)
                yesText = string.Empty;
            if (noText == null)
                noText = string.Empty;

            if (yesText.ToLower() == "yes")
                yesText = LocalizationManager.Instance.GetTranslatedString("button_yes");
            if (noText.ToLower() == "no")
                noText = LocalizationManager.Instance.GetTranslatedString("button_no");

            m_okButtonText.text = okText;
            m_yesButtonText.text = yesText;
            m_noButtonText.text = noText;
        }

        private void refreshTextHeightNextFrame()
        {
            m_shouldRefreshText = true;
        }
    }
}
