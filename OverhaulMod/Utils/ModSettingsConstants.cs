using OverhaulMod.UI;

namespace OverhaulMod.Utils
{
    public static class ModSettingsConstants
    {
        public const string UI_PREFIX = "ModUI_";

        /// <summary>
        /// <see cref="Engine.FadingVoxelManager"/>
        /// </summary>
        public const string ENABLE_VOXEL_FIRE_FADING = "EnableVoxelFireFading";

        /// <summary>
        /// <see cref="Engine.FadingVoxelManager"/>
        /// </summary>
        public const string ENABLE_VOXEL_BURNING = "EnableVoxelBurning";

        /// <summary>
        /// <see cref="Engine.ArenaRemodelManager"/>
        /// </summary>
        public const string ENABLE_ARENA_REMODEL = "EnableArenaRemodel";

        /// <summary>
        /// <see cref="Engine.TitleScreenCustomizationManager"/>
        /// </summary>
        public const string TITLE_SCREEN_MUSIC_TRACK_INDEX = "TitleScreenMusicTrackIndex";

        /// <summary>
        /// <see cref="Engine.TitleScreenCustomizationManager"/>
        /// </summary>
        public const string TITLE_SCREEN_BACKGROUND_TYPE = "TitleScreenBackgroundType";

        /// <summary>
        /// <see cref="ModUIManager"/>
        /// </summary>
        public const string SHOW_CHAPTER_SELECTION_MENU_REWORK = UI_PREFIX + nameof(UIChapterSelectMenuRework);

        /// <summary>
        /// <see cref="ModUIManager"/>
        /// </summary>
        public const string SHOW_ENDLESS_MODE_MENU = UI_PREFIX + nameof(UIEndlessModeMenu);

        /// <summary>
        /// <see cref="ModUIManager"/>
        /// </summary>
        public const string SHOW_CHALLENGES_MENU_REWORK = UI_PREFIX + nameof(UIChallengesMenuRework);

        /// <summary>
        /// <see cref="ModUIManager"/>
        /// </summary>
        public const string SHOW_WORKSHOP_BROWSER_REWORK = UI_PREFIX + nameof(UIWorkshopBrowser);

        /// <summary>
        /// <see cref="ModUIManager"/>
        /// </summary>
        public const string SHOW_ADVANCEMENTS_MENU_REWORK = UI_PREFIX + nameof(UIAdvancementsMenu);

        /// <summary>
        /// <see cref="ModUIManager"/>
        /// </summary>
        public const string SHOW_SETTINGS_MENU_REWORK = UI_PREFIX + nameof(UISettingsMenuRework);

        /// <summary>
        /// <see cref="ModUIManager"/>
        /// </summary>
        public const string SHOW_TITLE_SCREEN_REWORK = UI_PREFIX + nameof(UITitleScreenRework);

        /// <summary>
        /// <see cref="ModUIManager"/>
        /// </summary>
        public const string SHOW_DUEL_INVITE_MENU_REWORK = UI_PREFIX + nameof(UIDuelInviteMenuRework);

        /// <summary>
        /// <see cref="Engine.ModAudioManager"/>
        /// </summary>
        public const string ENABLE_REVERB_FILTER = "SoundReverbFilterEnabled";

        /// <summary>
        /// <see cref="Engine.ModAudioManager"/>
        /// </summary>
        public const string REVERB_FILTER_INTENSITY = "SoundReverbFilterIntensity";

        /// <summary>
        /// <see cref="ModCore"/>
        /// </summary>
        public const string ENABLE_TITLE_BAR_OVERHAUL = "TitleBarOverhaul";

        /// <summary>
        /// <see cref="Engine.CameraManager"/>
        /// </summary>
        public const string ENABLE_FIRST_PERSON_MODE = "EnableFirstPersonMode";

        /// <summary>
        /// <see cref="Engine.CameraManager"/>
        /// </summary>
        public const string CAMERA_MODE_TOGGLE_KEYBIND = "CameraModeToggleKeybind";

        /// <summary>
        /// <see cref="Content.UpdateManager"/>
        /// </summary>
        public const string CHECK_FOR_UPDATES_ON_STARTUP = "CheckForUpdatesOnStartup";

        /// <summary>
        /// <see cref="Engine.TitleScreenCustomizationManager"/>
        /// </summary>
        public const string INTRODUCE_TITLE_SCREEN_CUSTOMIZATION = "IntroduceTitleScreenCustomization";

        /// <summary>
        /// <see cref="ModManagers"/>
        /// </summary>
        public const string SHOW_MOD_SETUP_SCREEN_ON_START = "ShowModSetupScreenOnStart";

        /// <summary>
        /// <see cref="UI.UIVersionLabel"/>
        /// </summary>
        public const string SHOW_VERSION_LABEL = "ShowModVersionLabel";

