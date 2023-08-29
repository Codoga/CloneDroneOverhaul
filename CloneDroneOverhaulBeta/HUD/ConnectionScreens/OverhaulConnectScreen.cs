﻿namespace CDOverhaul.HUD
{
    public class OverhaulConnectScreen : UIController
    {
        public static bool IsEnabled() => OverhaulFeatureAvailabilitySystem.ImplementedInBuild.IsConnectionScreensRedesignEnabled;

        public override void Initialize()
        {
            base.Initialize();
            if (!IsEnabled())
                return;
        }

        public override void Show()
        {
            base.Show();
        }

        public override void Hide()
        {
            base.Hide();
        }
    }
}
