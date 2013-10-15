// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DiffEngine.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
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
namespace DifferenceEngine
{
    using System;
    using System.Collections;

    /// <summary>
    ///     The diff engine.
    /// </summary>
    public class DiffEngine
    {
        #region Fields

        /// <summary>
        ///     The _dest.
        /// </summary>
        private IDiffList dest;

        /// <summary>
        ///     The _level.
        /// </summary>
        private DiffEngineLevel level;

        /// <summary>
        ///     The _match list.
        /// </summary>
        private ArrayList matchList;

        /// <summary>
        ///     The _source.
        /// </summary>
        private IDiffList source;

        /// <summary>
        ///     The _state list.
        /// </summary>
        private DiffStateList stateList;

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     The diff report.
        /// </summary>
        /// <returns>
        ///     The <see cref="ArrayList" />.
        /// </returns>
        public ArrayList DiffReport()
        {
            var retval = new ArrayList();
            int dcount = this.dest.Count();
            int scount = this.source.Count();

            // Deal with the special case of empty files
            if (dcount == 0)
            {
                if (scount > 0)
                {
                    retval.Add(DiffResultSpan.CreateDeleteSource(0, scount));
                }

                return retval;
            }

            if (scount == 0)
            {
                retval.Add(DiffResultSpan.CreateAddDestination(0, dcount));
                return retval;
            }

            this.matchList.Sort();
            int curDest = 0;
            int curSource = 0;
            DiffResultSpan last = null;

            // Process each match record
            foreach (DiffResultSpan drs in this.matchList)
            {
                if ((!AddChanges(retval, curDest, drs.DestIndex, curSource, drs.SourceIndex)) && (last != null))
                {
                    last.AddLength(drs.Length);
                }
                else
                {
                    retval.Add(drs);
                }

                curDest = drs.DestIndex + drs.Length;
                curSource = drs.SourceIndex + drs.Length;
                last = drs;
            }

            // Process any tail end data
            AddChanges(retval, curDest, dcount, curSource, scount);

            return retval;
        }

        /// <summary>
        /// The process diff.
        /// </summary>
        /// <param name="sourceIn">
        /// The source in.
        /// </param>
        /// <param name="destination">
        /// The destination.
        /// </param>
        /// <param name="levelIn">
        /// The level in.
        /// </param>
        /// <returns>
        /// The <see cref="double"/>.
        /// </returns>
        public double ProcessDiff(IDiffList sourceIn, IDiffList destination, DiffEngineLevel levelIn)
        {
            this.level = levelIn;
            return this.ProcessDiff(sourceIn, destination);
        }

        /// <summary>
        /// The process diff.
        /// </summary>
        /// <param name="sourceIn">
        /// The source.
        /// </param>
        /// <param name="destination">
        /// The destination.
        /// </param>
        /// <returns>
        /// The <see cref="double"/>.
        /// </returns>
        public double ProcessDiff(IDiffList sourceIn, IDiffList destination)
        {
            DateTime dt = DateTime.Now;
            this.source = sourceIn;
            this.dest = destination;
            this.matchList = new ArrayList();

            int dcount = this.dest.Count();
            int scount = this.source.Count();

            if ((dcount > 0) && (scount > 0))
            {
                this.stateList = new DiffStateList(dcount);
                this.ProcessRange(0, dcount - 1, 0, scount - 1);
            }

            TimeSpan ts = DateTime.Now - dt;
            return ts.TotalSeconds;
        }

        #endregion

        #region Methods

        /// <summary>
        /// The add changes.
        /// </summary>
        /// <param name="report">
        /// The report.
        /// </param>
        /// <param name="curDest">
        /// The cur dest.
        /// </param>
        /// <param name="nextDest">
        /// The next dest.
        /// </param>
        /// <param name="curSource">
        /// The cur source.
        /// </param>
        /// <param name="nextSource">
        /// The next source.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private static bool AddChanges(ArrayList report, int curDest, int nextDest, int curSource, int nextSource)
        {
            var retval = false;
            var diffDest = nextDest - curDest;
            var diffSource = nextSource - curSource;
            if (diffDest > 0)
            {
                if (diffSource > 0)
                {
                    int minDiff = Math.Min(diffDest, diffSource);
                    report.Add(DiffResultSpan.CreateReplace(curDest, curSource, minDiff));
                    if (diffDest > diffSource)
                    {
                        curDest += minDiff;
                        report.Add(DiffResultSpan.CreateAddDestination(curDest, diffDest - diffSource));
                    }
                    else
                    {
                        if (diffSource > diffDest)
                        {
                            curSource += minDiff;
                            report.Add(DiffResultSpan.CreateDeleteSource(curSource, diffSource - diffDest));
                        }
                    }
                }
                else
                {
                    report.Add(DiffResultSpan.CreateAddDestination(curDest, diffDest));
                }

                retval = true;
            }
            else
            {
                if (diffSource > 0)
                {
                    report.Add(DiffResultSpan.CreateDeleteSource(curSource, diffSource));
                    retval = true;
                }
            }

            return retval;
        }

