using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.WindowsAzure.Storage.Table;

namespace Euyuil.Azure.Storage.Helper.Table
{
    public interface IEntityPropertyResolver
    {
        Func<object, EntityProperty> MemberToEntityPropertyConverter { get;}

        Func<EntityProperty, object> EntityPropertyToMemberConverter { get; }
    }

    public interface IEntityPropertyResolver<TMember> : IEntityPropertyResolver
    {
        new Func<TMember, EntityProperty> MemberToEntityPropertyConverter { get; }

        new Func<EntityProperty, TMember> EntityPropertyToMemberConverter { get; }
    }

    public class EntityPropertyResolver : IEntityPropertyResolver
    {
        public EntityPropertyResolver()
        {
        }

        public EntityPropertyResolver(
            Func<object, EntityProperty> memberToEntityPropertyConverter, Func<EntityProperty, object> entityPropertyToMemberConverter)
        {
            MemberToEntityPropertyConverter = memberToEntityPropertyConverter;
            EntityPropertyToMemberConverter = entityPropertyToMemberConverter;
        }

        public Func<object, EntityProperty> MemberToEntityPropertyConverter { get; }

        public Func<EntityProperty, object> EntityPropertyToMemberConverter { get; }
    }

    public class EntityPropertyResolver<TMember> : IEntityPropertyResolver<TMember>
    {
        public EntityPropertyResolver()
        {
        }

        public EntityPropertyResolver(
            Func<TMember, EntityProperty> memberToEntityPropertyConverter, Func<EntityProperty, TMember> entityPropertyToMemberConverter)
        {
            MemberToEntityPropertyConverter = memberToEntityPropertyConverter;
            EntityPropertyToMemberConverter = entityPropertyToMemberConverter;
        }

        Func<object, EntityProperty> IEntityPropertyResolver.MemberToEntityPropertyConverter =>
            MemberToEntityPropertyConverter == null ? (Func<object, EntityProperty>)null : ConvertMemberToEntityProperty;

        Func<EntityProperty, object> IEntityPropertyResolver.EntityPropertyToMemberConverter =>
            EntityPropertyToMemberConverter == null ? (Func<EntityProperty, object>)null : ConvertEntityPropertyToMember;

        public Func<TMember, EntityProperty> MemberToEntityPropertyConverter { get; set; }

        public Func<EntityProperty, TMember> EntityPropertyToMemberConverter { get; set; }

        private EntityProperty ConvertMemberToEntityProperty(object member)
        {
            return MemberToEntityPropertyConverter.Invoke((TMember)member);
        }

        private object ConvertEntityPropertyToMember(EntityProperty entityProperty)
        {
            return EntityPropertyToMemberConverter.Invoke(entityProperty);
        }
    }

    public static class EntityPropertyResolvers
    {
        private static readonly Dictionary<Type, IEntityPropertyResolver> DefaultInternal;

