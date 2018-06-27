module IssuesService

open FSharp.Data
open FSharp.Data.JsonExtensions
open VSSonarPlugins
open VSSonarPlugins.Types
open SonarRestService
open System
open System.Text
open System.Collections.ObjectModel
open System
open System.Web
open System.Net
open System.IO
open System.Text.RegularExpressions
open System.Linq
open System.Threading
open System.Threading.Tasks

type JsonIssues = JsonProvider<""" {"maxResultsReached":false,"paging":{"pageIndex":1,"pageSize":100,"total":5,"pages":1},"issues":[{"key":"whatever","component":"organization:projectid:directory/filename.cpp","project":"organization:projectid","author":"email@sdsasa.com","rule":"common-c++:InsufficientLineCoverage","status":"OPEN","resolution":"FIXED","severity":"MAJOR","message":"1 more lines of code need to be covered by unit tests to reach the minimum threshold of 40.0% lines coverage.","effortToFix":"1.0","creationDate":"2013-03-07T21:56:08+0200","updateDate":"2013-06-30T21:55:48+0300","closeDate":"2013-06-30T21:55:48+0300"},{"key":"df3c7c99-d6b5-4cec-8248-017201d092b6","component":"GroupId:Drawings:libgr_cloning/gr_cursor.cpp","project":"GroupId:Drawings","rule":"manual:this_is a manual review","status":"CONFIRMED","severity":"MAJOR","message":"please change this","line":4,"assignee":"username","creationDate":"2013-04-08T13:38:53+0300","updateDate":"2013-04-08T13:39:27+0300"},{"key":"22ecf99a-a2a1-419f-8783-48ba8238091e","component":"GroupId:Drawings:libgr_db/grdb_check.cpp","project":"GroupId:Drawings","rule":"manual:try_to detect when size is used without size_t","status":"CONFIRMED","severity":"MAJOR","message":"Try to detect when size is used without size_t","line":35,"reporter":"username","assignee":"username","creationDate":"2012-10-08T10:20:04+0300","updateDate":"2012-10-08T10:20:04+0300"},{"key":"a8f4984b-7be7-4d36-9c15-72e25431665b","component":"GroupId:TeklaStructures:libdialog/dia_option_button.cpp","project":"GroupId:TeklaStructures","rule":"cppcheck:unusedFunction","status":"CONFIRMED","severity":"MINOR","message":"The function 'SetBitmapName' is never used.","line":59,"assignee":"username","creationDate":"2012-11-15T06:59:33+0200","updateDate":"2012-11-20T11:52:57+0200","comments":[{"key":"dfsfdsdfs","login":"username","htmlText":"can you check if these are false positives","createdAt":"2012-11-19T11:51:54+0200"},{"key":"sfgfdgfdd","login":"vepi","htmlText":"Well technically they are not called, because BitmapName and InternalValue are only set by the constructor by the current code. However since diaOptionButton_ci is basically a data container with accessors, I think it's reasonable that there are symmetrical accessors to all the variables, even if they are not currently used.","createdAt":"2012-11-20T09:21:24+0200"},{"key":"gfdgfdfgdf","login":"username","htmlText":"same here","createdAt":"2012-11-20T11:52:57+0200"}]},{"key":"gdfgdfgfd","component":"GroupId:TeklaStructures:libdialog/dia_option_button.cpp","project":"GroupId:TeklaStructures","rule":"cppcheck:unusedFunction","status":"CONFIRMED","severity":"MINOR","message":"The function 'SetInternalValue' is never used.","line":46,"assignee":"username","actionPlan":"fsdfsdfsd","creationDate":"2012-11-15T06:59:33+0200","updateDate":"2012-11-20T11:52:32+0200","comments":[{"key":"sfsdfsdfsd","login":"username","htmlText":"can you check if these are false positives","createdAt":"2012-11-19T11:51:37+0200"},{"key":"gfdgfgdf","login":"vepi","htmlText":"Well technically they are not called, because BitmapName and InternalValue are only set by the constructor by the current code. However since diaOptionButton_ci is basically a data container with accessors, I think it's reasonable that there are symmetrical accessors to all the variables, even if they are not currently used.","createdAt":"2012-11-20T09:21:50+0200"},{"key":"sgdfgdfgdfgd","login":"username","htmlText":"So is there a chance of unit test this? If not you may just mark it as false positive.","createdAt":"2012-11-20T11:52:32+0200"}]},{"key":"46429559-4e6c-455a-96b6-b5c75ecdd4d7","component":"GroupId:Dimensioning:libgr_dim_lib/grdl_dimensioning_adapter.cpp","project":"GroupId:Dimensioning","rule":"cppcheck:incorrectStringBooleanError","status":"CONFIRMED","severity":"MINOR","message":"A boolean comparison with the string literal \"dmlGetSteelDimensioningInstance\" is always true.","line":49,"assignee":"username","creationDate":"2013-02-21T13:06:03+0200","updateDate":"2013-02-22T09:45:06+0200","comments":[{"key":"dfsdfdsfsdd","login":"username","htmlText":"how are we fixing those? do i need to update the tool not to generate this cases?","createdAt":"2013-02-22T09:36:03+0200"},{"key":"fgdfgdf","login":"pafi","htmlText":"Please update the tool if you can restrict this to ASSERT() expressions. Otherwise we might want to change this - this could be changed by having if(test) { ASSERT(!&quot;&lt;message&gt;&quot;) } but this makes it longer (and also messes with the test coverage - condition is never met)","createdAt":"2013-02-22T09:45:06+0200"}]}],"components":[{"key":"GroupId:TeklaStructures:libdialog/dia_option_button.cpp","qualifier":"FIL","name":"dia_option_button.cpp","longName":"libdialog/dia_option_button.cpp"},{"key":"GroupId:Dimensioning:libgr_dim_lib/grdl_dimensioning_adapter.cpp","qualifier":"FIL","name":"grdl_dimensioning_adapter.cpp","longName":"libgr_dim_lib/grdl_dimensioning_adapter.cpp"},{"key":"GroupId:Drawings:libgr_cloning/gr_cursor.cpp","qualifier":"FIL","name":"gr_cursor.cpp","longName":"libgr_cloning/gr_cursor.cpp"},{"key":"GroupId:Drawings:libgr_db/grdb_check.cpp","qualifier":"FIL","name":"grdb_check.cpp","longName":"libgr_db/grdb_check.cpp"}],"projects":[{"key":"GroupId:Drawings","qualifier":"TRK","name":"Drawings","longName":"Drawings"},{"key":"GroupId:Dimensioning","qualifier":"TRK","name":"Dimensioning","longName":"Dimensioning"},{"key":"GroupId:TeklaStructures","qualifier":"TRK","name":"TeklaStructures","longName":"TeklaStructures"}],"rules":[{"key":"manual:this_is a manual review","name":"This is a manual review","desc":"Rule created on the fly. A description should be provided.","status":"READY"},{"key":"cppcheck:incorrectStringBooleanError","name":"Suspicious comparison of boolean with a string literal","desc":"A boolean comparison with the string literal is always true.","status":"READY"},{"key":"cppcheck:unusedFunction","name":"Unused function","desc":"The function is never used.","status":"READY"},{"key":"manual:try_to detect when size is used without size_t","name":"Try to detect when size is used without size_t","desc":"Rule created on the fly. A description should be provided.","status":"READY"}],"users":[{"login":"pafi","name":"Filoche Pascal","active":true,"email":"pascal.filoche@tekla.com"},{"login":"username","name":"Costa Jorge","active":true,"email":"jorge.costa@tekla.com"},{"login":"login1","name":"","active":true},{"login":"vepi","name":"Piril\u00e4 Vesa","active":true,"email":"vesa.pirila@tekla.com"}],"actionPlans":[{"key":"fsfsdfsd","name":"Gate 3 Version  19.1","status":"OPEN","project":"GroupId:TeklaStructures","desc":"Gate 3 Version  19.1","userLogin":"username","createdAt":"2012-10-25T09:23:34+0300","updatedAt":"2013-06-29T13:23:45+0300"}]} """>