        /// <summary>
        /// The get longest source match.
        /// </summary>
        /// <param name="curItem">
        /// The cur item.
        /// </param>
        /// <param name="destIndex">
        /// The dest index.
        /// </param>
        /// <param name="destEnd">
        /// The dest end.
        /// </param>
        /// <param name="sourceStart">
        /// The source start.
        /// </param>
        /// <param name="sourceEnd">
        /// The source end.
        /// </param>
        private void GetLongestSourceMatch(
            DiffState curItem, 
            int destIndex, 
            int destEnd, 
            int sourceStart, 
            int sourceEnd)
        {
            var maxDestLength = (destEnd - destIndex) + 1;
            var curBestLength = 0;
            var curBestIndex = -1;
            for (var sourceIndex = sourceStart; sourceIndex <= sourceEnd; sourceIndex++)
            {
                var maxLength = Math.Min(maxDestLength, (sourceEnd - sourceIndex) + 1);
                if (maxLength <= curBestLength)
                {
                    // No chance to find a longer one any more
                    break;
                }

                var curLength = this.GetSourceMatchLength(destIndex, sourceIndex, maxLength);
                if (curLength > curBestLength)
                {
                    // This is the best match so far
                    curBestIndex = sourceIndex;
                    curBestLength = curLength;
                }

                // jump over the match
                sourceIndex += curBestLength;
            }

            if (curBestIndex == -1)
            {
                curItem.SetNoMatch();
            }
            else
            {
                curItem.SetMatch(curBestIndex, curBestLength);
            }
        }

        /// <summary>
        /// The get source match length.
        /// </summary>
        /// <param name="destIndex">
        /// The dest index.
        /// </param>
        /// <param name="sourceIndex">
        /// The source index.
        /// </param>
        /// <param name="maxLength">
        /// The max length.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        private int GetSourceMatchLength(int destIndex, int sourceIndex, int maxLength)
        {
            int matchCount;
            for (matchCount = 0; matchCount < maxLength; matchCount++)
            {
                if (
                    this.dest.GetByIndex(destIndex + matchCount)
                        .CompareTo(this.source.GetByIndex(sourceIndex + matchCount)) != 0)
                {
                    break;
                }
            }

            return matchCount;
        }

        /// <summary>
        /// The process range.
        /// </summary>
        /// <param name="destStart">
        /// The dest start.
        /// </param>
        /// <param name="destEnd">
        /// The dest end.
        /// </param>
        /// <param name="sourceStart">
        /// The source start.
        /// </param>
        /// <param name="sourceEnd">
        /// The source end.
        /// </param>
        private void ProcessRange(int destStart, int destEnd, int sourceStart, int sourceEnd)
        {
            var curBestIndex = -1;
            var curBestLength = -1;
            DiffState bestItem = null;
            for (int destIndex = destStart; destIndex <= destEnd; destIndex++)
            {
                int maxPossibleDestLength = (destEnd - destIndex) + 1;
                if (maxPossibleDestLength <= curBestLength)
                {
                    // we won't find a longer one even if we looked
                    break;
                }

                DiffState curItem = this.stateList.GetByIndex(destIndex);

                if (!curItem.HasValidLength(sourceStart, sourceEnd, maxPossibleDestLength))
                {
                    // recalc new best length since it isn't valid or has never been done.
                    this.GetLongestSourceMatch(curItem, destIndex, destEnd, sourceStart, sourceEnd);
                }

                if (curItem.Status == DiffStatus.Matched)
                {
                    switch (this.level)
                    {
                        case DiffEngineLevel.FastImperfect:
                            if (curItem.Length > curBestLength)
                            {
                                // this is longest match so far
                                curBestIndex = destIndex;
                                curBestLength = curItem.Length;
                                bestItem = curItem;
                            }

                            // Jump over the match 
                            destIndex += curItem.Length - 1;
                            break;
                        case DiffEngineLevel.Medium:
                            if (curItem.Length > curBestLength)
                            {
                                // this is longest match so far
                                curBestIndex = destIndex;
                                curBestLength = curItem.Length;
                                bestItem = curItem;

                                // Jump over the match 
                                destIndex += curItem.Length - 1;
                            }

                            break;
                        default:
                            if (curItem.Length > curBestLength)
                            {
                                // this is longest match so far
                                curBestIndex = destIndex;
                                curBestLength = curItem.Length;
                                bestItem = curItem;
                            }

                            break;
                    }
                }
            }

            if (curBestIndex < 0)
            {
                // we are done - there are no matches in this span
            }
            else
            {
                if (bestItem != null)
                {
                    var sourceIndex = bestItem.StartIndex;
                    this.matchList.Add(DiffResultSpan.CreateNoChange(curBestIndex, sourceIndex, curBestLength));
                    if (destStart < curBestIndex)
                    {
                        // Still have more lower destination data
                        if (sourceStart < sourceIndex)
                        {
                            // Still have more lower source data
                            // Recursive call to process lower indexes
                            this.ProcessRange(destStart, curBestIndex - 1, sourceStart, sourceIndex - 1);
                        }
                    }

                    int upperDestStart = curBestIndex + curBestLength;
                    int upperSourceStart = sourceIndex + curBestLength;
                    if (destEnd > upperDestStart)
                    {
                        // we still have more upper dest data
                        if (sourceEnd > upperSourceStart)
                        {
                            // set still have more upper source data
                            // Recursive call to process upper indexes
                            this.ProcessRange(upperDestStart, destEnd, upperSourceStart, sourceEnd);
                        }
                    }
                }
            }
        }

        #endregion
    }
}