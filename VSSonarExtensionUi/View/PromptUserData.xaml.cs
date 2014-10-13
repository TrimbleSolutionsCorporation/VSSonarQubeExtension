// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PromptUserData.xaml.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
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
namespace VSSonarExtensionUi.View
{
    using System.Windows;

    /// <summary>
	/// Interaction logic for PromptUserData.xaml
	/// </summary>
    public partial class PromptUserData : Window
    {
        public enum InputType
        {
            Text,
            Password
        }

        private InputType _inputType = InputType.Text;

        public PromptUserData(string question, string title, string defaultValue = "", InputType inputType = InputType.Text)
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(this.PromptDialog_Loaded);
            this.txtQuestion.Text = question;
            this.Title = title;
            this.txtResponse.Text = defaultValue;
            this._inputType = inputType;
            if (this._inputType == InputType.Password)
                this.txtResponse.Visibility = Visibility.Collapsed;

        }

        void PromptDialog_Loaded(object sender, RoutedEventArgs e)
        {
                this.txtResponse.Focus();
        }

        public static string Prompt(string question, string title, string defaultValue = "", InputType inputType = InputType.Text)
        {
            PromptUserData inst = new PromptUserData(question, title, defaultValue, inputType);
            inst.ShowDialog();
            if (inst.DialogResult == true)
                return inst.ResponseText;
            return null;
        }

        public string ResponseText
        {
            get
            {
                    return this.txtResponse.Text;
            }
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}