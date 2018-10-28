// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CreateJsondata.cs" company="Copyright © 2017 Trimble Solutions Corporation">
//     Copyright (C) 2017 [Trimble Solutions Corporation, tekla.buildmaster@trimble.com]
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
namespace JiraConnector
{
    using System.Collections.Generic;

    /// <summary>
    /// The Issue class 
    /// </summary>
    public class JIssue
    {
        /// <summary>
        /// Gets or sets fields
        /// </summary>
        public IssueFields fields { get; set; }

        /// <summary>
        /// The Issue 
        /// </summary>
        public JIssue()
        {
            fields = new IssueFields();
        }
    }

    /// <summary>
    /// The IssueFields 
    /// </summary>
    public class IssueFields
    {
        /// <summary>
        /// Gets or sets Project
        /// </summary>
        public Project project { get; set; }

        /// <summary>
        /// Gets or sets summary
        /// </summary>
        public string summary { get; set; }

        /// <summary>
        /// Gets or sets description
        /// </summary>
        public string description { get; set; }

        /// <summary>
        /// Gets or sets issuetype
        /// </summary>
        public IssueType issuetype { get; set; }

        /// <summary>
        /// Gets or sets customfield_12015
        /// </summary>
        public Segment customfield_12015 { get; set; }

        /// <summary>
        /// Gets or sets customfield_12103
        /// </summary>
        public List<Components> components { get; set; }
        public Origin customfield_12106 { get; set; }

        /// <summary>
        /// Gets or sets customfield_12114
        /// </summary>
        // public VersionFound customfield_12114 { get; set; }
        public List<VersionFound> customfield_12114 { get; set; }

        /// <summary>
        /// Gets or sets customfield_12104
        /// </summary>
        public DefectiveSinceV customfield_12104 { get; set; }

        /// <summary>
        /// Gets or sets customfield_10312
        /// </summary>
        public Type customfield_10312 { get; set; }

        public IssueFields()
        {
            project = new Project();
            issuetype = new IssueType();
            customfield_12015 = new Segment();
            components = new List<Components>();
            customfield_12106 = new Origin();
            customfield_12114 = new List<VersionFound>();
            customfield_12104 = new DefectiveSinceV();
            customfield_10312 = new Type();
        }
    }

    /// <summary>
    /// Type
    /// </summary>
    public class Type
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string value { get; set; }
    }

    /// <summary>
    /// Version found
    /// </summary>
    public class VersionFound
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string name { get; set; }
    }

    /// <summary>
    /// origin
    /// </summary>
    public class Origin
    {
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public string value { get; set; }
    }

    /// <summary>
    /// project
    /// </summary>
    public class Project
    {
        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public string key { get; set; }
    }

    /// <summary>
    /// issue type
    /// </summary>
    public class IssueType
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string name { get; set; }
    }

    /// <summary>
    /// segment
    /// </summary>
    public class Segment
    {
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public string value { get; set; }
    }

    public class DefectiveSinceV
    {
        public string value { get; set; }
    }

    public class Components
    {
        public string value { get; set; }
        public string name { get; set; }
    }

    /// <summary>
    /// The Issue class 
    /// </summary>
    public class Update
    {
        /// <summary>
        /// Gets or sets fields
        /// </summary>
        public UpdateFields update { get; set; }

        /// <summary>
        /// The Issue 
        /// </summary>
        public Update()
        {
            update = new UpdateFields();
        }
    }
    /// <summary>
    /// The IssueFields 
    /// </summary>
    public class UpdateFields
    {
        public List<Description> description { get; set; }
        public UpdateFields()
        {
            description = new List<Description>();
        }
    }
    public class Description
    {
       public string set { get; set; }
    }
        public static class CreateJsondata
    {
    }

    public class Comment
    {
        public string body { get; set; }
    }

     public class CommentFields
    {

    }

}
