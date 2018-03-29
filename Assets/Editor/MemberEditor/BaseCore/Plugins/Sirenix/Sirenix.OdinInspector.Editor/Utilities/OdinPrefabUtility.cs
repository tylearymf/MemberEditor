#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="OdinPrefabUtility.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
    using Sirenix.Serialization;
    using Sirenix.Utilities.Editor;
    using System;
    using UnityEditor;
    using UnityEngine;

    public static class OdinPrefabUtility
    {
        public static void UpdatePrefabInstancePropertyModifications(UnityEngine.Object prefabInstance, bool withUndo)
        {
            var prefabType = PrefabUtility.GetPrefabType(prefabInstance);

            //Assert.IsTrue(prefabInstance != null, "Argument is null");
            //Assert.IsTrue(prefabInstance is ISupportsPrefabSerialization, "Type must implement ISupportsPrefabSerialization");
            //Assert.IsTrue(prefabInstance is ISerializationCallbackReceiver, "Type must implement ISerializationCallbackReceiver");
            //Assert.IsTrue(prefabType == PrefabType.PrefabInstance || prefabType == PrefabType.ModelPrefabInstance, "Value must be a prefab instance");

            if (prefabInstance == null) throw new ArgumentNullException("prefabInstance");
            if (!(prefabInstance is ISupportsPrefabSerialization)) throw new ArgumentException("Type must implement ISupportsPrefabSerialization");
            if (!(prefabInstance is ISerializationCallbackReceiver)) throw new ArgumentException("Type must implement ISerializationCallbackReceiver");
            if (!(prefabType == PrefabType.PrefabInstance || prefabType == PrefabType.ModelPrefabInstance)) throw new ArgumentException("Value must be a prefab instance");

            Action action = null;

            EditorApplication.HierarchyWindowItemCallback hierarchyCallback = (arg1, arg2) => action();
            EditorApplication.ProjectWindowItemCallback projectCallback = (arg1, arg2) => action();
            SceneView.OnSceneFunc sceneCallback = (arg) => action();

            EditorApplication.hierarchyWindowItemOnGUI += hierarchyCallback;
            EditorApplication.projectWindowItemOnGUI += projectCallback;
            SceneView.onSceneGUIDelegate += sceneCallback;

            action = () =>
            {
                EditorApplication.hierarchyWindowItemOnGUI -= hierarchyCallback;
                EditorApplication.projectWindowItemOnGUI -= projectCallback;
                SceneView.onSceneGUIDelegate -= sceneCallback;

                try
                {
                    //Assert.IsTrue(prefabInstance != null, "Invalid Argument: The prefab instance to update modifications for has been destroyed before updating could start.");
                    //Assert.IsNotNull(Event.current, "Can only be called during the GUI event loop; Event.current must be accessible.");

                    //if (prefabInstance == null) throw new InvalidOperationException("The prefab instance to update modifications for has been destroyed before updating could start.");

                    if (prefabInstance == null)
                    {
                        // Ignore - the object has been destroyed since the method was invoked.
                        return;
                    }

                    if (Event.current == null) throw new InvalidOperationException("Delayed property modification delegate can only be called during the GUI event loop; Event.current must be accessible.");

                    try
                    {
                        PrefabUtility.RecordPrefabInstancePropertyModifications(prefabInstance);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("Exception occurred while calling Unity's PrefabUtility.RecordPrefabInstancePropertyModifications:");
                        Debug.LogException(ex);
                    }

                    var tree = PropertyTree.Create(prefabInstance);

                    //if (tree == null) Debug.LogError("Tree is null");

                    tree.DrawMonoScriptObjectField = false;

                    bool isRepaint = Event.current.type == EventType.Repaint;

                    if (!isRepaint)
                    {
                        GUIHelper.PushEventType(EventType.Repaint);
                    }

                    InspectorUtilities.BeginDrawPropertyTree(tree, withUndo);

                    foreach (var property in tree.EnumerateTree(true))
                    {
                        //if (property == null) Debug.LogError("Property is null");

                        if (property.ValueEntry == null) continue;
                        if (property.ValueEntry.GetDictionaryHandler() == null) continue;

                        if (property.ValueEntry.DictionaryChangedFromPrefab)
                        {
                            tree.RegisterPrefabDictionaryDeltaModification(property, 0);
                        }
                        else
                        {
                            var prefabProperty = tree.PrefabPropertyTree.GetPropertyAtPath(property.Path);

                            if (prefabProperty == null) continue;
                            if (prefabProperty.ValueEntry == null) continue;
                            if (prefabProperty.ValueEntry.GetDictionaryHandler() == null) continue;

                            //if (property.Children == null) Debug.LogError("Property children is null");
                            //if (prefabProperty.Children == null) Debug.LogError("Prefab property children is null");

                            if (property.Children.Count != prefabProperty.Children.Count)
                            {
                                tree.RegisterPrefabDictionaryDeltaModification(property, 0);
                            }
                        }
                    }

                    InspectorUtilities.EndDrawPropertyTree(tree);

                    if (!isRepaint)
                    {
                        GUIHelper.PopEventType();
                    }

                    ISerializationCallbackReceiver receiver = (prefabInstance as ISerializationCallbackReceiver);
                    if (receiver == null) Debug.LogError("Receiver is null");
                    receiver.OnBeforeSerialize();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            };

            foreach (SceneView scene in SceneView.sceneViews)
            {
                scene.Repaint();
            }
        }
    }
}
#endif