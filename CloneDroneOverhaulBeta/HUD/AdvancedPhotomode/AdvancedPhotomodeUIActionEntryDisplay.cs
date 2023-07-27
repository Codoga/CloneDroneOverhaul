﻿using CDOverhaul.Gameplay.QualityOfLife;
using UnityEngine.UI;

namespace CDOverhaul.HUD
{
    public class AdvancedPhotomodeUIActionEntryDisplay : OverhaulBehaviour
    {
        public Button ButtonReference
        {
            get;
            private set;
        }

        public AdvancedPhotomodeSettingAttribute SettingReference
        {
            get;
            private set;
        }

        public void Initialize(AdvancedPhotomodeSettingAttribute setting)
        {
            ButtonReference = base.GetComponent<Button>();
            ButtonReference.onClick.AddListener(onButtonClick);
            SettingReference = setting;
        }

        private void onButtonClick()
        {
            _ = SettingReference.Method.Invoke(null, null);
        }
    }
}
