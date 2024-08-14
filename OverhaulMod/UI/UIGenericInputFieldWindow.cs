using OverhaulMod.Utils;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace OverhaulMod.UI
{
    public class UIGenericInputFieldWindow : OverhaulUIBehaviour
    {
        [UIElementAction(nameof(Hide))]
        [UIElement("CloseButton")]
        private readonly Button m_closeButton;

        [UIElement("Header")]
        private readonly Text m_headerText;

        [UIElement("Description")]
        private readonly Text m_descriptionText;

        [UIElementAction(nameof(OnDoneButtonClicked))]
        [UIElement("DoneButton")]
        private readonly Button m_doneButton;

        [UIElement("InputField")]
        private readonly InputField m_inputField;

        [UIElement("Panel")]
        private readonly CanvasGroup m_panelCanvasGroup;

        [UIElement("Panel")]
        private readonly RectTransform m_panelTransform;

        public override bool refreshOnlyCursor => true;

        public Action<string> doneAction
        {
            get;
            set;
        }

        public override void Show()
        {
            base.Show();
            m_inputField.ActivateInputField();
        }

        public override void Update()
        {
            m_doneButton.interactable = !m_inputField.text.IsNullOrEmpty();
        }

        public void OnDoneButtonClicked()
        {
            doneAction?.Invoke(m_inputField.text);
            doneAction = null;
            Hide();
        }

        public void SetTexts(string header, string description)
        {
            m_headerText.text = header;
            m_descriptionText.text = description;
        }

        public void SetHeight(float height)
        {
            Vector2 sizeDelta = m_panelTransform.sizeDelta;
            sizeDelta.y = Mathf.Max(100f, height);
            m_panelTransform.sizeDelta = sizeDelta;
        }

        public void SetInputFieldText(string text, int limit)
        {
            m_inputField.text = text;
            m_inputField.characterLimit = Mathf.Max(0, limit);
        }
    }
}
