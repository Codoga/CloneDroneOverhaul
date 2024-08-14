using OverhaulMod.Utils;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace OverhaulMod.Engine
{
    public class TransitionBehaviour : ModBehaviour
    {
        private Image m_image;
        private CanvasGroup m_canvasGroup;
        private Text m_text;

        private ErrorManager m_errorManager;

        private float m_timeToFade;

        private bool m_destroyed;

        public bool fadeOut
        {
            get;
            set;
        }

        public bool ignoreDeltaTime
        {
            get;
            set;
        }

        public float deltaTimeMultiplier
        {
            get;
            set;
        }

        public float waitBeforeFadeOut
        {
            get;
            set;
        }

        public override void Awake()
        {
            m_image = base.GetComponent<Image>();
            m_canvasGroup = base.GetComponent<CanvasGroup>();
            m_text = moddedObjectReference.GetObject<Text>(0);
        }

        public override void Start()
        {
            m_errorManager = ErrorManager.Instance;
        }

        public override void Update()
        {
            if (m_destroyed)
                return;

            ErrorManager errorManager = ErrorManager.Instance;
            if (errorManager && errorManager.HasCrashed())
            {
                m_destroyed = true;
                Destroy(base.gameObject);
                return;
            }

            bool fo = fadeOut;
            if (fo && m_timeToFade > Time.unscaledTime)
                return;

            float alpha = m_canvasGroup.alpha;
            alpha = Mathf.Lerp(alpha, fo ? 0f : 1f, (ignoreDeltaTime ? 1f : Time.unscaledDeltaTime) * deltaTimeMultiplier);
            m_canvasGroup.alpha = alpha;
            m_canvasGroup.blocksRaycasts = alpha >= 0.9f;

            if (fo && alpha <= 0.05f)
            {
                Destroy(base.gameObject);
            }
        }

        public void SetTextVisible(bool value)
        {
            m_text.gameObject.SetActive(value);
        }

        public void SetBackgroundColor(Color color)
        {
            m_image.color = color;
        }

        public void RunCoroutine(IEnumerator enumerator)
        {
            if (enumerator == null)
                return;

            _ = ModActionUtils.RunCoroutine(enumerator);
        }

        public void Refresh()
        {
            m_canvasGroup.alpha = fadeOut ? 1f : 0f;
            m_timeToFade = Time.unscaledTime + waitBeforeFadeOut;
        }
    }
}
