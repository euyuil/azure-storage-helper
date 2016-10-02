using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.WindowsAzure.Storage.Table;

namespace Euyuil.Azure.Storage.Helper.Table
{
    public class PartitionInfo<TObject>
    {
        public PartitionInfo(EntityKeyInfo<TObject> partitionKey, params RowInfo<TObject>[] rows)
        {
            PartitionKey = partitionKey;
            Rows = new List<RowInfo<TObject>>(rows);
        }

        public PartitionInfo(
            string partitionKeyPrefix,
            Expression<Func<TObject, object>> partitionKeySegmentsExpression,
            IReadOnlyDictionary<Type, IEntityKeySegmentResolver> partitionKeySegmentResolvers = null)
        {
            PartitionKey = new EntityKeyInfo<TObject>(partitionKeyPrefix, partitionKeySegmentsExpression, partitionKeySegmentResolvers);
            Rows = new List<RowInfo<TObject>>();
        }

        public RowInfo<TObject> Row(
            string rowKeyPrefix,
            Expression<Func<TObject, object>> rowKeySegmentsExpression,
            Expression<Func<TObject, object>> propertiesExpression,
            IReadOnlyDictionary<Type, IEntityKeySegmentResolver> rowKeySegmentResolvers = null,
            IReadOnlyDictionary<Type, IEntityPropertyResolver> propertyResolvers = null)
        {
            var rowKey = new EntityKeyInfo<TObject>(rowKeyPrefix, rowKeySegmentsExpression, rowKeySegmentResolvers);
            var properties = new EntityPropertiesInfo<TObject>(propertiesExpression, propertyResolvers);
            var entityInfo = new RowInfo<TObject>(PartitionKey, rowKey, properties);
            Rows.Add(entityInfo);
            return entityInfo;
        }

        public EntityKeyInfo<TObject> PartitionKey { get; set; }

        public IList<RowInfo<TObject>> Rows { get; set; }
    }
}
