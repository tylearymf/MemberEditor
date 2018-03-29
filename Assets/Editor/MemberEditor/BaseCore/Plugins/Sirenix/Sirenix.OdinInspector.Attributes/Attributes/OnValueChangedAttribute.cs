//-----------------------------------------------------------------------
// <copyright file="OnValueChangedAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector
{
    using System;

    /// <summary>
    /// <para>
    ///    OnValueChanged works on properties and fields, and calls the specified function 
    ///    whenever the value has been changed via the inspector.
    /// </para>
    /// </summary>
	/// <remarks>
    /// <note type="note">Note that this attribute only works in the editor! Properties changed by script will not call the function.</note>
    /// </remarks>
	/// <example>
    /// <para>The following example shows how OnValueChanged is used to provide a callback for a property.</para>
    /// <code>
    /// public class MyComponent : MonoBehaviour
	/// {
	///		[OnValueChanged("MyCallback")]
	///		public int MyInt;
	///
	///		private void MyCallback()
	///		{
	///			// ..
	///		}
	/// }
    /// </code>
    /// </example>
	/// <example>
    /// <para>The following example show how OnValueChanged can be used to get a component from a prefab property.</para>
    /// <code>
	/// public class MyComponent : MonoBehaviour
	/// {
	///		[OnValueChanged("OnPrefabChange")]
	///		public GameObject MyPrefab;
	///
	///		// RigidBody component of MyPrefab.
	///		[SerializeField, HideInInspector]
	///		private RigidBody myPrefabRigidbody;
	///
	///		private void OnPrefabChange()
	///		{
	///			if(MyPrefab != null)
	///			{
	///				myPrefabRigidbody = MyPrefab.GetComponent&lt;Rigidbody&gt;();
	///			}
	///			else
	///			{
	///				myPrefabRigidbody = null;
	///			}
	///		}
	/// }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    [DontApplyToListElements]
    public sealed class OnValueChangedAttribute : Attribute
    {
        /// <summary>
        /// Name of callback member function.
        /// </summary>
        public string MethodName { get; private set; }

        /// <summary>
        /// Whether to invoke the method when a child value of the property is changed.
        /// </summary>
        public bool IncludeChildren { get; private set; }

        /// <summary>
        /// Adds a callback for when the property's value is changed.
        /// </summary>
        /// <param name="methodName">Name of the method.</param>
		/// <param name="includeChildren">Whether to invoke the method when a child value of the property is changed.</param>
        public OnValueChangedAttribute(string methodName, bool includeChildren = false)
        {
            this.MethodName = methodName;
            this.IncludeChildren = includeChildren;
        }
    }
}