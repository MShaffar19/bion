// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;

using BSOA.Column;

using Xunit;

namespace BSOA.Test
{
    public class ListColumnTests
    {
        // Note: ListColumn<int> is easiest to test, but don't use ListColumn for numeric types.
        //  NumberListColumn is more efficient for numeric types. Use ListColumn for Lists of more complex types.

        [Fact]
        public void ListColumn_Basics()
        {
            ListColumn<int> column = new ListColumn<int>(new NumberColumn<int>(-1), nullByDefault: false);
            column[0].Add(1);
            column[0].Add(2);
            column[0].Add(3);

            // Test the outer column (non-nullable)
            Column.Basics(() => new ListColumn<int>(new NumberColumn<int>(-1), nullByDefault: false), ColumnList<int>.Empty, column[0], (index) =>
            {
                IList<int> other = column[column.Count];
                other.Add(index);
                other.Add(index + 1);
                other.Add(index + 2);
                return other;
            });

            // Test the outer column (nullable)
            Column.Basics(() => new ListColumn<int>(new NumberColumn<int>(-1), nullByDefault: true), null, column[0], (index) =>
            {
                IList<int> other = column[column.Count];
                other.Add(index);
                other.Add(index + 1);
                other.Add(index + 2);
                return other;
            });

            // Test the ColumnList item members
            CollectionChangeVerifier.VerifyList(column[1], (index) => index % 20);

            // ColumnList.Empty handling
            ColumnList<int> empty = ColumnList<int>.Empty;
            Assert.Empty(empty);
            Assert.True(empty.Count == 0);
            Assert.True(empty.Contains(7) == false);
            Assert.Equal(-1, empty.IndexOf(3));

            Assert.True(empty == ColumnList<int>.Empty);
            Assert.False(empty != ColumnList<int>.Empty);

            // ColumnList.GetHashCode and Equals w/nulls
            ListColumn<string> stringColumn = new ListColumn<string>(new StringColumn(), nullByDefault: false);
            
            ColumnList<string> first = (ColumnList<string>)stringColumn[0];
            first.Add("One");
            first.Add(null);
            first.Add("Two");

            ColumnList<string> second = (ColumnList<string>)stringColumn[1];
            second.Add("One");
            second.Add(null);
            second.Add("Two");

            Assert.True(second == first);

            second[1] = "NotNull";
            Assert.NotEqual(second.GetHashCode(), first.GetHashCode());
            Assert.False(second == first);

            // Set to null, add to cached copy
            stringColumn[1] = null;
            Assert.True(second.Count == 0);
            second.Add("One");
            Assert.Single(second);
        }

        [Fact]
        public void ListColumn_CacheThreadSafety()
        {
            ListColumn<int> column = new ListColumn<int>(new NumberColumn<int>(0));
            List<int> empty = new List<int>();

            for (int i = 0; i < 100; ++i)
            {
                // Set to non-null
                column[i] = empty;

                // Retrieve list and set up
                IList<int> current = column[i];
                current.Add(i - 1);
                current.Add(i);
                current.Add(i + 1);
            }

            // Verify list across multiple threads to confirm read-only use works safely
            Parallel.For(0, column.Count * 100, (i) =>
            {
                int rowIndex = i % 100;
                IList<int> current = column[rowIndex];
                Assert.Equal(rowIndex - 1, current[0]);
            });
        }
    }
}
