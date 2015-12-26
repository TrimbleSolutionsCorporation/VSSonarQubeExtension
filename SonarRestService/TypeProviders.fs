// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TypeProviders.fs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
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

open FSharp.Data

type CopyProfileAnswer = JsonProvider<""" {"key":"cs-profile2-77634","name":"profile2","language":"cs","languageName":"C#","isDefault":false,"isInherited":false} """>

type JsonarProfileInheritance = JsonProvider<""" {
  "profile": {
    "key": "xoo-my-bu-profile-23456",
    "name": "My BU Profile",
    "parent": "xoo-my-company-profile-12345",
    "activeRuleCount": 3,
    "overridingRuleCount": 1
  },
  "ancestors": [
    {
      "key": "xoo-my-company-profile-12345",
      "name": "My Company Profile",
      "parent": "xoo-my-group-profile-01234",
      "activeRuleCount": 3
    },
    {
      "key": "xoo-my-group-profile-01234",
      "name": "My Group Profile",
      "activeRuleCount": 2
    }
  ],
  "children": [
    {
      "key": "xoo-for-project-one-34567",
      "name": "For Project One",
      "activeRuleCount": 5
    },
    {
      "key": "xoo-for-project-two-45678",
      "name": "For Project Two",
      "activeRuleCount": 4,
      "overridingRuleCount": 1
    }
  ]
} """>

type JsonTags = JsonProvider<""" {
  "tags": [
    "naming",
    "unused-code",
    "pitfall",
    "convention",
    "security",
    "size",
    "error-handling",
    "multithreading",
    "bug",
    "unused",
    "java8",
    "brain-overload",
    "comment",
    "formatting"
  ]
} """>

type PluginsMessage = JsonProvider<""" {"plugins":[{"key":"csharp","name":"C#","description":"Enable analysis and reporting on C# projects.","version":"4.4-SNAPSHOT","license":"GNU LGPL 3","organizationName":"SonarSource","organizationUrl":"http://www.sonarsource.com","homepageUrl":"http://redirect.sonarsource.com/plugins/csharp.html","issueTrackerUrl":"http://jira.codehaus.org/browse/SONARCS","implementationBuild":"de03f5e6ca851ecf2240651d94df9b2dc345e582"},{"key":"cxx","name":"C++ (Community)","description":"Enable analysis and reporting on c++ projects.","version":"0.9.5-SNAPSHOT","license":"GNU LGPL 3","organizationName":"Waleri Enns","homepageUrl":"https://github.com/SonarOpenCommunity/sonar-cxx/wiki","issueTrackerUrl":"https://github.com/SonarOpenCommunity/sonar-cxx/issues?state=open","implementationBuild":"0"},{"key":"fsharp","name":"F#","description":"Enable analysis and reporting on F# projects.","version":"1.0.RC1","license":"GNU LGPL 3","organizationName":"Jorge Costa and SonarSource","organizationUrl":"http://www.sonarsource.com","homepageUrl":"http://redirect.sonarsource.com/plugins/fsharp.html","issueTrackerUrl":"https://github.com/jmecosta/sonar-fsharp-plugin/issues","implementationBuild":"1d151f79651235403dbf6d1b94e0f997b95619b2"},{"key":"scmgit","name":"Git","description":"Git SCM Provider.","version":"1.1","license":"GNU LGPL 3","organizationName":"SonarSource","organizationUrl":"http://www.sonarsource.com","homepageUrl":"http://redirect.sonarsource.com/plugins/scmgit.html","issueTrackerUrl":"http://jira.codehaus.org/browse/SONARSCGIT","implementationBuild":"21e7329a632904350bb9a2e7f1b17b9967988db8"},{"key":"jira","name":"JIRA","description":"Connects SonarQube to Atlassian JIRA in various ways.","version":"1.2","license":"GNU LGPL 3","organizationName":"SonarSource","organizationUrl":"http://www.sonarsource.com","homepageUrl":"http://docs.codehaus.org/display/SONAR/Jira+Plugin","issueTrackerUrl":"http://jira.codehaus.org/browse/SONARPLUGINS/component/13914","implementationBuild":"71e8002a5e7948ec705648d336e8bb9ab8026c55"},{"key":"java","name":"Java","description":"SonarQube rule engine.","version":"3.7.1","license":"GNU LGPL 3","organizationName":"SonarSource","organizationUrl":"http://www.sonarsource.com","homepageUrl":"http://redirect.sonarsource.com/plugins/java.html","issueTrackerUrl":"http://jira.codehaus.org/browse/SONARJAVA","implementationBuild":"af982dcb9e04d3c0b8570185766b531e33b37948"},{"key":"javascript","name":"JavaScript","description":"Enables analysis of JavaScript projects.","version":"2.8","license":"GNU LGPL 3","organizationName":"SonarSource and Eriks Nukis","organizationUrl":"http://www.sonarsource.com","homepageUrl":"http://redirect.sonarsource.com/plugins/javascript.html","issueTrackerUrl":"https://jira.codehaus.org/browse/SONARJS","implementationBuild":"53ffb46f827d24be6173dc5a44afd74b2c0b4e3f"},{"key":"ldap","name":"LDAP","description":"Delegates authentication to LDAP","version":"1.5.1","license":"GNU LGPL 3","organizationName":"SonarSource","organizationUrl":"http://www.sonarsource.com","homepageUrl":"http://redirect.sonarsource.com/plugins/ldap.html","issueTrackerUrl":"http://jira.sonarsource.com/browse/LDAP","implementationBuild":"8960e08512a3d3ec4d9cf16c4c2c95017b5b7ec5"},{"key":"motionchart","name":"Motion Chart","description":"Display how a set of metrics evolves over time (requires an internet access).","version":"1.7","license":"GNU LGPL 3","organizationName":"SonarSource","organizationUrl":"http://www.sonarsource.com","homepageUrl":"http://docs.codehaus.org/display/SONAR/Motion+Chart+plugin","issueTrackerUrl":"http://jira.codehaus.org/browse/SONARPLUGINS/component/13722","implementationBuild":"e9c4a5c95c75564b3c3b5a887b63ef50fc59a156"},{"key":"python","name":"Python","description":"Enable analysis and reporting on python projects.","version":"1.5","license":"GNU LGPL 3","organizationName":"SonarSource and Waleri Enns","homepageUrl":"http://docs.codehaus.org/display/SONAR/Python+Plugin","issueTrackerUrl":"https://jira.codehaus.org/browse/SONARPY","implementationBuild":"10c8f1d2e8ded13634d3ee71c096e97d3fb3cfe9"},{"key":"roslyn","name":"Roslyn","description":"Roslyn diagnostic runner","version":"0.9-SNAPSHOT","license":"GNU LGPL 3","organizationName":"jmecsoftware.com","organizationUrl":"http://www.sonarsource.com","homepageUrl":"https://sites.google.com/site/jmecsoftware/","issueTrackerUrl":"http://jira.sonarsource.com","implementationBuild":"a72d5ce144164a9dda65ac4be277265194fa212c"},{"key":"scmsvn","name":"SVN","description":"SVN SCM Provider.","version":"1.2","license":"GNU LGPL 3","organizationName":"SonarSource","organizationUrl":"http://www.sonarsource.com","homepageUrl":"http://redirect.sonarsource.com/plugins/scmsvn.html","issueTrackerUrl":"https://jira.sonarsource.com/browse/SONARSCSVN","implementationBuild":"d04c3cdb21f48905dd8300d1129ec90281aa6db2"},{"key":"stylecop","name":"StyleCop","description":"Enables the use of StyleCop rules on C# code.","version":"1.1","license":"GNU LGPL 3","organizationName":"SonarSource","organizationUrl":"http://www.sonarsource.com","homepageUrl":"http://docs.codehaus.org/x/BoNEDg","issueTrackerUrl":"http://jira.codehaus.org/browse/SONARPLUGINS/component/16487","implementationBuild":"909438ebc609371919de34aa41262093711c58bc"},{"key":"timeline","name":"Timeline","description":"Advanced time machine chart (requires an internet access).","version":"1.5","license":"GNU LGPL 3","organizationName":"SonarSource","organizationUrl":"http://www.sonarsource.com","homepageUrl":"http://docs.codehaus.org/display/SONAR/Timeline+Plugin","issueTrackerUrl":"http://jira.codehaus.org/browse/SONARPLUGINS/component/14068","implementationBuild":"a9cae1328fd455a128b5d7d603381f47398c6e2a"},{"key":"widgetlab","name":"Widget Lab","description":"Additional widgets","version":"1.7","license":"GNU LGPL 3","organizationName":"Shaw Industries","organizationUrl":"http://shawfloors.com","homepageUrl":"http://docs.codehaus.org/display/SONAR/Widget+Lab+Plugin","issueTrackerUrl":"http://jira.codehaus.org/browse/SONARPLUGINS/component/15490","implementationBuild":"199762d2ed62a215601f3422a4973682c16618c0"},{"key":"xml","name":"XML","description":"Enable analysis and reporting on XML files.","version":"1.3","license":"The Apache Software License, Version 2.0","organizationName":"SonarSource","organizationUrl":"http://www.sonarsource.com","homepageUrl":"http://redirect.sonarsource.com/plugins/xml.html","issueTrackerUrl":"http://jira.codehaus.org/browse/SONARPLUGINS/component/14607","implementationBuild":"a8739cf424a5b42b64a3277373ab2d48aca5a6e0"},{"key":"ail","name":"ail","description":"Enable analysis and reporting on ail files.","version":"0.9-SNAPSHOT","license":"GNU LGPL 3","organizationName":"Jorge Costa","organizationUrl":"http://www.tekla.com","homepageUrl":"http://redirect.sonarsource.com/plugins/fsharp.html","issueTrackerUrl":"http://jira.codehaus.org/browse/SONARCS","implementationBuild":"1931514c7e0cefee876b21aeea139f895a5fac79"}]}""">

