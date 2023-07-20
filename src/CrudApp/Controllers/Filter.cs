using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace CrudApp.Controllers;


public static class Filter
{
    [StringSyntax(StringSyntaxAttribute.Regex)]
    private const string _filterConditionRegex = "(?<expression>(?<property>[^ ]+) (?<comparison>[^ ]+) (?<value>(?:[^ ]| (?!AND ))+))";
    private static readonly Regex _filterRegex = new Regex($"^{_filterConditionRegex}(?: AND {_filterConditionRegex})*$", RegexOptions.Compiled);

    public static bool TryApply<T>(ref IQueryable<T> query, string? filterString, [NotNullWhen(false)] out string? error)
    {
        if (TryParse<T>(filterString, out var conditions, out error))
        {
            query = query.Where(ToPredicate<T>(conditions));
            return true;
        }
        return false;
    }

    private static bool TryParse<T>(
        string? filterString,
        [NotNullWhen(true)] out List<FilterCondition>? conditions,
        [NotNullWhen(false)] out string? error)
    {
        conditions = new List<FilterCondition>();
        error = null;

        if (string.IsNullOrEmpty(filterString))
        {
            return true;
        }

        var match = _filterRegex.Match(filterString);
        if (!match.Success)
        {
            error = "Invalid filter syntax.";
            return false;
        }

        var expressionCount = match.Groups["expression"].Captures.Count;
        for (int i = 0; i < expressionCount; i++)
        {
            var propertyPath = match.Groups["property"].Captures[i].Value;
            if (!PropertyPathParser.TryParsePropertyPath<T>(propertyPath, out var propertyInfos, out error))
            {
                return false;
            }

            var valueAsString = match.Groups["value"].Captures[i].Value;
            object? value;
            try
            {
                value = Convert.ChangeType(valueAsString, propertyInfos.Last().PropertyType);
            }
            catch (Exception ex)
            {
                error = $"Could not convert value {valueAsString} to {propertyInfos.Last().PropertyType.Name}. " + ex.Message;
                return false;
            }

            var comparisonString = match.Groups["comparison"].Captures[i].Value;
            var comparison = Enum.Parse<QueryFilterComparison>(comparisonString);
            conditions.Add(new(propertyInfos, comparison, value));
        }
        return true;
    }

    private static Expression<Func<T, bool>> ToPredicate<T>(List<FilterCondition> conditions)
    {
        var body = (Expression)Expression.Constant(true);
        var entityParameter = Expression.Parameter(typeof(T));
        foreach (var filter in conditions)
        {
            Expression propertyExpression = entityParameter;
            foreach(var propertyInfo in filter.PropertyInfos)
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