type JsonIssue = JsonProvider<""" {"issue":{"key":"whatevervalue","author":"email@sdsasa.com","actionPlan":"fsdfsdfsd","component":"organization:projectid:directory/file.cpp","project":"organization:projectid","line":4,"assignee":"userlogin","rule":"common-c++:InsufficientBranchCoverage","status":"REOPENED", "resolution":"FIXED", "severity":"MINOR","message":"203 more branches need to be covered by unit tests to reach the minimum threshold of 30.0% branch coverage.","effortToFix":"203.0","creationDate":"2013-03-10T12:04:31+0200","closeDate":"2013-06-30T21:55:48+0300","updateDate":"2013-07-18T13:51:07+0300","comments":[{"key":"mdkmkvfsfsdk","login":"login","htmlText":"this is a comment","createdAt":"2013-07-17T22:21:00+0300"},{"key":"fsdfsdf","login":"login","htmlText":"this is another comment","createdAt":"2013-07-17T22:28:40+0300"},{"key":"fsdfsfsd","login":"login1","htmlText":"yet another comment","createdAt":"2013-07-17T22:46:02+0300"},{"key":"fgdfg;sdf","login":"login1","htmlText":"yet another comment","createdAt":"2013-07-17T23:42:44+0300"},{"key":"sdfsdfsd","login":"login1","htmlText":"fdlkflds","createdAt":"2013-07-17T23:43:13+0300"},{"key":"dsfsdgdfg","login":"login1","htmlText":"akskjds","createdAt":"2013-07-17T23:45:20+0300"},{"key":"s'dflksdl","login":"login1","htmlText":"fsdfsd","createdAt":"2013-07-17T23:46:44+0300"},{"key":";dlkf;sldkf;lskd","login":"login1","htmlText":"fsdfsd","createdAt":"2013-07-17T23:55:07+0300"},{"key":"kkslkjdfnv","login":"login1","htmlText":"fgsdgdfghgfgh","createdAt":"2013-07-17T23:55:19+0300"},{"key":"xkjdkiwlkkd","login":"login1","htmlText":"another comment","createdAt":"2013-07-18T10:57:42+0300"},{"key":"dsfdsdfsd","login":"login1","htmlText":"fgfdgdfgdf fgfd","createdAt":"2013-07-18T12:24:31+0300"},{"key":"Csdfsdfsd","login":"login1","htmlText":"sdfdsfsd","createdAt":"2013-07-18T12:25:22+0300"},{"key":"sd,mfs,dfs","login":"login1","htmlText":"sdsadsa asdsa d","createdAt":"2013-07-18T12:26:38+0300"}]}} """>

type JSonarReview = JsonProvider<""" [{"id":2769,"createdAt":"2013-06-17T13:51:28+0300","updatedAt":"2013-06-21T05:09:16+0300","author":"user21","title":"IllegalIncludeDirectories Include File is illegal in this Project e3_common_types.h","status":"CLOSED","severity":"MINOR","resource":"GroupId:project:directory/filename.cpp","line":7,"violationId":91986628,"comments":[]},{"id":2625,"createdAt":"2013-05-08T09:24:22+0300","updatedAt":"2013-05-20T05:18:51+0300","author":"user2","assignee":"tmr","title":"Empty loop bodies should use {} or continue","status":"CLOSED","severity":"MINOR","resource":"GroupId:project:directory/filename.cpp","line":3295,"violationId":84276926,"comments":[]},{"id":64,"createdAt":"2012-07-30T10:25:25+0300","updatedAt":"2013-03-06T12:37:33+0200","author":"user2","title":"The scope of the variable 'err' can be reduced","status":"CLOSED","resolution":"FALSE-POSITIVE","severity":"MINOR","resource":"GroupId:project:directory/filename.cpp","line":4367,"violationId":12721494,"comments":[{"id":63,"updatedAt":"2012-07-30T10:25:25+0300","text":"Value used in macro, not applicable.","author":"user2"}]},{"id":712,"createdAt":"2012-10-02T09:27:17+0300","updatedAt":"2013-01-11T02:17:08+0200","author":"mis","assignee":"mis","title":"Testing this","status":"CLOSED","resolution":"FIXED","severity":"INFO","resource":"GroupId:project:directory/filename.cpp","line":173,"violationId":32393083,"comments":[{"id":489,"updatedAt":"2012-10-02T09:27:46+0300","text":"xgsdgsdfsdf","author":"mis"}]},{"id":713,"createdAt":"2012-10-02T09:28:37+0300","updatedAt":"2013-01-10T02:26:04+0200","author":"mis","assignee":"mis","title":"test","status":"CLOSED","resolution":"FIXED","severity":"MAJOR","resource":"GroupId:project:directory/filename.cpp","line":7,"violationId":32393084,"comments":[{"id":494,"updatedAt":"2012-10-04T08:21:37+0300","text":"Yep. That's toxic.","author":"user1"}]}] """>

