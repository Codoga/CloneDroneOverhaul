using Discord;
using OverhaulMod.Utils;

namespace OverhaulMod.Engine
{
    public class RichPresenceDiscord : RichPresenceBase
    {
        /// <summary>
        /// Overhaul mod Discord App ID
        /// </summary>
        public const long APP_ID = 1091373211163308073;
        public const CreateFlags CREATE_FLAG = CreateFlags.NoRequireDiscord;

        private Discord.Discord m_client;
        private Activity m_activity;
        private ActivityParty m_party;
        private ActivitySecrets m_secrets;
        private PartySize m_partySize;
        private ActivityManager.UpdateActivityHandler m_activityHandler;

        public override void Start()
        {
            TryInitializeDiscord();
        }

        public override void OnDestroy()
        {
            DisposeDiscordClient();
        }

        private void OnApplicationQuit()
        {
            DisposeDiscordClient();
        }

        public override void Update()
        {
            base.Update();
            try
            {
                if (m_client != null)
                    m_client.RunCallbacks();
            }
            catch
            {
            }
        }

        public override void RefreshInformation()
        {
            base.RefreshInformation();
            if (m_client != null)
            {
                ActivityManager manager = m_client.GetActivityManager();
                if (manager == null)
                    return;

                bool isInModdedMultiplayer = ModIntegrationUtils.ModdedMultiplayer.IsInModdedMultiplayer();
                string id = isInModdedMultiplayer ? $"{ModIntegrationUtils.ModdedMultiplayer.GetCurrentGameModeInfoID()}_{ModIntegrationUtils.ModdedMultiplayer.GetLobbyID()}" : null;

                PartySize partySize = m_partySize;
                partySize.CurrentSize = isInModdedMultiplayer ? ModIntegrationUtils.ModdedMultiplayer.GetCurrentPlayerCount() : 0;
                partySize.MaxSize = isInModdedMultiplayer ? ModIntegrationUtils.ModdedMultiplayer.GetMaxPlayerCount() : 0;

                ActivitySecrets activitySecrets = m_secrets;
                activitySecrets.Join = isInModdedMultiplayer ? $"lobby_{id}" : null;

                ActivityParty party = m_party;
                party.Id = isInModdedMultiplayer ? $"cdo_{id}" : null;
                party.Size = partySize;

                Activity activity = m_activity;
                activity.State = !gameModeDetailsString.IsNullOrEmpty() ? gameModeDetailsString : string.Empty;
                activity.Details = $"v{ModBuildInfo.version} · {gameModeString}";
                activity.Party = party;
                activity.Secrets = activitySecrets;

                manager.UpdateActivity(activity, m_activityHandler);
            }
        }

        public void TryInitializeDiscord()
        {
            if (m_client == null)
            {
                try
                {
                    Discord.Discord client = new Discord.Discord(APP_ID, (ulong)CREATE_FLAG);
#if DEBUG
                    client.SetLogHook(LogLevel.Debug, (level, message) =>
                    {
                        switch (level)
                        {
                            case LogLevel.Error:
                            case LogLevel.Warn:
                                ModDebug.LogWarning($"Discord RPC: {message}");
                                break;
                            default:
                                ModDebug.Log($"Discord RPC: {message}");
                                break;
                        }
                    });
#endif
                    RelationshipManager relationshipManager = client.GetRelationshipManager();
                    ActivityManager activityManager = client.GetActivityManager();
                    activityManager.OnActivityJoin += secret =>
                    {
                        string[] split = secret.Split('_');
                        string gameModeId = split[1];
                        string lobbyCode = split[2];

                        ModIntegrationUtils.ModdedMultiplayer.OnInvite(gameModeId, lobbyCode);
                    };
                    activityManager.OnActivityJoinRequest += (ref User user) =>
                    {
                        Relationship relationship = relationshipManager.Get(user.Id);
                        ActivityJoinRequestReply reply = ActivityJoinRequestReply.Ignore;

                        switch (relationship.Type)
                        {
                            case RelationshipType.Friend:
                            case RelationshipType.Implicit:
                            case RelationshipType.PendingOutgoing:
                                {
                                    reply = ActivityJoinRequestReply.Yes;
                                    break;
                                }
                        }

                        activityManager.SendRequestReply(user.Id, reply, _ => { });
                    };

                    m_client = client;

                    Activity activity = new Activity()
                    {
                        Assets =
                        {
                            LargeImage = "defaultimage",
                            LargeText = "Overhaul Mod",
                        },
                    };
                    m_activity = activity;
                }
                catch
                {
                    base.enabled = false;
                }
            }

            if (m_activityHandler == null)
                m_activityHandler = new ActivityManager.UpdateActivityHandler(handleActivityUpdate);
        }

        public void DisposeDiscordClient()
        {
            try
            {
                if (m_client != null)
                {
                    m_client.Dispose();
                    m_client = null;
                }
            }
            catch
            {

            }
        }

        private void handleActivityUpdate(Result res)
        {
            if (res != Result.Ok)
                Destroy(this);
        }
    }
}
