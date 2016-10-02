using System;
using System.Collections.Generic;
using System.Linq.Expressions;

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

        public EntityKeyInfo<TObject> PartitionKey { get; set; }

        /// <summary>
        /// Gets or sets the types of the rows in the partition.
        /// If there are more than 1 type of row, every row should have a unique prefix for row key.
        /// </summary>
        public IList<RowInfo<TObject>> Rows { get; set; }

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
    }
}