type JsonErrorMessage = JsonProvider<""" {"errors":[{"msg":"Linear functions must only have a non empty coefficient"}]} """>

type JsonActionPlan = JsonProvider<"""
{
  "actionPlans": [
    {
      "key": "dfasdfsad",
      "name": "Version 3.6",
      "status": "OPEN",
      "project": "java-sonar-runner-simple",
      "userLogin": "admin",
      "deadLine": "2013-12-31T00:00:00+0100",
      "totalIssues": 10,
      "unresolvedIssues": 3,
      "createdAt": "2013-05-31T22:40:50+0200",
      "updatedAt": "2013-05-31T22:40:50+0200"
    },
    {
      "key": "asdfasdfsadf",
      "name": "Version 3.5",
      "status": "CLOSED",
      "project": "java-sonar-runner-simple4",
      "userLogin": "admin",
      "totalIssues": 0,
      "unresolvedIssues": 0,
      "createdAt": "2013-05-31T22:40:30+0200",
      "updatedAt": "2013-05-31T22:42:13+0200"
    }
  ]
}
""">

type JsonarPlan = JsonProvider<"""{"actionPlan":{"key":"dfsdffgdf","name":"asdsa3432","status":"OPEN","project":"sonar.training:SonarTraining:feature_improve-coverage","desc":"fdadfasdfsad","userLogin":"jocs","deadLine":"2015-08-22T00:00:00+0300","fDeadLine":"22 Aug 2015","createdAt":"2015-08-20T12:38:54+0300","fCreatedAt":"20 Aug 2015","updatedAt":"2015-08-20T12:38:54+0300","fUpdatedAt":"20 Aug 2015"}}""">

type JsonRuleSearchResponse = JsonProvider<""" {
  "total": 641,
  "p": 1,
  "ps": 10,
  "rules": [
    {
      "key": "cppcheck:unreadVariable",
      "repo": "cppcheck",
      "name": "Unused value",
      "createdAt": "2013-08-19T23:16:28+0300",
      "severity": "MAJOR",
      "status": "READY",
      "internalKey": "unreadVariable",
      "isTemplate": false,
      "tags": [
        "pitfall",
        "unused"
      ],
      "sysTags": [
        "pitfall",
        "unused"
      ],
      "lang": "c++",
      "langName": "c++",
      "htmlDesc": "Variable is assigned a value that is never used.",
      "defaultDebtChar": "RELIABILITY",
      "defaultDebtSubChar": "INSTRUCTION_RELIABILITY",
      "debtChar": "RELIABILITY",
      "debtSubChar": "INSTRUCTION_RELIABILITY",
      "debtCharName": "Reliability",
      "debtSubCharName": "Instruction",
      "defaultDebtRemFnType": "LINEAR",
      "defaultDebtRemFnCoeff": "5min",
      "debtOverloaded": false,
      "debtRemFnType": "LINEAR",
      "debtRemFnCoeff": "5min",
      "params": []
    },
    {
      "key": "cppcheck:arrayIndexOutOfBounds",
      "repo": "cppcheck",
      "name": "Array index out of bounds",
      "createdAt": "2013-08-19T23:16:28+0300",
      "severity": "MAJOR",
      "status": "READY",
      "internalKey": "arrayIndexOutOfBounds",
      "isTemplate": false,
      "tags": [],
      "sysTags": [],
      "lang": "c++",
      "langName": "c++",
      "htmlDesc": "Array index out of bounds.",
      "defaultDebtChar": "RELIABILITY",
      "defaultDebtSubChar": "INSTRUCTION_RELIABILITY",
      "debtChar": "RELIABILITY",
      "debtSubChar": "INSTRUCTION_RELIABILITY",
      "debtCharName": "Reliability",
      "debtSubCharName": "Instruction",
      "defaultDebtRemFnType": "LINEAR",
      "defaultDebtRemFnCoeff": "30min",
      "debtOverloaded": false,
      "debtRemFnType": "LINEAR",
      "debtRemFnCoeff": "30min",
      "params": [
        {
          "key": "CheckId",
          "type": "STRING",
          "defaultValue": "TE0027"
        }
      ]
      }
    ]
} """>

type JsonInternalData = JsonProvider<""" {
  "canWrite": true,
  "qualityprofiles": [
    {
      "key": "cs-default-tekla-c-84184",
      "name": "Default Tekla C#",
      "lang": "cs"
    },
    {
      "key": "c++-defaultc++reinforcement-41625",
      "name": "DefaultC++Reinforcement",
      "lang": "c++"
    }
  ],
  "languages": {
    "py": "Python",
    "c++": "c++",
    "xaml": "xaml",
    "cs": "C#"
  },
  "repositories": [
    {
      "key": "checkstyle",
      "name": "Checkstyle",
      "language": "java"
    },
    {
      "key": "common-c++",
      "name": "Common SonarQube",
      "language": "c++"
    }
  ],
  "statuses": {
    "BETA": "Beta",
    "DEPRECATED": "Deprecated",
    "READY": "Ready"
  },
  "characteristics": {
    "INTEGRATION_TESTABILITY": "Testability: Integration level",
    "UNIT_TESTABILITY": "Testability: Unit level",
    "REUSABILITY": "Reusability",
    "COMPILER_RELATED_PORTABILITY": "Portability: Compiler",
    "PORTABILITY": "Portability",
    "TRANSPORTABILITY": "Reusability: Transportability",
    "MODULARITY": "Reusability: Modularity",
    "SECURITY": "Security",
    "API_ABUSE": "Security: API abuse",
    "ERRORS": "Security: Errors",
    "INPUT_VALIDATION_AND_REPRESENTATION": "Security: Input validation and representation",
    "SECURITY_FEATURES": "Security: Security features",
    "EFFICIENCY": "Efficiency",
    "MEMORY_EFFICIENCY": "Efficiency: Memory use",
    "NETWORK_USE": "Efficiency: Network use",
    "HARDWARE_RELATED_PORTABILITY": "Portability: Hardware",
    "LANGUAGE_RELATED_PORTABILITY": "Portability: Language",
    "OS_RELATED_PORTABILITY": "Portability: OS",
    "SOFTWARE_RELATED_PORTABILITY": "Portability: Software",
    "TIME_ZONE_RELATED_PORTABILITY": "Portability: Time zone",
    "MAINTAINABILITY": "Maintainability",
    "READABILITY": "Maintainability: Readability",
    "UNDERSTANDABILITY": "Maintainability: Understandability",
    "FAULT_TOLERANCE": "Reliability: Fault tolerance",
    "EXCEPTION_HANDLING": "Reliability: Exception handling",
    "LOGIC_RELIABILITY": "Reliability: Logic",
    "INSTRUCTION_RELIABILITY": "Reliability: Instruction",
    "SYNCHRONIZATION_RELIABILITY": "Reliability: Synchronization",
    "RESOURCE_RELIABILITY": "Reliability: Resource",
    "TESTABILITY": "Testability",
    "UNIT_TESTS": "Reliability: Unit tests coverage",
    "CHANGEABILITY": "Changeability",
    "CPU_EFFICIENCY": "Efficiency: Processor use",
    "DATA_CHANGEABILITY": "Changeability: Data",
    "ARCHITECTURE_CHANGEABILITY": "Changeability: Architecture",
    "RELIABILITY": "Reliability",
    "LOGIC_CHANGEABILITY": "Changeability: Logic",
    "DATA_RELIABILITY": "Reliability: Data",
    "ARCHITECTURE_RELIABILITY": "Reliability: Architecture"
  }
}""" >

type JsonRule = JsonProvider<""" {"rule": {
    "key":"cppcheck:unreadVariable",
    "repo":"cppcheck",
    "name":"Unused value",
    "createdAt":"2013-08-19T23:16:28+0300",
    "severity":"MAJOR",
    "status":"READY",
    "internalKey":"unreadVariable",
    "isTemplate":false,
    "tags":[
      "pitfall",
      "unused"
    ],
    "sysTags":[
      "pitfall",
      "unused"
    ],
    "lang":"c++",
    "langName":"c++",
    "htmlDesc":"Variable is assigned a value that is never used.",
    "defaultDebtChar":"RELIABILITY",
    "defaultDebtSubChar":"INSTRUCTION_RELIABILITY",
    "debtChar":"RELIABILITY",
    "debtSubChar":"INSTRUCTION_RELIABILITY",
    "debtCharName":"Reliability",
    "debtSubCharName":"Instruction",
    "defaultDebtRemFnType":"LINEAR",
    "defaultDebtRemFnCoeff":"5min",
    "debtOverloaded":false,
    "debtRemFnType":"LINEAR",
    "debtRemFnCoeff":"5min",
    "params": [
        {
            "key": "max",
            "desc": "Maximum complexity allowed.",
            "defaultValue": "200"
        }
    ] 
}, "actives": [
    {
        "qProfile": "Sonar way with Findbugs:java",
        "inherit": "NONE",
        "severity": "MAJOR",
        "params": [
            {
                "key": "max",
                "value": "200"
            }
        ]
    },
    {
        "qProfile": "Sonar way:java",
        "inherit": "NONE",
        "severity": "MAJOR",
        "params": [
            {
                "key": "max",
                "value": "200"
            }
        ]
    }
]} """>

