// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Issue.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
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
namespace ExtensionTypes
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The issue.
    /// </summary>
    public class Issue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Issue"/> class.
        /// </summary>
        public Issue()
        {
            this.Comments = new List<Comment>();
        }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the creation date.
        /// </summary>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Gets or sets the close date.
        /// </summary>
        public DateTime CloseDate { get; set; }

        /// <summary>
        /// Gets or sets the component.
        /// </summary>
        public string Component { get; set; }

        /// <summary>
        /// Gets or sets the component.
        /// </summary>
        public string ComponentSafe { get; set; }

        /// <summary>
        /// Gets or sets the effort to fix.
        /// </summary>
        public decimal EffortToFix { get; set; }

        /// <summary>
        /// Gets or sets the line.
        /// </summary>
        public int Line { get; set; }

        /// <summary>
        /// Gets or sets the project.
        /// </summary>
        public string Project { get; set; }

        /// <summary>
        /// Gets or sets the update date.
        /// </summary>
        public DateTime UpdateDate { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the severity.
        /// </summary>
        public string Severity { get; set; }

        /// <summary>
        /// Gets or sets the rule.
        /// </summary>
        public string Rule { get; set; }

        /// <summary>
        /// Gets or sets the resolution.
        /// </summary>
        public string Resolution { get; set; }

        /// <summary>
        /// Gets or sets the assignee.
        /// </summary>
        public string Assignee { get; set; }

        /// <summary>
        /// Gets or sets the assignee.
        /// </summary>
        public bool IsNew { get; set; }

        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        public Guid Key { get; set; }

        /// <summary>
        /// Gets or sets the id, versious before 3.6
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the violation id.
        /// </summary>
        public int ViolationId { get; set; }

        /// <summary>
        /// Gets or sets the comments.
        /// </summary>
        public List<Comment> Comments { get; set; }

        /// <summary>
        /// The deep copy.
        /// </summary>
        /// <returns>
        /// The <see cref="Issue"/>.
        /// </returns>
        public Issue DeepCopy()
        {
            var copyIssue = new Issue
                                {
                                    Id = this.Id,
                                    Key = this.Key,
                                    Line = this.Line,
                                    Message = this.Message,
                                    Project = this.Project,
                                    Resolution = this.Resolution,
                                    Rule = this.Rule,
                                    Severity = this.Severity,
                                    Status = this.Status,
                                    UpdateDate = this.UpdateDate,
                                    CloseDate = this.CloseDate,
                                    Component = this.Component,
                                    CreationDate = this.CreationDate,
                                    EffortToFix = this.EffortToFix,
                                    ViolationId = this.ViolationId,
                                    Assignee = this.Assignee,
                                    Comments = new List<Comment>()
                                };

            foreach (var comment in this.Comments)
            {
                var copyComment = new Comment
                                      {
                                          CreatedAt = comment.CreatedAt,
                                          HtmlText = comment.HtmlText,
                                          Id = comment.Id,
                                          Key = comment.Key,
                                          Login = comment.Login
                                      };
                copyIssue.Comments.Add(copyComment);
            }

            return copyIssue;
        }
    }
}