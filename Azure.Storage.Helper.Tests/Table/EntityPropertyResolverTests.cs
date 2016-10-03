using Euyuil.Azure.Storage.Helper.Table;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage.Table;

namespace Euyuil.Azure.Storage.Helper.Tests.Table
{
    [TestClass]
    public class EntityPropertyResolverTests
    {
        [TestMethod]
        public void EnumTypeResolverTest()
        {
            var propertyResolver = EntityPropertyResolvers.GetEntityPropertyResolver(null, typeof(TestEnum));

            var barEnumValue = TestEnum.Bar;
            var barEntityProperty = propertyResolver.MemberToEntityPropertyConverter.Invoke(barEnumValue);
            Assert.AreEqual(barEntityProperty.StringValue, barEnumValue.ToString("G"));

            var bazEnumValue = TestEnum.Baz;
            var bazEntityProperty = EntityProperty.GeneratePropertyForString(bazEnumValue.ToString("G"));
            var bazEnumValueExpected = (TestEnum)propertyResolver.EntityPropertyToMemberConverter.Invoke(bazEntityProperty);
            Assert.AreEqual(bazEnumValue, bazEnumValueExpected);
        }

        [TestMethod]
        public void EnumFlagTypeResolverTest()
        {
            var propertyResolver = EntityPropertyResolvers.GetEntityPropertyResolver(null, typeof(TestEnumFlag));

            var fooBazFlags = TestEnumFlag.Baz | TestEnumFlag.Foo;
            var fooBazEntityProperty = propertyResolver.MemberToEntityPropertyConverter.Invoke(fooBazFlags);
            Assert.AreEqual("Foo, Baz", fooBazFlags.ToString("F"));
            Assert.AreEqual(fooBazEntityProperty.StringValue, fooBazFlags.ToString("F"));

            var fooBarFlags = TestEnumFlag.Bar | TestEnumFlag.Foo;
            var fooBarEntityProperty = EntityProperty.GeneratePropertyForString(fooBarFlags.ToString("F"));
            var fooBarFlagsExpected = (TestEnumFlag)propertyResolver.EntityPropertyToMemberConverter.Invoke(fooBarEntityProperty);
            Assert.AreEqual(fooBarFlags, fooBarFlagsExpected);

            fooBarEntityProperty = EntityProperty.GeneratePropertyForString("Foo, Bar");
            fooBarFlagsExpected = (TestEnumFlag)propertyResolver.EntityPropertyToMemberConverter.Invoke(fooBarEntityProperty);
            Assert.AreEqual(fooBarFlags, fooBarFlagsExpected);
        }

        [TestMethod]
        public void NullableEnumTypeResolverTest()
        {
            var propertyResolver = EntityPropertyResolvers.GetEntityPropertyResolver(null, typeof(TestEnum?));

            var barEnumValue = (TestEnum?)TestEnum.Bar;
            var barEntityProperty = propertyResolver.MemberToEntityPropertyConverter.Invoke(barEnumValue);
            Assert.AreEqual(barEntityProperty.StringValue, barEnumValue.Value.ToString("G"));

            var bazEnumValue = (TestEnum?)TestEnum.Baz;
            var bazEntityProperty = EntityProperty.GeneratePropertyForString(bazEnumValue.Value.ToString("G"));
            var bazEnumValueExpected = (TestEnum)propertyResolver.EntityPropertyToMemberConverter.Invoke(bazEntityProperty);
            Assert.AreEqual(bazEnumValue, bazEnumValueExpected);

            var nullEntityProperty = propertyResolver.MemberToEntityPropertyConverter.Invoke(null);
            Assert.AreEqual(nullEntityProperty.StringValue, null);

            nullEntityProperty = EntityProperty.GeneratePropertyForString(null);
            var nullEnumValueExpected = (TestEnum?)propertyResolver.EntityPropertyToMemberConverter.Invoke(nullEntityProperty);
            Assert.AreEqual(null, nullEnumValueExpected);
        }

        [TestMethod]
        public void NullableEnumFlagTypeResolverTest()
        {
            var propertyResolver = EntityPropertyResolvers.GetEntityPropertyResolver(null, typeof(TestEnumFlag?));

            TestEnumFlag? fooBazFlags = TestEnumFlag.Baz | TestEnumFlag.Foo;
            var fooBazEntityProperty = propertyResolver.MemberToEntityPropertyConverter.Invoke(fooBazFlags);
            Assert.AreEqual("Foo, Baz", fooBazFlags.Value.ToString("F"));
            Assert.AreEqual(fooBazEntityProperty.StringValue, fooBazFlags.Value.ToString("F"));

            TestEnumFlag? fooBarFlags = TestEnumFlag.Bar | TestEnumFlag.Foo;
            var fooBarEntityProperty = EntityProperty.GeneratePropertyForString(fooBarFlags.Value.ToString("F"));
            var fooBarFlagsExpected = (TestEnumFlag?)propertyResolver.EntityPropertyToMemberConverter.Invoke(fooBarEntityProperty);
            Assert.AreEqual(fooBarFlags, fooBarFlagsExpected);

            fooBarEntityProperty = EntityProperty.GeneratePropertyForString("Foo, Bar");
            fooBarFlagsExpected = (TestEnumFlag?)propertyResolver.EntityPropertyToMemberConverter.Invoke(fooBarEntityProperty);
            Assert.AreEqual(fooBarFlags, fooBarFlagsExpected);

            var nullEntityProperty = propertyResolver.MemberToEntityPropertyConverter.Invoke(null);
            Assert.AreEqual(nullEntityProperty.StringValue, null);

            nullEntityProperty = EntityProperty.GeneratePropertyForString(null);
            var nullFlagsExpected = (TestEnumFlag?)propertyResolver.EntityPropertyToMemberConverter.Invoke(nullEntityProperty);
            Assert.AreEqual(null, nullFlagsExpected);
        }
    }
}
