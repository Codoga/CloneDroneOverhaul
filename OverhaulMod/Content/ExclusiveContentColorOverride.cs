﻿using UnityEngine;

namespace OverhaulMod.Content
{
    public class ExclusiveContentColorOverride : ExclusiveContentBase
    {
        public Color TheColor;

        public int ColorIndex;

        public bool GetOverrideRobotColor(FirstPersonMover firstPersonMover, Color currentColor, out Color color)
        {
            color = currentColor;
            if (InfoReference == null || !firstPersonMover || !firstPersonMover.IsPlayer())
                return false;

            string playFabID = GameModeManager.IsSinglePlayer() ? MultiplayerLoginManager.Instance.GetLocalPlayFabID() : firstPersonMover.GetPlayFabID();

            if (string.IsNullOrEmpty(playFabID) || InfoReference.PlayFabID != playFabID)
                return false;

            int index = 0;
            foreach (HumanFavouriteColor favColor in HumanFactsManager.Instance.FavouriteColors)
            {
                if (favColor == null)
                    continue;

                if (favColor.ColorValue.Equals(currentColor) && index == ColorIndex)
                {
                    color = TheColor;
                    return true;
                }
                index++;
            }
            return false;
        }
    }
}