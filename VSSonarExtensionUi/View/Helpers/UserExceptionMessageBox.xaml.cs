// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserExceptionMessageBox.xaml.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
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
    using System;
    using System.Windows;

    /// <summary>
    ///     Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class UserExceptionMessageBox
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UserExceptionMessageBox"/> class.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="exception">
        /// The exception.
        /// </param>
        /// <param name="log">
        /// The log.
        /// </param>
        public UserExceptionMessageBox(string message, Exception exception, string log)
        {
            this.InitializeComponent();

            this.ErrorMessage.Text = string.Format(message + ": {0}", exception.Message);
            this.StackTrace.Text = exception.StackTrace + "\r\n" + log;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserExceptionMessageBox"/> class.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="stacktrace">
        /// The stacktrace.
        /// </param>
        public UserExceptionMessageBox(string message, string stacktrace)
        {
            this.InitializeComponent();

            this.ErrorMessage.Text = string.Format(message);
            this.StackTrace.Text = stacktrace;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The show.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="exception">
        /// The exception.
        /// </param>
        public static void ShowException(string message, Exception exception)
        {
            Application.Current.Dispatcher.Invoke(
                delegate
                    {
                        var box = new UserExceptionMessageBox(message, exception, string.Empty);
                        box.ShowDialog();
                    });
        }

        /// <summary>
        /// The show exception.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="exception">
        /// The exception.
        /// </param>
        /// <param name="stacktrace">
        /// The stacktrace.
        /// </param>
        public static void ShowException(string message, Exception exception, string stacktrace)
        {
            Application.Current.Dispatcher.Invoke(
                delegate
                    {
                        var box = new UserExceptionMessageBox(message, exception, stacktrace);
                        box.ShowDialog();
                    });
        }

        #endregion

        #region Methods

        /// <summary>
        /// The button base_ on click.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        #endregion
    }
}