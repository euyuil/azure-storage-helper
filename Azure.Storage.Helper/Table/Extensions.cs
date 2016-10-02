using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace Euyuil.Azure.Storage.Helper.Table
{
    public static class Extensions
    {
        public static DynamicTableEntity ConvertObjectToEntity<TObject>(this RowInfo<TObject> row, TObject obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            var partitionKey = row.PartitionKey.KeyGetter.Invoke(obj);
            var rowKey = row.RowKey.KeyGetter.Invoke(obj);
            var entityProperties = row.Properties.PropertiesGetter.Invoke(obj).ToDictionary(e => e.Key, e => e.Value);
            return new DynamicTableEntity(partitionKey, rowKey, null, entityProperties);
        }

        public static IEnumerable<DynamicTableEntity> ConvertObjectToEntities<TObject>(this PartitionInfo<TObject> partition, TObject obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            var partitionKey = partition.PartitionKey.KeyGetter.Invoke(obj);
            foreach (var entityInfo in partition.Rows)
            {
                var rowKey = entityInfo.RowKey.KeyGetter.Invoke(obj);
                var entityProperties = entityInfo.Properties.PropertiesGetter.Invoke(obj).ToDictionary(e => e.Key, e => e.Value);
                yield return new DynamicTableEntity(partitionKey, rowKey, null, entityProperties);
            }
        }

        public static bool FillObjectWithEntity<TObject>(this RowInfo<TObject> row,  TObject obj, DynamicTableEntity entity)
        {
            if (entity == null) return false;
            row.PartitionKey.KeySetter.Invoke(obj, entity.PartitionKey);
            row.RowKey.KeySetter.Invoke(obj, entity.RowKey);
            row.Properties.PropertiesSetter.Invoke(obj, entity.Properties);
            return true;
        }

        public static bool FillObjectWithEntity<TObject>(this RowInfo<TObject> row, TObject obj, IEnumerable<DynamicTableEntity> entities)
        {
            var entityCollection = entities as ICollection<DynamicTableEntity> ?? entities.ToArray();
            return entityCollection.Count > 0 && entityCollection.Aggregate(true, (b, entity) => b && row.FillObjectWithEntity(obj, entity));
        }

        public static bool FillObjectWithEntity<TObject>(this PartitionInfo<TObject> partition, TObject obj, DynamicTableEntity entity)
        {
            if (entity == null) return false;
            if (!DoesKeyInfoMatchKey(partition.PartitionKey, entity.PartitionKey)) return false;

            RowInfo<TObject> rowInfo;
            try
            {
                rowInfo = partition.Rows.Single(e => DoesKeyInfoMatchKey(e.RowKey, entity.RowKey));
            }
            catch (InvalidOperationException)
            {
                return false;
            }

            partition.PartitionKey.KeySetter.Invoke(obj, entity.PartitionKey);
            rowInfo.RowKey.KeySetter.Invoke(obj, entity.RowKey);
            rowInfo.Properties.PropertiesSetter.Invoke(obj, entity.Properties);

            return true;
        }

        public static bool FillObjectWithEntity<TObject>(this PartitionInfo<TObject> partition, TObject obj, IEnumerable<DynamicTableEntity> entities)
        {
            var entityCollection = entities as ICollection<DynamicTableEntity> ?? entities.ToArray();
            return entityCollection.Count > 0 && entityCollection.Aggregate(true, (b, entity) => b && partition.FillObjectWithEntity(obj, entity));
        }

        public static bool FillObjectWithEntity<TObject>(this IEnumerable<PartitionInfo<TObject>> partitions, TObject obj, DynamicTableEntity entity)
        {
            if (entity == null) return false;

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
            var entityCollection = entities as ICollection<DynamicTableEntity> ?? entities.ToArray();
            if (entityCollection.Count <= 0) return false;

            var partitionCollection = partitions as IReadOnlyCollection<PartitionInfo<TObject>> ?? partitions.ToArray();

            return entityCollection.Aggregate(true, (b, entity) => b && partitionCollection.FillObjectWithEntity(obj, entity));
        }

        public static async Task<bool> FillObjectWithExactMatchAsync<TObject>(this RowInfo<TObject> row, TObject obj, CloudTable table)
        {
            var partitionKey = row.PartitionKey.KeyGetter.Invoke(obj);
            var rowKey = row.RowKey.KeyGetter.Invoke(obj);

            var tableResult = await table.ExecuteAsync(TableOperation.Retrieve(partitionKey, rowKey));

            return tableResult.Result != null && row.FillObjectWithEntity(obj, (DynamicTableEntity)tableResult.Result);
        }

        public static async Task<bool> FillObjectWithFirstMatchAsync<TObject>(this RowInfo<TObject> row, TObject obj, CloudTable table, string filter = null)
        {
            if (filter == null) filter = row.GenerateEqPartitionGeRowFilter(obj);

            var tableQuerySegment = await table.ExecuteQuerySegmentedAsync(new TableQuery().Where(filter).Take(1), null);

            return tableQuerySegment.Results.Count > 0 && row.FillObjectWithEntity(obj, tableQuerySegment.Results);
        }

        public static string GenerateEqPartitionGeRowFilter<TObject>(this RowInfo<TObject> row, TObject obj)
        {
            var partitionKey = row.PartitionKey.KeyGetter.Invoke(obj);
            var rowKey = row.RowKey.KeyGetter.Invoke(obj);

            var partitionKeyFilter = TableQuery.GenerateFilterCondition(nameof(ITableEntity.PartitionKey), QueryComparisons.Equal, partitionKey);
            var rowKeyFilter = TableQuery.GenerateFilterCondition(nameof(ITableEntity.RowKey), QueryComparisons.GreaterThanOrEqual, rowKey);
            var filter = TableQuery.CombineFilters(partitionKeyFilter, TableOperators.And, rowKeyFilter);

            return filter;
        }

        public static string GenerateGePartitionEqRowFilter<TObject>(this RowInfo<TObject> row, TObject obj)
        {
            var partitionKey = row.PartitionKey.KeyGetter.Invoke(obj);
            var rowKey = row.RowKey.KeyGetter.Invoke(obj);

            var partitionKeyFilter = TableQuery.GenerateFilterCondition(nameof(ITableEntity.PartitionKey), QueryComparisons.GreaterThanOrEqual, partitionKey);
            var rowKeyFilter = TableQuery.GenerateFilterCondition(nameof(ITableEntity.RowKey), QueryComparisons.Equal, rowKey);
            var filter = TableQuery.CombineFilters(partitionKeyFilter, TableOperators.And, rowKeyFilter);

            return filter;
        }

        public static string GenerateEqRowFilter<TObject>(this RowInfo<TObject> row, TObject obj)
        {
            var rowKey = row.RowKey.KeyGetter.Invoke(obj);
            var filter = TableQuery.GenerateFilterCondition(nameof(ITableEntity.RowKey), QueryComparisons.Equal, rowKey);
            return filter;
        }

        private static bool DoesKeyInfoMatchKey<TObject>(EntityKeyInfo<TObject> keyInfo, string key)
        {
            if (keyInfo.KeyPrefix == null)
            {
                if (key.StartsWith(EntityKeyInfo.Separator)) return false;
            }
            else
            {
                if (!key.StartsWith($"{keyInfo.KeyPrefix}{EntityKeyInfo.Separator}")) return false;
            }

            return true;
        }
    }
}
