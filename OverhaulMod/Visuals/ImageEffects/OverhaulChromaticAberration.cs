using OverhaulMod.Utils;
using UnityEngine;

namespace OverhaulMod.Visuals.ImageEffects
{
    public class OverhaulChromaticAberration : MonoBehaviour
    {
        private float m_power;
        public float power
        {
            get
            {
                return m_power;
            }
            set
            {
                m_power = value;
                if (m_supported)
                    m_material.SetFloat("_ChromaticAberration", 0.01f * value);
            }
        }

        private float m_center;
        public float center
        {
            get
            {
                return m_center;
            }
            set
            {
                m_center = value;
                if (m_supported)
                    m_material.SetFloat("_Center", value);
            }
        }

        private Shader m_shader;

        private Material m_material;

        private bool m_supported;

        private void Start()
        {
            if (!SystemInfo.supportsImageEffects)
                return;

            m_shader = ModResources.Shader(AssetBundleConstants.IMAGE_EFFECTS, "ChromaticAberration");
            if (!m_shader || !m_shader.isSupported)
                return;

            m_material = new Material(m_shader);
            m_supported = true;
        }

        private void OnDestroy()
        {
            if (m_material)
            {
                Destroy(m_material);
                m_material = null;
                m_shader = null;
            }
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (m_supported)
            {
                Graphics.Blit(source, destination, m_material);
            }
            else
            {
                Graphics.Blit(source, destination);
            }
        }
    }
}
