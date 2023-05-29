﻿using CDOverhaul.HUD;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace CDOverhaul
{
    public static class SettingsController
    {
        /// <summary>
        /// This event is sent if any setting value has changed
        /// </summary>
        public const string SettingChangedEventString = "OnSettingChanged";

        /// <summary>
        /// If the default value of a setting equals this one, the setting will be shown as button in UI
        /// </summary>
        public const long SettingEventDispatcherFlag = 10000000000000L;

        /// <summary>
        /// All existing settings
        /// </summary>
        private static readonly List<SettingInfo> m_Settings = new List<SettingInfo>();
        private static readonly Dictionary<string, SettingDescription> m_SettingDescriptions = new Dictionary<string, SettingDescription>();

        /// <summary>
        /// Categories, sections and settings to be hidden in settings menu
        /// </summary>
        private static readonly List<string> m_HiddenEntries = new List<string>() { "Player", "WeaponSkins" };

        /// <summary>
        /// Loaded images from Assets/Settings/Ico directory
        /// </summary>
        private static readonly Dictionary<string, Sprite> m_CachedCategoryIcons = new Dictionary<string, Sprite>();
        private static Sprite m_UnknownCategoryIcon;

        /// <summary>
        /// UI instance
        /// </summary>
        public static OverhaulParametersMenu HUD;

        internal static void Initialize()
        {
            if (!OverhaulSessionController.GetKey<bool>("HasAddedOverhaulSettings"))
            {
                OverhaulSessionController.SetKey("HasAddedOverhaulSettings", true);

                List<OverhaulSettingAttribute> toParent = new List<OverhaulSettingAttribute>();
                foreach (System.Type type in Assembly.GetExecutingAssembly().GetTypes())
                {
                    foreach (FieldInfo field in type.GetFields(BindingFlags.Static | BindingFlags.Public))
                    {
                        OverhaulSettingAttribute neededAttribute = field.GetCustomAttribute<OverhaulSettingAttribute>();
                        if (neededAttribute != null)
                        {
                            SettingInforms informs = field.GetCustomAttribute<SettingInforms>();
                            SettingFormelyKnownAs formelyKnown = field.GetCustomAttribute<SettingFormelyKnownAs>();
                            SettingSliderParameters sliderParams = field.GetCustomAttribute<SettingSliderParameters>();
                            SettingDropdownParameters dropdownParams = field.GetCustomAttribute<SettingDropdownParameters>();
                            SettingForceInputField inputField = field.GetCustomAttribute<SettingForceInputField>();
                            SettingInfo info = AddSetting(neededAttribute.SettingRawPath, neededAttribute.DefaultValue, field, sliderParams, dropdownParams, formelyKnown);
                            info.ForceInputField = inputField != null;
                            info.SendMessageOfType = informs != null ? informs.Type : (byte)0;
                            if (info.DefaultValue is long && (long)info.DefaultValue == SettingEventDispatcherFlag)
                            {
                                info.EventDispatcher = (SettingEventDispatcher)field.GetValue(null);
                            }
                            if (neededAttribute.IsHidden)
                            {
                                m_HiddenEntries.Add(info.RawPath);
                            }
                            if (!string.IsNullOrEmpty(neededAttribute.Description))
                            {
                                AddDescription(neededAttribute.SettingRawPath, neededAttribute.Description, neededAttribute.Img4_3Path, neededAttribute.Img16_9Path);
                            }
                            if (!string.IsNullOrEmpty(neededAttribute.ParentSettingRawPath))
                            {
                                toParent.Add(neededAttribute);
                            }
                        }
                    }
                }

                SetSettingDependency("Gameplay.Camera.View mode", "Gameplay.Camera.Sync camera with head rotation", 1);

                DelegateScheduler.Instance.Schedule(delegate
                {
                    foreach (OverhaulSettingAttribute neededAttribute in toParent)
                    {
                        SetSettingParent(neededAttribute.SettingRawPath, neededAttribute.ParentSettingRawPath);
                    }
                }, 0.1f);

#if DEBUG
                DelegateScheduler.Instance.Schedule(delegate
                {
                    foreach (SettingInfo neededAttribute in m_Settings)
                    {
                        if (OverhaulLocalizationController.Error)
                        {
                            return;
                        }

                        if (!OverhaulLocalizationController.HasTranslation(OverhaulParametersMenu.SettingTranslationPrefix + neededAttribute.Name))
                        {
                            OverhaulLocalizationController.Localization.AddTranslation(OverhaulParametersMenu.SettingTranslationPrefix + neededAttribute.Name);
                            OverhaulLocalizationController.Localization.Translations["en"][OverhaulParametersMenu.SettingTranslationPrefix + neededAttribute.Name] = neededAttribute.Name;
                        }
                        if (!OverhaulLocalizationController.HasTranslation(OverhaulParametersMenu.SettingDescTranslationPrefix + neededAttribute.Name))
                        {
                            OverhaulLocalizationController.Localization.AddTranslation(OverhaulParametersMenu.SettingDescTranslationPrefix + neededAttribute.Name);
                        }
                        SettingDescription desc = SettingsController.GetSettingDescription(neededAttribute.RawPath);
                        if (desc != null) OverhaulLocalizationController.Localization.Translations["en"][OverhaulParametersMenu.SettingDescTranslationPrefix + neededAttribute.Name] = desc.Description;

                        if (!OverhaulLocalizationController.HasTranslation(OverhaulParametersMenu.SectionTranslationPrefix + neededAttribute.Section))
                        {
                            OverhaulLocalizationController.Localization.AddTranslation(OverhaulParametersMenu.SectionTranslationPrefix + neededAttribute.Section);
                            OverhaulLocalizationController.Localization.Translations["en"][OverhaulParametersMenu.SectionTranslationPrefix + neededAttribute.Section] = neededAttribute.Section;
                        }
                        if (!OverhaulLocalizationController.HasTranslation(OverhaulParametersMenu.SettingButtonTranslationPrefix + neededAttribute.Name))
                        {
                            OverhaulLocalizationController.Localization.AddTranslation(OverhaulParametersMenu.SettingButtonTranslationPrefix + neededAttribute.Name);
                            OverhaulLocalizationController.Localization.Translations["en"][OverhaulParametersMenu.SettingButtonTranslationPrefix + neededAttribute.Name] = neededAttribute.Name;
                        }

                        if (!OverhaulLocalizationController.HasTranslation(OverhaulParametersMenu.CategoryTranslationPrefix + neededAttribute.Category))
                        {
                            OverhaulLocalizationController.Localization.AddTranslation(OverhaulParametersMenu.CategoryTranslationPrefix + neededAttribute.Category);
                            OverhaulLocalizationController.Localization.Translations["en"][OverhaulParametersMenu.CategoryTranslationPrefix + neededAttribute.Category] = neededAttribute.Category;
                        }
                        if (!OverhaulLocalizationController.HasTranslation(OverhaulParametersMenu.CategoryDescTranslationPrefix + neededAttribute.Category))
                        {
                            OverhaulLocalizationController.Localization.AddTranslation(OverhaulParametersMenu.CategoryDescTranslationPrefix + neededAttribute.Category);
                            OverhaulLocalizationController.Localization.Translations["en"][OverhaulParametersMenu.CategoryDescTranslationPrefix + neededAttribute.Category] = GetCategoryDescription(neededAttribute.Category);
                        }
                    }
                }, 1f);
#endif
            }
            DelegateScheduler.Instance.Schedule(SettingInfo.DispatchSettingsRefreshedEvent, 0.1f);
        }

        internal static void CreateHUD()
        {
            OverhaulCanvasController h = OverhaulMod.Core.CanvasController;
            HUD = h.AddHUD<OverhaulParametersMenu>(h.HUDModdedObject.GetObject<ModdedObject>(3));
        }

        /// <summary>
        /// Add a setting and get full info about one
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="defaultValue"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        public static SettingInfo AddSetting<T>(in string path, in T defaultValue, in FieldInfo field, in SettingSliderParameters sliderParams = null, in SettingDropdownParameters dropdownParams = null, in SettingFormelyKnownAs formelyKnown = null)
        {
            SettingInfo newSetting = new SettingInfo();
            newSetting.SetUp<T>(path, defaultValue, field, sliderParams, dropdownParams, formelyKnown);
            m_Settings.Add(newSetting);
            return newSetting;
        }

        public static void AddDescription(in string settingPath, in string description, in string img43filename, in string img169filename)
        {
            if (GetSetting(settingPath) == null)
            {
                return;
            }
            SettingDescription desc = new SettingDescription(description, img43filename, img169filename);
            if (!m_SettingDescriptions.ContainsKey(settingPath))
            {
                m_SettingDescriptions.Add(settingPath, desc);
            }
        }

        public static void SetSettingDependency(in string toDepend, in string targetSetting, in object targetValue)
        {
            SettingInfo info = GetSetting(targetSetting);
            SettingInfo info2 = GetSetting(toDepend);
            if (info == null || info2 == null)
            {
                return;
            }

            info.CanBeLockedBy = info2;
            info.ValueToUnlock = targetValue;
        }

        public static void SetSettingParent(in string settingPath, in string targetSettingPath)
        {
            SettingInfo s1 = GetSetting(settingPath, true);
            SettingInfo s2 = GetSetting(targetSettingPath, true);
            s2.ParentSettingToThis(s1);
            SetSettingDependency(targetSettingPath, settingPath, true);
        }

        public static Sprite GetSpriteForCategory(in string categoryName)
        {
            string path = OverhaulMod.Core.ModDirectory + "Assets/Settings/Ico/" + categoryName + "-S-16x16.png";
            bool exists = File.Exists(path);
            if (!exists)
            {
                path = OverhaulMod.Core.ModDirectory + "Assets/Settings/Ico/UnknownCategory.png";
                if (!File.Exists(path))
                {
                    return null;
                }
                else
                {
                    if (m_UnknownCategoryIcon != null)
                    {
                        return m_UnknownCategoryIcon;
                    }

                    Texture2D texture1 = new Texture2D(1, 1)
                    {
                        filterMode = FilterMode.Point
                    };
                    texture1.LoadImage(File.ReadAllBytes(path), false);
                    texture1.Apply();

                    m_UnknownCategoryIcon = texture1.FastSpriteCreate();
                    return m_UnknownCategoryIcon;
                }
            }

            if (m_CachedCategoryIcons.ContainsKey(categoryName))
            {
                return m_CachedCategoryIcons[categoryName];
            }

            Texture2D texture = new Texture2D(1, 1);
            if (File.Exists(path))
            {
                byte[] content = File.ReadAllBytes(path);
                if (!content.IsNullOrEmpty())
                {
                    texture.filterMode = FilterMode.Point;
                    texture.LoadImage(content, false);
                    texture.Apply();

                    Sprite sprite = texture.FastSpriteCreate();
                    m_CachedCategoryIcons.Add(categoryName, sprite);
                    return sprite;
                }
            }
            return null;
        }

        /// <summary>
        /// Get all available categories including hidden if <paramref name="includeHidden"/> is true
        /// </summary>
        /// <returns></returns>
        public static List<string> GetAllCategories(in bool includeHidden = false)
        {
            List<string> result = new List<string>();
            foreach (SettingInfo s in m_Settings)
            {
                if (!result.Contains(s.Category))
                {
                    if (!IsEntryHidden(s.Category) || includeHidden)
                    {
                        result.Add(s.Category);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Get all available sections under category including hidden if <paramref name="includeHidden"/> is true
        /// </summary>
        /// <returns></returns>
        public static List<string> GetAllSections(in string categoryToSearchIn, in bool includeHidden = false)
        {
            List<string> result = new List<string>();
            foreach (SettingInfo s in m_Settings)
            {
                if (s.Category == categoryToSearchIn && !result.Contains(s.Category + "." + s.Section))
                {
                    if (!IsEntryHidden(s.Category + "." + s.Section) || includeHidden)
                    {
                        result.Add(s.Category + "." + s.Section);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Get all available settings including hidden if <paramref name="includeHidden"/> is true
        /// </summary>
        /// <returns></returns>
        public static List<string> GetAllSettings(in string categoryToSearchIn, in string sectionToSearchIn, in bool includeHidden = false)
        {
            List<string> result = new List<string>();
            foreach (SettingInfo s in m_Settings)
            {
                if (s.Category == categoryToSearchIn && s.Section == sectionToSearchIn && !result.Contains(s.RawPath))
                {
                    if ((!IsEntryHidden(s.RawPath) || includeHidden) && !s.IsChildSetting)
                    {
                        result.Add(s.RawPath);
                    }
                }
            }
            return result;
        }

        public static List<string> GetChildrenSettings(in string rawPath)
        {
            return GetSetting(rawPath, true).ChildSettings;
        }

        /// <summary>
        /// Get setting info by typing path
        /// </summary>
        /// <param name="path"></param>
        /// <param name="includeHidden"></param>
        /// <returns></returns>
        public static SettingInfo GetSetting(in string path, in bool includeHidden = false)
        {
            foreach (SettingInfo s in m_Settings)
            {
                if (s.RawPath == path && (!IsEntryHidden(s.RawPath) || includeHidden))
                {
                    return s;
                }
            }
            return null;
        }

        public static SettingDescription GetSettingDescription(in string path)
        {
            _ = m_SettingDescriptions.TryGetValue(path, out SettingDescription result);
            return result;
        }

        public static string GetCategoryDescription(string category)
        {
            switch (category)
            {
                case "Graphics":
                    return "Change the visuals of the game or the way meshes are rendered by\nSome of the settings can reduce your FPS!";
                case "Optimization":
                    return "Reduce memory usage";
                case "Game interface":
                    return "Bring some new things to the game's HUD";
                case "Gameplay":
                    return "Customize the gameplay experience for yourself";
                case "Shortcuts":
                    return "Open menus during the game, being in the settings";
            }
            return string.Empty;
        }

        /// <summary>
        /// Check if the setting is hidden
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsEntryHidden(in string path)
        {
            if (OverhaulVersion.IsDebugBuild) return false;

            bool isCategory = !path.Contains(".");
            string path1 = null;

            if (isCategory)
            {
                path1 = path;
                return m_HiddenEntries.Contains(path1);
            }

            string[] array = path.Split('.');

            bool isSection = array.Length == 2;
            if (isSection)
            {
                path1 = array[0] + "." + array[1];
            }

            bool isSetting = array.Length == 3;
            if (isSetting)
            {
                path1 = array[0] + "." + array[1] + "." + array[2];
            }
            return m_HiddenEntries.Contains(path1);
        }
    }
}
