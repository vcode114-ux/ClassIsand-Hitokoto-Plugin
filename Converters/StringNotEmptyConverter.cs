using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace HitokotoPlugin.Converters;

/// <summary>
/// 将字符串转为 bool：非空非白空格 → true，否则 → false。
/// 用于 XAML 中判断 StatusMessage 是否有内容。
/// </summary>
public class StringNotEmptyConverter : IValueConverter
{
    public static readonly StringNotEmptyConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is string s && !string.IsNullOrWhiteSpace(s);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
