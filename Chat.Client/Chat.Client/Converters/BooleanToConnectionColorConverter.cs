﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using Chat.Client.Styles;

namespace Chat.Client.Converters
{
    [ValueConversion(typeof(bool), typeof(SolidColorBrush))]
    public class BooleanToConnectionColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value), "Cannot convert boolean to ConnectionColor. Given value is null.");

            var boolean = (bool) value;
            if (boolean)
                return ProgramColors.AccentLightColor3;
            return new SolidColorBrush(Color.FromRgb(128, 0, 0));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
