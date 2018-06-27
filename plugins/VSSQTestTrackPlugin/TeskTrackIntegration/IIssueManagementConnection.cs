// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IIssueManagementConnection.cs" company="Copyright © 2015 jmecsoftware">
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

    /// <summary>
    ///     The IssueManagementConnection interface.
    /// </summary>
    public interface IIssueManagementConnection
    {
        #region Public Properties

        /// <summary>
        ///     Gets a value indicating whether is connected.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        ttsoapcgi Instance { get; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     The close.
        /// </summary>
        void Close();

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
        bool Connect(string SOAPURL, string Project, string UserName, string UserPass);

        /// <summary>
        /// Enables the formatted text support.
        /// </summary>
        void EnableFormattedTextSupport();

        /// <summary>
        /// The connect to project.
        /// </summary>
        /// <param name="ttSOAPURL">
        /// The tt soapurl.
        /// </param>
        /// <param name="TTProject">
        /// The tt project.
        /// </param>
        /// <param name="TTUserName">
        /// The tt user name.
        /// </param>
        /// <param name="TTUserPass">
        /// The tt user pass.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        bool ConnectToProject(string ttSOAPURL, string TTProject, string TTUserName, string TTUserPass);

        /// <summary>
        /// Gets the custom field.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        CField[] GetCustomField(string name);

        /// <summary>
        /// Creates the defect.
        /// </summary>
        /// <param name="Defect">The defect.</param>
        /// <returns></returns>
        long CreateDefect(CDefect Defect);

        /// <summary>
        /// The get project list.
        /// </summary>
        /// <param name="ttSOAPURL">
        /// The tt soapurl.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        List<string> GetProjectList(string ttSOAPURL);

        /// <summary>
        /// Gets the assigned items.
        /// </summary>
        /// <returns></returns>
        List<TestTrackItem> GetAssignedItems();

        /// <summary>
        /// Attaches the comment.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="comment">The comment.</param>
        bool AttachComment(long id, string userName, string comment);

        /// <summary>
        /// Gets the most recent items.
        /// </summary>
        /// <returns></returns>
        List<TestTrackItem> GetMostRecentItems();

        /// <summary>
        /// The get defect.
        /// </summary>
        /// <param name="defectNumber">
        /// The defect number.
        /// </param>
        /// <returns>
        /// The <see cref="CDefect"/>.
        /// </returns>
        CDefect getDefect(long defectNumber);

        /// <summary>
        /// The get defect by record id.
        /// </summary>
        /// <param name="recordId">
        /// The record id.
        /// </param>
        /// <returns>
        /// The <see cref="CDefect"/>.
        /// </returns>
        CDefect getDefectByRecordID(long recordId);

        /// <summary>
        /// The get links for defect.
        /// </summary>
        /// <param name="defectRecordId">
        /// The defect record id.
        /// </param>
        /// <returns>
        /// The <see cref="CLink[]"/>.
        /// </returns>
        CLink[] getLinksForDefect(long defectRecordId);

        #endregion
    }
}