using System.Diagnostics.CodeAnalysis;
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
    private const string _filterConditionRegex = "(?<expression>(?<property>[^ ]+) (?<operator>[^ ]+) (?<value>(?:[^ ]| (?!AND ))+))";
    private static readonly Regex _filterRegex = new Regex($"^{_filterConditionRegex}(?: AND {_filterConditionRegex})*$", RegexOptions.Compiled);

    public static Result<IQueryable<T>> ApplyFiltering<T>(this IQueryable<T> query, FilteringParams filteringParams) where T : notnull
    {
        return Parse<T>(filteringParams?.Filter)
            .Select(ToPredicate<T>)
            .Select(query.Where);
    }

    private static Result<List<FilterCondition>> Parse<T>(string? filterString)
    {
        var conditions = new List<FilterCondition>();

        if (string.IsNullOrEmpty(filterString))
        {
            return conditions;
        }

        var match = _filterRegex.Match(filterString);
        if (!match.Success)
        {
            return new Error.InvalidFilterFormat(filterString);
        }

        var expressionCount = match.Groups["expression"].Captures.Count;
        for (int i = 0; i < expressionCount; i++)
        {
            var propertyPath = match.Groups["property"].Captures[i].Value;
            if (typeof(T).ParsePropertyPath(propertyPath).TryGetError(out var error, out var propertyInfos))
                return error;

            var valueAsString = match.Groups["value"].Captures[i].Value;
            var lastProperty = propertyInfos[propertyInfos.Count - 1];
            var valueType = Nullable.GetUnderlyingType(lastProperty.PropertyType) ?? lastProperty.PropertyType; // if nullable then get underlying type else get property type as-is
            object? value;
            try
            {
                value = Convert.ChangeType(valueAsString, valueType);
            }
            catch (Exception ex)
            {
                return new Error.CannotConvertValueInFilterToTheExpectedType(valueAsString, valueType, ex);
            }

            var operatorString = match.Groups["operator"].Captures[i].Value;
            QueryFilterOperator? filterOperator;
            try
            {
                filterOperator = Enum.Parse<QueryFilterOperator>(operatorString);
            }
            catch (Exception ex)
            {
                return new Error.InvalidOperatorInFilter(operatorString, ex);
            }
            conditions.Add(new(propertyInfos, filterOperator.Value, value));
        }
        return conditions;
    }

    private static Result<Expression<Func<T, bool>>> ToPredicate<T>(List<FilterCondition> conditions)
    {
        var body = (Expression)Expression.Constant(true);
        var entityParameter = Expression.Parameter(typeof(T));
        foreach (var filter in conditions)
        {
            Expression propertyExpression = entityParameter;
            foreach (var propertyInfo in filter.PropertyInfos)
                propertyExpression = Expression.Property(propertyExpression, propertyInfo);

            var valueExpression = Expression.Constant(filter.Value, propertyExpression.Type);
            Expression? condition;
            try
            {
                condition = filter.Operator switch
                {
                    QueryFilterOperator.EQ => Expression.Equal(propertyExpression, valueExpression),
                    QueryFilterOperator.NE => Expression.NotEqual(propertyExpression, valueExpression),
                    QueryFilterOperator.GT => Expression.GreaterThan(propertyExpression, valueExpression),
                    QueryFilterOperator.LT => Expression.LessThan(propertyExpression, valueExpression),
                    QueryFilterOperator.GE => Expression.GreaterThanOrEqual(propertyExpression, valueExpression),
                    QueryFilterOperator.LE => Expression.LessThanOrEqual(propertyExpression, valueExpression),
                    _ => null
                };
            }
            catch (InvalidOperationException ex)
            {
                return new Error.OperatorCannotBeUsedOnTheValueType(filter.Operator.ToString(), filter.Value.GetType(), ex);
            }
            if (condition is null)
                throw new NotImplementedException($"Filter operator '{filter.Operator}' not implemented.");
            body = Expression.AndAlso(body, condition);
        }
        return Expression.Lambda<Func<T, bool>>(body, entityParameter);
    }

    private enum QueryFilterOperator
    {
        EQ, NE, LT, GT, GE, LE,
    }

    private sealed class FilterCondition
    {
        public List<PropertyInfo> PropertyInfos { get; }
        public QueryFilterOperator Operator { get; }
        public object Value { get; }

        public FilterCondition(List<PropertyInfo> propertyInfos, QueryFilterOperator filterOperator, object value)
        {
            PropertyInfos = propertyInfos;
            Operator = filterOperator;
            Value = value;
        }
    }
}