using OverhaulMod.Utils;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace OverhaulMod.UI
{
    public class UIAdvancementProgress : OverhaulUIBehaviour
    {
        [UIElement("Panel")]
        private readonly CanvasGroup m_canvasGroup;

        [UIElement("Image")]
        private readonly Image m_achievementImage;
        [UIElement("ProgressText")]
        private readonly Text m_achievementProgressText;
        [UIElement("ProgressFill")]
        private readonly Image m_achievementProgressBarFill;

        public override bool closeOnEscapeButtonPress => false;

        public float ShowUntil;

        public bool shouldBeVisible
        {
            get
            {
                return Time.unscaledTime < ShowUntil;
            }
        }

        protected override void OnInitialized()
        {
            m_canvasGroup.alpha = 0f;
        }

        public void ShowProgress(GameplayAchievement gameplayAchievement)
        {
            Show();

            int progress = GameplayAchievementManager.Instance.GetProgress(gameplayAchievement.AchievementID);
            int targetProgress = gameplayAchievement.TargetProgress;

            if (progress >= targetProgress || progress % 25 != 0)
                return;

            m_achievementImage.sprite = gameplayAchievement.GetImageSprite();
            m_achievementProgressBarFill.fillAmount = progress / (float)targetProgress;
            m_achievementProgressText.text = $"{progress}/{targetProgress}";
            _ = ModActionUtils.RunCoroutine(waitThenHide());
        }

        private IEnumerator waitThenHide()
        {
            m_canvasGroup.alpha = 0.6f;
            yield return new WaitForSecondsRealtime(5f);
            m_canvasGroup.alpha = 0f;
            yield break;
        }
    }
}
