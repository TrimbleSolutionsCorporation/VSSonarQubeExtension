// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LicenseViewerViewModel.cs" company="">
//   
// </copyright>
// <summary>
//   The license viewer view model.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace VSSonarExtensionUi.ViewModel.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Windows.Media;
    using GalaSoft.MvvmLight.Command;
    using PropertyChanged;
    using Helpers;
    using VSSonarPlugins;
    using VSSonarPlugins.Types;
    using Model.Helpers;

    /// <summary>
    /// The license viewer view model.
    /// </summary>
    [ImplementPropertyChanged]
    public class LicenseViewerViewModel : IViewModelBase, IOptionsViewModelBase
    {
        #region Fields

        /// <summary>
        /// The plugins.
        /// </summary>
        private readonly PluginManagerModel pluginModel;

        private readonly IConfigurationHelper confHelper;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LicenseViewerViewModel"/> class.
        /// </summary>
        /// <param name="plugincontroller">
        /// The plugincontroller.
        /// </param>
        /// <param name="configuration">
        /// The configuration.
        /// </param>
        public LicenseViewerViewModel(PluginManagerModel plugincontroller, IConfigurationHelper helper)
        {
            this.Header = "License Manager";
            this.pluginModel = plugincontroller;
            this.confHelper = helper;

            this.AvailableLicenses = new ObservableCollection<VsLicense>();
            this.GetLicensesFromServer();

            this.GenerateTokenCommand = new RelayCommand(this.OnGenerateTokenCommand, () => this.SelectedLicense != null);
            this.RefreshCommand = new RelayCommand(this.GetLicensesFromServer);

            this.ForeGroundColor = Colors.Black;
            this.ForeGroundColor = Colors.Black;
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the available licenses.
        /// </summary>
        public ObservableCollection<VsLicense> AvailableLicenses { get; set; }

        /// <summary>
        /// Gets or sets the back ground color.
        /// </summary>
        public Color BackGroundColor { get; set; }

        /// <summary>
        /// Gets or sets the fore ground color.
        /// </summary>
        public Color ForeGroundColor { get; set; }

        /// <summary>
        /// Gets or sets the generate token command.
        /// </summary>
        public RelayCommand GenerateTokenCommand { get; set; }

        public System.Windows.Input.ICommand RefreshCommand { get; set; }

        /// <summary>
        /// Gets or sets the generated token.
        /// </summary>
        public string GeneratedToken { get; set; }

        /// <summary>
        /// Gets or sets the header.
        /// </summary>
        public string Header { get; set; }

        /// <summary>
        /// Gets or sets the selected license.
        /// </summary>
        public VsLicense SelectedLicense { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The update services.
        /// </summary>
        /// <param name="restServiceIn">
        /// The rest service in.
        /// </param>
        /// <param name="vsenvironmenthelperIn">
        /// The vsenvironmenthelper in.
        /// </param>
        /// <param name="statusBar">
        /// The status bar.
        /// </param>
        /// <param name="provider">
        /// The provider.
        /// </param>
        public void UpdateServices(
            ISonarRestService restServiceIn,
            IVsEnvironmentHelper vsenvironmenthelperIn,
            IConfigurationHelper configurationHelper,
            IVSSStatusBar statusBar,
            IServiceProvider provider)
        {
        }

        public void SaveCurrentViewToDisk(IConfigurationHelper configurationHelper)
        {
        }

        /// <summary>
        ///     The refresh licenses.
        /// </summary>
        /// <returns>
        ///     The
        ///     <see>
        ///         <cref>ObservableCollection</cref>
        ///     </see>
        ///     .
        /// </returns>
        public void GetLicensesFromServer()
        {
            this.AvailableLicenses.Clear();

            var licenses = new ObservableCollection<VsLicense>();

            if (this.pluginModel != null)
            {
                foreach (var plugin in this.pluginModel.AnalysisPlugins)
                {
                    GetLicenceForPlugin(this.AvailableLicenses, plugin);
                }

                foreach (var plugin in this.pluginModel.MenuPlugins)
                {
                    GetLicenceForPlugin(this.AvailableLicenses, plugin);
                }
            }
        }

        private void GetLicenceForPlugin(ObservableCollection<VsLicense> licenses, IPlugin plugin)
        {
            if (AuthtenticationHelper.AuthToken == null)
            {
                return;
            }

            Dictionary<string, VsLicense> lics = plugin.GetLicenses(AuthtenticationHelper.AuthToken);
            if (lics != null)
            {
                foreach (var license in lics)
                {
                    bool existsAlready = false;
                    foreach (VsLicense existinglicense in licenses)
                    {
                        if (existinglicense.LicenseTxt.Equals(license.Value.LicenseTxt))
                        {
                            existsAlready = true;
                        }
                    }

                    if (!existsAlready)
                    {
                        licenses.Add(license.Value);
                    }
                }
            }
        }

        /// <summary>
        /// The init data association.
        /// </summary>
        /// <param name="associatedProject">
        /// The associated project.
        /// </param>
        /// <param name="userConnectionConfig">
        /// The user connection config.
        /// </param>
        /// <param name="workingDir">
        /// The working dir.
        /// </param>
        public void RefreshPropertiesInView(Resource associatedProject)
        {
        }

        /// <summary>
        /// The refresh data for resource.
        /// </summary>
        /// <param name="fullName">
        /// The full name.
        /// </param>
        public void RefreshDataForResource(Resource fullName)
        {
        }

        /// <summary>
        /// The save and close.
        /// </summary>
        public void SaveAndClose()
        {
        }

        /// <summary>
        /// The update colours.
        /// </summary>
        /// <param name="background">
        /// The background.
        /// </param>
        /// <param name="foreground">
        /// The foreground.
        /// </param>
        public void UpdateColours(Color background, Color foreground)
        {
            this.BackGroundColor = background;
            this.ForeGroundColor = foreground;
        }

        #endregion

        #region Methods

        /// <summary>
        /// The on generate token command.
        /// </summary>
        private void OnGenerateTokenCommand()
        {
            foreach (var plugin in this.pluginModel.AnalysisPlugins)
            {
                string key = plugin.GetPluginDescription().Name;
                if (this.SelectedLicense.ProductId.Contains(key))
                {
                    this.GeneratedToken = plugin.GenerateTokenId(AuthtenticationHelper.AuthToken);
                }
            }

            foreach (var plugin in this.pluginModel.MenuPlugins)
            {
                string key = plugin.GetPluginDescription().Name;
                if (this.SelectedLicense.ProductId.Contains(key))
                {
                    this.GeneratedToken = plugin.GenerateTokenId(AuthtenticationHelper.AuthToken);
                }
            }
        }

        #endregion
    }
}