type JSonViolation = JsonProvider<""" [{"id":140081595,"message":"2 more lines of code need to be covered by unit tests to reach the minimum threshold of 40.0% lines coverage.","priority":"MAJOR","createdAt":"2013-03-07T21:56:08+0200","rule":{"key":"common-c++:InsufficientLineCoverage","name":"Insufficient line coverage by unit tests"},"resource":{"key":"GroupId:projectId:directory/filename.cpp","name":"filename.cpp","scope":"FIL","qualifier":"FIL","language":"c++"}},{"id":140081594,"message":"9 more branches need to be covered by unit tests to reach the minimum threshold of 30.0% branch coverage.","priority":"MAJOR","createdAt":"2013-03-07T21:56:08+0200","rule":{"key":"common-c++:InsufficientBranchCoverage","name":"Insufficient branch coverage by unit tests"},"resource":{"key":"GroupId:projectId:directory/filename.cpp","name":"filename.cpp","scope":"FIL","qualifier":"FIL","language":"c++"}},{"id":140081586,"message":"Extra space before ( in function call","line":350,"priority":"MINOR","createdAt":"2013-06-25T05:08:29+0300","rule":{"key":"cxxexternal:cpplint.whitespace/parens-2","name":"Extra space before ( in function call"},"resource":{"key":"GroupId:projectId:directory/filename.cpp","name":"filename.cpp","scope":"FIL","qualifier":"FIL","language":"c++"}},{"id":140081585,"message":"Extra space before ( in function call","line":348,"priority":"MINOR","createdAt":"2013-06-25T05:08:29+0300","rule":{"key":"cxxexternal:cpplint.whitespace/parens-2","name":"Extra space before ( in function call"},"resource":{"key":"GroupId:projectId:directory/filename.cpp","name":"filename.cpp","scope":"FIL","qualifier":"FIL","language":"c++"}},{"id":140081576,"message":"The scope of the variable 'PARALLEL_TOLERANCE' can be reduced.","line":595,"priority":"MINOR","createdAt":"2013-03-06T16:40:07+0200","rule":{"key":"cppcheck:variableScope","name":"The scope of the variable can be reduced"},"resource":{"key":"GroupId:projectId:directory/filename.cpp","name":"filename.cpp","scope":"FIL","qualifier":"FIL","language":"c++"}},{"id":140081566,"message":"The scope of the variable 'error' can be reduced.","line":78,"priority":"MINOR","createdAt":"2013-03-06T16:40:07+0200","rule":{"key":"cppcheck:variableScope","name":"The scope of the variable can be reduced"},"resource":{"key":"GroupId:projectId:directory/filename.cpp","name":"filename.cpp","scope":"FIL","qualifier":"FIL","language":"c++"}},{"id":140081571,"message":"The scope of the variable 'Length' can be reduced.","line":335,"priority":"MINOR","createdAt":"2013-03-06T16:40:07+0200","rule":{"key":"cppcheck:variableScope","name":"The scope of the variable can be reduced"},"resource":{"key":"GroupId:projectId:directory/filename.cpp","name":"filename.cpp","scope":"FIL","qualifier":"FIL","language":"c++"}},{"id":140081575,"message":"The scope of the variable 'Angle' can be reduced.","line":561,"priority":"MINOR","createdAt":"2013-03-06T16:40:07+0200","rule":{"key":"cppcheck:variableScope","name":"The scope of the variable can be reduced"},"resource":{"key":"GroupId:projectId:directory/filename.cpp","name":"filename.cpp","scope":"FIL","qualifier":"FIL","language":"c++"}},{"id":140081573,"message":"The scope of the variable 'Bulge' can be reduced.","line":514,"priority":"MINOR","createdAt":"2013-03-06T16:40:07+0200","rule":{"key":"cppcheck:variableScope","name":"The scope of the variable can be reduced"},"resource":{"key":"GroupId:projectId:directory/filename.cpp","name":"filename.cpp","scope":"FIL","qualifier":"FIL","language":"c++"}},{"id":140081581,"message":"Extra space before ( in function call","line":330,"priority":"MINOR","createdAt":"2013-06-25T05:08:29+0300","rule":{"key":"cxxexternal:cpplint.whitespace/parens-2","name":"Extra space before ( in function call"},"resource":{"key":"GroupId:projectId:directory/filename.cpp","name":"filename.cpp","scope":"FIL","qualifier":"FIL","language":"c++"}},{"id":140081578,"message":"Extra space before ( in function call","line":242,"priority":"MINOR","createdAt":"2013-06-25T05:08:29+0300","rule":{"key":"cxxexternal:cpplint.whitespace/parens-2","name":"Extra space before ( in function call"},"resource":{"key":"GroupId:projectId:directory/filename.cpp","name":"filename.cpp","scope":"FIL","qualifier":"FIL","language":"c++"}},{"id":140081580,"message":"Extra space before ( in function call","line":282,"priority":"MINOR","createdAt":"2013-06-25T05:08:29+0300","rule":{"key":"cxxexternal:cpplint.whitespace/parens-2","name":"Extra space before ( in function call"},"resource":{"key":"GroupId:projectId:directory/filename.cpp","name":"filename.cpp","scope":"FIL","qualifier":"FIL","language":"c++"}},{"id":140081577,"message":"transform.hpp already included at 9","line":10,"priority":"MINOR","createdAt":"2013-06-25T05:08:29+0300","rule":{"key":"cxxexternal:cpplint.build/include-0","name":"\"%s\" already included at %s:%s  % (include, filename, include_state[include])"},"resource":{"key":"GroupId:projectId:directory/filename.cpp","name":"filename.cpp","scope":"FIL","qualifier":"FIL","language":"c++"}},{"id":140081579,"message":"Extra space before last semicolon. If this should be an empty statement, use {} instead.","line":250,"priority":"MINOR","createdAt":"2013-06-25T05:08:29+0300","rule":{"key":"cxxexternal:cpplint.whitespace/semicolon-3","name":"Extra space before last semicolon. If this should be an empty   statement, use {} instead."},"resource":{"key":"GroupId:projectId:directory/filename.cpp","name":"filename.cpp","scope":"FIL","qualifier":"FIL","language":"c++"}},{"id":140081582,"message":"Extra space before ( in function call","line":342,"priority":"MINOR","createdAt":"2013-06-25T05:08:29+0300","rule":{"key":"cxxexternal:cpplint.whitespace/parens-2","name":"Extra space before ( in function call"},"resource":{"key":"GroupId:projectId:directory/filename.cpp","name":"filename.cpp","scope":"FIL","qualifier":"FIL","language":"c++"}},{"id":140081584,"message":"Extra space before ( in function call","line":345,"priority":"MINOR","createdAt":"2013-06-25T05:08:29+0300","rule":{"key":"cxxexternal:cpplint.whitespace/parens-2","name":"Extra space before ( in function call"},"resource":{"key":"GroupId:projectId:directory/filename.cpp","name":"filename.cpp","scope":"FIL","qualifier":"FIL","language":"c++"}},{"id":140081572,"message":"The scope of the variable 'Eps' can be reduced.","line":430,"priority":"MINOR","createdAt":"2013-03-06T16:40:07+0200","rule":{"key":"cppcheck:variableScope","name":"The scope of the variable can be reduced"},"resource":{"key":"GroupId:projectId:directory/filename.cpp","name":"filename.cpp","scope":"FIL","qualifier":"FIL","language":"c++"}},{"id":140081583,"message":"Extra space before ( in function call","line":343,"priority":"MINOR","createdAt":"2013-06-25T05:08:29+0300","rule":{"key":"cxxexternal:cpplint.whitespace/parens-2","name":"Extra space before ( in function call"},"resource":{"key":"GroupId:projectId:directory/filename.cpp","name":"filename.cpp","scope":"FIL","qualifier":"FIL","language":"c++"}},{"id":140081567,"message":"The scope of the variable 'distance' can be reduced.","line":79,"priority":"MINOR","createdAt":"2013-03-06T16:40:07+0200","rule":{"key":"cppcheck:variableScope","name":"The scope of the variable can be reduced"},"resource":{"key":"GroupId:projectId:directory/filename.cpp","name":"filename.cpp","scope":"FIL","qualifier":"FIL","language":"c++"}},{"id":140081589,"message":"Extra space before ( in function call","line":360,"priority":"MINOR","createdAt":"2013-06-25T05:08:29+0300","rule":{"key":"cxxexternal:cpplint.whitespace/parens-2","name":"Extra space before ( in function call"},"resource":{"key":"GroupId:projectId:directory/filename.cpp","name":"filename.cpp","scope":"FIL","qualifier":"FIL","language":"c++"}},{"id":140081570,"message":"The scope of the variable 'Fz' can be reduced.","line":334,"priority":"MINOR","createdAt":"2013-03-06T16:40:07+0200","rule":{"key":"cppcheck:variableScope","name":"The scope of the variable can be reduced"},"resource":{"key":"GroupId:projectId:directory/filename.cpp","name":"filename.cpp","scope":"FIL","qualifier":"FIL","language":"c++"}},{"id":140081588,"message":"Extra space before ( in function call","line":354,"priority":"MINOR","createdAt":"2013-06-25T05:08:29+0300","rule":{"key":"cxxexternal:cpplint.whitespace/parens-2","name":"Extra space before ( in function call"},"resource":{"key":"GroupId:projectId:directory/filename.cpp","name":"filename.cpp","scope":"FIL","qualifier":"FIL","language":"c++"}},{"id":140081590,"message":"Extra space before ( in function call","line":375,"priority":"MINOR","createdAt":"2013-06-25T05:08:29+0300","rule":{"key":"cxxexternal:cpplint.whitespace/parens-2","name":"Extra space before ( in function call"},"resource":{"key":"GroupId:projectId:directory/filename.cpp","name":"filename.cpp","scope":"FIL","qualifier":"FIL","language":"c++"}},{"id":140081592,"message":"Extra space before ( in function call","line":377,"priority":"MINOR","createdAt":"2013-06-25T05:08:29+0300","rule":{"key":"cxxexternal:cpplint.whitespace/parens-2","name":"Extra space before ( in function call"},"resource":{"key":"GroupId:projectId:directory/filename.cpp","name":"filename.cpp","scope":"FIL","qualifier":"FIL","language":"c++"}},{"id":140081569,"message":"The scope of the variable 'Fy' can be reduced.","line":334,"priority":"MINOR","createdAt":"2013-03-06T16:40:07+0200","rule":{"key":"cppcheck:variableScope","name":"The scope of the variable can be reduced"},"resource":{"key":"GroupId:projectId:directory/filename.cpp","name":"filename.cpp","scope":"FIL","qualifier":"FIL","language":"c++"}},{"id":140081568,"message":"The scope of the variable 'Fx' can be reduced.","line":334,"priority":"MINOR","createdAt":"2013-03-06T16:40:07+0200","rule":{"key":"cppcheck:variableScope","name":"The scope of the variable can be reduced"},"resource":{"key":"GroupId:projectId:directory/filename.cpp","name":"filename.cpp","scope":"FIL","qualifier":"FIL","language":"c++"}},{"id":140081587,"message":"Extra space before ( in function call","line":352,"priority":"MINOR","createdAt":"2013-06-25T05:08:29+0300","rule":{"key":"cxxexternal:cpplint.whitespace/parens-2","name":"Extra space before ( in function call"},"resource":{"key":"GroupId:projectId:directory/filename.cpp","name":"filename.cpp","scope":"FIL","qualifier":"FIL","language":"c++"}},{"id":140081574,"message":"The scope of the variable 'TotalAngle' can be reduced.","line":561,"priority":"MINOR","createdAt":"2013-03-06T16:40:07+0200","rule":{"key":"cppcheck:variableScope","name":"The scope of the variable can be reduced"},"resource":{"key":"GroupId:projectId:directory/filename.cpp","name":"filename.cpp","scope":"FIL","qualifier":"FIL","language":"c++"}},{"id":140081591,"message":"Extra space before ( in function call","line":376,"priority":"MINOR","createdAt":"2013-06-25T05:08:29+0300","rule":{"key":"cxxexternal:cpplint.whitespace/parens-2","name":"Extra space before ( in function call"},"resource":{"key":"GroupId:projectId:directory/filename.cpp","name":"filename.cpp","scope":"FIL","qualifier":"FIL","language":"c++"}},{"id":140081593,"message":"Namespace should be terminated with // namespace geometry","line":917,"priority":"INFO","createdAt":"2013-06-25T05:08:29+0300","rule":{"key":"cxxexternal:cpplint.readability/namespace-0","name":"Namespace should be terminated with \"// namespace %s\"  %self.name"},"resource":{"key":"GroupId:projectId:directory/filename.cpp","name":"filename.cpp","scope":"FIL","qualifier":"FIL","language":"c++"}}] """>

