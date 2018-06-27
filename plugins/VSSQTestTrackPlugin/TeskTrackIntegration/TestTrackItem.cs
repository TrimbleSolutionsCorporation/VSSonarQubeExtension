// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestTrackItem.cs" company="Copyright © 2015 jmecsoftware">
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

namespace TestTrackConnector
{
    using System;

    /// <summary>
    /// The test track item.
    /// </summary>
    public class TestTrackItem
    {
        #region Static Fields

        /// <summary>
        /// The test track table columns.
        /// </summary>
        public static CTableColumn[] TestTrackTableColumns = new CTableColumn[14];

        #endregion

        #region Fields

        /// <summary>
        /// The _ date assigned.
        /// </summary>
        private string _DateAssigned = string.Empty;

        /// <summary>
        /// The _ date entered.
        /// </summary>
        private string _DateEntered = string.Empty;

        /// <summary>
        /// The _ date modified.
        /// </summary>
        private string _DateModified = string.Empty;

        /// <summary>
        /// The _ defect reason.
        /// </summary>
        private string _DefectReason = string.Empty;

        /// <summary>
        /// The _ origin.
        /// </summary>
        private string _Origin = string.Empty;

        /// <summary>
        /// The _ priority.
        /// </summary>
        private Priorities _Priority = Priorities.NOT_SET;

        /// <summary>
        /// The _ record id.
        /// </summary>
        private long _RecordId;

        /// <summary>
        /// The _ release blocker.
        /// </summary>
        private ReleaseBlockers _ReleaseBlocker = ReleaseBlockers.NOT_SET;

        /// <summary>
        /// The _ segment.
        /// </summary>
        private string _Segment = string.Empty;

        /// <summary>
        /// The _ status.
        /// </summary>
        private DefectStatuses _Status = DefectStatuses.NOT_SET;

        /// <summary>
        /// The _ summary.
        /// </summary>
        private string _Summary = string.Empty;

        /// <summary>
        /// The _ team.
        /// </summary>
        private string _Team = string.Empty;

