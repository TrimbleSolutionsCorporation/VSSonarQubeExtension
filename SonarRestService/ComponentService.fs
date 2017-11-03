module ComponentService

open System

open FSharp.Data
open SonarRestService
open VSSonarPlugins.Types
open System.Diagnostics

type Component = JsonProvider<"""
{
    "paging": {
        "pageIndex": 1,
        "pageSize": 100,
        "total": 11
    },
    "baseComponent": {
        "organization": "default-organization",
        "id": "8583ea75-8de3-471b-a91f-d742b6ad0390",
        "key": "org.group.core:ProjName",
        "name": "ProjName",
        "qualifier": "TRK",
        "tags": [],
        "visibility": "private"
    },
    "components": [{
        "organization": "default-organization",
        "id": "82e699ac-99e7-4a88-b3dc-135a56151e3a",
        "key": "org.group.core:ProjName:ProjName",
        "name": "ProjName",
        "qualifier": "DIR",
        "path": "ProjName"
    },
    {
        "organization": "default-organization",
        "id": "AVlm6oqek-GtPBMdmmbO",
        "key": "truasdas",
        "name": "asdasDB",
        "qualifier": "DIR",
        "path": "asdasDB"
    },    
    {
        "organization": "default-organization",
        "id": "cb0ec785-5735-45f2-82e7-3884ed01b711",
        "key": "org.group.core:ProjName:libProjNamedb/interface",
        "name": "libProjNamedb/interface",
        "qualifier": "DIR",
        "path": "libProjNamedb/interface"
    }]
}
""">

let IndexServerResources(conf : ISonarConfiguration, project : Resource, httpconnector : IHttpSonarConnector) =
    let resourcelist = new Collections.Generic.List<Resource>()
    let rec GetComponentsRec(page:int) = 
        let url = sprintf "/api/components/tree?qualifiers=DIR,TRK,BRC&depth=-1&component=%s&ps=100&p=%i" project.Key page
        let responsecontent = httpconnector.HttpSonarGetRequest(conf, url)
        let resources = Component.Parse(responsecontent)

        for resource in resources.Components do
            try
                let res = Resource()
                res.IdString <- resource.Id.String.Value
                res.Key <- resource.Key
                res.Name <- resource.Name
                res.Qualifier <- resource.Qualifier
                resourcelist.Add(res)
            with
                | ex -> Debug.WriteLine("ex: ", ex.Message)

        if resources.Components.Length > 0 then
            GetComponentsRec(page + 1)
    
    GetComponentsRec(1)
    resourcelist


let SearchProjects(conf : ISonarConfiguration, httpconnector : IHttpSonarConnector) =
    let resourcelist = System.Collections.Generic.List<Resource>()

    let rec GetComponentsRec(page:int) = 
        let url = sprintf "/api/components/search_projects?ps=100&p=%i" page
        let response = httpconnector.HttpSonarGetRequest(conf, url)
        let answer = JsonComponents.Parse(response)

        let ProcessComponents(elem:JsonComponents.Component) =
            let resource = new Resource()
            resource.Key <- elem.Key
            resource.Name <- elem.Name
            let keysElements = elem.Key.Split(':')
            if elem.Name.EndsWith(" " + keysElements.[keysElements.Length - 1]) then
                // this is brancnh
                resource.IsBranch <- true
                resource.BranchName <- keysElements.[keysElements.Length - 1]

            resource.IdType <- elem.Id
            resource.Qualifier <- "TRK"
            resourcelist.Add(resource)
            ()

        answer.Components |> Seq.iter (fun elem -> ProcessComponents(elem))
        let length = answer.Components.Length
        let size = answer.Paging.PageSize

        if answer.Components.Length > 0 then
            GetComponentsRec(page + 1)

    GetComponentsRec(1)
    resourcelist