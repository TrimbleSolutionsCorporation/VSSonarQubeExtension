// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SourceCoverage.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
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
namespace ExtensionTypes
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The source.
    /// </summary>
    public class SourceCoverage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SourceCoverage"/> class. 
        /// Initializes a new instance of the <see cref="Source"/> class.
        /// </summary>
        public SourceCoverage()
        {
            this.LinesHits = new List<LineCoverage>();
            this.BranchHits = new List<BranchCoverage>();
        }

        /// <summary>
        /// Gets or sets the line.
        /// </summary>
        public List<LineCoverage> LinesHits { get; set; }

        /// <summary>
        /// Gets or sets the branch hits.
        /// </summary>
        public List<BranchCoverage> BranchHits { get; set; }

        /// <summary>
        /// The add line coverage data.
        /// </summary>
        /// <param name="coveragedata">
        /// The coveragedata.
        /// </param>
        public void SetLineCoverageData(string coveragedata)
        {
            var hits = coveragedata.Split(';');

            foreach (var hit in hits)
            {
                var elems = hit.Split('=');
                var newline = new LineCoverage { Id = Convert.ToInt32(elems[0]), Hits = Convert.ToInt32(elems[1]) };
                this.LinesHits.Add(newline);
            }
        }

        /// <summary>
        /// The set branch coverage data.
        /// </summary>
        /// <param name="totalconditions">
        /// The totalconditions.
        /// </param>
        /// <param name="coveredconditions">
        /// The coveredconditions.
        /// </param>
        public void SetBranchCoverageData(string totalconditions, string coveredconditions)
        {
            var totalhits = totalconditions.Split(';');
            var coveredhits = coveredconditions.Split(';');

            for (int i = 0; i < totalhits.Length; i++)
            {
                var elemstot = totalhits[i].Split('=');
                var elemscov = coveredhits[i].Split('=');

                if (elemstot[0].Equals(elemscov[0]))
                {
                    var newline = new BranchCoverage
                                      {
                                          Id = Convert.ToInt32(elemstot[0]),
                                          TotalConditions = Convert.ToInt32(elemstot[1]),
                                          CoveredConditions = Convert.ToInt32(elemscov[1])
                                      };

                    this.BranchHits.Add(newline);
                }
            }
        }
    }
}
