namespace Euyuil.Azure.Storage.Helper.Table
{
    public class RowInfo<TObject>
    {
        public RowInfo(EntityKeyInfo<TObject> partitionKey, EntityKeyInfo<TObject> rowKey, EntityPropertiesInfo<TObject> properties)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
            Properties = properties;
        }

        public EntityKeyInfo<TObject> PartitionKey { get; set; }

        public EntityKeyInfo<TObject> RowKey { get; set; }

        public EntityPropertiesInfo<TObject> Properties { get; set; }
    }
}
