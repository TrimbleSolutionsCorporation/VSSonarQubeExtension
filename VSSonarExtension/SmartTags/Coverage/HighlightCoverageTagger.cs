// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HighlightCoverageTagger.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
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

namespace VSSonarExtension.SmartTags.Coverage
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;

    using DifferenceEngine;

    using ExtensionHelpers;

    using ExtensionTypes;

    using Microsoft.VisualStudio.Text;
    using Microsoft.VisualStudio.Text.Tagging;

    using VSSonarExtension.PackageImplementation;

    /// <summary>
    ///     This tagger will provide tags for every word in the buffer that
    ///     matches the word currently under the cursor.
    /// </summary>
    public class HighlightCoverageTagger : ITagger<TextMarkerTag>, IDisposable
    {
        #region Fields

        /// <summary>
        ///     The coverage line.
        /// </summary>
        private readonly Dictionary<int, string> coverageLine = new Dictionary<int, string>();

        /// <summary>
        ///     The file path.
        /// </summary>
        private readonly string filePath;

        /// <summary>
        ///     The register events.
        /// </summary>
        private readonly bool registerEvents;

        /// <summary>
        ///     The update lock.
        /// </summary>
        private readonly object updateLock = new object();

        /// <summary>
        ///     The coverage data.
        /// </summary>
        private SourceCoverage coverageData = new SourceCoverage();

        /// <summary>
        ///     The last saved source.
        /// </summary>
        private string lastReferencedSavedSource = string.Empty;

        /// <summary>
        /// The m disposed.
        /// </summary>
        private bool isDisposed;

        /// <summary>
        ///     The temp coverage line.
        /// </summary>
        private Dictionary<int, string> tempCoverageLine;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="HighlightCoverageTagger"/> class.
        /// </summary>
        /// <param name="sourceBuffer">
        /// The source buffer.
        /// </param>
        /// <param name="filePath">
        /// The file Path.
        /// </param>
        /// <param name="registerEvents">
        /// The register Events.
        /// </param>
        public HighlightCoverageTagger(ITextBuffer sourceBuffer, string filePath, bool registerEvents)
        {
            this.SourceBuffer = sourceBuffer;
            this.registerEvents = registerEvents;
            this.filePath = filePath;
            if (registerEvents)
            {
                VsSonarExtensionPackage.ExtensionModelData.PropertyChanged += this.CoverageDataChanged;
            }
        }

        #endregion

        #region Public Events

        /// <summary>
        ///     The tags changed.
        /// </summary>
        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the source buffer.
        /// </summary>
        public ITextBuffer SourceBuffer { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     The dispose.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// The get tags.
        /// </summary>
        /// <param name="spans">
        /// The spans.
        /// </param>
        /// <returns>
        /// The System.Collections.Generic.IEnumerable`1[T -&gt; Microsoft.VisualStudio.Text.Tagging.ITagSpan`1[T -&gt; SmartTags.Coverage.HighlightCoverageTag]].
        /// </returns>
        public IEnumerable<ITagSpan<TextMarkerTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (spans.Count == 0 || this.coverageLine.Count == 0)
            {
                yield break;
            }

            if (!VsSonarExtensionPackage.ExtensionModelData.EnableCoverageInEditor)
            {
                yield break;
            }

            var fileinModel = VsSonarExtensionPackage.ExtensionModelData.DocumentInView;
            var currFile = this.filePath.Replace('\\', '/');
            if (!currFile.Equals(fileinModel))
            {
                yield break;
            }

            lock (this.updateLock)
            {
                foreach (var hit in this.coverageLine)
                {
                    ITextSnapshotLine textsnapshot;

                    try
                    {
                        textsnapshot = this.SourceBuffer.CurrentSnapshot.GetLineFromLineNumber(hit.Key);
                    }
                    catch (Exception)
                    {
                        yield break;
                    }

                    var span = new SnapshotSpan(
                        this.SourceBuffer.CurrentSnapshot, textsnapshot.Start, textsnapshot.Length);

                    if (hit.Value.Equals("Covered"))
                    {
                        yield return new TagSpan<TextMarkerTag>(span, new TextMarkerTag("LineCovered"));
                    }
                    else if (hit.Value.Equals("Partial Covered"))
                    {
                        yield return new TagSpan<TextMarkerTag>(span, new TextMarkerTag("LinePartialCovered"));
                    }
                    else if (hit.Value.Equals("Not Covered"))
                    {
                        yield return new TagSpan<TextMarkerTag>(span, new TextMarkerTag("LineNotCovered"));
                    }
                    else
                    {
                        yield break;
                    }
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sets the coverage data.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void CoverageDataChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!e.PropertyName.Equals("CoverageInEditor") || VsSonarExtensionPackage.ExtensionModelData.LastReferenceSourceInServer == null)
            {
                return;
            }

            lock (this.updateLock)
            {
                this.coverageData = VsSonarExtensionPackage.ExtensionModelData.CoverageInEditor;
                this.lastReferencedSavedSource = VsSonarExtensionPackage.ExtensionModelData.LastReferenceSourceInServer;

                this.tempCoverageLine = new Dictionary<int, string>();

                foreach (var linemeasure in this.coverageData.LinesHits)
                {
                    var line = linemeasure.Id - 1;

                    this.tempCoverageLine.Add(line, linemeasure.Hits > 0 ? "Covered" : "Not Covered");
                }

                foreach (var branchmeasure in this.coverageData.BranchHits)
                {
                    var line = branchmeasure.Id - 1;

                    if (this.tempCoverageLine.ContainsKey(line))
                    {
                        if (branchmeasure.CoveredConditions == branchmeasure.TotalConditions)
                        {
                            this.tempCoverageLine[line] = "Covered";
                        }
                        else if (branchmeasure.CoveredConditions == 0)
                        {
                            this.tempCoverageLine[line] = "Not Covered";
                        }
                        else
                        {
                            this.tempCoverageLine[line] = "Partial Covered";
                        }
                    }
                    else
                    {
                        if (branchmeasure.CoveredConditions == branchmeasure.TotalConditions)
                        {
                            this.tempCoverageLine.Add(line, "Covered");
                        }
                        else if (branchmeasure.CoveredConditions == 0)
                        {
                            this.tempCoverageLine.Add(line, "Not Covered");
                        }
                        else
                        {
                            this.tempCoverageLine.Add(line, "Partial Covered");
                        }
                    }
                }
            }

            try
            {
                this.UpdateCoverageAdornmentsFast();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message + " " + ex.StackTrace);
            }            
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        /// <param name="disposing">
        /// The disposing.
        /// </param>
        private void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                if (this.registerEvents)
                {
                    VsSonarExtensionPackage.ExtensionModelData.PropertyChanged -= this.CoverageDataChanged;
                    HighlightCoverageTaggerProvider.AllCoverageTags.Remove(this.filePath);
                }

                this.SourceBuffer = null;
            }

            this.isDisposed = true;
        }

        /// <summary>
        ///     The update coverage adornments.
        /// </summary>
        private void UpdateCoverageAdornmentsFast()
        {
            lock (this.updateLock)
            {
                this.coverageLine.Clear();

                var diffReport = VsSonarUtils.GetSourceDiffFromStrings(
                    this.lastReferencedSavedSource, 
                    "\r\n", 
                    this.SourceBuffer.CurrentSnapshot.GetText(), 
                    "\r\n", 
                    DiffEngineLevel.FastImperfect);

                foreach (var linecov in this.tempCoverageLine)
                {
                    var line = VsSonarUtils.EstimateWhereSonarLineIsUsingSourceDifference(linecov.Key, diffReport);

                    if (line > 0 && !this.coverageLine.ContainsKey(line))
                    {
                        this.coverageLine.Add(line, linecov.Value);
                    }
                }
            }

            var tempEvent = this.TagsChanged;
            if (tempEvent != null)
            {
                tempEvent(
                    this, 
                    new SnapshotSpanEventArgs(
                        new SnapshotSpan(this.SourceBuffer.CurrentSnapshot, 0, this.SourceBuffer.CurrentSnapshot.Length)));
            }
        }

        #endregion
    }
}