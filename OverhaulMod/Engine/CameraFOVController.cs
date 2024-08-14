using OverhaulMod.Utils;
using UnityEngine;

namespace OverhaulMod.Engine
{
    public class CameraFOVController : MonoBehaviour
    {
        [ModSetting(ModSettingsConstants.ENABLE_FOV_OVERRIDE, false)]
        public static bool EnableFOVOverride;

        [ModSetting(ModSettingsConstants.CAMERA_FOV_OFFSET, 0f)]
        public static float FOVOffset;

        private CameraManager m_cameraManager;

        private Animator m_cameraAnimator;

        private Camera m_camera;

        private FirstPersonMover m_owner;

        private float m_timeToAllowUnclampedFovUntil;

        private float m_lerpedOffset;

        private void Start()
        {
            m_cameraManager = CameraManager.Instance;
            m_camera = base.GetComponent<Camera>();
            m_cameraAnimator = base.GetComponentInParent<Animator>();
            m_timeToAllowUnclampedFovUntil = Time.time + 1f;
            m_lerpedOffset = getFovOffset();
        }

        private void LateUpdate()
        {
            if (!EnableFOVOverride && !CameraManager.EnableFirstPersonMode)
                return;

            FirstPersonMover owner = m_owner;
            if (!owner || owner._isGrabbedForUpgrade)
                return;

            Animator animator = m_cameraAnimator;
            if (!animator || !animator.enabled /*|| (Time.timeScale <= 0f && animator.updateMode != AnimatorUpdateMode.UnscaledTime)*/)
                return;

            Camera camera = m_camera;
            if (!camera)
                return;

            m_lerpedOffset = Mathf.Lerp(m_lerpedOffset, !GameModeManager.UsesMultiplayerSpawnPoints() || owner.HasConstructionFinished() ? getFovOffset() : 0f, Time.unscaledDeltaTime * 9f);
            camera.fieldOfView = Mathf.Min(camera.fieldOfView + m_lerpedOffset, Time.time < m_timeToAllowUnclampedFovUntil ? 165f : 110f);
        }

        public void SetOwner(FirstPersonMover firstPersonMover)
        {
            m_owner = firstPersonMover;
        }

        private float getFovOffset()
        {
            bool fovOverrideEnabled = EnableFOVOverride;
            CameraManager cameraManager = m_cameraManager;
            return cameraManager.enableForceFOVOffset ? cameraManager.forceFOVOffset : (CameraManager.EnableFirstPersonMode ? Mathf.Min(fovOverrideEnabled ? FOVOffset + 15f : 15f, 40f) : (fovOverrideEnabled ? FOVOffset : 0));
        }
    }
}
