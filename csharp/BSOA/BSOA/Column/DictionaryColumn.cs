// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

using BSOA.Collections;
using BSOA.GC;
using BSOA.IO;
using BSOA.Model;

namespace BSOA.Column
{
    /// <summary>
    ///  DictionaryColumn stores a Dictionary for each row.
    /// </summary>
    /// <remarks>
    ///  DictionaryColumn really stores a huge flat list of Key/Value pairs across every per-row Dictionary.
    ///  
    ///  To find the Key/Value pairs in one particular row's Dictionary, we get the 'Pairs' list.
    ///  Each integer in the pairs list is the index of a Key and Value in the Keys and Values column which matches
    ///  and which belongs to the outer row's Dictionary.
    ///  
    ///  NOTE: Keys are required to be sorted in a standard way (Ordinal for string, default comparison otherwise).
    ///  Sorting makes finding a key O(log N) instead of O(N)
    ///  Insertion is O(N), because other keys must be shifted, but shifting is fast.
    ///  Insertion with unsorted keys is O(N) anyway, because all existing keys must be checked before insert.
    /// </remarks>
    /// <typeparam name="TKey">Type of Dictionary entry keys</typeparam>
    /// <typeparam name="TValue">Type of Dictionary entry values</typeparam>
    public class DictionaryColumn<TKey, TValue> : LimitedList<IDictionary<TKey, TValue>>, IColumn<IDictionary<TKey, TValue>> where TKey : IComparable<TKey>
    {
        internal IColumn<TKey> _keys;
        internal IColumn<TValue> _values;
        internal IColumn<NumberList<int>> _pairs;
        internal IComparer<TKey> _keyComparer;
        private NumberListColumn<int> _pairsInner;
        private CacheItem<ColumnDictionary<TKey, TValue>> _cache;

        public DictionaryColumn(IColumn<TKey> keys, IColumn<TValue> values, Nullability nullability = Nullability.DefaultToNull)
        {
            _keys = keys;
            _values = values;
            _pairsInner = new NumberListColumn<int>();
            _pairs = NullableColumn<NumberList<int>>.Wrap(_pairsInner, nullability);

            // Comparer for sorting Dictionary Keys, picked up by ColumnDictionary. 
            // Comparer not exposed as argument because column won't work if serialized Dictionaries were sorted differently than comparer would sort.
            _keyComparer = (typeof(TKey) == typeof(string) ? (IComparer<TKey>)StringComparer.Ordinal : new DefaultComparer<TKey>());
        }

        // ColumnFactory untyped constructor
        public DictionaryColumn(IColumn keys, IColumn values, object defaultValue) : this((IColumn<TKey>)keys, (IColumn<TValue>)values, (defaultValue == null ? Nullability.DefaultToNull : Nullability.DefaultToEmpty))
        { }

        public override IDictionary<TKey, TValue> this[int index]
        {
            get
            {
                CacheItem<ColumnDictionary<TKey, TValue>> item = _cache;
                if (item?.RowIndex != index)
                {
                    item = new CacheItem<ColumnDictionary<TKey, TValue>>(index, ColumnDictionary<TKey, TValue>.Get(this, index));
                    _cache = item;
                }

                return item.Value;
            }

            set
            {
                _cache = default;
                ColumnDictionary<TKey, TValue>.Set(this, index, value);
            }
        }

        public override int Count => _pairs.Count;

        public override void Clear()
        {
            _cache = default;
            _keys.Clear();
            _values.Clear();
            _pairs.Clear();
        }

        public override void Swap(int index1, int index2)
        {
            _cache = default;
            _pairs.Swap(index1, index2);
        }

        public override void RemoveFromEnd(int count)
        {
            _cache = default;
            _pairs.RemoveFromEnd(count);
        }

