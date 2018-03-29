namespace Sirenix.Utilities
{
	using System;
	using System.Collections;
	using System.Collections.Generic;

	/// <summary>
	/// Not yet documented.
	/// </summary>
	[Serializable]
	public class IndexedDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IDictionary
	{
		private Dictionary<TKey, TValue> dictionary;

		private List<TKey> indexer;

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public ICollection<TKey> Keys { get { return this.dictionary.Keys; } }

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public ICollection<TValue> Values { get { return this.dictionary.Values; } }

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public int Count { get { return this.dictionary.Count; } }

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public bool IsReadOnly { get { return ((IDictionary)this.dictionary).IsReadOnly; } }

		bool IDictionary.IsFixedSize { get { return ((IDictionary)this.dictionary).IsFixedSize; } }

		bool IDictionary.IsReadOnly { get { return ((IDictionary)this.dictionary).IsReadOnly; } }

		ICollection IDictionary.Keys { get { return ((IDictionary)this.dictionary).Keys; } }

		ICollection IDictionary.Values
		{
			get { return ((IDictionary)this.dictionary).Values; }
		}

		int ICollection.Count { get { return ((IDictionary)this.dictionary).Count; } }

		bool ICollection.IsSynchronized { get { return ((IDictionary)this.dictionary).IsSynchronized; } }

		object ICollection.SyncRoot
		{
			get
			{
				return ((IDictionary)this.dictionary).SyncRoot;
			}
		}

		object IDictionary.this[object key]
		{
			get
			{
				if (!(key is TKey))
				{
					throw new InvalidOperationException("Wrong key type.");
				}

				return this[(TKey)key];
			} 
			set
			{
				if (!(key is TKey && value is TValue))
				{
					throw new InvalidOperationException("Wrong type.");
				}

				this[(TKey)key] = (TValue)value;
			}
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public TValue this[TKey key]
		{
			get { return this.dictionary[key]; } 
			set
			{
				if (this.dictionary.ContainsKey(key))
				{
					this.dictionary[key] = value;
				}
				else
				{
					this.Add(key, value);
				}
			}
		}

        /// <summary>
        /// Not yet documented.
        /// </summary>
		public IndexedDictionary()
		{
			this.dictionary = new Dictionary<TKey, TValue>(0);
			this.indexer = new List<TKey>(0);
		}

        /// <summary>
        /// Not yet documented.
        /// </summary>
        /// <param name="capacity">Not yet documented.</param>
		public IndexedDictionary(int capacity)
		{
			this.dictionary = new Dictionary<TKey, TValue>(capacity);
			this.indexer = new List<TKey>(capacity);
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public KeyValuePair<TKey, TValue> Get(int index)
		{
			var k = this.indexer[index];
			return new KeyValuePair<TKey, TValue>(k, this[k]);
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public TKey GetKey(int index)
		{
			return this.indexer[index];
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public TValue GetValue(int index)
		{
			return this[this.indexer[index]];
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public int IndexOf(TKey key)
		{
			return this.indexer.IndexOf(key);
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		public void Add(TKey key, TValue value)
		{
			this.dictionary.Add(key, value);
			this.indexer.Add(key);
		}
		
		/// <summary>
		/// Not yet documented.
		/// </summary>
		public void Clear()
		{
			this.indexer.Clear();
			this.dictionary.Clear();
		}
		
		/// <summary>
		/// Not yet documented.
		/// </summary>
		public bool Remove(TKey key)
		{
			this.indexer.Remove(key);
			return this.dictionary.Remove(key);
		}
		
		/// <summary>
		/// Not yet documented.
		/// </summary>
		public void RemoveAt(int index)
		{
			if (index >= 0 && index < this.Count)
			{
				this.dictionary.Remove(this.indexer[index]);
				this.indexer.RemoveAt(index);
			}
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public bool ContainsKey(TKey key)
		{
			return this.dictionary.ContainsKey(key);
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public bool TryGetValue(TKey key, out TValue value)
		{
			return this.dictionary.TryGetValue(key, out value);
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public void Add(KeyValuePair<TKey, TValue> item)
		{
			this.dictionary.Add(item.Key, item.Value);
			this.indexer.Add(item.Key);
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public bool Contains(KeyValuePair<TKey, TValue> item)
		{
			return this.dictionary.ContainsKey(item.Key) && this.dictionary.ContainsValue(item.Value);
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public bool Remove(KeyValuePair<TKey, TValue> item)
		{
			this.indexer.Remove(item.Key);
			return this.dictionary.Remove(item.Key);
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			return this.dictionary.GetEnumerator();
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		void IDictionary.Add(object key, object value)
		{
			if (!(key is TKey && value is TValue))
			{
				throw new InvalidOperationException("Wrong type.");
			}

			this.Add((TKey)key, (TValue)value);
		}

		void IDictionary.Clear()
		{
			this.Clear();
		}

		bool IDictionary.Contains(object key)
		{
			if (!(key is TKey))
			{
				throw new InvalidOperationException("Wrong key type.");
			}

			return this.ContainsKey((TKey)key);
		}

		IDictionaryEnumerator IDictionary.GetEnumerator()
		{
			return (IDictionaryEnumerator)this.GetEnumerator();
		}

		void IDictionary.Remove(object key)
		{
			if (!(key is TKey))
			{
				throw new InvalidOperationException("Wrong key type.");
			}
		}

		void ICollection.CopyTo(Array array, int index)
		{
			throw new NotImplementedException();
		}
	}
}
