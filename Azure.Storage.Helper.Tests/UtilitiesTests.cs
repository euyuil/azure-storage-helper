using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Euyuil.Azure.Storage.Helper.Tests
{
    [TestClass]
    public class UtilitiesTests
    {
        [TestMethod]
        public void GetExpressionsOnePropertyTest()
        {
            Type[] memberTypes;
            string[] memberNames;
            Func<TestModel, object>[] memberGetters;
            Action<TestModel, object>[] memberSetters;

            InternalUtilities.ParseLambdaExpression(
                e => e.Id, out memberTypes, out memberNames, out memberGetters, out memberSetters);

            Assert.AreEqual(memberTypes.Length, 1);
            Assert.AreEqual(memberTypes[0], typeof(Guid));
        }

        [TestMethod]
        public void GetExpressionsTwoPropertiesTest()
        {
            Type[] memberTypes;
            string[] memberNames;
            Func<TestModel, object>[] memberGetters;
            Action<TestModel, object>[] memberSetters;

            InternalUtilities.ParseLambdaExpression(
                e => new { e.Id, e.Version }, out memberTypes, out memberNames, out memberGetters, out memberSetters);

            Assert.AreEqual(memberTypes.Length, 2);
            Assert.AreEqual(memberTypes[0], typeof(Guid));
            Assert.AreEqual(memberTypes[1], typeof(DateTime));
        }

        [TestMethod]
        public void GetExpressionsTwoPropertiesOneConstantTest()
        {
            Type[] memberTypes;
            string[] memberNames;
            Func<TestModel, object>[] memberGetters;
            Action<TestModel, object>[] memberSetters;

            InternalUtilities.ParseLambdaExpression(
                e => new { e.Id, e.Version, Dummy = "Constant" }, out memberTypes, out memberNames, out memberGetters, out memberSetters);

            Assert.AreEqual(memberTypes.Length, 3);
            Assert.AreEqual(memberTypes[0], typeof(Guid));
            Assert.AreEqual(memberTypes[1], typeof(DateTime));
            Assert.AreEqual(memberTypes[2], typeof(string));
        }
    }
}