        public void Trim()
        {
            if (Count == 0) { return; }
            _pairs.Trim();

            // Find Key/Value pairs no longer in any Dictionaries
            BitVector rowsToKeep = new BitVector(false, _keys.Count);
            _pairsInner.ForEach((slice) => IntRemapper.Instance.AddValues(slice, rowsToKeep));

            // Remove those from Keys and Values
            GarbageCollector.Collect<int>(_keys, null, rowsToKeep);
            GarbageCollector.Collect<int>(_values, new [] { _pairsInner }, rowsToKeep);
        }

        public void Write(ITreeWriter writer)
        {
            writer.WriteStartObject();
            writer.Write(Names.Keys, _keys);
            writer.Write(Names.Values, _values);
            writer.Write(Names.Pairs, _pairs);
            writer.WriteEndObject();
        }

        private static Dictionary<string, Setter<DictionaryColumn<TKey, TValue>>> setters = new Dictionary<string, Setter<DictionaryColumn<TKey, TValue>>>()
        {
            [Names.Keys] = (r, me) => me._keys.Read(r),
            [Names.Values] = (r, me) => me._values.Read(r),
            [Names.Pairs] = (r, me) => me._pairs.Read(r)
        };

        public void Read(ITreeReader reader)
        {
            reader.ReadObject(this, setters);
        }

        internal class DefaultComparer<T> : IComparer<T> where T : IComparable<T>
        {
            public int Compare(T x, T y)
            {
                return x.CompareTo(y);
            }
        }
    }

    /// <summary>
    ///  RefDictionaryColumn is used when either the Keys or Values of a DictionaryColumn
    ///  are typed references to another table. RefDictionaryColumn must be used in these
    ///  cases so that the Garbage Collector can correctly traverse dictionary entries to
    ///  find referenced rows and can see the inner RefColumn indices to update them when
    ///  rows have been swapped or moved.
    /// </summary>
    /// <typeparam name="TKey">Type of Dictionary Keys</typeparam>
    /// <typeparam name="TValue">Type of Dictionary Values</typeparam>
    public class RefDictionaryColumn<TKey, TValue> : DictionaryColumn<TKey, TValue>, IRefColumn where TKey : IComparable<TKey>
    {
        private readonly IRefColumn _refColumn;
        public string ReferencedTableName => _refColumn.ReferencedTableName;

        public RefDictionaryColumn(IColumn<TKey> keys, IColumn<TValue> values, IRefColumn refColumn, Nullability nullability = Nullability.DefaultToNull) : base(keys, values, nullability)
        {
            _refColumn = refColumn;
        }

        public long Traverse(int index, IGraphTraverser referencedTableCollector)
        {
            NumberList<int> pairs = _pairs[index];
            if (pairs == null) { return 0; }

            // Get the inner row indices used in the Keys and Values columns to store the values for the row[index] Dictionary
            long sum = 0;
            foreach (int innerRowIndex in pairs)
            {
                // Traverse references in the ref column for each inner row in the outer Dictionary
                sum += _refColumn.Traverse(innerRowIndex, referencedTableCollector);
            }

            return sum;
        }

        public void ForEach(Action<ArraySlice<int>> action)
        {
            _refColumn.ForEach(action);
        }
    }

    // To make a Dictionary with typed keys or values, make a WrappingColumn like this
    // to convert integers to typed values, then override BuildColumn to create the
    // DictionaryColumn using it.
    // 
    // NOTE: Cache the database, not the table, to correctly handle table successors
    // Garbage Collection creates.
    
    //internal class ArtifactLocationColumn : WrappingColumn<ArtifactLocation, int>
    //{
    //    private readonly SarifLogDatabase _database;

    //    public ArtifactLocationColumn(SarifLogDatabase database, RefColumn inner) : base(inner)
    //    {
    //        _database = database;
    //    }

    //    public override ArtifactLocation this[int index]
    //    {
    //        get => new ArtifactLocation(_database.ArtifactLocation, Inner[index]);
    //        set => Inner[index] = _database.ArtifactLocation.LocalIndex(value);
    //    }
    //}
}
