// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SonarQubeUserControlVS.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for SonarQubeWindow.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace VSSonarExtensionUi.View
{
    using System;
    using System.Diagnostics;

    using VSSonarExtensionUi.ViewModel;

    /// <summary>
    ///     Interaction logic for SonarQubeWindow.xaml
    /// </summary>
    public partial class SonarQubeUserControlVs
    {
        #region Fields

        /// <summary>
        ///     The data model.
        /// </summary>
        private SonarQubeViewModel dataModel;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SonarQubeUserControlVs"/> class.
        /// </summary>
        public SonarQubeUserControlVs()
        {
            try
            {
                this.InitializeComponent();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SonarQubeUserControlVs"/> class.
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        public SonarQubeUserControlVs(SonarQubeViewModel model)
        {
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

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The update data context.
        /// </summary>
        /// <param name="dataModelIn">
        /// The data model in.
        /// </param>
        public void UpdateDataContext(SonarQubeViewModel dataModelIn)
        {
            // bind data with view model
            this.dataModel = dataModelIn;
            this.DataContext = null;
            this.DataContext = dataModelIn;
        }

        #endregion
    }
}