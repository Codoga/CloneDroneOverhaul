﻿using OverhaulAPI;
using PicaVoxel;
using UnityEngine;

namespace CDOverhaul
{
    public class OverhaulVolumeController : OverhaulController
    {
        [OverhaulSetting("Gameplay.Voxels.Make laser burn voxels", true, false, "Cutting robots with normal sword would leave nearby voxels burnt")]
        public static bool MakeLaserBurnVoxels;

        private static float m_OgFireBurnColorMultiplier = 0.8f;

        public override void Initialize()
        {
            AttackManager manager = AttackManager.Instance;
            if (manager == null)
                return;

            manager.HitColor = new Color(4f, 0.65f, 0.35f, 0.2f);
            manager.BodyOnFireColor = new Color(1f, 0.42f, 0.22f, 0.1f);
            m_OgFireBurnColorMultiplier = manager.FireBurnColorMultiplier;
        }

        public static void OnVoxelDestroy(MechBodyPart bodyPart, PicaVoxelPoint picaVoxelPoint, Voxel? voxelAtPosition, Vector3 impactDirectionWorld, FireSpreadDefinition fireSpreadDefinition, Frame currentFrame)
        {
            bool hasFire = fireSpreadDefinition != null;
            if (!hasFire)
            {
                if (Random.Range(0, 10) < 2)
                {
                    Vector3 position = currentFrame.GetVoxelWorldPosition(picaVoxelPoint);
                    _ = PooledPrefabController.SpawnEntry<PooledPrefabInstanceBase>(Visuals.OverhaulVFXController.LASER_CUT_VFX, position, Vector3.zero);
                }

                if (MakeLaserBurnVoxels)
                {
                    foreach (PicaVoxelPoint p in GetSurroundingPoints(picaVoxelPoint))
                    {
                        if (bodyPart.IsVoxelWaitingToBeDestroyed(p))
                            continue;

                        Voxel? vox = currentFrame.GetVoxelAtArrayPosition(p);
                        if (vox == null)
                            continue;

                        Color32 oldColor = vox.Value.Color;
                        Voxel theVox = vox.Value;
                        theVox.Color = new Color32(getColor(oldColor.r),
                            getColor(oldColor.g),
                            getColor(oldColor.b),
                            oldColor.a);
                        currentFrame.SetVoxelAtArrayPosition(p, theVox);
                    }
                }
                return;
            }

            if (Random.Range(0, 10) < 2)
            {
                Vector3 position = currentFrame.GetVoxelWorldPosition(picaVoxelPoint);
                _ = PooledPrefabController.SpawnEntry<PooledPrefabInstanceBase>(Visuals.OverhaulVFXController.FIRE_CUT_VFX, position, Vector3.zero);
            }
        }

        private static byte getColor(byte color) => (byte)Mathf.RoundToInt(color * m_OgFireBurnColorMultiplier);

        public static PicaVoxelPoint GetOffsetPoint(in PicaVoxelPoint picaVoxelPoint, in int OffX, in int OffY, in int OffZ) => new PicaVoxelPoint(picaVoxelPoint.X + OffX, picaVoxelPoint.Y + OffY, picaVoxelPoint.Z + OffZ);
        public static PicaVoxelPoint[] GetSurroundingPoints(in PicaVoxelPoint picaVoxelPoint)
        {
            PicaVoxelPoint x1 = GetOffsetPoint(picaVoxelPoint, 1, 0, 0);
            PicaVoxelPoint x2 = GetOffsetPoint(picaVoxelPoint, -1, 0, 0);
            PicaVoxelPoint y1 = GetOffsetPoint(picaVoxelPoint, 0, 1, 0);
            PicaVoxelPoint y2 = GetOffsetPoint(picaVoxelPoint, 0, -1, 0);
            PicaVoxelPoint z1 = GetOffsetPoint(picaVoxelPoint, 0, 0, 1);
            PicaVoxelPoint z2 = GetOffsetPoint(picaVoxelPoint, 0, 0, -1);
            return new PicaVoxelPoint[6] { x1, x2, y1, y2, z1, z2 };
        }
    }
}