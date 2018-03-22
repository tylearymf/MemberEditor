#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="OdinSerializeAttributeWarningDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
    using Sirenix.Serialization;
    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using UnityEngine;

    /// <summary>
    /// <para>
    /// When first learning to use the Odin Inspector, it is common for people to misunderstand the OdinSerialize attribute,
    /// and use it in places where it does not achive the deceired goal.
    /// </para>
    /// <para>
    /// This drawer will display a warning message if the OdinSerialize attribute is potentially used in such cases.
    /// </para>
    /// </summary>
    /// <seealso cref="Sirenix.OdinInspector.Editor.OdinAttributeDrawer{Sirenix.Serialization.OdinSerializeAttribute}" />
    [OdinDrawer]
    [DrawerPriority(1000, 0, 0)]
    public sealed class OdinSerializeAttributeWarningDrawer : OdinAttributeDrawer<OdinSerializeAttribute>
    {
        /// <summary>
        /// Draws The Property.
        /// </summary>
        protected override void DrawPropertyLayout(InspectorProperty property, OdinSerializeAttribute attribute, GUIContent label)
        {
            if (GlobalSerializationConfig.Instance.HideOdinSerializeAttributeWarningMessages || property.Parent != null)
            {
                this.CallNextDrawer(property, label);
                return;
            }

            PropertyContext<string> msg;
            if (property.Context.Get(this, "Message", out msg))
            {
                if (property.Info.PropertyType == PropertyType.ValueType || property.Info.PropertyType == PropertyType.ReferenceType)
                {
                    string message = null;

                    if (property.ValueEntry.SerializationBackend == SerializationBackend.None)
                    {
                        message = "The following property is marked with the [OdinSerialize] attribute, " +
                                  "but the property is not part of any object that uses the Odin Serialization Protocol. \n\n" +
                                  "Are you perhaps forgetting to inherit from one of our serialized base classes such as SerializedMonoBehaviour or SerializedScriptableObject? \n\n";

                        var fieldInfo = property.Info.MemberInfo as System.Reflection.FieldInfo;
                        if (fieldInfo != null && fieldInfo.IsPublic && property.Info.GetAttribute<System.NonSerializedAttribute>() == null)
                        {
                            message += "Odin will also serialize public fields by default, so are you sure you need to mark the field with the [OdinSerialize] attribute?\n\n";
                        }
                    }                                                                                 // We need a way to find out if Unity will also serialize it.
                    else if (property.ValueEntry.SerializationBackend == SerializationBackend.Odin && UnitySerializationUtility.GuessIfUnityWillSerialize(property.Info.MemberInfo))
                    {
                        message = "The following property is marked with the [OdinSerialize] attribute, but Unity is also serializing it. " +
                                  "You can either remove the [OdinSerialize] attribute and let Unity serialize it, or you can use the [NonSerialized] " +
                                  "attribute together with the [OdinSerialize] attribute if you want Odin to serialize it instead of Unity.\n\n";

                        bool isCustomSerializableType =
                            property.Info.MemberInfo.DeclaringType.GetAttribute<System.SerializableAttribute>() != null &&
                            (property.Info.MemberInfo.DeclaringType.Assembly.GetAssemblyTypeFlag() & AssemblyTypeFlags.CustomTypes) != 0;

                        if (isCustomSerializableType)
                        {
                            message += "Odin's default serialization protocol does not require a type to be marked with the [Serializable] attribute in order for it to be serialized, which Unity does. " +
                                       "Therefore you could remove the System.Serializable attribute from " + property.Info.MemberInfo.DeclaringType.GetNiceFullName() + " if you want Unity never to serialize the type.\n\n";
                        }
                    }

                    if (message != null)
                    {
                        message += "Check out our online manual for more information.\n\n";
                        message += "This message can be disabled in the 'Tools > Odin Inspector > Preferences > Serialization' window, but it is recommended that you don't.";
                    }

                    msg.Value = message;
                }
            }

            if (msg.Value != null)
            {
                SirenixEditorGUI.WarningMessageBox(msg.Value);
            }

            this.CallNextDrawer(property, label);
        }
    }
}
#endif