type JSonIssues = JsonProvider<""" {"version":"4.0","issues":[{"key":"b2474b94-a6ae-4944-8d8d-104a3beff6d0","component":"tekla.CLI:IFCObjectConverter:Converter.cs","line":327,"message":"Refactor this method that has a complexity of 23 (which is greater than 10 authorized).","severity":"MAJOR","rule":"csharpsquid:FunctionComplexity","status":"OPEN","isNew":false,"author":"email@sdsasa.com","creationDate":"2013-08-13T15:50:48+0300","updateDate":"2013-11-28T19:18:08+0200"},{"key":"stylecop:ElementsMustBeDocumented","rule":"ElementsMustBeDocumented","repository":"stylecop"},{"key":"stylecop:UseStringEmptyForEmptyStrings","rule":"UseStringEmptyForEmptyStrings","repository":"stylecop"}]} """>

type JSonIssuesRest = JsonProvider<""" {"maxResultsReached":false,"paging":{"pageIndex":1,"pageSize":200,"total":11,"fTotal":"11","pages":1},"issues":[{"key":"asjdkjashdjas","component":"org.example.csharpplayground:MyLibrary:Calc.cs","componentId":159349,"author":"email@sdsasa.com","project":"org.example.csharpplayground","rule":"stylecop:UsingDirectivesMustBePlacedWithinNamespace","status":"CONFIRMED","severity":"MAJOR","message":"All using directives must be placed inside of the namespace.","line":4,"creationDate":"2014-12-14T16:38:10+0200","resolution":"FIXED","updateDate":"2014-12-16T10:55:05+0200","fUpdateAge":"2 hours"},{"key":"1ad8a103-88f8-434d-9b8a-15f506630b11","component":"org.example.csharpplayground:MyLibrary:Calc.cs","componentId":159349,"project":"org.example.csharpplayground","rule":"stylecop:ElementsMustBeDocumented","status":"CONFIRMED","severity":"MAJOR","message":"The method must have a documentation header.","line":36,"effort":"20min","assignee":"jocs","creationDate":"2014-12-14T16:38:10+0200","updateDate":"2014-12-16T10:55:05+0200","fUpdateAge":"2 hours"},{"key":"20f89fd8-5cfa-4110-b9d3-c6e896f28b9a","component":"org.example.csharpplayground:MyLibrary:Calc.cs","componentId":159349,"project":"org.example.csharpplayground","rule":"stylecop:UsingDirectivesMustBePlacedWithinNamespace","status":"CONFIRMED","severity":"MAJOR","message":"All using directives must be placed inside of the namespace.","line":5,"effort":"10min","assignee":"jocs","creationDate":"2014-12-14T16:38:10+0200","updateDate":"2014-12-16T10:55:05+0200","fUpdateAge":"2 hours"},{"key":"2c704812-3288-46a4-a520-bece7f932bc1","component":"org.example.csharpplayground:MyLibrary:Calc.cs","componentId":159349,"project":"org.example.csharpplayground","rule":"stylecop:ElementsMustBeDocumented","status":"CONFIRMED","severity":"MAJOR","message":"The method must have a documentation header.","line":31,"effort":"20min","assignee":"jocs","creationDate":"2014-12-14T16:38:10+0200","updateDate":"2014-12-16T10:55:05+0200","fUpdateAge":"2 hours"},{"key":"54987465-5ef4-496d-80cc-d87ee5d16f84","component":"org.example.csharpplayground:MyLibrary:Calc.cs","componentId":159349,"project":"org.example.csharpplayground","rule":"stylecop:UsingDirectivesMustBePlacedWithinNamespace","status":"CONFIRMED","severity":"MAJOR","message":"All using directives must be placed inside of the namespace.","line":2,"effort":"10min","assignee":"jocs","creationDate":"2014-12-14T16:38:10+0200","closeDate":"2013-06-30T21:55:48+0300","updateDate":"2014-12-16T12:38:32+0200","fUpdateAge":"13 minutes","tags":["bad-practice","adsa"],"comments":[{"key":"d;fs;ldkfls","login":"jocs","userName":"Costa Jorge","htmlText":"test","markdown":"test","updatable":true,"createdAt":"2014-12-16T12:38:32+0200"}]},{"key":"6d840475-e501-4261-9555-06959ff6a0a1","component":"org.example.csharpplayground:MyLibrary:Calc.cs","componentId":159349,"project":"org.example.csharpplayground","rule":"stylecop:ElementsMustBeDocumented","status":"CONFIRMED","severity":"MAJOR","message":"The method must have a documentation header.","line":21,"effort":"20min","assignee":"jocs","creationDate":"2014-12-14T16:38:10+0200","updateDate":"2014-12-16T10:55:05+0200","fUpdateAge":"2 hours"},{"key":"a5ff0892-7864-4c4f-81ee-99168234cde6","component":"org.example.csharpplayground:MyLibrary:Calc.cs","componentId":159349,"project":"org.example.csharpplayground","rule":"stylecop:ElementMustBeginWithUpperCaseLetter","status":"CONFIRMED","severity":"MAJOR","message":"method names begin with an upper-case letter: horrible_code.","line":31,"debt":"15min","assignee":"jocs","creationDate":"2014-12-14T16:38:10+0200","updateDate":"2014-12-16T10:55:05+0200","fUpdateAge":"2 hours"},{"key":"adf880e6-5e33-432f-afc9-4c1e35454bfa","component":"org.example.csharpplayground:MyLibrary:Calc.cs","componentId":159349,"project":"org.example.csharpplayground","rule":"stylecop:ElementsMustBeDocumented","status":"CONFIRMED","severity":"MAJOR","message":"The class must have a documentation header.","line":9,"debt":"20min","assignee":"jocs","creationDate":"2014-12-14T16:38:10+0200","updateDate":"2014-12-16T10:55:05+0200","fUpdateAge":"2 hours"},{"key":"c886a29c-6cb1-401d-a05e-153e4fb12f76","component":"org.example.csharpplayground:MyLibrary:Calc.cs","componentId":159349,"project":"org.example.csharpplayground","rule":"stylecop:UsingDirectivesMustBePlacedWithinNamespace","status":"CONFIRMED","severity":"MAJOR","message":"All using directives must be placed inside of the namespace.","line":3,"debt":"10min","assignee":"jocs","creationDate":"2014-12-14T16:38:10+0200","updateDate":"2014-12-16T10:55:05+0200","fUpdateAge":"2 hours"},{"key":"dcf8a8d8-401d-49bc-82de-de13152d2030","component":"org.example.csharpplayground:MyLibrary:Calc.cs","componentId":159349,"project":"org.example.csharpplayground","rule":"stylecop:ElementsMustBeDocumented","status":"CONFIRMED","severity":"MAJOR","message":"The method must have a documentation header.","line":11,"debt":"20min","assignee":"jocs","creationDate":"2014-12-14T16:38:10+0200","updateDate":"2014-12-16T10:55:05+0200","fUpdateAge":"2 hours"},{"key":"ed58ccef-0317-4902-b2d1-8922d38dc8f6","component":"org.example.csharpplayground:MyLibrary:Calc.cs","componentId":159349,"project":"org.example.csharpplayground","rule":"stylecop:UsingDirectivesMustBePlacedWithinNamespace","status":"CONFIRMED","severity":"MAJOR","message":"All using directives must be placed inside of the namespace.","line":1,"debt":"10min","assignee":"jocs","creationDate":"2014-12-14T16:38:10+0200","updateDate":"2014-12-16T10:55:05+0200","fUpdateAge":"2 hours"}],"components":[{"key":"org.example.csharpplayground","id":159318,"qualifier":"TRK","name":"C# playground","longName":"C# playground"},{"key":"org.example.csharpplayground:MyLibrary:Calc.cs","id":159349,"qualifier":"FIL","name":"Calc.cs","longName":"Calc.cs","path":"Calc.cs","projectId":159318,"subProjectId":159323},{"key":"org.example.csharpplayground:MyLibrary","id":159323,"qualifier":"BRC","name":"MyLibrary","longName":"MyLibrary","path":"MyLibrary","projectId":159318,"subProjectId":159318}],"projects":[{"key":"org.example.csharpplayground","id":159318,"qualifier":"TRK","name":"C# playground","longName":"C# playground"}],"rules":[{"key":"stylecop:UsingDirectivesMustBePlacedWithinNamespace","name":"Using directives must be placed within namespace","desc":"Using directives must be placed within namespace","status":"READY"},{"key":"stylecop:ElementMustBeginWithUpperCaseLetter","name":"Element must begin with upper case letter","desc":"Element must begin with upper case letter","status":"READY"},{"key":"stylecop:ElementsMustBeDocumented","name":"Elements must be documented","desc":"Elements must be documented","status":"READY"}],"users":[{"login":"jocs","name":"Costa Jorge","active":true,"email":"jorge.costa@tekla.com"}]} """>

