using OverhaulMod.Content.Personalization;
using OverhaulMod.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OverhaulMod.Engine
{
    public class TitleScreenCustomizationManager : Singleton<TitleScreenCustomizationManager>, IGameLoadListener
    {
        public const string CUSTOMIZATION_INFO_FILE = "TitleScreenCustomizationInfo.json";

        public const string CUSTOM_LEVEL_ID = "customTitleScreenLevel";

        [ModSetting(ModSettingsConstants.TITLE_SCREEN_MUSIC_TRACK_INDEX, 0)]
        public static int MusicTrackIndex;

        [ModSetting(ModSettingsConstants.INTRODUCE_TITLE_SCREEN_CUSTOMIZATION, true)]
        public static bool IntroduceCustomization;

        private TitleScreenCustomizationInfo m_customizationInfo;

        private GameObject m_levelIsLoadingBg;

        private float m_timeToRefreshMusicTrack;

        public LevelDescription overrideLevelDescription
        {
            get;
            set;
        }

        public bool disallowClickingLogo
        {
            get;
            set;
        }

        public override void Awake()
        {
            base.Awake();

            m_timeToRefreshMusicTrack = -1f;

            LoadCustomizationInfo();
        }

        private void Update()
        {
            if (m_timeToRefreshMusicTrack != -1f && Time.unscaledTime >= m_timeToRefreshMusicTrack)
            {
                m_timeToRefreshMusicTrack = -1f;
                RefreshMusicTrack();
            }
        }

        public void OnGameLoaded()
        {
            RefreshMusicTrack();

            GlobalEventManager.Instance.AddEventListener(GlobalEvents.LevelEditorStarted, StopTitleScreenMusic);
            GlobalEventManager.Instance.AddEventListener(PersonalizationEditorManager.EDITOR_STARTED_EVENT, StopTitleScreenMusic);

            Transform transform = ArenaCameraManager.Instance?.ArenaCameraTransform?.parent;
            if (transform)
                transform.SetParent(WorldRoot.Instance?.transform);
        }

        public void StopTitleScreenMusic()
        {
            ModGameUtils.FadeThenStopMusic(0.3f);
        }

        public void LoadCustomizationInfo()
        {
            TitleScreenCustomizationInfo titleScreenCustomizationInfo;
            try
            {
                titleScreenCustomizationInfo = ModDataManager.Instance.DeserializeFile<TitleScreenCustomizationInfo>(CUSTOMIZATION_INFO_FILE, false);
                titleScreenCustomizationInfo.FixValues();
            }
            catch
            {
                titleScreenCustomizationInfo = new TitleScreenCustomizationInfo();
                titleScreenCustomizationInfo.FixValues();
            }
            m_customizationInfo = titleScreenCustomizationInfo;

            overrideLevelDescription = titleScreenCustomizationInfo.StaticBackgroundInfo.Level;
        }

        public void SaveCustomizationInfo()
        {
            TitleScreenCustomizationInfo titleScreenCustomizationInfo = m_customizationInfo;
            if (titleScreenCustomizationInfo == null)
                return;

            ModDataManager.Instance.SerializeToFile(CUSTOMIZATION_INFO_FILE, titleScreenCustomizationInfo, false);
        }

        public TitleScreenBackgroundInfo GetStaticBackgroundInfo()
        {
            TitleScreenCustomizationInfo titleScreenCustomizationInfo = m_customizationInfo;
            if (titleScreenCustomizationInfo == null)
                return null;

            TitleScreenBackgroundInfo bgInfo = titleScreenCustomizationInfo.StaticBackgroundInfo;
            if (bgInfo == null)
            {
                bgInfo = new TitleScreenBackgroundInfo();
                titleScreenCustomizationInfo.StaticBackgroundInfo = bgInfo;
            }
            return bgInfo;
        }

        public void SetStaticLevel(LevelDescription level, bool refresh = true)
        {
            TitleScreenBackgroundInfo bgInfo = GetStaticBackgroundInfo();
            if (bgInfo == null)
                return;

            bgInfo.Level = level;

            if (refresh)
            {
                overrideLevelDescription = level;
                SpawnStaticBackground();
            }
        }

        public void SpawnStaticBackground()
        {
            TitleScreenCustomizationInfo titleScreenCustomizationInfo = m_customizationInfo;
            if (titleScreenCustomizationInfo == null)
                return;

            titleScreenCustomizationInfo.FixValues();
            _ = ModActionUtils.RunCoroutine(spawnStaticBackgroundCoroutine());
        }

        public void SetLevelIsLoadingBG(GameObject gameObject)
        {
            m_levelIsLoadingBg = gameObject;
        }

        public List<Dropdown.OptionData> GetMusicTracks()
        {
            List<string> stringList = new List<string>(AudioLibrary.Instance.GetMusicClipNames());
            _ = stringList.Remove("Chapter4TravelInTubesMusic");

            List<Dropdown.OptionData> list = DropdownStringOptionData.GetOptionsData(stringList);
            list[0].text = "None";
            foreach (Dropdown.OptionData od in list)
            {
                od.text = StringUtils.AddSpacesToCamelCasedString(od.text);
                if (od.text.Contains("_"))
                    od.text = od.text.Replace("_", string.Empty);
                if (od.text.Contains("C5"))
                    od.text = od.text.Replace("C5", "Chapter 5");
                if (od.text.Contains("Chapter5"))
                    od.text = od.text.Replace("Chapter5", "Chapter 5");
                if (od.text.Contains("Chapter4"))
                    od.text = od.text.Replace("Chapter4", "Chapter 4");
                if (od.text.Contains("Chapter3"))
                    od.text = od.text.Replace("Chapter3", "Chapter 3");
                if (od.text.Contains("Chapter2"))
                    od.text = od.text.Replace("Chapter2", "Chapter 2");
                if (od.text.Contains("Ghost"))
                    od.text = od.text.Replace("Ghost", "Emilia");
                if (od.text.Contains("Spidertron7000"))
                    od.text = od.text.Replace("Spidertron7000", "Spidertron 7000");
                if (od.text.Contains(" Music"))
                    od.text = od.text.Replace(" Music", string.Empty);
                if (od.text.Contains("Photo Clip"))
                    od.text = od.text.Replace("Photo Clip", "Photo Mode");
            }
            return list;
        }

        public void RefreshMusicTrackDelayed(float time)
        {
            m_timeToRefreshMusicTrack = Time.unscaledTime + time;
        }

        public void RefreshMusicTrack()
        {
            if (!GameModeManager.IsOnTitleScreen())
                return;

            CreditsCrawlAnimation creditsCrawlAnimation = ModCache.gameUIRoot.CreditsCrawl;
            if (creditsCrawlAnimation && creditsCrawlAnimation._isShowing)
                return;

            List<Dropdown.OptionData> list = GetMusicTracks();
            if (list.IsNullOrEmpty() || MusicTrackIndex < 0 || MusicTrackIndex >= list.Count)
                return;

            if (MusicTrackIndex == 0)
            {
                AudioManager.Instance.StopMusic();
                return;
            }

            AudioClipDefinition audioClip = AudioLibrary.Instance.GetAudioClip((list[MusicTrackIndex] as DropdownStringOptionData).StringValue);
            if (audioClip != null && audioClip.Clip)
            {
                AudioManager.Instance.PlayMusicClip(audioClip, true, true);
                AudioManager.Instance.FadeInMusic(2f);
            }
        }

        public Transform LoadLevel(string levelJSONPath, LevelManager levelManager)
        {
            LevelEditorLevelData levelEditorLevelData;
            try
            {
                levelEditorLevelData = ModJsonUtils.DeserializeStream<LevelEditorLevelData>(levelJSONPath);
            }
            catch
            {
                levelEditorLevelData = new LevelEditorLevelData()
                {
                    RootLevelObject = new LevelEditorLevelObject(),
                    DifficultyConfigurations = new List<LevelEditorDifficultyData>(),
                    ModdedMetadata = new Dictionary<string, string>()
                };
                levelEditorLevelData.AssignNewGeneratedUniqueID();
            }

            levelManager._currentWorkshopLevelDifficultyIndex = 0;
            levelManager._currentLevelHidesTheArena = levelEditorLevelData.ArenaIsHidden || levelEditorLevelData.LevelType == LevelType.Adventure;
            _ = levelManager.CreateLevelTransformFromLevelEditorData(levelEditorLevelData, false).MoveNext();
            return levelManager._currentLevelTransform;
        }

        private IEnumerator spawnStaticBackgroundCoroutine()
        {
            if (m_levelIsLoadingBg)
                m_levelIsLoadingBg.SetActive(true);

            yield return null;
            GameFlowManager.Instance.SwapOutTitleScreenLevel();
            yield return null;

            LevelManager levelManager = LevelManager.Instance;
            while (levelManager.IsCurrentlySwappingInLevel() || levelManager.IsSpawningCurrentLevel())
                yield return null;

            if (m_levelIsLoadingBg)
                m_levelIsLoadingBg.SetActive(false);

            yield break;
        }
    }
}
