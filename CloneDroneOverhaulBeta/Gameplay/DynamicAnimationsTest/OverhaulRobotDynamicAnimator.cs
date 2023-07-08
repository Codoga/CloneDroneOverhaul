﻿using UnityEngine;

namespace CDOverhaul.Gameplay.Combat
{
    public class OverhaulRobotDynamicAnimator : OverhaulCharacterExpansion
    {
        private Transform m_HeadTransform;

        public override void Start()
        {
            base.Start();

            m_HeadTransform = Owner.GetBodyPartParent("Head");
        }

        private void LateUpdate()
        {
            if (!Owner || !Owner.IsAlive())
                return;

            Vector3 vector = m_HeadTransform.eulerAngles;
            vector.x = GetCameraEulerAngles().x - 10f;
            m_HeadTransform.eulerAngles = vector;
        }

        public Vector3 GetCameraEulerAngles()
        {
            if (!Owner)
                return Vector3.zero;

            PlayerCameraMover cameraMover = Owner.GetCameraMover();
            if (!cameraMover)
                return Vector3.zero;

            Vector3 euler = cameraMover.transform.eulerAngles;
            return euler;
        }
    }
}