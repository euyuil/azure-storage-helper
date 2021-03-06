﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.WindowsAzure.Storage.Table;

namespace Euyuil.Azure.Storage.Helper.Table
{
    public class EntityPropertiesInfo<TObject>
    {
        private readonly Dictionary<string, Func<TObject, EntityProperty>> _propertyGetters = new Dictionary<string, Func<TObject, EntityProperty>>();

        private readonly Dictionary<string, Action<TObject, EntityProperty>> _propertySetters = new Dictionary<string, Action<TObject, EntityProperty>>();

        public EntityPropertiesInfo(
            Expression<Func<TObject, object>> propertiesExpression,
            IReadOnlyDictionary<Type, IEntityPropertyResolver> propertyResolvers = null)
        {
            if (propertiesExpression == null) return;

            Type[] memberTypes;
            string[] memberNames;
            Func<TObject, object>[] memberGetters;
            Action<TObject, object>[] memberSetters;

            var memberCount = InternalUtilities.ParseLambdaExpression(
                propertiesExpression, out memberTypes, out memberNames, out memberGetters, out memberSetters);

            for (var i = 0; i < memberCount; ++i)
            {
                var memberType = memberTypes[i];
                var memberName = memberNames[i];
                var memberGetter = memberGetters[i];
                var memberSetter = memberSetters[i];
                var propertyResolver = propertyResolvers.GetEntityPropertyResolver(memberType);

                _propertyGetters[memberName] = obj => propertyResolver.MemberToEntityPropertyConverter.Invoke(memberGetter.Invoke(obj));
                _propertySetters[memberName] = (obj, entityProperty) => memberSetter.Invoke(obj, propertyResolver.EntityPropertyToMemberConverter.Invoke(entityProperty));
            }
        }

        public Func<TObject, IEnumerable<KeyValuePair<string, EntityProperty>>> PropertiesGetter => GetEntityPropertiesFromObject;

        public Action<TObject, IEnumerable<KeyValuePair<string, EntityProperty>>> PropertiesSetter => SetEntityPropertiesToObject;

        private IEnumerable<KeyValuePair<string, EntityProperty>> GetEntityPropertiesFromObject(TObject obj)
        {
            return
                from entry in _propertyGetters
                let propertyName = entry.Key
                let propertyGetter = entry.Value
                select new KeyValuePair<string, EntityProperty>(propertyName, propertyGetter.Invoke(obj));
        }

        private void SetEntityPropertiesToObject(TObject obj, IEnumerable<KeyValuePair<string, EntityProperty>> entityProperties)
        {
            foreach (var entry in entityProperties)
            {
                var propertyName = entry.Key;
                var propertyValue = entry.Value;
                var propertySetter = _propertySetters[propertyName];

                propertySetter.Invoke(obj, propertyValue);
            }
        }
    }
}