        /// <summary>
        /// <see cref="Visuals.PostEffectsManager"/>
        /// </summary>
        public const string ENABLE_SSAO = "EnableSSAO";

        /// <summary>
        /// <see cref="UI.UIImageEffects"/>
        /// </summary>
        public const string ENABLE_VIGNETTE = "EnableVignette";

        /// <summary>
        /// <see cref="UI.UIImageEffects"/>
        /// </summary>
        public const string ENABLE_DITHERING = "EnableDithering";

        /// <summary>
        /// <see cref="Visuals.ParticleManager"/>
        /// </summary>
        public const string ENABLE_NEW_PARTICLES = "EnableParticles";

        /// <summary>
        /// <see cref="Visuals.Environment.SeveredBodyPartSparks"/>
        /// </summary>
        public const string ENABLE_GARBAGE_PARTICLES = "EnableGarbageParticles";

        /// <summary>
        /// <see cref="Engine.CameraRollingController"/>
        /// </summary>
        public const string ENABLE_CAMERA_ROLLING = "EnableCameraRolling";

        /// <summary>
        /// <see cref="Engine.CameraRollingController"/>
        /// </summary>
        public const string ENABLE_CAMERA_BOBBING = "EnableCameraBobbing";

        /// <summary>
        /// <see cref="Visuals.PostEffectsManager"/>
        /// </summary>
        public const string ENABLE_BLOOM = "EnableBloom";

        /// <summary>
        /// <see cref="Engine.LightingTransitionManager"/>
        /// </summary>
        public const string ENABLE_LIGHTING_TRANSITION = "EnableLightingTransition";

        /// <summary>
        /// <see cref="Engine.AutoBuildManager"/>
        /// </summary>
        public const string AUTO_BUILD_KEY_BIND = "AutoBuildKeyBind";

        /// <summary>
        /// <see cref="Engine.CameraFOVController"/>
        /// </summary>
        public const string CAMERA_FOV_OFFSET = "CameraFOVOffset";

        /// <summary>
        /// <see cref="Content.Personalization.PersonalizationController"/>
        /// </summary>
        public const string SWORD_SKIN = "SwordSkinV2";

        /// <summary>
        /// <see cref="Content.Personalization.PersonalizationController"/>
        /// </summary>
        public const string BOW_SKIN = "BowSkinV2";

        /// <summary>
        /// <see cref="Content.Personalization.PersonalizationController"/>
        /// </summary>
        public const string HAMMER_SKIN = "HammerSkinV2";

        /// <summary>
        /// <see cref="Content.Personalization.PersonalizationController"/>
        /// </summary>
        public const string SPEAR_SKIN = "SpearSkinV2";

        /// <summary>
        /// <see cref="Content.Personalization.PersonalizationController"/>
        /// </summary>
        public const string SHIELD_SKIN = "ShieldSkinV2";

        /// <summary>
        /// <see cref="Content.Personalization.PersonalizationController"/>
        /// </summary>
        public const string SCYTHE_SKIN = "ScytheSkin";

        /// <summary>
        /// <see cref="Visuals.RobotWeaponBag"/>
        /// </summary>
        public const string ENABLE_WEAPON_BAG = "EnableWeaponBag";

        /// <summary>
        /// <see cref="Patches.Behaviours.ColorsPatchBehaviour"/>
        /// </summary>
        public const string CHANGE_HIT_COLORS = "ChangeHitColors";

        /// <summary>
        /// <see cref="Visuals.Environment.WeatherManager"/>
        /// </summary>
        public const string ENABLE_WEATHER = "EnableWeather";

        /// <summary>
        /// <see cref="Visuals.Environment.WeatherManager"/>
        /// </summary>
        public const string FORCE_WEATHER_TYPE = "ForceWeatherType";

        /// <summary>
        /// <see cref="ModCore.ShowSpeakerName"/>
        /// </summary>
        public const string SHOW_SPEAKER_NAME = "ShowSpeakerName";

        /// <summary>
        /// <see cref="UI.UIFeedbackMenu"/>
        /// </summary>
        public const string HAS_EVER_SENT_FEEDBACK = "HasEverSentFeedback";

        /// <summary>
        /// <see cref="Visuals.Environment.FloatingDustManager"/>
        /// </summary>
        public const string ENABLE_FLOATING_DUST = "EnableFloatingDust";

        /// <summary>
        /// <see cref="Visuals.PostEffectsManager"/>
        /// </summary>
        public const string TWEAK_BLOOM = "TweakBloom";

        /// <summary>
        /// <see cref="Engine.TransitionManager"/>
        /// </summary>
        public const string OVERHAUL_SCENE_TRANSITIONS = "OverhaulSceneTransitions";

        /// <summary>
        /// <see cref="Engine.TransitionManager"/>
        /// </summary>
        public const string OVERHAUL_NON_SCENE_TRANSITIONS = "OverhaulNonSceneTransitions";

