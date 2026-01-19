namespace Tenders.Application.Common;

public static class ParseEnum
{
    public static TEnum ParseEnumOrDefault<TEnum>(string? value, TEnum @default)
    where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
            return @default;

        if (int.TryParse(value, out var intValue) && Enum.IsDefined(typeof(TEnum), intValue))
            return (TEnum)Enum.ToObject(typeof(TEnum), intValue);

        return Enum.TryParse<TEnum>(value, ignoreCase: true, out var parsed)
            ? parsed
            : @default;
    }
}
