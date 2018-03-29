//-----------------------------------------------------------------------
// <copyright file="SerializedMonoBehaviour.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector
{
    using Serialization;
    using UnityEngine;

    /// <summary>
    /// A Unity MonoBehaviour which is serialized by the Sirenix serialization system.
    /// </summary>
    [ShowOdinSerializedPropertiesInInspector]
    public abstract class SerializedMonoBehaviour : MonoBehaviour, ISerializationCallbackReceiver, ISupportsPrefabSerialization
    {
        [SerializeField, HideInInspector]
        private SerializationData serializationData;

        SerializationData ISupportsPrefabSerialization.SerializationData { get { return this.serializationData; } set { this.serializationData = value; } }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            UnitySerializationUtility.DeserializeUnityObject(this, ref this.serializationData);
            this.OnAfterDeserialize();
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            UnitySerializationUtility.SerializeUnityObject(this, ref this.serializationData);
            this.OnBeforeSerialize();
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        protected virtual void OnAfterDeserialize()
        {
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        protected virtual void OnBeforeSerialize()
        {
        }

#if UNITY_EDITOR

        [OnInspectorGUI, PropertyOrder(int.MinValue)]
        private void InternalOnInspectorGUI()
        {
            if (!EditorOnlyModeConfigUtility.IsSerializationEnabled)
            {
                GUILayout.Space(10);

                var color = GUI.color;
                GUI.color = new Color(0.8f, 0.1f, 0.4f, 1f);
                UnityEditor.EditorGUILayout.HelpBox(
                    EditorOnlyModeConfigUtility.SERIALIZATION_DISABLED_ERROR_TEXT,
                    UnityEditor.MessageType.Error);
                GUI.color = color;
            }

            if (!GlobalSerializationConfig.Instance.HideSerializationCautionaryMessage)
            {
                if (this.GetType().FullName.StartsWith("Sirenix.OdinInspector.Demos."))
                {
                    return;
                }

                GUILayout.Space(10);
                UnityEditor.EditorGUILayout.HelpBox(
                    GlobalSerializationConfig.ODIN_SERIALIZATION_CAUTIONARY_WARNING_TEXT + "\n\n\n\n",
                    UnityEditor.MessageType.Warning);

                var rect = GUILayoutUtility.GetLastRect();
                rect.xMin += 34;
                rect.yMax -= 10;
                rect.xMax -= 10;
                rect.yMin = rect.yMax - 25;

                if (GUI.Button(rect, GlobalSerializationConfig.ODIN_SERIALIZATION_CAUTIONARY_WARNING_BUTTON_TEXT))
                {
                    GlobalSerializationConfig.Instance.HideSerializationCautionaryMessage = true;
                    UnityEditor.EditorUtility.SetDirty(GlobalSerializationConfig.Instance);
                }
                GUILayout.Space(10);
            }
        }

#endif
    }
}