        /// <summary>
        /// <see cref="Engine.TransitionManager"/>
        /// </summary>
        public const string TRANSITION_ON_STARTUP = "TransitionOnStart";

        /// <summary>
        /// <see cref="Engine.AdvancedPhotoModeManager"/>
        /// </summary>
        public const string ADVANCED_PHOTO_MODE = "AdvancedPhotoMode";

        /// <summary>
        /// <see cref="Engine.RichPresenceManager"/>
        /// </summary>
        public const string ENABLE_RPC = "EnableRPC";

        /// <summary>
        /// <see cref="Engine.RichPresenceManager"/>
        /// </summary>
        public const string RPC_DETAILS = "RPCDetails";

        /// <summary>
        /// <see cref="Engine.RichPresenceManager"/>
        /// </summary>
        public const string RPC_DISPLAY_LEVEL_FILE_NAME = "RPCDisplayLevelFileName";

        /// <summary>
        /// <see cref="Patches.Behaviours.EnergyUIPatchBehaviour"/>
        /// </summary>
        public const string ENERGY_UI_REWORK = "EnergyUIRework";

        /// <summary>
        /// <see cref="EnergyUIBehaviour"/>
        /// </summary>
        public const string ENERGY_UI_FADE_OUT_IF_FULL = "EnergyUIFadeOutIfFull";

        /// <summary>
        /// <see cref="EnergyUIBehaviour"/>
        /// </summary>
        public const string ENERGY_UI_FADE_OUT_INTENSITY = "EnergyUIFadeOutIntensity";

        /// <summary>
        /// <see cref="Engine.AdvancedPhotoModeManager"/>
        /// </summary>
        public const string REQUIRE_RMB_HOLD_WHEN_UI_IS_HIDDEN = "RequireRMBHoldWhenUIIsHidden";

        /// <summary>
        /// <see cref="Engine.CameraFOVController"/>
        /// </summary>
        public const string ENABLE_FOV_OVERRIDE = "EnableFOVOverride";

        /// <summary>
        /// <see cref="Engine.AutoBuildManager"/>
        /// </summary>
        public const string AUTO_BUILD_ACTIVATION_ON_MATCH_START = "AutoBuildActivationOnMatchStart";

        /// <summary>
        /// <see cref="Content.Personalization.PersonalizationController"/>
        /// </summary>
        public const string ALLOW_ENEMIES_USE_WEAPON_SKINS = "AllowEnemiesUseWeaponSkins";

        /// <summary>
        /// <see cref="UI.UIPatchNotes"/>
        /// </summary>
        public const string LAST_BUILD_CHANGELOG_WAS_SHOWN = "LastBuildChangelogWasShownOn";

        /// <summary>
        /// <see cref="ModCore"/>
        /// </summary>
        public const string STREAMER_MODE = "StreamerMode";

        /// <summary>
        /// <see cref="UI.UIPersonalizationItemBrowser"/>
        /// </summary>
        public const string HAS_EVER_ROTATED_THE_CAMERA = "HasEverRotatedTheCamera";

        /// <summary>
        /// <see cref="UI.UIFeedbackMenu"/>
        /// </summary>
        public const string FEEDBACK_MENU_RATE = "FeedbackMenuRate";

        /// <summary>
        /// <see cref="UI.UIFeedbackMenu"/>
        /// </summary>
        public const string FEEDBACK_MENU_IMPROVE_TEXT = "FeedbackMenuImproveText";

        /// <summary>
        /// <see cref="UI.UIFeedbackMenu"/>
        /// </summary>
        public const string FEEDBACK_MENU_FAVORITE_TEXT = "FeedbackMenuFavoriteText";

        /// <summary>
        /// <see cref="Visuals.PostEffectsManager"/>
        /// </summary>
        public const string SSAO_SAMPLE_COUNT = "SSAOSampleCount";

        /// <summary>
        /// <see cref="Visuals.PostEffectsManager"/>
        /// </summary>
        public const string SSAO_INTENSITY = "SSAOIntensity";

        /// <summary>
        /// <see cref="Visuals.PostEffectsManager"/>
        /// </summary>
        public const string ENABLE_CHROMATIC_ABERRATION = "EnableCA";

        /// <summary>
        /// <see cref="Visuals.PostEffectsManager"/>
        /// </summary>
        public const string CHROMATIC_ABERRATION_INTENSITY = "CAIntensity";

        /// <summary>
        /// <see cref="Visuals.PostEffectsManager"/>
        /// </summary>
        public const string CHROMATIC_ABERRATION_ON_SCREEN_EDGES = "CAOnScreenEdges";

        /// <summary>
        /// <see cref="Visuals.PostEffectsManager"/>
        /// </summary>
        public const string COLOR_BLINDNESS_MODE = "CBMode";

