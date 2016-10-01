using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure.Storage.Table;

namespace Euyuil.Azure.Storage.Helper.Table
{
    public static class PartitionInfoExtensions
    {
        public static bool FillObjectWithEntity<TObject>(this PartitionInfo<TObject> partition, TObject obj, DynamicTableEntity entity)
        {
            if (!DoesKeyInfoMatchKey(partition.PartitionKey, entity.PartitionKey))
            {
                return false;
            }

            EntityInfo<TObject> entityInfo;
            try
            {
                entityInfo = partition.Entities.Single(e => DoesKeyInfoMatchKey(e.RowKey, entity.RowKey));
            }
            catch (InvalidOperationException)
            {
                return false;
            }

            partition.PartitionKey.CompoundKeySetter.Invoke(obj, entity.PartitionKey);
            entityInfo.RowKey.CompoundKeySetter.Invoke(obj, entity.RowKey);
            entityInfo.Properties.PropertiesSetter.Invoke(obj, entity.Properties);

            return true;
        }

        public static bool FillObjectWithEntity<TObject>(this PartitionInfo<TObject> partition, TObject obj, IEnumerable<DynamicTableEntity> entities)
        {
            return entities.Aggregate(true, (b, entity) => b && partition.FillObjectWithEntity(obj, entity));
        }

        public static bool FillObjectWithEntity<TObject>(this IEnumerable<PartitionInfo<TObject>> partitions, TObject obj, DynamicTableEntity entity)
        {
            PartitionInfo<TObject> partition;
            try
            {
                partition = partitions.Single(e => DoesKeyInfoMatchKey(e.PartitionKey, entity.PartitionKey));
            }
            catch (InvalidOperationException)
            {
                return false;
            }

            return partition.FillObjectWithEntity(obj, entity);
        }

        public static bool FillObjectWithEntity<TObject>(
            this IEnumerable<PartitionInfo<TObject>> partitions, TObject obj, IEnumerable<DynamicTableEntity> entities)
        {
            var partitionCollection = partitions as IReadOnlyCollection<PartitionInfo<TObject>> ?? partitions.ToArray();
            return entities.Aggregate(true, (b, entity) => b && partitionCollection.FillObjectWithEntity(obj, entity));
        }

        private static bool DoesKeyInfoMatchKey<TObject>(EntityCompoundKeyInfo<TObject> keyInfo, string key)
        {
            if (keyInfo.CompoundKeyPrefix == null)
            {
                if (key.StartsWith(EntityCompoundKeyInfo.Separator)) return false;
            }
            else
            {
                if (!key.StartsWith($"{keyInfo.CompoundKeyPrefix}{EntityCompoundKeyInfo.Separator}")) return false;
            }

            return true;
        }
    }
}
