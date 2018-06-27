// --------------------------------------------------------------------------------------------------------------------
// <copyright file="soaptotesttrack.cs" company="Copyright © 2015 jmecsoftware">
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
    using System.Collections.Generic;
    using System.Linq;

    class SoapToTestTrack
    {

        #region Static functions from ttsoapcgi to WasteMeasuring classes

        /// <summary>
        /// Returns the TestTrackItem.Priorities information from a CDefect from the TT soap api.
        /// </summary>
        /// <param name="SoapDefect">Defect from TT soap api</param>
        public static TestTrackItem.Priorities GetPriorityFromString(string StringPriority)
        {
            TestTrackItem.Priorities PriorityValue = TestTrackItem.Priorities.NOT_SET;

            if (StringPriority == null)
                PriorityValue = TestTrackItem.Priorities.NOT_SET;
            else if (StringPriority.Contains("ShowStopper"))
                PriorityValue = TestTrackItem.Priorities.SHOWSTOPPER;
            else if (StringPriority.Contains("Immediate"))
                PriorityValue = TestTrackItem.Priorities.IMMEDIATE;
            else if (StringPriority.Contains("High"))
                PriorityValue = TestTrackItem.Priorities.DEVLIST_HIGH;
            else if (StringPriority.Contains("Medium"))
                PriorityValue = TestTrackItem.Priorities.DEVLIST_MED;
            else if (StringPriority.Contains("Low"))
                PriorityValue = TestTrackItem.Priorities.DEVLIST_LOW;

            return PriorityValue;
        }

        /// <summary>
        /// Returns the TestTrackItem.DefectStatuses information from a CDefect from the TT soap api.
        /// </summary>
        /// <param name="SoapDefect">Defect from TT soap api</param>
        public static TestTrackItem.DefectStatuses GetDefectStatusFromString(string StatusString)
        {
            TestTrackItem.DefectStatuses StatusValue = TestTrackItem.DefectStatuses.NOT_SET;

            if (StatusString == null)
            {
                StatusValue = TestTrackItem.DefectStatuses.NOT_SET;
            }
            else if (StatusString.Contains("Open"))
            {
                if (StatusString.Contains("Verify failed"))
                    StatusValue = TestTrackItem.DefectStatuses.OPEN_VERIFY_FAILED;
                else if (StatusString.Contains("Re-Opened"))
                    StatusValue = TestTrackItem.DefectStatuses.OPEN_RE_OPENED;
                else
                    StatusValue = TestTrackItem.DefectStatuses.OPEN;
            }
            else if (StatusString.Contains("Fixed"))
            {
                StatusValue = TestTrackItem.DefectStatuses.FIXED;
            }
            else if (StatusString.Contains("Closed"))
            {
                if (StatusString.Contains("Verified"))
                    StatusValue = TestTrackItem.DefectStatuses.CLOSED_VERIFIED;
                else
                    StatusValue = TestTrackItem.DefectStatuses.CLOSED;

            }

            return StatusValue;

        }

        /// <summary>
        /// Returns the TestTrackItem.ReleaseBlockers information from a CDefect from the TT soap api.
        /// </summary>
        /// <param name="SoapDefect">Defect from TT soap api</param>
        public static TestTrackItem.ReleaseBlockers GetReleaseBlockerFromString(string ReleaseBlocker)
        {
            TestTrackItem.ReleaseBlockers ReleaseBlockerValue = TestTrackItem.ReleaseBlockers.NOT_SET;

            if (ReleaseBlocker == null)
                ReleaseBlockerValue = TestTrackItem.ReleaseBlockers.NOT_SET;
            else if (ReleaseBlocker.Equals("Does not prevent any new release"))
                ReleaseBlockerValue = TestTrackItem.ReleaseBlockers.DOES_NOT_PREVENT_ANY_NEW_RELEASE;
            else if (ReleaseBlocker.Equals("Prevents Alpha 1 release"))
                ReleaseBlockerValue = TestTrackItem.ReleaseBlockers.PREVENTS_ALPHA1_RELEASE;
            else if (ReleaseBlocker.Equals("Prevents next Alpha"))
                ReleaseBlockerValue = TestTrackItem.ReleaseBlockers.PREVENTS_NEXT_ALPHA;
            else if (ReleaseBlocker.Equals("Prevents Beta 1 release"))
                ReleaseBlockerValue = TestTrackItem.ReleaseBlockers.PREVENTS_BETA1_RELEASE;
            else if (ReleaseBlocker.Equals("Prevents next Beta"))
                ReleaseBlockerValue = TestTrackItem.ReleaseBlockers.PREVENTS_NEXT_BETA;
            else if (ReleaseBlocker.Equals("Prevents RC release"))
                ReleaseBlockerValue = TestTrackItem.ReleaseBlockers.PREVENTS_RC_RELEASE;
            else if (ReleaseBlocker.Equals("SR candidates"))
                ReleaseBlockerValue = TestTrackItem.ReleaseBlockers.SR_CANDIDATE;
            else if (ReleaseBlocker.Equals("SR blocker"))
                ReleaseBlockerValue = TestTrackItem.ReleaseBlockers.SR_BLOCKER;
            else if (ReleaseBlocker.Equals("Progress version candidate"))
                ReleaseBlockerValue = TestTrackItem.ReleaseBlockers.PROGRESS_VERSION_CANDIDATE;


            return ReleaseBlockerValue;
        }

        /// <summary>
        /// Creates a TestTrackItem list from a list of records from the TT soap api.
        /// </summary>
        /// <param name="SoapItemList">List of CRecords from TT soap api.</param>
        /// <returns></returns>
        public static List<TestTrackItem> RecordSoapTableToItemsList(CRecordListSoap SoapItemList)
        {
            List<TestTrackItem> ttItemsList = new List<TestTrackItem>();

            if (SoapItemList.records.Count() > 0)
            {

                foreach (CRecordRowSoap SoapRow in SoapItemList.records)
                {
                    ttItemsList.Add(new TestTrackItem(SoapRow));
                }
            }

            return ttItemsList;
        }

        #endregion

    }
}
