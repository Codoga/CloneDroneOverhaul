using OverhaulMod.Utils;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace OverhaulMod.Engine
{
    public class ArenaAudienceManager : Singleton<ArenaAudienceManager>, IGameLoadListener
    {
        public const string FILE_NAME = "arenaAudiencePlacement.json";

        private List<ArenaAudienceLinePoint> m_linePoints;

        private List<AudiencePlacementLine> m_customPlacementLines;
        private List<AudiencePlacementLine> m_customVipPlacementLines;

        private AudiencePlacementLine[] m_vanillaPlacementLines;
        private AudiencePlacementLine[] m_vanillaVipPlacementLines;

        private ArenaAudienceLinePointInfoList m_infoList;

        public void OnGameLoaded()
        {
            if (m_linePoints == null)
                m_linePoints = new List<ArenaAudienceLinePoint>();

            if (m_customPlacementLines == null)
                m_customPlacementLines = new List<AudiencePlacementLine>();

            if (m_customVipPlacementLines == null)
                m_customVipPlacementLines = new List<AudiencePlacementLine>();

            if (m_infoList == null)
                LoadCustomPlacementLinesFile();

            ClearCustomPlacementLines();
            GameObject gameObject = new GameObject("OverhaulAudienceLines");
            CreateCustomPlacementLines(gameObject.transform);
            SetAudiencePlacement(ArenaRemodelManager.EnableRemodel, true);
        }

        public void AddLinePoint(ArenaAudienceLinePoint arenaAudienceLine)
        {
            if (!m_linePoints.Contains(arenaAudienceLine))
                m_linePoints.Add(arenaAudienceLine);
        }

        public void RemoveLinePoint(ArenaAudienceLinePoint arenaAudienceLine)
        {
            _ = m_linePoints.Remove(arenaAudienceLine);
        }

        public void ClearLinesList(List<AudiencePlacementLine> list)
        {
            if (!list.IsNullOrEmpty())
            {
                foreach (AudiencePlacementLine line in list)
                {
                    if (line)
                    {
                        if (line.StartPos)
                            Destroy(line.StartPos.gameObject);

                        if (line.EndPos)
                            Destroy(line.EndPos.gameObject);

                        Destroy(line.gameObject);
                    }
                }
                list.Clear();
            }
        }

        public void LoadCustomPlacementLinesFile()
        {
            string path = Path.Combine(ModCore.dataFolder, FILE_NAME);

            ArenaAudienceLinePointInfoList arenaAudienceLinePointInfoList;
            try
            {
                arenaAudienceLinePointInfoList = ModJsonUtils.DeserializeStream<ArenaAudienceLinePointInfoList>(path);
                arenaAudienceLinePointInfoList.FixValues();
            }
            catch
            {
                arenaAudienceLinePointInfoList = new ArenaAudienceLinePointInfoList();
                arenaAudienceLinePointInfoList.FixValues();
            }
            m_infoList = arenaAudienceLinePointInfoList;
        }

        public void CreateCustomPlacementLines(Transform parent)
        {
            if (m_infoList == null || m_infoList.Points.IsNullOrEmpty())
                return;

            Dictionary<int, (ArenaAudienceLinePointInfo, ArenaAudienceLinePointInfo)> dictionary = new Dictionary<int, (ArenaAudienceLinePointInfo, ArenaAudienceLinePointInfo)>();
            foreach (ArenaAudienceLinePointInfo pointInfo in m_infoList.Points)
            {
                if (!dictionary.ContainsKey(pointInfo.ID))
                {
                    if (pointInfo.IsEnd)
                    {
                        dictionary.Add(pointInfo.ID, (null, pointInfo));
                    }
                    else
                    {
                        dictionary.Add(pointInfo.ID, (pointInfo, null));
                    }
                }
                else
                {
                    (ArenaAudienceLinePointInfo, ArenaAudienceLinePointInfo) tuple = dictionary[pointInfo.ID];
                    if (pointInfo.IsEnd)
                    {
                        tuple.Item2 = pointInfo;
                    }
                    else
                    {
                        tuple.Item1 = pointInfo;
                    }
                    dictionary[pointInfo.ID] = tuple;
                }
            }

            foreach (KeyValuePair<int, (ArenaAudienceLinePointInfo, ArenaAudienceLinePointInfo)> keyValue in dictionary)
            {
                (ArenaAudienceLinePointInfo, ArenaAudienceLinePointInfo) tuple = keyValue.Value;
                if (tuple.Item1 != null && tuple.Item2 != null)
                {
                    AudiencePlacementLine audiencePlacementLine = new GameObject($"AudiencePlacementLine {keyValue.Key}").AddComponent<AudiencePlacementLine>();
                    audiencePlacementLine.transform.SetParent(parent, true);
                    audiencePlacementLine.IsLower = tuple.Item1.IsLower && tuple.Item2.IsLower;
                    audiencePlacementLine.IsMiddle = tuple.Item1.IsMiddle && tuple.Item2.IsMiddle;
                    audiencePlacementLine.IsTop = tuple.Item1.IsTop && tuple.Item2.IsTop;

                    Transform transform = new GameObject($"AudienceLinePoint (ID {keyValue.Key}, Start)").transform;
                    transform.SetParent(parent, true);
                    transform.position = new Vector3(tuple.Item1.PositionX, tuple.Item1.PositionY, tuple.Item1.PositionZ);
                    transform.eulerAngles = new Vector3(tuple.Item1.EulerAnglesX, tuple.Item1.EulerAnglesY, tuple.Item1.EulerAnglesZ);
                    audiencePlacementLine.StartPos = transform;

                    Transform transform2 = new GameObject($"AudienceLinePoint (ID {keyValue.Key}, End)").transform;
                    transform2.SetParent(parent, true);
                    transform2.position = new Vector3(tuple.Item2.PositionX, tuple.Item2.PositionY, tuple.Item2.PositionZ);
                    transform2.eulerAngles = new Vector3(tuple.Item2.EulerAnglesX, tuple.Item2.EulerAnglesY, tuple.Item2.EulerAnglesZ);
                    audiencePlacementLine.EndPos = transform2;

                    if (tuple.Item1.IsVIP && tuple.Item2.IsVIP)
                    {
                        m_customVipPlacementLines.Add(audiencePlacementLine);
                    }
                    else
                    {
                        m_customPlacementLines.Add(audiencePlacementLine);
                    }
                }
            }
        }

        public void ClearCustomPlacementLines()
        {
            ClearLinesList(m_customPlacementLines);
            ClearLinesList(m_customVipPlacementLines);
        }

        public void SaveCustomPlacementLines()
        {
            if (m_linePoints.IsNullOrEmpty())
                return;

            ArenaAudienceLinePointInfoList arenaAudienceLinePointInfoList = new ArenaAudienceLinePointInfoList();
            arenaAudienceLinePointInfoList.FixValues();

            foreach (ArenaAudienceLinePoint point in m_linePoints)
            {
                ArenaAudienceLinePointInfo arenaAudienceLinePointInfo = new ArenaAudienceLinePointInfo(point);
                arenaAudienceLinePointInfoList.Points.Add(arenaAudienceLinePointInfo);
            }

            string path = Path.Combine(ModCore.dataFolder, FILE_NAME);
            ModJsonUtils.WriteStream(path, arenaAudienceLinePointInfoList);
        }

        public void SetAudiencePlacement(bool overhaul, bool refreshNow)
        {
            AudienceManager audienceManager = AudienceManager.Instance;

            bool endSearch = false;
            foreach (AudienceReactionConfiguration reaction in audienceManager.Reactions)
            {
                if (reaction != null && !reaction.Variants.IsNullOrEmpty())
                {
                    foreach (AudienceMatrixTransformerList variant in reaction.Variants)
                    {
                        if (variant != null && !variant.MatrixTransformerPrefabs.IsNullOrEmpty())
                        {
                            foreach (AudienceMatrixTransformer prefab in variant.MatrixTransformerPrefabs)
                            {
                                if (prefab && prefab is WaveAudienceMatrixTransformer wave)
                                {
                                    wave.WaveStartPositionX = -150f;
                                    wave.WaveEndPositionX = 150f;
                                    endSearch = true;
                                    break;
                                }
                            }
                        }
                        if (endSearch)
                            break;
                    }
                }
                if (endSearch)
                    break;
            }

            AudienceRoot2019 audienceRoot2019 = audienceManager.AudienceRoot;

            if (m_vanillaPlacementLines == null)
            {
                m_vanillaPlacementLines = audienceRoot2019.NormalPlacementLines;
            }

            if (m_vanillaVipPlacementLines == null)
            {
                m_vanillaVipPlacementLines = audienceRoot2019.VipPlacementLines;
            }

            if (overhaul)
            {
                audienceRoot2019.NormalPlacementLines = m_customPlacementLines.ToArray();
                audienceRoot2019.VipPlacementLines = m_customVipPlacementLines.ToArray();
            }
            else
            {
                audienceRoot2019.NormalPlacementLines = m_vanillaPlacementLines;
                audienceRoot2019.VipPlacementLines = m_vanillaVipPlacementLines;
            }

            if (refreshNow)
            {
                ArenaCustomizationManager.Instance.RefreshArenaAppearance(true);
                return;
            }
            ArenaCustomizationManager.Instance.RefreshArenaAppearanceNextFrame(true);
        }

        public void PatchAudienceRotation()
        {
            AudienceManager audienceManager = AudienceManager.Instance;
            if (audienceManager && !audienceManager._matrixLists.IsNullOrEmpty())
            {
                foreach (AudienceBotMatrixList audienceBotMatrixList in audienceManager._matrixLists)
                {
                    for (int i = 0; i < audienceBotMatrixList.StartMatricies.Length; i++)
                    {
                        Matrix4x4 matrix = audienceBotMatrixList.StartMatricies[i];
                        Vector4 col = matrix.GetColumn(3);
                        Vector3 pos = (Vector3)col;

                        float xForward = -80f;
                        float xBackward = 91f;

                        if (pos.y < 45f && pos.y > 35f)
                        {
                            xForward = -90f;
                            xBackward = 100f;
                        }
                        else if (pos.y < 55f && pos.y > 45f)
                        {
                            xForward = -100f;
                            xBackward = 110f;
                        }

                        if (pos.x < xForward)
                        {
                            Vector3 eulerAngles = matrix.rotation.eulerAngles;
                            eulerAngles.y = 180f;
                            matrix.SetTRS(pos, Quaternion.Euler(eulerAngles), matrix.lossyScale);
                        }
                        if (pos.x > xBackward)
                        {
                            Vector3 eulerAngles = matrix.rotation.eulerAngles;
                            eulerAngles.y = 0f;
                            matrix.SetTRS(pos, Quaternion.Euler(eulerAngles), matrix.lossyScale);
                        }
                        audienceBotMatrixList.StartMatricies[i] = matrix;
                    }
                }
            }
        }
    }
}
