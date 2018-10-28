// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PromptUserData.xaml.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
//     Copyright (C) 2014 [Jorge Costa, Jorge.Costa@tekla.com]
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

namespace VSSonarExtensionUi.View.Helpers
{
    using SonarRestService.Types;
    using System.Collections.Generic;
    using System.Windows;
    using VSSonarPlugins.Types;

    /// <summary>
    ///     Interaction logic for PromptUserData.xaml
    /// </summary>
    public partial class PromptForAssignUser
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PromptForAssignUser"/> class.
        /// </summary>
        /// <param name="question">The question.</param>
        /// <param name="title">The title.</param>
        /// <param name="users">The users.</param>
        public PromptForAssignUser(string question, string title, List<User> users)
        {
            this.InitializeComponent();
            this.UserBox.ItemsSource = users;
            this.txtQuestion.Text = question;
            this.Title = title;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The prompt.
        /// </summary>
        /// <param name="question">The question.</param>
        /// <param name="title">The title.</param>
        /// <param name="users">The users.</param>
        /// <param name="comment">The comment.</param>
        /// <returns>
        /// The <see cref="string" />.
        /// </returns>
        public static User Prompt(string question, string title, List<User> users, out string comment)
        {
            var inst = new PromptForAssignUser(question, title, users);
            inst.ShowDialog();

            if (inst.DialogResult == true)
            {
                comment = inst.Comment();
                return inst.SelectedUser();
            }

            comment = string.Empty;
            return null;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Selecteds the user.
        /// </summary>
        /// <returns>restures selected user.</returns>
        public User SelectedUser()
        {
            return this.UserBox.SelectedItem as User;
        }

        /// <summary>
        /// Comments this instance.
        /// </summary>
        /// <returns>returns comment.</returns>
        public string Comment()
        {
            return this.txtComment.Text;
        }

        /// <summary>
        /// The btn cancel_ click.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void BtnCancelClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// The btn ok_ click.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void BtnOkClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        #endregion
    }
}