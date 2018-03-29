#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ComplexTypePropertyChildren.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
    using Sirenix.Serialization;
    using System;

    /// <summary>
    /// Represents children for a property of a complex type; a type with several "child members" to represent.
    /// </summary>
    public sealed class ComplexTypePropertyChildren : PropertyChildren
    {
        private InspectorPropertyInfo[] infos;

        /// <summary>
        /// Initializes a new instance of the <see cref="ComplexTypePropertyChildren"/> class.
        /// </summary>
        /// <param name="property">The property to handle children for.</param>
        /// <exception cref="System.ArgumentException">
        /// Property to create complex type children for has no value entry.
        /// or
        /// Property must be either a reference type or a value type property.
        /// </exception>
        public ComplexTypePropertyChildren(InspectorProperty property) : base(property)
        {
            if (property.ValueEntry == null)
            {
                throw new ArgumentException("Property to create complex type children for has no value entry.");
            }

            if (property.Info.PropertyType != PropertyType.ReferenceType && property.Info.PropertyType != PropertyType.ValueType)
            {
                throw new ArgumentException("Property must be either a reference type or a value type property.");
            }

            this.ComplexType = property.ValueEntry.TypeOfValue;
            this.infos = InspectorPropertyInfo.Get(this.ComplexType, property.Tree.IncludesSpeciallySerializedMembers && property.ValueEntry.SerializationBackend != SerializationBackend.Unity);
        }

        /// <summary>
        /// The complex type that this instance represents.
        /// </summary>
        /// <value>
        /// The type of the complex.
        /// </value>
        public Type ComplexType { get; private set; }

        /// <summary>
        /// Whether this <see cref="PropertyChildren" /> instance represents the elements of a collection.
        /// </summary>
        public override bool IsCollection { get { return false; } }

        /// <summary>
        /// The actual number of children; this is different from <see cref="Count" />, in that <see cref="Count" /> will be 0 if <see cref="GetAllowChildren" /> is false.
        /// </summary>
        protected override int ActualCount { get { return this.infos.Length; } }

        /// <summary>
        /// Determines whether to allow children on the property or not.
        /// </summary>
        /// <returns>
        /// Whether to allow children on the property or not.
        /// </returns>
        protected override bool GetAllowChildren()
        {
            if (typeof(UnityEngine.Object).IsAssignableFrom(this.Property.ValueEntry.TypeOfValue))
            {
                return false;
            }

            var overrideType = this.Property.ValueEntry.ValueState;
            return overrideType == PropertyValueState.None || overrideType == PropertyValueState.CollectionLengthConflict || overrideType == PropertyValueState.PrimitiveValueConflict;
        }

        /// <summary>
        /// The implementaton that calculates a path for a given index.
        /// </summary>
        /// <param name="index">The index to calculate a path for.</param>
        /// <returns>
        /// The calculated path.
        /// </returns>
        /// <exception cref="System.ArgumentException">A dictionary element cannot have more than two children; invalid index '" + index + "'.</exception>
        protected override string GetPathImplementation(int index)
        {
            if (this.Property.ValueEntry != null && this.Property.ValueEntry.ValueCategory == PropertyValueCategory.DictionaryElement)
            {
                // Special case for dictionary key value pairs; key gets a special name, and value gets the "straight" name

                var handler = this.Property.Parent.ValueEntry.GetDictionaryHandler();
                var key = handler.GetKey(0, this.Property.Index);
                var keyStr = DictionaryKeyUtility.GetDictionaryKeyString(key);

                if (index == 0) // Key
                {
                    return this.Property.Parent.Path + "." + keyStr + ".#key";
                }
                else if (index == 1) // Value
                {
                    return this.Property.Parent.Path + "." + keyStr;
                }
                else
                {
                    throw new ArgumentException("A dictionary element cannot have more than two children; invalid index '" + index + "'.");
                }
            }

            return this.Property.Path + "." + this.infos[index].PropertyName;
        }

        /// <summary>
        /// Creates a child property for the given index.
        /// </summary>
        /// <param name="index">The index to create a child for.</param>
        /// <returns>
        /// The created child.
        /// </returns>
        protected override InspectorProperty CreateChild(int index)
        {
            return InspectorProperty.Create(this.Property.Tree, this.Property, this.infos[index], index);
        }
    }
}
#endif