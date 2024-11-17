using OverhaulMod.Content;
using OverhaulMod.Utils;
using System.Collections.Generic;
using System.IO;

namespace OverhaulMod
{
    public static class ModFeatures
    {
        public const string FEATURES_DATA_FILE_NAME = "featureStates.json";

        private static ModFeaturesData s_data;

        public static void Load()
        {
            string path = Path.Combine(ModCore.dataFolder, FEATURES_DATA_FILE_NAME);

            ModFeaturesData data;
            try
            {
                data = ModJsonUtils.DeserializeStream<ModFeaturesData>(path);
            }
            catch
            {
                data = new ModFeaturesData();
            }
            data.FixValues();

            bool anyChanges = false;
            foreach (object enumName in typeof(FeatureType).GetEnumValues())
            {
                FeatureType featureType = (FeatureType)enumName;
                if (!data.FeatureStates.ContainsKey(featureType))
                {
                    data.FeatureStates.Add(featureType, IsEnabled(featureType, false));
                    anyChanges = true;
                }
            }

            s_data = data;

            if (anyChanges)
                Save();
        }

        public static void Save()
        {
            if (s_data == null)
                return;

            ModJsonUtils.WriteStream(Path.Combine(ModCore.dataFolder, FEATURES_DATA_FILE_NAME), s_data);
        }

        /// <summary>
        /// Reset feature states
        /// </summary>
        /// <returns>False if feature states already were default</returns>
        public static bool Default()
        {
            bool anyChanges = false;

            Dictionary<FeatureType, bool> d = s_data.FeatureStates;
            foreach (object enumName in typeof(FeatureType).GetEnumValues())
            {
                FeatureType featureType = (FeatureType)enumName;
                bool state = IsEnabled(featureType, false);

                if (!d.ContainsKey(featureType))
                {
                    d.Add(featureType, state);
                    anyChanges = true;
                }
                else if (d[featureType] != state)
                {
                    d[featureType] = state;
                    anyChanges = true;
                }
            }

            return anyChanges;
        }

        public static ModFeaturesData GetData()
        {
            return s_data;
        }

        public static bool IsEnabled(FeatureType feature, bool useCaching = true)
        {
            if (useCaching)
            {
                Dictionary<FeatureType, bool> d = s_data?.FeatureStates;
                if (d != null && d.ContainsKey(feature))
                    return d[feature];
            }

            bool result;
            switch (feature)
            {
                case FeatureType.EndlessModeMenu:
                    result = true;
                    break;
                case FeatureType.NewWeapons:
                    result = true;
                    break;
                case FeatureType.WeaponBag:
                    result = false;
                    break;
                case FeatureType.ChapterSelectMenuRework:
                    result = true;
                    break;
                case FeatureType.LoadingScreenRework:
                    result = true;
                    break;
                case FeatureType.PauseMenuRework:
                    result = true;
                    break;
                case FeatureType.TitleScreenRework:
                    result = true;
                    break;
                case FeatureType.ArrowModelRefresh:
                    result = true;
                    break;
                case FeatureType.WeatherSystem:
                    result = true;
                    break;
                case FeatureType.WorkshopBrowserRework:
                    result = true;
                    break;
                case FeatureType.NightmariumDifficultyTier:
                    result = ModBuildInfo.enableV5;
                    break;
                case FeatureType.GameModeSelectionScreensRework:
                    result = true;
                    break;
                case FeatureType.NewGameplayContent:
                    result = ModBuildInfo.enableV5;
                    break;
                case FeatureType.NewEnemies:
                    result = ModBuildInfo.enableV5;
                    break;
                case FeatureType.DuelInviteMenuRework:
                    result = true;
                    break;
                case FeatureType.Tooltips:
                    result = false;
                    break;
                case FeatureType.AccessoriesAndPets:
                    result = false;
                    break;
                case FeatureType.AdvancedSettings:
                    result = true;
                    break;
                case FeatureType.WorkshopBrowserContextMenu:
                    result = false;
                    break;
                case FeatureType.StoryModeModifiers:
                    result = false;
                    break;
                case FeatureType.QuickReset:
                    result = false;
                    break;
                case FeatureType.WinLoseDialogRework:
                    result = false;
                    break;
                case FeatureType.ShieldSkins:
                    result = false;
                    break;
                case FeatureType.PersonalizationEditorTutorialVideo:
                    result = true;
                    break;
                case FeatureType.ScytheSkins:
                    result = true;
                    break;
                case FeatureType.MoreImageEffects:
                    result = ModBuildInfo.VERSION_4_3;
                    break;
                case FeatureType.SubtitleTextFieldRework:
                    result = true;
                    break;
                case FeatureType.UpdatesMenuRework:
                    result = false;
                    break;
                case FeatureType.RequireNormalAndFireVariantsForSwordAndSpearSkins:
                    result = false;
                    break;
                case FeatureType.DisplayDisconnectedPlayers:
                    result = true;
                    break;
                case FeatureType.Intro:
                    result = ModBuildInfo.VERSION_4_3;
                    break;
                case FeatureType.VersionLabelUpdates:
                    result = ModBuildInfo.VERSION_4_3;
                    break;
                case FeatureType.TitleScreenModdedSectionRework:
                    result = ModBuildInfo.VERSION_4_3;
                    break;
                case FeatureType.SettingDescriptionBox:
                    result = ModBuildInfo.VERSION_4_3;
                    break;
                case FeatureType.ColorBlindnessOptions:
                    result = true;
                    break;
                case FeatureType.ShowUpgradeUIExitButtonInLBS:
                    result = false;
                    break;
                case FeatureType.MoreSettingsMod_ColorBlindnessOptions:
                    result = false;
                    break;
                case FeatureType.ReflectionProbe:
                    result = false;
                    break;
                default:
                    return false;
            }

            if (!result)
            {
                ExclusiveContentManager modExclusiveContentManager = ExclusiveContentManager.Instance;
                if (modExclusiveContentManager && modExclusiveContentManager.IsFeatureUnlocked(feature))
                    result = true;
            }
            return result;
        }

        public enum FeatureType
        {
            EndlessModeMenu,

            ChapterSelectMenuRework,

            LoadingScreenRework,

            PauseMenuRework,

            TitleScreenRework,

            ArrowModelRefresh,

            NewWeapons,

            WeaponBag,

            WeatherSystem,

            WorkshopBrowserRework,

            NightmariumDifficultyTier,

            GameModeSelectionScreensRework,

            NewGameplayContent,

            NewEnemies,

            DuelInviteMenuRework,

            Tooltips,

            AccessoriesAndPets,

            AdvancedSettings,

            WinLoseDialogRework,

            WorkshopBrowserContextMenu,

            StoryModeModifiers,

            QuickReset,

            ShieldSkins,

            PersonalizationEditorTutorialVideo,

            ScytheSkins,

            MoreImageEffects,

            SubtitleTextFieldRework,

            UpdatesMenuRework,

            RequireNormalAndFireVariantsForSwordAndSpearSkins,

            DisplayDisconnectedPlayers,
            ShowUpgradeUIExitButtonInLBS,

            Intro,

            VersionLabelUpdates,

            TitleScreenModdedSectionRework,

            SettingDescriptionBox,

            ColorBlindnessOptions,

            MoreSettingsMod_ColorBlindnessOptions,

            ReflectionProbe
        }
    }
}
