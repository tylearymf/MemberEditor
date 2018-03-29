#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="OdinInspectorPreferences.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
    using Sirenix.Utilities.Editor;
    using Sirenix.Utilities;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Adds menu items to the Unity Editor, draws the About window, and the preference window found under Edit > Preferences > Odin Inspector.
    /// </summary>
    public class OdinInspectorAboutWindow : EditorWindow
    {
        [MenuItem("Tools/Odin Inspector/About", priority = 5)]
        private static void ShowAboutOdinInspector()
        {
#if ODIN_TRIAL_VERSION
            var rect = GUIHelper.GetEditorWindowRect().AlignCenter(465f).AlignMiddle(175f);
#else
            var rect = GUIHelper.GetEditorWindowRect().AlignCenter(465f).AlignMiddle(135f);
#endif
            var w = GetWindowWithRect<OdinInspectorAboutWindow>(rect, true, "Odin Inspector & Serializer");
            w.ShowUtility();
        }

#if ODIN_TRIAL_VERSION
		[MenuItem("Tools/Odin Inspector/Get it here", priority = 8)]
		private static void OpenStoreLink()
		{
			Application.OpenURL("https://www.assetstore.unity3d.com/en/#!/content/89041");
		}
#endif

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10f, 10f, this.position.width - 20f, this.position.height - 5f));
#if ODIN_TRIAL_VERSION
            SirenixEditorGUI.Title(
                "Odin Inspector & Serializer", 
                OdinTrialVersionInfo.TrialVersionName, 
                TextAlignment.Left, 
                true);
#else
            SirenixEditorGUI.Title("Odin Inspector & Serializer", null, TextAlignment.Left, true);
#endif
            DrawAboutGUI();
            GUILayout.EndArea();
            this.RepaintIfRequested();
        }

        [PreferenceItem("Odin Inspector")]
        private static void OnPreferencesGUI()
        {
            DrawAboutGUI();
            Rect rect = EditorGUILayout.GetControlRect();

#if ODIN_TRIAL_VERSION
            rect.y += 25;
#endif

            if (GUI.Button(new Rect(rect) { y = rect.y + 70f, height = 25f, }, "Show Odin Preferences"))
            {
                SirenixPreferencesWindow.OpenSirenixPreferences();
            }

            GUIHelper.RepaintIfRequested(GUIHelper.CurrentWindow);
        }

        internal static void DrawAboutGUI()
        {
#if ODIN_TRIAL_VERSION
            Rect position = new Rect(EditorGUILayout.GetControlRect()) { height = 110f };
#else
            Rect position = new Rect(EditorGUILayout.GetControlRect()) { height = 90f };
#endif

            // Logo
            GUI.DrawTexture(position.SetWidth(86).SetHeight(75).AddY(4).AddX(-5), EditorIcons.OdinInspectorLogo, ScaleMode.ScaleAndCrop);

            // About
#if ODIN_TRIAL_VERSION
            string version = "Version: " + typeof(InspectorConfig).Assembly.GetName(false).Version.ToString() + " (Trial)";

#else
            string version = "Version: " + typeof(InspectorConfig).Assembly.GetName(false).Version.ToString();
#endif

            GUI.Label(new Rect(position) { x = position.x + 82f, y = position.y + 20f * 0f - 2f, height = 18f, }, version, SirenixGUIStyles.LeftAlignedGreyMiniLabel);
            //DrawLink(new Rect(position) { x = position.x + 82f, y = position.y + 20f * 1f - 2f, height = 18f, }, "Developed by Sirenix", "http://www.sirenix.net", SirenixGUIStyles.LeftAlignedGreyMiniLabel);
            //DrawLink(new Rect(position) { x = position.x + 82f, y = position.y + 20f * 2f - 2f, height = 18f, }, "Published by DevDog", "http://www.devdog.io", SirenixGUIStyles.LeftAlignedGreyMiniLabel);
            GUI.Label(new Rect(position) { x = position.x + 82f, y = position.y + 20f * 1f - 2f, height = 18f, }, "Developed by Sirenix", SirenixGUIStyles.LeftAlignedGreyMiniLabel);
            GUI.Label(new Rect(position) { x = position.x + 82f, y = position.y + 20f * 2f - 2f, height = 18f, }, "Published by DevDog", SirenixGUIStyles.LeftAlignedGreyMiniLabel);
            GUI.Label(new Rect(position) { x = position.x + 82f, y = position.y + 20f * 3f - 2f, height = 18f, }, "All rights reserved", SirenixGUIStyles.LeftAlignedGreyMiniLabel);
            //""
            // Links
            DrawLink(new Rect(position) { x = position.xMax - 95f, y = position.y + 20f * 0f, width = 95f, height = 14f, }, "Manuals", "http://sirenix.net/odininspector/manual/introduction/getting-started", EditorStyles.miniButton);
            DrawLink(new Rect(position) { x = position.xMax - 95f, y = position.y + 20f * 1f, width = 95f, height = 14f, }, "Documentation", "http://sirenix.net/odininspector/documentation", EditorStyles.miniButton);
            DrawLink(new Rect(position) { x = position.xMax - 95f, y = position.y + 20f * 2f, width = 95f, height = 14f, }, "Forums", "https://forum.unity3d.com/threads/wip-odin-inspector-serializer-looking-for-feedback.457670/", EditorStyles.miniButton);
            DrawLink(new Rect(position) { x = position.xMax - 95f, y = position.y + 20f * 3f, width = 95f, height = 14f, }, "Issue tracker", "https://bitbucket.org/sirenix/odin-inspector/issues?status=new&status=open", EditorStyles.miniButton);

#if ODIN_TRIAL_VERSION
            if (GUI.Button(position.AlignBottom(20), " Purchase Odin Inspector & Serializer"))
            {
                Application.OpenURL("https://www.assetstore.unity3d.com/en/#!/content/89041");
            }
#endif

        }

        private static void DrawLink(Rect rect, string label, string link, GUIStyle style)
        {
            //EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
            if (GUI.Button(rect, label, style))
            {
                Application.OpenURL(link);
            }
        }
    }
}
#endif