using OverhaulMod.Utils;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace OverhaulMod.UI
{
    public class UIDeveloperMenu : ModBehaviour
    {
        private static readonly Dictionary<string, string> s_debugValues = new Dictionary<string, string>();
        private StringBuilder m_stringBuilder;

        public static bool Enabled;

        public override void Start()
        {
            m_stringBuilder = new StringBuilder();
        }

        public void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10f, 100f, 200f, 200f));
            if (s_debugValues == null || s_debugValues.Count == 0)
            {
                GUILayout.EndArea();
                return;
            }

            int index = 0;
            _ = m_stringBuilder.Clear();
            foreach (KeyValuePair<string, string> keyValue in s_debugValues)
            {
                _ = m_stringBuilder.Append(keyValue.Key);
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
            if (ModBuildInfo.debug && Input.GetKeyDown(KeyCode.Alpha7) && InputManager.Instance.GetKeyMode() != KeyMode.EditingInputField)
            {
                bool value = !Enabled;
                Enabled = value;
                if (value)
                {
                    if (!Cursor.visible || GameModeManager.IsOnTitleScreen())
                        _ = ModUIConstants.ShowDebugMenu();
                }
                else
                {
                    ModUIConstants.HideDebugMenu();
                }
            }
        }

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