type JsonProjectIndex = JsonProvider<""" [
  {
    "id": "5035",
    "k": "org.jenkins-ci.plugins:sonar",
    "nm": "Jenkins Sonar Plugin",
    "sc": "PRJ",
    "qu": "TRK"
  },
  {
    "id": "5146",
    "k": "org.codehaus.sonar-plugins:sonar-ant-task",
    "nm": "Sonar Ant Task",
    "sc": "PRJ",
    "qu": "TRK"
  },
  {
    "id": "15964",
    "k": "org.codehaus.sonar-plugins:sonar-build-breaker-plugin",
    "nm": "Sonar Build Breaker Plugin",
    "sc": "PRJ",
    "qu": "TRK"
  }
] """>

type JsonQualityProfiles = JsonProvider<""" [
  {
    "name": "Sonar way with Findbugs",
    "language": "java",
    "default": false
  },
  {
    "name": "Sonar way",
    "language": "java",
    "default": false
  }
] """>

type JsonProfileAfter44 = JsonProvider<""" [
  {
    "name": "Sonar way",
    "language": "java",
    "default": true,
    "rules": [
      {
        "key": "DuplicatedBlocks",
        "repo": "common-java",
        "severity": "MAJOR"
      },
      {
        "key": "InsufficientBranchCoverage",
        "repo": "common-java",
        "severity": "MAJOR",
        "params": [
          {
            "key": "minimumBranchCoverageRatio",
            "value": "65.0"
          }
        ]
      },
      {
        "key": "S00105",
        "repo": "squid",
        "severity": "MINOR"
      },
      {
        "key": "MethodCyclomaticComplexity",
        "repo": "squid",
        "severity": "MAJOR",
        "params": [
          {
            "key": "sdfsdfsd",
            "value": "sdfsdfsd"
          }
        ]
      }
    ]
  }
] """>

type JsonResourceWithMetrics = JsonProvider<""" [{"id":1,"key":"GroupId:ProjectId","name":"Common","scope":"PRJ","branch":"whatever","qualifier":"TRK","date":"2013-07-03T12:50:52+0300","lname":"Common","lang":"c++","version":"work","description":"","msr":[{"key":"ncloc","val":45499.0,"frmt_val":"45,499"},{"key":"coverage","val":54.7,"frmt_val":"54.7%"},{"key":"profile","val":7.0,"frmt_val":"7.0","data":"DefaultTeklaC++"}]}] """>

type JsonValidateUser = JsonProvider<""" {"valid":true} """>

type JsonUsers = JsonProvider<""" {"users":[{"login":"user1","name":"","active":true},{"login":"admin","name":"Administrator","active":true},{"login":"user2","name":"Real Name","active":true,"email":"real.name@org.com"}]} """>

type JsonIssues = JsonProvider<""" {"maxResultsReached":false,"paging":{"pageIndex":1,"pageSize":100,"total":5,"pages":1},"issues":[{"key":"whatever","component":"organization:projectid:directory/filename.cpp","project":"organization:projectid","author":"email@sdsasa.com","rule":"common-c++:InsufficientLineCoverage","status":"OPEN","resolution":"FIXED","severity":"MAJOR","actionPlan":"sdfsdfsd","message":"1 more lines of code need to be covered by unit tests to reach the minimum threshold of 40.0% lines coverage.","effortToFix":"1.0","creationDate":"2013-03-07T21:56:08+0200","updateDate":"2013-06-30T21:55:48+0300","closeDate":"2013-06-30T21:55:48+0300"},{"key":"df3c7c99-d6b5-4cec-8248-017201d092b6","component":"GroupId:Drawings:libgr_cloning/gr_cursor.cpp","project":"GroupId:Drawings","rule":"manual:this_is a manual review","status":"CONFIRMED","severity":"MAJOR","message":"please change this","line":4,"reporter":"login1","assignee":"username","creationDate":"2013-04-08T13:38:53+0300","updateDate":"2013-04-08T13:39:27+0300"},{"key":"22ecf99a-a2a1-419f-8783-48ba8238091e","component":"GroupId:Drawings:libgr_db/grdb_check.cpp","project":"GroupId:Drawings","rule":"manual:try_to detect when size is used without size_t","status":"CONFIRMED","severity":"MAJOR","message":"Try to detect when size is used without size_t","line":35,"reporter":"username","assignee":"username","creationDate":"2012-10-08T10:20:04+0300","updateDate":"2012-10-08T10:20:04+0300"},{"key":"a8f4984b-7be7-4d36-9c15-72e25431665b","component":"GroupId:TeklaStructures:libdialog/dia_option_button.cpp","project":"GroupId:TeklaStructures","rule":"cppcheck:unusedFunction","status":"CONFIRMED","severity":"MINOR","message":"The function 'SetBitmapName' is never used.","line":59,"assignee":"username","actionPlan":"fsdfsdfsd","creationDate":"2012-11-15T06:59:33+0200","updateDate":"2012-11-20T11:52:57+0200","comments":[{"key":"dfsfdsdfs","login":"username","htmlText":"can you check if these are false positives","createdAt":"2012-11-19T11:51:54+0200"},{"key":"sfgfdgfdd","login":"vepi","htmlText":"Well technically they are not called, because BitmapName and InternalValue are only set by the constructor by the current code. However since diaOptionButton_ci is basically a data container with accessors, I think it's reasonable that there are symmetrical accessors to all the variables, even if they are not currently used.","createdAt":"2012-11-20T09:21:24+0200"},{"key":"gfdgfdfgdf","login":"username","htmlText":"same here","createdAt":"2012-11-20T11:52:57+0200"}]},{"key":"gdfgdfgfd","component":"GroupId:TeklaStructures:libdialog/dia_option_button.cpp","project":"GroupId:TeklaStructures","rule":"cppcheck:unusedFunction","status":"CONFIRMED","severity":"MINOR","message":"The function 'SetInternalValue' is never used.","line":46,"assignee":"username","actionPlan":"fsdfsdfsd","creationDate":"2012-11-15T06:59:33+0200","updateDate":"2012-11-20T11:52:32+0200","comments":[{"key":"sfsdfsdfsd","login":"username","htmlText":"can you check if these are false positives","createdAt":"2012-11-19T11:51:37+0200"},{"key":"gfdgfgdf","login":"vepi","htmlText":"Well technically they are not called, because BitmapName and InternalValue are only set by the constructor by the current code. However since diaOptionButton_ci is basically a data container with accessors, I think it's reasonable that there are symmetrical accessors to all the variables, even if they are not currently used.","createdAt":"2012-11-20T09:21:50+0200"},{"key":"sgdfgdfgdfgd","login":"username","htmlText":"So is there a chance of unit test this? If not you may just mark it as false positive.","createdAt":"2012-11-20T11:52:32+0200"}]},{"key":"46429559-4e6c-455a-96b6-b5c75ecdd4d7","component":"GroupId:Dimensioning:libgr_dim_lib/grdl_dimensioning_adapter.cpp","project":"GroupId:Dimensioning","rule":"cppcheck:incorrectStringBooleanError","status":"CONFIRMED","severity":"MINOR","message":"A boolean comparison with the string literal \"dmlGetSteelDimensioningInstance\" is always true.","line":49,"assignee":"username","creationDate":"2013-02-21T13:06:03+0200","updateDate":"2013-02-22T09:45:06+0200","comments":[{"key":"dfsdfdsfsdd","login":"username","htmlText":"how are we fixing those? do i need to update the tool not to generate this cases?","createdAt":"2013-02-22T09:36:03+0200"},{"key":"fgdfgdf","login":"pafi","htmlText":"Please update the tool if you can restrict this to ASSERT() expressions. Otherwise we might want to change this - this could be changed by having if(test) { ASSERT(!&quot;&lt;message&gt;&quot;) } but this makes it longer (and also messes with the test coverage - condition is never met)","createdAt":"2013-02-22T09:45:06+0200"}]}],"components":[{"key":"GroupId:TeklaStructures:libdialog/dia_option_button.cpp","qualifier":"FIL","name":"dia_option_button.cpp","longName":"libdialog/dia_option_button.cpp"},{"key":"GroupId:Dimensioning:libgr_dim_lib/grdl_dimensioning_adapter.cpp","qualifier":"FIL","name":"grdl_dimensioning_adapter.cpp","longName":"libgr_dim_lib/grdl_dimensioning_adapter.cpp"},{"key":"GroupId:Drawings:libgr_cloning/gr_cursor.cpp","qualifier":"FIL","name":"gr_cursor.cpp","longName":"libgr_cloning/gr_cursor.cpp"},{"key":"GroupId:Drawings:libgr_db/grdb_check.cpp","qualifier":"FIL","name":"grdb_check.cpp","longName":"libgr_db/grdb_check.cpp"}],"projects":[{"key":"GroupId:Drawings","qualifier":"TRK","name":"Drawings","longName":"Drawings"},{"key":"GroupId:Dimensioning","qualifier":"TRK","name":"Dimensioning","longName":"Dimensioning"},{"key":"GroupId:TeklaStructures","qualifier":"TRK","name":"TeklaStructures","longName":"TeklaStructures"}],"rules":[{"key":"manual:this_is a manual review","name":"This is a manual review","desc":"Rule created on the fly. A description should be provided.","status":"READY"},{"key":"cppcheck:incorrectStringBooleanError","name":"Suspicious comparison of boolean with a string literal","desc":"A boolean comparison with the string literal is always true.","status":"READY"},{"key":"cppcheck:unusedFunction","name":"Unused function","desc":"The function is never used.","status":"READY"},{"key":"manual:try_to detect when size is used without size_t","name":"Try to detect when size is used without size_t","desc":"Rule created on the fly. A description should be provided.","status":"READY"}],"users":[{"login":"pafi","name":"Filoche Pascal","active":true,"email":"pascal.filoche@tekla.com"},{"login":"username","name":"Costa Jorge","active":true,"email":"jorge.costa@tekla.com"},{"login":"login1","name":"","active":true},{"login":"vepi","name":"Piril\u00e4 Vesa","active":true,"email":"vesa.pirila@tekla.com"}],"actionPlans":[{"key":"fsfsdfsd","name":"Gate 3 Version  19.1","status":"OPEN","project":"GroupId:TeklaStructures","desc":"Gate 3 Version  19.1","userLogin":"username","createdAt":"2012-10-25T09:23:34+0300","updatedAt":"2013-06-29T13:23:45+0300"}]} """>

