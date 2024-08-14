using OverhaulMod.Utils;
using UnityEngine;

namespace OverhaulMod.Visuals.ImageEffects
{
    public class OverhaulColorBlindness : MonoBehaviour
    {
        // 0 - Normal
        // 1 - Protanopia
        // 2 - Deuteranopia
        // 3 - Tritanopia

        private int m_type;
        public int type
        {
            get
            {
                return m_type;
            }
            set
            {
                if (m_material)
                    m_material.SetInt("type", value);

                m_type = value;
            }
        }

        public Shader m_shader;

        private Material m_material;

        private bool m_supported;

        private void Start()
        {
            if (!SystemInfo.supportsImageEffects || SystemInfo.graphicsShaderLevel < 30)
                return;

            m_shader = ModResources.Shader(AssetBundleConstants.IMAGE_EFFECTS, "Colorblind");
            if (!m_shader || !m_shader.isSupported)
                return;

            m_material = new Material(m_shader);
            m_material.SetInt("type", type);

            if (!m_material || m_material.passCount != 1)
                return;

            m_supported = true;
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
