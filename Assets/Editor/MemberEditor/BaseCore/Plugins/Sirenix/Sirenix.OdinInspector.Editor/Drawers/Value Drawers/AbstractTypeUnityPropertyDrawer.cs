#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="AbstractTypeUnityPropertyDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;
    using Utilities;
    using Utilities.Editor;

    /// <summary>
    /// Unity property drawer for abstract types.
    /// </summary>
    [DrawerPriority(0, 0, 0.5)]
    public sealed class AbstractTypeUnityPropertyDrawer<TDrawer, TDrawnType, T> : OdinValueDrawer<T>
        where TDrawer : UnityEditor.PropertyDrawer, new()
        where T : TDrawnType
    {
        private static readonly FieldInfo InternalFieldInfoFieldInfo = typeof(TDrawer).GetField("m_FieldInfo", Flags.InstanceAnyVisibility);
        private static readonly ValueSetter<TDrawer, FieldInfo> SetFieldInfo;

        private TDrawer drawer;
        private object propertyHandler;

        /// <summary>
        /// Initializes the property drawer.
        /// </summary>
        public AbstractTypeUnityPropertyDrawer()
        {
            this.drawer = new TDrawer();

            if (UnityPropertyHandlerUtility.IsAvailable)
            {
                this.propertyHandler = UnityPropertyHandlerUtility.CreatePropertyHandler(this.drawer);
            }
        }

        static AbstractTypeUnityPropertyDrawer()
        {
            if (InternalFieldInfoFieldInfo == null)
            {
                Debug.LogError("Could not find the internal Unity field 'PropertyDrawer.m_FieldInfo'; UnityPropertyDrawer alias '" + typeof(AbstractTypeUnityPropertyDrawer<TDrawer, TDrawnType, T>).GetNiceName() + "' has been disabled.");
            }
            else
            {
                SetFieldInfo = EmitUtilities.CreateInstanceFieldSetter<TDrawer, FieldInfo>(InternalFieldInfoFieldInfo);
            }
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<T> entry, GUIContent label)
        {
            if (SetFieldInfo == null)
            {
                SirenixEditorGUI.ErrorMessageBox("Could not find the internal Unity field 'PropertyDrawer.m_FieldInfo'; UnityPropertyDrawer alias '" + typeof(AbstractTypeUnityPropertyDrawer<TDrawer, TDrawnType, T>).GetNiceName() + "' has been disabled.");
                return;
            }

            FieldInfo fieldInfo;
            SerializedProperty unityProperty = entry.Property.Tree.GetUnityPropertyForPath(entry.Property.Path, out fieldInfo);

            if (unityProperty == null)
            {
                if (UnityVersion.IsVersionOrGreater(2017, 1))
                {
                    this.CallNextDrawer(entry, label);
                }
                else
                {
                    SirenixEditorGUI.ErrorMessageBox("Could not get a Unity SerializedProperty for the property '" + entry.Property.NiceName + "' of type '" + entry.TypeOfValue.GetNiceName() + "' at path '" + entry.Property.Path + "'.");
                }

                return;
            }

            SetFieldInfo(ref this.drawer, fieldInfo);

            if (unityProperty.serializedObject.targetObject is EmittedScriptableObject<T>)
            {
                var targetObjects = unityProperty.serializedObject.targetObjects;

                for (int i = 0; i < targetObjects.Length; i++)
                {
                    EmittedScriptableObject<T> target = (EmittedScriptableObject<T>)targetObjects[i];
                    target.SetValue(entry.Values[i]);
                }

                unityProperty.serializedObject.Update();
            }
            else if (unityProperty.serializedObject.targetObject is EmittedScriptableObject)
            {
                var targetObjects = unityProperty.serializedObject.targetObjects;

                for (int i = 0; i < targetObjects.Length; i++)
                {
                    EmittedScriptableObject target = (EmittedScriptableObject)targetObjects[i];
                    target.SetWeakValue(entry.Values[i]);
                }

                unityProperty.serializedObject.Update();
                unityProperty = unityProperty.serializedObject.FindProperty(unityProperty.propertyPath);
            }

            float height = this.drawer.GetPropertyHeight(unityProperty, label);
            Rect position = EditorGUILayout.GetControlRect(false, height);

            if (this.propertyHandler != null)
            {
                UnityPropertyHandlerUtility.PropertyHandlerOnGUI(this.propertyHandler, position, unityProperty, label, false);
            }
            else
            {
                this.drawer.OnGUI(position, unityProperty, label);
            }

            if (unityProperty.serializedObject.targetObject is EmittedScriptableObject<T>)
            {
                unityProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                var targetObjects = unityProperty.serializedObject.targetObjects;

                for (int i = 0; i < targetObjects.Length; i++)
                {
                    EmittedScriptableObject<T> target = (EmittedScriptableObject<T>)targetObjects[i];
                    entry.Values[i] = target.GetValue();
                }
            }
            else if (unityProperty.serializedObject.targetObject is EmittedScriptableObject)
            {
                unityProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                var targetObjects = unityProperty.serializedObject.targetObjects;

                for (int i = 0; i < targetObjects.Length; i++)
                {
                    EmittedScriptableObject target = (EmittedScriptableObject)targetObjects[i];
                    entry.Values[i] = (T)target.GetWeakValue();
                }
            }
        }
    }
}
#endif