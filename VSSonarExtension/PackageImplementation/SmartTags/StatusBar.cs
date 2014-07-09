/***************************************************************************

Copyright (c) 2008 Microsoft Corporation. All rights reserved.

***************************************************************************/

namespace VSSonarExtension.PackageImplementation.SmartTags.StatusBar
{
    using System;
    using System.Drawing;
    using System.Threading;

    using Microsoft.VisualStudio.Shell.Interop;

    /// <summary>
    /// Class to interact with the VS status bar
    /// </summary>
    public class VSSStatusBar
    {
        #region Fields
        IServiceProvider serviceProvider;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="StatusBar"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        public VSSStatusBar(IVsStatusbar bar)
        {
            this.bar = bar;
        }
        #endregion

        #region Properties
        private IVsStatusbar bar;

        /// <summary>
        /// Gets the status bar.
        /// </summary>
        /// <value>The status bar.</value>
        protected IVsStatusbar Bar
        {
            get
            {
                if (this.bar == null)
                {
                    this.bar = this.serviceProvider.GetService(typeof(SVsStatusbar)) as IVsStatusbar;
                }

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

        public void DisplayAndShowIcon(string message)
        {
            if (this.Bar == null) return;

            int frozen;

            this.Bar.IsFrozen(out frozen);

            if (frozen == 0)
            {
                this.Bar.SetText(message);
            }

            object icon = Constants.SBAI_Save;
            this.Bar.Animation(1, ref icon);
            this.Bar.SetText(message);
            System.Windows.Forms.MessageBox.Show(
        "Click OK to end status bar animation.");

            this.Bar.Animation(0, ref icon);
            this.Bar.Clear();
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