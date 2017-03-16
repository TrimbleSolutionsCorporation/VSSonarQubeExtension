
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SonarRestService.fs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
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
module MeasuresService

open FSharp.Data
open FSharp.Data.JsonExtensions
open RestSharp
open VSSonarPlugins
open VSSonarPlugins.Types
open System.Collections.ObjectModel
open System
open System.Web
open System.Net
open System.IO
open System.Text.RegularExpressions
open System.Linq

type JsonMeasuresDuplication = JsonProvider<""" {"component":{"id":"AVIw-jgM10T7Uv2c-n9V","key":"ProjectKey:Name:ProjectKey:Name:DE2D0BDB-1E81-4AE4-A1C6-D9F4FCD45E6B:master:File.cs","name":"File.cs","qualifier":"FIL","path":"File.cs","language":"cs","measures":[{"metric":"duplications_data","value":"<duplications><g><b s=\"1173\" l=\"17\" r=\"ProjectKey:Name:ProjectKey:Name:DE2D0BDB-1E81-4AE4-A1C6-D9F4FCD45E6B:master:File.cs\"/><b s=\"1363\" l=\"17\" r=\"ProjectKey:Name:ProjectKey:Name:DE2D0BDB-1E81-4AE4-A1C6-D9F4FCD45E6B:master:File.cs\"/></g><g><b s=\"577\" l=\"66\" r=\"ProjectKey:Name:ProjectKey:Name:DE2D0BDB-1E81-4AE4-A1C6-D9F4FCD45E6B:master:File.cs\"/><b s=\"1804\" l=\"66\" r=\"ProjectKey:Name:ProjectKey:Name:DE2D0BDB-1E81-4AE4-A1C6-D9F4FCD45E6B:master:File.cs\"/></g><g><b s=\"2464\" l=\"29\" r=\"ProjectKey:Name:ProjectKey:Name:DE2D0BDB-1E81-4AE4-A1C6-D9F4FCD45E6B:master:File.cs\"/><b s=\"2547\" l=\"29\" r=\"ProjectKey:Name:ProjectKey:Name:DE2D0BDB-1E81-4AE4-A1C6-D9F4FCD45E6B:master:File.cs\"/></g><g><b s=\"1533\" l=\"19\" r=\"ProjectKey:Name:ProjectKey:Name:DE2D0BDB-1E81-4AE4-A1C6-D9F4FCD45E6B:master:File.cs\"/><b s=\"2028\" l=\"19\" r=\"ProjectKey:Name:ProjectKey:Name:DE2D0BDB-1E81-4AE4-A1C6-D9F4FCD45E6B:master:File.cs\"/></g><g><b s=\"1556\" l=\"17\" r=\"ProjectKey:Name:ProjectKey:Name:DE2D0BDB-1E81-4AE4-A1C6-D9F4FCD45E6B:master:File.cs\"/><b s=\"2048\" l=\"17\" r=\"ProjectKey:Name:ProjectKey:Name:DE2D0BDB-1E81-4AE4-A1C6-D9F4FCD45E6B:master:File.cs\"/></g></duplications>"}]}} """>
type JsonMeasuresCoverage = JsonProvider<""" {"component":{"id":"AVm7rkxzk-GtPBMdopBj","key":"tekla.structures.core:Catalogs:tekla.structures.core:Catalogs:8ABB8A32-C281-412B-8EA7-EA24ACCA2DA4:master:int_profile_geometry.hpp","name":"int_profile_geometry.hpp","qualifier":"FIL","path":"int_profile_geometry.hpp","language":"c++","measures":[{"metric":"coverage_line_hits_data","value":"71=1;72=1;74=1;76=1;77=0;78=0;89=0;95=0;101=0;107=0;354=1;1147=0;1165=1;1167=1;1169=0;1186=1;1191=0"}]}} """ >

let GetCoverageFromMeasures(responsecontent : string) = 
    // "/api/measures/component?componentKey=" + resource + "&metricKeys=coverage_line_hits_data,conditions_by_line,covered_conditions_by_line"
    let source = new SourceCoverage()
    try
        let data = JsonMeasuresCoverage.Parse(responsecontent)
        if data.Component.Measures.Length > 0 then
            source.SetLineCoverageData(data.Component.Measures.[0].Value)
    with
        | ex -> ()

    source


