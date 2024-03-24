using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace CrudApp.Infrastructure.Primitives;
public abstract partial class Error
{
    private readonly DataDictionary _data;

    protected Error(
        int httpStatucCode,
        Exception? exception = default,
        DataDictionary? data = default,
        Dictionary<string, string[]>? errors = default)
    {
        Instance = GetRandomString(length: 8);
        HttpStatucCode = httpStatucCode;
        Exception = exception;
        TraceId = Activity.Current?.Id;
        Data = _data = data ?? new();
        Errors = errors ?? [];
    }

    public int HttpStatucCode { get; }
    public string? Instance { get; }
    public Exception? Exception { get; }
    public string? TraceId { get; }
    public IReadOnlyDictionary<string, object?> Data { get; }
    public Dictionary<string, string[]> Errors { get; }


    public Error AddData(string value, [CallerArgumentExpression(nameof(value))] string? key = null) => _data.Add(value, key).Return(this);
    public Error AddData(bool value, [CallerArgumentExpression(nameof(value))] string? key = null) => _data.Add(value, key).Return(this);
    public Error AddData(byte value, [CallerArgumentExpression(nameof(value))] string? key = null) => _data.Add(value, key).Return(this);
    public Error AddData(short value, [CallerArgumentExpression(nameof(value))] string? key = null) => _data.Add(value, key).Return(this);
    public Error AddData(int value, [CallerArgumentExpression(nameof(value))] string? key = null) => _data.Add(value, key).Return(this);
    public Error AddData(long value, [CallerArgumentExpression(nameof(value))] string? key = null) => _data.Add(value, key).Return(this);
    public Error AddData(float value, [CallerArgumentExpression(nameof(value))] string? key = null) => _data.Add(value, key).Return(this);
    public Error AddData(double value, [CallerArgumentExpression(nameof(value))] string? key = null) => _data.Add(value, key).Return(this);
    public Error AddData(decimal value, [CallerArgumentExpression(nameof(value))] string? key = null) => _data.Add(value, key).Return(this);
    public Error AddData(Guid value, [CallerArgumentExpression(nameof(value))] string? key = null) => _data.Add(value, key).Return(this);
    public Error AddData(DateTime value, [CallerArgumentExpression(nameof(value))] string? key = null) => _data.Add(value, key).Return(this);
    public Error AddData(DateTimeOffset value, [CallerArgumentExpression(nameof(value))] string? key = null) => _data.Add(value, key).Return(this);
    public Error AddData(Type value, [CallerArgumentExpression(nameof(value))] string? key = null) => _data.Add(value, key).Return(this);

    public Error AddError(string field, string error)
    {
        List<string> list = Errors.TryGetValue(field, out var array) ? new(array) : new();
        list.Add(error);
        Errors[field] = list.ToArray();
        return this;
    }

    // Characters that are easy for one person to tell another person.
    // Only lower case characters
    // No numbers or characters the looks like each other (i, 1, o and 0)
    private static readonly string _base32Alphabet = "abcdefghjklmnpqrstuvwxyz23456789";
    private static readonly Random _random = new Random();

    private static string GetRandomString(int length)
    {
        var resultChars = new char[length];
        for (int i = 0; i < resultChars.Length; i++)
            resultChars[i] = _base32Alphabet[_random.Next(_base32Alphabet.Length)];

        return new(resultChars);
    }
}