using OverhaulMod.Engine;
using OverhaulMod.Utils;
using UnityEngine;

namespace OverhaulMod.Visuals.Environment
{
    public class FloatingDustManager : Singleton<FloatingDustManager>
    {
        [ModSetting(ModSettingsConstants.ENABLE_FLOATING_DUST, true)]
        public static bool EnableFloatingDust;

        private ParticleSystem[] m_normalSpaceDust;
        private ParticleSystem[] m_mindSpaceDust;

        private float m_timeToUpdate;

        public Transform floatingDustVFXHolder
        {
            get;
            private set;
        }

        private void Start()
        {
            createHolderIfNull();
            populateVFX();
        }

        private void Update()
        {
            m_timeToUpdate -= Time.deltaTime;
            if (m_timeToUpdate <= 0f)
            {
                m_timeToUpdate = 1f;
                RefreshVFX();
            }
        }

        private void createHolderIfNull()
        {
            if (floatingDustVFXHolder)
                return;

            GameObject newHolder = new GameObject("OverhaulFloatingDust");
            newHolder.transform.position = Vector3.zero;
            DontDestroyOnLoad(newHolder);
            floatingDustVFXHolder = newHolder.transform;
        }

        private void populateVFX()
        {
            if (m_normalSpaceDust == null)
            {
                m_normalSpaceDust = new ParticleSystem[]
                {
                    Instantiate(ModResources.Prefab(AssetBundleConstants.VFX, "FloatingDust_Normal"), floatingDustVFXHolder).GetComponent<ParticleSystem>()
                };
            }

            if (m_mindSpaceDust == null)
            {
                m_mindSpaceDust = new ParticleSystem[]
                {
                    Instantiate(ModResources.Prefab(AssetBundleConstants.VFX, "FloatingDust_Mindspace0"), floatingDustVFXHolder).GetComponent<ParticleSystem>(),
                    Instantiate(ModResources.Prefab(AssetBundleConstants.VFX, "FloatingDust_Mindspace1"), floatingDustVFXHolder).GetComponent<ParticleSystem>()
                };
            }
        }

        public void RefreshVFX()
        {
            Transform transform = floatingDustVFXHolder;
            if (!transform)
                return;

            bool allowParticles = EnableFloatingDust;
            bool isMindspace = false;

            FirstPersonMover firstPersonMover = CharacterTracker.Instance?.GetPlayerRobot();
            if (firstPersonMover)
                isMindspace = firstPersonMover.IsMindSpaceCharacter;
            else
                allowParticles = false;

            SetParticlesActive(m_normalSpaceDust, !isMindspace && allowParticles);
            SetParticlesActive(m_mindSpaceDust, isMindspace && allowParticles);
            transform.position = allowParticles ? firstPersonMover.transform.position : Vector3.zero;
        }

        public void SetParticlesActive(ParticleSystem[] particleSystems, bool value)
        {
            if (particleSystems == null || particleSystems.Length == 0)
                return;

            foreach (ParticleSystem system in particleSystems)
            {
                system.SetEmissionEnabled(value);
                system.transform.localPosition = Vector3.zero;
            }
        }
    }
}
