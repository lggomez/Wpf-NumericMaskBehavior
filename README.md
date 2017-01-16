# Wpf-NumericMaskBehavior
Quick and dirty numeric mask for WPF TextBox controls

## Usage

### From code behind:

```csharp
TextBoxNumericMaskBehavior.Instance.SetMaskTo(this.ValueTextBox, MaskTypes.Decimal)
    .SetNumberFormatInfo(this.ValueTextBox, NumberFormatInfos.InvariantInfo)
    .WithDecimalPlacesFor(this.ValueTextBox, 3)
    .WithIntegerPlacesFor(this.ValueTextBox, 5)
    .WithMaximumValueFor(this.ValueTextBox, short.MaxValue)
    .WithMinimumValueFor(this.ValueTextBox, short.MinValue)
    .WithCultureFor(this.ValueTextBox, CultureInfos.InvariantCulture);
```

### From xaml defintion:

With wpf namespace being:
```xaml
    xmlns:wpf="clr-namespace:Lggomez.Common.WPF;assembly=Wpf.NumericMaskBehavior"
```

```xaml
    <TextBox x:Name="TotalTextBox"
        wpf:TextBoxNumericMaskBehaviorStatic.Mask="Decimal"
        wpf:TextBoxNumericMaskBehaviorStatic.DecimalPlaces="3"
        wpf:TextBoxNumericMaskBehaviorStatic.IntegerPlaces="5"
        wpf:TextBoxNumericMaskBehaviorStatic.MaximumValue="10000"
        wpf:TextBoxNumericMaskBehaviorStatic.MinimumValue="-10000"
        wpf:TextBoxNumericMaskBehaviorStatic.Culture="InvariantCulture"
        wpf:TextBoxNumericMaskBehaviorStatic.NumberFormat="InvariantInfo"/>
```