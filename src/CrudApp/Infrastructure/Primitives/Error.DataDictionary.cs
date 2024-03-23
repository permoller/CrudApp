using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace CrudApp.Infrastructure.Primitives;

public partial class Error
{
    protected class DataDictionary : IReadOnlyDictionary<string, object?>
    {
        private readonly Dictionary<string, object?> _dictionary = [];


        private DataDictionary AddInternal(string? key, object? value)
        {
            _dictionary[key ?? ""] = value;
            return this;
        }

        // Only expose methods to add simple types we know can be serialized to JSON or convert it to a string here
        public DataDictionary Add(string? value, [CallerArgumentExpression(nameof(value))] string? key = null) => AddInternal(key, value);
        public DataDictionary Add(bool? value, [CallerArgumentExpression(nameof(value))] string? key = null) => AddInternal(key, value);
        public DataDictionary Add(byte? value, [CallerArgumentExpression(nameof(value))] string? key = null) => AddInternal(key, value);
        public DataDictionary Add(short? value, [CallerArgumentExpression(nameof(value))] string? key = null) => AddInternal(key, value);
        public DataDictionary Add(int? value, [CallerArgumentExpression(nameof(value))] string? key = null) => AddInternal(key, value);
        public DataDictionary Add(long? value, [CallerArgumentExpression(nameof(value))] string? key = null) => AddInternal(key, value);
        public DataDictionary Add(float? value, [CallerArgumentExpression(nameof(value))] string? key = null) => AddInternal(key, value);
        public DataDictionary Add(decimal? value, [CallerArgumentExpression(nameof(value))] string? key = null) => AddInternal(key, value);
        public DataDictionary Add(Guid? value, [CallerArgumentExpression(nameof(value))] string? key = null) => AddInternal(key, value);
        public DataDictionary Add(DateTime? value, [CallerArgumentExpression(nameof(value))] string? key = null) => AddInternal(key, value);
        public DataDictionary Add(DateTimeOffset? value, [CallerArgumentExpression(nameof(value))] string? key = null) => AddInternal(key, value);
        public DataDictionary Add(Type? value, [CallerArgumentExpression(nameof(value))] string? key = null) => AddInternal(key, value?.Name);

        #region IReadOnlyDictionary

        public object? this[string key] => ((IReadOnlyDictionary<string, object?>)_dictionary)[key];

        public IEnumerable<string> Keys => ((IReadOnlyDictionary<string, object?>)_dictionary).Keys;

        public IEnumerable<object?> Values => ((IReadOnlyDictionary<string, object?>)_dictionary).Values;

        public int Count => ((IReadOnlyCollection<KeyValuePair<string, object?>>)_dictionary).Count;

        public bool ContainsKey(string key) => ((IReadOnlyDictionary<string, object?>)_dictionary).ContainsKey(key);

        public IEnumerator<KeyValuePair<string, object?>> GetEnumerator() => ((IEnumerable<KeyValuePair<string, object?>>)_dictionary).GetEnumerator();

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object? value) => ((IReadOnlyDictionary<string, object?>)_dictionary).TryGetValue(key, out value);

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_dictionary).GetEnumerator();

        #endregion
    }
}
