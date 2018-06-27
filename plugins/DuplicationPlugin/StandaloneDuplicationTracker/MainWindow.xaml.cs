// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Copyright © 2014 jmecsoftware">
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace StandaloneDuplicationTracker
{
    using VSSonarPlugins.Types;

    using ProjectDuplicationTracker;

    /// <summary>
    /// Interaction logic for DuplicationUserControl.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ButtonLogin_OnClick(object sender, RoutedEventArgs e)
        {
            this.Hide();

            var password = this.Password.Password;
            var hostname = this.Host.Text;
            var login = this.Login.Text;

            var model = new ProjectDuplicationTrackerModel();
            model.Login();
            var win = new MahApps.Metro.Controls.MetroWindow();
            var view = new DuplicationUserControl(model);
            win.Content = view;
            win.SizeToContent = SizeToContent.WidthAndHeight;
            win.ShowDialog();
            this.Close();
        }
    }
}
