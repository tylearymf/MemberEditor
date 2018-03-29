#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="NullableDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Property drawer for nullables.
    /// </summary>
    [OdinDrawer]
    public sealed class NullableDrawer<T> : OdinValueDrawer<T?>, IDefinesGenericMenuItems where T : struct
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<T?> entry, GUIContent label)
        {
            var context = entry.Property.Context.Get(this, "context", (PropertyTree<Wrapper>)null);

            if (context.Value == null)
            {
                Wrapper[] wrappers = new Wrapper[entry.ValueCount];

                for (int i = 0; i < wrappers.Length; i++)
                {
                    var wrapper = new Wrapper();
                    wrappers[i] = wrapper;
                }

                context.Value = new PropertyTree<Wrapper>(wrappers);
                context.Value.UpdateTree();
            }

            for (int i = 0; i < context.Value.Targets.Count; i++)
            {
                context.Value.Targets[i].SetValue(entry.Values[i]);
            }

            context.Value.GetRootProperty(0).Label = label;
            context.Value.Draw(false);

            for (int i = 0; i < context.Value.Targets.Count; i++)
            {
                var value = context.Value.Targets[i];

                if (value.Value == null)
                {
                    entry.Values[i] = null;
                }
                else
                {
                    entry.Values[i] = value.Value.Value;
                }
            }
        }

        void IDefinesGenericMenuItems.PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
        {
            var content = new GUIContent("Set to null");
            IPropertyValueEntry<T?> entry = (IPropertyValueEntry<T?>)property.ValueEntry;

            if (entry.SmartValue.HasValue)
            {
                genericMenu.AddItem(content, false, () =>
                {
                    property.Tree.DelayActionUntilRepaint(() =>
                    {
                        entry.SmartValue = null;
                    });
                });
            }
            else
            {
                genericMenu.AddDisabledItem(content);
            }
        }

        [ShowOdinSerializedPropertiesInInspector]
        private class Wrapper
        {
            public NullableValue<T> Value;

            public void SetValue(T? value)
            {
                if (value.HasValue)
                {
                    this.Value = new NullableValue<T>();
                    this.Value.Value = value.Value;
                }
            }
        }
    }

    internal class NullableValue<T>
    {
        [HideLabel]
        public T Value;
    }
}
#endif