        /// <summary>
        /// <see cref="Visuals.PostEffectsManager"/>
        /// </summary>
        public const string COLOR_BLINDNESS_AFFECT_UI = "CBAffectUI";

        /// <summary>
        /// <see cref="Visuals.PostEffectsManager"/>
        /// </summary>
        public const string ANTIALIASING_MODE = "AAMode";

        /// <summary>
        /// <see cref="Visuals.PostEffectsManager"/>
        /// </summary>
        public const string MSAA_PLUS_CUSTOM = "MSAAPlusCustom";

        /// <summary>
        /// <see cref="Visuals.PostEffectsManager"/>
        /// </summary>
        public const string ENABLE_DOF = "EnableDoF";

        /// <summary>
        /// <see cref="Visuals.PostEffectsManager"/>
        /// </summary>
        public const string ENABLE_SUN_SHAFTS = "EnableSunShafts";

        /// <summary>
        /// <see cref="UI.UISubtitleTextFieldRework"/>
        /// </summary>
        public const string ENABLE_SUBTITLE_TEXT_FIELD_REWORK = "EnableSubtitleTextFieldRework";

        /// <summary>
        /// <see cref="UI.UISubtitleTextFieldRework"/>
        /// </summary>
        public const string SUBTITLE_TEXT_FIELD_UPPER_POSITION = "SubtitleTextFieldUpperPosition";

        /// <summary>
        /// <see cref="UI.UISubtitleTextFieldRework"/>
        /// </summary>
        public const string SUBTITLE_TEXT_FIELD_BG = "SubtitleTextFieldBG";

        /// <summary>
        /// <see cref="UI.UISubtitleTextFieldRework"/>
        /// </summary>
        public const string SUBTITLE_TEXT_FIELD_FONT = "SubtitleTextFieldFont";

        /// <summary>
        /// <see cref="UI.UISubtitleTextFieldRework"/>
        /// </summary>
        public const string SUBTITLE_TEXT_FIELD_FONT_SIZE = "SubtitleTextFieldFontSize";

        /// <summary>
        /// <see cref="Engine.ScheduledActionsManager"/>
        /// </summary>
        public const string REFRESH_MOD_UPDATES_DATE_TIME = "RefreshModUpdatesDateTime";

        /// <summary>
        /// <see cref="Engine.ScheduledActionsManager"/>
        /// </summary>
        public const string REFRESH_EXCLUSIVE_PERKS_DATE_TIME = "RefreshExclusivePerksDateTime";

        /// <summary>
        /// <see cref="Engine.ScheduledActionsManager"/>
        /// </summary>
        public const string REFRESH_CUSTOMIZATION_ASSETS_REMOTE_VERSION_DATE_TIME = "RefreshCustomizationAssetsRemoteVersionDateTime";

        /// <summary>
        /// <see cref="Engine.ScheduledActionsManager"/>
        /// </summary>
        public const string REFRESH_NEWS_DATE_TIME = "RefreshNewsDateTime";

        /// <summary>
        /// <see cref="Content.UpdateManager"/>
        /// </summary>
        public const string SAVED_RELEASE_VERSION = "SavedReleaseVersion";

        /// <summary>
        /// <see cref="Content.UpdateManager"/>
        /// </summary>
        public const string SAVED_TESTING_VERSION = "SavedTestingVersion";

        /// <summary>
        /// <see cref="Content.NewsManager"/>
        /// </summary>
        public const string PREV_NEWS_COUNT = "PrevNewsCountV3";

        /// <summary>
        /// <see cref="Content.NewsManager"/>
        /// </summary>
        public const string DOWNLOADED_NEWS_COUNT = "DownloadedNewsCount";

        /// <summary>
        /// <see cref="Engine.AutoBuildManager"/>
        /// </summary>
        public const string AUTO_BUILD_INDEX_TO_USE_ON_MATCH_START = "AutoBuildIndexToUseOnMatchStart";

        /// <summary>
        /// <see cref="Engine.PressActionKeyObjectManager"/>
        /// </summary>
        public const string ENABLE_PRESS_BUTTON_TRIGGER_DESCRIPTION_REWORK = "EnablePressButtonTriggerDescriptionRework";

        /// <summary>
        /// <see cref="UI.UIPressActionKeyDescription"/>
        /// </summary>
        public const string PAK_DESCRIPTION_BG = "PAKBG";

        /// <summary>
        /// <see cref="UI.UIPressActionKeyDescription"/>
        /// </summary>
        public const string PAK_DESCRIPTION_FONT = "PAKFont";

        /// <summary>
        /// <see cref="UI.UIPressActionKeyDescription"/>
        /// </summary>
        public const string PAK_DESCRIPTION_FONT_SIZE = "PAKFontSize";
    }
}
