using System.ComponentModel;

namespace JonSkeet.WpfUtil;

public static class TypeConverters
{
    public static bool TryConvert<T>(this TypeConverter converter, string text, out T value)
    {
        if (converter.IsValid(text))
        {
            value = (T) converter.ConvertFrom(text);
            return true;
        }
        else
        {
            value = default(T);
            return false;
        }
    }
}
