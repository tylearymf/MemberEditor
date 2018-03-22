#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="PropertyContextContainer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
    using Sirenix.OdinInspector.Editor.Drawers;
    using Sirenix.Serialization;
    using System;
    using System.Collections.Generic;
    using Utilities;

    /// <summary>
    /// <para>Contains a context for an <see cref="InspectorProperty"/>, which offers the ability to address persistent values by key across several editor GUI frames.</para>
    /// <para>Use this in drawers to store contextual editor-only values such as the state of a foldout.</para>
    /// </summary>
    public sealed class PropertyContextContainer
    {
        private Dictionary<string, object> globalContexts = new Dictionary<string, object>();
        private Dictionary<string, ITemporaryContext> globalTemporaryContexts = new Dictionary<string, ITemporaryContext>();
        private DoubleLookupDictionary<int, int, DoubleLookupDictionary<Type, string, object>> drawerContexts = new DoubleLookupDictionary<int, int, DoubleLookupDictionary<Type, string, object>>();
        private DoubleLookupDictionary<int, int, DoubleLookupDictionary<Type, string, ITemporaryContext>> temporaryContexts = new DoubleLookupDictionary<int, int, DoubleLookupDictionary<Type, string, ITemporaryContext>>();
        private InspectorProperty property;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyContextContainer"/> class.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <exception cref="System.ArgumentNullException">property</exception>
        public PropertyContextContainer(InspectorProperty property)
        {
            if (property == null)
            {
                throw new ArgumentNullException("property");
            }

            this.property = property;
        }

        /// <summary>
        /// <para>Gets a global temporary context value for a given key, using a given delegate to generate a default value if the context doesn't already exist.</para>
        /// <para>Global contexts are not associated with any one specific drawer, and so are shared across all drawers for this property.</para>
        /// </summary>
        /// <typeparam name="T">The type of the context value to get.</typeparam>
        /// <param name="key">The key of the context value to get.</param>
        /// <param name="getDefaultValue">A delegate for generating a default value.</param>
        /// <returns>The found context.</returns>
        public TemporaryPropertyContext<T> GetGlobalTemporary<T>(string key, Func<T> getDefaultValue)
        {
            TemporaryPropertyContext<T> result;
            bool isNew;

            if (this.TryGetGlobalTemporaryConfig(key, out result, out isNew) && isNew)
            {
                result.Value = getDefaultValue();
            }

            return result;
        }

        /// <summary>
        /// <para>Gets a global temporary context value for a given key, using a given default value if the context doesn't already exist.</para>
        /// <para>Global contexts are not associated with any one specific drawer, and so are shared across all drawers for this property.</para>
        /// </summary>
        /// <typeparam name="T">The type of the context value to get.</typeparam>
        /// <param name="key">The key of the context value to get.</param>
        /// <param name="defaultValue">The default value to set if the context value doesn't exist yet.</param>
        /// <returns>The found context.</returns>
        public TemporaryPropertyContext<T> GetGlobalTemporary<T>(string key, T defaultValue)
        {
            TemporaryPropertyContext<T> result;
            bool isNew;

            if (this.TryGetGlobalTemporaryConfig(key, out result, out isNew) && isNew)
            {
                result.Value = defaultValue;
            }

            return result;
        }

        /// <summary>
        /// <para>Gets a global temporary context value for a given key, and creates a new instance of <see cref="T"/> as a default value if the context doesn't already exist.</para>
        /// <para>Global contexts are not associated with any one specific drawer, and so are shared across all drawers for this property.</para>
        /// </summary>
        /// <typeparam name="T">The type of the context value to get.</typeparam>
        /// <param name="key">The key of the context value to get.</param>
        /// <returns>The found context.</returns>
        public TemporaryPropertyContext<T> GetGlobalTemporary<T>(string key) where T : new()
        {
            TemporaryPropertyContext<T> result;
            bool isNew;

            if (this.TryGetGlobalTemporaryConfig(key, out result, out isNew) && isNew)
            {
                result.Value = new T();
            }

            return result;
        }

        private bool TryGetGlobalTemporaryConfig<T>(string key, out TemporaryPropertyContext<T> context, out bool isNew)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            context = null;
            ITemporaryContext value;

            if (this.globalTemporaryContexts == null)
            {
                this.globalTemporaryContexts = new Dictionary<string, ITemporaryContext>();
            }

            var contexts = this.globalTemporaryContexts;

            if (contexts.TryGetValue(key, out value))
            {
                isNew = false;
                context = value as TemporaryPropertyContext<T>;

                if (context == null)
                {
                    throw new InvalidOperationException("Tried to get global property of type " + typeof(T).GetNiceName() + " with key " + key + " on property at path " + this.property.Path + ", but a global property of a different type (" + value.GetType().GetArgumentsOfInheritedOpenGenericClass(typeof(PropertyContext<>))[0].GetNiceName() + ") already existed with the same key.");
                }
            }
            else
            {
                isNew = true;
                context = new TemporaryPropertyContext<T>();
                contexts[key] = context;
            }

            return true;
        }

        /// <summary>
        /// <para>Gets a global context value for a given key, using a given delegate to generate a default value if the context doesn't already exist.</para>
        /// <para>Global contexts are not associated with any one specific drawer, and so are shared across all drawers for this property.</para>
        /// </summary>
        /// <typeparam name="T">The type of the context value to get.</typeparam>
        /// <param name="key">The key of the context value to get.</param>
        /// <param name="getDefaultValue">A delegate for generating a default value.</param>
        /// <returns>The found context.</returns>
        public PropertyContext<T> GetGlobal<T>(string key, Func<T> getDefaultValue)
        {
            PropertyContext<T> result;
            bool isNew;

            if (this.TryGetGlobalConfig(key, out result, out isNew) && isNew)
            {
                result.Value = getDefaultValue();
            }

            return result;
        }

        /// <summary>
        /// <para>Gets a global context value for a given key, using a given default value if the context doesn't already exist.</para>
        /// <para>Global contexts are not associated with any one specific drawer, and so are shared across all drawers for this property.</para>
        /// </summary>
        /// <typeparam name="T">The type of the context value to get.</typeparam>
        /// <param name="key">The key of the context value to get.</param>
        /// <param name="defaultValue">The default value to set if the context value doesn't exist yet.</param>
        /// <returns>The found context.</returns>
        public PropertyContext<T> GetGlobal<T>(string key, T defaultValue)
        {
            PropertyContext<T> result;
            bool isNew;

            if (this.TryGetGlobalConfig(key, out result, out isNew) && isNew)
            {
                result.Value = defaultValue;
            }

            return result;
        }

        /// <summary>
        /// <para>Gets a global context value for a given key, and creates a new instance of <see cref="T"/> as a default value if the context doesn't already exist.</para>
        /// <para>Global contexts are not associated with any one specific drawer, and so are shared across all drawers for this property.</para>
        /// </summary>
        /// <typeparam name="T">The type of the context value to get.</typeparam>
        /// <param name="key">The key of the context value to get.</param>
        /// <returns>The found context.</returns>
        public PropertyContext<T> GetGlobal<T>(string key) where T : new()
        {
            PropertyContext<T> result;
            bool isNew;

            if (this.TryGetGlobalConfig(key, out result, out isNew) && isNew)
            {
                result.Value = new T();
            }

            return result;
        }

        private bool TryGetGlobalConfig<T>(string key, out PropertyContext<T> context, out bool isNew)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            context = null;
            object value;

            if (this.globalContexts == null)
            {
                this.globalContexts = new Dictionary<string, object>();
            }

            var contexts = this.globalContexts;

            if (contexts.TryGetValue(key, out value))
            {
                isNew = false;
                context = value as PropertyContext<T>;

                if (context == null)
                {
                    throw new InvalidOperationException("Tried to get global property of type " + typeof(T).GetNiceName() + " with key " + key + " on property at path " + this.property.Path + ", but a global property of a different type (" + value.GetType().GetArgumentsOfInheritedOpenGenericClass(typeof(PropertyContext<>))[0].GetNiceName() + ") already existed with the same key.");
                }
            }
            else
            {
                isNew = true;
                context = PropertyContext<T>.Create();
                contexts[key] = context;
            }

            return true;
        }

        /// <summary>
        /// <para>Gets a context value local to a drawer type for a given key, using a given delegate to generate a default value if the context doesn't already exist.</para>
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="drawerInstance">An instance of the drawer type linked to the context value to get.</param>
        /// <param name="key">The key of the context value to get.</param>
        /// <param name="getDefaultValue">A delegate for generating a default value.</param>
        /// <returns>
        /// The found context.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">drawerInstance is null</exception>
        public PropertyContext<TValue> Get<TValue>(OdinDrawer drawerInstance, string key, Func<TValue> getDefaultValue)
        {
            if (drawerInstance == null)
            {
                throw new ArgumentNullException("drawerInstance");
            }

            return this.Get(drawerInstance.GetType(), key, getDefaultValue);
        }

        /// <summary>
        /// <para>Gets a context value local to a drawer type for a given key, using a given delegate to generate a default value if the context doesn't already exist.</para>
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <typeparam name="TDrawer">The type of the drawer.</typeparam>
        /// <param name="key">The key of the context value to get.</param>
        /// <param name="getDefaultValue">A delegate for generating a default value.</param>
        /// <returns>
        /// The found context.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">drawerInstance is null</exception>
        public PropertyContext<TValue> Get<TValue, TDrawer>(string key, Func<TValue> getDefaultValue) where TDrawer : OdinDrawer
        {
            return this.Get(typeof(TDrawer), key, getDefaultValue);
        }

        /// <summary>
        /// <para>Gets a context value local to a drawer type for a given key, using a given delegate to generate a default value if the context doesn't already exist.</para>
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="drawerType">The type of the drawer.</param>
        /// <param name="key">The key of the context value to get.</param>
        /// <param name="getDefaultValue">A delegate for generating a default value.</param>
        /// <returns>
        /// The found context.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">drawerInstance is null</exception>
        public PropertyContext<TValue> Get<TValue>(Type drawerType, string key, Func<TValue> getDefaultValue)
        {
            PropertyContext<TValue> context;
            bool isNew;

            if (this.TryGetDrawerContext(drawerType, key, out context, out isNew) && isNew)
            {
                context.Value = getDefaultValue();
            }

            return context;
        }

        /// <summary>
        /// <para>Gets a context value local to a drawer type for a given key, using a given default value if the context doesn't already exist.</para>
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="drawerInstance">An instance of the drawer type linked to the context value to get.</param>
        /// <param name="key">The key of the context value to get.</param>
        /// <param name="defaultValue">The default value to set if the context value doesn't exist yet.</param>
        /// <returns>
        /// The found context.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">drawerInstance is null</exception>
        public PropertyContext<TValue> Get<TValue>(OdinDrawer drawerInstance, string key, TValue defaultValue)
        {
            if (drawerInstance == null)
            {
                throw new ArgumentNullException("drawerInstance");
            }

            return this.Get(drawerInstance.GetType(), key, defaultValue);
        }

        /// <summary>
        /// <para>Gets a context value local to a drawer type for a given key, using a given default value if the context doesn't already exist.</para>
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <typeparam name="TDrawer">The type of the drawer.</typeparam>
        /// <param name="key">The key of the context value to get.</param>
        /// <param name="defaultValue">The default value to set if the context value doesn't exist yet.</param>
        /// <returns>
        /// The found context.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">drawerInstance is null</exception>
        public PropertyContext<TValue> Get<TValue, TDrawer>(string key, TValue defaultValue) where TDrawer : OdinDrawer
        {
            return this.Get(typeof(TDrawer), key, defaultValue);
        }

        /// <summary>
        /// <para>Gets a context value local to a drawer type for a given key, using a given default value if the context doesn't already exist.</para>
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="drawerType">The type of the drawer.</param>
        /// <param name="key">The key of the context value to get.</param>
        /// <param name="defaultValue">The default value to set if the context value doesn't exist yet.</param>
        /// <returns>
        /// The found context.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">drawerInstance is null</exception>
        public PropertyContext<TValue> Get<TValue>(Type drawerType, string key, TValue defaultValue)
        {
            PropertyContext<TValue> context;
            bool isNew;

            if (this.TryGetDrawerContext(drawerType, key, out context, out isNew) && isNew)
            {
                context.Value = defaultValue;
            }

            return context;
        }

        /// <summary>
        /// Gets a context value local to a drawer type for a given key, and creates a new instance of <see cref="T" /> as a default value if the context doesn't already exist.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="drawerInstance">An instance of the drawer type linked to the context value to get.</param>
        /// <param name="key">The key of the context value to get.</param>
        /// <returns>
        /// The found context.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">drawerInstance is null</exception>
        public PropertyContext<TValue> Get<TValue>(OdinDrawer drawerInstance, string key) where TValue : new()
        {
            if (drawerInstance == null)
            {
                throw new ArgumentNullException("drawerInstance");
            }

            return this.Get<TValue>(drawerInstance.GetType(), key);
        }

        /// <summary>
        /// Gets a context value local to a drawer type for a given key, and creates a new instance of <see cref="T" /> as a default value if the context doesn't already exist.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <typeparam name="TDrawer">The type of the drawer.</typeparam>
        /// <param name="key">The key of the context value to get.</param>
        /// <returns>
        /// The found context.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">drawerInstance is null</exception>
        public PropertyContext<TValue> Get<TValue, TDrawer>(string key) where TValue : new() where TDrawer : OdinDrawer
        {
            return this.Get<TValue>(typeof(TDrawer), key);
        }

        /// <summary>
        /// Gets a context value local to a drawer type for a given key, and creates a new instance of <see cref="T" /> as a default value if the context doesn't already exist.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="drawerType">The type of the drawer.</param>
        /// <param name="key">The key of the context value to get.</param>
        /// <returns>
        /// The found context.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">drawerInstance is null</exception>
        public PropertyContext<TValue> Get<TValue>(Type drawerType, string key) where TValue : new()
        {
            PropertyContext<TValue> context;
            bool isNew;

            if (this.TryGetDrawerContext(drawerType, key, out context, out isNew) && isNew)
            {
                context.Value = new TValue();
            }

            return context;
        }

        /// <summary>
        /// Gets a context value local to a drawer type for a given key, and creates a new instance of <see cref="T" />
        /// as a default value if the context doesn't already exist.
        /// </summary>
        /// <returns>Returns true if a new context was created.</returns>
        public bool Get<TValue>(OdinDrawer drawerInstance, string key, out TValue context)
            where TValue : class, new()
        {
            PropertyContext<TValue> pContext;
            bool isNew;
            this.TryGetDrawerContext(drawerInstance.GetType(), key, out pContext, out isNew);

            if (isNew)
            {
                pContext.Value = new TValue();
            }

            context = pContext.Value;

            return isNew;
        }

        /// <summary>
        /// Gets a context value local to a drawer type for a given key, and creates a new instance of <see cref="T" />
        /// as a default value if the context doesn't already exist.
        /// </summary>
        /// <returns>Returns true if a new context was created.</returns>
        public bool Get<TValue>(OdinDrawer drawerInstance, string key, out PropertyContext<TValue> context)
        {
            bool isNew;
            this.TryGetDrawerContext(drawerInstance.GetType(), key, out context, out isNew);

            return isNew;
        }

        /// <summary>
        /// Gets a context value local to a drawer type for a given key, and creates a new instance of <see cref="T" />
        /// as a default value if the context doesn't already exist.
        /// </summary>
        /// <returns>Returns true if a new context was created.</returns>
        public bool Get<TValue>(Type drawerType, string key, out PropertyContext<TValue> context)
        {
            bool isNew;
            this.TryGetDrawerContext(drawerType, key, out context, out isNew);

            return isNew;
        }

        /// <summary>
        /// Gets a context value local to a drawer type for a given key, and creates a new instance of <see cref="T" />
        /// as a default value if the context doesn't already exist.
        /// </summary>
        /// <returns>Returns true if a new context was created.</returns>
        public bool Get<TValue, TDrawer>(string key, out PropertyContext<TValue> context)
        {
            bool isNew;
            this.TryGetDrawerContext(typeof(TDrawer), key, out context, out isNew);

            return isNew;
        }

        private bool TryGetDrawerContext<T>(Type drawerType, string key, out PropertyContext<T> context, out bool isNew)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            if (drawerType == null)
            {
                throw new ArgumentNullException("drawerType");
            }

            object value;

            var contexts = this.GetCurrentDrawerContexts();

            if (contexts.TryGetInnerValue(drawerType, key, out value))
            {
                isNew = false;
                context = value as PropertyContext<T>;

                if (context == null)
                {
                    throw new InvalidOperationException("Tried to get drawer property of type " + typeof(T).GetNiceName() + " with key " + key + " for drawer " + drawerType.GetNiceName() + " on property at path " + this.property.Path + ", but a drawer property for the same drawer type, of a different value type (" + value.GetType().GetArgumentsOfInheritedOpenGenericClass(typeof(PropertyContext<>))[0].GetNiceName() + ") already existed with the same key.");
                }
            }
            else
            {
                isNew = true;
                context = PropertyContext<T>.Create();
                contexts[drawerType][key] = context;
            }

            return true;
        }

        /// <summary>
        /// <para>Gets a temporary context value local to a drawer type for a given key, using a given delegate to generate a default value if the context doesn't already exist.</para>
        /// <para>Temporary context values are reset at the start of every GUI frame; arrays are set to default values, collections are cleared, and context types that implement <see cref="ITemporaryContext"/> have <see cref="ITemporaryContext.Reset"/> called.</para>
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="drawerInstance">An instance of the drawer type linked to the context value to get.</param>
        /// <param name="key">The key of the context value to get.</param>
        /// <param name="getDefaultValue">A delegate for generating a default value.</param>
        /// <returns>
        /// The found context.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">drawerInstance is null</exception>
        public TemporaryPropertyContext<TValue> GetTemporary<TValue>(OdinDrawer drawerInstance, string key, Func<TValue> getDefaultValue)
        {
            if (drawerInstance == null)
            {
                throw new ArgumentNullException("drawerInstance");
            }

            return this.GetTemporary(drawerInstance.GetType(), key, getDefaultValue);
        }

        /// <summary>
        /// <para>Gets a temporary context value local to a drawer type for a given key, using a given delegate to generate a default value if the context doesn't already exist.</para>
        /// <para>Temporary context values are reset at the start of every GUI frame; arrays are set to default values, collections are cleared, and context types that implement <see cref="ITemporaryContext" /> have <see cref="ITemporaryContext.Reset" /> called.</para>
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <typeparam name="TDrawer">The type of the drawer.</typeparam>
        /// <param name="key">The key of the context value to get.</param>
        /// <param name="getDefaultValue">A delegate for generating a default value.</param>
        /// <returns>
        /// The found context.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">drawerInstance is null</exception>
        public TemporaryPropertyContext<TValue> GetTemporary<TValue, TDrawer>(string key, Func<TValue> getDefaultValue) where TDrawer : OdinDrawer
        {
            return this.GetTemporary(typeof(TDrawer), key, getDefaultValue);
        }

        /// <summary>
        /// <para>Gets a temporary context value local to a drawer type for a given key, using a given delegate to generate a default value if the context doesn't already exist.</para>
        /// <para>Temporary context values are reset at the start of every GUI frame; arrays are set to default values, collections are cleared, and context types that implement <see cref="ITemporaryContext" /> have <see cref="ITemporaryContext.Reset" /> called.</para>
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="drawerType">The type of the drawer.</param>
        /// <param name="key">The key of the context value to get.</param>
        /// <param name="getDefaultValue">A delegate for generating a default value.</param>
        /// <returns>
        /// The found context.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">drawerInstance is null</exception>
        public TemporaryPropertyContext<TValue> GetTemporary<TValue>(Type drawerType, string key, Func<TValue> getDefaultValue)
        {
            TemporaryPropertyContext<TValue> context;
            bool isNew;

            if (this.TryGetTemporaryContext(drawerType, key, out context, out isNew) && isNew)
            {
                context.Value = getDefaultValue();
            }

            return context;
        }

        /// <summary>
        /// <para>Gets a temporary context value local to a drawer type for a given key, using a given default value if the context doesn't already exist.</para>
        /// <para>Temporary context values are reset at the start of every GUI frame; arrays are set to default values, collections are cleared, and context types that implement <see cref="ITemporaryContext"/> have <see cref="ITemporaryContext.Reset"/> called.</para>
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="drawerInstance">An instance of the drawer type linked to the context value to get.</param>
        /// <param name="key">The key of the context value to get.</param>
        /// <param name="defaultValue">The default value to set if the context value doesn't exist yet.</param>
        /// <returns>
        /// The found context.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">drawerInstance is null</exception>
        public TemporaryPropertyContext<TValue> GetTemporary<TValue>(OdinDrawer drawerInstance, string key, TValue defaultValue)
        {
            if (drawerInstance == null)
            {
                throw new ArgumentNullException("drawerInstance");
            }

            return this.GetTemporary(drawerInstance.GetType(), key, defaultValue);
        }

        /// <summary>
        /// <para>Gets a temporary context value local to a drawer type for a given key, using a given default value if the context doesn't already exist.</para>
        /// <para>Temporary context values are reset at the start of every GUI frame; arrays are set to default values, collections are cleared, and context types that implement <see cref="ITemporaryContext" /> have <see cref="ITemporaryContext.Reset" /> called.</para>
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <typeparam name="TDrawer">The type of the drawer.</typeparam>
        /// <param name="key">The key of the context value to get.</param>
        /// <param name="defaultValue">The default value to set if the context value doesn't exist yet.</param>
        /// <returns>
        /// The found context.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">drawerInstance is null</exception>
        public TemporaryPropertyContext<TValue> GetTemporary<TValue, TDrawer>(string key, TValue defaultValue) where TDrawer : OdinDrawer
        {
            return this.GetTemporary(typeof(TDrawer), key, defaultValue);
        }

        /// <summary>
        /// <para>Gets a temporary context value local to a drawer type for a given key, using a given default value if the context doesn't already exist.</para>
        /// <para>Temporary context values are reset at the start of every GUI frame; arrays are set to default values, collections are cleared, and context types that implement <see cref="ITemporaryContext" /> have <see cref="ITemporaryContext.Reset" /> called.</para>
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="drawerType">The type of the drawer.</param>
        /// <param name="key">The key of the context value to get.</param>
        /// <param name="defaultValue">The default value to set if the context value doesn't exist yet.</param>
        /// <returns>
        /// The found context.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">drawerInstance is null</exception>
        public TemporaryPropertyContext<TValue> GetTemporary<TValue>(Type drawerType, string key, TValue defaultValue)
        {
            TemporaryPropertyContext<TValue> context;
            bool isNew;

            if (this.TryGetTemporaryContext(drawerType, key, out context, out isNew) && isNew)
            {
                context.Value = defaultValue;
            }

            return context;
        }

        /// <summary>
        /// <para>Gets a temporary context value local to a drawer type for a given key, and creates a new instance of <see cref="T" /> as a default value if the context doesn't already exist.</para>
        /// <para>Temporary context values are reset at the start of every GUI frame; arrays are set to default values, collections are cleared, and context types that implement <see cref="ITemporaryContext" /> have <see cref="ITemporaryContext.Reset" /> called.</para>
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="drawerInstance">An instance of the drawer type linked to the context value to get.</param>
        /// <param name="key">The key of the context value to get.</param>
        /// <returns>
        /// The found context.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">drawerInstance is null</exception>
        public TemporaryPropertyContext<TValue> GetTemporary<TValue>(OdinDrawer drawerInstance, string key) where TValue : new()
        {
            if (drawerInstance == null)
            {
                throw new ArgumentNullException("drawerInstance");
            }

            return this.GetTemporary<TValue>(drawerInstance.GetType(), key);
        }

        /// <summary>
        /// <para>Gets a temporary context value local to a drawer type for a given key, and creates a new instance of <see cref="T" /> as a default value if the context doesn't already exist.</para>
        /// <para>Temporary context values are reset at the start of every GUI frame; arrays are set to default values, collections are cleared, and context types that implement <see cref="ITemporaryContext" /> have <see cref="ITemporaryContext.Reset" /> called.</para>
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <typeparam name="TDrawer">The type of the drawer.</typeparam>
        /// <param name="key">The key of the context value to get.</param>
        /// <returns>
        /// The found context.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">drawerInstance is null</exception>
        public TemporaryPropertyContext<TValue> GetTemporary<TValue, TDrawer>(string key) where TValue : new() where TDrawer : OdinDrawer
        {
            return this.GetTemporary<TValue>(typeof(TDrawer), key);
        }

        /// <summary>
        /// <para>Gets a temporary context value local to a drawer type for a given key, and creates a new instance of <see cref="T" /> as a default value if the context doesn't already exist.</para>
        /// <para>Temporary context values are reset at the start of every GUI frame; arrays are set to default values, collections are cleared, and context types that implement <see cref="ITemporaryContext" /> have <see cref="ITemporaryContext.Reset" /> called.</para>
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="drawerType">The type of the drawer.</param>
        /// <param name="key">The key of the context value to get.</param>
        /// <returns>
        /// The found context.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">drawerInstance is null</exception>
        public TemporaryPropertyContext<TValue> GetTemporary<TValue>(Type drawerType, string key) where TValue : new()
        {
            TemporaryPropertyContext<TValue> context;
            bool isNew;

            if (this.TryGetTemporaryContext(drawerType, key, out context, out isNew) && isNew)
            {
                context.Value = new TValue();
            }

            return context;
        }

        private bool TryGetTemporaryContext<T>(Type drawerType, string key, out TemporaryPropertyContext<T> context, out bool isNew)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            if (drawerType == null)
            {
                throw new ArgumentNullException("drawerType");
            }

            var contexts = this.GetCurrentTemporaryContexts();

            ITemporaryContext value;

            if (contexts.TryGetInnerValue(drawerType, key, out value))
            {
                isNew = false;
                context = value as TemporaryPropertyContext<T>;

                if (context == null)
                {
                    throw new InvalidOperationException("Tried to get temporary property of type " + typeof(T).GetNiceName() + " with key " + key + " for drawer " + drawerType.GetNiceName() + " on property at path " + this.property.Path + ", but a temporary property for the same drawer type, of a different value type (" + value.GetType().GetArgumentsOfInheritedOpenGenericClass(typeof(PropertyContext<>))[0].GetNiceName() + ") already existed with the same key.");
                }
            }
            else
            {
                isNew = true;
                context = new TemporaryPropertyContext<T>();
                contexts[drawerType][key] = context;
            }

            return true;
        }

        /// <summary>
        /// Resets all temporary contexts for the property.
        /// </summary>
        public void ResetTemporaryContexts()
        {
            if (this.temporaryContexts != null)
            {
                for (int i = 0; i < this.temporaryContexts.Count; i++)
                {
                    Dictionary<int, DoubleLookupDictionary<Type, string, ITemporaryContext>> innerDict;

                    if (this.temporaryContexts.TryGetValue(i, out innerDict))
                    {
                        for (int j = 0; j < innerDict.Count; j++)
                        {
                            DoubleLookupDictionary<Type, string, ITemporaryContext> innerInnerDict;

                            if (innerDict.TryGetValue(j, out innerInnerDict))
                            {
                                foreach (var configSet in innerInnerDict.GFValueIterator())
                                {
                                    foreach (var config in configSet.GFValueIterator())
                                    {
                                        config.Reset();
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (this.globalTemporaryContexts != null)
            {
                foreach (var context in this.globalTemporaryContexts.GFValueIterator())
                {
                    context.Reset();
                }
            }
        }

        /// <summary>
        /// Swaps context values with a given <see cref="PropertyContextContainer"/>.
        /// </summary>
        /// <param name="otherContext">The context to swap with.</param>
        public void SwapContext(PropertyContextContainer otherContext)
        {
            // Swap global configs
            {
                var temp = otherContext.globalContexts;
                otherContext.globalContexts = this.globalContexts;
                this.globalContexts = temp;
            }

            // Swap drawer configs
            {
                var temp = otherContext.drawerContexts;
                otherContext.drawerContexts = this.drawerContexts;
                this.drawerContexts = temp;
            }

            // Swap temporary configs
            {
                var temp = otherContext.temporaryContexts;
                otherContext.temporaryContexts = this.temporaryContexts;
                this.temporaryContexts = temp;
            }
        }

        private DoubleLookupDictionary<Type, string, object> GetCurrentDrawerContexts()
        {
            DoubleLookupDictionary<Type, string, object> contexts;

            if (!this.drawerContexts.TryGetInnerValue(this.property.DrawCount, this.property.DrawerChainIndex, out contexts))
            {
                contexts = new DoubleLookupDictionary<Type, string, object>();
                this.drawerContexts[this.property.DrawCount][this.property.DrawerChainIndex] = contexts;
            }

            return contexts;
        }

        private DoubleLookupDictionary<Type, string, ITemporaryContext> GetCurrentTemporaryContexts()
        {
            DoubleLookupDictionary<Type, string, ITemporaryContext> contexts;

            if (!this.temporaryContexts.TryGetInnerValue(this.property.DrawCount, this.property.DrawerChainIndex, out contexts))
            {
                contexts = new DoubleLookupDictionary<Type, string, ITemporaryContext>();
                this.temporaryContexts[this.property.DrawCount][this.property.DrawerChainIndex] = contexts;
            }

            return contexts;
        }

        /// <summary>
        /// Gets a <see cref="GlobalPersistentContext{T}"/> object and creates a <see cref="LocalPersistentContext{T}"/> object for it.
        /// </summary>
        /// <typeparam name="TValue">The type of the value of the context.</typeparam>
        /// <param name="drawer">The instance of the drawer.</param>
        /// <param name="key">The key for the context.</param>
        /// <param name="defaultValue">The default value for the context.</param>
        public LocalPersistentContext<TValue> GetPersistent<TValue>(OdinDrawer drawer, string key, TValue defaultValue)
        {
            PropertyContext<LocalPersistentContext<TValue>> context;
            if (this.Get<LocalPersistentContext<TValue>>(drawer, key, out context))
            {
                context.Value = LocalPersistentContext<TValue>.Create(PersistentContext.Get(
                    TwoWaySerializationBinder.Default.BindToName(drawer.GetType()),
                    TwoWaySerializationBinder.Default.BindToName(this.property.Tree.TargetType),
                    this.property.Path,
                    new DrawerStateSignature(this.property.RecursiveDrawDepth, InlineEditorAttributeDrawer.CurrentInlineEditorDrawDepth, this.property.DrawerChainIndex),
                    key,
                    defaultValue));
            }

            return context.Value;
        }

        /// <summary>
        /// Gets a <see cref="GlobalPersistentContext{T}"/> object and creates a <see cref="LocalPersistentContext{T}"/> object for it.
        /// Returns <c>true</c> when the <see cref="GlobalPersistentContext{T}"/> is first created. Otherwise <c>false</c>.
        /// </summary>
        /// <typeparam name="TValue">The type of the value of the context.</typeparam>
        /// <param name="drawer">The instance of the drawer.</param>
        /// <param name="key">The key for the context.</param>
        /// <param name="context">The <see cref="LocalPersistentContext{T}"/> object.</param>
        /// <returns>Returns <c>true</c> when the <see cref="GlobalPersistentContext{T}"/> is first created. Otherwise <c>false</c>.</returns>
        public bool GetPersistent<TValue>(OdinDrawer drawer, string key, out LocalPersistentContext<TValue> context)
        {
            bool isNew = false;
            PropertyContext<LocalPersistentContext<TValue>> propertyContext;
            if (this.Get<LocalPersistentContext<TValue>>(drawer, key, out propertyContext))
            {
                GlobalPersistentContext<TValue> global;
                isNew = PersistentContext.Get(
                    TwoWaySerializationBinder.Default.BindToName(drawer.GetType()),
                    TwoWaySerializationBinder.Default.BindToName(this.property.Tree.TargetType),
                    this.property.Path,
                    new DrawerStateSignature(this.property.RecursiveDrawDepth, InlineEditorAttributeDrawer.CurrentInlineEditorDrawDepth, this.property.DrawerChainIndex),
                    key,
                    out global);

                propertyContext.Value = LocalPersistentContext<TValue>.Create(global);
            }

            context = propertyContext.Value;

            return isNew;
        }

        [Serializable]
        private struct DrawerStateSignature : IEquatable<DrawerStateSignature>
        {
            public int RecursiveDrawDepth;
            public int CurrentInlineEditorDrawDepth;
            public int DrawerChainIndex;

            public DrawerStateSignature(int recursiveDrawDepth, int currentInlineEditorDrawDepth, int drawerChainIndex)
            {
                this.RecursiveDrawDepth = recursiveDrawDepth;
                this.CurrentInlineEditorDrawDepth = currentInlineEditorDrawDepth;
                this.DrawerChainIndex = drawerChainIndex;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = 17;
                    hash = hash * 31 + this.RecursiveDrawDepth;
                    hash = hash * 31 + this.CurrentInlineEditorDrawDepth;
                    hash = hash * 31 + this.DrawerChainIndex;
                    return hash;
                }
            }

            public override bool Equals(object obj)
            {
                return obj is DrawerStateSignature && this.Equals((DrawerStateSignature)obj);
            }

            public bool Equals(DrawerStateSignature other)
            {
                return this.RecursiveDrawDepth == other.RecursiveDrawDepth
                    && this.CurrentInlineEditorDrawDepth == other.CurrentInlineEditorDrawDepth
                    && this.DrawerChainIndex == other.DrawerChainIndex;
            }
        }
    }
}
#endif