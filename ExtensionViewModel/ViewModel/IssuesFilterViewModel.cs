// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IssuesFilterViewModel.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
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

namespace ExtensionViewModel.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    using ExtensionTypes;

    /// <summary>
    ///     The extension data model.
    /// </summary>
    public partial class ExtensionDataModel
    {
        #region Fields

        /// <summary>
        /// The issues filter view model key.
        /// </summary>
        private const string IssuesFilterViewModelKey = "IssuesFilterViewModel";

        /// <summary>
        /// The assignee in filter.
        /// </summary>
        private User assigneeInFilter;

        /// <summary>
        ///     The created before date.
        /// </summary>
        private DateTime createdBeforeDate;

        /// <summary>
        ///     The created since date.
        /// </summary>
        private DateTime createdSinceDate;

        /// <summary>
        ///     The project resources.
        /// </summary>
        private List<Resource> projectResources;

        /// <summary>
        /// The reporter in filter.
        /// </summary>
        private User reporterInFilter;

        /// <summary>
        ///     The selected project in filter.
        /// </summary>
        private Resource selectedProjectInFilter;

        /// <summary>
        ///     The selected user in filter.
        /// </summary>
        private User selectedUserInFilter;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the assignee in filter.
        /// </summary>
        public User AssigneeInFilter
        {
            get
            {
                return this.assigneeInFilter;
            }

            set
            {
                this.assigneeInFilter = value;
                this.OnPropertyChanged("AssigneeInFilter");
            }
        }

        /// <summary>
        ///     Gets or sets the created before date.
        /// </summary>
        public DateTime CreatedBeforeDate
        {
            get
            {
                return this.createdBeforeDate;
            }

            set
            {
                this.createdBeforeDate = value;
                this.OnPropertyChanged("CreatedBeforeDate");
            }
        }

        /// <summary>
        ///     Gets or sets the created since date.
        /// </summary>
        public DateTime CreatedSinceDate
        {
            get
            {
                return this.createdSinceDate;
            }

            set
            {
                this.createdSinceDate = value;
                this.OnPropertyChanged("CreatedSinceDate");
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether is assignee checked.
        /// </summary>
        public bool IsAssigneeChecked { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is blocker checked.
        /// </summary>
        public bool IsBlockerChecked { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is critical checked.
        /// </summary>
        public bool IsCriticalChecked { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is false positive checked.
        /// </summary>
        public bool IsFalsePositiveChecked { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is fixed checked.
        /// </summary>
        public bool IsFixedChecked { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is info checked.
        /// </summary>
        public bool IsInfoChecked { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is majaor checked.
        /// </summary>
        public bool IsMajaorChecked { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is minor checked.
        /// </summary>
        public bool IsMinorChecked { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is removed checked.
        /// </summary>
        public bool IsRemovedChecked { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is reporter checked.
        /// </summary>
        public bool IsReporterChecked { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is status closed checked.
        /// </summary>
        public bool IsStatusClosedChecked { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is status confirmed checked.
        /// </summary>
        public bool IsStatusConfirmedChecked { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is status open checked.
        /// </summary>
        public bool IsStatusOpenChecked { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is status reopened checked.
        /// </summary>
        public bool IsStatusReopenedChecked { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is status resolved checked.
        /// </summary>
        public bool IsStatusResolvedChecked { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is date before checked.
        /// </summary>
        public bool IsDateBeforeChecked { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is date since checked.
        /// </summary>
        public bool IsDateSinceChecked { get; set; }

        /// <summary>
        ///     Gets or sets the users list.
        /// </summary>
        public List<Resource> ProjectResources
        {
            get
            {
                return this.projectResources;
            }

            set
            {
                this.projectResources = value;
                this.OnPropertyChanged("ProjectResources");
            }
        }

        /// <summary>
        /// Gets or sets the reporter in filter.
        /// </summary>
        public User ReporterInFilter
        {
            get
            {
                return this.reporterInFilter;
            }

            set
            {
                this.reporterInFilter = value;
                this.OnPropertyChanged("ReporterInFilter");
            }
        }

        /// <summary>
        ///     Gets or sets the selected user in filter.
        /// </summary>
        public Resource SelectedProjectInFilter
        {
            get
            {
                return this.selectedProjectInFilter;
            }

            set
            {
                this.selectedProjectInFilter = value;
                this.OnPropertyChanged("SelectedProjectInFilter");
            }
        }

        /// <summary>
        ///     Gets or sets the selected user in filter.
        /// </summary>
        public User SelectedUserInFilter
        {
            get
            {
                return this.selectedUserInFilter;
            }

            set
            {
                this.selectedUserInFilter = value;
                this.OnPropertyChanged("SelectedUserInFilter");
            }
        }        

        #endregion

        #region Methods

        /// <summary>
        /// The save filter to disk.
        /// </summary>
        public void SaveFilterToDisk()
        {
            if (this.vsenvironmenthelper != null)
            {
                this.vsenvironmenthelper.WriteOptionInApplicationData(IssuesFilterViewModelKey, "IsStatusOpenChecked", this.IsStatusOpenChecked.ToString(CultureInfo.InvariantCulture));
                this.vsenvironmenthelper.WriteOptionInApplicationData(IssuesFilterViewModelKey, "IsStatusClosedChecked", this.IsStatusClosedChecked.ToString(CultureInfo.InvariantCulture));
                this.vsenvironmenthelper.WriteOptionInApplicationData(IssuesFilterViewModelKey, "IsStatusResolvedChecked", this.IsStatusResolvedChecked.ToString(CultureInfo.InvariantCulture));
                this.vsenvironmenthelper.WriteOptionInApplicationData(IssuesFilterViewModelKey, "IsStatusConfirmedChecked", this.IsStatusConfirmedChecked.ToString(CultureInfo.InvariantCulture));
                this.vsenvironmenthelper.WriteOptionInApplicationData(IssuesFilterViewModelKey, "IsStatusReopenedChecked", this.IsStatusReopenedChecked.ToString(CultureInfo.InvariantCulture));
                this.vsenvironmenthelper.WriteOptionInApplicationData(IssuesFilterViewModelKey, "IsBlockerChecked", this.IsBlockerChecked.ToString(CultureInfo.InvariantCulture));
                this.vsenvironmenthelper.WriteOptionInApplicationData(IssuesFilterViewModelKey, "IsCriticalChecked", this.IsCriticalChecked.ToString(CultureInfo.InvariantCulture));
                this.vsenvironmenthelper.WriteOptionInApplicationData(IssuesFilterViewModelKey, "IsMajaorChecked", this.IsMajaorChecked.ToString(CultureInfo.InvariantCulture));
                this.vsenvironmenthelper.WriteOptionInApplicationData(IssuesFilterViewModelKey, "IsMinorChecked", this.IsMinorChecked.ToString(CultureInfo.InvariantCulture));
                this.vsenvironmenthelper.WriteOptionInApplicationData(IssuesFilterViewModelKey, "IsInfoChecked", this.IsInfoChecked.ToString(CultureInfo.InvariantCulture));
                this.vsenvironmenthelper.WriteOptionInApplicationData(IssuesFilterViewModelKey, "IsFalsePositiveChecked", this.IsFalsePositiveChecked.ToString(CultureInfo.InvariantCulture));
                this.vsenvironmenthelper.WriteOptionInApplicationData(IssuesFilterViewModelKey, "IsRemovedChecked", this.IsRemovedChecked.ToString(CultureInfo.InvariantCulture));
                this.vsenvironmenthelper.WriteOptionInApplicationData(IssuesFilterViewModelKey, "IsFixedChecked", this.IsFixedChecked.ToString(CultureInfo.InvariantCulture));
            }
        }

        /// <summary>
        ///     The restore user filtering options.
        /// </summary>
        private void RestoreUserFilteringOptions()
        {
            this.createdBeforeDate = DateTime.Now;
            this.createdSinceDate = DateTime.Now;
            if (this.vsenvironmenthelper == null)
            {
                this.IsStatusOpenChecked = true;
                this.IsStatusClosedChecked = true;
                this.IsStatusResolvedChecked = true;
                this.IsStatusConfirmedChecked = true;
                this.IsStatusReopenedChecked = true;
                this.IsBlockerChecked = true;
                this.IsCriticalChecked = true;
                this.IsMajaorChecked = true;
                this.IsMinorChecked = true;
                this.IsInfoChecked = true;
                this.IsFalsePositiveChecked = true;
                this.IsRemovedChecked = true;
                this.IsFixedChecked = true;

                return;
            }

            Dictionary<string, string> options =
                this.vsenvironmenthelper.ReadAllOptionsForPluginOptionInApplicationData(IssuesFilterViewModelKey);
            if (options != null && options.Count > 0)
            {
                this.IsStatusOpenChecked = bool.Parse(options["IsStatusOpenChecked"]);
                this.IsStatusClosedChecked = bool.Parse(options["IsStatusClosedChecked"]);
                this.IsStatusResolvedChecked = bool.Parse(options["IsStatusResolvedChecked"]);
                this.IsStatusConfirmedChecked = bool.Parse(options["IsStatusConfirmedChecked"]);
                this.IsStatusReopenedChecked = bool.Parse(options["IsStatusReopenedChecked"]);
                this.IsBlockerChecked = bool.Parse(options["IsBlockerChecked"]);
                this.IsCriticalChecked = bool.Parse(options["IsCriticalChecked"]);
                this.IsMajaorChecked = bool.Parse(options["IsMajaorChecked"]);
                this.IsMinorChecked = bool.Parse(options["IsMinorChecked"]);
                this.IsInfoChecked = bool.Parse(options["IsInfoChecked"]);
                this.IsFalsePositiveChecked = bool.Parse(options["IsFalsePositiveChecked"]);
                this.IsRemovedChecked = bool.Parse(options["IsRemovedChecked"]);
                this.IsFixedChecked = bool.Parse(options["IsFixedChecked"]);
            }
            else
            {
                this.IsStatusOpenChecked = true;
                this.vsenvironmenthelper.WriteOptionInApplicationData(IssuesFilterViewModelKey, "IsStatusOpenChecked", "true");
                this.IsStatusClosedChecked = true;
                this.vsenvironmenthelper.WriteOptionInApplicationData(IssuesFilterViewModelKey, "IsStatusClosedChecked", "true");
                this.IsStatusResolvedChecked = true;
                this.vsenvironmenthelper.WriteOptionInApplicationData(IssuesFilterViewModelKey, "IsStatusResolvedChecked", "true");
                this.IsStatusConfirmedChecked = true;
                this.vsenvironmenthelper.WriteOptionInApplicationData(IssuesFilterViewModelKey, "IsStatusConfirmedChecked", "true");
                this.IsStatusReopenedChecked = true;
                this.vsenvironmenthelper.WriteOptionInApplicationData(IssuesFilterViewModelKey, "IsStatusReopenedChecked", "true");
                this.IsBlockerChecked = true;
                this.vsenvironmenthelper.WriteOptionInApplicationData(IssuesFilterViewModelKey, "IsBlockerChecked", "true");
                this.IsCriticalChecked = true;
                this.vsenvironmenthelper.WriteOptionInApplicationData(IssuesFilterViewModelKey, "IsCriticalChecked", "true");
                this.IsMajaorChecked = true;
                this.vsenvironmenthelper.WriteOptionInApplicationData(IssuesFilterViewModelKey, "IsMajaorChecked", "true");
                this.IsMinorChecked = true;
                this.vsenvironmenthelper.WriteOptionInApplicationData(IssuesFilterViewModelKey, "IsMinorChecked", "true");
                this.IsInfoChecked = true;
                this.vsenvironmenthelper.WriteOptionInApplicationData(IssuesFilterViewModelKey, "IsInfoChecked", "true");
                this.IsFalsePositiveChecked = true;
                this.vsenvironmenthelper.WriteOptionInApplicationData(IssuesFilterViewModelKey, "IsFalsePositiveChecked", "true");
                this.IsRemovedChecked = true;
                this.vsenvironmenthelper.WriteOptionInApplicationData(IssuesFilterViewModelKey, "IsRemovedChecked", "true");
                this.IsFixedChecked = true;
                this.vsenvironmenthelper.WriteOptionInApplicationData(IssuesFilterViewModelKey, "IsFixedChecked", "true");
            }
        }

        #endregion
    }
}