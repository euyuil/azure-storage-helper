using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Euyuil.Azure.Storage.Helper.Table
{
    public class EntityKeyInfo
    {
        public const string Separator = "__";

        protected const char SeparatorChar = '_';

        protected static readonly string[] SeparatorArray = { Separator };
    }

    public class EntityKeyInfo<TObject> : EntityKeyInfo
    {
        private readonly Func<TObject, string>[] _keySegmentGetters;

        private readonly Action<TObject, string>[] _keySegmentSetters;

        public EntityKeyInfo(
            Expression<Func<TObject, object>> keySegmentsExpression = null,
            IReadOnlyDictionary<Type, IEntityKeySegmentResolver> keySegmentResolvers = null)
            : this(null, keySegmentsExpression, keySegmentResolvers)
        {
        }

        public EntityKeyInfo(
            string keyPrefix,
            Expression<Func<TObject, object>> keySegmentsExpression = null,
            IReadOnlyDictionary<Type, IEntityKeySegmentResolver> keySegmentResolvers = null)
        {
            KeyPrefix = keyPrefix;

            if (keySegmentsExpression == null)
            {
                _keySegmentGetters = new Func<TObject, string>[0];
                _keySegmentSetters = new Action<TObject, string>[0];
                return;
            }

            Type[] memberTypes;
            string[] memberNames;
            Func<TObject, object>[] memberGetters;
            Action<TObject, object>[] memberSetters;

            var memberCount = InternalUtilities.ParseLambdaExpression(
                keySegmentsExpression, out memberTypes, out memberNames, out memberGetters, out memberSetters);

            _keySegmentGetters = new Func<TObject, string>[memberCount];
            _keySegmentSetters = new Action<TObject, string>[memberCount];

            for (var i = 0; i < memberCount; ++i)
            {
                var memberType = memberTypes[i];
                var memberGetter = memberGetters[i];
                var memberSetter = memberSetters[i];
                var keySegmentResolver = keySegmentResolvers.GetEntityKeySegmentResolver(memberType);

                _keySegmentGetters[i] = obj => keySegmentResolver.MemberToKeySegmentConverter.Invoke(memberGetter.Invoke(obj));
                _keySegmentSetters[i] = (obj, keySegment) => memberSetter.Invoke(obj, keySegmentResolver.KeySegmentToMemberConverter.Invoke(keySegment));
            }
        }

        public string KeyPrefix { get; }

        public Func<TObject, string> KeyGetter => GetKeyFromObject;

        public Action<TObject, string> KeySetter => SetKeyToObject;

        private string GetKeyFromObject(TObject obj)
        {
            var sb = new StringBuilder();

            if (KeyPrefix != null) sb.Append(KeyPrefix);

            for (var i = 0; i < _keySegmentGetters.Length; i++)
            {
                if (i > 0) sb.Append(Separator);
                else if (KeyPrefix != null) sb.Append(Separator);

                var key = _keySegmentGetters[i].Invoke(obj);

                if (key.Contains(SeparatorChar))
                    throw new FormatException($"The key {key} unexpectedly contains separator char {SeparatorChar}.");

                sb.Append(key);
            }

            return sb.ToString();
        }

        private void SetKeyToObject(TObject obj, string key)
        {
            if (key == null)
                throw new FormatException("The key is unexpectedly null.");

            if (KeyPrefix == null)
            {
                if (_keySegmentSetters.Length == 0)
                {
                    if (!string.Equals(key, string.Empty))
                        throw new FormatException($"The key {key} is expected to be empty.");
                    return;
                }
            }
            else
            {
                if (!key.StartsWith(KeyPrefix))
                    throw new FormatException($"The key {key} is expected to start with {KeyPrefix}.");

                if (_keySegmentSetters.Length == 0)
                {
                    if (key.Length != KeyPrefix.Length)
                        throw new FormatException($"The key {key} is expected to be the same as prefix {KeyPrefix}.");
                    return;
                }

                if (key.Length < KeyPrefix.Length + Separator.Length)
                    throw new FormatException($"The length of key {key} is not expected.");

                key = key.Substring(KeyPrefix.Length + Separator.Length);
            }

            var keyArray = key.Split(SeparatorArray, StringSplitOptions.None);

            if (keyArray.Length != _keySegmentSetters.Length)
                throw new FormatException(
                    $"The key {key} was expected to have {_keySegmentSetters.Length} parts but it actually has {keyArray.Length}.");

            for (var i = 0; i < keyArray.Length; ++i)
                _keySegmentSetters[i].Invoke(obj, keyArray[i]);
        }
    }
}
