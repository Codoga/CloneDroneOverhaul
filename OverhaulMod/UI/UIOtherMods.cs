﻿using UnityEngine.UI;

namespace OverhaulMod.UI
{
    public class UIOtherMods : OverhaulUIBehaviour
    {
        [UIElementAction(nameof(OnExitButtonClicked))]
        [UIElement("CloseButton")]
        private readonly Button m_ExitButton;

        public override void Show()
        {
            base.Show();
            SetTitleScreenButtonActive(false);
        }

        public override void Hide()
        {
            base.Hide();
            SetTitleScreenButtonActive(true);
        }

        public void OnExitButtonClicked()
        {
            Hide();
        }
    }
}
