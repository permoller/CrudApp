using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace CrudApp.Controllers;

public static class OrderBy 
{

    [StringSyntax(StringSyntaxAttribute.Regex)]
    private const string _singleOrderByRegex = "(?<expression>(?<property>[^ ]+)(?<descending>| desc))";
    private static readonly Regex _orderByRegex = new Regex($"^{_singleOrderByRegex}(?:,{_singleOrderByRegex})*$", RegexOptions.Compiled);

    public static bool TryApply<T>(ref IQueryable<T> query, string? orderBy, [NotNullWhen(false)] out string? error)
    {
        error = null;
        if (string.IsNullOrWhiteSpace(orderBy))
        {
            return true;
        }

        var match = _orderByRegex.Match(orderBy);
        if (!match.Success)
        {
            error = "Invalid orderBy syntax.";
            return false;
        }

        var count = match.Groups["expression"].Captures.Count;
        for (var i = 0; i < count; i++)
        {
            var propertyPath = match.Groups["property"].Captures[i].Value;
            if (!PropertyPathParser.TryParsePropertyPath<T>(propertyPath, out var propertyInfos, out error))
            {
                return false;
            }
            var isDescending = match.Groups["descending"].Captures[i].Value == " desc";
#pragma warning disable S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields
            var unboundApplySingleOrderBy = typeof(OrderBy).GetMethods(BindingFlags.NonPublic | BindingFlags.Static).Single(m => m.Name.Equals(nameof(ApplySingleOrderBy)));
#pragma warning restore S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields
            var applySingleOrderBy = unboundApplySingleOrderBy.MakeGenericMethod(typeof(T), propertyInfos.Last().PropertyType);
            var isFirst = i == 0;
            query = (IQueryable<T>)applySingleOrderBy.Invoke(null, new object[] { query, isDescending, isFirst, propertyInfos })!;
        }

        return true;
    }

    private static IOrderedQueryable<T> ApplySingleOrderBy<T, TKey>(IQueryable<T> query, bool isDescending, bool isFirst, List<PropertyInfo> propertyInfos)
    {
        var entityParameterExpression = Expression.Parameter(typeof(T));
        Expression propertyExpression = entityParameterExpression;
        foreach(var propertyInfo in propertyInfos)
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
