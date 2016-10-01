using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Euyuil.Azure.Storage.Helper.Table
{
    public class EntityCompoundKeyInfo
    {
        public const string Separator = "__";

        protected const char SeparatorChar = '_';

        protected static readonly string[] SeparatorArray = { Separator };
    }

    public class EntityCompoundKeyInfo<TObject> : EntityCompoundKeyInfo
    {
        private readonly Func<TObject, string>[] _keyGetters;

        private readonly Action<TObject, string>[] _keySetters;

        public EntityCompoundKeyInfo(
            Expression<Func<TObject, object>> compoundKeyExpression,
            IReadOnlyDictionary<Type, IEntityKeyResolver> keyResolvers = null)
            : this(null, compoundKeyExpression, keyResolvers)
        {
        }

        public EntityCompoundKeyInfo(
            string compoundKeyPrefix,
            Expression<Func<TObject, object>> compoundKeyExpression,
            IReadOnlyDictionary<Type, IEntityKeyResolver> keyResolvers = null)
        {
            if (compoundKeyExpression == null) throw new ArgumentNullException(nameof(compoundKeyExpression));

            CompoundKeyPrefix = compoundKeyPrefix;

            Type[] memberTypes;
            string[] memberNames;
            Func<TObject, object>[] memberGetters;
            Action<TObject, object>[] memberSetters;

            var memberCount = Utilities.ParseLambdaExpression(compoundKeyExpression, out memberTypes, out memberNames, out memberGetters, out memberSetters);

            if (keyResolvers == null) keyResolvers = EntityKeyResolvers.Default;

            _keyGetters = new Func<TObject, string>[memberCount];
            _keySetters = new Action<TObject, string>[memberCount];

            for (var i = 0; i < memberCount; ++i)
            {
                var memberType = memberTypes[i];
                var memberGetter = memberGetters[i];
                var memberSetter = memberSetters[i];
                var keyResolver = keyResolvers[memberType];

                _keyGetters[i] = obj => keyResolver.PropertyToKeyConverter.Invoke(memberGetter.Invoke(obj));
                _keySetters[i] = (obj, entityProperty) => memberSetter.Invoke(obj, keyResolver.KeyToPropertyConverter.Invoke(entityProperty));
            }
        }

        public string CompoundKeyPrefix { get; }

        public Func<TObject, string> CompoundKeyGetter => GetCompoundKeyFromObject;

        public Action<TObject, string> CompoundKeySetter => SetCompoundKeyToObject;

        private string GetCompoundKeyFromObject(TObject obj)
        {
            var sb = new StringBuilder();

            if (CompoundKeyPrefix != null) sb.Append(CompoundKeyPrefix).Append(Separator);

            for (var i = 0; i < _keyGetters.Length; i++)
            {
                if (i > 0) sb.Append(Separator);

                var key = _keyGetters[i].Invoke(obj);

                if (key.Contains(SeparatorChar))
                    throw new FormatException($"The key {key} unexpectedly contains separator char {SeparatorChar}.");

                sb.Append(key);
            }

            return sb.ToString();
        }

        private void SetCompoundKeyToObject(TObject obj, string key)
        {
            if (key == null)
                throw new FormatException("The key is unexpectedly null.");

            if (CompoundKeyPrefix != null)
            {
                if (!key.StartsWith($"{CompoundKeyPrefix}{Separator}"))
                    throw new FormatException($"The key {key} is expected to start with {CompoundKeyPrefix}{Separator}.");
                key = key.Substring(CompoundKeyPrefix.Length + Separator.Length);
            }

            if (string.Equals(key, string.Empty) && _keySetters.Length == 0)
                return;

            var keyArray = key.Split(SeparatorArray, StringSplitOptions.None);

            if (keyArray.Length != _keySetters.Length)
                throw new FormatException(
                    $"The key {key} was expected to have {_keySetters.Length} parts but it actually has {keyArray.Length}.");

            for (var i = 0; i < keyArray.Length; ++i)
                _keySetters[i].Invoke(obj, keyArray[i]);
        }
    }
}
