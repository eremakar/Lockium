using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Data.Repository.Helpers
{
    public static class SelectHelper
    {
        public static IQueryable<T> ApplySelect<T>(IQueryable<T> source, SelectBase select)
        {
            if (select == null)
                return source;

            var hasMembers = select.Members != null && select.Members.Count > 0;
            var hasRelations = select.Relations != null && select.Relations.Count > 0;
            if (!hasMembers && !hasRelations)
                return source;

            var param = Expression.Parameter(typeof(T), "_");
            var bindings = new List<MemberBinding>();
            var bound = new HashSet<string>(StringComparer.Ordinal);

            void AddBinding(PropertyInfo prop, Expression value)
            {
                if (bound.Add(prop.Name))
                    bindings.Add(Expression.Bind(prop, value));
            }

            var idProp = typeof(T).GetProperty("Id", BindingFlags.Instance | BindingFlags.Public);
            if (idProp != null)
                AddBinding(idProp, Expression.Property(param, idProp));

            if (hasMembers)
            {
                foreach (var name in select.Members.Distinct(StringComparer.Ordinal))
                {
                    if (string.IsNullOrEmpty(name) || string.Equals(name, "Id", StringComparison.Ordinal))
                        continue;
                    var prop = typeof(T).GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
                    if (prop == null)
                        continue;
                    AddBinding(prop, Expression.Property(param, prop));
                }
            }

            if (hasRelations)
            {
                foreach (var rel in select.Relations)
                {
                    if (rel == null || string.IsNullOrEmpty(rel.Name))
                        continue;
                    var prop = typeof(T).GetProperty(rel.Name, BindingFlags.Instance | BindingFlags.Public);
                    if (prop == null)
                        continue;
                    var valueExpr = BuildNavigationValue(Expression.Property(param, prop), prop.PropertyType, rel.Select);
                    AddBinding(prop, valueExpr);
                }
            }

            if (bindings.Count == 0)
                return source;

            var memberInit = Expression.MemberInit(CreateNewExpression(typeof(T)), bindings);
            var lambda = Expression.Lambda<Func<T, T>>(memberInit, param);
            return source.Select(lambda);
        }

        private static Expression BuildNavigationValue(Expression navAccessor, Type declaredType, SelectBase nestedSelect)
        {
            var effectiveType = Nullable.GetUnderlyingType(declaredType) ?? declaredType;

            if (effectiveType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(effectiveType))
                return navAccessor;

            var nestedEmpty = nestedSelect == null ||
                ((nestedSelect.Members?.Count ?? 0) == 0 && (nestedSelect.Relations?.Count ?? 0) == 0);
            if (nestedEmpty)
                return navAccessor;

            var newExpr = CreateNewExpression(effectiveType);
            var bindings = new List<MemberBinding>();
            var bound = new HashSet<string>(StringComparer.Ordinal);

            void AddBind(PropertyInfo p, Expression e)
            {
                if (bound.Add(p.Name))
                    bindings.Add(Expression.Bind(p, e));
            }

            var nestedId = effectiveType.GetProperty("Id", BindingFlags.Instance | BindingFlags.Public);
            if (nestedId != null)
                AddBind(nestedId, Expression.Property(navAccessor, nestedId));

            foreach (var name in nestedSelect.Members ?? Enumerable.Empty<string>())
            {
                if (string.IsNullOrEmpty(name) || string.Equals(name, "Id", StringComparison.Ordinal))
                    continue;
                var p = effectiveType.GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
                if (p == null)
                    continue;
                AddBind(p, Expression.Property(navAccessor, p));
            }

            foreach (var rel in nestedSelect.Relations ?? Enumerable.Empty<RelationBase>())
            {
                if (rel == null || string.IsNullOrEmpty(rel.Name))
                    continue;
                var p = effectiveType.GetProperty(rel.Name, BindingFlags.Instance | BindingFlags.Public);
                if (p == null)
                    continue;
                var innerAccess = Expression.Property(navAccessor, p);
                var innerExpr = BuildNavigationValue(innerAccess, p.PropertyType, rel.Select);
                AddBind(p, innerExpr);
            }

            var init = Expression.MemberInit(newExpr, bindings);

            if (!declaredType.IsValueType)
            {
                var nullConst = Expression.Constant(null, declaredType);
                var isNull = Expression.Equal(navAccessor, nullConst);
                return Expression.Condition(isNull, nullConst, init);
            }

            return init;
        }

        private static NewExpression CreateNewExpression(Type type)
        {
            var ctor = type.GetConstructor(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                Type.EmptyTypes,
                null);
            if (ctor == null)
                throw new InvalidOperationException($"Type {type.FullName} has no parameterless constructor; cannot build Select projection.");
            return Expression.New(ctor);
        }
    }
}
