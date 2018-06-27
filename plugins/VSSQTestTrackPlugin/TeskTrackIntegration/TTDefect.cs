// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TTDefect.cs" company="Copyright © 2015 jmecsoftware">
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
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Soap;
    using System.Xml.Linq;

    /// <summary>
    ///     The tt defect.
    /// </summary>
    public class TtDefect
    {
        #region Fields

        /// <summary>
        /// The execution path
        /// </summary>
        private readonly string executionPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase.Replace("file:///", string.Empty));

        /// <summary>
        ///     The _ attached files.
        /// </summary>
        private readonly List<CFileAttachment> attachedFiles = new List<CFileAttachment>();

        /// <summary>
        ///     The _ defect.
        /// </summary>
        private CDefect defect;

        /// <summary>
        /// The time d
        /// </summary>
        private CDefect timeD;
        private readonly string externalConfigFile;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TtDefect"/> class.
        /// </summary>
        /// <param name="connection">
        /// The connection.
        /// </param>
        /// <param name="TTNumber">
        /// The tt number.
        /// </param>
        public TtDefect(IIssueManagementConnection connection, long TTNumber)
        {
            connection.EnableFormattedTextSupport();
            this.defect = connection.getDefect(TTNumber);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TtDefect"/> class.
        /// </summary>
        /// <param name="DefaultConfigFile">
        /// The default config file.
        /// </param>
        public TtDefect(string DefaultConfigFile)
        {
            this.defect = ReadTTItemSoapConfig(DefaultConfigFile);
            this.SetDates();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TtDefect"/> class.
        /// </summary>
        /// <param name="DefaultConfigFile">
        /// The default config file.
        /// </param>
        /// <param name="username">
        /// The username.
        /// </param>
        /// <param name="foundinversion">
        /// The foundinversion.
        /// </param>
        /// <param name="summary">
        /// The summary.
        /// </param>
        /// <param name="comments">
        /// The comments.
        /// </param>
        public TtDefect(string DefaultConfigFile, string username, string foundinversion, string summary, string comments)
            : this(DefaultConfigFile)
        {
            this.SetCustomValues(username, summary);
            this.AddReportedByRecord(username, foundinversion, comments);
            this.SetDates();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TtDefect"/> class.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="foundinversion">The foundinversion.</param>
        /// <param name="summary">The summary.</param>
        /// <param name="comments">The comments.</param>
        /// <param name="getHomeFile">if set to <c>true</c> [get home file].</param>
        /// <param name="configFile">The configuration file.</param>
        public TtDefect(string username, string foundinversion, string summary, string comments, string configFile, bool getHomeFile = true)
        {
            this.externalConfigFile = configFile;
            CreteDefaultDefect(username, foundinversion, summary, comments, getHomeFile);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TtDefect"/> class.
        /// </summary>
        /// <param name="username">
        /// The username.
        /// </param>
        /// <param name="foundinversion">
        /// The foundinversion.
        /// </param>
        /// <param name="summary">
        /// The summary.
        /// </param>
        /// <param name="comments">
        /// The comments.
        /// </param>
        public TtDefect(string username, string foundinversion, string summary, string comments, bool getHomeFile = true)
        {
            CreteDefaultDefect(username, foundinversion, summary, comments, getHomeFile);
        }

        /// <summary>
        /// Cretes the default defect.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="foundinversion">The foundinversion.</param>
        /// <param name="summary">The summary.</param>
        /// <param name="comments">The comments.</param>
        /// <param name="getHomeFile">if set to <c>true</c> [get home file].</param>
        private void CreteDefaultDefect(string username, string foundinversion, string summary, string comments, bool getHomeFile)
        {
            this.defect = new CDefect();
            this.SetCustomValues(username, summary);

            this.AddReportedByRecord(username, foundinversion, comments);
            this.SetDates();

            this.defect.eventlist = new CEvent[0];
            this.defect.pSCCFileList = new CSCCFileRecord[0];
            var customfields = new List<CField>();
            var homeEnvironment = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            if (File.Exists(this.externalConfigFile))
            {
                CreateRecordFromConfigurationFile(
                    File.ReadAllLines(this.externalConfigFile),
                    this.defect,
                    customfields);
            }
            else
            {
                if (File.Exists(Path.Combine(homeEnvironment, "TesttrackSetup.cfg")) && getHomeFile)
                {

                    CreateRecordFromConfigurationFile(
                        File.ReadAllLines(Path.Combine(homeEnvironment, "TesttrackSetup.cfg")),
                        this.defect,
                        customfields);
                }
            }

            this.defect.customFieldList = customfields.ToArray();

            this.timeD = CreateDefaultCDefect(username, foundinversion, summary, comments);
        }

        private CDefect CreateDefaultCDefect(string username, string foundinversion, string summary, string comments)
        {
            var defectInd = new CDefect();

            defectInd.enteredby = username;
            defectInd.summary = summary;
            defectInd.createdbyuser = username;
            defectInd.modifiedbyuser = username;


            defectInd.type = "Incorrect functionality";
            defectInd.product = "Product Development (for PD use only)";
            defectInd.severity = "Automated Testing";
            defectInd.state = "Open";
            defectInd.disposition = "Bat";

            defectInd.reportedbylist = new CReportedByRecord[1];
            defectInd.reportedbylist[0] = new CReportedByRecord();
            defectInd.reportedbylist[0].foundby = username;
            defectInd.reportedbylist[0].foundinversion = foundinversion;
            defectInd.reportedbylist[0].comments = comments;

            defectInd.dateentered = DateTime.Now;
            defectInd.dateenteredSpecified = true;
            defectInd.datetimecreated = DateTime.Now;
            defectInd.datetimecreatedSpecified = true;
            defectInd.datetimemodified = DateTime.Now;
            defectInd.datetimemodifiedSpecified = true;
            defectInd.reportedbylist[0].datefound = DateTime.Now;
            defectInd.reportedbylist[0].datefoundSpecified = true;

            defectInd.eventlist = new CEvent[0];
            defectInd.pSCCFileList = new CSCCFileRecord[0];

            var customfields = new List<CField>();

            // ===== Maintenance
            AddCustomStringField(customfields, "Maintenance", string.Empty);
            // ===== Defective since v.
            AddCustomDropdownFieldField(customfields, "Defective since v.", "Work");
            // ===== ProjectPlan
            AddCustomDropdownFieldField(customfields, "ProjectPlan", string.Empty);
            // ===== Work order
            AddCustomStringField(customfields, "Work order", string.Empty);
            // ===== Release blocker
            AddCustomDropdownFieldField(customfields, "Release blocker", string.Empty);
            // ===== Defect reason
            AddCustomDropdownFieldField(customfields, "Defect reason", string.Empty);
            // ===== SubTeam
            AddCustomDropdownFieldField(customfields, "SubTeam", string.Empty);
            // ===== Reproduced
            AddCustomDropdownFieldField(customfields, "Reproduced", "Always");
            // ===== Maintenance - Area priorities
            AddCustomStringField(customfields, "Maintenance - Area priorities", string.Empty);
            // ===== Component
            AddCustomDropdownFieldField(customfields, "Component", "Drawings");
            // ===== SubComponent
            AddCustomDropdownFieldField(customfields, "SubComponent", "Other");
            // ===== Effect on usage
            AddCustomDropdownFieldField(customfields, "Effect on usage", "No workaround, work discontinued");
            // ===== Severity
            AddCustomDropdownFieldField(customfields, "Severity", string.Empty);

            defectInd.customFieldList = customfields.ToArray();

            return defectInd;
        }

        public static void CreateRecordFromConfigurationFile(string[] lines, CDefect defect, List<CField> customfields)
        {
            foreach (var line in lines)
            {
                if (line.Trim().StartsWith("//"))
                {
                    continue;
                }

                var elems = line.Split(';');

                if (line.Trim().StartsWith("CustomStringField"))
                {
                    try
                    {
                        AddCustomStringField(customfields, elems[1].Trim(), elems[2].Trim());
                    }
                    catch (Exception)
                    {
                        AddCustomStringField(customfields, elems[1].Trim(), string.Empty);
                    }

                    continue;
                }

                if (line.Trim().StartsWith("CustomIntField"))
                {
                    try
                    {
                        AddCustomIntField(customfields, elems[1].Trim(), int.Parse(elems[2].Trim()));
                    }
                    catch (Exception)
                    {
                        AddCustomIntField(customfields, elems[1].Trim(), 0);
                    }

                    continue;
                }

                if (line.Trim().StartsWith("CustomDropdownField"))
                {
                    try
                    {
                        AddCustomDropdownFieldField(customfields, elems[1].Trim(), elems[2].Trim());
                    }
                    catch (Exception)
                    {
                        AddCustomDropdownFieldField(customfields, elems[1].Trim(), string.Empty);
                    }

                    continue;
                }

                if (line.Trim().StartsWith("Type"))
                {
                    defect.type = elems[1].Trim();
                    continue;
                }

                if (line.Trim().StartsWith("Product"))
                {
                    defect.product = elems[1].Trim();
                    continue;
                }


                if (line.Trim().StartsWith("Severity"))
                {
                    defect.severity = elems[1].Trim();
                    continue;
                }

                if (line.Trim().StartsWith("State"))
                {
                    defect.state = elems[1].Trim();
                    continue;
                }

                if (line.Trim().StartsWith("Disposition"))
                {
                    try
                    {
                        defect.disposition = elems[1].Trim();
                    }
                    catch (Exception)
                    {
                        defect.disposition = string.Empty;
                    }

                    continue;
                }

            }
        }

        #endregion

        #region Public Methods and Operators

        public CDefect GetDefect()
        {
            return this.defect;
        }

        /// <summary>
        /// The read tt item soap config.
        /// </summary>
        /// <param name="ConfigFile">
        /// The config file.
        /// </param>
        /// <returns>
        /// The <see cref="CDefect"/>.
        /// </returns>
        public static CDefect ReadTTItemSoapConfig(string ConfigFile)
        {
            SerializableCDefect defect = null;

            using (Stream stream = File.Open(ConfigFile, FileMode.Open, FileAccess.Read))
            {
                IFormatter formatter = new SoapFormatter();
                defect = (SerializableCDefect)formatter.Deserialize(stream);
            }

            return defect != null ? defect.ToCDefect() : null;
        }

        // Optionally add attachments, such as screenshots, and create the attachment list array to add the file to the attachment list.
        /// <summary>
        /// The attach file.
        /// </summary>
        /// <param name="filename">
        /// The filename.
        /// </param>
        /// <exception cref="Exception">
        /// </exception>
        public void AttachFile(string filename)
        {
            try
            {
                var file = new CFileAttachment();
                file.mstrFileName = filename;
                using (var fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
                {
                    var reader = new BinaryReader(fs);
                    file.mpFileData = reader.ReadBytes((int)fs.Length);
                    reader.Close();
                }

                this.attachedFiles.Add(file);
            }
            catch (Exception ex)
            {
                throw new Exception("Attaching file \"" + filename + "\" didn't work. " + ex.Message);
            }
        }

        /// <summary>
        /// The create in test track.
        /// </summary>
        /// <param name="connection">
        /// The connection.
        /// </param>
        /// <returns>
        /// The <see cref="long"/>.
        /// </returns>
        /// <exception cref="Exception">
        /// </exception>
        public long CreateInTestTrack(TtConnection connection)
        {
            try
            {
                connection.EnableFormattedTextSupport();
                this.defect.reportedbylist[0].attachmentlist = this.attachedFiles.ToArray();
                return connection.CreateDefect(this.defect);
            }
            catch (Exception ex)
            {
                throw new Exception("Error during item creation : " + ex.Message, ex);
            }
        }

        /// <summary>
        /// The read tt item data.
        /// </summary>
        /// <param name="DataFile">
        /// The data file.
        /// </param>
        /// <exception cref="Exception">
        /// </exception>
        public void ReadTTItemData(string DataFile)
        {
            try
            {
                XDocument doc = XDocument.Load(DataFile);

                IEnumerable<XElement> elements = doc.Root.Elements();

                foreach (XElement element in elements)
                {
                    string value = element.Value;

                    if (element.Elements().Count<object>() == 0)
                    {
                        Console.WriteLine(element.Name.ToString().PadRight(25) + ": " + value);
                    }

                    switch (element.Name.ToString().ToLower())
                    {
                        case "summary":
                            this.defect.summary = value;
                            break;
                        case "disposition":
                            this.defect.disposition = value;
                            break;
                        case "type":
                            this.defect.type = value;
                            break;
                        case "product":
                            this.defect.product = value;
                            break;
                        case "severity":
                            this.defect.severity = value;
                            break;
                        case "user":
                            this.defect.enteredby = value;
                            this.defect.createdbyuser = value;
                            this.defect.modifiedbyuser = value;
                            this.defect.reportedbylist[0].foundby = value;
                            break;
                        case "foundinversion":
                            this.defect.reportedbylist[0].foundinversion = value;
                            break;
                        case "comments":
                            this.defect.reportedbylist[0].comments = value;
                            break;
                        case "commentfile":
                            this.defect.reportedbylist[0].comments = this.ReadFromFile(value);
                            break;
                        case "component":
                            this.SetCustomFieldValue("Component", value);
                            break;
                        case "reproduced":
                            this.SetCustomFieldValue("Reproduced", value);
                            break;
                        case "effect":
                            this.SetCustomFieldValue("Effect on usage", value);
                            break;
                        case "defectivesince":
                            this.SetCustomFieldValue("Defective since v.", value);
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error while reading xml data from file \"" + DataFile + "\". " + ex.Message);
            }
        }

        /// <summary>
        /// The save tt item soap config.
        /// </summary>
        /// <param name="ConfigFile">
        /// The config file.
        /// </param>
        public void SaveTTItemSoapConfig(string ConfigFile)
        {
            var defect = new SerializableCDefect(this.defect);

            using (Stream stream = File.Open(ConfigFile, FileMode.Create))
            {
                IFormatter formatter = new SoapFormatter();
                formatter.Serialize(stream, defect);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The add custom dropdown field field.
        /// </summary>
        /// <param name="customfields">
        /// The customfields.
        /// </param>
        /// <param name="Name">
        /// The name.
        /// </param>
        /// <param name="Value">
        /// The value.
        /// </param>
        private static void AddCustomDropdownFieldField(List<CField> customfields, string Name, string Value)
        {
            var Field = new CDropdownField();
            Field.name = Name;
            Field.value = Value;
            customfields.Add(Field);
        }

        /// <summary>
        /// The add custom int field.
        /// </summary>
        /// <param name="customfields">
        /// The customfields.
        /// </param>
        /// <param name="Name">
        /// The name.
        /// </param>
        /// <param name="Value">
        /// The value.
        /// </param>
        private static void AddCustomIntField(List<CField> customfields, string Name, int Value)
        {
            var Field = new CIntegerField();
            Field.name = Name;
            Field.value = Value;
            customfields.Add(Field);
        }

        /// <summary>
        /// The add custom string field.
        /// </summary>
        /// <param name="customfields">
        /// The customfields.
        /// </param>
        /// <param name="Name">
        /// The name.
        /// </param>
        /// <param name="Value">
        /// The value.
        /// </param>
        private static void AddCustomStringField(List<CField> customfields, string Name, string Value)
        {
            var Field = new CStringField();
            Field.name = Name;
            Field.value = Value;
            customfields.Add(Field);
        }

        /// <summary>
        /// The add reported by record.
        /// </summary>
        /// <param name="username">
        /// The username.
        /// </param>
        /// <param name="foundinversion">
        /// The foundinversion.
        /// </param>
        /// <param name="comments">
        /// The comments.
        /// </param>
        private void AddReportedByRecord(string username, string foundinversion, string comments)
        {
            this.defect.reportedbylist = new CReportedByRecord[1];
            this.defect.reportedbylist[0] = new CReportedByRecord();
            this.defect.reportedbylist[0].foundby = username;
            this.defect.reportedbylist[0].foundinversion = foundinversion;
            this.defect.reportedbylist[0].comments = comments;
        }

        /// <summary>
        /// The find custom field.
        /// </summary>
        /// <param name="fieldname">
        /// The fieldname.
        /// </param>
        /// <returns>
        /// The <see cref="long"/>.
        /// </returns>
        private long FindCustomField(string fieldname)
        {
            if (this.defect.customFieldList != null)
            {
                for (int i = 0; i < this.defect.customFieldList.Count(); ++i)
                {
                    if (this.defect.customFieldList[i].name == fieldname)
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        /// <summary>
        /// The read from file.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        /// <exception cref="FileNotFoundException">
        /// </exception>
        /// <exception cref="Exception">
        /// </exception>
        private string ReadFromFile(string value)
        {
            if (!File.Exists(value))
            {
                throw new FileNotFoundException("Comment file \"" + value + "\" was not found.");
            }

            string commentstring = string.Empty;
            try
            {
                using (var sr = new StreamReader(value))
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
        /// The set custom field value.
        /// </summary>
        /// <param name="fieldname">
        /// The fieldname.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <exception cref="Exception">
        /// </exception>
        private void SetCustomFieldValue(string fieldname, string value)
        {
            long index = this.FindCustomField(fieldname);
            if (index >= 0)
            {
                if (this.defect.customFieldList[index] is CStringField)
                {
                    (this.defect.customFieldList[index] as CStringField).value = value;
                }
                else if (this.defect.customFieldList[index] is CIntegerField)
                {
                    (this.defect.customFieldList[index] as CIntegerField).value = Convert.ToInt32(value);
                }
                else if (this.defect.customFieldList[index] is CDropdownField)
                {
                    (this.defect.customFieldList[index] as CDropdownField).value = value;
                }
                else
                {
                    throw new Exception("Unknown custom field type");
                }
            }
            else
            {
                throw new Exception("Custom field not found.");
            }
        }

        /// <summary>
        /// The set custom values.
        /// </summary>
        /// <param name="username">
        /// The username.
        /// </param>
        /// <param name="summary">
        /// The summary.
        /// </param>
        private void SetCustomValues(string username, string summary)
        {
            this.defect.enteredby = username;
            this.defect.summary = summary;
            this.defect.createdbyuser = username;
            this.defect.modifiedbyuser = username;
        }

        /// <summary>
        ///     The set dates.
        /// </summary>
        private void SetDates()
        {
            this.defect.dateentered = DateTime.Now;
            this.defect.dateenteredSpecified = true;
            this.defect.datetimecreated = DateTime.Now;
            this.defect.datetimecreatedSpecified = true;
            this.defect.datetimemodified = DateTime.Now;
            this.defect.datetimemodifiedSpecified = true;
            this.defect.reportedbylist[0].datefound = DateTime.Now;
            this.defect.reportedbylist[0].datefoundSpecified = true;
        }

        #endregion
    }
}