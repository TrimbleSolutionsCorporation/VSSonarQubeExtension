using System.Windows;

namespace VSSonarExtensionUi.View.Helpers
{
    /// <summary>
    /// Interaction logic for UserInputWindow.xaml
    /// </summary>
    public partial class QuestionUser
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserInputWindow" /> class.
        /// </summary>
        /// <param name="question">The question.</param>
        /// <param name="defaultAnswer">The default answer.</param>
        public QuestionUser(string message)
        {
            InitializeComponent();
        }


        /// <summary>
        /// The show.
        /// </summary>
        /// <param name="question">The question.</param>
        /// <returns>answer or empty if cancel</returns>
        public static bool GetInput(string question)
        {
            bool answer = false;
            Application.Current.Dispatcher.Invoke(
                delegate
                {
                    var box = new QuestionUser(question);
                    box.ShowDialog();
                    answer = (bool)box.DialogResult;
                });

            return answer;
        }

        /// <summary>
        /// Handles the Click event of the btnDialogOk control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void BtnDialogOkClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }


        /// <summary>
        /// Handles the Click event of the btnDialogOk control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void BtnDialogOkCancel(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
