using OverhaulMod.Engine;
using OverhaulMod.Utils;
using OverhaulMod.Visuals;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace OverhaulMod.UI
{
    public class UIImageEffects : OverhaulUIBehaviour
    {
        [UIElement("Vignette", true)]
        private readonly Image m_vignetteImage;

        [UIElement("Dithering", true)]
        private readonly RawImage m_ditheringImage;

        public override bool closeOnEscapeButtonPress => false;

        private float m_timeLeftToSwitchTexture;
        private int m_TextureIndex;

        public static Texture2D[] ditheringTextures
        {
            get;
            private set;
        }

        protected override void OnInitialized()
        {
            m_vignetteImage.enabled = false;
            m_ditheringImage.enabled = false;

            if (ditheringTextures == null)
                GenerateNoiseTextures();
        }

        public override void Update()
        {
            base.Update();

            bool isNotInLevelEditor = !GameModeManager.IsInLevelEditor();
            bool overrideSettings = AdvancedPhotoModeManager.Settings.overrideSettings;

            float v = m_timeLeftToSwitchTexture - Time.unscaledDeltaTime;
            m_timeLeftToSwitchTexture = v;
            if (v > 0f)
                return;
            m_timeLeftToSwitchTexture = 0.034f;

            bool hasCamera = CameraManager.Instance.mainCamera;
            bool enableVignette = isNotInLevelEditor && hasCamera && (overrideSettings ? AdvancedPhotoModeManager.Settings.EnableVignette : PostEffectsManager.EnableVignette);
            bool enableDithering = isNotInLevelEditor && hasCamera && (overrideSettings ? AdvancedPhotoModeManager.Settings.EnableDithering : PostEffectsManager.EnableDithering) && ditheringTextures != null;

            m_vignetteImage.enabled = enableVignette;
            m_ditheringImage.enabled = enableDithering;

            if (enableDithering)
            {
                Color color = m_ditheringImage.color;
                color.a = 0.25f;
                m_ditheringImage.color = color;

                int index = m_TextureIndex;
                m_ditheringImage.texture = ditheringTextures[index];
                index++;
                if (index > 2)
                    index = 0;
                m_TextureIndex = index;
            }

            if (enableVignette)
            {
                Color color = m_vignetteImage.color;
                color.a = overrideSettings ? AdvancedPhotoModeManager.Settings.VignetteIntensity : 0.5f;
                m_vignetteImage.color = color;
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            if (ditheringTextures != null)
            {
                foreach (Texture2D t in ditheringTextures)
                    if (t)
                        Destroy(t);
            }
            ditheringTextures = null;
        }

        public void GenerateNoiseTextures()
        {
            _ = ModActionUtils.RunCoroutine(generateNoiseTexturesCoroutine(onGeneratedTextures), true);
        }

        private IEnumerator generateNoiseTexturesCoroutine(Action<Texture2D[]> callback)
        {
            int width = 1024;
            int height = 576;

            Texture2D[] result = new Texture2D[3];
            yield return ModActionUtils.RunCoroutine(generatePerlinNoiseCoroutine(width, height, delegate (Texture2D texture)
            {
                result[0] = texture;
            }));
            yield return ModActionUtils.RunCoroutine(generatePerlinNoiseCoroutine(width, height, delegate (Texture2D texture)
            {
                result[1] = texture;
            }));
            yield return ModActionUtils.RunCoroutine(generatePerlinNoiseCoroutine(width, height, delegate (Texture2D texture)
            {
                result[2] = texture;
            }));
            callback?.Invoke(result);
            yield break;
        }

        private IEnumerator generatePerlinNoiseCoroutine(int width, int height, Action<Texture2D> callback)
        {
            Texture2D texture2D = new Texture2D(width, height);
            Color[] array = new Color[width * height];
            float num = 0f;
            float num2 = 0f;
            for (float num3 = 0f; num3 < texture2D.height; num3++)
            {
                for (float num4 = 0f; num4 < texture2D.width; num4++)
                {
                    float x = num + (num4 / texture2D.width);
                    float y = num2 + (num3 / texture2D.height);
                    float num5 = Mathf.PerlinNoise(x, y) + UnityEngine.Random.Range(-0.2f, 0.4f);
                    array[((int)num3 * texture2D.width) + (int)num4] = new Color(num5, num5, num5, 0.1f);

                    if (num3 + (num4 % 100) == 0)
                        yield return null;
                }
            }
            for (int i = 0; i < array.Length; i++)
            {
                float num6 = UnityEngine.Random.Range(-0.2f, 0.4f);
                array[i] = new Color(array[i].r + num6, array[i].g + num6, array[i].b + num6, 0.1f);
            }
            texture2D.SetPixels(array);
            texture2D.Apply();
            callback.Invoke(texture2D);
            yield break;
        }

        private static void onGeneratedTextures(Texture2D[] textures)
        {
            ditheringTextures = textures;
        }
    }
}
