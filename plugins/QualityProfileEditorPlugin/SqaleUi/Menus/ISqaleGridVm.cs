// --------------------------------------------------------------------------------------------------------------------
// <copyright file="isqalegridvm.cs" company="Copyright © 2014 jmecsoftware">
//     Copyright (C) 2014 [jmecsoftware, jmecsoftware2014@tekla.com]
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

namespace SqaleUi.Menus
{
    using System.Collections.Generic;

    using VSSonarPlugins;
    using VSSonarPlugins.Types;

    using SonarRestService;

    using SqaleUi.helpers;
    using SqaleUi.ViewModel;

    /// <summary>
    ///     The SqaleGridVm interface.
    /// </summary>
    public interface ISqaleGridVm
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the configuration.
        /// </summary>
        ISonarConfiguration Configuration { get; set; }

        /// <summary>
        /// Gets or sets the profile rules.
        /// </summary>
        ItemsChangeObservableCollection<Rule> ProfileRules { get; set; }

        /// <summary>
        /// Gets or sets the rest service.
        /// </summary>
        ISonarRestService RestService { get; set; }

        /// <summary>
        /// Gets or sets the selected profile.
        /// </summary>
        Profile SelectedProfile { get; set; }

        /// <summary>
        /// Gets or sets the selected rule.
        /// </summary>
        Rule SelectedRule { get; set; }

        /// <summary>
        /// Gets or sets the quality viewer model.
        /// </summary>
        QualityViewerViewModel QualityViewerModel { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The aggregate error strings.
        /// </summary>
        /// <param name="arg1">
        /// The arg 1.
        /// </param>
        /// <param name="arg2">
        /// The arg 2.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        string AggregateErrorStrings(string arg1, string arg2);

        /// <summary>
        /// The create new key.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        string CreateNewKey();

        /// <summary>
        /// The merge rules into project.
        /// </summary>
        /// <param name="rules">
        /// The rules.
        /// </param>
        void MergeRulesIntoProject(List<Rule> rules);

        /// <summary>
        /// The refresh view.
        /// </summary>
        void RefreshView();

        /// <summary>
        /// The send selected items to work area.
        /// </summary>
        /// <param name="commandText">
        /// The command text.
        /// </param>
        void SendSelectedItemsToWorkArea(string commandText);

        /// <summary>
        /// The set connected to server.
        /// </summary>
        /// <param name="b">
        /// The b.
        /// </param>
        void SetConnectedToServer(bool b);

        #endregion
    }
}