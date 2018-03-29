#if UNITY_EDITOR
//-----------------------------------------------------------------------Window/Odin Inspector
// <copyright file="OdinSceneValidatorWindow.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
    using UnityEngine;
    using UnityEditor;
    using System.Collections.Generic;
    using System.Linq;
    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;

    /// <summary>
    /// The Odin Scene Validator window.
    /// </summary>
    public class OdinSceneValidatorWindow : EditorWindow
    {
        private List<BehaviourValidationInfo> behaviourValidationInfos;
        private int errorCount;
        private bool isScanning = false;
        private float offsetLeftSide;
        private Vector2 scrollLeftSide;
        private Vector2 scrollRightRightSide;
        private BehaviourValidationInfo selectedValidationInfo;
        private BehaviourValidationInfo validationInfoToSelect;
        private bool triggerScan;
        private int validCount;
        private int warningCount;
        private bool includeValid = false;
        private bool includeErrors = true;
        private bool includeWarnings = true;

        /// <summary>
        /// Opens the window.
        /// </summary>
        [MenuItem("Tools/Odin Inspector/Scene Validator", priority = -15)]
        public static void OpenWindow()
        {
            var rect = GUIHelper.GetEditorWindowRect();
            var size = new Vector2(800, 600);
            var window = GetWindow<OdinSceneValidatorWindow>();
            window.Show();
            window.position = new Rect(rect.center - size * 0.5f, size);
            window.wantsMouseMove = true;
            window.titleContent = new GUIContent("Odin Scene Validator");
        }

        private void OnGUI()
        {
            if (this.behaviourValidationInfos == null)
            {
                this.FullScan();
            }

            if (Event.current.type == EventType.Layout)
            {
                if (this.validationInfoToSelect != null)
                {
                    this.selectedValidationInfo = this.validationInfoToSelect;
                    this.validationInfoToSelect = null;
                }
            }

            this.DrawToolbar();

            EditorGUILayout.BeginHorizontal();
            {
                var rect = EditorGUILayout.BeginVertical(GUILayoutOptions.Width(300 + this.offsetLeftSide));
                {
                    this.scrollLeftSide = EditorGUILayout.BeginScrollView(this.scrollLeftSide);
                    this.DrawHierachy();
                    EditorGUILayout.EndScrollView();
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical();
                {
                    SirenixEditorGUI.DrawSolidRect(GUIHelper.GetCurrentLayoutRect(), SirenixGUIStyles.DarkEditorBackground);
                    this.scrollRightRightSide = EditorGUILayout.BeginScrollView(this.scrollRightRightSide);
                    this.DrawPropertyTree();
                    EditorGUILayout.EndScrollView();
                }
                EditorGUILayout.EndVertical();

                rect.xMin = rect.xMax - 4;
                rect.x += 4;
                SirenixEditorGUI.DrawSolidRect(rect, SirenixGUIStyles.BorderColor);
                rect.xMin -= 2;
                rect.xMax += 2;
                this.offsetLeftSide = this.offsetLeftSide + SirenixEditorGUI.SlideRect(rect).x;
            }
            EditorGUILayout.EndHorizontal();

            if (this.isScanning && (Event.current.type == EventType.Repaint))
            {
                this.warningCount = 0;
                this.errorCount = 0;
                this.validCount = 0;

                for (int i = 0; i < this.behaviourValidationInfos.Count; i++)
                {
                    var o = this.behaviourValidationInfos[i];
                    if (o.ErrorCount == 0 && o.WarningCount == 0)
                    {
                        this.validCount++;
                    }
                    this.errorCount += o.ErrorCount;
                    this.warningCount += o.WarningCount;
                }
                this.behaviourValidationInfos = this.behaviourValidationInfos.OrderByDescending(x => x.ErrorCount).ThenByDescending(x => x.WarningCount).ThenBy(x => x.Name).ToList();
                this.isScanning = false;
            }
            else if (this.triggerScan && Event.current.type == EventType.Repaint)
            {
                this.isScanning = true;
                this.triggerScan = false;
                this.Repaint();
            }

            this.RepaintIfRequested();
        }

        private void DrawHierachy()
        {
            if (this.behaviourValidationInfos != null)
            {
                SirenixEditorGUI.BeginVerticalList();
                for (int i = 0; i < this.behaviourValidationInfos.Count; i++)
                {
                    this.behaviourValidationInfos[i].DrawMenu();
                }
                SirenixEditorGUI.EndVerticalList();
            }
        }

        private void DrawPropertyTree()
        {
            if (this.behaviourValidationInfos != null && this.behaviourValidationInfos.Count > 0)
            {
                if (this.isScanning)
                {
                    // Spamming the progress bar dialog a lot across several frames apparently crashes the editor on Mac OSX.
                    bool showProgress = Application.platform != RuntimePlatform.OSXEditor;

                    float total = this.behaviourValidationInfos.Count * 2;
                    float offset = Event.current.type == EventType.Layout ? 0 : this.behaviourValidationInfos.Count;
                    for (int i = 0; i < this.behaviourValidationInfos.Count; i++)
                    {
                        float t = (offset + i) / total;

                        if (showProgress)
                        {
                            EditorUtility.DisplayProgressBar("Scanning in " + Event.current.type, this.behaviourValidationInfos[i].Name, t);
                        }

                        this.behaviourValidationInfos[i].DrawPropertyTree();
                    }

                    if (showProgress)
                    {
                        EditorUtility.ClearProgressBar();
                    }
                }
                else
                {
                    if (this.selectedValidationInfo != null)
                    {
                        this.selectedValidationInfo.DrawPropertyTree();
                    }
                }
            }
        }

        private void DrawToolbar()
        {
            this.includeWarnings = EditorPrefs.GetBool("OdinValidation.includeWarnings", this.includeWarnings);
            this.includeErrors = EditorPrefs.GetBool("OdinValidation.includeErrors", this.includeErrors);
            this.includeValid = EditorPrefs.GetBool("OdinValidation.includeValid", this.includeValid);

            SirenixEditorGUI.BeginHorizontalToolbar();
            {
                //GL.sRGBWrite = QualitySettings.activeColorSpace == ColorSpace.Linear;
                //this.includeValid = SirenixEditorGUI.ToolbarButton(new GUIContent("      " + this.validCount + " "), this.includeValid) ? !this.includeValid : this.includeValid;
                this.includeValid = SirenixEditorGUI.ToolbarToggle(this.includeValid, new GUIContent("      " + this.validCount + " "));
                GUIHelper.PushColor(Color.green);
                GUI.DrawTexture(new Rect(GUILayoutUtility.GetLastRect().position + new Vector2(6, 4), new Vector2(16, 16)), EditorIcons.Checkmark.Highlighted, ScaleMode.ScaleToFit);
                GUIHelper.PopColor();

                //this.includeWarnings = SirenixEditorGUI.ToolbarButton(new GUIContent("      " + this.warningCount + " "), this.includeWarnings) ? !this.includeWarnings : this.includeWarnings;
                this.includeWarnings = SirenixEditorGUI.ToolbarToggle(this.includeWarnings, new GUIContent("      " + this.warningCount + " "));
                GUI.DrawTexture(new Rect(GUILayoutUtility.GetLastRect().position + Vector2.one * 2, new Vector2(20, 20)), EditorIcons.UnityWarningIcon, ScaleMode.ScaleToFit);

                //this.includeErrors = SirenixEditorGUI.ToolbarButton(new GUIContent("      " + this.errorCount + " "), this.includeErrors) ? !this.includeErrors : this.includeErrors;
                this.includeErrors = SirenixEditorGUI.ToolbarToggle(this.includeErrors, new GUIContent("      " + this.errorCount + " "));
                GUI.DrawTexture(new Rect(GUILayoutUtility.GetLastRect().position + Vector2.one * 2, new Vector2(22, 22)), EditorIcons.UnityErrorIcon, ScaleMode.ScaleToFit);
                //GL.sRGBWrite = false;

                GUILayout.FlexibleSpace();

                if (SirenixEditorGUI.ToolbarButton(GUIHelper.TempContent("  Scan Scene  ")))
                {
                    this.FullScan();
                }
            }
            SirenixEditorGUI.EndHorizontalToolbar();

            EditorPrefs.SetBool("OdinValidation.includeWarnings", this.includeWarnings);
            EditorPrefs.SetBool("OdinValidation.includeErrors", this.includeErrors);
            EditorPrefs.SetBool("OdinValidation.includeValid", this.includeValid);
        }

        private void FullScan()
        {
            Object prevSelected = this.selectedValidationInfo == null ? (Behaviour)null : this.selectedValidationInfo.Behaviour ?? (Object)this.selectedValidationInfo.GameObject;

            var drawingConfig = InspectorConfig.Instance.DrawingConfig;
            var odinEditorType = typeof(OdinEditor);

            this.behaviourValidationInfos = Resources.FindObjectsOfTypeAll<Transform>()
                .Where(x => (x.gameObject.scene.IsValid() && (x.gameObject.hideFlags & HideFlags.HideInHierarchy) == 0))
                .Select(x => new { go = x.gameObject, components = x.GetComponents(typeof(Behaviour)) })
                .SelectMany(x => x.components.Select(c => new { go = x.go, component = c }))
                .Where(x => x.component == null || drawingConfig.GetEditorType(x.component.GetType()) == odinEditorType)
                .OrderBy(x => x.go.name)
                .ThenBy(x => x.component == null ? "" : x.component.name)
                .Select(x => new { tree = x.component == null ? (PropertyTree)null : PropertyTree.Create(new SerializedObject(x.component)), go = x.go })
                .Examine(x => { if (x.tree != null) x.tree.UpdateTree(); })
                .Where(x => x.tree == null || x.tree.RootPropertyCount != 0)
                .Select((x, i) => new BehaviourValidationInfo(x.tree, this, x.go))
                .ToList();

            if (prevSelected != null)
            {
                var prev = this.behaviourValidationInfos.FirstOrDefault(x => x.Behaviour == prevSelected || x.GameObject == prevSelected);
                if (prev != null)
                {
                    prev.Select();
                }
            }

            this.triggerScan = true;
        }

        private void OnDisable()
        {
            this.behaviourValidationInfos = null;
        }

        private class BehaviourValidationInfo
        {
            private readonly PropertyTree propertyTree;
            private readonly OdinSceneValidatorWindow window;

            public readonly GameObject GameObject;
            public readonly Behaviour Behaviour;
            public readonly string Name;
            public int ErrorCount;
            public int WarningCount;

            public BehaviourValidationInfo(PropertyTree tree, OdinSceneValidatorWindow window, GameObject go)
            {
                if (tree != null)
                {
                    this.Behaviour = (Behaviour)tree.WeakTargets[0];
                }
                else
                {
                    this.Behaviour = null;
                }

                this.propertyTree = tree;
                this.window = window;
                this.GameObject = go;

                this.Name = "           " + this.GameObject.name + (this.Behaviour != null ? " - " + this.Behaviour.GetType().GetNiceName() : "");
            }

            public bool IsSelected
            {
                get { return this.window.selectedValidationInfo == this; }
            }

            public void Select()
            {
                this.window.validationInfoToSelect = this;
            }

            private bool IsIncluded
            {
                get
                {
                    return
                        this.window.includeErrors && this.ErrorCount > 0 ||
                        this.window.includeWarnings && this.WarningCount > 0 ||
                        this.window.includeValid && (this.WarningCount + this.ErrorCount) == 0;
                }
            }

            public void DrawMenu()
            {
                if (this.GameObject == null) { return; }

                if (!this.IsIncluded) { return; }

                GUIHelper.PushGUIEnabled(GUI.enabled && this.ErrorCount + this.WarningCount > 0);

                var rect = SirenixEditorGUI.BeginListItem(true);
                {
                    if (Event.current.rawType == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
                    {
                        if (this.IsSelected || this.Behaviour == null)
                        {
                            EditorGUIUtility.PingObject(this.GameObject);
                        }

                        this.Select();
                        GUIHelper.RequestRepaint();
                    }

                    if (this.IsSelected)
                    {
                        GUIHelper.PushGUIEnabled(true);
                        SirenixEditorGUI.DrawSolidRect(rect, SirenixGUIStyles.MenuButtonActiveBgColor);
                        GUIHelper.PushLabelColor(Color.white);
                        EditorGUILayout.LabelField(this.Name);
                        GUIHelper.PopLabelColor();
                        GUIHelper.PopGUIEnabled();
                    }
                    else
                    {
                        EditorGUILayout.LabelField(this.Name);
                    }
                    rect = new Rect(rect.position, new Vector2(20, 20));
                    rect.x += 6;

                    const float offAlpha = 0.1f;
                    var tmpColor = GUI.color;
                    GUI.color = this.WarningCount > 0 ? Color.white : new Color(1, 1, 1, offAlpha);
                    //GL.sRGBWrite = QualitySettings.activeColorSpace == ColorSpace.Linear;
                    GUI.DrawTexture(rect, EditorIcons.UnityWarningIcon);

                    rect.x += 20;
                    GUI.color = this.ErrorCount > 0 ? Color.white : new Color(1, 1, 1, offAlpha);
                    GUI.DrawTexture(rect, EditorIcons.UnityErrorIcon);

                    if (this.IsIncluded && this.ErrorCount == 0 && this.WarningCount == 0)
                    {
                        rect.x -= 10;
                        GUI.color = (this.ErrorCount + this.WarningCount) == 0 ? Color.green : new Color(0, 1, 0, offAlpha);
                        GUI.DrawTexture(rect, EditorIcons.Checkmark.Highlighted);
                    }
                    //GL.sRGBWrite = false;

                    GUI.color = tmpColor;
                }
                SirenixEditorGUI.EndListItem();

                GUIHelper.PopGUIEnabled();
            }

            public void DrawPropertyTree()
            {
                if (this.GameObject == null) return;

                if (this.window.isScanning && Event.current.type == EventType.Repaint)
                {
                    OdinInspectorValidationChecker.BeginValidationCheck();
                }

                GUILayout.BeginVertical(new GUIStyle() { padding = new RectOffset(10, 10, 6, 10) });
                {
                    if (this.propertyTree != null)
                    {
                        if (this.window.isScanning)
                        {
                            InspectorUtilities.BeginDrawPropertyTree(this.propertyTree, true);

                            foreach (var property in this.propertyTree.EnumerateTree(true))
                            {
                                try
                                {
                                    InspectorUtilities.DrawProperty(property);
                                }
                                catch (System.Exception ex)
                                {
                                    if (ex is ExitGUIException || ex.InnerException is ExitGUIException)
                                    {
                                        throw ex;
                                    }
                                    else
                                    {
                                        Debug.Log("The following exception was thrown when drawing property " + property.Path + ".");
                                        Debug.LogException(ex);
                                    }
                                }
                            }

                            InspectorUtilities.EndDrawPropertyTree(this.propertyTree);
                        }
                        else
                        {
                            this.propertyTree.Draw(true);
                        }
                    }
                    else
                    {
                        SirenixEditorGUI.ErrorMessageBox("Missing Reference.");
                    }
                }
                GUILayout.EndVertical();

                if (this.window.isScanning && Event.current.type == EventType.Repaint)
                {
                    // We can't count the correct the correct number of warnings and errors for each behavior
                    // until we have a proper way of drawing a property tree with the guarantee that every property will be drawn.
                    this.WarningCount = OdinInspectorValidationChecker.WarningMessages.Count();
                    this.ErrorCount = OdinInspectorValidationChecker.ErrorMessages.Count();

                    OdinInspectorValidationChecker.EndValidationCheck();
                }
            }
        }
    }
}
#endif