type JsonIssue = JsonProvider<""" {"issue":{"key":"whatevervalue","author":"email@sdsasa.com","actionPlan":"fsdfsdfsd","component":"organization:projectid:directory/file.cpp","project":"organization:projectid","line":4,"assignee":"userlogin","rule":"common-c++:InsufficientBranchCoverage","status":"REOPENED", "resolution":"FIXED", "severity":"MINOR","message":"203 more branches need to be covered by unit tests to reach the minimum threshold of 30.0% branch coverage.","effortToFix":"203.0","creationDate":"2013-03-10T12:04:31+0200","closeDate":"2013-06-30T21:55:48+0300","updateDate":"2013-07-18T13:51:07+0300","comments":[{"key":"mdkmkvfsfsdk","login":"login","htmlText":"this is a comment","createdAt":"2013-07-17T22:21:00+0300"},{"key":"fsdfsdf","login":"login","htmlText":"this is another comment","createdAt":"2013-07-17T22:28:40+0300"},{"key":"fsdfsfsd","login":"login1","htmlText":"yet another comment","createdAt":"2013-07-17T22:46:02+0300"},{"key":"fgdfg;sdf","login":"login1","htmlText":"yet another comment","createdAt":"2013-07-17T23:42:44+0300"},{"key":"sdfsdfsd","login":"login1","htmlText":"fdlkflds","createdAt":"2013-07-17T23:43:13+0300"},{"key":"dsfsdgdfg","login":"login1","htmlText":"akskjds","createdAt":"2013-07-17T23:45:20+0300"},{"key":"s'dflksdl","login":"login1","htmlText":"fsdfsd","createdAt":"2013-07-17T23:46:44+0300"},{"key":";dlkf;sldkf;lskd","login":"login1","htmlText":"fsdfsd","createdAt":"2013-07-17T23:55:07+0300"},{"key":"kkslkjdfnv","login":"login1","htmlText":"fgsdgdfghgfgh","createdAt":"2013-07-17T23:55:19+0300"},{"key":"xkjdkiwlkkd","login":"login1","htmlText":"another comment","createdAt":"2013-07-18T10:57:42+0300"},{"key":"dsfdsdfsd","login":"login1","htmlText":"fgfdgdfgdf fgfd","createdAt":"2013-07-18T12:24:31+0300"},{"key":"Csdfsdfsd","login":"login1","htmlText":"sdfdsfsd","createdAt":"2013-07-18T12:25:22+0300"},{"key":"sd,mfs,dfs","login":"login1","htmlText":"sdsadsa asdsa d","createdAt":"2013-07-18T12:26:38+0300"}]}} """>

type JSonarReview = JsonProvider<""" [{"id":2769,"createdAt":"2013-06-17T13:51:28+0300","updatedAt":"2013-06-21T05:09:16+0300","author":"user21","title":"IllegalIncludeDirectories Include File is illegal in this Project e3_common_types.h","status":"CLOSED","severity":"MINOR","resource":"GroupId:project:directory/filename.cpp","line":7,"violationId":91986628,"comments":[]},{"id":2625,"createdAt":"2013-05-08T09:24:22+0300","updatedAt":"2013-05-20T05:18:51+0300","author":"user2","assignee":"tmr","title":"Empty loop bodies should use {} or continue","status":"CLOSED","severity":"MINOR","resource":"GroupId:project:directory/filename.cpp","line":3295,"violationId":84276926,"comments":[]},{"id":64,"createdAt":"2012-07-30T10:25:25+0300","updatedAt":"2013-03-06T12:37:33+0200","author":"user2","title":"The scope of the variable 'err' can be reduced","status":"CLOSED","resolution":"FALSE-POSITIVE","severity":"MINOR","resource":"GroupId:project:directory/filename.cpp","line":4367,"violationId":12721494,"comments":[{"id":63,"updatedAt":"2012-07-30T10:25:25+0300","text":"Value used in macro, not applicable.","author":"user2"}]},{"id":712,"createdAt":"2012-10-02T09:27:17+0300","updatedAt":"2013-01-11T02:17:08+0200","author":"mis","assignee":"mis","title":"Testing this","status":"CLOSED","resolution":"FIXED","severity":"INFO","resource":"GroupId:project:directory/filename.cpp","line":173,"violationId":32393083,"comments":[{"id":489,"updatedAt":"2012-10-02T09:27:46+0300","text":"xgsdgsdfsdf","author":"mis"}]},{"id":713,"createdAt":"2012-10-02T09:28:37+0300","updatedAt":"2013-01-10T02:26:04+0200","author":"mis","assignee":"mis","title":"test","status":"CLOSED","resolution":"FIXED","severity":"MAJOR","resource":"GroupId:project:directory/filename.cpp","line":7,"violationId":32393084,"comments":[{"id":494,"updatedAt":"2012-10-04T08:21:37+0300","text":"Yep. That's toxic.","author":"user1"}]}] """>

type JSonProfile = JsonProvider<""" [{"name":"profile","language":"lang","default":true,"rules":[{"key":"key1","repo":"repo1","severity":"BLOCKER"},{"key":"key2","repo":"repo","severity":"CRITICAL"}],"alerts":[{"metric":"metric1","operator":">","error":"50","warning":"70"},{"metric":"metric1","operator":">","error":"50","warning":"70"}]}] """>

type JSonServerInfo = JsonProvider<""" {"id":"20130712144608","version":"3.6.1-SNAPSHOT","status":"UP"} """>

type JSonSource = JsonProvider<""" [{"line1":"/**","line2":"    @file       bla.cpp","line3":"    Source file of the bla class.","line4":"    @author     bla","line5":"    @date       25.11.2002","line6":"*/"}] """>

