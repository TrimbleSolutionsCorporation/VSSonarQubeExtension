// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FallbackForegroundBrush.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
//     Copyright (C) 2013 [Jorge Costa, Jorge.Costa@tekla.com]
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
// This program is free software; you can redistribute it and/or modify it under the terms of the GNU Lesser General Public License
// as published by the Free Software Foundation; either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
// of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details. 
// You should have received a copy of the GNU Lesser General Public License along with this program; if not, write to the Free
// Software Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// --------------------------------------------------------------------------------------------------------------------
namespace VSSonarExtensionUi.Styles
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Markup;
    using System.Windows.Media;

    using Microsoft.VisualStudio.Shell;

    using VSSonarExtensionUi.ViewModel;

    /// <summary>
    ///     The fallback brush.
    /// </summary>
    [MarkupExtensionReturnType(typeof(object))]
    public class FallbackForegroundBrush : MarkupExtension
    {
        #region Fields

        public FallbackForegroundBrush()
        {
            this.FallBackBrush = Brushes.Black;
        }

        /// <summary>
        /// The _base object.
        /// </summary>
        private DependencyObject _baseObject;

        /// <summary>
        /// The _base property.
        /// </summary>
        private DependencyProperty _baseProperty;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the secondary brush.
        /// </summary>
        public Brush FallBackBrush { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The provide value.
        /// </summary>
        /// <param name="serviceProvider">
        /// The service provider.
        /// </param>
        /// <returns>
        /// The <see cref="object"/>.
        /// </returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var valueProvider = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
            if (valueProvider != null)
            {
                this._baseProperty = valueProvider.TargetProperty as DependencyProperty;
                this._baseObject = valueProvider.TargetObject as DependencyObject;
            }

            var brushBinding = new Binding { Converter = new FallbackBrushConverter(this) };
            return brushBinding.ProvideValue(serviceProvider);
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Refreshes the target property with an updated brush determined by UseSecondaryBrush abstract method.
        /// </summary>
        protected void RefreshValue()
        {
            if (this._baseObject != null && this._baseProperty != null)
            {
                BindingExpressionBase bindingExpression = BindingOperations.GetBindingExpressionBase(this._baseObject, this._baseProperty);
                if (bindingExpression != null)
                {
                    bindingExpression.UpdateTarget();
                }
            }
        }

        #endregion

        /// <summary>
        /// The fallback brush converter.
        /// </summary>
        internal class FallbackBrushConverter : IValueConverter
        {
            #region Fields

            /// <summary>
            /// The _fallback brush.
            /// </summary>
            private readonly FallbackForegroundBrush _fallbackBrush;

            #endregion

            #region Constructors and Destructors

            /// <summary>
            /// Initializes a new instance of the <see cref="FallbackBrushConverter"/> class.
            /// </summary>
            /// <param name="extension">
            /// The extension.
            /// </param>
            public FallbackBrushConverter(FallbackForegroundBrush extension)
            {
                this._fallbackBrush = extension;
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
                var model = value as SonarQubeViewModel;

                if (model != null)
                {
                    return model.IsRunningInVisualStudio() ? VsBrushes.ToolWindowBackgroundKey : this._fallbackBrush.FallBackBrush;
                }
                else
                {
                    return this._fallbackBrush.FallBackBrush;
                }               
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
            /// <exception cref="NotImplementedException">
            /// </exception>
            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }

            #endregion
        }
    }
}