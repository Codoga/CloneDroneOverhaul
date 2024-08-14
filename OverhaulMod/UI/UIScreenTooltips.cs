using UnityEngine;
using UnityEngine.UI;

namespace OverhaulMod.UI
{
    public class UIScreenTooltips : OverhaulUIBehaviour
    {
        public static UIScreenTooltips instance
        {
            get;
            private set;
        }

        [UIElement("Panel")]
        private readonly CanvasGroup m_canvasGroup;

        [UIElement("Text")]
        private readonly Text m_text;

        private float m_alpha;

        private float m_timeLeft;

        public override bool refreshOnlyCursor => true;
        public override bool closeOnEscapeButtonPress => false;

        protected override void OnInitialized()
        {
            instance = this;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            instance = null;
        }

        public override void Update()
        {
            base.Update();

            float d = Time.unscaledDeltaTime;
            m_alpha = Mathf.Lerp(m_alpha, m_timeLeft > 0f ? 1f : 0f, d * 10f);
            m_canvasGroup.alpha = NumberUtils.EaseInOutCubic(0f, 1f, m_alpha);

            if (m_timeLeft > 0f)
                m_timeLeft -= d;
        }

        public void ShowText(string text, float duration)
        {
            m_text.text = text;
            m_timeLeft = duration;
        }
    }
}
