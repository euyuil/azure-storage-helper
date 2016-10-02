using System;
using Euyuil.Azure.Storage.Helper.Table;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Euyuil.Azure.Storage.Helper.Tests.Table
{
    [TestClass]
    public class EntityKeySegmentResolverTests
    {
        private IEntityPropertyResolver _dateTimeResolver;
        private IEntityPropertyResolver _dateTimeOffsetResolver;

        [TestInitialize]
        public void TestInitialize()
        {
            _dateTimeResolver = EntityPropertyResolvers.Default[typeof(DateTime)];
            _dateTimeOffsetResolver = EntityPropertyResolvers.Default[typeof(DateTimeOffset)];
        }

        [TestMethod]
        public void ConvertDateTimeToKeySegmentUtcLocalTest()
        {
            var dateTimeNow = DateTime.Now;
            var dateTimeUtcNow = dateTimeNow.ToUniversalTime();

            var dateTimeNowKeySeg = EntityKeySegmentResolvers.ConvertDateTimeToKeySegment(dateTimeNow);
            var dateTimeUtcNowKeySeg = EntityKeySegmentResolvers.ConvertDateTimeToKeySegment(dateTimeUtcNow);
            Assert.AreEqual(dateTimeNowKeySeg, dateTimeUtcNowKeySeg);
        }

        [TestMethod]
        public void ConvertDateTimeOffsetToKeySegmentUtcLocalTest()
        {
            var dateTimeOffsetNow = DateTimeOffset.Now;
            var dateTimeOffsetUtcNow = dateTimeOffsetNow.ToUniversalTime();

            var dateTimeOffsetNowKeySeg = EntityKeySegmentResolvers.ConvertDateTimeOffsetToKeySegment(dateTimeOffsetNow);
            var dateTimeOffsetUtcNowKeySeg = EntityKeySegmentResolvers.ConvertDateTimeOffsetToKeySegment(dateTimeOffsetUtcNow);
            Assert.AreEqual(dateTimeOffsetNowKeySeg, dateTimeOffsetUtcNowKeySeg);
        }

        [TestMethod]
        public void ConvertMaxDateTimeToKeySegmentTest()
        {
            var maxDateTime = DateTimeOffset.MaxValue.UtcDateTime;
            var maxDateTimeKeySeg = EntityKeySegmentResolvers.ConvertDateTimeToKeySegment(maxDateTime);
            Assert.AreEqual(maxDateTimeKeySeg, "5435d78a0bc8c000");
        }

        [TestMethod]
        public void ConvertMinDateTimeToKeySegmentTest()
        {
            var minDateTime = DateTimeOffset.MinValue.UtcDateTime;
            var minDateTimeKeySeg = EntityKeySegmentResolvers.ConvertDateTimeToKeySegment(minDateTime);
            Assert.AreEqual(minDateTimeKeySeg, "7fffffffffffffff");
        }

        [TestMethod]
        public void ConvertMaxDateTimeOffsetToKeySegmentTest()
        {
            var maxDateTimeOffset = DateTimeOffset.MaxValue;
            var maxDateTimeOffsetKeySeg = EntityKeySegmentResolvers.ConvertDateTimeOffsetToKeySegment(maxDateTimeOffset);
            Assert.AreEqual(maxDateTimeOffsetKeySeg, "5435d78a0bc8c000");
        }

        [TestMethod]
        public void ConvertMinDateTimeOffsetToKeySegmentTest()
        {
            var minDateTimeOffset = DateTimeOffset.MinValue;
            var minDateTimeOffsetKeySeg = EntityKeySegmentResolvers.ConvertDateTimeOffsetToKeySegment(minDateTimeOffset);
            Assert.AreEqual(minDateTimeOffsetKeySeg, "7fffffffffffffff");
        }
    }
}
