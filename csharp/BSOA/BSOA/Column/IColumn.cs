﻿using BSOA.IO;
using System.Collections.Generic;

namespace BSOA.Column
{
    public interface IColumn : ITreeSerializable
    {
        // Return if unused (Count == 0)
        bool Empty { get; }

        // Remove excess capacity and prepare to serialize
        void Trim();

        // Swap two values within column
        void Swap(int index1, int index2);

        //// Remove last items from column
        //void RemoveFromEnd(int length);
    }

    public interface IColumn<T> : IReadOnlyList<T>, IColumn
    {
        new T this[int index] { get; set; }

        // IReadOnlyList
        // -------------
        // int Count { get; }
        // T this[int index] { get; }
        // IEnumerator<T> GetEnumerator();

        // IBinarySerializable
        // -------------------
        // void Read(BinaryReader reader, ref byte[] buffer);
        // void Write(BinaryWriter writer, ref byte[] buffer);

        // ITreeSerializable
        // -----------------
        // void Write(ITreeWriter writer);
        // void Read(ITreeReader reader);
    }
}
