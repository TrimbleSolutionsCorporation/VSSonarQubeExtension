/***************************************************************************

Copyright (c) 2008 Microsoft Corporation. All rights reserved.

***************************************************************************/

namespace VSSonarExtension.PackageImplementation.SmartTags.StatusBar
{
    using System;
    using System.Drawing;
    using System.Threading;

    using EnvDTE80;

    using Microsoft.VisualStudio.Shell.Interop;

    /// <summary>
    /// Class to interact with the VS status bar
    /// </summary>
    public class VSSStatusBar
    {
        #region Fields
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="StatusBar"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        public VSSStatusBar(IVsStatusbar bar, DTE2 dte)
        {
            this.bar = bar;
            this.dte = dte;
        }
        #endregion

        #region Properties
        private IVsStatusbar bar;

        private DTE2 dte;

        /// <summary>
        /// Gets the status bar.
        /// </summary>
        /// <value>The status bar.</value>
        protected IVsStatusbar Bar
        {
            get
            {
                return this.bar;
            }
        }
        #endregion

        #region Public Implementation
        /// <summary>
        /// Displays the message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void DisplayMessage(string message)
        {
            int frozen;

            this.Bar.IsFrozen(out frozen);

            if (frozen == 0)
            {
                this.Bar.SetText(message);
            }
        }


        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);


        public void ShowIcons()
        {

            Bitmap b = new Bitmap(@"E:\Development\SonarQube\VssonarExtension\VSSonarQubeExtension\VSSonarExtension\PackageImplementation\Resources\sonariconsource.bmp");
            IntPtr hdc = IntPtr.Zero;
            hdc = b.GetHbitmap();

            object hdcObject = (object)hdc;

            this.Bar.Animation(1, ref hdcObject);

        }

        public void DisplayAndShowIcon(string message)
        {

            Bitmap b = new Bitmap(@"E:\Development\SonarQube\VssonarExtension\VSSonarQubeExtension\VSSonarExtension\PackageImplementation\Resources\sonariconsource.bmp");
            IntPtr hdc = IntPtr.Zero;
            hdc = b.GetHbitmap();

            object hdcObject = (object)hdc;

            this.Bar.Animation(1, ref hdcObject);

            Thread.Sleep(10000);

            this.Bar.Animation(0, ref hdcObject);
            DeleteObject(hdc);


            return;

            object icon = Constants.SBAI_Build;
            this.dte.StatusBar.Animate(true, icon);
            //this.Bar.Animation(1, ref icon);
            this.dte.StatusBar.Text = message;
            //this.Bar.SetText(message);

            

            this.dte.StatusBar.Animate(false, icon);
            //this.Bar.Animation(0, ref icon);
            this.dte.StatusBar.Text = "Build Succeeded";
        }

        public void DisplayAndShowProgress(string message)
        {
            var messages = new string[]
                {
                    "Demo Long running task...Step 1...",
                    "Step 2...",
                    "Step 3...",
                    "Step 4...",
                    "Completing long running task."
                };
            uint cookie = 0;

            // Initialize the progress bar.
            this.Bar.Progress(ref cookie, 1, "", 0, 0);

            for (uint j = 0; j < 5; j++)
            {
                uint count = j + 1;
                this.Bar.Progress(ref cookie, 1, "", count, 5);
                this.Bar.SetText(messages[j]);
                // Display incremental progress.
                Thread.Sleep(1500);
            }

            // Clear the progress bar.
            this.Bar.Progress(ref cookie, 0, "", 0, 0);
            this.Bar.FreezeOutput(0);
            this.Bar.Clear();
        }
        #endregion
    }
}