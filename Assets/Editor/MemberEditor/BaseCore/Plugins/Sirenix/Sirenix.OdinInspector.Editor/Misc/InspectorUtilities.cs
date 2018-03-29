#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="InspectorUtilities.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY_5_3 && !UNITY_5_3_OR_NEWER
#define UNITY_5_3_OR_NEWER
#endif

namespace Sirenix.OdinInspector.Editor
{
    using System;
    using System.Linq;
    using System.Text;
    using Utilities;
    using Utilities.Editor;
    using UnityEditor;
    using UnityEngine;
    using Sirenix.OdinInspector.Editor.Drawers;

    /// <summary>
    /// Provides a variety of miscellaneous utilities widely used in the inspector.
    /// </summary>
    public static class InspectorUtilities
    {
        static InspectorUtilities()
        {
            var mPropertyField = typeof(SerializedObject).GetField("m_Property", Flags.InstanceAnyVisibility);

            if (mPropertyField != null)
            {
                mPropertyFieldGetter = EmitUtilities.CreateInstanceFieldGetter<SerializedObject, IntPtr>(mPropertyField);
            }
            else
            {
                Debug.LogWarning("The internal Unity field SerializedObject.m_Property has been renamed in this version of Unity!");
            }

        }

        private static ValueGetter<SerializedObject, IntPtr> mPropertyFieldGetter;
        private static GUIFrameCounter frameCounter = new GUIFrameCounter();
        private static int drawnInspectorDepthCount = 0;

        /// <summary>
        /// Converts an Odin property path to a deep reflection path.
        /// </summary>
        public static string ConvertToDeepReflectionPath(string odinPropertyPath)
        {
            bool hasArrayElement = false;

            for (int i = 0; i < odinPropertyPath.Length; i++)
            {
                if (odinPropertyPath[i] == '$')
                {
                    hasArrayElement = true;
                    break;
                }
            }

            if (hasArrayElement)
            {
                StringBuilder sb = new StringBuilder(odinPropertyPath.Replace("$", "["));

                for (int i = 0; i < sb.Length; i++)
                {
                    if (sb[i] == '[')
                    {
                        // Array index number starts at i + 1

                        do
                        {
                            i++;
                        }
                        while (i < sb.Length && char.IsNumber(sb[i]));

                        // Insert ']' char after array number
                        sb.Insert(i, ']');
                    }
                }

                return sb.ToString();
            }
            else
            {
                return odinPropertyPath;
            }
        }

        /// <summary>
        /// Converts an Odin property path into a Unity property path.
        /// </summary>
        public static string ConvertToUnityPropertyPath(string odinPropertyPath)
        {
            bool hasArrayElement = false;

            for (int i = 0; i < odinPropertyPath.Length; i++)
            {
                if (odinPropertyPath[i] == '$')
                {
                    hasArrayElement = true;
                    break;
                }
            }

            if (hasArrayElement)
            {
                StringBuilder sb = new StringBuilder(odinPropertyPath.Replace("$", "Array.data["));

                for (int i = 0; i < sb.Length; i++)
                {
                    if (sb[i] == '[')
                    {
                        // Array index number starts at i + 1

                        do
                        {
                            i++;
                        }
                        while (i < sb.Length && char.IsNumber(sb[i]));

                        // Insert ']' char after array number
                        sb.Insert(i, ']');
                    }
                }

                return sb.ToString();
            }
            else
            {
                return odinPropertyPath;
            }
        }

        /// <summary>
        /// Converts a Unity property path into an Odin property path.
        /// </summary>
        public static string ConvertToSirenixPropertyPath(string unityPropertyPath)
        {
            bool hasArrayElement = false;

            for (int i = 0; i < unityPropertyPath.Length; i++)
            {
                if (unityPropertyPath[i] == '[')
                {
                    hasArrayElement = true;
                    break;
                }
            }

            if (hasArrayElement)
            {
                return unityPropertyPath.Replace("Array.data[", "$").Replace("]", "");
            }
            else
            {
                return unityPropertyPath;
            }
        }

