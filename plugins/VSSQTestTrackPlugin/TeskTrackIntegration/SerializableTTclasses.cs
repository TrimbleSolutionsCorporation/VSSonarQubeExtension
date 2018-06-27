// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SerializableTTclasses.cs" company="Copyright © 2015 jmecsoftware">
//     Copyright (C) 2014 [jmecsoftware, jmecsoftware2014@tekla.com]
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
namespace TestTrackConnector
{
    using System;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;

    [Serializable]
    [XmlRoot("SerializableCDefect", Namespace = "", IsNullable = false)]
    public partial class SerializableCDefect : ISerializable
    {
        private CDefect _defect;

        public SerializableCDefect(CDefect defect)
        {
            this._defect = defect;
        }

        public CDefect ToCDefect()
        {
            return this._defect;
        }

        public SerializableCDefect()
        {
            this._defect = new CDefect();

        }
        public SerializableCDefect(SerializationInfo info, StreamingContext ctxt)
            : this()
        {
            this._defect.defectnumber = (long)info.GetValue("defectnumber", typeof(long));
            this._defect.defectnumberSpecified = (bool)info.GetValue("defectnumberSpecified", typeof(bool));
            this._defect.summary = (string)info.GetValue("summary", typeof(string));
            this._defect.state = (string)info.GetValue("state", typeof(string));
            this._defect.disposition = (string)info.GetValue("disposition", typeof(string));
            this._defect.type = (string)info.GetValue("type", typeof(string));
            this._defect.priority = (string)info.GetValue("priority", typeof(string));
            this._defect.product = (string)info.GetValue("product", typeof(string));
            this._defect.component = (string)info.GetValue("component", typeof(string));
            this._defect.reference = (string)info.GetValue("reference", typeof(string));
            this._defect.severity = (string)info.GetValue("severity", typeof(string));
            this._defect.enteredby = (string)info.GetValue("enteredby", typeof(string));
            this._defect.workaround = (string)info.GetValue("workaround", typeof(string));
            this._defect.workaroundInlineAttachList = (CFileAttachment[])info.GetValue("workaroundInlineAttachList", typeof(CFileAttachment[]));
            this._defect.dateentered = (DateTime)info.GetValue("dateentered", typeof(DateTime));
            this._defect.dateenteredSpecified = (bool)info.GetValue("dateenteredSpecified", typeof(bool));
            this._defect.locationaddedfrom = (string)info.GetValue("locationaddedfrom", typeof(string));
            this._defect.datetimecreated = (DateTime)info.GetValue("datetimecreated", typeof(DateTime));
            this._defect.datetimecreatedSpecified = (bool)info.GetValue("datetimecreatedSpecified", typeof(bool));
            this._defect.datetimemodified = (DateTime)info.GetValue("datetimemodified", typeof(DateTime));
            this._defect.datetimemodifiedSpecified = (bool)info.GetValue("datetimemodifiedSpecified", typeof(bool));
            this._defect.createdbyuser = (string)info.GetValue("createdbyuser", typeof(string));
            this._defect.modifiedbyuser = (string)info.GetValue("modifiedbyuser", typeof(string));
            this._defect.actualhourstofix = (double)info.GetValue("actualhourstofix", typeof(double));
            this._defect.actualhourstofixSpecified = (bool)info.GetValue("actualhourstofixSpecified", typeof(bool));
            this._defect.estimatedhours = (double)info.GetValue("estimatedhours", typeof(double));
            this._defect.estimatedhoursSpecified = (bool)info.GetValue("estimatedhoursSpecified", typeof(bool));
            this._defect.remaininghours = (double)info.GetValue("remaininghours", typeof(double));
            this._defect.remaininghoursSpecified = (bool)info.GetValue("remaininghoursSpecified", typeof(bool));
            this._defect.variance = (double)info.GetValue("variance", typeof(double));
            this._defect.varianceSpecified = (bool)info.GetValue("varianceSpecified", typeof(bool));
            this._defect.storypoints = (long)info.GetValue("storypoints", typeof(long));
            this._defect.storypointsSpecified = (bool)info.GetValue("storypointsSpecified", typeof(bool));
            this._defect.percentdone = (long)info.GetValue("percentdone", typeof(long));
            this._defect.percentdoneSpecified = (bool)info.GetValue("percentdoneSpecified", typeof(bool));
            this._defect.reportedbylist = (CReportedByRecord[])info.GetValue("reportedbylist", typeof(CReportedByRecord[]));
            this._defect.eventlist = (CEvent[])info.GetValue("eventlist", typeof(CEvent[]));
            this._defect.pSCCFileList = (CSCCFileRecord[])info.GetValue("pSCCFileList", typeof(CSCCFileRecord[]));
            this._defect.customFieldList = (CField[])info.GetValue("customFieldList", typeof(CField[]));
        }

        //Serialization function.
        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("defectnumber", this._defect.defectnumber);
            info.AddValue("defectnumberSpecified", this._defect.defectnumberSpecified);
            info.AddValue("summary", this._defect.summary);
            info.AddValue("state", this._defect.state);
            info.AddValue("disposition", this._defect.disposition);
            info.AddValue("type", this._defect.type);
            info.AddValue("priority", this._defect.priority);
            info.AddValue("product", this._defect.product);
            info.AddValue("component", this._defect.component);
            info.AddValue("reference", this._defect.reference);
            info.AddValue("severity", this._defect.severity);
            info.AddValue("enteredby", this._defect.enteredby);
            info.AddValue("workaround", this._defect.workaround);
            info.AddValue("workaroundInlineAttachList", this._defect.workaroundInlineAttachList);
            info.AddValue("dateentered", this._defect.dateentered);
            info.AddValue("dateenteredSpecified", this._defect.dateenteredSpecified);
            info.AddValue("locationaddedfrom", this._defect.locationaddedfrom);
            info.AddValue("datetimecreated", this._defect.datetimecreated);
            info.AddValue("datetimecreatedSpecified", this._defect.datetimecreatedSpecified);
            info.AddValue("datetimemodified", this._defect.datetimemodified);
            info.AddValue("datetimemodifiedSpecified", this._defect.datetimemodifiedSpecified);
            info.AddValue("createdbyuser", this._defect.createdbyuser);
            info.AddValue("modifiedbyuser", this._defect.modifiedbyuser);
            info.AddValue("actualhourstofix", this._defect.actualhourstofix);
            info.AddValue("actualhourstofixSpecified", this._defect.actualhourstofixSpecified);
            info.AddValue("estimatedhours", this._defect.estimatedhours);
            info.AddValue("estimatedhoursSpecified", this._defect.estimatedhoursSpecified);
            info.AddValue("remaininghours", this._defect.remaininghours);
            info.AddValue("remaininghoursSpecified", this._defect.remaininghoursSpecified);
            info.AddValue("variance", this._defect.variance);
            info.AddValue("varianceSpecified", this._defect.varianceSpecified);
            info.AddValue("storypoints", this._defect.storypoints);
            info.AddValue("storypointsSpecified", this._defect.storypointsSpecified);
            info.AddValue("percentdone", this._defect.percentdone);
            info.AddValue("percentdoneSpecified", this._defect.percentdoneSpecified);
            info.AddValue("reportedbylist", this._defect.reportedbylist);
            info.AddValue("eventlist", this._defect.eventlist);
            info.AddValue("pSCCFileList", this._defect.pSCCFileList);
            info.AddValue("customFieldList", this._defect.customFieldList);
        }
    }
}
