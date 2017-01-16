namespace Wpf.NumericMaskBehavior
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    using Lggomez.Common.WPF;

    internal static class NumericMaskBehaviorCore
    {
        //Constants
        internal const byte MAX_DECIMAL_PLACES = 29; //Following decimal type significant digits

        internal const byte MAX_INTEGER_PLACES = 29; //Following decimal type significant digits

        #region Event handlers and masking

        internal static void TextBoxPastingEventHandler(
            object sender,
            DataObjectPastingEventArgs e,
            NumberFormatInfo numberFormatInfo,
            CultureInfo cultureInfo,
            MaskTypes maskType,
            decimal maximumValue,
            decimal minimumValue,
            byte decimalPlaces)
        {
            TextBox _this = sender as TextBox;
            string clipboard = e.DataObject.GetData(typeof(string)) as string;
            clipboard = ValidateValue(
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

        internal static void PreviewKeyDownCallBack(
            object sender,
            KeyEventArgs e,
            NumberFormatInfo numberFormatInfo,
            byte integerPlaces)
        {
            if (e.Key.Equals(Key.Back) || e.Key.Equals(Key.Delete))
            {
                TextBox _this = sender as TextBox;
                int caretIndex = _this?.CaretIndex ?? 0;
                string inputText = _this?.Text ?? string.Empty;
                int selectionLength = _this?.SelectionLength ?? 0;

                if (caretIndex > 0)
                {
                    if (selectionLength == 0)
                    {
                        var elementToDelete = inputText.ElementAt(caretIndex - 1).ToString();

                        if (elementToDelete.Equals(numberFormatInfo.NumberDecimalSeparator))
                        {
                            _this.Text = inputText.Remove(inputText.IndexOf(elementToDelete, StringComparison.Ordinal));
                            _this.CaretIndex = caretIndex - 1;
                            bool isNegative = inputText.StartsWith(numberFormatInfo.NegativeSign);

                            if (inputText.Length - (isNegative ? 1 : 0) >= integerPlaces)
                            {
                                _this.Text = inputText.Substring(0, integerPlaces);
                                _this.CaretIndex = inputText.Length;
                            }

                            e.Handled = true;
                        }
                    }
                    else
                    {
                        int selectionStart = _this?.SelectionStart ?? 0;
                        var elementToDelete = inputText.Substring(selectionStart, selectionLength);

                        if (elementToDelete.Contains(numberFormatInfo.NumberDecimalSeparator))
                        {
                            var remainderPlaces = inputText.Length - (selectionStart + selectionLength);
                            _this.Text = inputText.Substring(0, selectionStart)
                                         + inputText.Substring(
                                             selectionStart + selectionLength,
                                             remainderPlaces >= integerPlaces ? integerPlaces : remainderPlaces);
                            _this.CaretIndex = selectionStart;

                            bool isNegative = inputText.StartsWith(numberFormatInfo.NegativeSign);

                            if (_this.Text.Length - (isNegative ? 1 : 0) > integerPlaces)
                            {
                                _this.Text = _this.Text.Substring(0, integerPlaces);
                                _this.CaretIndex = inputText.Length;
                            }

                            e.Handled = true;
                        }
                    }
                }
            }
        }

        internal static void PreviewTextInputCallBack(
            object sender,
            TextCompositionEventArgs e,
            NumberFormatInfo numberFormatInfo,
            CultureInfo cultureInfo,
            MaskTypes maskType,
            byte decimalPlaces,
            byte integerPlaces)
        {
            TextBox _this = sender as TextBox;
            int caretIndex = _this?.CaretIndex ?? 0;
            string inputText = _this?.Text ?? string.Empty;
            string eventText = e.Text ?? string.Empty;
            int selectionLength = _this?.SelectionLength ?? 0;
            bool isValid = IsSymbolValid(maskType, e.Text, numberFormatInfo);
            bool isTextInserted = eventText.Length > 1;
            bool isNegative = inputText.StartsWith(numberFormatInfo.NegativeSign)
                              || eventText.StartsWith(numberFormatInfo.NegativeSign);
            var separatorIndex = inputText.IndexOf(numberFormatInfo.NumberDecimalSeparator, StringComparison.Ordinal);

            decimal output;

            //Invalid input
            if (!isValid || (isTextInserted && (eventText.Length != selectionLength)))
            {
                if (isTextInserted)
                {
                    _this.CaretIndex = inputText.Length;
                    _this.SelectionLength = 0;
                }

                e.Handled = true;
                return;
            }

            //Allow only one decimal separator
            if (eventText.Contains(numberFormatInfo.NumberDecimalSeparator)
                && inputText.Contains(numberFormatInfo.NumberDecimalSeparator))
            {
                e.Handled = true;
                return;
            }

            //Negative sign check
            if (eventText.Contains(numberFormatInfo.NegativeSign) && inputText.StartsWith(numberFormatInfo.NegativeSign))
            {
                e.Handled = true;
                return;
            }

            //Restrict decimal places to DecimalPlaces option
            if (inputText.Contains(numberFormatInfo.NumberDecimalSeparator))
            {
                if ((inputText.Length - separatorIndex > decimalPlaces) && (separatorIndex < caretIndex))
                {
                    e.Handled = true;
                    return;
                }
            }

            //Negative sign handling
            if (eventText.Equals(numberFormatInfo.NegativeSign))
            {
                if (string.IsNullOrEmpty(inputText) || (selectionLength == inputText.Length))
                {
                    _this.Text = eventText;
                    _this.CaretIndex++;
                }
                else
                {
                    if ((_this.SelectionStart == 0) && (selectionLength > 0))
                    {
                        _this.Text = inputText.Replace(
                            inputText.Substring(_this.SelectionStart, _this.SelectionLength),
                            eventText);
                        _this.CaretIndex = _this.SelectionStart + eventText.Length;
                    }

                    if ((caretIndex == 0) && !inputText.StartsWith(numberFormatInfo.NegativeSign))
                    {
                        _this.Text = eventText + inputText;
                        _this.CaretIndex++;
                    }
                }

                e.Handled = true;
                return;
            }

            //If first digit is 0 or dec separator, turn to decimal part (include negative signs)
            if ((eventText.Equals("0") || eventText.Equals(numberFormatInfo.NumberDecimalSeparator))
                && (string.IsNullOrEmpty(inputText) || (inputText.Length == selectionLength)
                    || inputText.Equals(numberFormatInfo.NegativeSign)))
            {
                if (inputText.Equals(numberFormatInfo.NegativeSign) && (selectionLength == 0))
                {
                    _this.Text = numberFormatInfo.NegativeSign;
                }
                else
                {
                    _this.Text = string.Empty;
                }

                _this.Text += "0" + numberFormatInfo.NumberDecimalSeparator;
                _this.CaretIndex = _this.Text.Length;
                e.Handled = true;
                return;
            }

            //Character selection handling
            if (selectionLength > 0)
            {
                if (eventText.Equals(numberFormatInfo.NegativeSign) && (_this.SelectionLength == inputText.Length))
                {
                    _this.Text = numberFormatInfo.NegativeSign;
                    _this.CaretIndex = 1;
                    e.Handled = true;
                    return;
                }

                inputText = inputText.Replace(
                    inputText.Substring(_this.SelectionStart, _this.SelectionLength),
                    eventText);
                caretIndex = _this.SelectionStart + eventText.Length;

                //For insertions (multi-digit input)
                if (isTextInserted && (eventText.Length != selectionLength))
                {
                    inputText = eventText;
                    caretIndex = _this.SelectionStart + eventText.Length;
                }

                if (decimal.TryParse(inputText, out output))
                {
                    _this.Text = inputText;
                    _this.CaretIndex = caretIndex;
                    _this.SelectionStart = caretIndex;
                    _this.SelectionLength = 0;
                    e.Handled = true;
                    return;
                }
            }

            //Single digit handling
            if (caretIndex != inputText.Length)
            {
                //Truncate decimals to IntegerPlaces
                if (!inputText.Contains(numberFormatInfo.NumberDecimalSeparator)
                    && !eventText.Equals(numberFormatInfo.NumberDecimalSeparator))
                {
                    if (inputText.Length >= integerPlaces)
                    {
                        e.Handled = true;
                        return;
                    }
                }
                else
                {
                    if ((separatorIndex > 0) && !IsCaretInDecimalPart(separatorIndex, caretIndex)
                        && (inputText.Substring(0, separatorIndex - 1).Length - (isNegative ? 1 : 0) >= integerPlaces))
                    {
                        e.Handled = true;
                        return;
                    }
                }

                var newValue = inputText.Insert(caretIndex, eventText);

                if (decimal.TryParse(newValue, out output))
                {
                    inputText = newValue;
                    caretIndex++;

                    //Truncate decimals to DecimalPlaces
                    if (eventText.Equals(numberFormatInfo.NumberDecimalSeparator)
                        && (inputText.Length - separatorIndex > decimalPlaces))
                    {
                        _this.Text = inputText.Substring(
                            0,
                            CalculateDecPartLength(caretIndex, decimalPlaces, inputText));
                        _this.CaretIndex = caretIndex;
                        e.Handled = true;
                        return;
                    }
                }
            }
            else
            {
                if (!inputText.Contains(numberFormatInfo.NumberDecimalSeparator)
                    && !eventText.Equals(numberFormatInfo.NumberDecimalSeparator))
                {
                    if (inputText.Length - (isNegative ? 1 : 0) >= integerPlaces)
                    {
                        e.Handled = true;
                        return;
                    }
                }

                if (decimal.TryParse(inputText + eventText, out output))
                {
                    inputText += eventText;
                    caretIndex++;
                }
            }

            _this.Text = inputText;
            _this.CaretIndex = caretIndex;
            _this.SelectionStart = caretIndex;
            _this.SelectionLength = 0;
            e.Handled = true;
        }

        #endregion

        #region Input validators and helpers

        internal static void ValidateTextBox(
            TextBox _this,
            NumberFormatInfo numberFormatInfo,
            CultureInfo cultureInfo,
            MaskTypes maskType,
            decimal maximumValue,
            decimal minimumValue,
            byte decimalPlaces)
        {
            if (maskType != MaskTypes.Any)
            {
                _this.Text = ValidateValue(
                    _this,
                    _this.Text,
                    numberFormatInfo,
                    cultureInfo,
                    maskType,
                    minimumValue,
                    maximumValue,
                    decimalPlaces);
            }
        }

        internal static string ValidateValue(
            DependencyObject dependencyObject,
            string value,
            NumberFormatInfo numberFormatInfo,
            CultureInfo cultureInfo,
            MaskTypes maskType,
            decimal maximumValue,
            decimal minimumValue,
            byte decimalPlaces)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;

            value = value.Trim();

            switch (maskType)
            {
                case MaskTypes.Integer:
                    {
                        int val;
                        if (int.TryParse(value, out val))
                        {
                            val = (int)ValidateLimits(maximumValue, minimumValue, val);
                            return val.ToString(cultureInfo);
                        }

                        return string.Empty;
                    }

                case MaskTypes.Decimal:
                    {
                        decimal val;
                        if (decimal.TryParse(value, out val))
                        {
                            if (value.Contains(numberFormatInfo.NumberDecimalSeparator)
                                && (value.Length
                                    - value.IndexOf(numberFormatInfo.NumberDecimalSeparator, StringComparison.Ordinal)
                                    > decimalPlaces))
                            {
                                return string.Empty;
                            }

                            val = ValidateLimits(maximumValue, minimumValue, val);
                            return val.ToString(cultureInfo);
                        }

                        return string.Empty;
                    }

                case MaskTypes.Any:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(maskType), maskType, null);
            }

            return value;
        }

        internal static decimal ValidateLimits(decimal min, decimal max, decimal value)
        {
            if (!min.Equals(decimal.MinValue))
            {
                if (value < min) return min;
            }

            if (!max.Equals(decimal.MaxValue))
            {
                if (value > max) return max;
            }

            return value;
        }

        internal static bool IsSymbolValid(MaskTypes mask, string str, NumberFormatInfo numberFormatInfo)
        {
            switch (mask)
            {
                case MaskTypes.Any:
                    return true;
                case MaskTypes.Integer:
                    if (str == numberFormatInfo.NegativeSign) return true;
                    break;
                case MaskTypes.Decimal:
                    if ((str == numberFormatInfo.NumberDecimalSeparator) || (str == numberFormatInfo.NegativeSign)) return true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mask), mask, null);
            }

            if (mask.Equals(MaskTypes.Integer) || mask.Equals(MaskTypes.Decimal))
            {
                foreach (char ch in str)
                {
                    if (!char.IsDigit(ch)) return false;
                }

                return true;
            }

            return false;
        }

        internal static bool IsCaretInDecimalPart(int separatorIndex, int caretIndex)
        {
            if (separatorIndex == -1) return false;
            return caretIndex > separatorIndex;
        }

        internal static int CalculateDecPartLength(int caretIndex, byte decimalPlaces, string inputText)
        {
            return caretIndex + decimalPlaces - (caretIndex + decimalPlaces) % inputText.Length;
        }

        #endregion

        #region Parameter validations

        internal static void ValidateDecimalPlaces(byte value)
        {
            if (value > MAX_DECIMAL_PLACES)
                throw new ArgumentOutOfRangeException(
                          nameof(value),
                          value,
                          $"Value cannot be higher than {MAX_DECIMAL_PLACES}");
        }

        internal static void ValidateIntegerPlaces(byte value)
        {
            if (value > MAX_INTEGER_PLACES)
                throw new ArgumentOutOfRangeException(
                          nameof(value),
                          value,
                          $"Value cannot be higher than {MAX_INTEGER_PLACES}");
        }

        #endregion

        #region Culture resolvers

        internal static CultureInfo ResolveCultureInfo(CultureInfos value)
        {
            if (value.ToString().Equals(CultureInfos.CurrentCulture.ToString()))
            {
                return CultureInfo.CurrentCulture;
            }

            if (value.ToString().Equals(CultureInfos.CurrentUICulture.ToString()))
            {
                return CultureInfo.CurrentUICulture;
            }

            if (value.ToString().Equals(CultureInfos.DefaultThreadCurrentCulture.ToString()))
            {
                return CultureInfo.DefaultThreadCurrentCulture;
            }

            if (value.ToString().Equals(CultureInfos.InstalledUICulture.ToString()))
            {
                return CultureInfo.InstalledUICulture;
            }

            if (value.ToString().Equals(CultureInfos.InvariantCulture.ToString()))
            {
                return CultureInfo.InvariantCulture;
            }

            //Default culture for behavior
            return CultureInfo.InvariantCulture;
        }

        internal static NumberFormatInfo ResolveNumberFormatInfo(NumberFormatInfos value)
        {
            if (value.ToString().Equals(NumberFormatInfos.CurrentInfo.ToString()))
            {
                return NumberFormatInfo.CurrentInfo;
            }

            if (value.ToString().Equals(NumberFormatInfos.InvariantInfo.ToString()))
            {
                return NumberFormatInfo.InvariantInfo;
            }

            //Default formatInfo for behavior
            return NumberFormatInfo.InvariantInfo;
        }

        #endregion
    }
}