using ExtensionTypes;
using PropertyChanged;
using System.Collections.ObjectModel;
using System;
using VSSonarPlugins;
using System.Collections.Generic;
using GalaSoft.MvvmLight.Command;

namespace VSSonarExtensionUi.ViewModel
{
    [ImplementPropertyChanged]
    public class LicenseViewerViewModel : IViewModelBase
    {
        private List<IAnalysisPlugin> plugins;

        public LicenseViewerViewModel()
        {
            this.Header = "License Manager";
            this.AvailableLicenses = new ObservableCollection<VsLicense>();
            this.AvailableLicenses = this.GetLicensesFromServer();
        }

        public LicenseViewerViewModel(PluginController plugincontroller, ISonarConfiguration configuration)
        {
            this.Header = "License Manager";
            this.AvailableLicenses = new ObservableCollection<VsLicense>();
            this.AvailableLicenses = this.GetLicensesFromServer();
            this.plugins = plugincontroller.GetPlugins();
            this.ConnectionConfiguration = configuration;

            this.GenerateTokenCommand = new RelayCommand(this.OnGenerateTokenCommand, () => !(this.SelectedLicense == null));
        }

        private void OnGenerateTokenCommand()
        {
            foreach (var plugin in this.plugins)
            {
                var key = plugin.GetKey(this.ConnectionConfiguration);
                if (this.SelectedLicense.ProductId.Contains(key))
                {
                    this.GeneratedToken = plugin.GenerateTokenId(this.ConnectionConfiguration);
                }
            }
        }


        /// <summary>
        /// Gets or sets the available licenses.
        /// </summary>
        public ObservableCollection<VsLicense> AvailableLicenses { get; set; }
        public ISonarConfiguration ConnectionConfiguration { get; set; }
        public string Header { get; set; }

        public RelayCommand GenerateTokenCommand { get; set; }
        public VsLicense SelectedLicense { get; set; }
        public string GeneratedToken { get; set; }




        /// <summary>
        /// The refresh licenses.
        /// </summary>
        /// <returns>
        /// The <see>
        ///         <cref>ObservableCollection</cref>
        ///     </see>
        ///     .
        /// </returns>
        public ObservableCollection<VsLicense> GetLicensesFromServer()
        {
            var licenses = new ObservableCollection<VsLicense>();

            if (this.plugins != null)
            {
                foreach (var plugin in this.plugins)
                {
                    var lics = plugin.GetLicenses(this.ConnectionConfiguration);
                    if (lics != null)
                    {
                        foreach (var license in lics)
                        {
                            bool existsAlready = false;
                            foreach (var existinglicense in licenses)
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

            return licenses;
        }
    }
}