        /// <summary>
        /// Prepares a property tree for drawing, and handles management of undo, as well as marking scenes and drawn assets dirty.
        /// </summary>
        /// <param name="tree">The tree to be drawn.</param>
        /// <param name="withUndo">Whether to register undo commands for the changes made to the tree. This can only be set to true if the tree has a <see cref="SerializedObject"/> to represent.</param>
        /// <exception cref="System.ArgumentNullException">tree is null</exception>
        public static void BeginDrawPropertyTree(PropertyTree tree, bool withUndo)
        {
            if (frameCounter.Update().IsNewFrame)
            {
                drawnInspectorDepthCount = 0;
            }
            drawnInspectorDepthCount++;

            if (tree == null)
            {
                throw new ArgumentNullException("tree");
            }

            for (int i = 0; i < tree.WeakTargets.Count; i++)
            {
                if (tree.WeakTargets[i] == null)
                {
                    GUILayout.Label("An inspected object has been destroyed; please refresh the inspector.");
                    return;
                }
            }

            if (tree.UnitySerializedObject != null)
            {
                tree.UnitySerializedObject.Update();
            }

            tree.UpdateTree();

            tree.WillUndo = false;

            if (withUndo)
            {
                if (tree.TargetType.ImplementsOrInherits(typeof(UnityEngine.Object)) == false)
                {
                    Debug.LogError("Automatic inspector undo only works when you're inspecting a type derived from UnityEngine.Object, and you are inspecting '" + tree.TargetType.GetNiceName() + "'.");
                }
                else
                {
                    tree.WillUndo = true;
                }
            }

            if (tree.WillUndo)
            {
                for (int i = 0; i < tree.WeakTargets.Count; i++)
                {
                    Undo.RecordObject((UnityEngine.Object)tree.WeakTargets[i], "Sirenix Inspector value change");
                }
            }

            if (tree.DrawMonoScriptObjectField)
            {
                var scriptProp = tree.UnitySerializedObject.FindProperty("m_Script");

                if (scriptProp != null)
                {
                    GUIHelper.PushGUIEnabled(false);
                    EditorGUILayout.PropertyField(scriptProp);
                    GUIHelper.PopGUIEnabled();
                }
            }
        }