type JSonViolation = JsonProvider<""" [{"id":140081595,"message":"2 more lines of code need to be covered by unit tests to reach the minimum threshold of 40.0% lines coverage.","priority":"MAJOR","createdAt":"2013-03-07T21:56:08+0200","rule":{"key":"common-c++:InsufficientLineCoverage","name":"Insufficient line coverage by unit tests"},"resource":{"key":"GroupId:projectId:directory/filename.cpp","name":"filename.cpp","scope":"FIL","qualifier":"FIL","language":"c++"}},{"id":140081594,"message":"9 more branches need to be covered by unit tests to reach the minimum threshold of 30.0% branch coverage.","priority":"MAJOR","createdAt":"2013-03-07T21:56:08+0200","rule":{"key":"common-c++:InsufficientBranchCoverage","name":"Insufficient branch coverage by unit tests"},"resource":{"key":"GroupId:projectId:directory/filename.cpp","name":"filename.cpp","scope":"FIL","qualifier":"FIL","language":"c++"}},{"id":140081586,"message":"Extra space before ( in function call","line":350,"priority":"MINOR","createdAt":"2013-06-25T05:08:29+0300","rule":{"key":"cxxexternal:cpplint.whitespace/parens-2","name":"Extra space before ( in function call"},"resource":{"key":"GroupId:projectId:directory/filename.cpp","name":"filename.cpp","scope":"FIL","qualifier":"FIL","language":"c++"}},{"id":140081585,"message":"Extra space before ( in function call","line":348,"priority":"MINOR","createdAt":"2013-06-25T05:08:29+0300","rule":{"key":"cxxexternal:cpplint.whitespace/parens-2","name":"Extra space before ( in function call"},"resource":{"key":"GroupId:projectId:directory/filename.cpp","name":"filename.cpp","scope":"FIL","qualifier":"FIL","language":"c++"}},{"id":140081576,"message":"The scope of the variable 'PARALLEL_TOLERANCE' can be reduced.","line":595,"priority":"MINOR","createdAt":"2013-03-06T16:40:07+0200","rule":{"key":"cppcheck:variableScope","name":"The scope of the variable can be reduced"},"resource":{"key":"GroupId:projectId:directory/filename.cpp","name":"filename.cpp","scope":"FIL","qualifier":"FIL","language":"c++"}},{"id":140081566,"message":"The scope of the variable 'error' can be reduced.","line":78,"priority":"MINOR","createdAt":"2013-03-06T16:40:07+0200","rule":{"key":"cppcheck:variableScope","name":"The scope of the variable can be reduced"},"resource":{"key":"GroupId:projectId:directory/filename.cpp","name":"filename.cpp","scope":"FIL","qualifier":"FIL","language":"c++"}},{"id":140081571,"message":"The scope of the variable 'Length' can be reduced.","line":335,"priority":"MINOR","createdAt":"2013-03-06T16:40:07+0200","rule":{"key":"cppcheck:variableScope","name":"The scope of the variable can be reduced"},"resource":{"key":"GroupId:projectId:directory/filename.cpp","name":"filename.cpp","scope":"FIL","qualifier":"FIL","language":"c++"}},{"id":140081575,"message":"The scope of the variable 'Angle' can be reduced.","line":561,"priority":"MINOR","createdAt":"2013-03-06T16:40:07+0200","rule":{"key":"cppcheck:variableScope","name":"The scope of the variable can be reduced"},"resource":{"key":"GroupId:projectId:directory/filename.cpp","name":"filename.cpp","scope":"FIL","qualifier":"FIL","language":"c++"}},{"id":140081573,"message":"The scope of the variable 'Bulge' can be reduced.","line":514,"priority":"MINOR","createdAt":"2013-03-06T16:40:07+0200","rule":{"key":"cppcheck:variableScope","name":"The scope of the variable can be reduced"},"resource":{"key":"GroupId:projectId:directory/filename.cpp","name":"filename.cpp","scope":"FIL","qualifier":"FIL","language":"c++"}},{"id":140081581,"message":"Extra space before ( in function call","line":330,"priority":"MINOR","createdAt":"2013-06-25T05:08:29+0300","rule":{"key":"cxxexternal:cpplint.whitespace/parens-2","name":"Extra space before ( in function call"},"resource":{"key":"GroupId:projectId:directory/filename.cpp","name":"filename.cpp","scope":"FIL","qualifier":"FIL","language":"c++"}},{"id":140081578,"message":"Extra space before ( in function call","line":242,"priority":"MINOR","createdAt":"2013-06-25T05:08:29+0300","rule":{"key":"cxxexternal:cpplint.whitespace/parens-2","name":"Extra space before ( in function call"},"resource":{"key":"GroupId:projectId:directory/filename.cpp","name":"filename.cpp","scope":"FIL","qualifier":"FIL","language":"c++"}},{"id":140081580,"message":"Extra space before ( in function call","line":282,"priority":"MINOR","createdAt":"2013-06-25T05:08:29+0300","rule":{"key":"cxxexternal:cpplint.whitespace/parens-2","name":"Extra space before ( in function call"},"resource":{"key":"GroupId:projectId:directory/filename.cpp","name":"filename.cpp","scope":"FIL","qualifier":"FIL","language":"c++"}},{"id":140081577,"message":"transform.hpp already included at 9","line":10,"priority":"MINOR","createdAt":"2013-06-25T05:08:29+0300","rule":{"key":"cxxexternal:cpplint.build/include-0","name":"\"%s\" already included at %s:%s  % (include, filename, include_state[include])"},"resource":{"key":"GroupId:projectId:directory/filename.cpp","name":"filename.cpp","scope":"FIL","qualifier":"FIL","language":"c++"}},{"id":140081579,"message":"Extra space before last semicolon. If this should be an empty statement, use {} instead.","line":250,"priority":"MINOR","createdAt":"2013-06-25T05:08:29+0300","rule":{"key":"cxxexternal:cpplint.whitespace/semicolon-3","name":"Extra space before last semicolon. If this should be an empty   statement, use {} instead."},"resource":{"key":"GroupId:projectId:directory/filename.cpp","name":"filename.cpp","scope":"FIL","qualifier":"FIL","language":"c++"}},{"id":140081582,"message":"Extra space before ( in function call","line":342,"priority":"MINOR","createdAt":"2013-06-25T05:08:29+0300","rule":{"key":"cxxexternal:cpplint.whitespace/parens-2","name":"Extra space before ( in function call"},"resource":{"key":"GroupId:projectId:directory/filename.cpp","name":"filename.cpp","scope":"FIL","qualifier":"FIL","language":"c++"}},{"id":140081584,"message":"Extra space before ( in function call","line":345,"priority":"MINOR","createdAt":"2013-06-25T05:08:29+0300","rule":{"key":"cxxexternal:cpplint.whitespace/parens-2","name":"Extra space before ( in function call"},"resource":{"key":"GroupId:projectId:directory/filename.cpp","name":"filename.cpp","scope":"FIL","qualifier":"FIL","language":"c++"}},{"id":140081572,"message":"The scope of the variable 'Eps' can be reduced.","line":430,"priority":"MINOR","createdAt":"2013-03-06T16:40:07+0200","rule":{"key":"cppcheck:variableScope","name":"The scope of the variable can be reduced"},"resource":{"key":"GroupId:projectId:directory/filename.cpp","name":"filename.cpp","scope":"FIL","qualifier":"FIL","language":"c++"}},{"id":140081583,"message":"Extra space before ( in function call","line":343,"priority":"MINOR","createdAt":"2013-06-25T05:08:29+0300","rule":{"key":"cxxexternal:cpplint.whitespace/parens-2","name":"Extra space before ( in function call"},"resource":{"key":"GroupId:projectId:directory/filename.cpp","name":"filename.cpp","scope":"FIL","qualifier":"FIL","language":"c++"}},{"id":140081567,"message":"The scope of the variable 'distance' can be reduced.","line":79,"priority":"MINOR","createdAt":"2013-03-06T16:40:07+0200","rule":{"key":"cppcheck:variableScope","name":"The scope of the variable can be reduced"},"resource":{"key":"GroupId:projectId:directory/filename.cpp","name":"filename.cpp","scope":"FIL","qualifier":"FIL","language":"c++"}},{"id":140081589,"message":"Extra space before ( in function call","line":360,"priority":"MINOR","createdAt":"2013-06-25T05:08:29+0300","rule":{"key":"cxxexternal:cpplint.whitespace/parens-2","name":"Extra space before ( in function call"},"resource":{"key":"GroupId:projectId:directory/filename.cpp","name":"filename.cpp","scope":"FIL","qualifier":"FIL","language":"c++"}},{"id":140081570,"message":"The scope of the variable 'Fz' can be reduced.","line":334,"priority":"MINOR","createdAt":"2013-03-06T16:40:07+0200","rule":{"key":"cppcheck:variableScope","name":"The scope of the variable can be reduced"},"resource":{"key":"GroupId:projectId:directory/filename.cpp","name":"filename.cpp","scope":"FIL","qualifier":"FIL","language":"c++"}},{"id":140081588,"message":"Extra space before ( in function call","line":354,"priority":"MINOR","createdAt":"2013-06-25T05:08:29+0300","rule":{"key":"cxxexternal:cpplint.whitespace/parens-2","name":"Extra space before ( in function call"},"resource":{"key":"GroupId:projectId:directory/filename.cpp","name":"filename.cpp","scope":"FIL","qualifier":"FIL","language":"c++"}},{"id":140081590,"message":"Extra space before ( in function call","line":375,"priority":"MINOR","createdAt":"2013-06-25T05:08:29+0300","rule":{"key":"cxxexternal:cpplint.whitespace/parens-2","name":"Extra space before ( in function call"},"resource":{"key":"GroupId:projectId:directory/filename.cpp","name":"filename.cpp","scope":"FIL","qualifier":"FIL","language":"c++"}},{"id":140081592,"message":"Extra space before ( in function call","line":377,"priority":"MINOR","createdAt":"2013-06-25T05:08:29+0300","rule":{"key":"cxxexternal:cpplint.whitespace/parens-2","name":"Extra space before ( in function call"},"resource":{"key":"GroupId:projectId:directory/filename.cpp","name":"filename.cpp","scope":"FIL","qualifier":"FIL","language":"c++"}},{"id":140081569,"message":"The scope of the variable 'Fy' can be reduced.","line":334,"priority":"MINOR","createdAt":"2013-03-06T16:40:07+0200","rule":{"key":"cppcheck:variableScope","name":"The scope of the variable can be reduced"},"resource":{"key":"GroupId:projectId:directory/filename.cpp","name":"filename.cpp","scope":"FIL","qualifier":"FIL","language":"c++"}},{"id":140081568,"message":"The scope of the variable 'Fx' can be reduced.","line":334,"priority":"MINOR","createdAt":"2013-03-06T16:40:07+0200","rule":{"key":"cppcheck:variableScope","name":"The scope of the variable can be reduced"},"resource":{"key":"GroupId:projectId:directory/filename.cpp","name":"filename.cpp","scope":"FIL","qualifier":"FIL","language":"c++"}},{"id":140081587,"message":"Extra space before ( in function call","line":352,"priority":"MINOR","createdAt":"2013-06-25T05:08:29+0300","rule":{"key":"cxxexternal:cpplint.whitespace/parens-2","name":"Extra space before ( in function call"},"resource":{"key":"GroupId:projectId:directory/filename.cpp","name":"filename.cpp","scope":"FIL","qualifier":"FIL","language":"c++"}},{"id":140081574,"message":"The scope of the variable 'TotalAngle' can be reduced.","line":561,"priority":"MINOR","createdAt":"2013-03-06T16:40:07+0200","rule":{"key":"cppcheck:variableScope","name":"The scope of the variable can be reduced"},"resource":{"key":"GroupId:projectId:directory/filename.cpp","name":"filename.cpp","scope":"FIL","qualifier":"FIL","language":"c++"}},{"id":140081591,"message":"Extra space before ( in function call","line":376,"priority":"MINOR","createdAt":"2013-06-25T05:08:29+0300","rule":{"key":"cxxexternal:cpplint.whitespace/parens-2","name":"Extra space before ( in function call"},"resource":{"key":"GroupId:projectId:directory/filename.cpp","name":"filename.cpp","scope":"FIL","qualifier":"FIL","language":"c++"}},{"id":140081593,"message":"Namespace should be terminated with // namespace geometry","line":917,"priority":"INFO","createdAt":"2013-06-25T05:08:29+0300","rule":{"key":"cxxexternal:cpplint.readability/namespace-0","name":"Namespace should be terminated with \"// namespace %s\"  %self.name"},"resource":{"key":"GroupId:projectId:directory/filename.cpp","name":"filename.cpp","scope":"FIL","qualifier":"FIL","language":"c++"}}] """>

