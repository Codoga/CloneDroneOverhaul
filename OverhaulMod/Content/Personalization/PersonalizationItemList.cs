using OverhaulMod.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OverhaulMod.Content.Personalization
{
    public class PersonalizationItemList
    {
        public List<PersonalizationItemInfo> Items;

        public List<PersonalizationItemInfo> DuplicateItems;

        [NonSerialized]
        public Dictionary<string, Exception> ItemLoadErrors;

        [NonSerialized]
        public Exception LoadError;

        [NonSerialized]
        private int m_version;

        public void Load()
        {
            m_version++;
            Dictionary<string, Exception> errors = new Dictionary<string, Exception>();
            List<PersonalizationItemInfo> list = Items ?? new List<PersonalizationItemInfo>();
            list.Clear();

            if (DuplicateItems == null)
                DuplicateItems = new List<PersonalizationItemInfo>();
            List<string> directories;

            int userItemsStartIndex;
            try
            {
                directories = new List<string>();
                directories.AddRange(Directory.GetDirectories(ModCore.customizationFolder));
                userItemsStartIndex = directories.Count;
                directories.AddRange(Directory.GetDirectories(ModCore.customizationPersistentFolder));
            }
            catch (Exception exc)
            {
                LoadError = exc;
                Items = list;
                ItemLoadErrors = errors;
                return;
            }

            Items = list;
            ItemLoadErrors = errors;

            if (directories.IsNullOrEmpty())
                return;

            int index = -1;
            foreach (string directory in directories)
            {
                index++;

                try
                {
                    PersonalizationItemInfo personalizationItemInfo = LoadItemInfo(directory);
                    personalizationItemInfo.HideInBrowser = userItemsStartIndex != -1 && index < userItemsStartIndex && !personalizationItemInfo.IsVerified;
                    if (getItem(personalizationItemInfo.ItemID, list) == null)
                        list.Add(personalizationItemInfo);
                    else
                        DuplicateItems.Add(personalizationItemInfo);
                }
                catch (Exception exc)
                {
                    if (errors.ContainsKey(directory))
                        continue;

                    errors.Add(directory, exc);
                }
            }

            PersonalizationCacheManager.Instance.CacheFiles(list);
        }

        public PersonalizationItemInfo LoadItemInfo(string directory)
        {
            string rootDirectory = Directory.GetParent(directory).FullName;
            string rootDirectoryName = ModIOUtils.GetDirectoryName(rootDirectory);

            string infoFilePath = Path.Combine(directory, PersonalizationEditorManager.ITEM_INFO_FILE);
            string metaDataFilePath = Path.Combine(directory, PersonalizationEditorManager.ITEM_META_DATA_FILE);

            bool updateInfoFile = false;
            bool updateMetaDataFile = false;

            PersonalizationItemMetaData personalizationItemMetaData;
            if (File.Exists(metaDataFilePath))
            {
                personalizationItemMetaData = ModJsonUtils.DeserializeStream<PersonalizationItemMetaData>(metaDataFilePath);
            }
            else
            {
                updateMetaDataFile = true;
                personalizationItemMetaData = new PersonalizationItemMetaData() // the first version of customization system (0) didn't have meta data files
                {
                    CustomizationSystemVersion = 0,
                };
            }

            PersonalizationItemInfo personalizationItemInfo;
            if (File.Exists(infoFilePath))
            {
                string filesDirectory = Path.Combine(directory, "files");
                if (!Directory.Exists(filesDirectory))
                    _ = Directory.CreateDirectory(filesDirectory);

                string rawData = ModIOUtils.ReadText(infoFilePath);
                if (personalizationItemMetaData.CustomizationSystemVersion < 1) // meta data files update, renamed OverhaulMod.Content.Personalization.PersonalizationEditorObjectShowConditions to OverhaulMod.Engine.WeaponVariant
                {
                    updateMetaDataFile = true;
                    updateInfoFile = true;

                    rawData = rawData.Replace("OverhaulMod.Content.Personalization.PersonalizationEditorObjectShowConditions", "OverhaulMod.Engine.WeaponVariant");
                }
                if (personalizationItemMetaData.CustomizationSystemVersion < 2) // removed "Is" from every value name in WeaponVariant enum
                {
                    updateMetaDataFile = true;
                    updateInfoFile = true;

                    rawData = rawData.Replace("\"IsNormal\"", "\"Normal\"");
                    rawData = rawData.Replace("\"IsOnFire\"", "\"OnFire\"");
                    rawData = rawData.Replace("\"IsNormalMultiplayer\"", "\"NormalMultiplayer\"");
                    rawData = rawData.Replace("\"IsOnFireMultiplayer\"", "\"OnFireMultiplayer\"");
                }
                personalizationItemMetaData.CustomizationSystemVersion = PersonalizationItemMetaData.CurrentCustomizationSystemVersion;

                personalizationItemInfo = ModJsonUtils.Deserialize<PersonalizationItemInfo>(rawData);
                personalizationItemInfo.FolderPath = directory;
                personalizationItemInfo.RootFolderPath = rootDirectory;
                personalizationItemInfo.RootFolderName = rootDirectoryName;
                personalizationItemInfo.IsPersistentAsset = rootDirectoryName == ModCore.CUSTOMIZATION_PERSISTENT_FOLDER_NAME;
                personalizationItemInfo.MetaData = personalizationItemMetaData;
                personalizationItemInfo.FixValues();

                if (updateInfoFile)
                    ModIOUtils.WriteText(rawData, infoFilePath);
            }
            else
            {
                personalizationItemInfo = null;
            }

            if (updateMetaDataFile)
                ModJsonUtils.WriteStream(metaDataFilePath, personalizationItemMetaData);

            return personalizationItemInfo;
        }

        private PersonalizationItemInfo getItem(string id, List<PersonalizationItemInfo> list)
        {
            foreach (PersonalizationItemInfo item in list)
            {
                if (item.ItemID == id)
                    return item;
            }
            return null;
        }

        public PersonalizationItemInfo GetItem(string id)
        {
            return getItem(id, GetItems());
        }

        public List<PersonalizationItemInfo> GetItems()
        {
            return Items;
        }

        public List<PersonalizationItemInfo> GetItems(PersonalizationCategory personalizationCategory)
        {
            List<PersonalizationItemInfo> list = new List<PersonalizationItemInfo>();
            foreach (PersonalizationItemInfo item in GetItems())
            {
                if (item.Category == personalizationCategory)
                {
                    list.Add(item);
                }
            }
            return list;
        }

        public List<PersonalizationItemInfo> GetItems(PersonalizationCategory personalizationCategory, PersonalizationItemsSortType sort)
        {
            // todo: cache this using versions
            List<PersonalizationItemInfo> result;
            List<PersonalizationItemInfo> list = GetItems(personalizationCategory);
            switch (sort)
            {
                case PersonalizationItemsSortType.Alphabet:
                    result = GetItems(personalizationCategory).OrderBy(f => f.Name).ToList();
                    break;
                case PersonalizationItemsSortType.AlphabetReverse:
                    result = GetItems(personalizationCategory).OrderBy(f => f.Name).Reverse().ToList();
                    break;
                case PersonalizationItemsSortType.Exclusivity:
                    result = GetItems(personalizationCategory).OrderBy(f => f.Name).OrderBy(f => !f.IsExclusive()).ToList();
                    break;
                case PersonalizationItemsSortType.ExclusivityReverse:
                    result = GetItems(personalizationCategory).OrderBy(f => f.Name).OrderBy(f => f.IsExclusive()).ToList();
                    break;
                default:
                    result = list;
                    break;
            }
            return result;
        }
    }
}