        static EntityPropertyResolvers()
        {
            // ReSharper disable PossibleInvalidOperationException
            DefaultInternal = new Dictionary<Type, IEntityPropertyResolver>
            {
                { typeof(bool), new EntityPropertyResolver<bool>(member => EntityProperty.GeneratePropertyForBool(member), entityProperty => entityProperty.BooleanValue.Value) },
                { typeof(bool?), new EntityPropertyResolver<bool?>(EntityProperty.GeneratePropertyForBool, entityProperty => entityProperty.BooleanValue) },
                { typeof(int), new EntityPropertyResolver<int>(member => EntityProperty.GeneratePropertyForInt(member), entityProperty => entityProperty.Int32Value.Value) },
                { typeof(int?), new EntityPropertyResolver<int?>(EntityProperty.GeneratePropertyForInt, entityProperty => entityProperty.Int32Value) },
                { typeof(long), new EntityPropertyResolver<long>(member => EntityProperty.GeneratePropertyForLong(member), entityProperty => entityProperty.Int64Value.Value) },
                { typeof(long?), new EntityPropertyResolver<long?>(EntityProperty.GeneratePropertyForLong, entityProperty => entityProperty.Int64Value) },
                { typeof(double), new EntityPropertyResolver<double>(member => EntityProperty.GeneratePropertyForDouble(member), entityProperty => entityProperty.DoubleValue.Value) },
                { typeof(double?), new EntityPropertyResolver<double?>(EntityProperty.GeneratePropertyForDouble, entityProperty => entityProperty.DoubleValue) },
                { typeof(decimal), new EntityPropertyResolver<decimal>(member => EntityProperty.GeneratePropertyForString(member.ToString(CultureInfo.InvariantCulture)), entityProperty => decimal.Parse(entityProperty.StringValue)) },
                { typeof(decimal?), new EntityPropertyResolver<decimal?>(member => EntityProperty.GeneratePropertyForString(member?.ToString(CultureInfo.InvariantCulture)), entityProperty => entityProperty.StringValue == null ? (decimal?)null : decimal.Parse(entityProperty.StringValue)) },
                { typeof(Guid), new EntityPropertyResolver<Guid>(member => EntityProperty.GeneratePropertyForGuid(member), entityProperty => entityProperty.GuidValue.Value) },
                { typeof(Guid?), new EntityPropertyResolver<Guid?>(EntityProperty.GeneratePropertyForGuid, entityProperty => entityProperty.GuidValue) },
                { typeof(DateTime), new EntityPropertyResolver<DateTime>(member => EntityProperty.GeneratePropertyForDateTimeOffset(member), entityProperty => entityProperty.DateTime.Value) },
                { typeof(DateTime?), new EntityPropertyResolver<DateTime?>(member => EntityProperty.GeneratePropertyForDateTimeOffset(member), entityProperty => entityProperty.DateTime) },
                { typeof(DateTimeOffset), new EntityPropertyResolver<DateTimeOffset>(member => EntityProperty.GeneratePropertyForDateTimeOffset(member), entityProperty => entityProperty.DateTimeOffsetValue.Value) },
                { typeof(DateTimeOffset?), new EntityPropertyResolver<DateTimeOffset?>(EntityProperty.GeneratePropertyForDateTimeOffset, entityProperty => entityProperty.DateTimeOffsetValue) },
                { typeof(string), new EntityPropertyResolver<string>(EntityProperty.GeneratePropertyForString, entityProperty => entityProperty.StringValue) },
                { typeof(byte[]), new EntityPropertyResolver<byte[]>(EntityProperty.GeneratePropertyForByteArray, entityProperty => entityProperty.BinaryValue) },
                { typeof(Uri), new EntityPropertyResolver<Uri>(member => EntityProperty.GeneratePropertyForString(member?.ToString()), entityProperty => entityProperty.StringValue == null ? null : new Uri(entityProperty.StringValue)) }
            };
            // ReSharper restore PossibleInvalidOperationException
        }

        public static IReadOnlyDictionary<Type, IEntityPropertyResolver> Default => DefaultInternal;

        internal static IEntityPropertyResolver GetEntityPropertyResolver(this IReadOnlyDictionary<Type, IEntityPropertyResolver> propertyResolvers, Type type)
        {
            IEntityPropertyResolver propertyResolver;

            if (propertyResolvers != null)
            {
                if (propertyResolvers.TryGetValue(type, out propertyResolver))
                {
                    return propertyResolver;
                }
            }

            if (Default.TryGetValue(type, out propertyResolver))
            {
                return propertyResolver;
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return GetEntityPropertyResolverForNullable(GetEntityPropertyResolverForOtherTypes(type.GenericTypeArguments[0]));
            }

            return GetEntityPropertyResolverForOtherTypes(type);
        }

        private static IEntityPropertyResolver GetEntityPropertyResolverForNullable(IEntityPropertyResolver propertyResolver)
        {
            return new EntityPropertyResolver(
                obj => obj == null ? EntityProperty.GeneratePropertyForString(null) : propertyResolver.MemberToEntityPropertyConverter.Invoke(obj),
                entityProperty => entityProperty.StringValue == null ? null : propertyResolver.EntityPropertyToMemberConverter.Invoke(entityProperty));
        }

        private static IEntityPropertyResolver GetEntityPropertyResolverForOtherTypes(Type type)
        {
            if (type.IsEnum)
            {
                if (type.GetCustomAttributes(typeof(FlagsAttribute), true).Any())
                {
                    return new EntityPropertyResolver<Enum>(
                        enumValue => EntityProperty.GeneratePropertyForString(enumValue.ToString("F")),
                        entityProperty => (Enum)Enum.Parse(type, entityProperty.StringValue, true));
                }

                return new EntityPropertyResolver<Enum>(
                    enumValue => EntityProperty.GeneratePropertyForString(enumValue.ToString("G")),
                    entityProperty => (Enum)Enum.Parse(type, entityProperty.StringValue, true));
            }

            throw new ResolverNotFoundException($"The property resolver is not found for type {type.FullName}.");
        }
    }
}
