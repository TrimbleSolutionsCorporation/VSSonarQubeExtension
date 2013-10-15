// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProjectAssociationWindow.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for ProjectAssociationWindow.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ExtensionView
{
    using System.Windows;
    using System.Windows.Input;

    using ExtensionViewModel.ViewModel;

    /// <summary>
    /// Interaction logic for ProjectAssociationWindow.xaml
    /// </summary>
    public partial class ProjectAssociationWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectAssociationWindow"/> class.
        /// </summary>
        /// <param name="dataModel">
        /// The data model.
        /// </param>
        public ProjectAssociationWindow(ProjectAssociationDataModel dataModel)
        {
            this.InitializeComponent();
            
            // Insert code required on object creation below this point.
            this.DataContext = dataModel;
        }

        /// <summary>
        /// The close command handler.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void CloseCommandHandler(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }
    }
}