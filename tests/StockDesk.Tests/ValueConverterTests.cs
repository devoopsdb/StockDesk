using System;
using System.Globalization;
using System.Windows;
using StockDesk.Common.Converters;
using Xunit;

namespace StockDesk.Tests;

public class ValueConverterTests
{
    [Fact]
    public void NullToVisibilityConverter_WithoutInvert_ReturnsExpectedVisibility()
    {
        var converter = new NullToVisibilityConverter();

        Assert.Equal(Visibility.Collapsed, converter.Convert(null, typeof(Visibility), null, CultureInfo.InvariantCulture));
        Assert.Equal(Visibility.Collapsed, converter.Convert("", typeof(Visibility), null, CultureInfo.InvariantCulture));
        Assert.Equal(Visibility.Collapsed, converter.Convert("   ", typeof(Visibility), null, CultureInfo.InvariantCulture));
        Assert.Equal(Visibility.Visible, converter.Convert("test", typeof(Visibility), null, CultureInfo.InvariantCulture));
        Assert.Equal(Visibility.Visible, converter.Convert(new object(), typeof(Visibility), null, CultureInfo.InvariantCulture));
    }

    [Fact]
    public void NullToVisibilityConverter_WithInvertProperty_ReturnsInvertedVisibility()
    {
        var converter = new NullToVisibilityConverter { Invert = true };

        Assert.Equal(Visibility.Visible, converter.Convert(null, typeof(Visibility), null, CultureInfo.InvariantCulture));
        Assert.Equal(Visibility.Visible, converter.Convert("", typeof(Visibility), null, CultureInfo.InvariantCulture));
        Assert.Equal(Visibility.Collapsed, converter.Convert("test", typeof(Visibility), null, CultureInfo.InvariantCulture));
    }

    [Theory]
    [InlineData("invert")]
    [InlineData("Invert")]
    [InlineData("INVERT")]
    public void NullToVisibilityConverter_WithConverterParameterInvert_ReturnsInvertedVisibility(string parameter)
    {
        var converter = new NullToVisibilityConverter();

        Assert.Equal(Visibility.Visible, converter.Convert(null, typeof(Visibility), parameter, CultureInfo.InvariantCulture));
        Assert.Equal(Visibility.Visible, converter.Convert("", typeof(Visibility), parameter, CultureInfo.InvariantCulture));
        Assert.Equal(Visibility.Visible, converter.Convert("   ", typeof(Visibility), parameter, CultureInfo.InvariantCulture));
        Assert.Equal(Visibility.Collapsed, converter.Convert("valid_image.png", typeof(Visibility), parameter, CultureInfo.InvariantCulture));
        Assert.Equal(Visibility.Collapsed, converter.Convert(new object(), typeof(Visibility), parameter, CultureInfo.InvariantCulture));
    }
}
