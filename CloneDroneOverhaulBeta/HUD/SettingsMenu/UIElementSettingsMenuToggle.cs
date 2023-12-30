﻿using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CDOverhaul.HUD
{
    public class UIElementSettingsMenuToggle : OverhaulBehaviour
    {
        [UIElementReference("EnabledGraphic")]
        private readonly GameObject m_EnabledStateObjects;
        [UIElementReference("HandleOn")]
        private readonly Graphic m_EnabledStateHandle;

        [UIElementReference("DisabledGraphic")]
        private readonly GameObject m_DisabledStateObjects;
        [UIElementReference("HandleOff")]
        private readonly Graphic m_DisabledStateHandle;

        private Button m_Button;

        private bool m_IsOn;
        public bool isOn
        {
            get => m_IsOn;
            set
            {
                m_IsOn = value;
                m_DisabledStateObjects.SetActive(!value);
                m_EnabledStateObjects.SetActive(value);
                m_Button.targetGraphic = value ? m_EnabledStateHandle : m_DisabledStateHandle;
            }
        }

        public UnityAction<bool> onValueChanged { get; set; }

        public override void Awake()
        {
            m_Button = base.GetComponent<Button>();
            m_Button.AddOnClickListener(OnClicked);
            UIController.AssignVariables(this);
        }

        public void OnClicked()
        {
            isOn = !isOn;
            onValueChanged?.Invoke(isOn);
        }
    }
}