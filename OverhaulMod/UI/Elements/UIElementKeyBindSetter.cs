﻿using OverhaulMod.Utils;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace OverhaulMod.UI
{
    public class UIElementKeyBindSetter : OverhaulUIBehaviour
    {
        [UIElement("Text")]
        private readonly Text m_keyBindText;

        [UIElementAction(nameof(OnSetBindButtonClicked))]
        [UIElement("SetBindButton")]
        private readonly Button m_setBindButton;

        [UIElementAction(nameof(OnSetDefaultButtonClicked))]
        [UIElement("SetDefaultBindButton")]
        private readonly Button m_setDefaultBindButton;

        [UIElement("Description")]
        private readonly Text m_description;

        private KeyCode m_key;
        public KeyCode key
        {
            get
            {
                return m_key;
            }
            set
            {
                m_key = value;
                m_keyBindText.text = value.ToString().Replace("Alpha", string.Empty);
                onValueChanged.Invoke(value);
            }
        }

        public KeyCode defaultKey { get; set; }

        public KeyCodeChangedEvent onValueChanged { get; set; } = new KeyCodeChangedEvent();

        public void OnSetBindButtonClicked()
        {
            ModUIUtils.KeyBinder(m_description.text, defaultKey, delegate (KeyCode kc)
            {
                if (kc == (KeyCode)(-1))
                    return;

                key = kc;
            }, null);
        }

        public void OnSetDefaultButtonClicked()
        {
            key = defaultKey;
        }

        public void SetDescription(string text)
        {
            m_description.text = text;
        }

        [Serializable]
        public class KeyCodeChangedEvent : UnityEvent<KeyCode>
        {
        }
    }
}
