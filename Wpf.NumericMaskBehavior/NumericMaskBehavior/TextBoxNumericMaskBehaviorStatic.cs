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

    using Wpf.NumericMaskBehavior;

    public static class TextBoxNumericMaskBehaviorStatic
    {
        #region Behavior Properties

        #region MinimumValue Property

        public static decimal GetMinimumValue(DependencyObject dependencyObject)
        {
            return (decimal)dependencyObject.GetValue(MinimumValueProperty);
        }

        public static void SetMinimumValue(DependencyObject dependencyObject, decimal value)
        {
            dependencyObject.SetValue(MinimumValueProperty, value);
        }

        public static readonly DependencyProperty MinimumValueProperty;

        #endregion

        #region MaximumValue Property

        public static decimal GetMaximumValue(DependencyObject dependencyObject)
        {
            return (decimal)dependencyObject.GetValue(MaximumValueProperty);
        }

        public static void SetMaximumValue(DependencyObject dependencyObject, decimal value)
        {
            dependencyObject.SetValue(MaximumValueProperty, value);
        }

        public static readonly DependencyProperty MaximumValueProperty;

        #endregion

        #region DecimalPlaces Property

        public static byte GetDecimalPlaces(DependencyObject dependencyObject)
        {
            return (byte)dependencyObject.GetValue(DecimalPlacesProperty);
        }

        public static void SetDecimalPlaces(DependencyObject dependencyObject, byte value)
        {
            NumericMaskBehaviorCore.ValidateDecimalPlaces(value);
            dependencyObject.SetValue(DecimalPlacesProperty, value);
        }

        public static readonly DependencyProperty DecimalPlacesProperty;

        #endregion

        #region IntegerPlaces Property

        public static byte GetIntegerPlaces(DependencyObject dependencyObject)
        {
            return (byte)dependencyObject.GetValue(IntegerPlacesProperty);
        }

        public static void SetIntegerPlaces(DependencyObject dependencyObject, byte value)
        {
            NumericMaskBehaviorCore.ValidateIntegerPlaces(value);
            dependencyObject.SetValue(IntegerPlacesProperty, value);
        }

        public static readonly DependencyProperty IntegerPlacesProperty;

        #endregion

        #region Mask Property

        public static MaskTypes GetMask(DependencyObject dependencyObject)
        {
            return (MaskTypes)dependencyObject.GetValue(MaskProperty);
        }

        public static void SetMask(DependencyObject dependencyObject, MaskTypes value)
        {
            dependencyObject.SetValue(MaskProperty, value);
        }

        public static readonly DependencyProperty MaskProperty;

        #endregion

        #region Culture Property

        public static CultureInfos GetCulture(DependencyObject dependencyObject)
        {
            return (CultureInfos)dependencyObject.GetValue(CultureProperty);
        }

        public static CultureInfo GetCultureInfo(DependencyObject dependencyObject)
        {
            if (dependencyObject == null) throw new ArgumentException(nameof(dependencyObject));
            var value = GetCulture(dependencyObject);

            return NumericMaskBehaviorCore.ResolveCultureInfo(value);
        }

        public static void SetCulture(DependencyObject dependencyObject, CultureInfos value)
        {
            dependencyObject.SetValue(CultureProperty, value);
        }

        public static readonly DependencyProperty CultureProperty;

        #endregion

        #region NumberFormatInfo Property

        public static NumberFormatInfos GetNumberFormat(DependencyObject dependencyObject)
        {
            return (NumberFormatInfos)dependencyObject.GetValue(NumberFormatInfoProperty);
        }

        public static NumberFormatInfo GetNumberFormatInfo(DependencyObject dependencyObject)
        {
            if (dependencyObject == null) throw new ArgumentException(nameof(dependencyObject));
            var value = GetNumberFormat(dependencyObject);

            return NumericMaskBehaviorCore.ResolveNumberFormatInfo(value);
        }

        public static void SetNumberFormatInfo(DependencyObject dependencyObject, NumberFormatInfos value)
        {
            dependencyObject.SetValue(NumberFormatInfoProperty, value);
        }

        public static readonly DependencyProperty NumberFormatInfoProperty;

        #endregion

        #endregion

        #region Property callbacks

        private static void ValidateControl(TextBox _this)
        {
            var decimalPlaces = GetDecimalPlaces(_this);
            NumberFormatInfo numberFormatInfo = GetNumberFormatInfo(_this);
            CultureInfo cultureInfo = GetCultureInfo(_this);
            decimal minimumValue = GetMinimumValue(_this);
            decimal maximumValue = GetMaximumValue(_this);
            var maskType = GetMask(_this);

            NumericMaskBehaviorCore.ValidateTextBox(
                _this,
                numberFormatInfo,
                cultureInfo,
                maskType,
                minimumValue,
                maximumValue,
                decimalPlaces);
        }

        private static void MinimumValueChangedCallback(
            DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs e)
        {
            TextBox _this = dependencyObject as TextBox;
            ValidateControl(_this);
        }

        private static void MaximumValueChangedCallback(
            DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs e)
        {
            TextBox _this = dependencyObject as TextBox;
            ValidateControl(_this);
        }

        private static void DecimalPlacesChangedCallback(
            DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs e)
        {
            TextBox _this = dependencyObject as TextBox;
            ValidateControl(_this);
        }

        private static void IntegerPlacesChangedCallback(
            DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs e)
        {
            TextBox _this = dependencyObject as TextBox;
            ValidateControl(_this);
        }

        private static void MaskChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var box = e.OldValue as TextBox;

            if (box != null)
            {
                box.PreviewTextInput -= TextBox_PreviewTextInput;
                box.PreviewKeyDown -= TextBox_PreviewKeyDown;
                DataObject.RemovePastingHandler(box, TextBoxPastingEventHandler);
            }

            TextBox _this = dependencyObject as TextBox;
            if (_this == null) return;

            if ((MaskTypes)e.NewValue != MaskTypes.Any)
            {
                _this.PreviewTextInput += TextBox_PreviewTextInput;
                _this.PreviewKeyDown += TextBox_PreviewKeyDown;
                DataObject.AddPastingHandler(_this, TextBoxPastingEventHandler);
            }

            ValidateControl(_this);
        }

        private static void CultureChangedCallback(
            DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs e)
        {
            TextBox _this = dependencyObject as TextBox;
            ValidateControl(_this);
        }

        private static void NumberFormatChangedCallback(
            DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs e)
        {
            TextBox _this = dependencyObject as TextBox;
            ValidateControl(_this);
        }

        #endregion

        static TextBoxNumericMaskBehaviorStatic()
        {
            #region Property registration

            MaskProperty = DependencyProperty.RegisterAttached(
                "Mask",
                typeof(MaskTypes),
                typeof(TextBoxNumericMaskBehaviorStatic),
                new FrameworkPropertyMetadata(MaskChangedCallback));

            MaximumValueProperty = DependencyProperty.RegisterAttached(
                "MaximumValue",
                typeof(decimal),
                typeof(TextBoxNumericMaskBehaviorStatic),
                new FrameworkPropertyMetadata(decimal.MaxValue, MaximumValueChangedCallback));

            MinimumValueProperty = DependencyProperty.RegisterAttached(
                "MinimumValue",
                typeof(decimal),
                typeof(TextBoxNumericMaskBehaviorStatic),
                new FrameworkPropertyMetadata(decimal.MinValue, MinimumValueChangedCallback));

            DecimalPlacesProperty = DependencyProperty.RegisterAttached(
                "DecimalPlaces",
                typeof(byte),
                typeof(TextBoxNumericMaskBehaviorStatic),
                new FrameworkPropertyMetadata(NumericMaskBehaviorCore.MAX_DECIMAL_PLACES, DecimalPlacesChangedCallback));

            IntegerPlacesProperty = DependencyProperty.RegisterAttached(
                "IntegerPlaces",
                typeof(byte),
                typeof(TextBoxNumericMaskBehaviorStatic),
                new FrameworkPropertyMetadata(NumericMaskBehaviorCore.MAX_INTEGER_PLACES, IntegerPlacesChangedCallback));

            CultureProperty = DependencyProperty.RegisterAttached(
                "Culture",
                typeof(CultureInfos),
                typeof(TextBoxNumericMaskBehaviorStatic),
                new FrameworkPropertyMetadata(CultureInfos.InvariantCulture, CultureChangedCallback));

            NumberFormatInfoProperty = DependencyProperty.RegisterAttached(
                "NumberFormatInfo",
                typeof(NumberFormatInfos),
                typeof(TextBoxNumericMaskBehaviorStatic),
                new FrameworkPropertyMetadata(NumberFormatInfos.InvariantInfo, NumberFormatChangedCallback));

            #endregion
        }

        #region Event handlers

        private static void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox _this = sender as TextBox;
            var decimalPlaces = GetDecimalPlaces(_this);
            var integerPlaces = GetIntegerPlaces(_this);
            NumberFormatInfo numberFormatInfo = GetNumberFormatInfo(_this);
            CultureInfo cultureInfo = GetCultureInfo(_this);
            var maskType = GetMask(_this);

            NumericMaskBehaviorCore.PreviewTextInputCallBack(
                sender,
                e,
                numberFormatInfo,
                cultureInfo,
                maskType,
                decimalPlaces,
                integerPlaces);
        }

        private static void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            TextBox _this = sender as TextBox;
            var integerPlaces = GetIntegerPlaces(_this);
            NumberFormatInfo numberFormatInfo = GetNumberFormatInfo(_this);

            NumericMaskBehaviorCore.PreviewKeyDownCallBack(sender, e, numberFormatInfo, integerPlaces);
        }

        private static void TextBoxPastingEventHandler(object sender, DataObjectPastingEventArgs e)
        {
            TextBox _this = sender as TextBox;
            var decimalPlaces = GetDecimalPlaces(_this);
            NumberFormatInfo numberFormatInfo = GetNumberFormatInfo(_this);
            CultureInfo cultureInfo = GetCultureInfo(_this);
            decimal minimumValue = GetMinimumValue(_this);
            decimal maximumValue = GetMaximumValue(_this);
            var maskType = GetMask(_this);
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