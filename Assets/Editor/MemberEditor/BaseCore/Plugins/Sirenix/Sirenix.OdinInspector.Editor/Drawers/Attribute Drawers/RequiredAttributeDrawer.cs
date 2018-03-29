#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="RequiredAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
    using UnityEditor;
    using UnityEngine;
    using Sirenix.Utilities.Editor;

    /// <summary>
    /// Draws properties marked with <see cref="RequiredAttribute"/>.
    /// </summary>
	/// <seealso cref="RequiredAttribute"/>
	/// <seealso cref="InfoBoxAttribute"/>
	/// <seealso cref="ValidateInputAttribute"/>
    [OdinDrawer]
    [DrawerPriority(DrawerPriorityLevel.WrapperPriority)]
    public sealed class RequiredAttributeDrawer : OdinAttributeDrawer<RequiredAttribute>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(InspectorProperty property, RequiredAttribute attribute, GUIContent label)
        {
            if (property.ValueEntry.BaseValueType.IsValueType)
            {
                SirenixEditorGUI.ErrorMessageBox("Value types cannot be null, and thus cannot be marked as required.");
                return;
            }

			// Message context.
			PropertyContext<StringMemberHelper> context = null;
			if (attribute.ErrorMessage != null)
			{
				context = property.Context.Get<StringMemberHelper>(this, "ErrorMessage", (StringMemberHelper)null);
				if (context.Value == null)
				{
					context.Value = new StringMemberHelper(property.ParentType, attribute.ErrorMessage);
				}

				if (context.Value.ErrorMessage != null)
				{
					SirenixEditorGUI.ErrorMessageBox(context.Value.ErrorMessage);
				}
			}

            var isMissing = CheckIsMissing(property);

            if (isMissing)
            {
                string msg = attribute.ErrorMessage != null ? context.Value.GetString(property) : (property.NiceName + " is required.");
                if (attribute.MessageType == InfoMessageType.Warning)
                {
                    SirenixEditorGUI.WarningMessageBox(msg);
                }
                else if (attribute.MessageType == InfoMessageType.Error)
                {
                    SirenixEditorGUI.ErrorMessageBox(msg);
                }
                else
                {
                    EditorGUILayout.HelpBox(msg, (MessageType)attribute.MessageType);
                }
            }

            var key = UniqueDrawerKey.Create(property, this);
            SirenixEditorGUI.BeginShakeableGroup(key);
            this.CallNextDrawer(property, label);
            SirenixEditorGUI.EndShakeableGroup(key);

            if (!isMissing && CheckIsMissing(property))
            {
                SirenixEditorGUI.StartShakingGroup(key);
            }


        }

        private static bool CheckIsMissing(InspectorProperty property)
        {
            bool isMissing = property.ValueEntry.WeakSmartValue == null;

            if (isMissing == false && property.ValueEntry.WeakSmartValue is UnityEngine.Object)
            {
                var unityObject = property.ValueEntry.WeakSmartValue as UnityEngine.Object;
                if (unityObject == null)
                {
                    isMissing = true;
                }
            }
            else if (isMissing == false && property.ValueEntry.WeakSmartValue is string)
            {
                if (string.IsNullOrEmpty((string)property.ValueEntry.WeakSmartValue))
                {
                    isMissing = true;
                }
            }

            return isMissing;
        }
    }
}
#endif