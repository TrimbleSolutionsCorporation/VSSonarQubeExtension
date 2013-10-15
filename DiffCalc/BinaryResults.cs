// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BinaryResults.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
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
    using System.Drawing;
    using System.Globalization;
    using System.Windows.Forms;

    using DifferenceEngine;

    /// <summary>
    /// The binary results.
    /// </summary>
    public sealed class BinaryResults : Form
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
        /// The list view 1.
        /// </summary>
        private ListView listView1;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryResults"/> class.
        /// </summary>
        /// <param name="al">
        /// The al.
        /// </param>
        /// <param name="secs">
        /// The secs.
        /// </param>
        public BinaryResults(ArrayList al, double secs)
        {
            this.InitializeComponent();
            this.Text = string.Format("Binary Results: {0} secs.", secs.ToString("#0.00"));
            foreach (DiffResultSpan drs in al)
            {
                var lvi = new ListViewItem(drs.Status.ToString());
                lvi.SubItems.Add(drs.DestIndex == -1 ? "---" : drs.DestIndex.ToString(CultureInfo.InvariantCulture));
                lvi.SubItems.Add(drs.SourceIndex == -1 ? "---" : drs.SourceIndex.ToString(CultureInfo.InvariantCulture));
                lvi.SubItems.Add(drs.Length.ToString(CultureInfo.InvariantCulture));

                switch (drs.Status)
                {
                    case DiffResultSpanStatus.NoChange:
                        lvi.ForeColor = Color.Black;
                        lvi.BackColor = Color.White;
                        break;
                    case DiffResultSpanStatus.DeleteSource:
                        lvi.ForeColor = Color.White;
                        lvi.BackColor = Color.LightCoral;
                        break;
                    case DiffResultSpanStatus.AddDestination:
                        lvi.ForeColor = Color.White;
                        lvi.BackColor = Color.LightGreen;
                        break;
                    case DiffResultSpanStatus.Replace:
                        lvi.ForeColor = Color.White;
                        lvi.BackColor = Color.LightSkyBlue;
                        break;
                }

                this.listView1.Items.Add(lvi);
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
        /// The binary results load.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void BinaryResultsLoad(object sender, EventArgs e)
        {
            this.ListView1Resize(this, e);
        }

        /// <summary>
        ///     Required method for Designer support - do not modify
        ///     the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.listView1 = new ListView();
            this.columnHeader1 = new ColumnHeader();
            this.columnHeader2 = new ColumnHeader();
            this.columnHeader3 = new ColumnHeader();
            this.columnHeader4 = new ColumnHeader();
            this.SuspendLayout();

            // listView1
            this.listView1.Columns.AddRange(
                new[] { this.columnHeader1, this.columnHeader2, this.columnHeader3, this.columnHeader4 });
            this.listView1.Dock = DockStyle.Fill;
            this.listView1.FullRowSelect = true;
            this.listView1.GridLines = true;
            this.listView1.Location = new Point(0, 0);
            this.listView1.MultiSelect = false;
            this.listView1.Name = "listView1";
            this.listView1.Size = new Size(368, 308);
            this.listView1.TabIndex = 0;
            this.listView1.View = View.Details;
            this.listView1.Resize += this.ListView1Resize;

            // columnHeader1
            this.columnHeader1.Text = "Result";

            // columnHeader2
            this.columnHeader2.Text = "Dest Index";
            this.columnHeader2.TextAlign = HorizontalAlignment.Right;

            // columnHeader3
            this.columnHeader3.Text = "Source Index";
            this.columnHeader3.TextAlign = HorizontalAlignment.Right;

            // columnHeader4
            this.columnHeader4.Text = "Length";
            this.columnHeader4.TextAlign = HorizontalAlignment.Right;

            // BinaryResults
            this.AutoScaleBaseSize = new Size(5, 13);
            this.ClientSize = new Size(368, 308);
            this.Controls.Add(this.listView1);
            this.Name = "BinaryResults";
            this.Text = "BinaryResults";
            this.Load += this.BinaryResultsLoad;
            this.ResumeLayout(false);
        }

        /// <summary>
        /// The list view 1_ resize.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void ListView1Resize(object sender, EventArgs e)
        {
            int w = Math.Max((this.listView1.Width - 20) / 4, 50);
            foreach (ColumnHeader ch in this.listView1.Columns)
            {
                ch.Width = w;
            }
        }

        #endregion
    }
}