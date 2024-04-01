namespace CrudApp.Infrastructure.UtilityCode;

public static class Base32ForHumans
{
    // Characters that are easy for a person to read and enter or tell another person.
    // Only lower case characters.
    // No numbers or characters the looks like each other (l, 1, o and 0)
    private static readonly string _base32Alphabet = "abcdefghijkmnpqrstuvwxyz23456789";
    private static readonly Random _random = new Random();

    public static string GetRandomString(int length)
    {
        var resultChars = new char[length];
        for (int i = 0; i < resultChars.Length; i++)
            resultChars[i] = _base32Alphabet[_random.Next(_base32Alphabet.Length)];

        return new(resultChars);
    }
}
