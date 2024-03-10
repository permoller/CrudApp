using System.Collections;

namespace CrudApp.Infrastructure.UtilityCode;

public static class DictionaryUtils
{
    public enum DuplicateKeyHandling { Throw, KeepFirst, KeepLast, Rename }

    public static IEnumerable<KeyValuePair<TKeyOut, TValueOut>> ToKeyValuePairs<TKeyOut, TValueOut>(this IDictionary dictionary, Func<object, TKeyOut> keyConverter, Func<object?, TValueOut> valueConverter)
    {
        var enumerator = dictionary.GetEnumerator();
        while (enumerator.MoveNext())
        {
            yield return new KeyValuePair<TKeyOut, TValueOut>(keyConverter(enumerator.Key), valueConverter(enumerator.Value));
        }
    }

    public static Dictionary<string, T> ToDictionary<T>(this IEnumerable<KeyValuePair<string,T>> keyValuePairs, DuplicateKeyHandling duplicateKeyHandling)
    {
        var dic = new Dictionary<string,T>();
        foreach(var kvp in keyValuePairs)
        {
            var counter = 1;
            while (!dic.TryAdd(counter == 1 ? kvp.Key : $"{kvp.Key}#{counter}" , kvp.Value))
            {
                switch (duplicateKeyHandling)
                {
                    case DuplicateKeyHandling.Throw:
                        throw new ArgumentException($"Duplicate key '{kvp.Key}'.");
                    case DuplicateKeyHandling.KeepFirst:
                        break;
                    case DuplicateKeyHandling.KeepLast:
                        dic[kvp.Key] = kvp.Value;
                        break;
                    case DuplicateKeyHandling.Rename:
                        counter++;
                        continue;
                    default:
                        throw new ArgumentException($"'{nameof(duplicateKeyHandling)}' has en invalid value '{duplicateKeyHandling}'.");
                }
            }
        }
        return dic;
    }
}
