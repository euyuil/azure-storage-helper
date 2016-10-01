using System;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Table;

namespace Euyuil.Azure.Storage.Helper.Table
{
    public interface IEntityPropertyResolver
    {
        Func<object, EntityProperty> PropertyToEntityPropertyConverter { get;}

        Func<EntityProperty, object> EntityPropertyToPropertyConverter { get; }
    }

    public interface IEntityPropertyResolver<TProperty> : IEntityPropertyResolver
    {
        new Func<TProperty, EntityProperty> PropertyToEntityPropertyConverter { get; }

        new Func<EntityProperty, TProperty> EntityPropertyToPropertyConverter { get; }
    }

    public class EntityPropertyResolver<TProperty> : IEntityPropertyResolver<TProperty>
    {
        public EntityPropertyResolver()
        {
        }

        public EntityPropertyResolver(Func<TProperty, EntityProperty> propertyToEntityPropertyConverter, Func<EntityProperty, TProperty> entityPropertyToPropertyConverter)
        {
            PropertyToEntityPropertyConverter = propertyToEntityPropertyConverter;
            EntityPropertyToPropertyConverter = entityPropertyToPropertyConverter;
        }

        Func<object, EntityProperty> IEntityPropertyResolver.PropertyToEntityPropertyConverter => PropertyToEntityPropertyConverter == null ? (Func<object, EntityProperty>)null : PropertyToEntityProperty;

        Func<EntityProperty, object> IEntityPropertyResolver.EntityPropertyToPropertyConverter => EntityPropertyToPropertyConverter == null ? (Func<EntityProperty, object>)null : EntityPropertyToProperty;

        public Func<TProperty, EntityProperty> PropertyToEntityPropertyConverter { get; set; }

        public Func<EntityProperty, TProperty> EntityPropertyToPropertyConverter { get; set; }

        private EntityProperty PropertyToEntityProperty(object property)
        {
            return PropertyToEntityPropertyConverter.Invoke((TProperty)property);
        }

        private object EntityPropertyToProperty(EntityProperty entityProperty)
        {
            return EntityPropertyToPropertyConverter.Invoke(entityProperty);
        }
    }

    public static class EntityPropertyResolvers
    {
        private static readonly Dictionary<Type, IEntityPropertyResolver> DefaultInternal;

        static EntityPropertyResolvers()
        {
            DefaultInternal = new Dictionary<Type, IEntityPropertyResolver>
            {
                { typeof(string), new EntityPropertyResolver<string>(EntityProperty.GeneratePropertyForString, entityProperty => entityProperty.StringValue) },
                { typeof(Guid?), new EntityPropertyResolver<Guid?>(EntityProperty.GeneratePropertyForGuid, entityProperty => entityProperty.GuidValue) },
            };
        }

        public static IReadOnlyDictionary<Type, IEntityPropertyResolver> Default => DefaultInternal;
    }
}
