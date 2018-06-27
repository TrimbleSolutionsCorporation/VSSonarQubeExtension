// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TTConnection.cs" company="Copyright © 2015 jmecsoftware">
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
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;

    /*  TT Error codes 
        0 	Unknown error
        1 	Invalid database
        2 	Missing parameter
        5 	Not allowed
        6 	Record not found
        7 	Edit record not found
        8 	User already logged in
        9 	Server not found
        10 	Communication error
        11 	Invalid parameter
        12 	Database error
        13 	Max concurrent users
        16 	General error
        17 	Not logged in
        19 	No license found
        21 	License in use
        22 	Record locked
        23 	Data validation error
        72 	Max SOAP users
        74 	Multiple records found
        75 	Admin lock not obtained
        77 	Invalid table
        86 	Duplicate name
        108 	Inactive user
        127 	Communication error with license server
        135 	Not allowed license server
        140 	No license type assigned
        148 	Password change required
        237 	Workflow not defined
        239 	Audit logging enabled
     */

    /// <summary>
    ///     The tt connection.
    /// </summary>
    public class TtConnection : IIssueManagementConnection, IDisposable
    {
        #region Static Fields

        bool disposed = false;

        #endregion

        #region Fields

        /// <summary>
        ///     The tt cookie.
        /// </summary>
        private long ttCookie;

        /// <summary>
        ///     The tt soap.
        /// </summary>
        private ttsoapcgi ttSOAP;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Prevents a default instance of the <see cref="TtConnection" /> class from being created.
        /// </summary>
        public TtConnection()
        {
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the instance.
        /// </summary>
        public ttsoapcgi Instance
        {
            get
            {
                return this.ttSOAP;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether is connected.
        /// </summary>
        public bool IsConnected
        {
            get
            {
                return this.ttCookie != 0;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     The close.
        /// </summary>
        public void Close()
        {
            if (this.ttCookie > 0)
            {
                try
                {
                    if (this.ttSOAP != null)
                    {
                        this.ttSOAP.DatabaseLogoff(this.ttCookie);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error during logging off from TT: " + ex);
                }
                finally
                {
                    this.ttSOAP = null;
                }

                this.ttCookie = 0;
            }
        }

        public void Dispose()
        {
            this.ttSOAP.Dispose();
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void IDisposableDispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                this.Dispose();
            }

            disposed = true;
        }

        public List<TestTrackItem> GetMostRecentItems()
        {
            var testTrackTableColumns = new CTableColumn[14];
            testTrackTableColumns[0] = new CTableColumn();
            testTrackTableColumns[0].name = "Record ID";
            testTrackTableColumns[1] = new CTableColumn();
            testTrackTableColumns[1].name = "Number";
            testTrackTableColumns[2] = new CTableColumn();
            testTrackTableColumns[2].name = "Dev Priority";
            testTrackTableColumns[3] = new CTableColumn();
            testTrackTableColumns[3].name = "Status";
            testTrackTableColumns[4] = new CTableColumn();
            testTrackTableColumns[4].name = "Currently Assigned To";
            testTrackTableColumns[5] = new CTableColumn();
            testTrackTableColumns[5].name = "Origin";
            testTrackTableColumns[6] = new CTableColumn();
            testTrackTableColumns[6].name = "Dev Team";
            testTrackTableColumns[7] = new CTableColumn();
            testTrackTableColumns[7].name = "Date Found";
            testTrackTableColumns[8] = new CTableColumn();
            testTrackTableColumns[8].name = "Date Modified";
            testTrackTableColumns[9] = new CTableColumn();
            testTrackTableColumns[9].name = "Assign Date";
            testTrackTableColumns[10] = new CTableColumn();
            testTrackTableColumns[10].name = "Summary";
            testTrackTableColumns[11] = new CTableColumn();
            testTrackTableColumns[11].name = "Type";
            testTrackTableColumns[12] = new CTableColumn();
            testTrackTableColumns[12].name = "Release blocker";
            testTrackTableColumns[13] = new CTableColumn();
            testTrackTableColumns[13].name = "Product and Segment";
            return
                SoapToTestTrack.RecordSoapTableToItemsList(
                    this.ttSOAP.getRecordListForTable(this.ttCookie, "Defect", "05 Reported in the last 15 days", testTrackTableColumns));
        }

        /// <summary>
        /// Gets the custom field.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns></returns>
        public CField[] GetCustomField(string field)
        {
            return this.ttSOAP.getCustomFieldsDefinitionList(this.ttCookie, field);
        }

        /// <summary>
        /// The connect.
        /// </summary>
        /// <param name="SOAPURL">
        /// The soapurl.
        /// </param>
        /// <param name="Project">
        /// The project.
        /// </param>
        /// <param name="UserName">
        /// The user name.
        /// </param>
        /// <param name="UserPass">
        /// The user pass.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool Connect(string SOAPURL, string Project, string UserName, string UserPass)
        {
            bool ttConnectionEstablished = false;
            if (this.ttCookie == 0)
            {
                ttConnectionEstablished = this.ConnectToProject(SOAPURL, Project, UserName, UserPass);
                if (ttConnectionEstablished)
                {
                    Debug.WriteLine("Logged in as " + UserName + " to " + Project + "@" + SOAPURL);
                }
            }
            else
            {
                ttConnectionEstablished = true;
            }

            return ttConnectionEstablished;
        }

        /// <summary>
        /// The connect to project.
        /// </summary>
        /// <param name="ttSOAPURL">
        /// The tt soapurl.
        /// </param>
        /// <param name="ttProject">
        /// The tt project.
        /// </param>
        /// <param name="ttUserName">
        /// The tt user name.
        /// </param>
        /// <param name="ttUserPass">
        /// The tt user pass.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        /// <exception cref="Exception">
        /// </exception>
        public bool ConnectToProject(string ttSOAPURL, string ttProject, string ttUserName, string ttUserPass)
        {
            if (ttProject == string.Empty)
            {
                throw new Exception("You need to select a project to connect to");
            }

            try
            {
                if (this.ttSOAP == null)
                {
                    this.ttSOAP = new ttsoapcgi(ttSOAPURL);
                }

                if (this.ttCookie > 0)
                {
                    this.ttSOAP.DatabaseLogoff(this.ttCookie);
                    this.ttCookie = 0;
                }

                if (string.IsNullOrEmpty(ttUserName))
                {
                    this.ttCookie = this.ttSOAP.DatabaseLogon(ttProject, "viewer", "viewer");
                }
                else
                {
                    this.ttCookie = this.ttSOAP.DatabaseLogon(ttProject, ttUserName, ttUserPass);
                }

                
                if (this.ttCookie == 0)
                {
                    throw new Exception("Unknown error.");
                }
            }
            catch (Exception fcc)
            {
                throw new Exception("Unable to connect to Soap Server or Project.\n" + fcc.Message, fcc);
            }

            return true;
        }

        /// <summary>
        ///     The disable formatted text support.
        /// </summary>
        public void DisableFormattedTextSupport()
        {
            this.ttSOAP.formattedTextSupport(this.ttCookie, false);
        }

        /// <summary>
        ///     The enable formatted text support.
        /// </summary>
        public void EnableFormattedTextSupport()
        {
            this.ttSOAP.formattedTextSupport(this.ttCookie, true);
        }

        /// <summary>
        /// The get project list.
        /// </summary>
        /// <param name="ttSOAPURL">
        /// The tt soapurl.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public List<string> GetProjectList(string ttSOAPURL)
        {
            var result = new List<string>();

            if (this.ttSOAP == null)
            {
                this.ttSOAP = new ttsoapcgi(ttSOAPURL);
            }

            CDatabase[] dbList = this.ttSOAP.getDatabaseList();
            if (dbList != null)
            {
                foreach (CDatabase db in dbList)
                {
                    result.Add(db.name);
                }
            }

            return result;
        }

        /// <summary>
        /// The create defect.
        /// </summary>
        /// <param name="Defect">
        /// The defect.
        /// </param>
        /// <returns>
        /// The <see cref="long"/>.
        /// </returns>
        public long CreateDefect(CDefect Defect)
        {
            return this.ttSOAP.addDefect(this.ttCookie, Defect);
        }

        /// <summary>
        ///     The get assigned items.
        /// </summary>
        /// <returns>
        ///     The <see cref="CDefect[]" />.
        /// </returns>
        public List<TestTrackItem> GetAssignedItems()
        {
            var testTrackTableColumns = new CTableColumn[14];
            testTrackTableColumns[0] = new CTableColumn();
            testTrackTableColumns[0].name = "Record ID";
            testTrackTableColumns[1] = new CTableColumn();
            testTrackTableColumns[1].name = "Number";
            testTrackTableColumns[2] = new CTableColumn();
            testTrackTableColumns[2].name = "Dev Priority";
            testTrackTableColumns[3] = new CTableColumn();
            testTrackTableColumns[3].name = "Status";
            testTrackTableColumns[4] = new CTableColumn();
            testTrackTableColumns[4].name = "Currently Assigned To";
            testTrackTableColumns[5] = new CTableColumn();
            testTrackTableColumns[5].name = "Origin";
            testTrackTableColumns[6] = new CTableColumn();
            testTrackTableColumns[6].name = "Dev Team";
            testTrackTableColumns[7] = new CTableColumn();
            testTrackTableColumns[7].name = "Date Found";
            testTrackTableColumns[8] = new CTableColumn();
            testTrackTableColumns[8].name = "Date Modified";
            testTrackTableColumns[9] = new CTableColumn();
            testTrackTableColumns[9].name = "Assign Date";
            testTrackTableColumns[10] = new CTableColumn();
            testTrackTableColumns[10].name = "Summary";
            testTrackTableColumns[11] = new CTableColumn();
            testTrackTableColumns[11].name = "Type";
            testTrackTableColumns[12] = new CTableColumn();
            testTrackTableColumns[12].name = "Release blocker";
            testTrackTableColumns[13] = new CTableColumn();
            testTrackTableColumns[13].name = "Product and Segment";
            return
                SoapToTestTrack.RecordSoapTableToItemsList(
                    this.ttSOAP.getRecordListForTable(this.ttCookie, "Defect", "10 AssignedToMe_All", testTrackTableColumns));
        }

        /// <summary>
        /// The get defect.
        /// </summary>
        /// <param name="defectNumber">
        /// The defect number.
        /// </param>
        /// <returns>
        /// The <see cref="CDefect"/>.
        /// </returns>
        public CDefect getDefect(long defectNumber)
        {
            return this.ttSOAP.getDefect(this.ttCookie, defectNumber, string.Empty, false);
        }

        /// <summary>
        /// The get defect by record id.
        /// </summary>
        /// <param name="recordId">
        /// The record id.
        /// </param>
        /// <returns>
        /// The <see cref="CDefect"/>.
        /// </returns>
        public CDefect getDefectByRecordID(long recordId)
        {
            return this.ttSOAP.getDefectByRecordID(this.ttCookie, recordId, false);
        }

        /// <summary>
        /// The get links for defect.
        /// </summary>
        /// <param name="defectRecordId">
        /// The defect record id.
        /// </param>
        /// <returns>
        /// The <see cref="CLink[]"/>.
        /// </returns>
        public CLink[] getLinksForDefect(long defectRecordId)
        {
            return this.ttSOAP.getLinksForDefect(this.ttCookie, defectRecordId);
        }

        public void ReOpenDefect(long id, string userName, string comment)
        {
            CDefect def = this.ttSOAP.editDefect(this.ttCookie, id, "", true);

            try
            {
                CEvent de = CreateReopenEvent(userName);

                this.AppendEvent(def, de);

                this.ttSOAP.saveDefect(this.ttCookie, def);
            }
            catch (Exception)
            {
                this.ttSOAP.cancelSaveDefect(this.ttCookie, def.recordid);
            }
        }

        public bool AttachComment(long id, string userName, string comment)
        {
            CDefect def = this.ttSOAP.editDefect(this.ttCookie, id, "", true);

            try
            {
                if (def.state.Equals("Closed"))
                {
                    this.AppendEvent(def, this.CreateReopenEvent(userName));
                }

                this.AppendEvent(def, this.CreateCommentEvent(userName, comment));
                this.ttSOAP.saveDefect(this.ttCookie, def);
                return true;
            }
            catch (Exception)
            {
                this.ttSOAP.cancelSaveDefect(this.ttCookie, def.recordid);
                return false;
            }
        }

        private CEvent CreateCommentEvent(string userName, string comment)
        {
            // Create the Fix event.
            CEvent de = new CEvent();
            de.name = "Comment";
            de.user = userName;
            de.date = DateTime.Now;
            de.notes = comment;
            return de;
        }

        private void AppendEvent(CDefect def, CEvent de)
        {
            // Add the event to defect's eventlist.
            if (def.eventlist == null || def.eventlist.Length == 0)
            {  // No events, so we can just create a new list.
                def.eventlist = new CEvent[1];
                def.eventlist[0] = de;
            }
            else
            {  // Append new event to end of existing list.
                ArrayList list = new ArrayList();
                for (int i = 0; i < def.eventlist.Length; ++i)
                    list.Add(def.eventlist[i]);

                // add new event to list
                list.Add(de);

                def.eventlist = new CEvent[def.eventlist.Length + 1];

                for (int i = 0; i < list.Count; ++i)
                    def.eventlist[i] = (CEvent)list[i];
            }
        }

        private CEvent CreateReopenEvent(string userName)
        {
            // Create the Fix event.
            CEvent de = new CEvent();
            de.name = "Re-Open";
            de.resultingstate = "Open";
            de.user = userName;
            de.date = DateTime.Now;
            de.notes = "";
            return de;
        }

        #endregion
    }
}