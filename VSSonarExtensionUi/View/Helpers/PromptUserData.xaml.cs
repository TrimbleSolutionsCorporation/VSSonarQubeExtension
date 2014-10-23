// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PromptUserData.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for PromptUserData.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace VSSonarExtensionUi.View.Helpers
{
    using System.Windows;

    /// <summary>
    ///     Interaction logic for PromptUserData.xaml
    /// </summary>
    public partial class PromptUserData
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PromptUserData"/> class.
        /// </summary>
        /// <param name="question">
        /// The question.
        /// </param>
        /// <param name="title">
        /// The title.
        /// </param>
        /// <param name="defaultValue">
        /// The default value.
        /// </param>
        /// <param name="inputType">
        /// The input type.
        /// </param>
        public PromptUserData(string question, string title, string defaultValue = "", InputType inputType = InputType.TEXT)
        {
            this.InitializeComponent();
            this.Loaded += this.PromptDialogLoaded;
            this.txtQuestion.Text = question;
            this.Title = title;
            this.txtResponse.Text = defaultValue;
            if (inputType == InputType.PASSWORD)
            {
                this.txtResponse.Visibility = Visibility.Collapsed;
            }
        }

        #endregion

        #region Enums

        /// <summary>
        /// The input type.
        /// </summary>
        public enum InputType
        {
            /// <summary>
            /// The text.
            /// </summary>
            TEXT, 

            /// <summary>
            /// The password.
            /// </summary>
            PASSWORD
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the response text.
        /// </summary>
        public string ResponseText
        {
            get
            {
                return this.txtResponse.Text;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The prompt.
        /// </summary>
        /// <param name="question">
        /// The question.
        /// </param>
        /// <param name="title">
        /// The title.
        /// </param>
        /// <param name="defaultValue">
        /// The default value.
        /// </param>
        /// <param name="inputType">
        /// The input type.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string Prompt(string question, string title, string defaultValue = "", InputType inputType = InputType.TEXT)
        {
            var inst = new PromptUserData(question, title, defaultValue, inputType);
            inst.ShowDialog();
            if (inst.DialogResult == true)
            {
                return inst.ResponseText;
            }

            return null;
        }

        #endregion

        #region Methods

        /// <summary>
        /// The prompt dialog_ loaded.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void PromptDialogLoaded(object sender, RoutedEventArgs e)
        {
            this.txtResponse.Focus();
        }

        /// <summary>
        /// The btn cancel_ click.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void BtnCancelClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// The btn ok_ click.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void BtnOkClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        #endregion
    }
}