        /// <summary>
        /// Ends drawing a property tree, and handles management of undo, as well as marking scenes and drawn assets dirty.
        /// </summary>
        /// <param name="tree">The tree.</param>
        public static void EndDrawPropertyTree(PropertyTree tree)
        {

            tree.InvokeDelayedActions();

            if (tree.UnitySerializedObject != null)
            {
                if (mPropertyFieldGetter != null)
                {
                    var obj = tree.UnitySerializedObject;
                    IntPtr ptr = mPropertyFieldGetter(ref obj);

                    if (ptr == IntPtr.Zero)
                    {
                        // SerializedObject has been disposed, likely due to a scene change invoked from GUI code.
                        // BAIL THE FUCK OUT! :D Crashes will happen.
                        return;
                    }
                }

                if (tree.WillUndo)
                {
                    tree.UnitySerializedObject.ApplyModifiedProperties();
                }
                else
                {
                    tree.UnitySerializedObject.ApplyModifiedPropertiesWithoutUndo();
                }
            }

            bool appliedOdinChanges = false;

            if (tree.ApplyChanges())
            {
                appliedOdinChanges = true;
                GUIHelper.RequestRepaint();

                if (tree.TargetType.ImplementsOrInherits(typeof(UnityEngine.Object)))
                {
                    var targets = tree.WeakTargets;

                    for (int i = 0; i < targets.Count; i++)
                    {
                        var target = (UnityEngine.Object)targets[i];

                        if (AssetDatabase.Contains(target))
                        {
                            EditorUtility.SetDirty(target);
                        }
                        else if (Application.isPlaying == false)
                        {
#if UNITY_5_3_OR_NEWER
                            if (tree.TargetType.ImplementsOrInherits(typeof(Component)))
                            {
                                Component component = (Component)target;
                                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(component.gameObject.scene);
                            }
                            else
                            {
                                // We can't find out where this thing is from
                                // It is probably a "temporary" UnityObject created from a script somewhere
                                // Just to be safe, mark it as dirty, and mark all scenes as dirty

                                EditorUtility.SetDirty(target);
                                UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
                            }
#else
                            EditorApplication.MarkSceneDirty();
#endif
                        }
                    }
                }
            }

            // This is very important, as applying changes may cause more actions to be delayed
            tree.InvokeDelayedActions();

            if (appliedOdinChanges)
            {
                tree.InvokeOnValidate();
            }

            drawnInspectorDepthCount--;

#if ODIN_TRIAL_VERSION
            if (drawnInspectorDepthCount == 0)
            {
                float height = OdinTrialVersionInfo.IsExpired ? 22 : 17;
                var rect = GUILayoutUtility.GetRect(16, height, GUILayoutOptions.ExpandWidth().Height(height));

                var bgRect = rect;
                bgRect.xMin -= 20;
                bgRect.xMax += 20;
                bgRect.y += 2;
                SirenixEditorGUI.DrawBorders(bgRect, 0, 0, 1, 0, SirenixGUIStyles.LightBorderColor);

                rect.y += 2;
                if (OdinTrialVersionInfo.IsExpired)
                {
                    EditorGUI.DrawRect(bgRect, Color.black);
                    GUIHelper.PushContentColor(Color.red);

                    GUI.Label(rect.AddY(3), "Odin Inspector Trial expired!", SirenixGUIStyles.Label);
                    GUIHelper.PopContentColor();
                    var btnRect = rect.AlignRight(EditorStyles.miniButton.CalcSize(new GUIContent("Purchase Odin Inspector")).x);
                    btnRect.yMin += 2;
                    btnRect.yMax -= 2;
                    GUIHelper.PushColor(Color.green);
                    if (GUI.Button(btnRect, "Purchase Odin Inspector", EditorStyles.miniButton))
                    {
                        UnityEditorInternal.AssetStore.Open("content/89041");
                    }
                    GUIHelper.PopColor();
                }
                else
                {
                    GUI.Label(rect, "Odin Inspector Trial Version", SirenixGUIStyles.LeftAlignedGreyMiniLabel);
                    GUI.Label(rect, "Expires " + OdinTrialVersionInfo.ExpirationDate.ToShortDateString(), EditorStyles.centeredGreyMiniLabel);
                    GUI.Label(rect, OdinTrialVersionInfo.ExpirationDate.Subtract(System.DateTime.Now).TotalHours.ToString("F2") + " hours remaining.", SirenixGUIStyles.RightAlignedGreyMiniLabel);
                }
            }
#endif
        }

