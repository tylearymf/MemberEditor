#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="PersistentContext.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Provides context objects that still persist when Unity reloads or is restartet.
	/// </summary>
	public static class PersistentContext
	{
		/// <summary>
		/// Gets a GlobalPersistentContext object for the specified key.
		/// </summary>
		/// <typeparam name="TKey1">The type of the first key.</typeparam>
		/// <typeparam name="TValue">The type of the value stored in the context object.</typeparam>
		/// <param name="alphaKey">The first key.</param>
		/// <param name="defaultValue">The default value, used for when the context object is first created.</param>
		public static GlobalPersistentContext<TValue> Get<TKey1, TValue>(TKey1 alphaKey, TValue defaultValue)
		{
			bool isNew;
			GlobalPersistentContext<TValue> context = PersistentContextCache.Instance.GetContext<TKey1, TValue>(alphaKey, out isNew);

			if (isNew)
			{
				context.Value = defaultValue;
			}

			return context;
		}
		
		/// <summary>
		/// Gets a GlobalPersistentContext object for the specified keys.
		/// </summary>
		/// <typeparam name="TKey1">The type of the first key.</typeparam>
		/// <typeparam name="TKey2">The type of the second key.</typeparam>
		/// <typeparam name="TValue">The type of the value stored in the context object.</typeparam>
		/// <param name="alphaKey">The first key.</param>
		/// <param name="betaKey">The second key.</param>
		/// <param name="defaultValue">The default value, used for when the context object is first created.</param>
		public static GlobalPersistentContext<TValue> Get<TKey1, TKey2, TValue>(TKey1 alphaKey, TKey2 betaKey, TValue defaultValue)
		{
			bool isNew;
			GlobalPersistentContext<TValue> context = PersistentContextCache.Instance.GetContext<TKey1, TKey2, TValue>(alphaKey, betaKey, out isNew);

			if (isNew)
			{
				context.Value = defaultValue;
			}

			return context;
		}
		
		/// <summary>
		/// Gets a GlobalPersistentContext object for the specified keys.
		/// </summary>
		/// <typeparam name="TKey1">The type of the first key.</typeparam>
		/// <typeparam name="TKey2">The type of the second key.</typeparam>
		/// <typeparam name="TKey3">The type of the third key.</typeparam>
		/// <typeparam name="TValue">The type of the value stored in the context object.</typeparam>
		/// <param name="alphaKey">The first key.</param>
		/// <param name="betaKey">The second key.</param>
		/// <param name="gammaKey">The third key.</param>
		/// <param name="defaultValue">The default value, used for when the context object is first created.</param>
		public static GlobalPersistentContext<TValue> Get<TKey1, TKey2, TKey3, TValue>(TKey1 alphaKey, TKey2 betaKey, TKey3 gammaKey, TValue defaultValue)
		{
			bool isNew;
			GlobalPersistentContext<TValue> context = PersistentContextCache.Instance.GetContext<TKey1, TKey2, TKey3, TValue>(alphaKey, betaKey, gammaKey, out isNew);

			if (isNew)
			{
				context.Value = defaultValue;
			}

			return context;
		}
		
		/// <summary>
		/// Gets a GlobalPersistentContext object for the specified keys.
		/// </summary>
		/// <typeparam name="TKey1">The type of the first key.</typeparam>
		/// <typeparam name="TKey2">The type of the second key.</typeparam>
		/// <typeparam name="TKey3">The type of the third key.</typeparam>
		/// <typeparam name="TKey4">The type of the fourth key.</typeparam>
		/// <typeparam name="TValue">The type of the value stored in the context object.</typeparam>
		/// <param name="alphaKey">The first key.</param>
		/// <param name="betaKey">The second key.</param>
		/// <param name="gammaKey">The third key.</param>
		/// <param name="deltaKey">The fourth key.</param>
		/// <param name="defaultValue">The default value, used for when the context object is first created.</param>
		public static GlobalPersistentContext<TValue> Get<TKey1, TKey2, TKey3, TKey4, TValue>(TKey1 alphaKey, TKey2 betaKey, TKey3 gammaKey, TKey4 deltaKey, TValue defaultValue)
		{
			bool isNew;
			GlobalPersistentContext<TValue> context = PersistentContextCache.Instance.GetContext<TKey1, TKey2, TKey3, TKey4, TValue>(alphaKey, betaKey, gammaKey, deltaKey, out isNew);

			if (isNew)
			{
				context.Value = defaultValue;
			}

			return context;
		}
		
		/// <summary>
		/// Gets a GlobalPersistentContext object for the specified keys.
		/// </summary>
		/// <typeparam name="TKey1">The type of the first key.</typeparam>
		/// <typeparam name="TKey2">The type of the second key.</typeparam>
		/// <typeparam name="TKey3">The type of the third key.</typeparam>
		/// <typeparam name="TKey4">The type of the fourth key.</typeparam>
		/// <typeparam name="TKey5">The type of the fifth key.</typeparam>
		/// <typeparam name="TValue">The type of the value stored in the context object.</typeparam>
		/// <param name="alphaKey">The first key.</param>
		/// <param name="betaKey">The second key.</param>
		/// <param name="gammaKey">The third key.</param>
		/// <param name="deltaKey">The fourth key.</param>
		/// <param name="epsilonKey">The fifth key.</param>
		/// <param name="defaultValue">The default value, used for when the context object is first created.</param>
		public static GlobalPersistentContext<TValue> Get<TKey1, TKey2, TKey3, TKey4, TKey5, TValue>(TKey1 alphaKey, TKey2 betaKey, TKey3 gammaKey, TKey4 deltaKey, TKey5 epsilonKey, TValue defaultValue)
		{
			bool isNew;
			GlobalPersistentContext<TValue> context = PersistentContextCache.Instance.GetContext<TKey1, TKey2, TKey3, TKey4, TKey5, TValue>(alphaKey, betaKey, gammaKey, deltaKey, epsilonKey, out isNew);

			if (isNew)
			{
				context.Value = defaultValue;
			}

			return context;
		}

		/// <summary>
		/// Gets a GlobalPersistentContext object for the specified key.
		/// Returns <c>true</c> when the context is first created. Otherwise <c>false</c>.
		/// </summary>
		/// <typeparam name="TKey1">The type of the first key.</typeparam>
		/// <typeparam name="TValue">The type of the value stored in the context object.</typeparam>
		/// <param name="alphaKey">The first key.</param>
		/// <param name="context">The persistent context object.</param>
		/// <returns>Returns <c>true</c> when the context is first created. Otherwise <c>false</c>.</returns>
		public static bool Get<TKey1, TValue>(TKey1 alphaKey, out GlobalPersistentContext<TValue> context)
		{
			bool isNew;
			context = PersistentContextCache.Instance.GetContext<TKey1, TValue>(alphaKey, out isNew);

			return isNew;
		}
		
		/// <summary>
		/// Gets a GlobalPersistentContext object for the specified keys.
		/// Returns <c>true</c> when the context is first created. Otherwise <c>false</c>.
		/// </summary>
		/// <typeparam name="TKey1">The type of the first key.</typeparam>
		/// <typeparam name="TKey2">The type of the second key.</typeparam>
		/// <typeparam name="TValue">The type of the value stored in the context object.</typeparam>
		/// <param name="alphaKey">The first key.</param>
		/// <param name="betaKey">The second key.</param>
		/// <param name="context">The persistent context object.</param>
		/// <returns>Returns <c>true</c> when the context is first created. Otherwise <c>false</c>.</returns>
		public static bool Get<TKey1, TKey2, TValue>(TKey1 alphaKey, TKey2 betaKey, out GlobalPersistentContext<TValue> context)
		{
			bool isNew;
			context = PersistentContextCache.Instance.GetContext<TKey1, TKey2, TValue>(alphaKey, betaKey, out isNew);

			return isNew;
		}
		
		/// <summary>
		/// Gets a GlobalPersistentContext object for the specified keys.
		/// Returns <c>true</c> when the context is first created. Otherwise <c>false</c>.
		/// </summary>
		/// <typeparam name="TKey1">The type of the first key.</typeparam>
		/// <typeparam name="TKey2">The type of the second key.</typeparam>
		/// <typeparam name="TKey3">The type of the third key.</typeparam>
		/// <typeparam name="TValue">The type of the value stored in the context object.</typeparam>
		/// <param name="alphaKey">The first key.</param>
		/// <param name="betaKey">The second key.</param>
		/// <param name="gammaKey">The third key.</param>
		/// <param name="context">The persistent context object.</param>
		/// <returns>Returns <c>true</c> when the context is first created. Otherwise <c>false</c>.</returns>
		public static bool Get<TKey1, TKey2, TKey3, TValue>(TKey1 alphaKey, TKey2 betaKey, TKey3 gammaKey, out GlobalPersistentContext<TValue> context)
		{
			bool isNew;
			context = PersistentContextCache.Instance.GetContext<TKey1, TKey2, TKey3, TValue>(alphaKey, betaKey, gammaKey, out isNew);

			return isNew;
		}
		
		/// <summary>
		/// Gets a GlobalPersistentContext object for the specified keys.
		/// Returns <c>true</c> when the context is first created. Otherwise <c>false</c>.
		/// </summary>
		/// <typeparam name="TKey1">The type of the first key.</typeparam>
		/// <typeparam name="TKey2">The type of the second key.</typeparam>
		/// <typeparam name="TKey3">The type of the third key.</typeparam>
		/// <typeparam name="TKey4">The type of the fourth key.</typeparam>
		/// <typeparam name="TValue">The type of the value stored in the context object.</typeparam>
		/// <param name="alphaKey">The first key.</param>
		/// <param name="betaKey">The second key.</param>
		/// <param name="gammaKey">The third key.</param>
		/// <param name="deltaKey">The fourth key.</param>
		/// <param name="context">The persistent context object.</param>
		/// <returns>Returns <c>true</c> when the context is first created. Otherwise <c>false</c>.</returns>
		public static bool Get<TKey1, TKey2, TKey3, TKey4, TValue>(TKey1 alphaKey, TKey2 betaKey, TKey3 gammaKey, TKey4 deltaKey, out GlobalPersistentContext<TValue> context)
		{
			bool isNew;
			context = PersistentContextCache.Instance.GetContext<TKey1, TKey2, TKey3, TKey4, TValue>(alphaKey, betaKey, gammaKey, deltaKey, out isNew);

			return isNew;
		}
		
		/// <summary>
		/// Gets a GlobalPersistentContext object for the specified keys.
		/// Returns <c>true</c> when the context is first created. Otherwise <c>false</c>.
		/// </summary>
		/// <typeparam name="TKey1">The type of the first key.</typeparam>
		/// <typeparam name="TKey2">The type of the second key.</typeparam>
		/// <typeparam name="TKey3">The type of the third key.</typeparam>
		/// <typeparam name="TKey4">The type of the fourth key.</typeparam>
		/// <typeparam name="TKey5">The type of the fifth key.</typeparam>
		/// <typeparam name="TValue">The type of the value stored in the context object.</typeparam>
		/// <param name="alphaKey">The first key.</param>
		/// <param name="betaKey">The second key.</param>
		/// <param name="gammaKey">The third key.</param>
		/// <param name="deltaKey">The fourth key.</param>
		/// <param name="epsilonKey">The fifth key.</param>
		/// <param name="context">The persistent context object.</param>
		/// <returns>Returns <c>true</c> when the context is first created. Otherwise <c>false</c>.</returns>
		public static bool Get<TKey1, TKey2, TKey3, TKey4, TKey5, TValue>(TKey1 alphaKey, TKey2 betaKey, TKey3 gammaKey, TKey4 deltaKey, TKey5 epsilonKey, out GlobalPersistentContext<TValue> context)
		{
			bool isNew;
			context = PersistentContextCache.Instance.GetContext<TKey1, TKey2, TKey3, TKey4, TKey5, TValue>(alphaKey, betaKey, gammaKey, deltaKey, epsilonKey, out isNew);

			return isNew;
		}
	}
}
#endif