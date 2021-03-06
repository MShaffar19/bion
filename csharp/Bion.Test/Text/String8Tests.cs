// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Bion.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bion.Test.Text
{
    [TestClass]
    public class String8Tests
    {
        [TestMethod]
        public void String8_Basics()
        {
            byte[] b1 = null, b2 = null, b3 = null;

            String8 one = String8.Copy("one", ref b1);
            String8 two = String8.Copy("two", ref b2);
            String8 one2 = String8.Copy("one", ref b3);

            Assert.IsFalse(one.Equals(two));
            Assert.AreNotEqual(one.GetHashCode(), two.GetHashCode());

            Assert.IsTrue(one.Equals(one2));
            Assert.AreEqual(one.GetHashCode(), one2.GetHashCode());

            Assert.IsTrue(one.CompareTo(two) < 0);
            Assert.IsTrue(two.CompareTo(one) > 0);
            Assert.AreEqual(0, one.CompareTo(one2));
            Assert.AreEqual(0, one.CompareTo(one));
        }
    }
}
