using System;
using System.Linq;
using Euyuil.Azure.Storage.Helper.Table;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Euyuil.Azure.Storage.Helper.Tests.Table
{
    [TestClass]
    public class EntityTests
    {
        [TestMethod]
        public void ConvertObjectToEntitiesThenConvertBackTest()
        {
            var partition = new PartitionInfo<TestModel>("UR", obj => obj.Id);
            partition.HasEntityInfo("NM", obj => obj.Version, e => new { e.FirstName, e.LastName });
            partition.HasEntityInfo("DS", obj => obj.Version, e => e.Description);

            var model = new TestModel
            {
                Id = Guid.NewGuid(),
                Version = DateTime.UtcNow,
                FirstName = "Yue",
                LastName = "Liu",
                Description = "This is my model."
            };

            var entities = partition.ConvertObjectToEntities(model).ToArray();

            Assert.AreEqual(entities.Length, 2);

            var emptyModel = new TestModel();

            partition.FillObjectWithEntity(emptyModel, entities[0]);
            partition.FillObjectWithEntity(emptyModel, entities[1]);

            Assert.AreEqual(emptyModel.Id, model.Id);
            Assert.AreEqual(emptyModel.Version, model.Version);
            Assert.AreEqual(emptyModel.FirstName, model.FirstName);
            Assert.AreEqual(emptyModel.LastName, model.LastName);
            Assert.AreEqual(emptyModel.Description, model.Description);
        }
    }
}
