using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.WindowsAzure.Storage.Table;

namespace Euyuil.Azure.Storage.Helper.Table
{
    public interface IEntityKeySegmentResolver
    {
        Func<object, string> MemberToKeySegmentConverter { get; }

        Func<string, object> KeySegmentToMemberConverter { get; }
    }

    public interface IEntityKeySegmentResolver<TMember> : IEntityKeySegmentResolver
    {
        new Func<TMember, string> MemberToKeySegmentConverter { get; }

        new Func<string, TMember> KeySegmentToMemberConverter { get; }
    }

    public class EntityKeySegmentResolver<TMember> : IEntityKeySegmentResolver<TMember>
    {
        public EntityKeySegmentResolver()
        {
        }

        public EntityKeySegmentResolver(Func<TMember, string> memberToKeySegmentConverter, Func<string, TMember> keySegmentToMemberConverter)
        {
            MemberToKeySegmentConverter = memberToKeySegmentConverter;
            KeySegmentToMemberConverter = keySegmentToMemberConverter;
        }

        Func<object, string> IEntityKeySegmentResolver.MemberToKeySegmentConverter =>
            MemberToKeySegmentConverter == null ? (Func<object, string>)null : ConvertMemberToKeySegment;

        Func<string, object> IEntityKeySegmentResolver.KeySegmentToMemberConverter =>
            KeySegmentToMemberConverter == null ? (Func<string, object>)null : ConvertKeySegmentToMember;

        public Func<TMember, string> MemberToKeySegmentConverter { get; set; }

        public Func<string, TMember> KeySegmentToMemberConverter { get; set; }

        private string ConvertMemberToKeySegment(object member)
        {
            return MemberToKeySegmentConverter.Invoke((TMember)member);
        }

        private object ConvertKeySegmentToMember(string key)
        {
            return KeySegmentToMemberConverter.Invoke(key);
        }
    }

    public static class EntityKeySegmentResolvers
    {
        private const long DateTimeMaxValueTicks = long.MaxValue; // DateTime.MaxValue.Ticks;

        private static readonly Dictionary<Type, IEntityKeySegmentResolver> DefaultInternal;

        static EntityKeySegmentResolvers()
        {
            DefaultInternal = new Dictionary<Type, IEntityKeySegmentResolver>
            {
                { typeof(int), new EntityKeySegmentResolver<int>(member => member.ToString("x8"), key => int.Parse(key, NumberStyles.HexNumber)) },
                { typeof(long), new EntityKeySegmentResolver<long>(member => member.ToString("x16"), key => long.Parse(key, NumberStyles.HexNumber)) },
                { typeof(Guid), new EntityKeySegmentResolver<Guid>(member => member.ToString("d"), Guid.Parse) },
                { typeof(string), new EntityKeySegmentResolver<string>(member => member, key => key) },
                { typeof(DateTime), new EntityKeySegmentResolver<DateTime>(ConvertDateTimeToKeySegment, ConvertKeySegmentToDateTime) },
                { typeof(DateTimeOffset), new EntityKeySegmentResolver<DateTimeOffset>(ConvertDateTimeOffsetToKeySegment, ConvertKeySegmentToDateTimeOffset) }
            };
        }

        public static IReadOnlyDictionary<Type, IEntityKeySegmentResolver> Default => DefaultInternal;

        internal static IEntityKeySegmentResolver GetEntityKeySegmentResolver(this IReadOnlyDictionary<Type, IEntityKeySegmentResolver> keySegmentResolvers, Type type)
        {
            IEntityKeySegmentResolver keySegmentResolver;

            if (keySegmentResolvers != null)
            {
                if (keySegmentResolvers.TryGetValue(type, out keySegmentResolver))
                {
                    return keySegmentResolver;
                }
            }

            if (Default.TryGetValue(type, out keySegmentResolver))
            {
                return keySegmentResolver;
            }

            return GetEntityKeySegmentResolverForOtherTypes(type);
        }

        private static IEntityKeySegmentResolver GetEntityKeySegmentResolverForOtherTypes(Type type)
        {
            if (type.IsEnum)
            {
                if (type.GetCustomAttributes(typeof(FlagsAttribute), true).Any())
                {
                    return new EntityKeySegmentResolver<Enum>(
                        enumValue => enumValue.ToString("F"),
                        stringValue => (Enum)Enum.Parse(type, stringValue, true));
                }

                return new EntityKeySegmentResolver<Enum>(
                    enumValue => enumValue.ToString("G"),
                    stringValue => (Enum)Enum.Parse(type, stringValue, true));
            }

            throw new ResolverNotFoundException($"The property resolver is not found for type {type.FullName}.");
        }

        internal static string ConvertDateTimeToKeySegment(DateTime dateTime)
        {
            var utcDateTime = dateTime.Kind == DateTimeKind.Utc ? dateTime : dateTime.ToUniversalTime();
            var ticksToMax = DateTimeMaxValueTicks - utcDateTime.Ticks;
            return ticksToMax.ToString("x16");
        }

        internal static DateTime ConvertKeySegmentToDateTime(string ticksToMaxStr)
        {
            var ticksToMax = long.Parse(ticksToMaxStr, NumberStyles.HexNumber);
            var ticks = DateTimeMaxValueTicks - ticksToMax;
            var utcDateTime = new DateTime(ticks, DateTimeKind.Utc);
            return utcDateTime;
        }

        internal static string ConvertDateTimeOffsetToKeySegment(DateTimeOffset dateTime)
        {
            var utcDateTime = dateTime.UtcDateTime;
            var ticksToMax = DateTimeMaxValueTicks - utcDateTime.Ticks;
            return ticksToMax.ToString("x16");
        }

        internal static DateTimeOffset ConvertKeySegmentToDateTimeOffset(string ticksToMaxStr)
        {
            var ticksToMax = long.Parse(ticksToMaxStr, NumberStyles.HexNumber);
            var ticks = DateTimeMaxValueTicks - ticksToMax;
            var utcDateTimeOffset = new DateTimeOffset(ticks, TimeSpan.Zero);
            return utcDateTimeOffset;
        }
    }
}