        /// <summary>
        /// Draws all properties in a given property tree; must be wrapped by a <see cref="BeginDrawPropertyTree(PropertyTree, bool)"/> and <see cref="EndDrawPropertyTree(PropertyTree)"/>.
        /// </summary>
        /// <param name="tree">The tree to be drawn.</param>
        public static void DrawPropertiesInTree(PropertyTree tree)
        {
            foreach (var property in tree.EnumerateTree(false))
            {
                try
                {
                    InspectorUtilities.DrawProperty(property);
                }
                catch (Exception ex)
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
        }

        /// <summary>
        /// Draws a property in the inspector.
        /// </summary>
        public static void DrawProperty(InspectorProperty property)
        {
            DrawProperty(property, property.Label);
        }

        /// <summary>
        /// Draws a property in the inspector using a given label.
        /// </summary>
        public static void DrawProperty(InspectorProperty property, GUIContent label)
        {
            if (property == null)
            {
                throw new ArgumentNullException("property");
            }

            bool popGUIEnabled = false;

            try
            {
                property.PushDraw();

                if ((property.RecursiveDrawDepth + InlineEditorAttributeDrawer.CurrentInlineEditorDrawDepth) > GeneralDrawerConfig.Instance.MaxRecursiveDrawDepth)
                {
                    SirenixEditorGUI.ErrorMessageBox("The property '" + property.NiceName + "' has exceeded the maximum recursive draw depth limit of " + GeneralDrawerConfig.Instance.MaxRecursiveDrawDepth + ".");
                    return;
                }

                if (property.ValueEntry != null && !property.SupportsPrefabModifications && property.Tree.HasPrefabs && !GUIHelper.IsDrawingDictionaryKey && (property.Info.PropertyType == PropertyType.ReferenceType || property.Info.PropertyType == PropertyType.ValueType))
                {
                    if ((property.Parent == null || property.Parent.SupportsPrefabModifications) && GeneralDrawerConfig.Instance.ShowPrefabModificationsDisabledMessage)
                    {
                        SirenixEditorGUI.InfoMessageBox("The property '" + property.NiceName + "' does not support being modified on prefab instances. (You can disable this message the general drawer config.)");
                    }

                    GUIHelper.PushGUIEnabled(false);
                    popGUIEnabled = true;
                }

                OdinDrawer[] drawers = DrawerLocator.GetDrawersForProperty(property);

                GUIHelper.BeginLayoutMeasuring();
                {
                    try
                    {
                        if (drawers.Length > 0)
                        {
                            bool setIsBoldState = property.ValueEntry != null && Event.current.type == EventType.Repaint;
                            if (setIsBoldState)
                            {
                                var boldState = property.ValueEntry.ValueChangedFromPrefab;

                                if (GUIHelper.IsDrawingDictionaryKey)
                                {
                                    // Always propagate changed state down through dictionary keys
                                    boldState |= GUIHelper.IsBoldLabel;
                                }

                                GUIHelper.PushIsBoldLabel(boldState);
                            }
                            drawers[0].DrawProperty(property, label);
                            if (setIsBoldState) GUIHelper.PopIsBoldLabel();
                        }
                        else
                        {
                            if (property.Info.PropertyType == PropertyType.Method)
                            {
                                EditorGUILayout.LabelField(property.NiceName, "No drawers could be found for the method property '" + property.Name + "' with signature '" + property.Info.MemberInfo.GetNiceName() + "'.");
                            }
                            else if (property.Info.PropertyType == PropertyType.Group)
                            {
                                var attr = property.Info.GetAttribute<PropertyGroupAttribute>();

                                if (attr != null)
                                {
                                    EditorGUILayout.LabelField(property.NiceName, "No drawers could be found for the property group '" + property.Name + "' with property group attribute type '" + attr.GetType().GetNiceName() + "'.");
                                }
                                else
                                {
                                    EditorGUILayout.LabelField(property.NiceName, "No drawers could be found for the property group '" + property.Name + "'.");
                                }
                            }
                            //else if (property.Info.GetAttribute<HideInInspector>() == null)
                            //{
                            //    EditorGUILayout.LabelField(property.NiceName, "No drawers could be found for the value property '" + property.Name + "' of type '" + property.ValueEntry.TypeOfValue.GetNiceName() + "'.");
                            //}
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex is ExitGUIException || ex.InnerException is ExitGUIException)
                        {
                            throw ex;
                        }
                        else
                        {
                            var msg =
                                "This error occurred while being drawn by Odin. \n" +
                                "Odin Property Path: " + property.Path + "\n" +
                                "Odin Drawer Chain: " + string.Join(", ", drawers.Select(n => n.GetType().GetNiceName()).ToArray()) + ".";

                            Debug.LogException(new OdinPropertyException(msg, ex));
                        }
                    }
                }
                if (Event.current.type != EventType.Layout)
                {
                    property.LastDrawnValueRect = GUIHelper.EndLayoutMeasuring();
                }
            }
            finally
            {
                property.PopDraw();

                if (popGUIEnabled)
                {
                    GUIHelper.PopGUIEnabled();
                }
            }
        }

        private class OdinPropertyException : Exception
        {
            public OdinPropertyException(string message, Exception ex)
                : base(message, ex)
            {

            }
        }
    }
}
#endif