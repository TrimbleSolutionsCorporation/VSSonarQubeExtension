// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Class1.cs" company="">
//   
// </copyright>
// <summary>
//   The boolean to visibility converter.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VSSonarExtensionUi.Converters
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    /// <summary>
    /// The boolean to visibility converter.
    /// </summary>
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class ConfigurableBooleanToVisibilityConverter : IValueConverter
    {
        #region Enums

        /// <summary>
        /// The parameters.
        /// </summary>
        private enum Parameters
        {
            /// <summary>
            /// The normal.
            /// </summary>
            Normal, 

            /// <summary>
            /// The inverted.
            /// </summary>
            Inverted
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The convert.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="targetType">
        /// The target type.
        /// </param>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        /// <param name="culture">
        /// The culture.
        /// </param>
        /// <returns>
        /// The <see cref="object"/>.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var boolValue = (bool)value;
            var direction = Parameters.Normal;
            if (parameter != null)
            {
                direction = (Parameters)Enum.Parse(typeof(Parameters), (string)parameter);
            }

            if (direction == Parameters.Inverted)
            {
                return !boolValue ? Visibility.Visible : Visibility.Collapsed;
            }

            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// The convert back.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="targetType">
        /// The target type.
        /// </param>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        /// <param name="culture">
        /// The culture.
        /// </param>
        /// <returns>
        /// The <see cref="object"/>.
        /// </returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        #endregion
    }
}