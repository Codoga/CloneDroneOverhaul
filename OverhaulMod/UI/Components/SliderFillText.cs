using System;
using UnityEngine;
using UnityEngine.UI;

namespace OverhaulMod.UI
{
    public class SliderFillText : MonoBehaviour
    {
        public Slider SliderComponent;

        public Text Label;

        public Func<float, string> Function;

        private float m_prevValue;

        private void Start()
        {
            Slider sl = SliderComponent;
            if (!sl)
                return;

            m_prevValue = sl.value;
            UpdateText();
        }

        private void Update()
        {
            Slider sl = SliderComponent;
            if (!sl)
                return;

            float newValue = sl.value;
            if (newValue != m_prevValue)
            {
                m_prevValue = newValue;
                UpdateText();
            }
        }

        public void UpdateText()
        {
            Slider sl = SliderComponent;
            Text label = Label;
            Func<float, string> func = Function;

            if (func == null || !sl || !label)
                return;

            float value = sl.value;
            string st = func(value);

            label.text = st;
        }
    }
}
