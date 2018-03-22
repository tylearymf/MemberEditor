#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="PropertyChildren.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
    using System;
    using System.Collections.Generic;
    using Utilities;

    /// <summary>
    /// Represents the children of an <see cref="InspectorProperty"/>.
    /// </summary>
    public abstract class PropertyChildren
    {
        private List<InspectorProperty> children = new List<InspectorProperty>();
        private List<string> paths = new List<string>();
        private bool allowChildren;

        /// <summary>
        /// The <see cref="InspectorProperty"/> that this instance handles children for.
        /// </summary>
        protected readonly InspectorProperty Property;

        /// <summary>
        /// Gets a child by index. This is an alias for <see cref="Get(int)" />.
        /// </summary>
        /// <param name="index">The index of the child to get.</param>
        /// <returns>The child at the given index.</returns>
        public InspectorProperty this[int index]
        {
            get { return this.Get(index); }
        }

        /// <summary>
        /// Gets a child by name. This is an alias for <see cref="Get(string)" />.
        /// </summary>
        /// <param name="name">The name of the child to get.</param>
        /// <returns>The child, if a child was found; otherwise, null.</returns>
        public InspectorProperty this[string name]
        {
            get { return this.Get(name); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyChildren"/> class.
        /// </summary>
        /// <param name="property">The property to handle children for.</param>
        /// <exception cref="System.ArgumentNullException">property is null</exception>
        protected internal PropertyChildren(InspectorProperty property)
        {
            if (property == null)
            {
                throw new ArgumentNullException("property");
            }

            this.Property = property;
        }

        /// <summary>
        /// The number of children on the property.
        /// </summary>
        public int Count
        {
            get
            {
                if (this.allowChildren == false)
                {
                    return 0;
                }

                return this.ActualCount;
            }
        }

        /// <summary>
        /// The actual number of children; this is different from <see cref="Count"/>, in that <see cref="Count"/> will be 0 if <see cref="GetAllowChildren"/> is false.
        /// </summary>
        protected abstract int ActualCount { get; }

        /// <summary>
        /// Whether this <see cref="PropertyChildren"/> instance represents the elements of a collection.
        /// </summary>
        public abstract bool IsCollection { get; }

        internal void ClearPathCache()
        {
            for (int i = 0; i < this.paths.Count; i++)
            {
                this.paths[i] = null;
            }
        }

        /// <summary>
        /// Updates this instance of <see cref="PropertyChildren"/>.
        /// </summary>
        public void Update()
        {
            bool baseAllowChildren = true;

            if (this.Property.ValueEntry != null && this.Property.ValueEntry.ValueState == PropertyValueState.Reference)
            {
                baseAllowChildren = false;
            }

            this.allowChildren = baseAllowChildren && this.GetAllowChildren();
            this.UpdateCount();

            if (this.children.Count != this.Count)
            {
                this.children.SetLength(this.Count);
            }
        }

        /// <summary>
        /// Updates the child count of the property.
        /// </summary>
        protected virtual void UpdateCount()
        {
        }

        /// <summary>
        /// Gets a child by name.
        /// </summary>
        /// <param name="name">The name of the child to get.</param>
        /// <returns>The child, if a child was found; otherwise, null.</returns>
        /// <exception cref="System.ArgumentNullException">name</exception>
        public InspectorProperty Get(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            for (int i = 0; i < this.children.Count; i++)
            {
                if (this.children[i].Name == name)
                {
                    return this.children[i];
                }
            }

            return this.Property.Tree.GetPropertyAtPath(this.Property.Path + "." + name);
        }

        /// <summary>
        /// Gets a child by index.
        /// </summary>
        /// <param name="index">The index of the child to get.</param>
        /// <returns>
        /// The child at the given index.
        /// </returns>
        /// <exception cref="System.IndexOutOfRangeException">The given index was out of range.</exception>
        public InspectorProperty Get(int index)
        {
            if (index < 0 || index >= this.Count)
            {
                throw new IndexOutOfRangeException();
            }

            if (this.children.Count != this.Count)
            {
                this.children.SetLength(this.Count);
            }

            InspectorProperty result = this.children[index];

            if (result == null)
            {
                // This order is very important. Calling result.Update() can cause all sorts of things to happen.
                // Including trying to get this very same child, resulting in an infinite loop because it hasn't
                // been set yet, so a new child will be created, ad infinitum.

                // Setting the child value, then updating, makes sure this sort of thing can never happen.

                result = this.CreateChild(index);
                this.children[index] = result;
                result.Update();
            }

            return result;
        }

        /// <summary>
        /// Gets the path of the child at a given index.
        /// </summary>
        /// <param name="index">The index to get the path of.</param>
        /// <returns>The path of the child at the given index.</returns>
        /// <exception cref="System.IndexOutOfRangeException">The given index was out of range.</exception>
        public string GetPath(int index)
        {
            if (index < 0 || index >= this.Count)
            {
                throw new IndexOutOfRangeException();
            }

            if (this.paths.Count != this.Count)
            {
                this.paths.SetLength(this.Count);
            }

            string result = this.paths[index];

            if (result == null)
            {
                result = this.GetPathImplementation(index);
                this.paths[index] = result;
            }

            return result;
        }

        /// <summary>
        /// Returns an IEnumerable that recursively yields all children of the property, depth first.
        /// </summary>
        public IEnumerable<InspectorProperty> Recurse()
        {
            for (int i = 0; i < this.Count; i++)
            {
                var child = this[i];

                yield return child;

                foreach (var subChild in child.Children.Recurse())
                {
                    yield return subChild;
                }
            }
        }

        /// <summary>
        /// Creates a child property for the given index.
        /// </summary>
        /// <param name="index">The index to create a child for.</param>
        /// <returns>The created child.</returns>
        protected abstract InspectorProperty CreateChild(int index);

        /// <summary>
        /// Determines whether to allow children on the property or not.
        /// </summary>
        /// <returns>Whether to allow children on the property or not.</returns>
        protected abstract bool GetAllowChildren();

        /// <summary>
        /// The implementaton that calculates a path for a given index.
        /// </summary>
        /// <param name="index">The index to calculate a path for.</param>
        /// <returns>The calculated path.</returns>
        protected abstract string GetPathImplementation(int index);

        /// <summary>
        /// Insert space for a child at the given index.
        /// </summary>
        /// <param name="index">The index to insert a space at.</param>
        protected void InsertSpaceAt(int index)
        {
            this.children.Insert(index, null);

            for (int i = 0; i < this.children.Count; i++)
            {
                var child = this.children[i];

                if (child != null)
                {
                    child.ForceUpdatePropertyNameAndPath(i);
                }
            }
        }

        /// <summary>
        /// Remove the child at the given index.
        /// </summary>
        /// <param name="index">The index to remove a child at.</param>
        protected void RemoveChildAt(int index)
        {
            this.children.RemoveAt(index);

            for (int i = 0; i < this.children.Count; i++)
            {
                var child = this.children[i];

                if (child != null)
                {
                    child.ForceUpdatePropertyNameAndPath(i);
                }
            }
        }
    }
}
#endif