        /// <summary>
        /// The _ type.
        /// </summary>
        private string _Type = string.Empty;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TestTrackItem"/> class.
        /// </summary>
        public TestTrackItem()
        {
            TestTrackTableColumns[0] = new CTableColumn { name = "Record ID" };
            TestTrackTableColumns[1] = new CTableColumn { name = "Number" };
            TestTrackTableColumns[2] = new CTableColumn { name = "Dev Priority" };
            TestTrackTableColumns[3] = new CTableColumn { name = "Status" };
            TestTrackTableColumns[4] = new CTableColumn { name = "Currently Assigned To" };
            TestTrackTableColumns[5] = new CTableColumn { name = "Origin" };
            TestTrackTableColumns[6] = new CTableColumn { name = "Dev Team" };
            TestTrackTableColumns[7] = new CTableColumn { name = "Date Found" };
            TestTrackTableColumns[8] = new CTableColumn { name = "Date Modified" };
            TestTrackTableColumns[9] = new CTableColumn { name = "Assign Date" };
            TestTrackTableColumns[10] = new CTableColumn { name = "Summary" };
            TestTrackTableColumns[11] = new CTableColumn { name = "Type" };
            TestTrackTableColumns[12] = new CTableColumn { name = "Release blocker" };
            TestTrackTableColumns[13] = new CTableColumn { name = "Product and Segment" };

            this.Number = -1;
            this.Summary = "test;";
            this.Priority = Priorities.IMMEDIATE;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestTrackItem"/> class.
        /// </summary>
        /// <param name="SoapRow">
        /// The soap row.
        /// </param>
        public TestTrackItem(CRecordRowSoap SoapRow)
        {
            if (SoapRow.row[0].value == null)
            {
                this._RecordId = 0;
            }
            else
            {
                this._RecordId = Convert.ToInt64(SoapRow.row[0].value);
            }

            if (SoapRow.row[1].value == null)
            {
                this.Number = 0;
            }
            else
            {
                this.Number = Convert.ToInt64(SoapRow.row[1].value);
            }

            this._Priority = SoapToTestTrack.GetPriorityFromString(SoapRow.row[2].value);
            this._Status = SoapToTestTrack.GetDefectStatusFromString(SoapRow.row[3].value);

            if (SoapRow.row[5].value == null)
            {
                this._Origin = string.Empty;
            }
            else
            {
                this._Origin = SoapRow.row[5].value;
            }

            if (SoapRow.row[6].value == null)
            {
                this._Team = string.Empty;
            }
            else
            {
                this._Team = SoapRow.row[6].value;
            }

            if (SoapRow.row[7].value == null)
            {
                this._DateEntered = string.Empty;
            }
            else
            {
                this._DateEntered = SoapRow.row[7].value;
            }

            if (SoapRow.row[8].value == null)
            {
                this._DateModified = string.Empty;
            }
            else
            {
                this._DateModified = SoapRow.row[8].value;
            }

            if (SoapRow.row[9].value == null)
            {
                this._DateAssigned = string.Empty;
            }
            else
            {
                this._DateAssigned = SoapRow.row[9].value;
            }

            if (SoapRow.row[10].value == null)
            {
                this._Summary = string.Empty;
            }
            else
            {
                this._Summary = SoapRow.row[10].value;
            }

            if (SoapRow.row[11].value == null)
            {
                this._Type = string.Empty;
            }
            else
            {
                this._Type = SoapRow.row[11].value;
            }

            this._ReleaseBlocker = SoapToTestTrack.GetReleaseBlockerFromString(SoapRow.row[12].value);

            if (SoapRow.row[13].value == null)
            {
                this._Segment = string.Empty;
            }
            else
            {
                this._Segment = SoapRow.row[13].value;
            }
        }

        #endregion

        #region Enums

        /// <summary>
        /// The defect statuses.
        /// </summary>
        public enum DefectStatuses
        {
            /// <summary>
            /// The open.
            /// </summary>
            OPEN = 0, 

            /// <summary>
            /// The ope n_ r e_ opened.
            /// </summary>
            OPEN_RE_OPENED = 1, 

            /// <summary>
            /// The ope n_ verif y_ failed.
            /// </summary>
            OPEN_VERIFY_FAILED = 2, 

            /// <summary>
            /// The fixed.
            /// </summary>
            FIXED = 3, 

            /// <summary>
            /// The closed.
            /// </summary>
            CLOSED = 4, 

            /// <summary>
            /// The close d_ verified.
            /// </summary>
            CLOSED_VERIFIED = 5, 

            /// <summary>
            /// The no t_ set.
            /// </summary>
            NOT_SET = 6 // Error!
        }

        /// <summary>
        /// The priorities.
        /// </summary>
        public enum Priorities
        {
            /// <summary>
            /// The no t_ set.
            /// </summary>
            NOT_SET = 0, 

            /// <summary>
            /// The showstopper.
            /// </summary>
            SHOWSTOPPER = 1, 

            /// <summary>
            /// The immediate.
            /// </summary>
            IMMEDIATE = 2, 

            /// <summary>
            /// The devlis t_ high.
            /// </summary>
            DEVLIST_HIGH = 3, 

            /// <summary>
            /// The devlis t_ med.
            /// </summary>
            DEVLIST_MED = 4, 

            /// <summary>
            /// The devlis t_ low.
            /// </summary>
            DEVLIST_LOW = 5
        }

        /// <summary>
        /// The release blockers.
        /// </summary>
        public enum ReleaseBlockers
        {
            /// <summary>
            /// The no t_ set.
            /// </summary>
            NOT_SET = 0, 

            /// <summary>
            /// The doe s_ no t_ preven t_ an y_ ne w_ release.
            /// </summary>
            DOES_NOT_PREVENT_ANY_NEW_RELEASE = 1, 

            /// <summary>
            /// The prevent s_ alph a 1_ release.
            /// </summary>
            PREVENTS_ALPHA1_RELEASE = 2, 

            /// <summary>
            /// The prevent s_ nex t_ alpha.
            /// </summary>
            PREVENTS_NEXT_ALPHA = 3, 

            /// <summary>
            /// The prevent s_ bet a 1_ release.
            /// </summary>
            PREVENTS_BETA1_RELEASE = 4, 

            /// <summary>
            /// The prevent s_ nex t_ beta.
            /// </summary>
            PREVENTS_NEXT_BETA = 5, 

            /// <summary>
            /// The prevent s_ r c_ release.
            /// </summary>
            PREVENTS_RC_RELEASE = 6, 

            /// <summary>
            /// The s r_ candidate.
            /// </summary>
            SR_CANDIDATE = 7, 

            /// <summary>
            /// The s r_ blocker.
            /// </summary>
            SR_BLOCKER = 8, 

            /// <summary>
            /// The progres s_ versio n_ candidate.
            /// </summary>
            PROGRESS_VERSION_CANDIDATE = 9
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the date assigned.
        /// </summary>
        public string DateAssigned
        {
            get
            {
                return this._DateAssigned;
            }

            set
            {
                this._DateAssigned = value;
            }
        }

        /// <summary>
        /// Gets or sets the date entered.
        /// </summary>
        public string DateEntered
        {
            get
            {
                return this._DateEntered;
            }

            set
            {
                this._DateEntered = value;
            }
        }

        /// <summary>
        /// Gets or sets the date modified.
        /// </summary>
        public string DateModified
        {
            get
            {
                return this._DateModified;
            }

            set
            {
                this._DateModified = value;
            }
        }

        /// <summary>
        /// Gets or sets the defect reason.
        /// </summary>
        public string DefectReason
        {
            get
            {
                return this._DefectReason;
            }

            set
            {
                this._DefectReason = value;
            }
        }

        /// <summary>
        /// Gets or sets the number.
        /// </summary>
        public long Number { get; set; }

        /// <summary>
        /// Gets or sets the origin.
        /// </summary>
        public string Origin
        {
            get
            {
                return this._Origin;
            }

            set
            {
                this._Origin = value;
            }
        }

        /// <summary>
        /// Gets or sets the priority.
        /// </summary>
        public Priorities Priority
        {
            get
            {
                return this._Priority;
            }

            set
            {
                this._Priority = value;
            }
        }

        /// <summary>
        /// Gets or sets the record id.
        /// </summary>
        public long RecordId
        {
            get
            {
                return this._RecordId;
            }

            set
            {
                this._RecordId = value;
            }
        }

        /// <summary>
        /// Gets or sets the release blocker.
        /// </summary>
        public ReleaseBlockers ReleaseBlocker
        {
            get
            {
                return this._ReleaseBlocker;
            }

            set
            {
                this._ReleaseBlocker = value;
            }
        }

        /// <summary>
        /// Gets or sets the segment.
        /// </summary>
        public string Segment
        {
            get
            {
                return this._Segment;
            }

            set
            {
                this._Segment = value;
            }
        }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        public DefectStatuses Status
        {
            get
            {
                return this._Status;
            }

            set
            {
                this._Status = value;
            }
        }

        /// <summary>
        /// Gets or sets the summary.
        /// </summary>
        public string Summary
        {
            get
            {
                return this._Summary;
            }

            set
            {
                this._Summary = value;
            }
        }

        /// <summary>
        /// Gets or sets the team.
        /// </summary>
        public string Team
        {
            get
            {
                return this._Team;
            }

            set
            {
                this._Team = value;
            }
        }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        public string Type
        {
            get
            {
                return this._Type;
            }

            set
            {
                this._Type = value;
            }
        }

        #endregion
    }
}