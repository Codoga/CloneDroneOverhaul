using OverhaulMod.Content;
using System.Collections.Generic;

namespace OverhaulMod
{
    public static class ModFeatures
    {
        private static readonly Dictionary<FeatureType, bool> s_cachedValues = new Dictionary<FeatureType, bool>();

        public static void CacheValues()
        {
            Dictionary<FeatureType, bool> dictionary = s_cachedValues;
            dictionary.Clear();
            foreach (object enumName in typeof(FeatureType).GetEnumValues())
            {
                FeatureType featureType = (FeatureType)enumName;
                dictionary.Add(featureType, IsEnabled(featureType, false));
            }
        }

        public static bool IsEnabled(FeatureType feature, bool useCaching = true)
        {
            if (useCaching && s_cachedValues.ContainsKey(feature))
                return s_cachedValues[feature];

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
                    result = false;
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
                case FeatureType.JetpackAndDashToggle:
                    result = ModBuildInfo.enableV5;
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

            JetpackAndDashToggle
        }
    }
}