type JSonIssuesOld = JsonProvider<""" {"version":"3.4.1","violations_per_resource":
{
"resource0":[{"message":"This assembly is not decorated with the [CLSCompliant] attribute.","severity":"MAJOR","rule_key":"MarkAssemblyWithCLSCompliantRule","rule_repository":"gendarme","rule_name":"MarkAssemblyWithCLSCompliantRule"},{"message":"Sign 'CxxPlugin.Test.dll' with a strong name key.","severity":"MAJOR","rule_key":"AssembliesShouldHaveValidStrongNames","rule_repository":"fxcop","rule_name":"Assemblies should have valid strong names"}],
"resource1":[{"message":"This assembly is not decorated with the [CLSCompliant] attribute.","severity":"MAJOR","rule_key":"MarkAssemblyWithCLSCompliantRule","rule_repository":"gendarme","rule_name":"MarkAssemblyWithCLSCompliantRule"},{"message":"Sign 'CxxPlugin.Test.dll' with a strong name key.","severity":"MAJOR","rule_key":"AssembliesShouldHaveValidStrongNames","rule_repository":"fxcop","rule_name":"Assemblies should have valid strong names"}],
"resource2":[{"message":"This assembly is not decorated with the [CLSCompliant] attribute.","severity":"MAJOR","rule_key":"MarkAssemblyWithCLSCompliantRule","rule_repository":"gendarme","rule_name":"MarkAssemblyWithCLSCompliantRule"},{"message":"Sign 'CxxPlugin.Test.dll' with a strong name key.","severity":"MAJOR","rule_key":"AssembliesShouldHaveValidStrongNames","rule_repository":"fxcop","rule_name":"Assemblies should have valid strong names"}],
"resource3":[{"message":"Only 1 visible types are defined inside this namespace.","severity":"CRITICAL","rule_key":"AvoidSmallNamespaceRule","rule_repository":"gendarme","rule_name":"AvoidSmallNamespaceRule"},{"message":"Only 1 visible types are defined inside this namespace.","severity":"CRITICAL","rule_key":"AvoidSmallNamespaceRule","rule_repository":"gendarme","rule_name":"AvoidSmallNamespaceRule"},{"message":"Only 2 visible types are defined inside this namespace.","severity":"CRITICAL","rule_key":"AvoidSmallNamespaceRule","rule_repository":"gendarme","rule_name":"AvoidSmallNamespaceRule"},{"message":"This assembly is not decorated with the [CLSCompliant] attribute.","severity":"MAJOR","rule_key":"MarkAssemblyWithCLSCompliantRule","rule_repository":"gendarme","rule_name":"MarkAssemblyWithCLSCompliantRule"},{"message":"Consider merging the types defined in 'CxxPlugin' with another namespace.","severity":"MAJOR","rule_key":"AvoidNamespacesWithFewTypes","rule_repository":"fxcop","rule_name":"Avoid namespaces with few types"},{"message":"Consider merging the types defined in 'CxxPlugin.Options' with another namespace.","severity":"MAJOR","rule_key":"AvoidNamespacesWithFewTypes","rule_repository":"fxcop","rule_name":"Avoid namespaces with few types"},{"message":"Consider merging the types defined in 'CxxPlugin.ServerExtensions' with another namespace.","severity":"MAJOR","rule_key":"AvoidNamespacesWithFewTypes","rule_repository":"fxcop","rule_name":"Avoid namespaces with few types"},{"message":"Sign 'CxxPlugin.dll' with a strong name key.","severity":"MAJOR","rule_key":"AssembliesShouldHaveValidStrongNames","rule_repository":"fxcop","rule_name":"Assemblies should have valid strong names"}]
}}
""">

