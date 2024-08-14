using OverhaulMod.Content.Personalization;
using OverhaulMod.Utils;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace OverhaulMod.UI
{
    public class UIPersonalizationEditorPlaytestHUD : OverhaulUIBehaviour
    {
        public override void Hide()
        {
            base.Hide();
            PersonalizationEditorManager.Instance.ExitPlaytestMode();
        }
    }
}
