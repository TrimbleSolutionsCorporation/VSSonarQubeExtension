namespace SonarRestService.Test

open NUnit.Framework
open FsUnit
open SonarRestService

type IssuesTest() =

    [<Test>]
    member test.``Should Report Correct Number of Issues`` () = 
        let userConf = new ConnectionConfiguration("http://localhost:9000", "jocs1", "jocs1");
        SonarRestService().GetIssuesForComponent(userConf, "tekla.structures.core:Common:libgeometry/vector_utilities.cpp");
        ()
        //task.ExecuteRats mockExecutor "foo bar" "out.xml" |> should be True
