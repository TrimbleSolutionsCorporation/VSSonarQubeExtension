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
    using System.Diagnostics;
    using System.Windows.Media;

    using GalaSoft.MvvmLight.Command;
    using Helpers;
    using Model.Helpers;
    using PropertyChanged;
    using SonarLocalAnalyser;
    using VSSonarPlugins;
    using VSSonarPlugins.Types;
    using Model.Association;

    /// <summary>
    /// The license viewer view model.
    /// </summary>
    [ImplementPropertyChanged]
    public class LicenseViewerViewModel : IOptionsViewModelBase, IOptionsModelBase
    {
        /// <summary>
        /// The plugins.
        /// </summary>
        private readonly PluginManagerModel pluginModel;

        /// <summary>
        /// The conf helper
        /// </summary>
        private readonly IConfigurationHelper confHelper;

        /// <summary>
        /// The associated project
        /// </summary>
        private Resource associatedProject;

        /// <summary>
        /// The sonar configuration
        /// </summary>
        private ISonarConfiguration sonarConfig;

        /// <summary>
        /// The source dir
        /// </summary>
        private string sourceDir;

        /// <summary>
        /// Initializes a new instance of the <see cref="LicenseViewerViewModel" /> class.
        /// </summary>
        /// <param name="plugincontroller">The plugincontroller.</param>
        /// <param name="helper">The helper.</param>
        public LicenseViewerViewModel(
            PluginManagerModel plugincontroller,
            IConfigurationHelper helper)
        {
            this.Header = "License Manager";
            this.pluginModel = plugincontroller;
            this.confHelper = helper;

            this.ForeGroundColor = Colors.Black;
            this.ForeGroundColor = Colors.Black;
            this.AvailableLicenses = new ObservableCollection<VsLicense>();
            this.RefreshCommand = new RelayCommand(this.GetLicensesFromServer);
            this.GenerateTokenCommand = new RelayCommand(this.OnGenerateTokenCommand, () => this.SelectedLicense != null);

            try
            {
                this.GetLicensesFromServer();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            // register model so it can be updated
            AssociationModel.RegisterNewModelInPool(this);
            SonarQubeViewModel.RegisterNewViewModelInPool(this);
        }

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

        /// <summary>
        /// Gets or sets the refresh command.
        /// </summary>
        /// <value>
        /// The refresh command.
        /// </value>
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

        /// <summary>
        /// The refresh licenses.
        /// </summary>
        public void GetLicensesFromServer()
        {
            this.AvailableLicenses.Clear();

            if (this.pluginModel != null)
            {
                foreach (var plugin in this.pluginModel.AnalysisPlugins)
                {
                    this.GetLicenceForPlugin(this.AvailableLicenses, plugin);
                }

                foreach (var plugin in this.pluginModel.MenuPlugins)
                {
                    this.GetLicenceForPlugin(this.AvailableLicenses, plugin);
                }
            }
        }

        /// <summary>
        /// The init data association.
        /// </summary>
        /// <param name="associatedProjectIn">The associated project in.</param>
        public void ReloadDataFromDisk(Resource associatedProjectIn)
        {
            // does not refresh data
        }

        /// <summary>
        /// The refresh data for resource.
        /// </summary>
        /// <param name="fullName">The full name.</param>
        public void RefreshDataForResource(Resource fullName)
        {
            // does not refresh data
        }

        /// <summary>
        /// The save and close.
        /// </summary>
        public void SaveAndClose()
        {
            // does not save data
        }

        /// <summary>
        /// Gets the view model.
        /// </summary>
        /// <returns>
        /// returns view model
        /// </returns>
        public object GetViewModel()
        {
            return this;
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

        /// <summary>
        /// Gets the available model, TODO: needs to be removed after viewmodels are split into models and view models
        /// </summary>
        /// <returns>
        /// returns optinal model
        /// </returns>
        public object GetAvailableModel()
        {
            return null;
        }

        /// <summary>
        /// Saves the data.
        /// </summary>
        public void SaveData()
        {
            // does not save data
        }

        /// <summary>
        /// Updates the services.
        /// </summary>
        /// <param name="vsenvironmenthelperIn">The vsenvironmenthelper in.</param>
        /// <param name="statusBar">The status bar.</param>
        /// <param name="provider">The provider.</param>
        public void UpdateServices(IVsEnvironmentHelper vsenvironmenthelperIn, IVSSStatusBar statusBar, IServiceProvider provider)
        {
            // does not access visual studio services
        }

        /// <summary>
        /// Associates the with new project.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="project">The project.</param>
        /// <param name="workDir">The work dir.</param>
        /// <param name="provider">The provider.</param>
        public void AssociateWithNewProject(ISonarConfiguration config, Resource project, string workDir, ISourceControlProvider provider)
        {
            this.sonarConfig = config;
            this.associatedProject = project;
            this.sourceDir = workDir;
        }

        /// <summary>
        /// The end data association.
        /// </summary>
        public void EndDataAssociation()
        {
            this.sonarConfig = null;
            this.associatedProject = null;
            this.sourceDir = string.Empty;
        }

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

        /// <summary>
        /// Gets the licence for plugin.
        /// </summary>
        /// <param name="licenses">The licenses.</param>
        /// <param name="plugin">The plugin.</param>
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
    }
}