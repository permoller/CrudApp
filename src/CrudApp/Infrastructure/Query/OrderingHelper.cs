﻿using CrudApp.Infrastructure.UtilityCode;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace CrudApp.Infrastructure.Query;

public class OrderingParams
{
    public string? OrderBy { get; set; }
    public int? Skip { get; set; }
    public int? Take { get; set; }
}

public static class OrderingHelper
{

    [StringSyntax(StringSyntaxAttribute.Regex)]
    private const string _singleOrderByRegex = "(?<expression>(?<property>[^ ,]+)(?<descending>| desc))";
    private static readonly Regex _orderByRegex = new Regex($"^{_singleOrderByRegex}(?:,{_singleOrderByRegex})*$", RegexOptions.Compiled);

#pragma warning disable S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields
    private static readonly MethodInfo _unboundApplySingleOrderBy = typeof(OrderingHelper).GetMethods(BindingFlags.NonPublic | BindingFlags.Static).Single(m => m.Name.Equals(nameof(ApplySingleOrderBy)));
#pragma warning restore S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields


    public static Result<IQueryable<T>> ApplyOrdering<T>(this IQueryable<T> queryable, OrderingParams orderingParams)
    {
        if (orderingParams == default)
            return queryable.ToResult();

        if ((orderingParams.Skip.HasValue || orderingParams.Take.HasValue) && string.IsNullOrWhiteSpace(orderingParams.OrderBy))
            return new Error.OrderByIsRequiredWhenUsingSkipAndTake();

        if (string.IsNullOrWhiteSpace(orderingParams.OrderBy))
            return queryable.ToResult();

        var match = _orderByRegex.Match(orderingParams.OrderBy);
        if (!match.Success)
            return new Error.InvalidOrderByFormat(orderingParams.OrderBy);

        var count = match.Groups["expression"].Captures.Count;
        for (var i = 0; i < count; i++)
        {
            var propertyPath = match.Groups["property"].Captures[i].Value;
            if (typeof(T).ParsePropertyPath(propertyPath).TryGetError(out var error, out var propertyInfos))
                return error;
            var isDescending = match.Groups["descending"].Captures[i].Value == " desc";
            var keyType = propertyInfos[propertyInfos.Count - 1].PropertyType;
            var isFirst = i == 0;

            var applySingleOrderBy = _unboundApplySingleOrderBy.MakeGenericMethod(typeof(T), keyType);
            queryable = (IQueryable<T>)applySingleOrderBy.Invoke(null, new object[] { queryable, isDescending, isFirst, propertyInfos })!;
        }

        if (orderingParams.Skip.HasValue)
            queryable = queryable.Skip(orderingParams.Skip.Value);

        if (orderingParams.Take.HasValue)
            queryable = queryable.Take(orderingParams.Take.Value);

        return queryable.ToResult();
    }

    private static IOrderedQueryable<T> ApplySingleOrderBy<T, TKey>(IQueryable<T> query, bool isDescending, bool isFirst, List<PropertyInfo> propertyInfos)
    {
        var entityParameterExpression = Expression.Parameter(typeof(T));
        Expression propertyExpression = entityParameterExpression;
        foreach (var propertyInfo in propertyInfos)
            propertyExpression = Expression.Property(propertyExpression, propertyInfo);

        Expression<Func<T, TKey>> keySelectorExpression = Expression.Lambda<Func<T, TKey>>(propertyExpression, entityParameterExpression);
        if (isDescending)
        {
            if (!isFirst && query is IOrderedQueryable<T> orderedQuery)
                return orderedQuery.ThenByDescending(keySelectorExpression);
            else
                return query.OrderByDescending(keySelectorExpression);
        }
        else
        {
            if (!isFirst && query is IOrderedQueryable<T> orderedQuery)
                return orderedQuery.ThenBy(keySelectorExpression);
            else
                return query.OrderBy(keySelectorExpression);
        }
    }
}
