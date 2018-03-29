#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="InspectorPropertyGroupInfo.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Contains meta-data information about a group property in the inspector.
    /// </summary>
    public class InspectorPropertyGroupInfo : InspectorPropertyInfo
    {
        /// <summary>
        /// The <see cref="InspectorPropertyInfo"/>s of all the individual members in this group.
        /// </summary>
        public InspectorPropertyInfo[] GroupInfos { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InspectorPropertyGroupInfo"/> class.
        /// </summary>
        /// <param name="groupName">The group name.</param>
        /// <param name="order">The group order.</param>
        /// <param name="infos">The member infos.</param>
        public InspectorPropertyGroupInfo(string groupName, int order, IList<InspectorPropertyInfo> infos)
            : base(groupName, order, infos)
        {
            this.GroupInfos = infos.ToArray();
        }

        /// <summary>
        /// Gets the value of this property from the given owner. This method will throw an exception.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">Cannot get the value of a property of type PropertyType.Group.</exception>
        public override object GetValue(object owner)
        {
            throw new InvalidOperationException("Cannot get the value of a property of type PropertyType.Group.");
        }

        /// <summary>
        /// Gets the value of this property from the given owner. This method will throw an exception.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="System.InvalidOperationException">Cannot set the value of a property of type PropertyType.Group.</exception>
        public override void SetValue(object owner, object value)
        {
            throw new InvalidOperationException("Cannot set the value of a property of type PropertyType.Group.");
        }

        /// <summary>
        /// Returns false and a null getter setter.
        /// </summary>
        /// <typeparam name="TOwner2">The type of the owner2.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="getterSetter">The getter setter.</param>
        /// <returns></returns>
        public override bool TryConvertToGetterSetter<TOwner2, TValue>(out IValueGetterSetter<TOwner2, TValue> getterSetter)
        {
            getterSetter = null;
            return false;
        }
    }
}
#endif