type JSonComment = JsonProvider<""" {"comment":{"key":"xfasdegfdfDffd","login":"login1","htmlText":"fsdfsd","createdAt":"2013-07-17T23:46:44+0300"}} """>

type JSonProperties = JsonProvider<""" [{"key":"sonar.permission.template.default","value":"default_template_for_projects"},{"key":"sonar.cpd.cross_project","value":"true"},{"key":"sonar.dbcleaner.weeksBeforeDeletingAllSnapshots","value":"6000"},{"key":"sonar.fxcop.installDirectory","value":"C:\\\\Program Files (x86)\\\\Microsoft Fxcop 10.0"},{"key":"sonar.email.enabled","value":"true"},{"key":"sonar.dp.scm.command","value":"git log --numstat --date=iso"},{"key":"devcockpit.analysisDelayingInMinutes","value":"0"},{"key":"sonar.timeline.defaultmetrics","value":"lines,violations,coverage"},{"key":"sonar.branding.link","value":"http://www.tekla.com"},{"key":"sonar.branding.image","value":"http://www.tekla.com/_layouts/Tekla/images/logo.gif"},{"key":"sonar.branding.logo.location","value":"TOP"},{"key":"sonar.doxygenProperties.path","value":"e:\\\\sonar\\\\scripts\\\\doxygen.properties"},{"key":"sonar.forceAuthentication","value":"false"},{"key":"report.custom.plugins","value":"TQ, TDEBT, QI, SIGMM, TAGLIST"},{"key":"sonar.pdf.password","value":"username1"},{"key":"report.timeline.metrics","value":"loc, coverage, coverage_line_hits_data"},{"key":"report.delta.days","value":"300"},{"key":"sonar.pdf.username","value":"username1"},{"key":"send.email.to","value":"jmecosta@gmail.com"},{"key":"sonar.devcockpit.license.developers","value":"325435d2d4dfada9b7e78c5de7380b435a8e"},{"key":"sonar.cxx.suffixes","value":"c,cxx,cpp,cc,h,hxx,hpp,hh"},{"key":"sonar.core.projectsdashboard.showTreemap","value":"true"},{"key":"sonar.doxygen.path","value":"C:\\\\doxygen\\\\"},{"key":"sonar.pdf.skip","value":"true"},{"key":"vssonarextension.keys.138342495524701.vssonarextension.license.id","value":"<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<license id=\"b3f8d666-69e0-446f-8f19-26f7d910328c\" expiration=\"2014-12-12T00:00:00.0000000\" type=\"Standard\" ServerId=\"dsd6661ca19e7d\">\n  <name>BlaBla</name>\n  <Signature xmlns=\"http://www.w3.org/2000/09/xmldsig#\">\n    <SignedInfo>\n      <CanonicalizationMethod Algorithm=\"http://www.w3.org/TR/2001/REC-xml-c14n-20010315\" />\n      <SignatureMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#rsa-sha1\" />\n      <Reference URI=\"\">\n        <Transforms>\n          <Transform Algorithm=\"http://www.w3.org/2000/09/xmldsig#enveloped-signature\" />\n        </Transforms>\n        <DigestMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#sha1\" />\n        <DigestValue>io7viqk8/YhrSJdKiGjzP9q6sIg=</DigestValue>\n      </Reference>\n    </SignedInfo>\n    <SignatureValue>YbgAz8I69MVRLup3TZ6jSY4x5kranazEoAzsRzhdPorqFf8SrVDCzTfeR+thpv9EnP2OrqDmn+cTCkN1stuJEmnCvzEMo8VgomSyS66B5snkwQFO2DOjamUZl9mo8TxY15pwoULD7ejCpmgx3X1gIi+02BxKvfvL4wsbciaDUwA=</SignatureValue>\n  </Signature>\n</license>"},{"key":"vssonarextension.keys.138342495524701.vssonarextension.user.id","value":"dsadjks"},{"key":"vssonarextension.keys.138342567310101.vssonarextension.license.id","value":"dsjhjfdh"},{"key":"email.from","value":"sonar@tekla.com"},{"key":"email.prefix","value":"[SONAR]"},{"key":"sonar.doxygen.deploymentUrl","value":"http://sonar"},{"key":"sonar.doxygen.deploymentPath","value":"C:\\\\sonar-3.2\\\\war\\\\sonar-server"},{"key":"org.sonar.plugins.piwik.website-id","value":"3"},{"key":"sonar.scm.enabled","value":"false"},{"key":"sonar.core.serverBaseURL","value":"http://sonar:80"},{"key":"sonar.timemachine.period3","value":"previous_version"},{"key":"sonar.timemachine.period2","value":"5"},{"key":"com.tocea.scertifyRefactoringAssessment.projectNameEncryption","value":"true"},{"key":"sonar.profile.c","value":"Sonar way"},{"key":"sonar.profile.java","value":"Sonar way"},{"key":"sonar.profile.c++","value":"DefaultTeklaC++"},{"key":"sonar.core.projectsdashboard.defaultSortedColumn","value":"violations"},{"key":"sonar.dbcleaner.cleanDirectory","value":"false"},{"key":"sonar.profile.cs","value":"Default Tekla C#"},{"key":"sonar.profile.xaml","value":"TeklaCopXaml"},{"key":"sonar.organisation","value":"BlaBla"},{"key":"org.sonar.plugins.piwik.server","value":"esx-sonar:8000"},{"key":"sonar.server_id.ip_address","value":"10.42.65.244"},{"key":"sonar.server_id","value":"19fd6661ca19e7d"},{"key":"com.tocea.scertifyRefactoringAssessment.contactEmail","value":"jmecosta@gmail.com"},{"key":"sonar.profile.vbnet","value":"Sonar way"},{"key":"sonar.defaultGroup","value":"NewRegisteredUsers"},{"key":"sonar.switchoffviolations.multicriteria.135996266268601.resourceKey","value":"**/test/**"},{"key":"sonar.switchoffviolations.multicriteria.135996266268601.ruleKey","value":"cxxcustom:cpplint.readability/casting-0"},{"key":"sonar.global.exclusions","value":"AssemblyInfo.cs,Properties/AssemblyInfo.cs,**/*.ipch,**/_ReSharper.*/**,**/**/*.rc,**/**/resource.h,libcommondbase/v_*.h,libxml/**/**,**/**/*.bsc,file:**/libgr_plotdip/d_plotdev.c,file:**/libgr_plotdip/v_**plotdev.h,file:**/xs_cancel/dakbind.c,file:**/xs_cancel/dakbind.h,file:**/dllcom_analysis/analysisoptimisation_i.c,file:**/ail/dakbind.c,ail/dakbind.h,libenvdb_tables/Interface/v_*.h,AssemblyInfo.cpp","values":["AssemblyInfo.cs","Properties/AssemblyInfo.cs","**/*.ipch","**/_ReSharper.*/**","**/**/*.rc","**/**/resource.h","libcommondbase/v_*.h","libxml/**/**","**/**/*.bsc","file:**/libgr_plotdip/d_plotdev.c","file:**/libgr_plotdip/v_**plotdev.h","file:**/xs_cancel/dakbind.c","file:**/xs_cancel/dakbind.h","file:**/dllcom_analysis/analysisoptimisation_i.c","file:**/ail/dakbind.c","ail/dakbind.h","libenvdb_tables/Interface/v_*.h","AssemblyInfo.cpp"]},{"key":"sonar.global.test.exclusions","value":"Properties/AssemblyInfo.cs,AssemblyInfo.cpp","values":["Properties/AssemblyInfo.cs","AssemblyInfo.cpp"]},{"key":"sonar.switchoffviolations.multicriteria.135996266268601.lineRange","value":"*"},{"key":"sonar.core.projectsdashboard.columns","value":"METRIC.violations_density;METRIC.violations;METRIC.it_coverage;METRIC.it_line_coverage;METRIC.it_branch_coverage;METRIC.complexity;METRIC.line_coverage;METRIC.branch_coverage;"},{"key":"devcockpit.status","value":"D,20130819T16:59:27+0000"},{"key":"tendency.depth","value":"15"},{"key":"sonar.switchoffviolations.multicriteria","value":"135996266268601","values":["135996266268601"]},{"key":"sonar.allowUsersToSignUp","value":"true"},{"key":"sonar.permission.template.TRK.default","value":"default_template_for_projects"},{"key":"sonar.permission.template.DEV.default","value":"default_template_for_developers"},{"key":"sonar.core.id","value":"20131102223837"},{"key":"sonar.core.version","value":"3.7"},{"key":"sonar.core.startTime","value":"2013-11-02T22:38:37+0200"},{"key":"vssonarextension.keys","value":"138342495524701,138342567310101","values":["138342495524701","138342567310101"]}] """>

