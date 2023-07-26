﻿using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace CrudApp.Infrastructure.Query;

public class FilteringParams
{
    public string? Filter { get; set; }
}

public static class FilteringHelper
{
    [StringSyntax(StringSyntaxAttribute.Regex)]
    private const string _filterConditionRegex = "(?<expression>(?<property>[^ ]+) (?<comparison>[^ ]+) (?<value>(?:[^ ]| (?!AND ))+))";
    private static readonly Regex _filterRegex = new Regex($"^{_filterConditionRegex}(?: AND {_filterConditionRegex})*$", RegexOptions.Compiled);

    public static IQueryable<T> ApplyFiltering<T>(this IQueryable<T> query, FilteringParams filteringParams)
    {
        if (filteringParams == default)
            return query;

        var conditions = Parse<T>(filteringParams.Filter);

        if(conditions.Count > 0)
            query = query.Where(ToPredicate<T>(conditions));

        return query;
    }

    private static List<FilterCondition> Parse<T>(string? filterString)
    {
        var conditions = new List<FilterCondition>();

        if (string.IsNullOrEmpty(filterString))
        {
            return conditions;
        }

        var match = _filterRegex.Match(filterString);
        if (!match.Success)
        {
            throw new ProblemDetailsException(HttpStatus.BadRequest, "Invalid filter syntax.");
        }

        var expressionCount = match.Groups["expression"].Captures.Count;
        for (int i = 0; i < expressionCount; i++)
        {
            var propertyPath = match.Groups["property"].Captures[i].Value;
            var propertyInfos = PropertyPathHelper.ParsePropertyPath<T>(propertyPath);

            var valueAsString = match.Groups["value"].Captures[i].Value;
            var lastProperty = propertyInfos[propertyInfos.Count - 1];
            var valueType = lastProperty.PropertyType;
            object? value;
            try
            {
                value = Convert.ChangeType(valueAsString, valueType);
            }
            catch (Exception ex)
            {
                throw new ProblemDetailsException(HttpStatus.BadRequest, $"Could not convert value {valueAsString} to {valueType.Name}.", ex);
            }

            var comparisonString = match.Groups["comparison"].Captures[i].Value;
            var comparison = Enum.Parse<QueryFilterComparison>(comparisonString);
            conditions.Add(new(propertyInfos, comparison, value));
        }
        return conditions;
    }

    private static Expression<Func<T, bool>> ToPredicate<T>(List<FilterCondition> conditions)
    {
        var body = (Expression)Expression.Constant(true);
        var entityParameter = Expression.Parameter(typeof(T));
        foreach (var filter in conditions)
        {
            Expression propertyExpression = entityParameter;
            foreach (var propertyInfo in filter.PropertyInfos)
                propertyExpression = Expression.Property(propertyExpression, propertyInfo);

            var valueExpression = Expression.Constant(filter.Value);
            var condition = filter.Comparison switch
            {
                QueryFilterComparison.EQ => Expression.Equal(propertyExpression, valueExpression),
                QueryFilterComparison.NE => Expression.NotEqual(propertyExpression, valueExpression),
                QueryFilterComparison.GT => Expression.GreaterThan(propertyExpression, valueExpression),
                QueryFilterComparison.LT => Expression.LessThan(propertyExpression, valueExpression),
                QueryFilterComparison.GE => Expression.GreaterThanOrEqual(propertyExpression, valueExpression),
                QueryFilterComparison.LE => Expression.LessThanOrEqual(propertyExpression, valueExpression),
                _ => throw new NotImplementedException($"Comparison {filter.Comparison} not implemented.")
            };
            body = Expression.AndAlso(body, condition);
        }
        return Expression.Lambda<Func<T, bool>>(body, entityParameter);
    }

    private enum QueryFilterComparison
    {
        EQ, NE, LT, GT, GE, LE,
    }

    private sealed class FilterCondition
    {
        public List<PropertyInfo> PropertyInfos { get; }
        public QueryFilterComparison Comparison { get; }
        public object Value { get; }

        public FilterCondition(List<PropertyInfo> propertyInfos, QueryFilterComparison comparison, object value)
        {
            PropertyInfos = propertyInfos;
            Comparison = comparison;
            Value = value;
        }
    }
}