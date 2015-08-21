namespace VSSonarExtensionUi.View.Helpers
{
    using System.Windows;

    /// <summary>
    /// Interaction logic for UserInputWindow.xaml
    /// </summary>
    public partial class MessageDisplayBox
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageDisplayBox"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="extendedMessage">The extended message.</param>
        public MessageDisplayBox(string message, string extendedMessage)
        {
            InitializeComponent();
            this.Message.Text = message;
            this.DetailedMessage.Text = extendedMessage;
        }

        /// <summary>
        /// The show.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="detailedMessage">The detailed message.</param>
        public static void DisplayMessage(string message, string detailedMessage)
        {
            Application.Current.Dispatcher.Invoke(
                delegate
                {
                    var box = new MessageDisplayBox(message, detailedMessage);
                    box.ShowDialog();
                });
        }

        /// <summary>
        /// Handles the Click event of the btnDialogOk control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void BtnDialogOkClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
