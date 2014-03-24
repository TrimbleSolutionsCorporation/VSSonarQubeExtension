namespace VSSonarExtension.MainView
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    using VSSonarExtension.MainViewModel.ViewModel;

    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class UserExceptionMessageBox
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserExceptionMessageBox"/> class.
        /// </summary>
        /// <param name="message">
        ///     The message.
        /// </param>
        /// <param name="exception">
        ///     The exception.
        /// </param>
        /// <param name="stacktrace"></param>
        public UserExceptionMessageBox(string message, Exception exception, string log)
        {
            this.InitializeComponent();

            this.ErrorMessage.Text = string.Format(message + ": {0}", exception.Message);
            this.StackTrace.Text = exception.StackTrace + "\r\n" + log;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserExceptionMessageBox"/> class.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="stacktrace">
        /// The stacktrace.
        /// </param>
        public UserExceptionMessageBox(string message, string stacktrace)
        {
            this.InitializeComponent();

            this.ErrorMessage.Text = string.Format(message);
            this.StackTrace.Text = stacktrace;
        }

        /// <summary>
        /// The show.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="exception">
        /// The exception.
        /// </param>
        public static void ShowException(string message, Exception exception)
        {
            Application.Current.Dispatcher.Invoke(delegate
                {
                    var box = new UserExceptionMessageBox(message, exception, string.Empty);
                    box.ShowDialog();
                });
        }

        /// <summary>
        /// The show.
        /// </summary>
        /// <param name="message">
        ///     The message.
        /// </param>
        /// <param name="exception">
        ///     The exception.
        /// </param>
        /// <param name="stacktrace"></param>
        public static void ShowException(string message, Exception exception, string stacktrace)
        {
            Application.Current.Dispatcher.Invoke(delegate
            {
                var box = new UserExceptionMessageBox(message, exception, stacktrace);
                box.ShowDialog();
            });
        }

        /// <summary>
        /// The mouse button event handler.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void MouseButtonEventHandler(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}