type JSonIssuesDryRun = JsonProvider<""" {"version":"3.7.4","violations_per_resource":{"tekla.tools:CxxPlugin.Test":[{"message":"Sign 'CxxPlugin.Test.dll' with a strong name key.","severity":"MAJOR","rule_key":"AssembliesShouldHaveValidStrongNames","rule_repository":"fxcop","switched_off":false,"is_new":true,"created_at":"2014-01-04T18:07:27+0200"},{"message":"This assembly is not decorated with the [CLSCompliant] attribute.","severity":"MAJOR","rule_key":"MarkAssemblyWithCLSCompliantRule","rule_repository":"gendarme","switched_off":false,"is_new":true,"created_at":"2014-01-04T18:07:27+0200"}],"tekla.tools:CxxPlugin":[{"message":"Sign 'CxxPlugin.dll' with a strong name key.","severity":"MAJOR","rule_key":"AssembliesShouldHaveValidStrongNames","rule_repository":"fxcop","switched_off":false,"is_new":false,"created_at":"2014-01-04T17:46:59+0200"},{"message":"Only 1 visible types are defined inside this namespace.","severity":"CRITICAL","rule_key":"AvoidSmallNamespaceRule","rule_repository":"gendarme","switched_off":false,"is_new":false,"created_at":"2014-01-04T17:46:59+0200"},{"message":"Only 1 visible types are defined inside this namespace.","severity":"CRITICAL","rule_key":"AvoidSmallNamespaceRule","rule_repository":"gendarme","switched_off":false,"is_new":false,"created_at":"2014-01-04T17:46:59+0200"},{"message":"This assembly is not decorated with the [CLSCompliant] attribute.","severity":"MAJOR","rule_key":"MarkAssemblyWithCLSCompliantRule","rule_repository":"gendarme","switched_off":false,"is_new":false,"created_at":"2014-01-04T17:46:59+0200"},{"message":"Consider merging the types defined in 'CxxPlugin' with another namespace.","severity":"MAJOR","rule_key":"AvoidNamespacesWithFewTypes","rule_repository":"fxcop","switched_off":false,"is_new":false,"created_at":"2014-01-04T17:46:59+0200"},{"message":"Only 2 visible types are defined inside this namespace.","severity":"CRITICAL","rule_key":"AvoidSmallNamespaceRule","rule_repository":"gendarme","switched_off":false,"is_new":false,"created_at":"2014-01-04T17:46:59+0200"},{"message":"Consider merging the types defined in 'CxxPlugin.Options' with another namespace.","severity":"MAJOR","rule_key":"AvoidNamespacesWithFewTypes","rule_repository":"fxcop","switched_off":false,"is_new":false,"created_at":"2014-01-04T17:46:59+0200"},{"message":"Consider merging the types defined in 'CxxPlugin.ServerExtensions' with another namespace.","severity":"MAJOR","rule_key":"AvoidNamespacesWithFewTypes","rule_repository":"fxcop","switched_off":false,"is_new":false,"created_at":"2014-01-04T17:46:59+0200"}]}} """>



let GetSeverity(value : string) =
    (EnumHelper.asEnum<Severity>(value)).Value

