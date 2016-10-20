using System;
using System.Collections.Generic;
using Euyuil.Azure.Storage.Helper.Table;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Euyuil.Azure.Storage.Helper.Tests.Table
{
    [TestClass]
    public class EntityKeyInfoTests
    {
        private TestModel _testModel;

        // ReSharper disable once NotAccessedField.Local
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
        public void NoSegmentTest()
        {
            var keyInfoArray = new[]
            {
                new EntityKeyInfo<TestModel>(), // Recommended way.
                new EntityKeyInfo<TestModel>(e => new { }),
            };

            // ReSharper disable once ObjectCreationAsStatement
            Expect.Exception<Exception>(() => new EntityKeyInfo<TestModel>(e => null)); // No resolver for Object type.

            foreach (var keyInfo in keyInfoArray)
            {
                var key = keyInfo.KeyGetter.Invoke(_testModel);
                Assert.AreEqual(key, string.Empty);

                var testModel = new TestModel();

                keyInfo.KeySetter.Invoke(testModel, string.Empty);
                Assert.AreEqual(testModel.FirstName, null);
                Assert.AreEqual(testModel.LastName, null);
                Assert.AreEqual(testModel.Description, null);

                Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, null));
                Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, "C"));
            }
        }

        [TestMethod]
        public void OneSegmentConstantStringTest()
        {
            var keyInfoArray = new[]
            {
                new EntityKeyInfo<TestModel>(e => ""),
                new EntityKeyInfo<TestModel>(e => string.Empty),
                new EntityKeyInfo<TestModel>(e => new { DoesntMatter = "" }),
                new EntityKeyInfo<TestModel>(e => new { string.Empty }),
            };

            foreach (var keyInfo in keyInfoArray)
            {
                var key = keyInfo.KeyGetter.Invoke(_testModel);
                Assert.AreEqual(key, string.Empty);

                var testModel = new TestModel();

                keyInfo.KeySetter.Invoke(testModel, string.Empty);
                Assert.AreEqual(testModel.FirstName, null);
                Assert.AreEqual(testModel.LastName, null);
                Assert.AreEqual(testModel.Description, null);

                Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, null));
                Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, "C"));
            }

            keyInfoArray = new[]
            {
                new EntityKeyInfo<TestModel>(e => "C"), // Recommended way.
                new EntityKeyInfo<TestModel>(e => new { DoesntMatter = "C" }),
            };

            foreach (var keyInfo in keyInfoArray)
            {
                var key = keyInfo.KeyGetter.Invoke(_testModel);
                Assert.AreEqual(key, "C");

                var testModel = new TestModel();

                keyInfo.KeySetter.Invoke(testModel, "C");
                Assert.AreEqual(testModel.FirstName, null);
                Assert.AreEqual(testModel.LastName, null);
                Assert.AreEqual(testModel.Description, null);

                Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, null));
                Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, string.Empty));
            }
        }

        [TestMethod]
        public void OneSegmentObjectPropertyTest()
        {
            var keyInfoArray = new[]
            {
                new EntityKeyInfo<TestModel>(e => e.LastName), // Recommended way.
                new EntityKeyInfo<TestModel>(e => new { e.LastName }),
            };

            foreach (var keyInfo in keyInfoArray)
            {
                var key = keyInfo.KeyGetter.Invoke(_testModel);
                Assert.AreEqual(key, "Liu");

                var testModel = new TestModel();

                keyInfo.KeySetter.Invoke(testModel, "Liu");
                Assert.AreEqual(testModel.FirstName, null);
                Assert.AreEqual(testModel.LastName, "Liu");
                Assert.AreEqual(testModel.Description, null);

                keyInfo.KeySetter.Invoke(testModel, string.Empty);
                Assert.AreEqual(testModel.FirstName, null);
                Assert.AreEqual(testModel.LastName, string.Empty);
                Assert.AreEqual(testModel.Description, null);

                Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, null));
                Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, "__"));
                Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, "a__"));
                Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, "__b"));
                Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, "a__b"));
            }
        }

        [TestMethod]
        public void TwoSegmentsObjectPropertiesTest()
        {
            var keyInfo = new EntityKeyInfo<TestModel>(e => new { e.LastName, e.FirstName });

            var key = keyInfo.KeyGetter.Invoke(_testModel);
            Assert.AreEqual(key, "Liu__Yue");

            var testModel = new TestModel();

            keyInfo.KeySetter.Invoke(testModel, "Liu__Yue");
            Assert.AreEqual(testModel.FirstName, "Yue");
            Assert.AreEqual(testModel.LastName, "Liu");
            Assert.AreEqual(testModel.Description, null);

            Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, null));
            Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, string.Empty));
            Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, ",="));
            Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, ",-"));
            Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, "Liu"));
            Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, ","));
            Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, ",L"));
            Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, ",Liu"));
        }

        [TestMethod]
        public void MultipleSegmentsConstantStringsObjectPropertiesTest()
        {
            var keyInfo = new EntityKeyInfo<TestModel>(e => new { PrefixNameDoesntMatter = "LF", e.LastName, e.FirstName });

            var key = keyInfo.KeyGetter.Invoke(_testModel);
            Assert.AreEqual(key, "LF__Liu__Yue");

            var testModel = new TestModel();

            keyInfo.KeySetter.Invoke(testModel, "LF__Liu__Yue");
            Assert.AreEqual(testModel.FirstName, "Yue");
            Assert.AreEqual(testModel.LastName, "Liu");
            Assert.AreEqual(testModel.Description, null);

            keyInfo.KeySetter.Invoke(testModel, "LF__Liu__");
            Assert.AreEqual(testModel.FirstName, string.Empty);
            Assert.AreEqual(testModel.LastName, "Liu");
            Assert.AreEqual(testModel.Description, null);

            keyInfo.KeySetter.Invoke(testModel, "LF____");
            Assert.AreEqual(testModel.FirstName, string.Empty);
            Assert.AreEqual(testModel.LastName, string.Empty);
            Assert.AreEqual(testModel.Description, null);

            Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, null));
            Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, string.Empty));
            Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, ",="));
            Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, ",-"));
            Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, "Liu"));
            Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, ","));
            Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, ",L"));
            Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, ",Liu"));
            Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, "Liu__Yue"));
            Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, "__Liu__Yue"));
            Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, "LFLiu__Yue"));
            Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, "LF__Liu"));
            Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, "LF__Liu_"));
            Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, "LF__"));
            Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, "LF___"));
        }

        [TestMethod]
        public void PrefixWithNoSegmentTest()
        {
            var keyInfoArray = new[]
            {
                new EntityKeyInfo<TestModel>("UR"), // Recommended way.
                new EntityKeyInfo<TestModel>("UR", e => new { }),
            };

            // ReSharper disable once ObjectCreationAsStatement
            Expect.Exception<Exception>(() => new EntityKeyInfo<TestModel>("UR", e => null)); // No resolver for Object type.

            foreach (var keyInfo in keyInfoArray)
            {
                var key = keyInfo.KeyGetter.Invoke(_testModel);
                Assert.AreEqual(key, "UR");

                var testModel = new TestModel();

                keyInfo.KeySetter.Invoke(testModel, "UR");
                Assert.AreEqual(testModel.FirstName, null);
                Assert.AreEqual(testModel.LastName, null);
                Assert.AreEqual(testModel.Description, null);

                Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, null));
                Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, string.Empty));
                Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, "C"));
                Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, "UR__"));
            }
        }

        [TestMethod]
        public void PrefixWithOneSegmentConstantStringTest()
        {
            var keyInfoArray = new[]
            {
                new EntityKeyInfo<TestModel>("UR", e => ""),
                new EntityKeyInfo<TestModel>("UR", e => string.Empty),
                new EntityKeyInfo<TestModel>("UR", e => new { DoesntMatter = "" }),
                new EntityKeyInfo<TestModel>("UR", e => new { string.Empty }),
            };

            foreach (var keyInfo in keyInfoArray)
            {
                var key = keyInfo.KeyGetter.Invoke(_testModel);
                Assert.AreEqual(key, "UR__");

                var testModel = new TestModel();

                keyInfo.KeySetter.Invoke(testModel, "UR__");
                Assert.AreEqual(testModel.FirstName, null);
                Assert.AreEqual(testModel.LastName, null);
                Assert.AreEqual(testModel.Description, null);

                Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, null));
                Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, string.Empty));
                Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, "C"));
                Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, "UR"));
            }

            keyInfoArray = new[]
            {
                new EntityKeyInfo<TestModel>("UR", e => "C"), // Recommended way.
                new EntityKeyInfo<TestModel>("UR", e => new { DoesntMatter = "C" }),
            };

            foreach (var keyInfo in keyInfoArray)
            {
                var key = keyInfo.KeyGetter.Invoke(_testModel);
                Assert.AreEqual(key, "UR__C");

                var testModel = new TestModel();

                keyInfo.KeySetter.Invoke(testModel, "UR__C");
                Assert.AreEqual(testModel.FirstName, null);
                Assert.AreEqual(testModel.LastName, null);
                Assert.AreEqual(testModel.Description, null);

                Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, null));
                Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, string.Empty));
                Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, "C"));
            }
        }

        [TestMethod]
        public void PrefixWithOneSegmentObjectPropertyTest()
        {
            var keyInfoArray = new[]
            {
                new EntityKeyInfo<TestModel>("UR", e => e.LastName), // Recommended way.
                new EntityKeyInfo<TestModel>("UR", e => new { e.LastName }),
            };

            foreach (var keyInfo in keyInfoArray)
            {
                var key = keyInfo.KeyGetter.Invoke(_testModel);
                Assert.AreEqual(key, "UR__Liu");

                var testModel = new TestModel();

                keyInfo.KeySetter.Invoke(testModel, "UR__Liu");
                Assert.AreEqual(testModel.FirstName, null);
                Assert.AreEqual(testModel.LastName, "Liu");
                Assert.AreEqual(testModel.Description, null);

                keyInfo.KeySetter.Invoke(testModel, "UR__");
                Assert.AreEqual(testModel.FirstName, null);
                Assert.AreEqual(testModel.LastName, string.Empty);
                Assert.AreEqual(testModel.Description, null);

                Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, null));
                Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, string.Empty));
                Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, "__"));
                Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, "a__"));
                Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, "__b"));
                Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, "a__b"));
                Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, "UR____"));
                Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, "UR__a__"));
                Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, "UR____b"));
                Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, "UR__a__b"));
            }
        }

        [TestMethod]
        public void PrefixWithTwoSegmentsObjectPropertiesTest()
        {
            var keyInfo = new EntityKeyInfo<TestModel>("UR", e => new { e.LastName, e.FirstName });

            var key = keyInfo.KeyGetter.Invoke(_testModel);
            Assert.AreEqual(key, "UR__Liu__Yue");

            var testModel = new TestModel();

            keyInfo.KeySetter.Invoke(testModel, "UR__Liu__Yue");
            Assert.AreEqual(testModel.FirstName, "Yue");
            Assert.AreEqual(testModel.LastName, "Liu");
            Assert.AreEqual(testModel.Description, null);

            keyInfo.KeySetter.Invoke(testModel, "UR____Yue");
            Assert.AreEqual(testModel.FirstName, "Yue");
            Assert.AreEqual(testModel.LastName, string.Empty);
            Assert.AreEqual(testModel.Description, null);

            keyInfo.KeySetter.Invoke(testModel, "UR__Liu__");
            Assert.AreEqual(testModel.FirstName, string.Empty);
            Assert.AreEqual(testModel.LastName, "Liu");
            Assert.AreEqual(testModel.Description, null);

            keyInfo.KeySetter.Invoke(testModel, "UR____");
            Assert.AreEqual(testModel.FirstName, string.Empty);
            Assert.AreEqual(testModel.LastName, string.Empty);
            Assert.AreEqual(testModel.Description, null);

            Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, null));
            Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, string.Empty));
            Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, "UR"));
            Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, "UR_"));
            Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, "UR__"));
            Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, "UR___"));
            Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, ",="));
            Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, ",-"));
            Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, "Liu"));
            Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, ","));
            Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, ",L"));
            Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, ",Liu"));
            Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, "UR__,="));
            Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, "UR__,-"));
            Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, "UR__Liu"));
            Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, "UR__,"));
            Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, "UR__,L"));
            Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, "UR__,Liu"));
        }

        [TestMethod]
        public void PrefixWithMultipleSegmentsConstantStringsObjectPropertiesTest()
        {
            var keyInfo = new EntityKeyInfo<TestModel>("UR", e => new { PrefixNameDoesntMatter = "LF", e.LastName, e.FirstName });

            var key = keyInfo.KeyGetter.Invoke(_testModel);
            Assert.AreEqual(key, "UR__LF__Liu__Yue");

            var testModel = new TestModel();

            keyInfo.KeySetter.Invoke(testModel, "UR__LF__Liu__Yue");
            Assert.AreEqual(testModel.FirstName, "Yue");
            Assert.AreEqual(testModel.LastName, "Liu");
            Assert.AreEqual(testModel.Description, null);

            keyInfo.KeySetter.Invoke(testModel, "UR__LF__Liu__");
            Assert.AreEqual(testModel.FirstName, string.Empty);
            Assert.AreEqual(testModel.LastName, "Liu");
            Assert.AreEqual(testModel.Description, null);

            keyInfo.KeySetter.Invoke(testModel, "UR__LF____");
            Assert.AreEqual(testModel.FirstName, string.Empty);
            Assert.AreEqual(testModel.LastName, string.Empty);
            Assert.AreEqual(testModel.Description, null);

            Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, null));
            Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, string.Empty));
            Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, ",="));
            Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, ",-"));
            Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, "Liu"));
            Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, ","));
            Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, ",L"));
            Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, ",Liu"));
            Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, "Liu__Yue"));
            Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, "__Liu__Yue"));
            Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, "LFLiu__Yue"));
            Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, "LF__Liu"));
            Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, "LF__Liu_"));
            Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, "LF__"));
            Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, "LF___"));
            Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, "UR__,="));
            Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, "UR__,-"));
            Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, "UR__Liu"));
            Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, "UR__,"));
            Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, "UR__,L"));
            Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, "UR__,Liu"));
            Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, "UR__Liu__Yue"));
            Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, "UR____Liu__Yue"));
            Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, "UR__LFLiu__Yue"));
            Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, "UR__LF__Liu"));
            Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, "UR__LF__Liu_"));
            Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, "UR__LF__"));
            Expect.Exception<FormatException>(() => keyInfo.KeySetter.Invoke(testModel, "UR__LF___"));
        }
    }
}
