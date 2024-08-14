using System.Collections.Generic;
using System.Reflection;

namespace OverhaulMod.Content
{
    public static class ExclusiveContentEditor
    {
        public static ExclusiveContentInfoList contentList
        {
            get
            {
                return ExclusiveContentManager.Instance.contentInfoList;
            }
        }

        public static ExclusiveContentInfo editingContentInfo
        {
            get;
            private set;
        }

        public static ExclusiveContentBase editingContentBase
        {
            get
            {
                return editingContentInfo?.Content;
            }
        }

        public static void CreateInfo(ExclusiveContentType contentType, ulong steamId, string playFabId)
        {
            ExclusiveContentInfo newInfo = new ExclusiveContentInfo()
            {
                ContentType = contentType,
                SteamID = steamId,
                PlayFabID = playFabId
            };

            verifyContentBase(newInfo);

            ExclusiveContentInfoList cList = contentList;
            if (cList.List == null)
                cList.List = new List<ExclusiveContentInfo>();

            cList.List.Add(newInfo);
        }

        public static void EditInfo(int index)
        {
            ExclusiveContentInfo info = contentList.List[index];
            verifyContentBase(info);
            editingContentInfo = info;
        }

        private static void verifyContentBase(ExclusiveContentInfo newInfo)
        {
            if (newInfo.Content != null)
                return;

            switch (newInfo.ContentType)
            {
                case ExclusiveContentType.ColorOverride:
                    newInfo.Content = new ExclusiveContentColorOverride();
                    break;
                case ExclusiveContentType.FeatureUnlock:
                    newInfo.Content = new ExclusiveContentFeatureUnlock();
                    break;
                case ExclusiveContentType.TesterRole:
                    newInfo.Content = new ExclusiveContentTesterRole();
                    break;
                case ExclusiveContentType.CustomizationItemsVerifierRole:
                    newInfo.Content = new ExclusiveContentItemsVerifierRole();
                    break;
            }
        }

        public static void Save()
        {
            ModDataManager.Instance.SerializeToFile(ExclusiveContentManager.REPOSITORY_FILE, contentList, false);
        }

        public static void Save(string contents)
        {
            ModDataManager.Instance.WriteFile(ExclusiveContentManager.REPOSITORY_FILE, contents, false);
        }

        public static FieldInfo[] GetContentFields() => editingContentBase.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
    }
}
