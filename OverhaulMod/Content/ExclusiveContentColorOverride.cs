using OverhaulMod.Utils;
using UnityEngine;

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

            string playFabID = firstPersonMover.GetPlayFabID();
            if (GameModeManager.IsSinglePlayer())
            {
                ExclusiveContentManager exclusiveContentManager = ExclusiveContentManager.Instance;
                MultiplayerLoginManager multiplayerLoginManager = MultiplayerLoginManager.Instance;
                playFabID = (multiplayerLoginManager && multiplayerLoginManager.IsLoggedIntoPlayfab() && !multiplayerLoginManager.IsBanned()) ? multiplayerLoginManager.GetLocalPlayFabID() : exclusiveContentManager?.localPlayFabId;
            }

            if (playFabID.IsNullOrEmpty() || InfoReference.PlayFabID != playFabID)
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
