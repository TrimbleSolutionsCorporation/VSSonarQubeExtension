// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GitExtensionBorrow.cs" company="Copyright © 2015 jmecsoftware">
//     Copyright (C) 2014 [jmecsoftware, jmecsoftware2014@tekla.com]
// </copyright>
// 
// This has been borrowed from https://github.com/gitextensions/gitextensions/blob/master/GitCommands/Git/GitModule.cs
// Unit libsharp2git allows to use ignore space in blame
// --------------------------------------------------------------------------------------------------------------------
// This program is free software; you can redistribute it and/or modify it under the terms of the GNU Lesser General Public License
// as published by the Free Software Foundation; either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
// of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details. 
// You should have received a copy of the GNU Lesser General Public License along with this program; if not, write to the Free
// Software Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQGitPlugin
{
    /// <summary>
    /// 
    /// </summary>
    public class GitExtensionsFunctions
    {
        /// <summary>
        /// Froms the unix time.
        /// </summary>
        /// <param name="unixTime">The unix time.</param>
        /// <returns>
        /// DateTime from epoch.
        /// </returns>
        private static DateTime FromUnixTime(long unixTime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddSeconds(unixTime);
        }

        public static string ReEncodeFileNameFromLossless(string fileName)
        {
            fileName = ReEncodeStringFromLossless(fileName, Encoding.Default);
            return UnquoteFileName(fileName);
        }

        //Encoding that let us read all bytes without replacing any char
        //It is using to read output of commands, which may consist of:
        //1) commit header (message, author, ...) encoded in CommitEncoding, recoded to LogOutputEncoding or not dependent of
        //   pretty parameter (pretty=raw - recoded, pretty=format - not recoded)
        //2) file content encoded in its original encoding
        //3) file path (file name is encoded in system default encoding),
        //   when core.quotepath is on, every non ASCII character is escaped
        //   with \ followed by its code as a three digit octal number
        //4) branch, tag name, errors, warnings, hints encoded in system default encoding
        public static readonly Encoding LosslessEncoding = Encoding.GetEncoding("ISO-8859-1");//is any better?

        public static string ReEncodeString(string s, Encoding fromEncoding, Encoding toEncoding)
        {
            if (s == null || fromEncoding.HeaderName.Equals(toEncoding.HeaderName))
                return s;
            else
            {
                byte[] bytes = fromEncoding.GetBytes(s);
                s = toEncoding.GetString(bytes);
                return s;
            }
        }


        /// <summary>
        /// reencodes string from GitCommandHelpers.LosslessEncoding to toEncoding
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string ReEncodeStringFromLossless(string s, Encoding toEncoding)
        {
            if (toEncoding == null)
                return s;
            return ReEncodeString(s, LosslessEncoding, toEncoding);
        }

        public static string ReEncodeStringFromLossless(string s)
        {
            return ReEncodeStringFromLossless(s, Encoding.Default);
        }

        public static string UnquoteFileName(string fileName)
        {
            char[] chars = fileName.ToCharArray();
            IList<byte> blist = new List<byte>();
            int i = 0;
            StringBuilder sb = new StringBuilder();
            while (i < chars.Length)
            {
                char c = chars[i];
                if (c == '\\')
                {
                    //there should be 3 digits
                    if (chars.Length >= i + 3)
                    {
                        string octNumber = "" + chars[i + 1] + chars[i + 2] + chars[i + 3];

                        try
                        {
                            int code = Convert.ToInt32(octNumber, 8);
                            blist.Add((byte)code);
                            i += 4;
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
                else
                {
                    if (blist.Count > 0)
                    {
                        sb.Append(Encoding.Default.GetString(blist.ToArray()));
                        blist.Clear();
                    }

                    sb.Append(c);
                    i++;
                }
            }
            if (blist.Count > 0)
            {
                sb.Append(Encoding.Default.GetString(blist.ToArray()));
                blist.Clear();
            }
            return sb.ToString();
        }


        /// <summary>
        /// Generates the blame for file.
        /// </summary>
        /// <param name="relativePath">The relative path.</param>
        /// <param name="lines">The lines.</param>
        /// <returns>
        /// Returns command line blame line.
        /// </returns>
        public static GitBlame GenerateBlameForFile(string relativePath, IList<string> lines)
        {
            GitBlame blame = new GitBlame();
            GitBlameHeader blameHeader = null;
            GitBlameLine blameLine = null;

            foreach (var line in lines)
            {
                try
                {
                    //The contents of the actual line is output after the above header, prefixed by a TAB. This is to allow adding more header elements later.
                    if (line.StartsWith("\t"))
                    {
                        blameLine.LineText = line.Substring(1) //trim ONLY first tab
                                                 .Trim(new char[] { '\r' }); //trim \r, this is a workaround for a \r\n bug
                        blameLine.LineText = GitExtensionsFunctions.ReEncodeStringFromLossless(blameLine.LineText, Encoding.Default);
                    }
                    else if (line.StartsWith("author-mail"))
                        blameHeader.AuthorMail = GitExtensionsFunctions.ReEncodeStringFromLossless(line.Substring("author-mail".Length).Trim());
                    else if (line.StartsWith("author-time"))
                        blameHeader.AuthorTime = DateTimeUtils.ParseUnixTime(line.Substring("author-time".Length).Trim());
                    else if (line.StartsWith("author-tz"))
                        blameHeader.AuthorTimeZone = line.Substring("author-tz".Length).Trim();
                    else if (line.StartsWith("author"))
                    {
                        blameHeader = new GitBlameHeader();
                        blameHeader.CommitGuid = blameLine.CommitGuid;
                        blameHeader.Author = GitExtensionsFunctions.ReEncodeStringFromLossless(line.Substring("author".Length).Trim());
                        blame.Headers.Add(blameHeader);
                    }
                    else if (line.StartsWith("committer-mail"))
                        blameHeader.CommitterMail = line.Substring("committer-mail".Length).Trim();
                    else if (line.StartsWith("committer-time"))
                        blameHeader.CommitterTime = DateTimeUtils.ParseUnixTime(line.Substring("committer-time".Length).Trim());
                    else if (line.StartsWith("committer-tz"))
                        blameHeader.CommitterTimeZone = line.Substring("committer-tz".Length).Trim();
                    else if (line.StartsWith("committer"))
                        blameHeader.Committer = GitExtensionsFunctions.ReEncodeStringFromLossless(line.Substring("committer".Length).Trim());
                    else if (line.StartsWith("summary"))
                        blameHeader.Summary = GitExtensionsFunctions.ReEncodeStringFromLossless(line.Substring("summary".Length).Trim());
                    else if (line.StartsWith("filename"))
                        blameHeader.FileName = GitExtensionsFunctions.ReEncodeFileNameFromLossless(line.Substring("filename".Length).Trim());
                    else if (line.IndexOf(' ') == 40) //SHA1, create new line!
                    {
                        blameLine = new GitBlameLine();
                        var headerParams = line.Split(' ');
                        blameLine.CommitGuid = headerParams[0];
                        if (headerParams.Length >= 3)
                        {
                            blameLine.OriginLineNumber = int.Parse(headerParams[1]);
                            blameLine.FinalLineNumber = int.Parse(headerParams[2]);
                        }
                        blame.Lines.Add(blameLine);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }


            return blame;
        }
    }

    public class GitBlame
    {
        public GitBlame()
        {
            Headers = new List<GitBlameHeader>();
            Lines = new List<GitBlameLine>();
        }
        public IList<GitBlameHeader> Headers { get; private set; }
        public IList<GitBlameLine> Lines { get; private set; }

        public GitBlameHeader FindHeaderForCommitGuid(string commitGuid)
        {
            return Headers.First(h => h.CommitGuid == commitGuid);
        }
    }

    public static class DateTimeUtils
    {
        /// <summary>
        /// Midnight 1 January 1970.
        /// </summary>
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static bool TryParseUnixTime(string unixTime, out DateTime result)
        {
            long seconds;
            if (long.TryParse(unixTime, out seconds))
            {
                result = UnixEpoch.AddSeconds(seconds).ToLocalTime();
                return true;
            }

            result = default(DateTime);
            return false;
        }

        public static DateTime ParseUnixTime(string unixTime)
        {
            return UnixEpoch.AddSeconds(long.Parse(unixTime)).ToLocalTime();
        }
    }

    public class GitBlameLine
    {
        //Line
        public string CommitGuid { get; set; }
        public int FinalLineNumber { get; set; }
        public int OriginLineNumber { get; set; }

        public string LineText { get; set; }
    }

    public class GitBlameHeader
    {
        //Header
        public string CommitGuid { get; set; }
        public string AuthorMail { get; set; }
        public DateTime AuthorTime { get; set; }
        public string AuthorTimeZone { get; set; }
        public string Author { get; set; }
        public string CommitterMail { get; set; }
        public DateTime CommitterTime { get; set; }
        public string CommitterTimeZone { get; set; }
        public string Committer { get; set; }
        public string Summary { get; set; }
        public string FileName { get; set; }


        private int GenerateIntFromString(string text)
        {
            int number = 0;
            foreach (char c in text)
            {
                number += (int)c;
            }
            return number;
        }

        public override string ToString()
        {
            StringBuilder toStringValue = new StringBuilder();
            toStringValue.AppendLine("Author: " + Author);
            toStringValue.AppendLine("AuthorTime: " + AuthorTime.ToString());
            toStringValue.AppendLine("Committer: " + Committer);
            toStringValue.AppendLine("CommitterTime: " + CommitterTime.ToString());
            toStringValue.AppendLine("Summary: " + Summary);
            toStringValue.AppendLine();
            toStringValue.AppendLine("FileName: " + FileName);

            return toStringValue.ToString().Trim();
        }

        public override bool Equals(object obj)
        {
            return this == (GitBlameHeader)obj;
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public static bool operator ==(GitBlameHeader x, GitBlameHeader y)
        {
            if (Object.ReferenceEquals(x, y))
                return true;
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;
            return x.Author == y.Author && x.AuthorTime == y.AuthorTime &&
                x.Committer == y.Committer && x.CommitterTime == y.CommitterTime &&
                x.Summary == y.Summary && x.FileName == y.FileName;
        }

        public static bool operator !=(GitBlameHeader x, GitBlameHeader y)
        {
            return !(x == y);
        }
    }
}