let getIssueFromString(responsecontent : string) =
    let data = JsonIssue.Parse(responsecontent).Issue
    let issue = new Issue()
    issue.Message <- data.Message
    issue.CreationDate <- data.CreationDate

    issue.Component <- data.Component
    try
        issue.Line <- data.Line
    with
    | ex -> issue.Line <- 0

    issue.Project <- data.Project
    issue.UpdateDate <- data.UpdateDate
    issue.Status <- (EnumHelper.asEnum<IssueStatus>(data.Status)).Value
    issue.Severity <- GetSeverity(data.Severity)
    issue.Rule <- data.Rule
    issue.Key <- data.Key.ToString()

    if not(obj.ReferenceEquals(data.JsonValue.TryGetProperty("assignee"), null)) then
        issue.Assignee <- data.Assignee

    if not(obj.ReferenceEquals(data.JsonValue.TryGetProperty("author"), null)) then
        issue.Author <- data.Author

    if not(obj.ReferenceEquals(data.JsonValue.TryGetProperty("comments"), null)) then
        for elemC in data.Comments do issue.Comments.Add(new Comment(elemC.CreatedAt, elemC.HtmlText, elemC.Key, elemC.Login, -1))

    if not(obj.ReferenceEquals(data.JsonValue.TryGetProperty("closeDate"), null)) then
        issue.CloseDate <- data.CloseDate

    if not(obj.ReferenceEquals(data.JsonValue.TryGetProperty("resolution"), null)) then
        issue.Resolution <- (EnumHelper.asEnum<Resolution>(data.Resolution.Replace("-","_"))).Value

    if issue.Comments.Count <> 0 then
        for comment in issue.Comments do
            if comment.HtmlText.StartsWith("[VSSonarQubeExtension] Attached to issue: ") then
                for item in Regex.Matches(comment.HtmlText, "\\d+") do
                    if issue.IssueTrackerId = null || issue.IssueTrackerId = "" then
                        issue.IssueTrackerId <- item.Value
    issue

let getIssuesFromStringAfter45(responsecontent : string) =
    let data = JSonIssuesRest.Parse(responsecontent)
    let issueList = new System.Collections.Generic.List<Issue>()
    for elem in data.Issues do
        let issue = new Issue()
        issue.Message <- elem.Message
        issue.CreationDate <- elem.CreationDate

        issue.Component <- elem.Component
        try
            if not(obj.ReferenceEquals(elem.Line, null)) then
                issue.Line <- elem.Line
            else
                issue.Line <- 0
        with
        | ex -> ()

        issue.Project <- elem.Project
        issue.UpdateDate <- elem.UpdateDate
        issue.Status <- (EnumHelper.asEnum<IssueStatus>(elem.Status)).Value
        issue.Severity <- GetSeverity(elem.Severity)
        issue.Rule <- elem.Rule
        issue.Key <- elem.Key.ToString().Replace("\"", "")

        if not(obj.ReferenceEquals(elem.Assignee, null)) then
            match elem.Assignee with
            | None -> ()
            | Some value -> issue.Assignee <- value

        if not(obj.ReferenceEquals(elem.Author, null)) then
            match elem.Author with
            | None -> ()
            | Some value -> issue.Author <- value

        if not(obj.ReferenceEquals(elem.Comments, null)) then
            for elemC in elem.Comments do issue.Comments.Add(new Comment(elemC.CreatedAt, elemC.HtmlText, elemC.Key, elemC.Login, -1))

        if not(obj.ReferenceEquals(elem.CloseDate, null)) then
            match elem.CloseDate with
            | None -> ()
            | Some value -> issue.CloseDate <- value

        if not(obj.ReferenceEquals(elem.Resolution, null)) then
            match elem.Resolution with
            | None -> ()
            | Some value -> issue.Resolution <- (EnumHelper.asEnum<Resolution>(value.Replace("-", "_"))).Value

        if not(obj.ReferenceEquals(elem.Effort, null)) then
            match elem.Effort with
            | None -> ()
            | Some value -> issue.Effort <- value

        if not(obj.ReferenceEquals(elem.Tags, null)) then
            for tag in elem.Tags do
                issue.Tags.Add(tag) |> ignore

        if issue.Comments.Count <> 0 then
            for comment in issue.Comments do
                if comment.HtmlText.StartsWith("[VSSonarQubeExtension] Attached to issue: ") then
                    for item in Regex.Matches(comment.HtmlText, "\\d+") do
                        if issue.IssueTrackerId = null || issue.IssueTrackerId = "" then
                            issue.IssueTrackerId <- item.Value

        issueList.Add(issue)

    issueList

let getIssuesFromString(responsecontent : string) =
    let data = JsonIssues.Parse(responsecontent)
    let issueList = new System.Collections.Generic.List<Issue>()
    for elem in data.Issues do
        try
            let issue = new Issue()
            issue.Message <- elem.Message
            issue.CreationDate <- elem.CreationDate

            issue.Component <- elem.Component
            if not(obj.ReferenceEquals(elem.Line, null)) then
                issue.Line <- elem.Line.Value
            else
                issue.Line <- 0

            issue.Project <- elem.Project
            issue.UpdateDate <- elem.UpdateDate
            issue.Status <- (EnumHelper.asEnum<IssueStatus>(elem.Status)).Value
            issue.Severity <- GetSeverity(elem.Severity)
            issue.Rule <- elem.Rule
            issue.Key <- elem.Key.ToString()

            if not(obj.ReferenceEquals(elem.Assignee, null)) then
                issue.Assignee <- elem.Assignee.Value

            if not(obj.ReferenceEquals(elem.Author, null)) then
                issue.Author <- elem.Author.Value
            
            if not(obj.ReferenceEquals(elem.Comments, null)) then
                for elemC in elem.Comments do issue.Comments.Add(new Comment(elemC.CreatedAt, elemC.HtmlText, elemC.Key, elemC.Login, -1))

            if not(obj.ReferenceEquals(elem.CloseDate, null)) then
                issue.CloseDate <- elem.CloseDate.Value

            if not(obj.ReferenceEquals(elem.Resolution, null)) then
                issue.Resolution <- (EnumHelper.asEnum<Resolution>(elem.Resolution.Value.Replace("-", "_"))).Value

            if issue.Comments.Count <> 0 then
                for comment in issue.Comments do
                    if comment.HtmlText.StartsWith("[VSSonarQubeExtension] Attached to issue: ") then
                        for item in Regex.Matches(comment.HtmlText, "\\d+") do
                            if issue.IssueTrackerId = null || issue.IssueTrackerId = "" then
                                issue.IssueTrackerId <- item.Value

            issueList.Add(issue)
        with
        | ex -> ()

    issueList

