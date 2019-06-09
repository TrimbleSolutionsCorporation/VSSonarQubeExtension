// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPlugin.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
//   Copyright (C) 2013 [Jorge Costa, Jorge.Costa@tekla.com]
// </copyright>
// <summary>
//   The message.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace VSSonarPlugins
{
    /// <summary>The message.</summary>
    public class Message
    {
        /// <summary>Gets or sets the id.</summary>
        public string Id { get; set; }

        /// <summary>Gets or sets the data.</summary>
        public string Data { get; set; }
    }
}