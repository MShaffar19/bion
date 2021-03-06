// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using BSOA.GC;
using BSOA.Model;

namespace BSOA.Column
{
    /// <summary>
    ///  RefColumn provides a reference from an item in one table to an item
    ///  in another table. It stores the integer index of the reference.
    /// </summary>
    public class RefColumn : NumberColumn<int>, IRefColumn
    {
        public string ReferencedTableName { get; }

        public RefColumn(string referencedTableName) : base(-1)
        {
            ReferencedTableName = referencedTableName;
        }

        public long Traverse(int index, IGraphTraverser referencedTableCollector)
        {
            int targetIndex = this[index];
            if (targetIndex >= 0)
            {
                return referencedTableCollector.Traverse(targetIndex);
            }

            return 0;
        }
    }
}
