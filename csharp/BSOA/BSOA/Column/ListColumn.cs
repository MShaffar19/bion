﻿using BSOA.IO;
using BSOA.Model;
using System.Collections.Generic;

namespace BSOA.Column
{
    /// <summary>
    ///  RefListColumn provides a reference from an item in one table to a set
    ///  of items in another table. It stores the integer indices of the references.
    /// </summary>
    public class ListColumn<T> : LimitedList<ColumnList<T>>, IColumn<ColumnList<T>>
    {
        internal NumberListColumn<int> _indices;
        internal IColumn<T> _values;

        public override int Count => _indices.Count;

        public ListColumn(IColumn<T> values)
        {
            _indices = new NumberListColumn<int>();
            _values = values;
        }

        public override ColumnList<T> this[int index]
        {
            get => new ColumnList<T>(this, index);

            set
            {
                ColumnList<T> current = this[index];
                current.Clear();

                for (int i = 0; i < value.Count; ++i)
                {
                    current.Add(value[i]);
                }
            }
        }

        public override void Swap(int index1, int index2)
        {
            _indices.Swap(index1, index2);
        }

        public override void RemoveFromEnd(int count)
        {
            _indices.RemoveFromEnd(count);
        }

        public override void Clear()
        {
            _indices.Clear();
            _values.Clear();
        }

        public void Trim()
        {
            // Trim indices first to consolidate references before garbage collection
            _indices.Trim();

            // Find any unused values and remove them
            GarbageCollector.Collect<int, T>(_indices, _values);

            // Trim values afterward to clean up any newly unused space
            _values.Trim();
        }

        public void Write(ITreeWriter writer)
        {
            writer.WriteStartObject();
            writer.Write(Names.Indices, _indices);
            writer.Write(Names.Values, _values);
            writer.WriteEndObject();
        }

        private static Dictionary<string, Setter<ListColumn<T>>> setters = new Dictionary<string, Setter<ListColumn<T>>>()
        {
            [Names.Indices] = (r, me) => me._indices.Read(r),
            [Names.Values] = (r, me) => me._values.Read(r)
        };

        public void Read(ITreeReader reader)
        {
            reader.ReadObject(this, setters);
        }
    }
}