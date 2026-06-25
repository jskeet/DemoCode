using JonSkeet.CoreAppUtil;
using System.ComponentModel;

namespace JonSkeet.WpfUtil;

public static class TypeConverters
{
    public static bool TryConvert<T>(this TypeConverter converter, string text, out T? value) where T : class
    {
        if (converter.IsValid(text))
        {
            value = (T?) converter.ConvertFrom(text);
            return true;
        }
        else
        {
            value = default;
            return false;
        }
    }

    public static bool TryConvertValueType<T>(this TypeConverter converter, string text, out T value) where T : struct
    {
        if (converter.IsValid(text))
        {
            value = (T) converter.ConvertFrom(text).OrThrow();
            return true;
        }
        else
        {
            value = default;
            return false;
        }
    }
}
