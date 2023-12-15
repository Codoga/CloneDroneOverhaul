﻿using OverhaulMod.Utils;
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
                return ModExclusiveContentManager.Instance.contentInfoList;
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
            var info = contentList.List[index];
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
            }
        }

        public static void Save()
        {
            ModUserDataManager.Instance.WriteFile(ModExclusiveContentManager.REPOSITORY_FILE, ModJsonUtils.Serialize(contentList), false);
        }

        public static FieldInfo[] GetContentFields() => editingContentBase.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
    }
}
