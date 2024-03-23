using System.Runtime.CompilerServices;
namespace CrudApp.Infrastructure.Primitives;

public partial class Error
{
    [InterpolatedStringHandler]
    public readonly ref struct ErrorDetailsInterpolatedStringHandler
    {
        private readonly StringBuilder _sb;
        public readonly Dictionary<string, object?> Properties { get; }

        public ErrorDetailsInterpolatedStringHandler(int literalLength, int formattedCount)
        {
            _sb = new StringBuilder(literalLength);
            Properties = new(formattedCount);
        }

        internal string GetFormattedText() => _sb.ToString();

        public void AppendLiteral(string s) => _sb.Append(s);

        public void AppendFormatted<T>(T? value, [CallerArgumentExpression(nameof(value))] string? key = null)
        {
            if (key is not null)
                Properties[key] = value;
            _sb.Append(value?.ToString());
        }

        public void AppendFormatted<T>(T value, string format, [CallerArgumentExpression(nameof(value))] string? key = null) where T : IFormattable
        {
            if (key is not null)
                Properties[key] = value;
            _sb.Append(value?.ToString(format, null));
        }

        public void AppendFormatted(Type value, [CallerArgumentExpression(nameof(value))] string? key = null)
        {
            if (key is not null)
                Properties[key] = value;
            _sb.Append(value?.Name);
        }
    }
}
