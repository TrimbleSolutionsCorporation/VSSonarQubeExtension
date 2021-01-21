// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InternalStateVariables.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
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
namespace VSSonarQubeExtension
{
    using System;
    using System.Threading.Tasks;
    using VSSonarExtensionUi.ViewModel;
    using VSSonarQubeExtension.Helpers;

    /// <summary>
    /// model factory
    /// </summary>
    internal class SonarQubeViewModelFactory
    {
        /// <summary>
        /// The model
        /// </summary>
        private static SonarQubeViewModel model;

        /// <summary>
        /// Prevents a default instance of the <see cref="SonarQubeViewModelFactory"/> class from being created.
        /// </summary>
        private SonarQubeViewModelFactory()
        {
        }

        /// <summary>
        /// Gets the sq view model.
        /// </summary>
        /// <value>
        /// The sq view model.
        /// </value>
        public static SonarQubeViewModel SQViewModel
        {
            get
            {
                if (model == null)
                {
                    model = new SonarQubeViewModel(string.Empty, null);
                }

                return model;
            }
        }

        /// <summary>
        /// Startups the model with vs version.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <returns>returns model</returns>
        public static async Task<SonarQubeViewModel> StartupModelWithVsVersionAsync(string version, IServiceProvider provider)
        {
            if (model == null)
            {
				await System.Threading.Tasks.Task.Run(() => { model = new SonarQubeViewModel(version, new VsConfigurationHelper(version)); });
            }

			return model;
        }

		/// <summary>
		/// Startups the model with vs version.
		/// </summary>
		/// <param name="version">The version.</param>
		/// <returns>returns model</returns>
		public static SonarQubeViewModel StartupModelWithVsVersion(string version, IServiceProvider provider)
		{
			if (model == null)
			{
				model = new SonarQubeViewModel(version, new VsConfigurationHelper(version));
			}

			return model;
		}
	}
}
