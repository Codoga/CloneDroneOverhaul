﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OverhaulMod.UI
{
    public class UIFileExplorerItemDisplay : OverhaulUIBehaviour, IPointerClickHandler
    {
        private float m_timeForDoubleClick;

        private Button m_button;

        public string fullName
        {
            get;
            set;
        }

        public string displayName
        {
            get;
            set;
        }

        public bool isFolder
        {
            get;
            set;
        }

        public Action<UIFileExplorerItemDisplay> clickAction
        {
            get;
            set;
        }

        public Action<UIFileExplorerItemDisplay> doubleClickAction
        {
            get;
            set;
        }

        protected override void OnInitialized()
        {
            m_button = base.GetComponent<Button>();
        }

        public override void Start()
        {
            UIElementShowTooltipOnHightLight showTooltipOnHightLight = base.gameObject.AddComponent<UIElementShowTooltipOnHightLight>();
            showTooltipOnHightLight.InitializeElement();
            showTooltipOnHightLight.tooltipText = displayName;
            showTooltipOnHightLight.tooltipShowDuration = 2f;
        }

        public bool IsDoubleClicked() => Time.unscaledTime < m_timeForDoubleClick;

        private void onClicked()
        {
            if (IsDoubleClicked())
            {
                doubleClickAction?.Invoke(this);
                return;
            }
            m_timeForDoubleClick = Time.unscaledTime + 0.25f;
            clickAction?.Invoke(this);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            onClicked();
        }
    }
}
