#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="InspectorConfig.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if ODIN_TRIAL_VERSION

namespace Sirenix.OdinInspector.Editor
{
    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;
    using System.IO;

    internal class OdinTrialVersionInfo
    {
        public static Vector2 PopupWindowSize = new Vector2(900, 500);
        public static bool IsExpired { get { return DateTime.Now > ExpirationDate; } }
        public static string TrialVersionName { get { return "Unite Austin 2017 Trial Version"; } }
        public static DateTime ExpirationDate { get { return new DateTime(2018, 01, 01); } }
        public static float TimeBetweenPopupsBeforeExpirationDate { get { return 60 * 60 * 24; } } // Every 24 hours (Disabled)
        public static float TimeBetweenPopupsAfterExpirationDate { get { return 60 * 60 * 2; } } // Every second hour
        public static float TimeBetweenPopupsOneMonthAfterExpirationDate { get { return 60 * 60 * 1; } } // Every hour

        public static string TrialVersionPopupURL
        {
            get
            {
                return "www.sirenix.net/tryodin/popup" +
                    "?numberOfTimesOpened=" + EditorPrefs.GetInt("ODIN_INSPECTOR_POPUP_COUNT", 0) +
                    "&isProSkin=" + EditorGUIUtility.isProSkin.ToString().ToLower() +
                    "&expirationDate=" + WWW.EscapeURL(ExpirationDate.ToShortDateString()) +
                    "&trialVersionName=" + WWW.EscapeURL(TrialVersionName);
            }
        }
    }

    internal class OdinTrialVersionPopupWindow : EditorWindow
    {
        private ScriptableObject webView;

        [InitializeOnLoadMethod]
        private static void OpenPopupIfNeeded()
        {
            DateTime lastShownPopup = DateTime.FromBinary(long.Parse(EditorPrefs.GetString("TimeOfLastDrawnOdinPopup", DateTime.MinValue.Ticks.ToString("D"))));
            TimeSpan popupFrequency;

            {
                float frequencySeconds;
                if (OdinTrialVersionInfo.IsExpired)
                {
                    if (DateTime.Now > OdinTrialVersionInfo.ExpirationDate.AddMonths(1))
                    {
                        frequencySeconds = OdinTrialVersionInfo.TimeBetweenPopupsOneMonthAfterExpirationDate;
                    }
                    else
                    {
                        frequencySeconds = OdinTrialVersionInfo.TimeBetweenPopupsAfterExpirationDate;
                    }
                }
                else
                {
                    return;
                    //frequencySeconds = OdinTrialVersionInfo.TimeBetweenPopupsBeforeExpirationDate;
                }

                popupFrequency = TimeSpan.FromSeconds(frequencySeconds);
            }

            EditorApplication.update -= OpenPopupIfNeeded;
            EditorApplication.update += OpenPopupIfNeeded;

            int currProcessId = System.Diagnostics.Process.GetCurrentProcess().Id;
            bool isNewUnityEditorInstance = EditorPrefs.GetInt("LAST_UNITY_PROCESS_ID", -1) != currProcessId;

            if (lastShownPopup + popupFrequency <= DateTime.Now || isNewUnityEditorInstance)
            {
                EditorApplication.delayCall += () =>
                {
                    EditorPrefs.SetInt("LAST_UNITY_PROCESS_ID", currProcessId);
                    EditorPrefs.SetString("TimeOfLastDrawnOdinPopup", DateTime.Now.Ticks.ToString("D"));
                    var rect = GUIHelper.GetEditorWindowRect().AlignCenter(OdinTrialVersionInfo.PopupWindowSize.x).AlignMiddle(OdinTrialVersionInfo.PopupWindowSize.y);
                    var window = GetWindowWithRect<OdinTrialVersionPopupWindow>(rect, true, "Trial Version Popup: " + OdinTrialVersionInfo.TrialVersionName);
                    window.ShowUtility();
                    window.OpenWebView();
                };
            }
        }

        private void OpenWebView()
        {
            if (this.webView == null)
            {
                EditorPrefs.SetInt("ODIN_INSPECTOR_POPUP_COUNT", EditorPrefs.GetInt("ODIN_INSPECTOR_POPUP_COUNT", 0) + 1);
                var thisWindowGuiView = typeof(EditorWindow).GetField("m_Parent", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this);
                Type webViewType = GetTypeFromAllAssemblies("WebView");
                this.webView = ScriptableObject.CreateInstance(webViewType);
                Rect webViewRect = new Rect(0, 0, OdinTrialVersionInfo.PopupWindowSize.x, OdinTrialVersionInfo.PopupWindowSize.y);
                webViewType.GetMethod("InitWebView").Invoke(this.webView, new object[] { thisWindowGuiView, (int)webViewRect.x, (int)webViewRect.y, (int)webViewRect.width, (int)webViewRect.height, true });
                webViewType.GetMethod("LoadURL").Invoke(this.webView, new object[] { OdinTrialVersionInfo.TrialVersionPopupURL });
            }
        }

        private void OnGUI()
        {
            if (this.webView == null)
            {
                this.OpenWebView();
            }

            if (this.webView == null && Event.current.type == EventType.Repaint)
            {
                this.OpenWebView();
            }
        }

        private void OnDisable()
        {
            this.OnDestroy();
        }

        private void OnDestroy()
        {
            if (this.webView != null)
            {
                DestroyImmediate(this.webView);
                this.webView = null;
            }
        }

        public static Type GetTypeFromAllAssemblies(string typeName)
        {
            // It's from github - don't judge.
            Assembly[] assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                Type[] types = assembly.GetTypes();
                foreach (Type type in types)
                {
                    if (type.Name.Equals(typeName, StringComparison.CurrentCultureIgnoreCase) || type.Name.Contains('+' + typeName)) //+ check for inline classes
                        return type;
                }
            }
            return null;
        }
    }
}

#endif
#endif