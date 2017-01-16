/* lggomez, 2016
 * Original copyright notice:
 * 
 * Copyright(c) 2009, Rubenhak
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *
 *   -> Redistributions of source code must retain the above copyright
 *      notice, this list of conditions and the following disclaimer.
 * 
 *   -> Redistributions in binary form must reproduce the above copyright
 *      notice, this list of conditions and the following disclaimer in the
 *      documentation and/or other materials provided with the distribution.
 */

// ReSharper disable SuspiciousTypeConversion.Global
// ReSharper disable once CheckNamespace

namespace Lggomez.Common.WPF
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Interactivity;

    using Wpf.NumericMaskBehavior;

    #region Documentation Tags

    /// <summary>
    ///     WPF Maskable TextBox class. Just specify the TextBoxNumericMaskBehavior.Mask attached property to a TextBox. 
    ///     It protects your TextBox from unwanted non numeric symbols and make it easy to modify your numbers.
    /// </summary>
    /// <remarks>
    /// <para>
    ///     Class Information:
    ///	    <list type="bullet">
    ///         <item name="authors">Authors: Ruben Hakopian</item>
    ///         <item name="date">February 2009</item>
    ///         <item name="originalURL">http://www.rubenhak.com/?p=8</item>
    ///     </list>
    ///	    <list type="bullet">
    ///         <item name="authors">Authors: Luis Gomez</item>
    ///         <item name="date">May 2016</item>
    ///         <remarks>
    ///             -Fixed support for negative values and rewrote the 
    ///               masking code
    ///             -Changed value types to decimal (for consistent precision)
    ///             -Added DecimalPlaces option
    ///             -Added Culture and NumberFormat options
    ///         </remarks>
    ///     </list>
    /// </para>
    /// </remarks>

    #endregion
    public sealed class TextBoxNumericMaskBehavior : Behavior<TextBox>
    {
        public static TextBoxNumericMaskBehavior Instance { get; } = new TextBoxNumericMaskBehavior();

        #region Behavior Properties

        #region MinimumValue Property

        public decimal GetMinimumValue(TextBox dependencyObject)
        {
            return (decimal)dependencyObject.GetValue(this.MinimumValueProperty);
        }

        public TextBoxNumericMaskBehavior WithMinimumValueFor(TextBox dependencyObject, decimal value)
        {
            dependencyObject.SetValue(this.MinimumValueProperty, value);
            return Instance;
        }

        private readonly DependencyProperty MinimumValueProperty;

        #endregion

        #region MaximumValue Property

        public decimal GetMaximumValue(TextBox dependencyObject)
        {
            return (decimal)dependencyObject.GetValue(this.MaximumValueProperty);
        }

        public TextBoxNumericMaskBehavior WithMaximumValueFor(TextBox dependencyObject, decimal value)
        {
            dependencyObject.SetValue(this.MaximumValueProperty, value);
            return Instance;
        }

        private readonly DependencyProperty MaximumValueProperty;

        #endregion

        #region DecimalPlaces Property

        public byte GetDecimalPlaces(TextBox dependencyObject)
        {
            return (byte)dependencyObject.GetValue(this.DecimalPlacesProperty);
        }

        public TextBoxNumericMaskBehavior WithDecimalPlacesFor(TextBox dependencyObject, byte value)
        {
            NumericMaskBehaviorCore.ValidateDecimalPlaces(value);
            dependencyObject.SetValue(this.DecimalPlacesProperty, value);
            return Instance;
        }

        private readonly DependencyProperty DecimalPlacesProperty;

        #endregion

        #region IntegerPlaces Property

        public byte GetIntegerPlaces(TextBox dependencyObject)
        {
            return (byte)dependencyObject.GetValue(IntegerPlacesProperty);
        }

        public TextBoxNumericMaskBehavior WithIntegerPlacesFor(TextBox attachedTextBox, byte value)
        {
            NumericMaskBehaviorCore.ValidateIntegerPlaces(value);
            attachedTextBox.SetValue(IntegerPlacesProperty, value);
            return Instance;
        }

        private static DependencyProperty IntegerPlacesProperty;

        #endregion

        #region Mask Property

        public MaskTypes GetMask(TextBox dependencyObject)
        {
            return (MaskTypes)dependencyObject.GetValue(this.MaskProperty);
        }

        public TextBoxNumericMaskBehavior SetMaskTo(TextBox dependencyObject, MaskTypes value)
        {
            dependencyObject.SetValue(this.MaskProperty, value);
            return Instance;
        }

        private readonly DependencyProperty MaskProperty;

        #endregion

        #region Culture Property

        public CultureInfo GetCultureInfo(DependencyObject dependencyObject)
        {
            if (dependencyObject == null) throw new ArgumentException(nameof(dependencyObject));
            var value = this.GetCulture(dependencyObject as TextBox);

            return NumericMaskBehaviorCore.ResolveCultureInfo(value);
        }

        public CultureInfos GetCulture(TextBox dependencyObject)
        {
            return (CultureInfos)dependencyObject.GetValue(this.CultureProperty);
        }

        public TextBoxNumericMaskBehavior WithCultureFor(TextBox dependencyObject, CultureInfos value)
        {
            dependencyObject.SetValue(this.CultureProperty, value);
            return Instance;
        }

        private readonly DependencyProperty CultureProperty;

        #endregion

        #region NumberFormatInfo Property

        public NumberFormatInfos GetNumberFormat(DependencyObject dependencyObject)
        {
            return (NumberFormatInfos)dependencyObject.GetValue(this.NumberFormatInfoProperty);
        }

        public NumberFormatInfo GetNumberFormatInfo(DependencyObject dependencyObject)
        {
            if (dependencyObject == null) throw new ArgumentException(nameof(dependencyObject));
            var value = this.GetNumberFormat(dependencyObject);

            return NumericMaskBehaviorCore.ResolveNumberFormatInfo(value);
        }

        public TextBoxNumericMaskBehavior SetNumberFormatInfo(DependencyObject dependencyObject, NumberFormatInfos value)
        {
            dependencyObject.SetValue(this.NumberFormatInfoProperty, value);
            return Instance;
        }

        public readonly DependencyProperty NumberFormatInfoProperty;

        #endregion

        #endregion

        #region Property callbacks

        private void ValidateControl(TextBox _this)
        {
            var decimalPlaces = this.GetDecimalPlaces(_this);
            NumberFormatInfo numberFormatInfo = this.GetNumberFormatInfo(_this);
            CultureInfo cultureInfo = this.GetCultureInfo(_this);
            decimal minimumValue = this.GetMinimumValue(_this);
            decimal maximumValue = this.GetMaximumValue(_this);
            var maskType = this.GetMask(_this);

            NumericMaskBehaviorCore.ValidateTextBox(
                _this,
                numberFormatInfo,
                cultureInfo,
                maskType,
                minimumValue,
                maximumValue,
                decimalPlaces);
        }

        private void MinimumValueChangedCallback(
            DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs e)
        {
            TextBox _this = dependencyObject as TextBox;
            this.ValidateControl(_this);
        }

        private void MaximumValueChangedCallback(
            DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs e)
        {
            TextBox _this = dependencyObject as TextBox;
            this.ValidateControl(_this);
        }

        private void DecimalPlacesChangedCallback(
            DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs e)
        {
            TextBox _this = dependencyObject as TextBox;
            this.ValidateControl(_this);
        }

        private void IntegerPlacesChangedCallback(
            DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs e)
        {
            TextBox _this = dependencyObject as TextBox;
            this.ValidateControl(_this);
        }

        private void MaskChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var box = e.OldValue as TextBox;

            if (box != null)
            {
                box.PreviewTextInput -= this.TextBox_PreviewTextInput;
                box.PreviewKeyDown -= this.TextBox_PreviewKeyDown;
                DataObject.RemovePastingHandler(box, this.TextBoxPastingEventHandler);
            }

            TextBox _this = dependencyObject as TextBox;
            if (_this == null) return;

            if ((MaskTypes)e.NewValue != MaskTypes.Any)
            {
                _this.PreviewTextInput += this.TextBox_PreviewTextInput;
                _this.PreviewKeyDown += this.TextBox_PreviewKeyDown;
                DataObject.AddPastingHandler(_this, this.TextBoxPastingEventHandler);
            }

            this.ValidateControl(_this);
        }

        private void CultureChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            TextBox _this = dependencyObject as TextBox;
            this.ValidateControl(_this);
        }

        private void NumberFormatChangedCallback(
            DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs e)
        {
            TextBox _this = dependencyObject as TextBox;
            this.ValidateControl(_this);
        }

        #endregion

        private TextBoxNumericMaskBehavior()
        {
            try
            {
                #region Property registration

                this.MaskProperty = DependencyProperty.RegisterAttached(
                    "Mask",
                    typeof(MaskTypes),
                    typeof(TextBoxNumericMaskBehavior),
                    new FrameworkPropertyMetadata(this.MaskChangedCallback));

                this.MaximumValueProperty = DependencyProperty.RegisterAttached(
                    "MaximumValue",
                    typeof(decimal),
                    typeof(TextBoxNumericMaskBehavior),
                    new FrameworkPropertyMetadata(decimal.MaxValue, this.MaximumValueChangedCallback));

                this.MinimumValueProperty = DependencyProperty.RegisterAttached(
                    "MinimumValue",
                    typeof(decimal),
                    typeof(TextBoxNumericMaskBehavior),
                    new FrameworkPropertyMetadata(decimal.MinValue, this.MinimumValueChangedCallback));

                this.DecimalPlacesProperty = DependencyProperty.RegisterAttached(
                    "DecimalPlaces",
                    typeof(byte),
                    typeof(TextBoxNumericMaskBehavior),
                    new FrameworkPropertyMetadata(
                        NumericMaskBehaviorCore.MAX_DECIMAL_PLACES,
                        this.DecimalPlacesChangedCallback));

                IntegerPlacesProperty = DependencyProperty.RegisterAttached(
                    "IntegerPlaces",
                    typeof(byte),
                    typeof(TextBoxNumericMaskBehavior),
                    new FrameworkPropertyMetadata(
                        NumericMaskBehaviorCore.MAX_INTEGER_PLACES,
                        this.IntegerPlacesChangedCallback));

                this.CultureProperty = DependencyProperty.RegisterAttached(
                    "Culture",
                    typeof(CultureInfos),
                    typeof(TextBoxNumericMaskBehavior),
                    new FrameworkPropertyMetadata(CultureInfos.InvariantCulture, this.CultureChangedCallback));

                this.NumberFormatInfoProperty = DependencyProperty.RegisterAttached(
                    "NumberFormatInfo",
                    typeof(NumberFormatInfos),
                    typeof(TextBoxNumericMaskBehavior),
                    new FrameworkPropertyMetadata(NumberFormatInfos.InvariantInfo, this.NumberFormatChangedCallback));

                #endregion
            }
            catch
            {
                // ignored
            }
        }

        #region Event handlers

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox _this = sender as TextBox;
            var decimalPlaces = this.GetDecimalPlaces(_this);
            var integerPlaces = this.GetIntegerPlaces(_this);
            NumberFormatInfo numberFormatInfo = this.GetNumberFormatInfo(_this);
            CultureInfo cultureInfo = this.GetCultureInfo(_this);
            var maskType = this.GetMask(_this);

            NumericMaskBehaviorCore.PreviewTextInputCallBack(
                sender,
                e,
                numberFormatInfo,
                cultureInfo,
                maskType,
                decimalPlaces,
                integerPlaces);
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            TextBox _this = sender as TextBox;
            var integerPlaces = this.GetIntegerPlaces(_this);
            NumberFormatInfo numberFormatInfo = this.GetNumberFormatInfo(_this);

            NumericMaskBehaviorCore.PreviewKeyDownCallBack(sender, e, numberFormatInfo, integerPlaces);
        }

        private void TextBoxPastingEventHandler(object sender, DataObjectPastingEventArgs e)
        {
            TextBox _this = sender as TextBox;
            var decimalPlaces = this.GetDecimalPlaces(_this);
            NumberFormatInfo numberFormatInfo = this.GetNumberFormatInfo(_this);
            CultureInfo cultureInfo = this.GetCultureInfo(_this);
            decimal minimumValue = this.GetMinimumValue(_this);
            decimal maximumValue = this.GetMaximumValue(_this);
            var maskType = this.GetMask(_this);
            string clipboard = e.DataObject.GetData(typeof(string)) as string;
            clipboard = NumericMaskBehaviorCore.ValidateValue(
                _this,
                clipboard,
                numberFormatInfo,
                cultureInfo,
                maskType,
                minimumValue,
                maximumValue,
                decimalPlaces);

            if (!string.IsNullOrEmpty(clipboard))
            {
                _this.Text = clipboard;
            }

            e.CancelCommand();
            e.Handled = true;
        }

        #endregion
    }
}