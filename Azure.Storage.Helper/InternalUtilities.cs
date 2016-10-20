using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Euyuil.Azure.Storage.Helper
{
    internal static class InternalUtilities
    {
        internal static int ParseLambdaExpression<TObject>(
            Expression<Func<TObject, object>> lambdaExpression,
            out Type[] memberTypes,
            out string[] memberNames,
            out Func<TObject, object>[] memberGetters,
            out Action<TObject, object>[] memberSetters)
        {
            var parameterExpression = lambdaExpression.Parameters.Single();
            if (parameterExpression.Type != typeof(TObject))
                throw new ArgumentException($"The type {parameterExpression.Type} of the parameter of the expression is not expected {typeof(TObject)}.");

            var newExpression = lambdaExpression.Body as NewExpression;
            if (newExpression == null)
            {
                var memberConstantExpression = UnwrapConvertExpression(lambdaExpression.Body);

                Type memberType;
                string memberName;
                Func<TObject, object> memberGetter;
                Action<TObject, object> memberSetter;
                ParseMemberConstantExpression(memberConstantExpression, out memberType, out memberName, out memberGetter, out memberSetter);

                memberTypes = Enumerable.Repeat(memberType, 1).ToArray();
                memberNames = Enumerable.Repeat(memberName, 1).ToArray();
                memberGetters = Enumerable.Repeat(memberGetter, 1).ToArray();
                memberSetters = Enumerable.Repeat(memberSetter, 1).ToArray();
            }
            else
            {
                var properties = newExpression.Members?.Where(member => member is PropertyInfo).Cast<PropertyInfo>().ToArray();

                memberNames = properties?.Select(property => property.Name).ToArray() ?? new string[0];

                var argumentExpressions = newExpression.Arguments;

                if (memberNames.Length != argumentExpressions.Count)
                    throw new FormatException("The input expression is incorrect.");

                memberTypes = new Type[argumentExpressions.Count];
                memberGetters = new Func<TObject, object>[argumentExpressions.Count];
                memberSetters = new Action<TObject, object>[argumentExpressions.Count];

                for (var i = 0; i < argumentExpressions.Count; i++)
                {
                    string memberName;
                    ParseMemberConstantExpression(argumentExpressions[i], out memberTypes[i], out memberName, out memberGetters[i], out memberSetters[i]);
                }
            }

            return memberNames.Length;
        }

        private static void ParseMemberConstantExpression<TObject>(
            Expression memberConstantExpression,
            out Type memberType,
            out string memberName,
            out Func<TObject, object> memberGetter,
            out Action<TObject, object> memberSetter)
        {
            var memberExpression = memberConstantExpression as MemberExpression;
            if (memberExpression != null)
            {
                var propertyInfo = memberExpression.Member as PropertyInfo;
                if (propertyInfo != null)
                {
                    memberType = propertyInfo.PropertyType;
                    memberName = propertyInfo.Name;
                    memberGetter = obj => ((PropertyInfo)memberExpression.Member).GetValue(obj);
                    memberSetter = (obj, value) => propertyInfo.SetValue(obj, value);
                    return;
                }

                var fieldInfo = memberExpression.Member as FieldInfo;
                if (fieldInfo != null)
                {
                    memberType = fieldInfo.FieldType;
                    memberName = fieldInfo.Name;
                    memberGetter = obj => fieldInfo.GetValue(obj);

                    if (fieldInfo.IsStatic || fieldInfo.IsInitOnly)
                    {
                        var expectedValue = fieldInfo.GetValue(null);

                        memberSetter = (obj, value) =>
                        {
                            if (!Equals(value, expectedValue))
                                throw new FormatException($"The value {value} is not the same as expected {expectedValue}.");
                        };
                    }
                    else
                    {
                        memberSetter = (obj, value) => fieldInfo.SetValue(obj, value);
                    }

                    return;
                }

                throw new ArgumentException($"Unknown member type {memberExpression.Member.GetType()}.", nameof(memberConstantExpression));
            }

            var constantExpression = memberConstantExpression as ConstantExpression;
            if (constantExpression != null)
            {
                var expectedValue = constantExpression.Value;

                memberType = constantExpression.Type;
                memberName = null;
                memberGetter = obj => expectedValue;
                memberSetter = (obj, value) =>
                {
                    if (!Equals(value, expectedValue))
                        throw new FormatException($"The value {value} is not the same as expected {expectedValue}.");
                };

                return;
            }

            throw new InvalidOperationException($"Unable to analyze expression {memberConstantExpression}.");
        }

        private static Expression UnwrapConvertExpression(Expression expression)
        {
            var unaryExpression = expression as UnaryExpression;
            if (unaryExpression != null && unaryExpression.NodeType == ExpressionType.Convert)
                return unaryExpression.Operand;
            return expression;
        }
    }
}
