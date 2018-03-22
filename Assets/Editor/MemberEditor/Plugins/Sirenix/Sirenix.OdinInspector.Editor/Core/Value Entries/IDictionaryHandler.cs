#if UNITY_EDITOR
namespace Sirenix.OdinInspector.Editor
{
    /// <summary>
    /// <para>An interface that provides various utilities for modifying and querying dictionary values represented by a <see cref="PropertyValueEntry"/>.</para>
    /// <para>It is also responsible for translating and ordering dictionary keys into persistent indices.</para>
    /// </summary>
    public interface IDictionaryHandler
    {
        /// <summary>
        /// Whether the dictionary represented by this handler supports prefab modifications.
        /// </summary>
        bool SupportsPrefabModifications { get; }

        /// <summary>
        /// Gets the key value at the given selection and dictionary index.
        /// </summary>
        object GetKey(int selectionIndex, int childIndex);

        /// <summary>
        /// Gets the key value at the given index from the given dictionary.
        /// </summary>
        object GetKey(object dictionary, int childIndex);

        /// <summary>
        /// Queues a remove modification for a given key. Modifications are applied in Repaint.
        /// </summary>
        void Remove(object key);

        /// <summary>
        /// Queues a set value modification for a given key. Modifications are applied in Repaint.
        /// </summary>
        void SetValue(object key, object value);

        /// <summary>
        /// Apply all queued changes, and apply prefab modifications if applicable. This method only does something during Repaint.
        /// </summary>
        /// <returns>true if any changes were made, otherwise false</returns>
        bool ApplyChanges();

        /// <summary>
        /// Force the dictionary handler to update its internal dictionary index mappings.
        /// </summary>
        void ForceUpdate();
    }

    /// <summary>
    /// <para>An interface that provides various utilities for modifying and querying dictionary values represented by a <see cref="PropertyValueEntry"/>.</para>
    /// <para>It is also responsible for translating and ordering dictionary keys into persistent indices.</para>
    /// </summary>
    public interface IDictionaryHandler<TKey> : IDictionaryHandler
    {
        /// <summary>
        /// Gets the key value at the given selection and dictionary index.
        /// </summary>
        new TKey GetKey(int selectionIndex, int childIndex);

        /// <summary>
        /// Gets the key value at the given index from the given dictionary.
        /// </summary>
        new TKey GetKey(object dictionary, int childIndex);

        /// <summary>
        /// Queues a remove modification for a given key. Modifications are applied in Repaint.
        /// </summary>
        void Remove(TKey key);

        /// <summary>
        /// Queues a set value modification for a given key. Modifications are applied in Repaint.
        /// </summary>
        void SetValue(TKey key, object value);
    }
}
#endif