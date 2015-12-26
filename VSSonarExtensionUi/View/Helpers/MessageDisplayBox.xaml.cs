namespace VSSonarExtensionUi.View.Helpers
{
    using System;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Documents;
    using System.Windows.Navigation;    
    
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
        public MessageDisplayBox(string message, string extendedMessage, string url)
        {
            InitializeComponent();
            this.Message.Document.Blocks.Add(new Paragraph(new Run(message)));
            if (!string.IsNullOrEmpty(extendedMessage))
            {
                this.Message.Document.Blocks.Add(new Paragraph(new Run(extendedMessage)));
            }            

            if (!string.IsNullOrEmpty(url))
            {
                this.AddHyperlinkText(url, url);
            }

            this.Message.AddHandler(Hyperlink.RequestNavigateEvent, new RequestNavigateEventHandler(RequestNavigateHandler));
        }

        /// <summary>
        /// Requests the navigate handler.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Navigation.RequestNavigateEventArgs" /> instance containing the event data.</param>
        private void RequestNavigateHandler(object sender, RequestNavigateEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// The show.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="detailedMessage">The detailed message.</param>
        public static void DisplayMessage(string message, string detailedMessage = "", string helpurl = "")
        {
            Application.Current.Dispatcher.Invoke(
                delegate
                {
                    try
                    {
                        var box = new MessageDisplayBox(message, detailedMessage, helpurl);
                        box.WindowStyle = WindowStyle.None;
                        box.ShowDialog();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                });
        }

        /// <summary>
        /// Adds the hyperlink text.
        /// </summary>
        /// <param name="linkURL">The link URL.</param>
        /// <param name="linkName">Name of the link.</param>
        private void AddHyperlinkText(string linkURL, string linkName)
        {
            Paragraph para = new Paragraph();
            para.Margin = new Thickness(0); // remove indent between paragraphs

            Hyperlink link = new Hyperlink();
            link.IsEnabled = true;
            link.Inlines.Add(linkName);
            link.NavigateUri = new Uri(linkURL);
            link.RequestNavigate += (sender, args) => Process.Start(args.Uri.ToString());

            para.Inlines.Add("Please visit ");
            para.Inlines.Add(link);
            para.Inlines.Add(" for more information.");

            this.Message.Document.Blocks.Add(para);
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
