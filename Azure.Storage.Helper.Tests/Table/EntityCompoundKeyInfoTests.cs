using System;
using Euyuil.Azure.Storage.Helper.Table;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Euyuil.Azure.Storage.Helper.Tests.Table
{
    [TestClass]
    public class EntityCompoundKeyInfoTests
    {
        private TestModel _testModel;

        private TestModel _invalidTestModel;

        [TestInitialize]
        public void TestInitialize()
        {
            _testModel = new TestModel
            {
                FirstName = "Yue",
                LastName = "Liu",
                Description = "The man who created this project."
            };

            _invalidTestModel = new TestModel
            {
                FirstName = "Y_u_e",
                LastName = "L_i_u",
                Description = "The_man_who_created_this_project."
            };
        }

        [TestMethod]
        public void ConstructNoPropertyNullTest()
        {
            // ReSharper disable once ObjectCreationAsStatement
            Expect.Exception<Exception>(() => new EntityCompoundKeyInfo<TestModel>(e => null));
        }

        [TestMethod]
        public void ConstructNoPropertyNoMemberTest()
        {
            // ReSharper disable once ObjectCreationAsStatement
            Expect.Exception<Exception>(() => new EntityCompoundKeyInfo<TestModel>(e => new { }));
        }

        [TestMethod]
        public void GetterNoPropertyEmptyStringTest()
        {
            var keyInfo = new EntityCompoundKeyInfo<TestModel>(e => string.Empty);

            var key = keyInfo.CompoundKeyGetter.Invoke(_testModel);
            Assert.AreEqual(key, string.Empty);
        }

        [TestMethod]
        public void SetterNoPropertyEmptyStringTest()
        {
            var keyInfo = new EntityCompoundKeyInfo<TestModel>(e => string.Empty);

            var testModel = new TestModel();

            keyInfo.CompoundKeySetter.Invoke(testModel, string.Empty);
            Assert.AreEqual(testModel.FirstName, null);
            Assert.AreEqual(testModel.LastName, null);
            Assert.AreEqual(testModel.Description, null);

            Expect.Exception<FormatException>(() => keyInfo.CompoundKeySetter.Invoke(testModel, null));
            Expect.Exception<FormatException>(() => keyInfo.CompoundKeySetter.Invoke(testModel, "C"));
        }

        [TestMethod]
        public void GetterConstantPropertyTest()
        {
            var keyInfo = new EntityCompoundKeyInfo<TestModel>(e => "C");

            var key = keyInfo.CompoundKeyGetter.Invoke(_testModel);
            Assert.AreEqual(key, "C");
        }

        [TestMethod]
        public void SetterConstantPropertyTest()
        {
            var keyInfo = new EntityCompoundKeyInfo<TestModel>(e => "C");

            var testModel = new TestModel();

            keyInfo.CompoundKeySetter.Invoke(testModel, "C");
            Assert.AreEqual(testModel.FirstName, null);
            Assert.AreEqual(testModel.LastName, null);
            Assert.AreEqual(testModel.Description, null);

            Expect.Exception<FormatException>(() => keyInfo.CompoundKeySetter.Invoke(testModel, null));
            Expect.Exception<FormatException>(() => keyInfo.CompoundKeySetter.Invoke(testModel, string.Empty));
        }

        [TestMethod]
        public void GetterSingleSimplePropertyTest()
        {
            var keyInfo = new EntityCompoundKeyInfo<TestModel>(e => e.LastName);

            var key = keyInfo.CompoundKeyGetter.Invoke(_testModel);
            Assert.AreEqual(key, "Liu");
        }

        [TestMethod]
        public void SetterSingleSimplePropertyTest()
        {
            var keyInfo = new EntityCompoundKeyInfo<TestModel>(e => e.LastName);

            var testModel = new TestModel();

            keyInfo.CompoundKeySetter.Invoke(testModel, "Liu");
            Assert.AreEqual(testModel.FirstName, null);
            Assert.AreEqual(testModel.LastName, "Liu");
            Assert.AreEqual(testModel.Description, null);

            keyInfo.CompoundKeySetter.Invoke(testModel, string.Empty);
            Assert.AreEqual(testModel.FirstName, null);
            Assert.AreEqual(testModel.LastName, string.Empty);
            Assert.AreEqual(testModel.Description, null);

            Expect.Exception<FormatException>(() => keyInfo.CompoundKeySetter.Invoke(testModel, null));
        }

        [TestMethod]
        public void GetterSingleComplexPropertyTest()
        {
            var keyInfo = new EntityCompoundKeyInfo<TestModel>(e => new { e.LastName });

            var key = keyInfo.CompoundKeyGetter.Invoke(_testModel);
            Assert.AreEqual(key, "Liu");
        }

        [TestMethod]
        public void SetterSingleComplexPropertyTest()
        {
            var keyInfo = new EntityCompoundKeyInfo<TestModel>(e => new { e.LastName });

            var testModel = new TestModel();

            keyInfo.CompoundKeySetter.Invoke(testModel, "Liu");
            Assert.AreEqual(testModel.FirstName, null);
            Assert.AreEqual(testModel.LastName, "Liu");
            Assert.AreEqual(testModel.Description, null);

            keyInfo.CompoundKeySetter.Invoke(testModel, string.Empty);
            Assert.AreEqual(testModel.FirstName, null);
            Assert.AreEqual(testModel.LastName, string.Empty);
            Assert.AreEqual(testModel.Description, null);

            Expect.Exception<FormatException>(() => keyInfo.CompoundKeySetter.Invoke(testModel, null));
            Expect.Exception<FormatException>(() => keyInfo.CompoundKeySetter.Invoke(testModel, "__"));
            Expect.Exception<FormatException>(() => keyInfo.CompoundKeySetter.Invoke(testModel, "a__"));
            Expect.Exception<FormatException>(() => keyInfo.CompoundKeySetter.Invoke(testModel, "__b"));
            Expect.Exception<FormatException>(() => keyInfo.CompoundKeySetter.Invoke(testModel, "a__b"));
        }

        [TestMethod]
        public void GetterMultiplePropertiesTest()
        {
            var keyInfo = new EntityCompoundKeyInfo<TestModel>(e => new { e.LastName, e.FirstName });

            var key = keyInfo.CompoundKeyGetter.Invoke(_testModel);
            Assert.AreEqual(key, "Liu__Yue");
        }

        [TestMethod]
        public void SetterMultiplePropertiesTest()
        {
            var keyInfo = new EntityCompoundKeyInfo<TestModel>(e => new { e.LastName, e.FirstName });

            var testModel = new TestModel();

            keyInfo.CompoundKeySetter.Invoke(testModel, "Liu__Yue");
            Assert.AreEqual(testModel.FirstName, "Yue");
            Assert.AreEqual(testModel.LastName, "Liu");
            Assert.AreEqual(testModel.Description, null);

            Expect.Exception<FormatException>(() => keyInfo.CompoundKeySetter.Invoke(testModel, null));
            Expect.Exception<FormatException>(() => keyInfo.CompoundKeySetter.Invoke(testModel, string.Empty));
            Expect.Exception<FormatException>(() => keyInfo.CompoundKeySetter.Invoke(testModel, ",="));
            Expect.Exception<FormatException>(() => keyInfo.CompoundKeySetter.Invoke(testModel, ",-"));
            Expect.Exception<FormatException>(() => keyInfo.CompoundKeySetter.Invoke(testModel, "Liu"));
            Expect.Exception<FormatException>(() => keyInfo.CompoundKeySetter.Invoke(testModel, ","));
            Expect.Exception<FormatException>(() => keyInfo.CompoundKeySetter.Invoke(testModel, ",L"));
            Expect.Exception<FormatException>(() => keyInfo.CompoundKeySetter.Invoke(testModel, ",Liu"));
        }

        [TestMethod]
        public void SetterMultiplePropertiesWithConstantsTest()
        {
            var keyInfo = new EntityCompoundKeyInfo<TestModel>(e => new { PrefixNameDoesntMatter = "LF", e.LastName, e.FirstName });

            var testModel = new TestModel();

            keyInfo.CompoundKeySetter.Invoke(testModel, "LF__Liu__Yue");
            Assert.AreEqual(testModel.FirstName, "Yue");
            Assert.AreEqual(testModel.LastName, "Liu");
            Assert.AreEqual(testModel.Description, null);

            Expect.Exception<FormatException>(() => keyInfo.CompoundKeySetter.Invoke(testModel, null));
            Expect.Exception<FormatException>(() => keyInfo.CompoundKeySetter.Invoke(testModel, string.Empty));
            Expect.Exception<FormatException>(() => keyInfo.CompoundKeySetter.Invoke(testModel, ",="));
            Expect.Exception<FormatException>(() => keyInfo.CompoundKeySetter.Invoke(testModel, ",-"));
            Expect.Exception<FormatException>(() => keyInfo.CompoundKeySetter.Invoke(testModel, "Liu"));
            Expect.Exception<FormatException>(() => keyInfo.CompoundKeySetter.Invoke(testModel, ","));
            Expect.Exception<FormatException>(() => keyInfo.CompoundKeySetter.Invoke(testModel, ",L"));
            Expect.Exception<FormatException>(() => keyInfo.CompoundKeySetter.Invoke(testModel, ",Liu"));
        }
    }
}
