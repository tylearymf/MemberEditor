#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="PropertyMemberValueEntry.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
    using System;
    using Utilities;

    /// <summary>
    /// Represents the values of a member-backed <see cref="InspectorProperty"/>, and contains utilities for querying the values' type and getting and setting them.
    /// </summary>
    public sealed class PropertyMemberValueEntry<TParent, TValue> : PropertyValueEntry<TParent, TValue>
    {
        private static readonly bool ParentIsValueType = typeof(TParent).IsValueType;

        private IValueGetterSetter<TParent, TValue> valueGetterSetter;

        private PropertyMemberValueEntry()
        {
        }

        /// <summary>
        /// The value category of this value entry.
        /// </summary>
        public override PropertyValueCategory ValueCategory { get { return PropertyValueCategory.Member; } }

        /// <summary>
        /// Initializes this value entry.
        /// </summary>
        /// <exception cref="System.ArgumentException">On a value entry of type " + this.GetType().GetNiceName() + " on property " + this.Property.Name + " at path " + this.Property.Path + ", the given InspectorPropertyInfo of type " + this.Property.Info.GetType().GetNiceName() + " could not be converted into a reference of type " + typeof(IValueGetterSetter&lt;TParent, TValue&gt;).GetNiceName() + ".</exception>
        protected override void Initialize()
        {
            base.Initialize();

            if (!this.Property.Info.TryConvertToGetterSetter(out this.valueGetterSetter))
            {
                throw new ArgumentException("On a value entry of type " + this.GetType().GetNiceName() + " on property " + this.Property.Name + " at path " + this.Property.Path + ", the given InspectorPropertyInfo of type " + this.Property.Info.GetType().GetNiceName() + " could not be converted into a reference of type " + typeof(IValueGetterSetter<TParent, TValue>).GetNiceName() + ".");
            }
        }

        /// <summary>
        /// Gets the actual boxed value of the tree target.
        /// </summary>
        protected override object GetActualBoxedValue(TParent parent)
        {
            return this.GetActualValue(parent);
        }

        /// <summary>
        /// Gets the actual value of the tree target.
        /// </summary>
        protected override TValue GetActualValue(TParent parent)
        {
            return this.valueGetterSetter.GetValue(ref parent);
        }

        /// <summary>
        /// Sets the actual target tree value.
        /// </summary>
        protected override void SetActualBoxedValueImplementation(int index, object value)
        {
            this.SetActualValueImplementation(index, (TValue)value);
        }

        /// <summary>
        /// Sets the actual target tree value.
        /// </summary>
        protected override void SetActualValueImplementation(int index, TValue value)
        {
            TParent parent = this.GetParent(index);

            if (ParentIsValueType)
            {
                this.valueGetterSetter.SetValue(ref parent, value);
                ((IValueEntryActualValueSetter<TParent>)this.ParentValueProperty.ValueEntry).SetActualValue(index, parent);
            }
            else
            {
                this.valueGetterSetter.SetValue(ref parent, value);
            }
        }
    }
}
#endif