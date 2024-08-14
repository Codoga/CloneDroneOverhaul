using OverhaulMod.Utils;
using System.Collections.Generic;
using System.Text;

namespace OverhaulMod.Content.Personalization
{
    public class PersonalizationMultiplayerManager : Singleton<PersonalizationMultiplayerManager>
    {
        public const string PLAYER_INFO_UPDATED_EVENT = "MultiplayerPlayerCustomizationInfoUpdated";

        public static readonly char Separator = '|';

        private static readonly string s_dataVersion = "0";

        private string m_prefixTrue, m_prefixFalse;

        private StringBuilder m_stringBuilder;

        private Dictionary<string, PersonalizationMultiplayerPlayerInfo> m_playerInfos;

        public override void Awake()
        {
            base.Awake();
            m_stringBuilder = new StringBuilder();
            m_prefixTrue = getPrefix(true, false);
            m_prefixFalse = getPrefix(false, false);
            m_playerInfos = new Dictionary<string, PersonalizationMultiplayerPlayerInfo>();
        }

        public void SendPlayerCustomizationDataEvent(bool sendForRecentlyConnectedPlayer)
        {
            string swordSkin = normalizeId(PersonalizationController.SwordSkin);
            string bowSkin = normalizeId(PersonalizationController.BowSkin);
            string hammerSkin = normalizeId(PersonalizationController.HammerSkin);
            string spearSkin = normalizeId(PersonalizationController.SpearSkin);
            string shieldSkin = normalizeId(PersonalizationController.ShieldSkin);
            string scytheSkin = normalizeId(PersonalizationController.ScytheSkin);

            StringBuilder stringBuilder = m_stringBuilder;
            _ = stringBuilder.Clear();
            _ = stringBuilder.Append(getPrefix(sendForRecentlyConnectedPlayer));
            appendValue(stringBuilder, ModUserInfo.localPlayerPlayFabID, false);
            appendValue(stringBuilder, ModUserInfo.localPlayerSteamID.ToString(), false);
            appendValue(stringBuilder, s_dataVersion, false);
            appendValue(stringBuilder, swordSkin, false);
            appendValue(stringBuilder, bowSkin, false);
            appendValue(stringBuilder, hammerSkin, false);
            appendValue(stringBuilder, spearSkin, false);
            appendValue(stringBuilder, shieldSkin, true);
            appendValue(stringBuilder, scytheSkin, true);

            _ = GenericStringForModdingEvent.Post(sendForRecentlyConnectedPlayer ? Bolt.GlobalTargets.Others : Bolt.GlobalTargets.AllClients, Bolt.ReliabilityModes.ReliableOrdered, stringBuilder.ToString());
        }

        public void OnEvent(GenericStringForModdingEvent evnt)
        {
            string eventData = evnt.EventData;
            if (eventData.StartsWith(m_prefixFalse))
            {
                SendPlayerCustomizationDataEvent(true);
                registerPlayerInfo(eventData.Substring(m_prefixFalse.Length));
            }
            else if (eventData.StartsWith(m_prefixTrue))
            {
                registerPlayerInfo(eventData.Substring(m_prefixTrue.Length));
            }
        }

        public PersonalizationMultiplayerPlayerInfo GetPlayInfo(string playFaId)
        {
            if (playFaId == null)
                return null;

            if (!m_playerInfos.TryGetValue(playFaId, out PersonalizationMultiplayerPlayerInfo playerInfo))
                return null;

            return playerInfo;
        }

        private void registerPlayerInfo(string rawData)
        {
            ModDebug.Log(rawData);
            if (rawData.Length > 16)
            {
                string playFabId = rawData.Remove(16);
                ModDebug.Log(playFabId);

                if (m_playerInfos.ContainsKey(playFabId))
                {
                    m_playerInfos[playFabId].SetData(rawData);
                }
                else
                {
                    m_playerInfos.Add(playFabId, new PersonalizationMultiplayerPlayerInfo(rawData));
                }
                GlobalEventManager.Instance.Dispatch(PLAYER_INFO_UPDATED_EVENT, playFabId);
            }
        }

        private void appendValue(StringBuilder stringBuilder, string value, bool isLast)
        {
            _ = stringBuilder.Append(value);
            if (!isLast)
                _ = stringBuilder.Append(Separator);
        }

        private string getPrefix(bool value, bool useCache = true)
        {
            if (useCache)
            {
                return value ? m_prefixTrue : m_prefixFalse;
            }
            return $"[OverhaulV4_{value.ToString().ToLower()}] ";
        }

        private string normalizeId(string id)
        {
            if (id.IsNullOrEmpty())
                id = "_";

            return id;
        }

        public static bool CompareDataVersion(string dataVersion)
        {
            return dataVersion == s_dataVersion;
        }
    }
}
