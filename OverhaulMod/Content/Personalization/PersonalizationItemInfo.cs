using OverhaulMod.Utils;
using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace OverhaulMod.Content.Personalization
{
    public class PersonalizationItemInfo
    {
        public string Name, Description;

        public bool IsSentForVerification, IsVerified, ReuploadedTheItem;

        public string ItemID, EditorID;

        public List<string> Authors;

        public List<PersonalizationItemLockInfo> ExclusiveFor_V2;

        public PersonalizationCategory Category;

        public WeaponType Weapon;

        public bool HideBowStrings;

        public float BowStringsWidth = 1f;

        public string OverrideParent;

        public string BodyPartName;

        public int Version;

        public PersonalizationEditorObjectInfo RootObject;

        [NonSerialized]
        public bool Corrupted;

        [NonSerialized]
        public string RootFolderName;

        [NonSerialized]
        public string RootFolderPath;

        [NonSerialized]
        public string FolderPath;

        [NonSerialized]
        public List<string> ImportedFiles;

        [NonSerialized]
        public bool IsPersistentAsset;

        [NonSerialized]
        public PersonalizationItemMetaData MetaData;

        [NonSerialized]
        public bool HideInBrowser;

        public void GetImportedFiles()
        {
            List<string> list = new List<string>();
            foreach (string f in Directory.GetFiles(GetImportedFilesFolder(this)))
                list.Add(Path.GetFileName(f));

            ImportedFiles = list;
        }

        public void FixValues()
        {
            if (Authors == null)
                Authors = new List<string>();

            if (ExclusiveFor_V2 == null)
                ExclusiveFor_V2 = new List<PersonalizationItemLockInfo>();

            if (RootObject == null)
            {
                RootObject = new PersonalizationEditorObjectInfo()
                {
                    Name = "Root",
                    Path = "Empty",
                    IsRoot = true,
                    Children = new List<PersonalizationEditorObjectInfo>(),
                    PropertyValues = new Dictionary<string, object>()
                };
            }

            RootObject.SetPosition(Vector3.zero);
            RootObject.SetEulerAngles(Vector3.zero);
            RootObject.SetScale(Vector3.one);

            if (!PersonalizationManager.IsWeaponCustomizationSupported(Weapon))
                Weapon = WeaponType.Sword;

            GetImportedFiles();
        }

        public void SetAuthor(string name)
        {
            Authors.Clear();
            Authors.Add(name);
        }

        public string GetAuthorsString(bool translate = false)
        {
            if (Authors.IsNullOrEmpty())
                return "N/A (no author)";

            string result = Authors[0];
            if (Authors.Count != 1)
            {
                for (int i = 1; i < Authors.Count; i++)
                    result += $"{((translate && i == Authors.Count - 1) ? $" {LocalizationManager.Instance.GetTranslatedString("customization_authors_and")}" : ",")} {Authors[i]}";
            }
            return result;
        }

        public bool IsExclusive()
        {
            return ExclusiveFor_V2 != null && ExclusiveFor_V2.Count != 0;
        }

        public bool CanBeEdited()
        {
            return EditorID.IsNullOrEmpty() || EditorID.Contains(SteamUser.GetSteamID().ToString());
        }

        public bool IsUnlocked(Character character)
        {
            if (!IsExclusive())
                return true;

            if (!character)
                return false;

            bool isSinglePlayer = GameModeManager.IsSinglePlayer();
            string playFabId = isSinglePlayer ? ModUserInfo.localPlayerPlayFabID : character.GetPlayFabID();

            bool result = false;
            List<PersonalizationItemLockInfo> exclusiveForList = ExclusiveFor_V2;
            foreach (PersonalizationItemLockInfo lockInfo in exclusiveForList)
            {
                if (lockInfo.PlayerPlayFabID == playFabId)
                {
                    result = true;
                    break;
                }
            }

            return result || (isSinglePlayer && (character.IsClone() || !character.IsMainPlayer()));
        }

        public bool IsUnlocked()
        {
            return IsUnlocked(CharacterTracker.Instance.GetPlayer());
        }

        public bool IsCompatibleWithMod()
        {
            return MetaData == null || PersonalizationItemMetaData.CurrentCustomizationSystemVersion >= MetaData.CustomizationSystemVersion;
        }

        public bool IsEquipped()
        {
            return PersonalizationManager.GetIsItemEquipped(this);
        }

        public string GetSpecialInfoString()
        {
            switch (Category)
            {
                case PersonalizationCategory.WeaponSkins:
                    switch (Weapon)
                    {
                        case Combat.ModWeaponsManager.SCYTHE_TYPE:
                            return "Scythe";
                    }
                    return Weapon.ToString();
                case PersonalizationCategory.Accessories:
                    return BodyPartName;
            }
            return string.Empty;
        }

        public static string GetFolderName(PersonalizationItemInfo personalizationItemInfo)
        {
            return ModIOUtils.GetDirectoryName(personalizationItemInfo.FolderPath);
        }

        public static string GetImportedFilesFolder(PersonalizationItemInfo personalizationItemInfo)
        {
            return Path.Combine(personalizationItemInfo.FolderPath, "files/");
        }

        public static string GetImportedFileFullPath(PersonalizationItemInfo personalizationItemInfo, string fileName)
        {
            return Path.Combine(GetImportedFilesFolder(personalizationItemInfo), fileName);
        }

        public static string GetCategoryString(PersonalizationCategory personalizationCategory, bool many = false)
        {
            switch (personalizationCategory)
            {
                case PersonalizationCategory.None:
                    return "None";
                case PersonalizationCategory.WeaponSkins:
                    return many ? "Weapon skins" : "Weapon skin";
                case PersonalizationCategory.Accessories:
                    return many ? "Accessories" : "Accessory";
                case PersonalizationCategory.Pets:
                    return many ? "Pets" : "Pet";
            }
            return personalizationCategory.ToString();
        }
    }
}
