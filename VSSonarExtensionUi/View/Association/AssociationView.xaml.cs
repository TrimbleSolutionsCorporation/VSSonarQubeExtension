using System;
using System.Diagnostics;
using VSSonarExtensionUi.ViewModel.Association;

namespace VSSonarExtensionUi.View.Association
{
    /// <summary>
    /// Interaction logic for AssociationView.xaml
    /// </summary>
    public partial class AssociationView
    {
        public AssociationView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SonarQubeUserControlVs"/> class.
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        public AssociationView(AssociationViewModel model)
        {
            if (model != null)
            {
                model.RequestClose += (s, e) => this.Close();
            }

            this.DataContext = model;
            try
            {
                this.InitializeComponent();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}
