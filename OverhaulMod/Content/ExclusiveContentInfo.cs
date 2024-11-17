using OverhaulMod.Utils;
using Steamworks;

namespace OverhaulMod.Content
{
    public class ExclusiveContentInfo
    {
        public ExclusiveContentType ContentType;

        public ulong SteamID;

        public string PlayFabID;

        public ExclusiveContentBase Content;

        public string Name;

        public void VerifyFields()
        {
            if (Content != null && Content.InfoReference == null)
            {
                Content.InfoReference = this;
            }
        }

        public bool HasSteamID()
        {
            return SteamID != default;
        }

        public bool HasPlayFabID()
        {
            return !PlayFabID.IsNullOrEmpty();
        }

        public bool IsAvailable()
        {
            ExclusiveContentManager exclusiveContentManager = ExclusiveContentManager.Instance;
            if (!exclusiveContentManager)
                return !HasPlayFabID() && !HasSteamID();

            bool hasSteamId = HasSteamID();
            if (hasSteamId)
            {
                SteamManager steamManager = SteamManager.Instance;
                if ((steamManager && steamManager.Initialized) ? SteamID == (ulong)SteamUser.GetSteamID() : SteamID == exclusiveContentManager.localSteamId)
                    return true;
            }

            bool hasPlayFabId = HasPlayFabID();
            if (hasPlayFabId)
            {
                MultiplayerLoginManager multiplayerLoginManager = MultiplayerLoginManager.Instance;
                if ((multiplayerLoginManager && multiplayerLoginManager.IsLoggedIntoPlayfab() && !multiplayerLoginManager.IsBanned()) ? multiplayerLoginManager.GetLocalPlayFabID() == PlayFabID : exclusiveContentManager.localPlayFabId == PlayFabID)
                    return true;
            }

            return !hasSteamId && !hasPlayFabId;
        }
    }
}