type JSonRule = JsonProvider<""" [{"title":"title of rule","key":"rulekey","plugin":"cxxexternal","config_key":"configKey","description":"this is the description","priority":"MAJOR"},{"title":"this is the title","key":"rulekey","plugin":"cxxexternal","config_key":"ruleconfigkey","description":"rule description","priority":"MAJOR"}] """>

type JSonIssues = JsonProvider<""" {"version":"4.0","issues":[{"key":"b2474b94-a6ae-4944-8d8d-104a3beff6d0","component":"tekla.CLI:IFCObjectConverter:Converter.cs","line":327,"message":"Refactor this method that has a complexity of 23 (which is greater than 10 authorized).","severity":"MAJOR","rule":"csharpsquid:FunctionComplexity","status":"OPEN","isNew":false,"author":"email@sdsasa.com","creationDate":"2013-08-13T15:50:48+0300","updateDate":"2013-11-28T19:18:08+0200"},{"key":"stylecop:ElementsMustBeDocumented","rule":"ElementsMustBeDocumented","repository":"stylecop"},{"key":"stylecop:UseStringEmptyForEmptyStrings","rule":"UseStringEmptyForEmptyStrings","repository":"stylecop"}]} """>

type JSonIssuesRest = JsonProvider<""" {"maxResultsReached":false,"paging":{"pageIndex":1,"pageSize":200,"total":11,"fTotal":"11","pages":1},"issues":[{"key":"asjdkjashdjas","component":"org.example.csharpplayground:MyLibrary:Calc.cs","componentId":159349,"author":"email@sdsasa.com","project":"org.example.csharpplayground","rule":"stylecop:UsingDirectivesMustBePlacedWithinNamespace","status":"CONFIRMED","severity":"MAJOR","message":"All using directives must be placed inside of the namespace.","line":4,"creationDate":"2014-12-14T16:38:10+0200","resolution":"FIXED","updateDate":"2014-12-16T10:55:05+0200","fUpdateAge":"2 hours"},{"key":"1ad8a103-88f8-434d-9b8a-15f506630b11","actionPlan":"fsdfsdfsdf","component":"org.example.csharpplayground:MyLibrary:Calc.cs","componentId":159349,"project":"org.example.csharpplayground","rule":"stylecop:ElementsMustBeDocumented","status":"CONFIRMED","severity":"MAJOR","message":"The method must have a documentation header.","line":36,"debt":"20min","assignee":"jocs","creationDate":"2014-12-14T16:38:10+0200","updateDate":"2014-12-16T10:55:05+0200","fUpdateAge":"2 hours"},{"key":"20f89fd8-5cfa-4110-b9d3-c6e896f28b9a","component":"org.example.csharpplayground:MyLibrary:Calc.cs","componentId":159349,"project":"org.example.csharpplayground","rule":"stylecop:UsingDirectivesMustBePlacedWithinNamespace","status":"CONFIRMED","severity":"MAJOR","message":"All using directives must be placed inside of the namespace.","line":5,"debt":"10min","assignee":"jocs","creationDate":"2014-12-14T16:38:10+0200","updateDate":"2014-12-16T10:55:05+0200","fUpdateAge":"2 hours"},{"key":"2c704812-3288-46a4-a520-bece7f932bc1","component":"org.example.csharpplayground:MyLibrary:Calc.cs","componentId":159349,"project":"org.example.csharpplayground","rule":"stylecop:ElementsMustBeDocumented","status":"CONFIRMED","severity":"MAJOR","message":"The method must have a documentation header.","line":31,"debt":"20min","assignee":"jocs","creationDate":"2014-12-14T16:38:10+0200","updateDate":"2014-12-16T10:55:05+0200","fUpdateAge":"2 hours"},{"key":"54987465-5ef4-496d-80cc-d87ee5d16f84","component":"org.example.csharpplayground:MyLibrary:Calc.cs","componentId":159349,"project":"org.example.csharpplayground","rule":"stylecop:UsingDirectivesMustBePlacedWithinNamespace","status":"CONFIRMED","severity":"MAJOR","message":"All using directives must be placed inside of the namespace.","line":2,"debt":"10min","assignee":"jocs","creationDate":"2014-12-14T16:38:10+0200","closeDate":"2013-06-30T21:55:48+0300","updateDate":"2014-12-16T12:38:32+0200","fUpdateAge":"13 minutes","comments":[{"key":"d;fs;ldkfls","login":"jocs","userName":"Costa Jorge","htmlText":"test","markdown":"test","updatable":true,"createdAt":"2014-12-16T12:38:32+0200"}]},{"key":"6d840475-e501-4261-9555-06959ff6a0a1","component":"org.example.csharpplayground:MyLibrary:Calc.cs","componentId":159349,"project":"org.example.csharpplayground","rule":"stylecop:ElementsMustBeDocumented","status":"CONFIRMED","severity":"MAJOR","message":"The method must have a documentation header.","line":21,"debt":"20min","assignee":"jocs","creationDate":"2014-12-14T16:38:10+0200","updateDate":"2014-12-16T10:55:05+0200","fUpdateAge":"2 hours"},{"key":"a5ff0892-7864-4c4f-81ee-99168234cde6","component":"org.example.csharpplayground:MyLibrary:Calc.cs","componentId":159349,"project":"org.example.csharpplayground","rule":"stylecop:ElementMustBeginWithUpperCaseLetter","status":"CONFIRMED","severity":"MAJOR","message":"method names begin with an upper-case letter: horrible_code.","line":31,"debt":"15min","assignee":"jocs","creationDate":"2014-12-14T16:38:10+0200","updateDate":"2014-12-16T10:55:05+0200","fUpdateAge":"2 hours"},{"key":"adf880e6-5e33-432f-afc9-4c1e35454bfa","component":"org.example.csharpplayground:MyLibrary:Calc.cs","componentId":159349,"project":"org.example.csharpplayground","rule":"stylecop:ElementsMustBeDocumented","status":"CONFIRMED","severity":"MAJOR","message":"The class must have a documentation header.","line":9,"debt":"20min","assignee":"jocs","creationDate":"2014-12-14T16:38:10+0200","updateDate":"2014-12-16T10:55:05+0200","fUpdateAge":"2 hours"},{"key":"c886a29c-6cb1-401d-a05e-153e4fb12f76","component":"org.example.csharpplayground:MyLibrary:Calc.cs","componentId":159349,"project":"org.example.csharpplayground","rule":"stylecop:UsingDirectivesMustBePlacedWithinNamespace","status":"CONFIRMED","severity":"MAJOR","message":"All using directives must be placed inside of the namespace.","line":3,"debt":"10min","assignee":"jocs","creationDate":"2014-12-14T16:38:10+0200","updateDate":"2014-12-16T10:55:05+0200","fUpdateAge":"2 hours"},{"key":"dcf8a8d8-401d-49bc-82de-de13152d2030","component":"org.example.csharpplayground:MyLibrary:Calc.cs","componentId":159349,"project":"org.example.csharpplayground","rule":"stylecop:ElementsMustBeDocumented","status":"CONFIRMED","severity":"MAJOR","message":"The method must have a documentation header.","line":11,"debt":"20min","assignee":"jocs","creationDate":"2014-12-14T16:38:10+0200","updateDate":"2014-12-16T10:55:05+0200","fUpdateAge":"2 hours"},{"key":"ed58ccef-0317-4902-b2d1-8922d38dc8f6","component":"org.example.csharpplayground:MyLibrary:Calc.cs","componentId":159349,"project":"org.example.csharpplayground","rule":"stylecop:UsingDirectivesMustBePlacedWithinNamespace","status":"CONFIRMED","severity":"MAJOR","message":"All using directives must be placed inside of the namespace.","line":1,"debt":"10min","assignee":"jocs","creationDate":"2014-12-14T16:38:10+0200","updateDate":"2014-12-16T10:55:05+0200","fUpdateAge":"2 hours"}],"components":[{"key":"org.example.csharpplayground","id":159318,"qualifier":"TRK","name":"C# playground","longName":"C# playground"},{"key":"org.example.csharpplayground:MyLibrary:Calc.cs","id":159349,"qualifier":"FIL","name":"Calc.cs","longName":"Calc.cs","path":"Calc.cs","projectId":159318,"subProjectId":159323},{"key":"org.example.csharpplayground:MyLibrary","id":159323,"qualifier":"BRC","name":"MyLibrary","longName":"MyLibrary","path":"MyLibrary","projectId":159318,"subProjectId":159318}],"projects":[{"key":"org.example.csharpplayground","id":159318,"qualifier":"TRK","name":"C# playground","longName":"C# playground"}],"rules":[{"key":"stylecop:UsingDirectivesMustBePlacedWithinNamespace","name":"Using directives must be placed within namespace","desc":"Using directives must be placed within namespace","status":"READY"},{"key":"stylecop:ElementMustBeginWithUpperCaseLetter","name":"Element must begin with upper case letter","desc":"Element must begin with upper case letter","status":"READY"},{"key":"stylecop:ElementsMustBeDocumented","name":"Elements must be documented","desc":"Elements must be documented","status":"READY"}],"users":[{"login":"jocs","name":"Costa Jorge","active":true,"email":"jorge.costa@tekla.com"}]} """>

