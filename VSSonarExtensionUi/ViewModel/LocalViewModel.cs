// --------------------------------------------------------------------------------------------------------------------
// <copyright file="localviewmodel.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
//     Copyright (C) 2013 [Jorge Costa, Jorge.Costa@tekla.com]
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSSonarExtensionUi.ViewModel
{
    using GalaSoft.MvvmLight;

    using PropertyChanged;

    using SonarLocalAnalyser;
    using SonarRestService;
    using VSSonarPlugins;

    [ImplementPropertyChanged]
    public class LocalViewModel : IViewModelBase
    {
        private readonly SonarQubeViewModel sonarQubeViewModel;

        public LocalViewModel(SonarQubeViewModel sonarQubeViewModel, ISonarRestService service, IVsEnvironmentHelper vshelper, List<IAnalysisPlugin> plugins)
        {
            this.RestService = service;
            this.Vsenvironmenthelper = vshelper;
            this.sonarQubeViewModel = sonarQubeViewModel;
            this.Header = "Local Analysis";

            this.LocalAnalyserModule = new SonarLocalAnalyser(new List<IAnalysisPlugin>(plugins), this.RestService, this.Vsenvironmenthelper);
            this.LocalAnalyserModule.StdOutEvent += this.UpdateOutputMessagesFromPlugin;
            this.LocalAnalyserModule.LocalAnalysisCompleted += this.UpdateLocalIssues;
        }

        private void UpdateLocalIssues(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void UpdateOutputMessagesFromPlugin(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        public ISonarLocalAnalyser LocalAnalyserModule { get; set; }

        public string Header { get; set; }
        public ISonarRestService RestService { get; private set; }
        public IVsEnvironmentHelper Vsenvironmenthelper { get; private set; }
    }
}
