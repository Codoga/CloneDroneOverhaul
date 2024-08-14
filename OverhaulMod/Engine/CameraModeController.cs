using OverhaulMod.Utils;
using UnityEngine;

namespace OverhaulMod.Engine
{
    public class CameraModeController : MonoBehaviour
    {
        private CameraManager m_cameraManager;

        private FirstPersonMover m_owner;

        private PlayerCameraMover m_cameraMover;

        private Animator m_cameraAnimator;

        private Transform m_transformToFollow;

        private Vector3 m_offset;

        private float m_lerp;

        public float ForwardVectorMultiplier = 0.25f;

        public Vector3 ShakePositionOffset, ShakeRotationOffset;

        private Renderer[] m_headRenderers;
        public Renderer[] headRenderers
        {
            get
            {
                if (m_headRenderers.IsNullOrEmpty())
                    m_headRenderers = m_owner.GetRenderersOfBodyPart("Head");

                return m_headRenderers;
            }
        }

        private Renderer[] m_jawRenderers;
        public Renderer[] jawRenderers
        {
            get
            {
                if (m_jawRenderers.IsNullOrEmpty())
                    m_jawRenderers = m_owner.GetRenderersOfBodyPart("Jaw");

                return m_jawRenderers;
            }
        }

        private Renderer[] m_shieldRenderers;
        public Renderer[] shieldRenderers
        {
            get
            {
                if (m_shieldRenderers.IsNullOrEmpty())
                    m_shieldRenderers = m_owner.GetRenderersOfBodyPart("Shield");

                return m_shieldRenderers;
            }
        }

        private Renderer[] m_torsoRenderers;
        public Renderer[] torsoRenderers
        {
            get
            {
                if (m_torsoRenderers.IsNullOrEmpty())
                    m_torsoRenderers = m_owner.GetRenderersOfBodyPart("Torso");

                return m_torsoRenderers;
            }
        }

        private void Start()
        {
            m_lerp = CameraManager.EnableFirstPersonMode ? 0f : 1f;
            m_cameraManager = CameraManager.Instance;

            GlobalEventManager.Instance.AddEventListener(GlobalEvents.ConsciousnessTransferStarted, ForceEnableHeadRenderers);
            GlobalEventManager.Instance.AddEventListener(GlobalEvents.ConsciousnessTransferComplete, ForceEnableHeadRenderers);
        }

        private void OnDestroy()
        {
            GlobalEventManager.Instance.RemoveEventListener(GlobalEvents.ConsciousnessTransferStarted, ForceEnableHeadRenderers);
            GlobalEventManager.Instance.RemoveEventListener(GlobalEvents.ConsciousnessTransferComplete, ForceEnableHeadRenderers);
        }

        public void SetOwner(FirstPersonMover firstPersonMover)
        {
            m_owner = firstPersonMover;
        }

        public void SetHeadRenderersActive(bool value)
        {
            setRenderersActive(headRenderers, value);
            setRenderersActive(jawRenderers, value);
            setRenderersActive(shieldRenderers, value);
            setRenderersActive(torsoRenderers, value || m_owner._isOnFloorFromKick);
        }

        public bool IsMindTransferInProgress()
        {
            PlayerCameraMover mover = m_cameraMover;
            return mover && mover._isConsciousnessTransferInProgress;
        }

        public void ForceEnableHeadRenderers()
        {
            SetHeadRenderersActive(true);
        }

        public void RefreshFields()
        {
            if (!m_cameraMover)
                m_cameraMover = base.GetComponent<PlayerCameraMover>();

            FirstPersonMover firstPersonMover = m_owner;
            if (!firstPersonMover || !firstPersonMover.HasCharacterModel())
                return;

            m_cameraAnimator = firstPersonMover._cameraHolderAnimator;

            if (m_transformToFollow)
                return;

            BaseBodyPart headPart = firstPersonMover.IsMindSpaceCharacter ? (BaseBodyPart)firstPersonMover.GetBodyPartParent("Head").GetComponentInChildren<MindSpaceBodyPart>() : firstPersonMover.GetBodyPart(MechBodyPartType.Head);
            if (!headPart)
                return;

            Transform transform = headPart.transform.parent;
            if (!transform)
                return;

            m_transformToFollow = transform;

            RefreshOffset();
        }

