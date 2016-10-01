using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.WindowsAzure.Storage.Table;

namespace Euyuil.Azure.Storage.Helper.Table
{
    public class PartitionInfo<TObject>
    {
        public PartitionInfo()
        {
            Entities = new List<EntityInfo<TObject>>();
        }

        public PartitionInfo(EntityCompoundKeyInfo<TObject> partitionKey)
        {
            PartitionKey = partitionKey;
            Entities = new List<EntityInfo<TObject>>();
        }

        public PartitionInfo(EntityCompoundKeyInfo<TObject> partitionKey, params EntityInfo<TObject>[] entities)
        {
            PartitionKey = partitionKey;
            Entities = new List<EntityInfo<TObject>>(entities);
        }

        public PartitionInfo(string partitionKeyPrefix, Expression<Func<TObject, object>> partitionKeyExpression, IReadOnlyDictionary<Type, IEntityKeyResolver> partitionKeyResolvers = null)
        {
            PartitionKey = new EntityCompoundKeyInfo<TObject>(partitionKeyPrefix, partitionKeyExpression, partitionKeyResolvers);
            Entities = new List<EntityInfo<TObject>>();
        }

        public PartitionInfo<TObject> HasEntityInfo(EntityInfo<TObject> entityInfo)
        {
            Entities.Add(entityInfo);
            return this;
        }

        public PartitionInfo<TObject> HasEntityInfo(
            string rowKeyPrefix,
            Expression<Func<TObject, object>> rowKeyExpression,
            Expression<Func<TObject, object>> propertiesExpression,
            IReadOnlyDictionary<Type, IEntityKeyResolver> rowKeyResolvers = null,
            IReadOnlyDictionary<Type, IEntityPropertyResolver> propertyResolvers = null)
        {
            var rowKey = new EntityCompoundKeyInfo<TObject>(rowKeyPrefix, rowKeyExpression, rowKeyResolvers);
            var properties = new EntityPropertiesInfo<TObject>(propertiesExpression, propertyResolvers);
            Entities.Add(new EntityInfo<TObject>(PartitionKey, rowKey, properties));
            return this;
        }

        public EntityCompoundKeyInfo<TObject> PartitionKey { get; set; }

        public IList<EntityInfo<TObject>> Entities { get; set; }

        public IEnumerable<DynamicTableEntity> ConvertObjectToEntities(TObject obj)
        {
            var partitionKey = PartitionKey.CompoundKeyGetter.Invoke(obj);

            foreach (var entityInfo in Entities)
            {
                var rowKey = entityInfo.RowKey.CompoundKeyGetter.Invoke(obj);
                var entityProperties = entityInfo.Properties.PropertiesGetter.Invoke(obj).ToDictionary(e => e.Key, e => e.Value);
                yield return new DynamicTableEntity(partitionKey, rowKey, null, entityProperties);
            }
        }

        public void FillObjectWithEntity(TObject obj, DynamicTableEntity entity)
        {
            if (!KeyInfoMatchesKey(PartitionKey, entity.PartitionKey))
                throw new FormatException($"The format of partition key {entity.PartitionKey} of the entity is not expected");

            EntityInfo<TObject> entityInfo;
            try
            {
                entityInfo = Entities.Single(e => KeyInfoMatchesKey(e.RowKey, entity.RowKey));
            }
            catch (InvalidOperationException)
            {
                throw new FormatException($"The format of row key {entity.RowKey} of the entity is not expected");
            }

            PartitionKey.CompoundKeySetter.Invoke(obj, entity.PartitionKey);
            entityInfo.RowKey.CompoundKeySetter.Invoke(obj, entity.RowKey);
            entityInfo.Properties.PropertiesSetter.Invoke(obj, entity.Properties);
        }

        private static bool KeyInfoMatchesKey(EntityCompoundKeyInfo<TObject> keyInfo, string key)
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
