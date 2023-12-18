﻿using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace OverhaulMod.UI
{
    public class UIDeveloperMenu : OverhaulBehaviour
    {
        private static readonly Dictionary<string, string> s_debugValues = new Dictionary<string, string>();
        private StringBuilder m_stringBuilder;

        public static bool Enabled;

        public override void Start()
        {
            m_stringBuilder = new StringBuilder();
        }

#if DEBUG
        public void OnGUI()
        {
            if (!Enabled)
                return;

            GUILayout.BeginArea(new Rect(10f, 100f, 200f, 200f));
            if (GameModeManager.IsOnTitleScreen())
            {
                if (GUILayout.Button("Update build compile date"))
                    ModBuildInfo.GenerateExtraInfo();
            }

            ModDebug.forceDisableCursor = GUILayout.Toggle(ModDebug.forceDisableCursor, "Force disable cursor [Ctrl+I]");

            if (s_debugValues == null || s_debugValues.Count == 0)
            {
                GUILayout.EndArea();
                return;
            }

            int index = 0;
            _ = m_stringBuilder.Clear();
            foreach (KeyValuePair<string, string> keyValue in s_debugValues)
            {
                _ = m_stringBuilder.Append(keyValue.key);
                _ = m_stringBuilder.Append(": ");
                _ = m_stringBuilder.Append(keyValue.Value);
                if (index < s_debugValues.Count - 1)
                    _ = m_stringBuilder.Append("\n");

                index++;
            }
            _ = GUILayout.TextArea(m_stringBuilder.ToString());
            GUILayout.EndArea();
        }

        public override void Update()
        {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.D))
            {
                Enabled = !Enabled;
            }
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.I))
            {
                ModDebug.forceDisableCursor = false;
                GameUIRoot.Instance.RefreshCursorEnabled();
            }
        }
#endif

        public static void SetKeyValue(string key, string value)
        {
            if (!s_debugValues.ContainsKey(key))
            {
                s_debugValues.Add(key, value);
                return;
            }
            s_debugValues[key] = value;
        }
    }
}
