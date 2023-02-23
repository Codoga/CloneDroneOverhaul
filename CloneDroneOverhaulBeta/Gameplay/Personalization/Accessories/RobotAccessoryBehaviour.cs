﻿using OverhaulAPI;
using UnityEngine;

namespace CDOverhaul.Gameplay
{
    /// <summary>
    /// The accessory gameobject controller
    /// </summary>
    public class RobotAccessoryBehaviour : MonoBehaviour
    {
        public FirstPersonMover Owner;

        public SerializeTransform TargetTransform;

        public RobotAccessoryItemDefinition Item;

        private void Awake()
        {
            Item = RobotAccessoriesController.GetAccessoryByPrefabName(base.gameObject.name);
        }

        private void OnCollisionEnter(Collision collision)
        {
            TryDestroyAccessory(collision.collider);
        }

        private void OnTriggerEnter(Collider other)
        {
            TryDestroyAccessory(other);
        }

        public void TryDestroyAccessory(Collider other)
        {
            if (other == null)
            {
                return;
            }

            MeleeImpactArea a = other.GetComponent<MeleeImpactArea>();
            LavaFloor fl = other.GetComponent<LavaFloor>();
            if (fl != null || (a && a.IsDamageActive() && (Owner == null || a.Owner != Owner)))
            {
                _ = PooledPrefabController.SpawnObject<RobotAccessoryDestroyVFX>(RobotAccessoriesController.AccessoryDestroyVFX_ID, base.transform.position, base.transform.eulerAngles);
                _ = AudioManager.Instance.PlayClipAtTransform(RobotAccessoriesController.AccessoryDestroyedSound, base.transform, 0f, false, 2f, 0.8f, 0.1f);

                RobotAccessoriesWearer w = FirstPersonMoverExtention.GetExtention<RobotAccessoriesWearer>(Owner);
                if (w != null)
                {
                    w.UnregisterAccessory(base.gameObject);
                }
                Destroy(base.gameObject);
            }
        }
    }
}