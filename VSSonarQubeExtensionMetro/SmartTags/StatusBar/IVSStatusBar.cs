// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IVSStatusBar.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
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
namespace VSSonarQubeExtension.SmartTags.StatusBar
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.Linq;
    using System.Runtime.InteropServices;

    using EnvDTE;

    using Constants = Microsoft.VisualStudio.Shell.Interop.Constants;
    using Thread = System.Threading.Thread;

    public class IVSStatusBar
    {
        private VSSStatusBar vssStatusBar;

        public IVSStatusBar(VSSStatusBar vssStatusBar)
        {
            this.vssStatusBar = vssStatusBar;
        }

        /// <summary>
        /// The delete object.
        /// </summary>
        /// <param name="hObject">
        /// The h object.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        /// <summary>
        /// The display and show icon.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        public void DisplayAndShowIcon(string message)
        {
            var b =
                new Bitmap(
                    @"E:\Development\SonarQube\VssonarExtension\VSSonarQubeExtension\VSSonarExtension\PackageImplementation\Resources\sonariconsource.bmp");
            IntPtr hdc = IntPtr.Zero;
            hdc = b.GetHbitmap();

            object hdcObject = hdc;

            vssStatusBar.Bar.Animation(1, ref hdcObject);

            Thread.Sleep(10000);

            vssStatusBar.Bar.Animation(0, ref hdcObject);
            DeleteObject(hdc);

            return;

            object icon = Constants.SBAI_Build;
            vssStatusBar.dte.StatusBar.Animate(true, icon);

            // this.Bar.Animation(1, ref icon);
            vssStatusBar.dte.StatusBar.Text = message;

            // this.Bar.SetText(message);
            vssStatusBar.dte.StatusBar.Animate(false, icon);

            // this.Bar.Animation(0, ref icon);
            vssStatusBar.dte.StatusBar.Text = "Build Succeeded";
        }

        /// <summary>
        /// The display and show progress.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        public void DisplayAndShowProgress(string message)
        {
            var messages = new[] { "Demo Long running task...Step 1...", "Step 2...", "Step 3...", "Step 4...", "Completing long running task." };
            uint cookie = 0;

            // Initialize the progress bar.
            vssStatusBar.Bar.Progress(ref cookie, 1, string.Empty, 0, 0);

            for (uint j = 0; j < 5; j++)
            {
                uint count = j + 1;
                vssStatusBar.Bar.Progress(ref cookie, 1, string.Empty, count, 5);
                vssStatusBar.Bar.SetText(messages[j]);

                // Display incremental progress.
                Thread.Sleep(1500);
            }

            // Clear the progress bar.
            vssStatusBar.Bar.Progress(ref cookie, 0, string.Empty, 0, 0);
            vssStatusBar.Bar.FreezeOutput(0);
            vssStatusBar.Bar.Clear();
        }

        /// <summary>
        /// Displays the message.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        public void DisplayMessage(string message)
        {
            int frozen;

            vssStatusBar.Bar.IsFrozen(out frozen);

            if (frozen == 0)
            {
                vssStatusBar.Bar.SetText(message);
            }
        }

        /// <summary>
        /// The show icons.
        /// </summary>
        public void ShowIcons()
        {
            Properties properties = vssStatusBar.dte.Properties["Environment", "General"];

            Property propertyAutoAdjust = properties.Cast<Property>().FirstOrDefault(p => p.Name == "AutoAdjustExperience");

            propertyAutoAdjust.Value = "False";
            Property propertyAnimations = properties.Cast<Property>().FirstOrDefault(p => p.Name == "Animations");

            propertyAnimations.Value = "False";
            foreach (object property in properties)
            {
                Debug.WriteLine(((Property)property).Name + " : " + ((Property)property).Value);
            }

            var b =
                new Bitmap(
                    @"E:\Development\SonarQube\VssonarExtension\VSSonarQubeExtension\VSSonarExtension\PackageImplementation\Resources\sonariconsource.bmp");
            IntPtr hdc = IntPtr.Zero;
            hdc = b.GetHbitmap();

            object hdcObject = hdc;

            vssStatusBar.Bar.Animation(1, ref hdcObject);
        }
    }
}