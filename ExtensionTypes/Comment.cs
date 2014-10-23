// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Comment.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
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

    /// <summary>
    /// The comment.
    /// </summary>
    public class Comment
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Comment"/> class.
        /// </summary>
        public Comment()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Comment"/> class.
        /// </summary>
        /// <param name="createAt">
        /// The create at.
        /// </param>
        /// <param name="htmlText">
        /// The html text.
        /// </param>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="login">
        /// The login.
        /// </param>
        /// <param name="id">
        /// The id.
        /// </param>
        public Comment(DateTime createAt, string htmlText, Guid key, string login, int id)
        {
            this.CreatedAt = createAt;
            this.HtmlText = htmlText;
            this.Key = key;
            this.Login = login;
            this.Id = id;
        }

        /// <summary>
        /// Gets or sets the created at.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the html text.
        /// </summary>
        public string HtmlText { get; set; }

        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        public Guid Key { get; set; }

        /// <summary>
        /// Gets or sets the login.
        /// </summary>
        public string Login { get; set; }

        /// <summary>
        /// Gets or sets the id. Versions before 3.6
        /// </summary>
        public int Id { get; set; }
    }
}