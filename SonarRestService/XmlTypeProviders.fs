// --------------------------------------------------------------------------------------------------------------------
// <copyright file="XmlTypeProviders.fs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
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
namespace SonarRestService

open System.Xml.Linq
open FSharp.Data

type DupsData = XmlProvider<""" <duplications>
<g>
    <b s="46" l="90" r="com.tekla.bim:Tekla.WPF:Composition/ImportMany.cs"/>
    <b s="46" l="90" r="com.tekla.bim:Tekla.WPF:Composition/Import.cs"/>
</g>
<g>
    <b s="91" l="38" r="com.tekla.bim:Tekla.WPF:Composition/Placeholder.cs"/>
    <b s="78" l="38" r="com.tekla.bim:Tekla.WPF:Composition/ImportMany.cs"/>
    <b s="78" l="38" r="com.tekla.bim:Tekla.WPF:Composition/Import.cs"/>
</g>
<g>
    <b s="101" l="26" r="com.tekla.bim:Tekla.WPF:Composition/Placeholder.cs"/>
    <b s="95" l="25" r="com.tekla.bim:Tekla.Common.UI.Wpf:Composition/Placeholder.cs"/>
    <b s="88" l="26" r="com.tekla.bim:Tekla.WPF:Composition/ImportMany.cs"/>
    <b s="88" l="26" r="com.tekla.bim:Tekla.WPF:Composition/Import.cs"/></g>
<g>
    <b s="99" l="26" r="com.tekla.bim:Tekla.Common.UI.Wpf:Composition/ImportMany.cs"/>
    <b s="101" l="24" r="com.tekla.bim:Tekla.WPF:Composition/Placeholder.cs"/>
    <b s="99" l="26" r="com.tekla.bim:Tekla.Common.UI.Wpf:Composition/Import.cs"/>
    <b s="95" l="24" r="com.tekla.bim:Tekla.Common.UI.Wpf:Composition/Placeholder.cs"/>
    <b s="88" l="24" r="com.tekla.bim:Tekla.WPF:Composition/ImportMany.cs"/>
    <b s="88" l="24" r="com.tekla.bim:Tekla.WPF:Composition/Import.cs"/></g>
</duplications> """>