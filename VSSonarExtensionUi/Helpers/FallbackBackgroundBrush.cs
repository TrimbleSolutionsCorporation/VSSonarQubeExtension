// --------------------------------------------------------------------------------------------------------------------
// <copyright company="" file="FallbackBrush.cs">
//   
// </copyright>
// <summary>
//   The fallback brush.
// </summary>
// 
// --------------------------------------------------------------------------------------------------------------------
namespace VSSonarExtensionUi.Helpers
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
    public class FallbackBackgroundBrush : MarkupExtension
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FallbackBackgroundBrush"/> class.
        /// </summary>
        public FallbackBackgroundBrush()
        {
            this.FallBackBrush = Brushes.Aqua;
        }

        #region Fields

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
            private readonly FallbackBackgroundBrush _fallbackBrush;

            #endregion

            #region Constructors and Destructors

            /// <summary>
            /// Initializes a new instance of the <see cref="FallbackBrushConverter"/> class.
            /// </summary>
            /// <param name="extension">
            /// The extension.
            /// </param>
            public FallbackBrushConverter(FallbackBackgroundBrush extension)
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