        public void RefreshOffset()
        {
            if (m_owner.IsMindSpaceCharacter)
            {
                m_offset = Vector3.up * 0.575f;
                return;
            }

            Bounds bounds = new Bounds(m_transformToFollow.localPosition, Vector3.zero);
            int index = 0;
            foreach (MeshFilter meshFilter in m_transformToFollow.GetComponentsInChildren<MeshFilter>())
            {
                Mesh mesh = meshFilter.mesh;
                if (mesh)
                {
                    if (index == 0)
                        bounds = mesh.bounds;
                    else
                        bounds.Encapsulate(mesh.bounds);
                }
                index++;
            }

            m_offset = new Vector3(0f, bounds.center.y + (bounds.extents.y * 0.2f), 0f);
        }

        public void RefreshHeadVisibility(float lerpValue)
        {
            Animator animator = m_cameraAnimator;
            SetHeadRenderersActive(lerpValue > 0.1f || !animator || !animator.enabled || !m_owner.IsAlive() || PhotoManager.Instance.IsInPhotoMode());
        }

        private void setRenderersActive(Renderer[] array, bool value)
        {
            if (!array.IsNullOrEmpty())
                foreach (Renderer renderer3 in array)
                    if (renderer3)
                        renderer3.enabled = value;
        }

        private void LateUpdate()
        {
            int fc = Time.frameCount;
            bool refresh = fc % 5 == 0;
            if (refresh)
                RefreshFields();

            FirstPersonMover firstPersonMover = m_owner;
            if (IsMindTransferInProgress() || !firstPersonMover || !firstPersonMover.IsAlive())
                return;

            m_lerp = Mathf.Clamp01(m_lerp + ((CameraManager.EnableFirstPersonMode && !m_cameraManager.enableThirdPerson && (!GameModeManager.UsesMultiplayerSpawnPoints() || firstPersonMover.HasConstructionFinished()) && !firstPersonMover._isGrabbedForUpgrade ? -Time.unscaledDeltaTime : Time.unscaledDeltaTime) * 3f));

            if (refresh)
                RefreshHeadVisibility(m_lerp);

            Animator animator = m_cameraAnimator;
            if (!m_transformToFollow || !animator || !animator.enabled)
                return;

            PlayerCameraMover playerCameraMover = m_cameraMover;
            if (playerCameraMover)
            {
                if (m_lerp > 0f && m_lerp < 1f)
                    playerCameraMover._hasCollidedWithEnvironment = true;
                else if (m_lerp == 0f)
                    playerCameraMover._hasCollidedWithEnvironment = false;

                playerCameraMover.LateUpdate();
            }

            Vector3 forwardVector = m_transformToFollow.forward * (firstPersonMover._isOnFloorFromKick ? 0f : ForwardVectorMultiplier);
            Vector3 upVector = m_transformToFollow.forward * (firstPersonMover._isOnFloorFromKick ? 0.35f : 0f);

            Transform transform = base.transform;
            Vector3 difference = transform.position;
            transform.position = ModUnityUtils.LerpVector3(m_transformToFollow.position + m_offset + forwardVector + upVector, difference, NumberUtils.EaseInOutCubic(0f, 1f, m_lerp)) + ShakePositionOffset;

            if (m_lerp <= 0.99f)
            {
                Vector3 localPosition = transform.localPosition;
                localPosition.x = ShakePositionOffset.x;
                transform.localPosition = localPosition;
            }

            /*
            if (CameraManager.EnableFirstPersonMode)
            {
                CharacterModel characterModel = firstPersonMover.GetCharacterModel();
                if (characterModel)
                    characterModel.transform.localPosition = Vector3.zero;
            }*/
        }
    }
}
