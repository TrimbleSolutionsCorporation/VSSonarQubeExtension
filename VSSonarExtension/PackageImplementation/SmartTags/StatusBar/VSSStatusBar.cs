// --------------------------------------------------------------------------------------------------------------------
// <copyright company="" file="VSSStatusBar.cs">
//   
// </copyright>
// <summary>
//   Class to interact with the VS status bar
// </summary>
// 
// --------------------------------------------------------------------------------------------------------------------
namespace VSSonarExtension.PackageImplementation.SmartTags.StatusBar
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.Linq;
    using System.Runtime.InteropServices;

    using EnvDTE;

    using EnvDTE80;

    using Microsoft.VisualStudio.Shell.Interop;

    using Constants = Microsoft.VisualStudio.Shell.Interop.Constants;
    using Thread = System.Threading.Thread;

    /// <summary>
    ///     Class to interact with the VS status bar
    /// </summary>
    public class VSSStatusBar
    {
        #region Fields

        /// <summary>
        /// The bar.
        /// </summary>
        private readonly IVsStatusbar bar;

        /// <summary>
        /// The dte.
        /// </summary>
        private readonly DTE2 dte;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="VSSStatusBar"/> class. 
        /// Initializes a new instance of the <see cref="VSSonarExtension.PackageImplementation.SmartTags.StatusBar"/> class.
        /// </summary>
        /// <param name="bar">
        /// The bar.
        /// </param>
        /// <param name="dte">
        /// The dte.
        /// </param>
        public VSSStatusBar(IVsStatusbar bar, DTE2 dte)
        {
            this.bar = bar;
            this.dte = dte;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the status bar.
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

        #region Public Methods and Operators

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

            this.Bar.Animation(1, ref hdcObject);

            Thread.Sleep(10000);

            this.Bar.Animation(0, ref hdcObject);
            DeleteObject(hdc);

            return;

            object icon = Constants.SBAI_Build;
            this.dte.StatusBar.Animate(true, icon);

            // this.Bar.Animation(1, ref icon);
            this.dte.StatusBar.Text = message;

            // this.Bar.SetText(message);
            this.dte.StatusBar.Animate(false, icon);

            // this.Bar.Animation(0, ref icon);
            this.dte.StatusBar.Text = "Build Succeeded";
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
            this.Bar.Progress(ref cookie, 1, string.Empty, 0, 0);

            for (uint j = 0; j < 5; j++)
            {
                uint count = j + 1;
                this.Bar.Progress(ref cookie, 1, string.Empty, count, 5);
                this.Bar.SetText(messages[j]);

                // Display incremental progress.
                Thread.Sleep(1500);
            }

            // Clear the progress bar.
            this.Bar.Progress(ref cookie, 0, string.Empty, 0, 0);
            this.Bar.FreezeOutput(0);
            this.Bar.Clear();
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

            this.Bar.IsFrozen(out frozen);

            if (frozen == 0)
            {
                this.Bar.SetText(message);
            }
        }

        /// <summary>
        /// The show icons.
        /// </summary>
        public void ShowIcons()
        {
            Properties properties = this.dte.Properties["Environment", "General"];

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

            this.Bar.Animation(1, ref hdcObject);
        }

        #endregion
    }
}