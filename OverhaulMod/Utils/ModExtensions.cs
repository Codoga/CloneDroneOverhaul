using OverhaulMod.Combat.Weapons;
using OverhaulMod.Engine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OverhaulMod.Utils
{
    internal static class ModExtensions
    {
        public static bool IsNullOrEmpty(this ICollection list) => list == null || list.Count == 0;
        public static bool IsNullOrEmpty(this System.Array array) => array == null || array.Length == 0;
        public static bool IsNullOrEmpty(this string @string) => string.IsNullOrEmpty(@string);
        public static bool IsNullOrWhiteSpace(this string @string) => string.IsNullOrWhiteSpace(@string);

        public static List<T> CloneList<T>(this List<T> list)
        {
            return Clone(list);
        }

        public static T Clone<T>(this T obj) where T : class
        {
            if (obj == null)
                return null;

            System.Reflection.MethodInfo method = obj.GetType().GetMethod("MemberwiseClone", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (method != null)
                return (T)method.Invoke(obj, null);
            else
                return null;
        }

        public static bool IsModdedEnumValue(this System.Enum enumValue)
        {
            string enumName = enumValue.ToString();
            string[] enumNames = enumValue.GetType().GetEnumNames();
            foreach (string name in enumNames)
            {
                if (name == enumName)
                    return false;
            }
            return true;
        }

        public static T GetObject<T>(this ModdedObject moddedObject, int index) where T : Object
        {
            if (!moddedObject)
                return null;

            GameObject gameObject = moddedObject.objects[index] as GameObject;
            return typeof(T) == typeof(GameObject) ? gameObject as T : (gameObject?.GetComponent<T>());
        }

        public static Object GetObject(this ModdedObject moddedObject, System.Type type, int index)
        {
            if (!moddedObject)
                return null;

            GameObject gameObject = moddedObject.objects[index] as GameObject;
            return type == typeof(GameObject) ? gameObject : (Object)(gameObject?.GetComponent(type));
        }

        public static T GetObject<T>(this ModdedObject moddedObject, string name) where T : Object
        {
            if (!moddedObject)
                return null;

            GameObject gameObject = null;
            foreach (Object @object in moddedObject.objects)
            {
                if (@object.name == name)
                {
                    gameObject = @object as GameObject;
                    break;
                }
            }
            return typeof(T) == typeof(GameObject) ? gameObject as T : (gameObject?.GetComponent<T>());
        }

        public static Object GetObject(this ModdedObject moddedObject, System.Type type, string name)
        {
            if (!moddedObject)
                return null;

            GameObject gameObject = null;
            foreach (Object @object in moddedObject.objects)
            {
                if (@object.name == name)
                {
                    gameObject = @object as GameObject;
                    break;
                }
            }
            return type == typeof(GameObject) ? gameObject : (Object)(gameObject?.GetComponent(type));
        }

        public static void SetLocalTransform(this Transform transform, Vector3 localPosition, Vector3 localEulerAngles, Vector3 localScale)
        {
            transform.localPosition = localPosition;
            transform.localEulerAngles = localEulerAngles;
            transform.localScale = localScale;
        }

        public static void SetLocalTransform(this Transform transform, Vector3 localPosition, Vector3 localEulerAngles)
        {
            transform.localPosition = localPosition;
            transform.localEulerAngles = localEulerAngles;
        }

        public static void SetLocalTransform(this Transform transform, Vector3 localPosition)
        {
            transform.localPosition = localPosition;
        }

        public static void SetLocalTransform(this Transform transform, TransformInfo transformInfo)
        {
            transform.SetLocalTransform(transformInfo.Position, transformInfo.EulerAngles, transformInfo.LocalScale);
        }

        public static void RandomizeLocalTransform(this Transform transform, float min, float max, bool rPos, bool rRot, bool rLS)
        {
            transform.SetLocalTransform(transform.localPosition * (rPos ? Random.Range(min, max) : 1f), transform.localEulerAngles * (rRot ? Random.Range(min, max) : 1f), transform.localScale * (rLS ? Random.Range(min, max) : 1f));
        }

        public static Transform FindChildRecursive(this Transform transform, string name)
        {
            return TransformUtils.FindChildRecursive(transform, name);
        }

        public static List<ModWeaponModel> GetModWeaponModels(this FirstPersonMover firstPersonMover)
        {
            if (firstPersonMover && firstPersonMover.IsAttachedAndAlive())
            {
                CharacterModel characterModel = firstPersonMover.GetCharacterModel();
                if (characterModel && characterModel.WeaponModels != null)
                {
                    List<ModWeaponModel> list = new List<ModWeaponModel>();
                    foreach (WeaponModel weapon in characterModel.WeaponModels)
                    {
                        if (weapon is ModWeaponModel)
                        {
                            list.Add((ModWeaponModel)weapon);
                        }
                    }
                    return list;
                }
            }
            return null;
        }

        public static void RefreshModWeaponModels(this FirstPersonMover firstPersonMover)
        {
            List<ModWeaponModel> list = firstPersonMover.GetModWeaponModels();
            if (list == null)
                return;

            foreach (ModWeaponModel weapon in list)
            {
                weapon.OnUpgradesRefresh(firstPersonMover);
            }
        }

        public static bool IsModded(this WeaponModel weaponModel)
        {
            return weaponModel && weaponModel is ModWeaponModel;
        }

        public static ModWeaponModel ModdedReference(this WeaponModel weaponModel)
        {
            return weaponModel as ModWeaponModel;
        }

        public static bool RevertUpgrade(this UpgradeManager upgradeManager, UpgradeDescription upgradeDescription)
        {
            if (!upgradeDescription || !upgradeManager)
                return false;

            UpgradeType upgradeToRevert = upgradeDescription.UpgradeType;
            int upgradeLevelToRevert = upgradeDescription.Level;

            Dictionary<UpgradeType, int> dictionary = GameDataManager.Instance.GetPlayerUpgradeDictionary();
            if (!upgradeDescription.CanBeReverted())
                return false;

            if (upgradeLevelToRevert > 1)
                GameDataManager.Instance.SetUpgradeLevel(upgradeToRevert, upgradeLevelToRevert - 1);
            else
                _ = dictionary.Remove(upgradeToRevert);

            upgradeManager.SetAvailableSkillPoints(upgradeManager.GetAvailableSkillPoints() + upgradeDescription.GetSkillPointCost());
            GlobalEventManager.Instance.Dispatch("UpgradeCompleted", upgradeDescription);
            return true;
        }

        public static List<UpgradeDescription> GetUpgradesRequiringUpgrade(this UpgradeManager upgradeManager, UpgradeDescription upgradeDescription)
        {
            List<UpgradeDescription> result = new List<UpgradeDescription>();
            if (!upgradeDescription || !upgradeManager)
                return result;

            foreach (UpgradeDescription description in upgradeManager.UpgradeDescriptions)
            {
                if (!description || description.IsTheSameUpgrade(upgradeDescription))
                    continue;

                if (description.Requirement == upgradeDescription || description.Requirement2 == upgradeDescription)
                    result.Add(description);
            }
            return result;
        }

        public static bool IsUpgraded(this UpgradeDescription upgradeDescription)
        {
            if (!CharacterTracker.Instance)
                return false;

            FirstPersonMover firstPersonMover = CharacterTracker.Instance.GetPlayerRobot();
            if (!firstPersonMover)
                return false;

            List<UpgradeTypeAndLevel> upgrades = firstPersonMover.GetUpgradeTypesAndLevels();
            if (upgrades.IsNullOrEmpty())
                return false;

            foreach (UpgradeTypeAndLevel upgradeTypeAndLevel in upgrades)
            {
                if (upgradeTypeAndLevel.IsTheSameUpgrade(upgradeDescription))
                    return true;
            }
            return false;
        }

        public static bool CanBeReverted(this UpgradeDescription upgradeDescription)
        {
            if (!CharacterTracker.Instance)
                return false;

            FirstPersonMover firstPersonMover = CharacterTracker.Instance.GetPlayerRobot();
            if (!firstPersonMover)
                return false;

            Dictionary<UpgradeType, int> dictionary = GameDataManager.Instance.GetPlayerUpgradeDictionary();
            if (UpgradeModesManager.IsUnrevertableUpgrade(upgradeDescription.UpgradeType, upgradeDescription.Level) || dictionary.IsNullOrEmpty() || upgradeDescription.IsRepeatable || !dictionary.ContainsKey(upgradeDescription.UpgradeType))
                return false;

            int playerLevel = dictionary[upgradeDescription.UpgradeType];
            if (playerLevel != upgradeDescription.Level)
                return false;

            List<UpgradeDescription> otherUpgrades = UpgradeManager.Instance.GetUpgradesRequiringUpgrade(upgradeDescription);
            if (!otherUpgrades.IsNullOrEmpty())
            {
                foreach (UpgradeDescription upgrade in otherUpgrades)
                {
                    if (upgrade.IsUpgraded())
                        return false;
                }
            }
            return true;
        }

        public static bool IsTheSameUpgrade(this UpgradeTypeAndLevel upgradeTypeAndLevel, UpgradeDescription other) => (upgradeTypeAndLevel.UpgradeType, upgradeTypeAndLevel.Level) == (other.UpgradeType, other.Level);
        public static bool IsTheSameUpgrade(this UpgradeDescription upgradeDescription, UpgradeDescription other) => (upgradeDescription.UpgradeType, upgradeDescription.Level) == (other.UpgradeType, other.Level);

        public static void SetUIOverLogoModeEnabled(this GameUIRoot gameUIRoot, bool enabled)
        {
            if (!gameUIRoot)
                return;

            Canvas canvas = gameUIRoot.GetComponent<Canvas>();
            if (canvas)
            {
                Camera camera = canvas.worldCamera;
                if (camera)
                {
                    camera.depth = enabled ? 100f : 1f;
                }
            }

            TitleScreenUI titleScreenUI = gameUIRoot.TitleScreenUI;
            if (titleScreenUI)
            {
                Transform transform = titleScreenUI.transform.FindChildRecursive("LeftFadeBG");
                if (transform)
                {
                    transform.gameObject.SetActive(!enabled);
                }
            }
        }

        public static string GetLevelSource(this LevelDescription levelDescription)
        {
            return levelDescription.PrefabName.IsNullOrEmpty() ? levelDescription.LevelJSONPath : levelDescription.PrefabName;
        }

        public static string GetTierString(this DifficultyTier difficultyTier)
        {
            switch (difficultyTier)
            {
                case (DifficultyTier)9:
                    return "Nightmarium";
            }
            return difficultyTier.ToString();
        }

        public static bool HasUpgrade(this UpgradeManager upgradeManager, UpgradeType upgradeType, int level)
        {
            foreach (UpgradeDescription ud in upgradeManager.UpgradeDescriptions)
                if (ud && ud.UpgradeType == upgradeType && ud.Level == level)
                    return true;

            return false;
        }

        public static UpgradeUIIcon GetUpgradeUIIcon(this UpgradeUI upgradeUI, UpgradeType upgradeType, int upgradeLevel)
        {
            List<UpgradeUIIcon> list = upgradeUI._icons;
            if (list.IsNullOrEmpty())
                return null;

            foreach (UpgradeUIIcon icon in list)
            {
                if (!icon)
                    continue;

                UpgradeDescription upgradeDescription = icon._upgradeDescription;
                if (!upgradeDescription)
                    continue;

                if (upgradeDescription.UpgradeType == upgradeType && upgradeDescription.Level == upgradeLevel)
                    return icon;
            }
            return null;
        }
    }
}
