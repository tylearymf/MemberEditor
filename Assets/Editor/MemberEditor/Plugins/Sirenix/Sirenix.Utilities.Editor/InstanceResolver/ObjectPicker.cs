#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ObjectPicker.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.Utilities.Editor
{
    using Sirenix.Serialization;
    using System;
    using System.Reflection;
    using System.Runtime.Serialization;
    using UnityEditor;
    using UnityEngine;
    using Utilities;

    /// <summary>
    /// Not yet documented.
    /// </summary>
    public sealed class ObjectPicker<T>
    {
        private readonly ObjectPicker picker;

        private static readonly object objectPickerConfigKey = new object();

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public static ObjectPicker<T> GetObjectPicker(object key)
        {
            var objectPicker = GUIHelper.GetTemporaryNullableContext<ObjectPicker<T>>(objectPickerConfigKey, key);
            objectPicker.Value = objectPicker.Value ?? new ObjectPicker<T>(ObjectPicker.GetObjectPicker(key, typeof(T)));
            objectPicker.Value.Update();
            return objectPicker;
        }

        private void Update()
        {
            this.picker.Update();
        }

        private ObjectPicker(ObjectPicker picker)
        {
            this.picker = picker;
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public T CurrentSelectedObject { get { return (T)this.picker.CurrentSelectedObject; } }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public bool IsReadyToClaim { get { return this.picker.IsReadyToClaim; } }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public bool IsPickerOpen
        {
            get { return this.picker.IsPickerOpen; }
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public T ClaimObject()
        {
            var obj = this.picker.ClaimObject();
            if (obj == null)
            {
                return default(T);
            }
            return (T)obj;
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public void ShowObjectPicker(bool allowSceneObjects, Rect buttonRect = default(Rect), bool isUnitySerialized = false)
        {
            this.picker.ShowObjectPicker(allowSceneObjects, buttonRect, isUnitySerialized);
            
        }
    }

    /// <summary>
    /// Not yet documented.
    /// </summary>
    public sealed class ObjectPicker
    {
        private static readonly object objectPickerConfigKey = new object();

        private readonly bool isUnityObject;
        private readonly bool isUnityComponent;
        private readonly bool isString;
        private readonly bool isClass;
        private readonly bool isInterface;
        private readonly bool isStruct;
        private readonly bool isAbstract;
        private readonly Type type;
        private int controlId;
        private bool isPickerOpen;
        private bool isUnitySerialized;

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public static ObjectPicker GetObjectPicker(object key, Type type)
        {
            var objectPicker = GUIHelper.GetTemporaryNullableContext<ObjectPicker>(objectPickerConfigKey, key);
            objectPicker.Value = objectPicker.Value ?? new ObjectPicker(type);

            if (objectPicker.Value.type != type)
            {
                objectPicker.Value = new ObjectPicker(type);
            }

            objectPicker.Value.Update();
            return objectPicker;
        }

        private ObjectPicker(Type type)
        {
            this.type = type;
            this.isString = this.type == typeof(string);
            this.isUnityObject = this.type.InheritsFrom(typeof(UnityEngine.Object));
            this.isClass = this.type.IsClass;
            this.isInterface = this.type.IsInterface;
            this.isAbstract = this.type.IsAbstract;
            this.isStruct = !this.isString && !this.isUnityObject && !this.isClass && !this.isInterface;
            this.isUnityComponent = this.type.InheritsFrom(typeof(UnityEngine.Component));
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public object CurrentSelectedObject { get; private set; }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public bool IsReadyToClaim { get; private set; }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public bool IsPickerOpen
        {
            get { return this.isPickerOpen; }
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public object ClaimObject()
        {
            if (this.IsReadyToClaim)
            {
                GUIHelper.RequestRepaint();
                this.IsReadyToClaim = false;
                this.isPickerOpen = false;
                var obj = this.CurrentSelectedObject;
                this.CurrentSelectedObject = null;
                return obj;
            }
            else
            {
                GUIHelper.RequestRepaint();
                this.isPickerOpen = false;
                this.IsReadyToClaim = false;
                Debug.LogError("No object is ready to be claimed.");
                return null;
            }
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public void ShowObjectPicker(bool allowSceneObjects, Rect buttonRect = default(Rect), bool isUnitySerialized = false)
        {
            this.isPickerOpen = true;
            this.isUnitySerialized = isUnitySerialized;

            if (this.isUnityObject)
            {
                if (Event.current.modifiers == EventModifiers.Control)
                {
                    this.CurrentSelectedObject = null;
                    this.IsReadyToClaim = true;
                }
                else
                {
                    typeof(EditorGUIUtility)
                        .GetMethod("ShowObjectPicker", BindingFlags.Static | BindingFlags.Public)
                        .MakeGenericMethod(this.type)
                        .Invoke(null, new object[] { null, allowSceneObjects, null, this.controlId });
                }
            }
            else if (this.isString)
            {
                this.CurrentSelectedObject = "";
            }
            else if (this.isStruct)
            {
                this.CurrentSelectedObject = UnitySerializationUtility.CreateDefaultUnityInitializedObject(this.type);
            }
            else if (typeof(Delegate).IsAssignableFrom(this.type))
            {
                this.CurrentSelectedObject = null;
                this.IsReadyToClaim = true;
            }
            else if (this.isClass || this.isInterface)
            {
                if (isUnitySerialized)
                {
                    if (this.isInterface)
                    {
                        Debug.LogError("Property is serialized by Unity, where interfaces are not supported.");
                        return;
                    }
                    else if (this.isAbstract)
                    {
                        Debug.LogError("Property is serialized by Unity, where abstract classes are not supported.");
                        return;
                    }
                    else
                    {
                        this.CurrentSelectedObject = UnitySerializationUtility.CreateDefaultUnityInitializedObject(this.type);
                    }
                }
                else
                {
                    if (buttonRect.width > 0 && buttonRect.height > 0)
                    {
                        InstanceCreator.Show(this.type, this.controlId, buttonRect);
                    }
                    else
                    {
                        InstanceCreator.Show(this.type, this.controlId);
                    }
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        internal void Update()
        {
            this.controlId = GUIUtility.GetControlID(FocusType.Passive);

            if (this.isPickerOpen == false)
            {
                this.IsReadyToClaim = false;
                return;
            }

            if (this.isUnityObject)
            {
                if (Event.current.type != EventType.Layout)
                {
                    if (EditorGUIUtility.GetObjectPickerControlID() == this.controlId && this.controlId > 0)
                    {
                        if (Event.current.commandName == "ObjectSelectorUpdated")
                        {
                            var val = EditorGUIUtility.GetObjectPickerObject();
                            if (val == null)
                            {
                                this.CurrentSelectedObject = null;
                            }
                            else if (this.isUnityComponent)
                            {
                                this.CurrentSelectedObject = ((GameObject)val).GetComponent(this.type);
                            }
                            else
                            {
                                this.CurrentSelectedObject = val;
                            }
                        }
                        else if (Event.current.commandName == "ObjectSelectorClosed")
                        {
                            this.IsReadyToClaim = true;
                        }
                    }
                }
            }
            else if (this.isString || this.isStruct)
            {
                this.IsReadyToClaim = true;
            }
            else if (this.isClass || this.isInterface)
            {
                if ((Event.current == null || Event.current.type != EventType.Layout))
                {
                    if (this.isUnitySerialized)
                    {
                        //this.CurrentSelectedObject = null; // Let Unity fill in the value.

                        // No don't - this will mess up OnValueChanged; instead, create a passable Unity default value
                        // if one hasn't been created already

                        if (this.CurrentSelectedObject == null)
                        {
                            this.CurrentSelectedObject = UnitySerializationUtility.CreateDefaultUnityInitializedObject(this.type);
                        }

                        this.IsReadyToClaim = true;
                    }
                    else if (InstanceCreator.ControlID == this.controlId && this.controlId > 0 && InstanceCreator.HasCreatedInstance)
                    {
                        var val = InstanceCreator.GetCreatedInstance();
                        if (val == null)
                        {
                            this.CurrentSelectedObject = null;
                        }
                        else
                        {
                            this.CurrentSelectedObject = val;
                        }
                        this.IsReadyToClaim = true;
                    }
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
#endif