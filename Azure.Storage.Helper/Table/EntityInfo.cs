using System.Linq;
using Microsoft.WindowsAzure.Storage.Table;

namespace Euyuil.Azure.Storage.Helper.Table
{
    public class EntityInfo<TObject>
    {
        public EntityInfo(EntityCompoundKeyInfo<TObject> partitionKey, EntityCompoundKeyInfo<TObject> rowKey, EntityPropertiesInfo<TObject> properties)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
            Properties = properties;
        }

        public EntityCompoundKeyInfo<TObject> PartitionKey { get; set; }

        public EntityCompoundKeyInfo<TObject> RowKey { get; set; }

        public EntityPropertiesInfo<TObject> Properties { get; set; }

        public DynamicTableEntity ConvertObjectToEntity(TObject obj)
        {
            var partitionKey = PartitionKey.CompoundKeyGetter.Invoke(obj);
            var rowKey = RowKey.CompoundKeyGetter.Invoke(obj);
            var entityProperties = Properties.PropertiesGetter.Invoke(obj).ToDictionary(e => e.Key, e => e.Value);
            return new DynamicTableEntity(partitionKey, rowKey, null, entityProperties);
        }

        public void FillObjectWithEntity(TObject obj, DynamicTableEntity entity)
        {
            PartitionKey.CompoundKeySetter.Invoke(obj, entity.PartitionKey);
            RowKey.CompoundKeySetter.Invoke(obj, entity.RowKey);
            Properties.PropertiesSetter.Invoke(obj, entity.Properties);
        }
    }
}
