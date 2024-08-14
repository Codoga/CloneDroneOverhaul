using OverhaulMod.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace OverhaulMod.UI
{
    public class UILoadingScreenRework : OverhaulUIBehaviour
    {
        private ChapterLoadingScreen m_chapterLoadingScreen;
        private LoadingScreen m_loadingScreen;

        [UIElement("Description")]
        private readonly Text m_stateText;

        [UIElement("ObjectProgress")]
        private readonly Text m_objectProgressText;

        [UIElement("Fill")]
        private readonly Image m_progressBarFill;

        [UIElement("ProgressBar")]
        private readonly GameObject m_progressBarObject;

        public override bool closeOnEscapeButtonPress => false;

        public override void Show()
        {
            base.Show();
            m_progressBarObject.SetActive(false);
            m_stateText.text = "Please wait";
            m_objectProgressText.text = string.Empty;
            m_progressBarFill.fillAmount = 0f;
            m_chapterLoadingScreen = ModCache.gameUIRoot.ChapterLoadingScreen;
            m_loadingScreen = ModCache.gameUIRoot.LoadingScreen;
        }

        public string GetStateString(ChapterLoadingScreenState chapterLoadingScreenState)
        {
            switch (chapterLoadingScreenState)
            {
                case ChapterLoadingScreenState.NOT_STARTED:
                    return "Please wait";
                case ChapterLoadingScreenState.LOADING_LEVEL:
                    return "Loading level data";
                case ChapterLoadingScreenState.LOADING_OBJECTS:
                    return "Spawning objects";
                case ChapterLoadingScreenState.STARTING_OBJECTS:
                    return "Starting objects";
                case ChapterLoadingScreenState.DONE:
                    return "Complete!";
            }
            return chapterLoadingScreenState.ToString();
        }

        public override void Update()
        {
            ErrorManager errorManager = ErrorManager.Instance;
            if (errorManager && errorManager.HasCrashed())
            {
                Hide();
            }

            ChapterLoadingScreen chapterLoadingScreen = m_chapterLoadingScreen;
            if (chapterLoadingScreen)
            {
                if (Time.frameCount % 5 == 0)
                {
                    if (GameModeManager.Is(GameMode.Story))
                    {
                        m_stateText.text = GetStateString(chapterLoadingScreen._screenState);
                        m_progressBarObject.SetActive(chapterLoadingScreen.gameObject.activeInHierarchy);
                        m_objectProgressText.enabled = true;
                        m_objectProgressText.text = $"Instantiated: {chapterLoadingScreen._objectsToInstantiateTotal - chapterLoadingScreen._objectsToInstantiateRemaining}/{chapterLoadingScreen._objectsToInstantiateTotal}\nStarted: {chapterLoadingScreen._objectsToAwakenTotal - chapterLoadingScreen._objectstoAwakenRemaining}/{chapterLoadingScreen._objectsToAwakenTotal}";
                    }
                    else
                    {
                        m_stateText.text = "Please wait";
                        m_objectProgressText.enabled = false;
                        m_progressBarObject.SetActive(false);
                    }
                }
                m_progressBarFill.fillAmount = chapterLoadingScreen.getProgress();
            }
        }
    }
}
