// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RoslynManagerViewModel.cs" company="Copyright © 2015 Tekla Corporation. Tekla is a Trimble Company">
//     Copyright (C) 2014 [Jorge Costa, Jorge.Costa@tekla.com]
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
// This program is free software; you can redistribute it and/or modify it under the terms of the GNU Lesser General Public License
// as published by the Free Software Foundation; either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
// of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details. 
// You should have received a copy of the GNU Lesser General Public License along with this program; if not, write to the Free
// Software Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// --------------------------------------------------------------------------------------------------------------------
namespace VSSonarExtensionUi.ViewModel.Configuration
{
    using System;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Windows.Input;
    using System.Windows.Forms;
    using System.Windows.Media;
   
    using PropertyChanged;   
    using Model.Configuration;
    using Helpers;
    using GalaSoft.MvvmLight.Command;    
    using View.Helpers;

    /// <summary>
    /// roslyn manager view model
    /// </summary>
    [ImplementPropertyChanged]
    public class RoslynManagerViewModel : IOptionsViewModelBase
    {
        /// <summary>
        /// The model
        /// </summary>
        private readonly RoslynManagerModel model;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoslynManagerViewModel"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        public RoslynManagerViewModel(RoslynManagerModel model)
        {
            this.Header = "Roslyn Manager";
            this.model = model;
            this.BackGroundColor = Colors.White;
            this.ForeGroundColor = Colors.Black;
            this.AvailableDllDiagnostics = new ObservableCollection<VSSonarExtensionDiagnostic>();

            foreach (var item in model.ExtensionDiagnostics)
            {
                AvailableDllDiagnostics.Add(item.Value);
            }

            SonarQubeViewModel.RegisterNewViewModelInPool(this);
            this.InstallNewDllCommand = new RelayCommand(this.OnInstallNewDllCommand);
            this.RemoveDllCommand = new RelayCommand(this.OnRemoveDllCommand);
            this.AutoImportLocalDiagnosticsCommand = new RelayCommand(this.OnAutoImportLocalDiagnosticsCommand);
        }

        /// <summary>
        /// Called when [automatic import local diagnostics command].
        /// </summary>
        private void OnAutoImportLocalDiagnosticsCommand()
        {
            if (!this.model.VerifyExistenceOfRoslynPlugin())
            {
                MessageDisplayBox.DisplayMessage(
                    "Adding new diagnostics analyzers requires Roslyn plugin installed in your SonarQube server. Administer Quality Profiles and Gates and Provision Projects permissions must be available to user.",
                    helpurl: "https://visualstudiogallery.msdn.microsoft.com/7fc312c3-f1ab-49f8-b286-dbf7fff37305");
                return;
            }

            if (this.model.Profile == null)
            {
                MessageDisplayBox.DisplayMessage(
                    "Auto detection only available when a solution is associated with a sonar project.");
                return;
            }

            var result = this.model.AutoDetectInstalledAnalysers();
            if (string.IsNullOrEmpty(result))
            {
                MessageDisplayBox.DisplayMessage("Rules have been created in SonarQube. Please use tools > sqale editor to enabled them or use the Roslyn Plugin to automatically sync those against your project. ", helpurl: "https://visualstudiogallery.msdn.microsoft.com/7fc312c3-f1ab-49f8-b286-dbf7fff37305");
            }
            else
            {
                MessageDisplayBox.DisplayMessage(result);
            }
        }

        /// <summary>
        /// Called when [install new DLL command].
        /// </summary>
        private void OnInstallNewDllCommand()
        {
            using (var filedialog = new OpenFileDialog { InitialDirectory = this.model.SourceDir, Filter = @"dlls|*.dll", Title = "Select dll available in project folder."})
            {
                DialogResult result = filedialog.ShowDialog();

                if (result == DialogResult.OK)
                {
                    if (!File.Exists(filedialog.FileName))
                    {
                        UserExceptionMessageBox.ShowException("Error Choosing File, File Does not exits", null);
                        return;
                    }

                    if (!this.model.VerifyExistenceOfRoslynPlugin())
                    {
                        MessageDisplayBox.DisplayMessage(
                            "Adding new diagnostics analyzers requires Roslyn plugin installed in your SonarQube server. Administer Quality Profiles and Gates and Provision Projects permissions must be available to user.",
                            helpurl: "https://visualstudiogallery.msdn.microsoft.com/7fc312c3-f1ab-49f8-b286-dbf7fff37305");
                        return;
                    }

                    if (this.model.AddNewRoslynPack(filedialog.FileName, true))
                    {
                        this.SyncDiagInView();
                        MessageDisplayBox.DisplayMessage("Rules have been created in SonarQube. Please use tools > sqale editor to enabled them or use the Roslyn Plugin to automatically sync those against your project. ", helpurl: "https://visualstudiogallery.msdn.microsoft.com/7fc312c3-f1ab-49f8-b286-dbf7fff37305");
                    }
                }
            }
        }

        /// <summary>
        /// Called when [remove DLL command].
        /// </summary>
        private void OnRemoveDllCommand()
        {
            if (this.SelectedDllDiagnostic != null)
            {
                this.model.RemoveDllFromList(this.SelectedDllDiagnostic);
                this.SyncDiagInView();
            }
        }

        private void SyncDiagInView()
        {
            this.AvailableDllDiagnostics.Clear();

            foreach (var item in model.ExtensionDiagnostics)
            {
                this.AvailableDllDiagnostics.Add(item.Value);
            }
        }

        /// <summary>
        /// Gets or sets the selected DLL diagnostic.
        /// </summary>
        /// <value>
        /// The selected DLL diagnostic.
        /// </value>
        public VSSonarExtensionDiagnostic SelectedDllDiagnostic { get; set; }

        /// <summary>
        /// Gets or sets the available DLL diagnostics.
        /// </summary>
        /// <value>
        /// The available DLL diagnostics.
        /// </value>
        public ObservableCollection<VSSonarExtensionDiagnostic> AvailableDllDiagnostics { get; set; }

        /// <summary>
        /// Gets or sets the back ground color.
        /// </summary>
        public Color BackGroundColor { get; set; }

        /// <summary>
        /// Gets or sets the fore ground color.
        /// </summary>
        public Color ForeGroundColor { get; set; }

        /// <summary>
        /// Gets or sets the header.
        /// </summary>
        public string Header { get; set; }
        
        /// <summary>
        /// Gets the install new DLL command.
        /// </summary>
        /// <value>
        /// The install new DLL command.
        /// </value>
        public ICommand InstallNewDllCommand { get; private set; }

        /// <summary>
        /// Gets the remove DLL command.
        /// </summary>
        /// <value>
        /// The remove DLL command.
        /// </value>
        public ICommand RemoveDllCommand { get; private set; }

        /// <summary>
        /// Gets the automatic import local diagnostics command.
        /// </summary>
        /// <value>
        /// The automatic import local diagnostics command.
        /// </value>
        public ICommand AutoImportLocalDiagnosticsCommand { get; private set; }
        
        /// <summary>
        /// The update colours.
        /// </summary>
        /// <param name="background">The background.</param>
        /// <param name="foreground">The foreground.</param>
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
    }
}
