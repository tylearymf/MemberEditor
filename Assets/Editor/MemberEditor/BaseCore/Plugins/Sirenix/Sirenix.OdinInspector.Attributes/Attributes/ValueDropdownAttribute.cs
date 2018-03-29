//-----------------------------------------------------------------------
// <copyright file="ValueDropdownAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// <para>ValueDropdown is used on any property and creates a dropdown with configurable options.</para>
    /// <para>Use this to give the user a specific set of options to select from.</para>
    /// </summary>
	/// <remarks>
    /// <note type="note">Due to a bug in Unity, enums will sometimes not work correctly. The last example shows how this can be fixed.</note>
    /// </remarks>
	/// <example>
    /// <para>The following example shows a how the ValueDropdown can be used on an int property.</para>
    /// <code>
    /// public class MyComponent : MonoBehaviour
	///	{
	///		[ValueDropdown("myValues")]
	///		public int MyInt;
	///
	///		// The selectable values for the dropdown.
	///		private int[] myValues = { 1, 2, 3 };
	///	}
    /// </code>
    /// </example>
	/// <example>
    /// <para>The following example shows how ValueDropdownList can be used for objects, that do not implement a usable ToString.</para>
    /// <code>
	/// public class MyComponent : MonoBehaviour
	/// {
	///		[ValueDropdown("myVectorValues")]
	///		public Vector3 MyVector;
	///
	///		// The selectable values for the dropdown, with custom names.
	///		private ValueDropdownList&lt;Vector3&gt; myVectorValues = new ValueDropdownList&lt;Vector3&gt;()
	///		{
	///			{"Forward",	Vector3.forward	},
	///			{"Back",	Vector3.back	},
	///			{"Up",		Vector3.up		},
	///			{"Down",	Vector3.down	},
	///			{"Right",	Vector3.right	},
	///			{"Left",	Vector3.left	},
	///		};
	/// }
    /// </code>
    /// </example>
	///	<example>
	///	<para>The following example shows how the ValueDropdown can on any member that implements IList.</para>
	///	<code>
	/// public class MyComponent : MonoBehaviour
	/// {
	///		// Member field of type float[].
	///		private float[] valuesField;
	///
	///		[ValueDropdown("valuesField")]
	///		public float MyFloat;
	///
	///		// Member property of type List&lt;thing&gt;.
	///		private List&lt;string&gt; ValuesProperty { get; set; }
	///
	///		[ValueDropdown("ValuesProperty")]
	///		public string MyString;
	///
	///		// Member function that returns an object of type IList.
	///		private IList ValuesFunction()
	///		{
	///			return new ValueDropdownList&lt;int&gt;
	///			{
	///				{ "The first option",	1 },
	///				{ "The second option",	2 },
	///				{ "The third option",	3 },
	///			};
	///		}
	///
	///		[ValueDropdown("ValuesFunction")]
	///		public int MyInt;
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// <para>Due to a bug in Unity, enums member arrays will in some cases appear as empty. This example shows how you can get around that.</para>
	/// <code>
	/// public class MyComponent : MonoBehaviour
	/// {
	///		// Make the field static.
	///		private static MyEnum[] MyStaticEnumArray = MyEnum[] { ... };
	///
	///		// Force Unity to serialize the field, and hide the property from the inspector.
	///		[SerializeField, HideInInspector]
	///		private MyEnum MySerializedEnumArray = MyEnum[] { ... };
	/// }
	/// </code>
	/// </example>
	/// <seealso cref="ValueDropdownList{T}"/>
    public class ValueDropdownAttribute : Attribute
    {
        /// <summary>
        /// Name of any field, property or method member that implements IList. E.g. arrays or Lists.
        /// </summary>
        public string MemberName { get; private set; }

        /// <summary>
        /// Creates a dropdown menu for a property.
        /// </summary>
        /// <param name="memberName">Name of any field, property or method member that implements IList. E.g. arrays or Lists.</param>
        public ValueDropdownAttribute(string memberName)
        {
            this.MemberName = memberName;
        }
    }

    /// <summary>
    /// Use this with <see cref="ValueDropdownAttribute"/> to specify custom names for values.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    public class ValueDropdownList<T> : List<ValueDropdownItem<T>>
    {
        /// <summary>
        /// Adds the specified value with a custom name.
        /// </summary>
        /// <param name="text">The name of the item.</param>
        /// <param name="value">The value.</param>
        public void Add(string text, T value)
        {
            this.Add(new ValueDropdownItem<T>(text, value));
        }

        /// <summary>
        /// Adds the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        public void Add(T value)
        {
            this.Add(new ValueDropdownItem<T>(value.ToString(), value));
        }
    }

    /// <summary>
    /// Used by <see cref="ValueDropdownList{T}"/> for value dropdowns.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    public struct ValueDropdownItem<T> : IEquatable<ValueDropdownItem<T>>
    {
        /// <summary>
        /// The name of the item.
        /// </summary>
        public readonly string Text;

        /// <summary>
        /// The value of the item.
        /// </summary>
        public readonly T Value;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueDropdownItem{T}"/> class.
        /// </summary>
        /// <param name="text">The name of the item.</param>
        /// <param name="value">The value of the item.</param>
        public ValueDropdownItem(string text, T value)
        {
            this.Text = text;
            this.Value = value;
        }

        /// <summary>
        /// The name of this item.
        /// </summary>
        /// <returns>
        /// The name of this item.
        /// </returns>
        public override string ToString()
        {
            return this.Text ?? (this.Value + "");
        }

        /// <summary>
        /// Equalses the specified other.
        /// </summary>
        /// <param name="other">The other item.</param>
        /// <returns><c>true</c> if equal. Otherwise <c>false</c>.</returns>
        public bool Equals(ValueDropdownItem<T> other)
        {
            return EqualityComparer<T>.Default.Equals(this.Value, other.Value);
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(obj, null))
            {
                return false;
            }

            if (obj.GetType() != typeof(ValueDropdownItem<T>))
            {
                return false;
            }

            return this.Equals((ValueDropdownItem<T>)obj);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            if (this.Value == null)
            {
                return -1;
            }

            return this.Value.GetHashCode();
        }
    }
}