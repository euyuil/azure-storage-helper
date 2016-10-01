using System;
using System.Collections.Generic;
using System.Globalization;

namespace Euyuil.Azure.Storage.Helper.Table
{
    public interface IEntityKeyResolver
    {
        Func<object, string> PropertyToKeyConverter { get; }

        Func<string, object> KeyToPropertyConverter { get; }
    }

    public interface IEntityKeyResolver<TProperty> : IEntityKeyResolver
    {
        new Func<TProperty, string> PropertyToKeyConverter { get; }

        new Func<string, TProperty> KeyToPropertyConverter { get; }
    }

    public class EntityKeyResolver<TProperty> : IEntityKeyResolver<TProperty>
    {
        public EntityKeyResolver()
        {
        }

        public EntityKeyResolver(Func<TProperty, string> propertyToKeyConverter, Func<string, TProperty> keyToPropertyConverter)
        {
            PropertyToKeyConverter = propertyToKeyConverter;
            KeyToPropertyConverter = keyToPropertyConverter;
        }

        Func<object, string> IEntityKeyResolver.PropertyToKeyConverter => PropertyToKeyConverter == null ? (Func<object, string>)null : PropertyToKey;

        Func<string, object> IEntityKeyResolver.KeyToPropertyConverter => KeyToPropertyConverter == null ? (Func<string, object>)null : KeyToProperty;

        public Func<TProperty, string> PropertyToKeyConverter { get; set; }

        public Func<string, TProperty> KeyToPropertyConverter { get; set; }

        private string PropertyToKey(object property)
        {
            return PropertyToKeyConverter.Invoke((TProperty)property);
        }

        private object KeyToProperty(string key)
        {
            return KeyToPropertyConverter.Invoke(key);
        }
    }

    public static class EntityKeyResolvers
    {
        private static readonly long DateTimeMaxValueTicks = DateTime.MaxValue.Ticks;

        private static readonly Dictionary<Type, IEntityKeyResolver> DefaultInternal;

        static EntityKeyResolvers()
        {
            DefaultInternal = new Dictionary<Type, IEntityKeyResolver>
            {
                { typeof(string), new EntityKeyResolver<string>(property => property, key => key) },
                { typeof(Guid), new EntityKeyResolver<Guid>(property => property.ToString("n"), key => new Guid(key)) },
                { typeof(DateTime), new EntityKeyResolver<DateTime>(DateTimeToKey, KeyToDateTime) }
            };
        }

        public static IReadOnlyDictionary<Type, IEntityKeyResolver> Default => DefaultInternal;

        private static string DateTimeToKey(DateTime dateTime)
        {
            var utcDateTime = dateTime.Kind == DateTimeKind.Utc ? dateTime : dateTime.ToUniversalTime();
            var ticksToMax = DateTimeMaxValueTicks - utcDateTime.Ticks;
            return ticksToMax.ToString("x16");
        }

        private static DateTime KeyToDateTime(string ticksToMaxStr)
        {
            var ticksToMax = long.Parse(ticksToMaxStr, NumberStyles.HexNumber);
            var ticks = DateTimeMaxValueTicks - ticksToMax;
            var utcDateTime = new DateTime(ticks, DateTimeKind.Utc);
            return utcDateTime;
        }
    }
}