let getViolationsFromString(responsecontent : string, reviewAsIssues : System.Collections.Generic.List<Issue>) =
    let data = JSonViolation.Parse(responsecontent)
    let issueList = new System.Collections.Generic.List<Issue>()
    for elem in data do
        let issue = new Issue()
        issue.CreationDate <- elem.CreatedAt
        issue.Component <- elem.Resource.Key
        issue.Id <- elem.Id
        issue.Message <- elem.Message
        if not(obj.ReferenceEquals(elem.Line, null)) then
            issue.Line <- elem.Line.Value
        else
            issue.Line <- 0

        issue.Severity <- GetSeverity(elem.Priority)
        issue.Status <- IssueStatus.OPEN

        // convert violation into review if a review is present
        for review in reviewAsIssues do
            if review.Message = issue.Message && review.Line = issue.Line then
                issue.Id <- review.Id
                issue.Assignee <- review.Assignee
                issue.Resolution <- review.Resolution
                issue.Status <- review.Status
                for comment in review.Comments do
                    issue.Comments.Add(comment)
                    
        issueList.Add(issue)
    issueList
        
let getReviewsFromString(responsecontent : string) =
    let data = JSonarReview.Parse(responsecontent)
    let issueList = new System.Collections.Generic.List<Issue>()
    for elem in data do
        let issue = new Issue()
        issue.Message <- elem.Title
        issue.CreationDate <- elem.CreatedAt

        issue.Component <- elem.Resource
        try
            issue.Line <- elem.Line
        with
        | ex -> issue.Line <- 0

        let keyelems = elem.Resource.Split(':')
        issue.Project <-  keyelems.[0] + ":" + keyelems.[1] 
        issue.UpdateDate <- elem.UpdatedAt
        issue.Status <- (EnumHelper.asEnum<IssueStatus>(elem.Status)).Value
        issue.Severity <- GetSeverity(elem.Severity)
        issue.Id <- elem.Id
        issue.Rule <- ""
        let violationId = elem.ViolationId
        issue.ViolationId <- violationId
        try
            issue.Assignee <- elem.Assignee.Value
        with
        | ex -> ()            

        match elem.JsonValue.TryGetProperty("comments") with
        | NotNull ->
            for elemC in elem.Comments do issue.Comments.Add(new Comment(elemC.UpdatedAt, elemC.Text, "", elemC.Author, elem.Id))
        | _ -> ()

        issueList.Add(issue)

    issueList

let SearchForIssues(userConf:ISonarConfiguration,
                    url : string,
                    token:CancellationToken,
                    httpconnector:IHttpSonarConnector,
                    logger:INotificationManager) = 

    try
        let allIssues = new System.Collections.Generic.List<Issue>()
        try
            let responsecontent = httpconnector.HttpSonarGetRequest(userConf, url)
            let data = JSonIssuesRest.Parse(responsecontent)

            let AddElements(all : System.Collections.Generic.List<Issue>) = 
                for issue in all do
                    allIssues.Add(issue)

            AddElements(getIssuesFromStringAfter45(responsecontent))

            // we need to get all pages
            let value = int(System.Math.Ceiling(float(data.Paging.Total) / float(data.Paging.PageSize)))

            for i = 2 to value do
                logger.ReportMessage((sprintf "Request Page: %i of %i" i value))
                if not(token.IsCancellationRequested) then
                    try
                        let url = url + "&pageIndex=" + Convert.ToString(i)
                        let newresponse = httpconnector.HttpSonarGetRequest(userConf, url)
                        AddElements(getIssuesFromStringAfter45(newresponse))
                    with | ex -> ()
        with
        | ex -> 
            let responsecontent = httpconnector.HttpSonarGetRequest(userConf, url)
            let data = JsonIssues.Parse(responsecontent)

            let AddElements(all : System.Collections.Generic.List<Issue>) = 
                for issue in all do
                    allIssues.Add(issue)

            AddElements(getIssuesFromString(responsecontent))

            // we need to get all pages
            
            for i = 2 to data.Paging.Pages do
                logger.ReportMessage((sprintf "Request Page: %i of %i" i data.Paging.Pages))
                if not(token.IsCancellationRequested) then
                    let url = url + "&pageIndex=" + Convert.ToString(i)
                    let newresponse = httpconnector.HttpSonarGetRequest(userConf, url)
                    AddElements(getIssuesFromString(newresponse))
        allIssues
    with
    | ex -> logger.ReportMessage("Failed to Collect Issues For Resource: " + ex.Message + " : " + url)
            System.Collections.Generic.List<Issue>()


let SearchForIssuesInResource(userConf:ISonarConfiguration,
                              resource : string,
                              httpconnector:IHttpSonarConnector,
                              logger:INotificationManager) = 
    let url =  "/api/issues/search?components=" + resource + "&statuses=OPEN,CONFIRMED,REOPENED"
    try
        let responsecontent = httpconnector.HttpSonarGetRequest(userConf, url)
        try getIssuesFromStringAfter45(responsecontent) with | ex -> getIssuesFromString(responsecontent)
    with
    | ex ->
        logger.ReportMessage("Failed to Collect Issues For Resource: " + ex.Message + " : " + url)
        System.Collections.Generic.List<Issue>()


let PerformWorkFlowTransition(userconf : ISonarConfiguration, issue : Issue, transition : string, httpconnector:IHttpSonarConnector) =
    let parameters = Map.empty.Add("issue", issue.Key.ToString()).Add("transition", transition)
    httpconnector.HttpSonarPostRequest(userconf, "/api/issues/do_transition", parameters)

let DoStateTransition(userconf : ISonarConfiguration, issue : Issue, finalState : IssueStatus, transition : string, httpconnector:IHttpSonarConnector) = 
    let mutable status = Net.HttpStatusCode.OK

    let response = PerformWorkFlowTransition(userconf, issue, transition, httpconnector)
    status <- response.StatusCode
    if status = Net.HttpStatusCode.OK then
        let data = getIssueFromString(response.Content)
        if data.Status.Equals(finalState) then
            issue.Status <- data.Status
            issue.Resolution <- data.Resolution
        else
            status <- Net.HttpStatusCode.BadRequest

    status

let SetIssueTags(conf : ISonarConfiguration, issue : Issue, tags : System.Collections.Generic.List<string>, httpconnector:IHttpSonarConnector, token:CancellationToken, logger:INotificationManager) =
    let url = "/api/issues/set_tags"
    let tags = List.ofSeq tags |> String.concat ","
    try
        let parameters = Map.empty.Add("key", issue.Key).Add("tags", tags)
        let reply = httpconnector.HttpSonarPostRequest(conf, url, parameters)
        reply.StatusCode.ToString()
    with
    | ex -> logger.ReportMessage("Failed to set tags: " +  ex.Message)
            logger.ReportMessage(ex.StackTrace)
            "Nok"

let GetAvailableTags(newConf : ISonarConfiguration, httpconnector:IHttpSonarConnector, token:CancellationToken, logger:INotificationManager) =
    let url = "/api/issues/tags?ps=100"
    let listOfTags = new System.Collections.Generic.List<string>();
    try
        let responsecontent = httpconnector.HttpSonarGetRequest(newConf, url)
        let tags = JsonTags.Parse(responsecontent)
        for tag in tags.Tags do
            listOfTags.Add(tag)
    with
    | ex -> logger.ReportMessage("Failed to get tags: " + ex.Message)
            logger.ReportMessage("ex: " + ex.StackTrace)

    listOfTags