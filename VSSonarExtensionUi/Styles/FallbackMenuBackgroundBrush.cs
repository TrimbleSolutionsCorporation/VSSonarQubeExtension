// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FallbackMenuBackgroundBrush.cs" company="">
//   
// </copyright>
// <summary>
//   The fallback brush.
// </summary>
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
    public class FallbackMenuBackgroundBrush : MarkupExtension
    {
        #region Fields

        /// <summary>
        ///     The _base object.
        /// </summary>
        private DependencyObject baseObject;

        /// <summary>
        ///     The _base property.
        /// </summary>
        private DependencyProperty baseProperty;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FallbackMenuBackgroundBrush"/> class.
        /// </summary>
        public FallbackMenuBackgroundBrush()
        {
            this.FallBackBrush = Brushes.Black;
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the secondary brush.
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
                this.baseProperty = valueProvider.TargetProperty as DependencyProperty;
                this.baseObject = valueProvider.TargetObject as DependencyObject;
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
            if (this.baseObject != null && this.baseProperty != null)
            {
                BindingExpressionBase bindingExpression = BindingOperations.GetBindingExpressionBase(this.baseObject, this.baseProperty);
                if (bindingExpression != null)
                {
                    bindingExpression.UpdateTarget();
                }
            }
        }

        #endregion

        /// <summary>
        ///     The fallback brush converter.
        /// </summary>
        internal class FallbackBrushConverter : IValueConverter
        {
            #region Fields

            /// <summary>
            ///     The _fallback brush.
            /// </summary>
            private readonly FallbackMenuBackgroundBrush fallbackBrush;

            #endregion

            #region Constructors and Destructors

            /// <summary>
            /// Initializes a new instance of the <see cref="FallbackBrushConverter"/> class.
            /// </summary>
            /// <param name="extension">
            /// The extension.
            /// </param>
            public FallbackBrushConverter(FallbackMenuBackgroundBrush extension)
            {
                this.fallbackBrush = extension;
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
                    return model.IsRunningInVisualStudio() ? VsBrushes.ToolWindowBackgroundKey : this.fallbackBrush.FallBackBrush;
                }

                return this.fallbackBrush.FallBackBrush;
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