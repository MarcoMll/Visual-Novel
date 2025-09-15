using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomInspector
{
    /// <summary>
    /// Only valid for SerializableSet's! Used to for display in the inspector
    /// </summary>
    public class SetAttribute : PropertyAttribute { }

    /// <summary>
    /// Only valid for SerializableSet! Used to for display in the inspector
    /// </summary>
    [Obsolete("Use the " + nameof(SetAttribute) + " instead")]
    public class SerializableSetAttribute : PropertyAttribute { }

    /// <summary>
    /// A list without duplicates
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [System.Serializable]
    public class SerializableSet<T> : ICollection, ICollection<T>, IEnumerable, IEnumerable<T>
    {
#if UNITY_EDITOR
        [MessageBox("Use the [SetAttribute] attribute for displaying in the inspector", MessageBoxType.Error)]
        [SerializeField, HideField] bool info;
#endif

        [SerializeField, HideInInspector]
        protected List<T> values = new();
        public int Count => values.Count;

        
        /// <summary>
        /// Adds an element to the set
        /// </summary>
        /// <exception cref="ArgumentException">element already exists</exception>
        public void Add(T item)
        {
            if (!TryAdd(item))
                throw new ArgumentException($"Item '{item}' already existed in set");
        }

        /// <summary>
        /// Adds an element to the end of the set if not already exists
        /// </summary>
        /// <returns>true if item did not already existed</returns>
        public virtual bool TryAdd(T item)
        {
            if (values.Contains(item))
            {
                return false;
            }
            else
            {
                values.Add(item);
                return true;
            }
        }

        /// <returns>true if item was removed successfully</returns>
        public virtual bool Remove(T item)
        {
            return values.Remove(item);
        }

        /// <summary>
        /// Removes element at index
        /// </summary>
        public void RemoveAt(int index)
            => values.RemoveAt(index);

        /// <returns>
        /// The index of the item.
        /// <para> -1 if the item doesnt exist </para>
        /// </returns>
        public virtual int GetIndexOf(T item)
        {
            for (int i = 0; i < values.Count; i++)
            {
                if(values[i].Equals(item))
                {
                    return i;
                }
            }
            return -1;
        }

        public T GetByIndex(int index)
            => values[index];

        public void Clear()
        {
            values.Clear();
        }

        /// <returns>If item exist in set</returns>
        public virtual bool Contains(T item)
        {
            return values.Contains(item);
        }

        /*public static explicit operator SerializableSortedSet<TKey, TValue>(SerializableSet<TKey, TValue> s)
        {
            s 
            List<(TKey, TValue)> res = new();
            for (int i = 0; i < d.Count; i++)
            {
                res.Add((d.keys.GetByIndex(i), d.values[i]));
            }
            return res;
        }*/

        IEnumerator IEnumerable.GetEnumerator()
        {
            return values.GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (T value in values)
            {
                yield return value;
            }
        }

        public static explicit operator List<T>(SerializableSet<T> s)
            => s.values;

        void ICollection.CopyTo(Array array, int index)
        {
            if (index + Count > array.Length)
                throw new ArgumentException("The number of elements in the source Dictionary is greater than the available space from index to the end of the destination array");

            for (int i = index + Count - 1; i >= index; i--)
            {
                array.SetValue(values[i], i);
            }
        }

        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            if (arrayIndex + Count > array.Length)
                throw new ArgumentException("The number of elements in the source Dictionary is greater than the available space from index to the end of the destination array");

            for (int i = arrayIndex + Count - 1; i >= arrayIndex; i--)
            {
                array[i] = values[i];
            }
        }

        bool ICollection.IsSynchronized => true;

        object ICollection.SyncRoot => false;

        bool ICollection<T>.IsReadOnly => false;

#if UNITY_EDITOR
        /// <summary>
        /// This is just for editorPurpose.
        /// </summary>
        [SerializeField]
        T editor_input;
#endif
    }
}

