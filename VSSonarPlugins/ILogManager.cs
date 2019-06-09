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
    using System;

    public interface ILogManager
    {
        /// <summary>The report exception.</summary>
        /// <param name="ex">The ex.</param>
        void ReportException(Exception ex);

        /// <summary>The report message.</summary>
        /// <param name="messages">The messages.</param>
        void ReportMessage(Message messages);

        /// <summary>
        /// Writes the exception.
        /// </summary>
        /// <param name="ex">The ex.</param>
        void WriteExceptionToLog(Exception ex);

        /// <summary>
        /// Writes the message.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        void WriteMessageToLog(string msg);
    }
}