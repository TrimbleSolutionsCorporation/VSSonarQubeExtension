// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VsProjectItem.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
//   Copyright (C) 2013 [Jorge Costa, Jorge.Costa@tekla.com]
// </copyright>
// <summary>
//   The vs project item.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace VSSonarPlugins.Types
{
    using SonarRestService.Types;

    public class VsSolutionItem
    {
        /// <summary>Gets or sets the solution path. Full path includes file name</summary>
        public string SolutionPath { get; set; }

        /// <summary>Gets or sets the solution name.</summary>
        public string SolutionName { get; set; }

        public Resource SonarProject { get; set; }
    }
}