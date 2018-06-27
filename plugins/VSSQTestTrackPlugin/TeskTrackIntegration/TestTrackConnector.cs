// --------------------------------------------------------------------------------------------------------------------
// <copyright file="testtrackconnector.cs" company="Copyright © 2015 jmecsoftware">
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
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// The test track connector.
    /// </summary>
    public class TestTrackConnector
    {
        /// <summary>
        /// The configuration file
        /// </summary>
        private readonly string configFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "TesttrackSetup.cfg");

        /// <summary>
        /// The execution path
        /// </summary>
        private readonly string executionPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase.Replace("file:///", string.Empty));

        /// <summary>
        /// The connector
        /// </summary>
        private readonly IIssueManagementConnection connector;

        /// <summary>
        /// The user name for creation
        /// </summary>
        private string userNameForCreation;

        /// <summary>
        /// The found in version
        /// </summary>
        private string foundInVersion;

        /// <summary>
        /// The server
        /// </summary>
        private string server;

        /// <summary>
        /// The project
        /// </summary>
        private string project;

        /// <summary>
        /// The user name
        /// </summary>
        private string userName;

        /// <summary>
        /// The password
        /// </summary>
        private string password;

        /// <summary>
        /// The ttstudioaddress
        /// </summary>
        private string ttstudioaddress;

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TestTrackConnector"/> class.
        /// </summary>
        /// <param name="userName">
        /// The user name.
        /// </param>
        /// <param name="password">
        /// The password.
        /// </param>
        public TestTrackConnector(string userName, string password, bool forceConnection, IIssueManagementConnection connector)
        {
            this.connector = connector;

            try
            {
                if (forceConnection)
                {
                    connector.Close();
                }

                if (!connector.IsConnected)
                {
                    this.server = "";
                    this.project = "";
                    
                    if (File.Exists(configFile))
                    {
                        this.GetServerAndProject();
                    }

                    connector.ConnectToProject(this.server, this.project, this.userName, this.password);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }


        public bool AttachCommentToTestTrackItem(long id, string comment)
        {
            this.RefreshConnection(false);

            if (this.connector.IsConnected)
            {
                try
                {
                    return this.connector.AttachComment(id, this.userNameForCreation, comment);
                }
                catch (Exception)
                {
                    this.RefreshConnection(true);
                    this.connector.EnableFormattedTextSupport();
                    return this.connector.AttachComment(id, this.userNameForCreation, comment);
                }                
            }

            return false;
        }

        public void Disconnect()
        {
            if (this.connector.IsConnected)
            {
                this.connector.Close();
            }
        }

        public void RefreshConnection(bool force)
        {
            if (!force)
            {
                if (this.connector.IsConnected)
                {
                    return;
                }
            }

            try
            {
                this.connector.Close();

                if (!this.connector.IsConnected)
                {
                    this.connector.ConnectToProject(this.server, this.project, this.userName, this.password);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }


        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The get my assigned defects.
        /// </summary>
        /// <returns>
        /// The <see>
        ///         <cref>List</cref>
        ///     </see>
        ///     .
        /// </returns>
        public List<TestTrackItem> GetMyAssignedDefects()
        {
            this.RefreshConnection(false);

            if (this.connector.IsConnected)
            {
                return this.connector.GetAssignedItems();
            }

            return new List<TestTrackItem>();
        }

        /// <summary>
        /// The get my assigned defects.
        /// </summary>
        /// <returns>
        /// The <see>
        ///         <cref>List</cref>
        ///     </see>
        ///     .
        /// </returns>
        public List<TestTrackItem> GetLatestDefects()
        {
            this.RefreshConnection(false);
            try
            {
                if (this.connector.IsConnected)
                {
                    return this.connector.GetMostRecentItems();
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return new List<TestTrackItem>();
        }

        public string GetUrlForDefect(long defect)
        {
            return this.ttstudioaddress + "//" + this.project + "/dfct?recordID=" + defect;
        }

        private void GenerateSoapConfigFile(string _ConfigFile)
        {
            var defect = new TtDefect(this.connector, 114329);
            defect.SaveTTItemSoapConfig(_ConfigFile);
            Console.WriteLine("Created config file: " + _ConfigFile);
        }

        private void GenerateCustomFields()
        {
            CField[] customFields = this.connector.GetCustomField("Defect");

            foreach (var item in customFields)
            {
                Debug.WriteLine(" ===== " + item.name);
            }
        }

        public long CreateDefect(string summary, string comments)
        {
            this.RefreshConnection(false);

            if (this.connector.IsConnected)
            {
                try
                {
                    var def = new TtDefect(this.userNameForCreation, this.foundInVersion, summary, comments);
                    this.connector.EnableFormattedTextSupport();
                    return this.connector.CreateDefect(def.GetDefect());
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    this.RefreshConnection(true);
                    this.connector.EnableFormattedTextSupport();
                    var def = new TtDefect(this.userNameForCreation, this.foundInVersion, summary, comments);
                    return this.connector.CreateDefect(def.GetDefect());                    
                }
            }

            return -1;
        }


        private string ReadFromFile(string value)
        {
            if (!File.Exists(value))
                throw new FileNotFoundException("Comment file \"" + value + "\" was not found.");

            string commentstring = "";
            try
            {
                using (StreamReader sr = new StreamReader(value))
                {
                    commentstring = sr.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error while reading comment file \"" + value + "\". " + ex.Message);
            }
            return commentstring;
        }

        /// <summary>
        /// Gets the defect.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public TestTrackItem GetDefect(long item)
        {
            this.RefreshConnection(false);

            if (this.connector.IsConnected)
            {
                var elem = this.connector.getDefect(item);

                TestTrackItem tt = new TestTrackItem();
                if (elem != null)
                {
                    tt.Summary = elem.summary;
                    tt.Status = SoapToTestTrack.GetDefectStatusFromString(elem.state);
                }
                else
                {
                    tt.Summary = "Item was not properly returned from server";
                    tt.Status = TestTrackItem.DefectStatuses.NOT_SET;

                }

                return tt;
            }

            return null;           
        }

        /// <summary>
        /// Gets the server and project.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <param name="project">The project.</param>
        private void GetServerAndProject()
        {
            foreach (var line in File.ReadAllLines(this.configFile))
            {

                var elems = line.Split(';');

                if (line.Trim().StartsWith("UserName;"))
                {
                    try
                    {
                        this.userName = elems[1].Trim();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }

                if (line.Trim().StartsWith("Password"))
                {
                    try
                    {
                        this.password = elems[1].Trim();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }

                if (line.Trim().StartsWith("Server"))
                {
                    try
                    {
                        this.server = elems[1].Trim();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }

                if (line.Trim().StartsWith("TTStudio"))
                {
                    try
                    {
                        this.ttstudioaddress = elems[1].Trim();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }



                if (line.Trim().StartsWith("Project"))
                {
                    try
                    {
                        this.project = elems[1].Trim();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }

                if (line.Trim().StartsWith("FoundInVersion"))
                {
                    try
                    {
                        this.foundInVersion = elems[1].Trim();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                        this.foundInVersion = "Work";
                    }
                }

                if (line.Trim().StartsWith("UserNameToUseForIssueCreation"))
                {
                    try
                    {
                        this.userNameForCreation = elems[1].Trim();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                        project = "Work";
                    }
                }
            }
        }

        #endregion
    }
}