type JSonIssuesOld = JsonProvider<""" {"version":"3.4.1","violations_per_resource":
{
"resource0":[{"message":"This assembly is not decorated with the [CLSCompliant] attribute.","severity":"MAJOR","rule_key":"MarkAssemblyWithCLSCompliantRule","rule_repository":"gendarme","rule_name":"MarkAssemblyWithCLSCompliantRule"},{"message":"Sign 'CxxPlugin.Test.dll' with a strong name key.","severity":"MAJOR","rule_key":"AssembliesShouldHaveValidStrongNames","rule_repository":"fxcop","rule_name":"Assemblies should have valid strong names"}],
"resource1":[{"message":"This assembly is not decorated with the [CLSCompliant] attribute.","severity":"MAJOR","rule_key":"MarkAssemblyWithCLSCompliantRule","rule_repository":"gendarme","rule_name":"MarkAssemblyWithCLSCompliantRule"},{"message":"Sign 'CxxPlugin.Test.dll' with a strong name key.","severity":"MAJOR","rule_key":"AssembliesShouldHaveValidStrongNames","rule_repository":"fxcop","rule_name":"Assemblies should have valid strong names"}],
"resource2":[{"message":"This assembly is not decorated with the [CLSCompliant] attribute.","severity":"MAJOR","rule_key":"MarkAssemblyWithCLSCompliantRule","rule_repository":"gendarme","rule_name":"MarkAssemblyWithCLSCompliantRule"},{"message":"Sign 'CxxPlugin.Test.dll' with a strong name key.","severity":"MAJOR","rule_key":"AssembliesShouldHaveValidStrongNames","rule_repository":"fxcop","rule_name":"Assemblies should have valid strong names"}],
"resource3":[{"message":"Only 1 visible types are defined inside this namespace.","severity":"CRITICAL","rule_key":"AvoidSmallNamespaceRule","rule_repository":"gendarme","rule_name":"AvoidSmallNamespaceRule"},{"message":"Only 1 visible types are defined inside this namespace.","severity":"CRITICAL","rule_key":"AvoidSmallNamespaceRule","rule_repository":"gendarme","rule_name":"AvoidSmallNamespaceRule"},{"message":"Only 2 visible types are defined inside this namespace.","severity":"CRITICAL","rule_key":"AvoidSmallNamespaceRule","rule_repository":"gendarme","rule_name":"AvoidSmallNamespaceRule"},{"message":"This assembly is not decorated with the [CLSCompliant] attribute.","severity":"MAJOR","rule_key":"MarkAssemblyWithCLSCompliantRule","rule_repository":"gendarme","rule_name":"MarkAssemblyWithCLSCompliantRule"},{"message":"Consider merging the types defined in 'CxxPlugin' with another namespace.","severity":"MAJOR","rule_key":"AvoidNamespacesWithFewTypes","rule_repository":"fxcop","rule_name":"Avoid namespaces with few types"},{"message":"Consider merging the types defined in 'CxxPlugin.Options' with another namespace.","severity":"MAJOR","rule_key":"AvoidNamespacesWithFewTypes","rule_repository":"fxcop","rule_name":"Avoid namespaces with few types"},{"message":"Consider merging the types defined in 'CxxPlugin.ServerExtensions' with another namespace.","severity":"MAJOR","rule_key":"AvoidNamespacesWithFewTypes","rule_repository":"fxcop","rule_name":"Avoid namespaces with few types"},{"message":"Sign 'CxxPlugin.dll' with a strong name key.","severity":"MAJOR","rule_key":"AssembliesShouldHaveValidStrongNames","rule_repository":"fxcop","rule_name":"Assemblies should have valid strong names"}]
}}
""">

type JSonIssuesDryRun = JsonProvider<""" {"version":"3.7.4","violations_per_resource":{"tekla.tools:CxxPlugin.Test":[{"message":"Sign 'CxxPlugin.Test.dll' with a strong name key.","severity":"MAJOR","rule_key":"AssembliesShouldHaveValidStrongNames","rule_repository":"fxcop","switched_off":false,"is_new":true,"created_at":"2014-01-04T18:07:27+0200"},{"message":"This assembly is not decorated with the [CLSCompliant] attribute.","severity":"MAJOR","rule_key":"MarkAssemblyWithCLSCompliantRule","rule_repository":"gendarme","switched_off":false,"is_new":true,"created_at":"2014-01-04T18:07:27+0200"}],"tekla.tools:CxxPlugin":[{"message":"Sign 'CxxPlugin.dll' with a strong name key.","severity":"MAJOR","rule_key":"AssembliesShouldHaveValidStrongNames","rule_repository":"fxcop","switched_off":false,"is_new":false,"created_at":"2014-01-04T17:46:59+0200"},{"message":"Only 1 visible types are defined inside this namespace.","severity":"CRITICAL","rule_key":"AvoidSmallNamespaceRule","rule_repository":"gendarme","switched_off":false,"is_new":false,"created_at":"2014-01-04T17:46:59+0200"},{"message":"Only 1 visible types are defined inside this namespace.","severity":"CRITICAL","rule_key":"AvoidSmallNamespaceRule","rule_repository":"gendarme","switched_off":false,"is_new":false,"created_at":"2014-01-04T17:46:59+0200"},{"message":"This assembly is not decorated with the [CLSCompliant] attribute.","severity":"MAJOR","rule_key":"MarkAssemblyWithCLSCompliantRule","rule_repository":"gendarme","switched_off":false,"is_new":false,"created_at":"2014-01-04T17:46:59+0200"},{"message":"Consider merging the types defined in 'CxxPlugin' with another namespace.","severity":"MAJOR","rule_key":"AvoidNamespacesWithFewTypes","rule_repository":"fxcop","switched_off":false,"is_new":false,"created_at":"2014-01-04T17:46:59+0200"},{"message":"Only 2 visible types are defined inside this namespace.","severity":"CRITICAL","rule_key":"AvoidSmallNamespaceRule","rule_repository":"gendarme","switched_off":false,"is_new":false,"created_at":"2014-01-04T17:46:59+0200"},{"message":"Consider merging the types defined in 'CxxPlugin.Options' with another namespace.","severity":"MAJOR","rule_key":"AvoidNamespacesWithFewTypes","rule_repository":"fxcop","switched_off":false,"is_new":false,"created_at":"2014-01-04T17:46:59+0200"},{"message":"Consider merging the types defined in 'CxxPlugin.ServerExtensions' with another namespace.","severity":"MAJOR","rule_key":"AvoidNamespacesWithFewTypes","rule_repository":"fxcop","switched_off":false,"is_new":false,"created_at":"2014-01-04T17:46:59+0200"}]}} """>

type JSonDuplications = JsonProvider<""" [{"id":54245,"key":"groupId:ModuleId:ModelFile.cs","name":"ModelFile.cs","scope":"FIL","qualifier":"FIL","date":"2014-01-08T12:45:41+0200","creationDate":null,"lname":"ModelFile.cs","lang":"cs","msr":[{"key":"duplications_data","data":"<duplications><g><b s=\"439\" l=\"43\" r=\"groupId:ModuleId:file4.cs\"/><b s=\"499\" l=\"43\" r=\"groupId:ModuleId:file5.cs\"/></g></duplications>"}]},{"id":54595,"key":"groupId:ModuleId2:MeasurementGraphics/Providers/MeasurementGraphicsPointFace.cs","name":"MeasurementGraphicsPointFace.cs","scope":"FIL","qualifier":"FIL","date":"2014-01-08T12:45:41+0200","creationDate":null,"lname":"MeasurementGraphics/Providers/file1.cs","lang":"cs","msr":[{"key":"duplications_data","data":"<duplications><g><b s=\"141\" l=\"64\" r=\"groupId:ModuleId2:MeasurementGraphics/Providers/MeasurementGraphicsEdgeFace.cs\"/><b s=\"109\" l=\"64\" r=\"groupId:ModuleId2:MeasurementGraphics/Providers/MeasurementGraphicsPointFace.cs\"/></g></duplications>"}]},{"id":54232,"key":"groupId:ModuleId:Distance.cs","name":"Distance.cs","scope":"FIL","qualifier":"FIL","date":"2014-01-08T12:45:41+0200","creationDate":null,"lname":"Distance.cs","lang":"cs","msr":[{"key":"duplications_data","data":"<duplications><g><b s=\"825\" l=\"248\" r=\"groupId:ModuleId:Distance.cs\"/><b s=\"878\" l=\"216\" r=\"groupdId2:ModuleId3:Distance.cs\"/></g><g><b s=\"2250\" l=\"85\" r=\"groupId:ModuleId:Distance.cs\"/><b s=\"1308\" l=\"68\" r=\"groupdId2:ModuleId3:Distance.cs\"/></g><g><b s=\"740\" l=\"34\" r=\"groupId:ModuleId:Distance.cs\"/><b s=\"658\" l=\"34\" r=\"groupdId2:ModuleId3:Distance.cs\"/></g><g><b s=\"1822\" l=\"28\" r=\"groupId:ModuleId:Distance.cs\"/><b s=\"544\" l=\"28\" r=\"groupdId2:ModuleId3:Distance.cs\"/></g><g><b s=\"616\" l=\"28\" r=\"groupId:ModuleId:Distance.cs\"/><b s=\"352\" l=\"28\" r=\"groupdId2:ModuleId3:Distance.cs\"/></g><g><b s=\"2219\" l=\"19\" r=\"groupId:ModuleId:Distance.cs\"/><b s=\"1279\" l=\"19\" r=\"groupdId2:ModuleId3:Distance.cs\"/></g></duplications>"}]}] """>