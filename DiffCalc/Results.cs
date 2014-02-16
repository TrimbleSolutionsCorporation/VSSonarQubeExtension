// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Results.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
//     Copyright (C) 2013 [Jorge Costa, Jorge.Costa@tekla.com]
// </copyright>
// All credit goes to http://www.codeproject.com/Articles/6943/A-Generic-Reusable-Diff-Algorithm-in-C-II#_comments
// --------------------------------------------------------------------------------------------------------------------
// This program is free software; you can redistribute it and/or modify it under the terms of the GNU Lesser General Public License
// as published by the Free Software Foundation; either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
// of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details. 
// You should have received a copy of the GNU Lesser General Public License along with this program; if not, write to the Free
// Software Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// --------------------------------------------------------------------------------------------------------------------
namespace DiffCalc
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Globalization;
    using System.Windows.Forms;

    using DifferenceEngine;

    /// <summary>
    /// The results.
    /// </summary>
    public sealed class Results : Form
    {
        #region Fields

        /// <summary>
        ///     Required designer variable.
        /// </summary>
        private readonly Container components = null;

        /// <summary>
        /// The column header 1.
        /// </summary>
        private ColumnHeader columnHeader1;

        /// <summary>
        /// The column header 2.
        /// </summary>
        private ColumnHeader columnHeader2;

        /// <summary>
        /// The column header 3.
        /// </summary>
        private ColumnHeader columnHeader3;

        /// <summary>
        /// The column header 4.
        /// </summary>
        private ColumnHeader columnHeader4;

        /// <summary>
        /// The lv destination.
        /// </summary>
        private ListView lvdestination;

        /// <summary>
        /// The lv source.
        /// </summary>
        private ListView lvsource;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Results"/> class.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="destination">
        /// The destination.
        /// </param>
        /// <param name="diffLines">
        /// The diff lines.
        /// </param>
        /// <param name="seconds">
        /// The seconds.
        /// </param>
        public Results(DiffListTextFile source, DiffListTextFile destination, ArrayList diffLines, double seconds)
        {
            this.InitializeComponent();
            this.Text = string.Format("Results: {0} secs.", seconds.ToString("#0.00", CultureInfo.InvariantCulture));

            var cnt = 1;

            try
            {
                foreach (DiffResultSpan drs in diffLines)
                {
                    ListViewItem lviS;
                    ListViewItem lviD;
                    int i;
                    switch (drs.Status)
                    {
                        case DiffResultSpanStatus.DeleteSource:
                            for (i = 0; i < drs.Length; i++)
                            {
                                lviS = new ListViewItem(cnt.ToString("00000", CultureInfo.InvariantCulture));
                                lviD = new ListViewItem(cnt.ToString("00000", CultureInfo.InvariantCulture));
                                lviS.BackColor = Color.Red;
                                lviS.SubItems.Add(((TextLine)source.GetByIndex(drs.SourceIndex + i)).Line);
                                lviD.BackColor = Color.LightGray;
                                lviD.SubItems.Add(string.Empty);

                                this.lvsource.Items.Add(lviS);
                                this.lvdestination.Items.Add(lviD);
                                cnt++;
                            }

                            break;
                        case DiffResultSpanStatus.NoChange:
                            for (i = 0; i < drs.Length; i++)
                            {
                                lviS = new ListViewItem(cnt.ToString("00000", CultureInfo.InvariantCulture));
                                lviD = new ListViewItem(cnt.ToString("00000", CultureInfo.InvariantCulture));
                                lviS.BackColor = Color.White;
                                lviS.SubItems.Add(((TextLine)source.GetByIndex(drs.SourceIndex + i)).Line);
                                lviD.BackColor = Color.White;
                                lviD.SubItems.Add(((TextLine)destination.GetByIndex(drs.DestIndex + i)).Line);

                                this.lvsource.Items.Add(lviS);
                                this.lvdestination.Items.Add(lviD);
                                cnt++;
                            }

                            break;
                        case DiffResultSpanStatus.AddDestination:
                            for (i = 0; i < drs.Length; i++)
                            {
                                lviS = new ListViewItem(cnt.ToString("00000", CultureInfo.InvariantCulture));
                                lviD = new ListViewItem(cnt.ToString("00000", CultureInfo.InvariantCulture));
                                lviS.BackColor = Color.LightGray;
                                lviS.SubItems.Add(string.Empty);
                                lviD.BackColor = Color.LightGreen;
                                lviD.SubItems.Add(((TextLine)destination.GetByIndex(drs.DestIndex + i)).Line);

                                this.lvsource.Items.Add(lviS);
                                this.lvdestination.Items.Add(lviD);
                                cnt++;
                            }

                            break;
                        case DiffResultSpanStatus.Replace:
                            for (i = 0; i < drs.Length; i++)
                            {
                                lviS = new ListViewItem(cnt.ToString("00000", CultureInfo.InvariantCulture));
                                lviD = new ListViewItem(cnt.ToString("00000", CultureInfo.InvariantCulture));
                                lviS.BackColor = Color.Red;
                                lviS.SubItems.Add(((TextLine)source.GetByIndex(drs.SourceIndex + i)).Line);
                                lviD.BackColor = Color.LightGreen;
                                lviD.SubItems.Add(((TextLine)destination.GetByIndex(drs.DestIndex + i)).Line);

                                this.lvsource.Items.Add(lviS);
                                this.lvdestination.Items.Add(lviD);
                                cnt++;
                            }

                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.StackTrace);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">
        /// The disposing.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.components != null)
                {
                    this.components.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        /// <summary>
        ///     Required method for Designer support - do not modify
        ///     the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lvsource = new ListView();
            this.columnHeader1 = new ColumnHeader();
            this.columnHeader2 = new ColumnHeader();
            this.lvdestination = new ListView();
            this.columnHeader3 = new ColumnHeader();
            this.columnHeader4 = new ColumnHeader();
            this.SuspendLayout();

            // lvsource
            this.lvsource.Columns.AddRange(new[] { this.columnHeader1, this.columnHeader2 });
            this.lvsource.FullRowSelect = true;
            this.lvsource.HideSelection = false;
            this.lvsource.Location = new Point(28, 17);
            this.lvsource.MultiSelect = false;
            this.lvsource.Name = "lvsource";
            this.lvsource.Size = new Size(114, 102);
            this.lvsource.TabIndex = 0;
            this.lvsource.View = View.Details;
            this.lvsource.Resize += this.LvSourceResize;
            this.lvsource.SelectedIndexChanged += this.LvSourceSelectedIndexChanged;

            // columnHeader1
            this.columnHeader1.Text = "Line";
            this.columnHeader1.Width = 50;

            // columnHeader2
            this.columnHeader2.Text = "Text (Source)";
            this.columnHeader2.Width = 147;

            // lvdestination
            this.lvdestination.Columns.AddRange(new[] { this.columnHeader3, this.columnHeader4 });
            this.lvdestination.FullRowSelect = true;
            this.lvdestination.HideSelection = false;
            this.lvdestination.Location = new Point(176, 15);
            this.lvdestination.MultiSelect = false;
            this.lvdestination.Name = "lvdestination";
            this.lvdestination.Size = new Size(123, 110);
            this.lvdestination.TabIndex = 2;
            this.lvdestination.View = View.Details;
            this.lvdestination.Resize += this.LvDestinationResize;
            this.lvdestination.SelectedIndexChanged += this.LvDestinationSelectedIndexChanged;

            // columnHeader3
            this.columnHeader3.Text = "Line";
            this.columnHeader3.Width = 50;

            // columnHeader4
            this.columnHeader4.Text = "Text (Destination)";
            this.columnHeader4.Width = 198;

            // Results
            this.AutoScaleBaseSize = new Size(5, 13);
            this.ClientSize = new Size(533, 440);
            this.Controls.Add(this.lvdestination);
            this.Controls.Add(this.lvsource);
            this.Name = "Results";
            this.Text = "Results";
            this.Resize += this.ResultsResize;
            this.Load += this.ResultsLoad;
            this.ResumeLayout(false);
        }

        /// <summary>
        /// The lv destination selected index changed.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void LvDestinationSelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.lvdestination.SelectedItems.Count > 0)
            {
                ListViewItem lvi = this.lvsource.Items[this.lvdestination.SelectedItems[0].Index];
                lvi.Selected = true;
                lvi.EnsureVisible();
            }
        }

        /// <summary>
        /// The results_ load.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void ResultsLoad(object sender, EventArgs e)
        {
            this.ResultsResize(sender, e);
        }

        /// <summary>
        /// The results_ resize.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void ResultsResize(object sender, EventArgs e)
        {
            int w = this.ClientRectangle.Width / 2;
            this.lvsource.Location = new Point(0, 0);
            this.lvsource.Width = w;
            this.lvsource.Height = this.ClientRectangle.Height;

            this.lvdestination.Location = new Point(w + 1, 0);
            this.lvdestination.Width = this.ClientRectangle.Width - (w + 1);
            this.lvdestination.Height = this.ClientRectangle.Height;
        }

        /// <summary>
        /// The lv destination_ resize.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void LvDestinationResize(object sender, EventArgs e)
        {
            if (this.lvdestination.Width > 100)
            {
                this.lvdestination.Columns[1].Width = -2;
            }
        }

        /// <summary>
        /// The lv source_ resize.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void LvSourceResize(object sender, EventArgs e)
        {
            if (this.lvsource.Width > 100)
            {
                this.lvsource.Columns[1].Width = -2;
            }
        }

        /// <summary>
        /// The lv source_ selected index changed.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void LvSourceSelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.lvsource.SelectedItems.Count > 0)
            {
                ListViewItem lvi = this.lvdestination.Items[this.lvsource.SelectedItems[0].Index];
                lvi.Selected = true;
                lvi.EnsureVisible();
            }
        }

        #endregion
    }
}
