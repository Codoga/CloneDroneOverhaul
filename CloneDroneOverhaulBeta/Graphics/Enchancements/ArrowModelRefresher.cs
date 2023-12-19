﻿using System;
using System.Collections;
using UnityEngine;

namespace CDOverhaul.Graphics
{
    public class ArrowModelRefresher : OverhaulBehaviour
    {
        private ArrowProjectile m_arrowProjectile;

        private Transform m_normalVisualsTransform;
        private GameObject[] m_normalVisuals;

        private Transform m_fireVisualsTransform;
        private GameObject[] m_fireVisuals;

        private Transform m_newNormalModelTransform;
        private Transform m_newFireModelTransform;

        private bool m_hasStarted;

        public override void Start()
        {
            m_arrowProjectile = base.GetComponent<ArrowProjectile>();

            Transform normalVisualsObject = TransformUtils.FindChildRecursive(base.transform, "NormalVisuals");
            if (normalVisualsObject && normalVisualsObject.childCount != 0)
            {
                GameObject[] gameObjects = new GameObject[normalVisualsObject.childCount];
                for (int i = 0; i < normalVisualsObject.childCount; i++)
                {
                    gameObjects[i] = normalVisualsObject.GetChild(i).gameObject;
                }
                m_normalVisuals = gameObjects;
                m_normalVisualsTransform = normalVisualsObject;
            }
            else
                m_normalVisuals = Array.Empty<GameObject>();

            Transform fireVisualsObject = TransformUtils.FindChildRecursive(base.transform, "FlamingVisuals");
            if (fireVisualsObject && fireVisualsObject.childCount != 0)
            {
                GameObject[] gameObjects = new GameObject[fireVisualsObject.childCount];
                for (int i = 0; i < fireVisualsObject.childCount; i++)
                {
                    gameObjects[i] = fireVisualsObject.GetChild(i).gameObject;
                }
                m_fireVisuals = gameObjects;
                m_fireVisualsTransform = fireVisualsObject;
            }
            else
                m_fireVisuals = Array.Empty<GameObject>();

            InstantiateNewModels();
            m_hasStarted = true;
        }

        public override void OnEnable()
        {
            _ = StaticCoroutineRunner.StartStaticCoroutine(waitThenRefreshAllVisuals());
        }

        public void RefreshAllVisuals()
        {
            if (!m_arrowProjectile || !m_hasStarted)
                return;

            SetDefaultVisuals(false);

            Transform[] transforms = m_arrowProjectile.BladeScaleTransforms;
            if (transforms != null && transforms.Length != 0)
            {
                Transform transform = transforms[0];
                if (transform)
                {
                    Vector3 vector = m_newNormalModelTransform.localScale;
                    vector.x = Mathf.Max(0.4f, transform.localScale.x / 4f);

                    m_newNormalModelTransform.localScale = vector;
                    m_newFireModelTransform.localScale = vector;
                }
            }
        }

        public void SetDefaultVisuals(bool visible)
        {
            if (m_fireVisuals != null)
            {
                foreach (GameObject gameObject in m_fireVisuals)
                    gameObject.SetActive(visible);
            }

            if (m_normalVisuals != null)
            {
                foreach (GameObject gameObject in m_normalVisuals)
                    gameObject.SetActive(visible);
            }

            if (m_newNormalModelTransform)
                m_newNormalModelTransform.gameObject.SetActive(!visible);

            if (m_newFireModelTransform)
                m_newFireModelTransform.gameObject.SetActive(!visible);
        }

        public void InstantiateNewModels()
        {
            if (!m_newNormalModelTransform)
            {
                Transform arrowModel = Instantiate(OverhaulAssetsController.GetAsset<GameObject>("OverhaulVRArrowModel", "overhaul_models", false)).transform;
                arrowModel.SetParent(m_normalVisualsTransform);
                arrowModel.localPosition = new Vector3(0.025f, -0.025f, -0.6f);
                arrowModel.localEulerAngles = new Vector3(0f, 180f, 0f);
                arrowModel.localScale = Vector3.one * 0.4f;
                MeshRenderer meshRenderer = arrowModel.GetComponent<MeshRenderer>();
                if (meshRenderer)
                {
                    meshRenderer.material.shader = Shader.Find("Standard");
                    meshRenderer.material.SetColor("_EmissionColor", new Color(0.7f, 1.5f, 3f) * 2f);
                }
                m_newNormalModelTransform = arrowModel;
            }

            if (!m_newFireModelTransform)
            {
                Transform arrowModel = Instantiate(OverhaulAssetsController.GetAsset<GameObject>("OverhaulVRArrowModel", "overhaul_models", false)).transform;
                arrowModel.SetParent(m_fireVisualsTransform);
                arrowModel.localPosition = new Vector3(0.025f, -0.025f, -0.6f);
                arrowModel.localEulerAngles = new Vector3(0f, 180f, 0f);
                arrowModel.localScale = Vector3.one * 0.4f;
                MeshRenderer meshRenderer = arrowModel.GetComponent<MeshRenderer>();
                if (meshRenderer)
                {
                    meshRenderer.material.shader = Shader.Find("Standard");
                    meshRenderer.material.SetColor("_EmissionColor", WeaponManager.Instance.FireSpearModelPrefab.GetComponent<MeshRenderer>().material.GetColor("_EmissionColor"));
                }

                Transform ogVfx = TransformUtils.FindChildRecursive(m_fireVisualsTransform, "FireVFX");
                if (ogVfx)
                {
                    Transform newVfx = Instantiate(ogVfx, arrowModel);
                    newVfx.localPosition = new Vector3(0.05f, 0f, -1.4f);
                    newVfx.localEulerAngles = new Vector3(270f, 180f, 0f);
                    newVfx.localScale = Vector3.one * 0.03f;
                }

                m_newFireModelTransform = arrowModel;
            }
        }

        private IEnumerator waitThenRefreshAllVisuals()
        {
            if (!m_hasStarted)
            {
                yield return new WaitForEndOfFrame();
            }
            yield return new WaitForEndOfFrame();
            RefreshAllVisuals();
            yield break;
        }
    }
}