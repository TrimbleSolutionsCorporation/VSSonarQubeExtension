namespace SonarRestService

open System.Collections.ObjectModel
open FSharp.Data
open System.Xml.Linq

type IssueResponse = XmlProvider<"""<?xml version="1.0" encoding="UTF-8"?>
<issues>
  <maxResultsReached>false</maxResultsReached>
  <paging>
    <pageIndex>1</pageIndex>
    <pageSize>999999</pageSize>
    <total>59</total>
    <pages>1</pages>
  </paging>
  <issues>
    <issue>
      <key>f63d759b-0052-4b74-b3cd-8195c8c38d4f</key>
      <component>groupid:projectid:directory/filename.cpp</component>
      <project>groupid:projectid</project>
      <rule>common-c++:InsufficientBranchCoverage</rule>
      <status>REOPENED</status>
      <severity>MAJOR</severity>
      <message>9 more branches need to be covered by unit tests to reach the minimum threshold of 30.0% branch coverage.</message>
      <effortToFix>9.0</effortToFix>
      <creationDate>2013-03-07T21:56:08+0200</creationDate>
      <updateDate>2013-04-23T16:04:23+0300</updateDate>
      <comments>
        <comment>
          <key>4b48ab40-9e62-4a1e-a20f-90dff0694e0b</key>
          <login>loginname1</login>
          <htmlText>this is a test</htmlText>
          <createdAt>2013-04-23T16:00:28+0300</createdAt>
        </comment>
        <comment>
          <key>cac28873-95f3-40da-9102-4eeeb4297cec</key>
          <login>loginname</login>
          <htmlText>wqrwerw</htmlText>
          <createdAt>2013-04-23T16:03:47+0300</createdAt>
        </comment>
      </comments>
    </issue>
    <issue>
      <key>d3ec2ee7-74eb-4c6b-bacf-2a170d92fa6e</key>
      <component>groupid:projectid:directory/filename.cpp</component>
      <project>groupid:projectid</project>
      <rule>cxxexternal:cpplint.whitespace/parens-2</rule>
      <status>CLOSED</status>
      <resolution>FIXED</resolution>
      <severity>INFO</severity>
      <message>Extra space before ( in function call</message>
      <line>377</line>
      <creationDate>2013-06-25T05:08:29+0300</creationDate>
      <updateDate>2013-06-30T21:55:48+0300</updateDate>
      <closeDate>2013-06-30T21:55:48+0300</closeDate>
    </issue>
    <issue>
      <key>b47b18ed-3b51-48b2-b5fc-f40482eccd2f</key>
      <component>groupid:projectid:directory/filename.cpp</component>
      <project>groupid:projectid</project>
      <rule>cxxexternal:cpplint.whitespace/parens-2</rule>
      <status>OPEN</status>
      <severity>MINOR</severity>
      <message>Extra space before ( in function call</message>
      <line>342</line>
      <creationDate>2013-07-03T09:27:50+0300</creationDate>
      <updateDate>2013-07-03T09:27:50+0300</updateDate>
    </issue>
    <issue>
      <key>cb32ea80-0928-41c7-afb1-c36b42a0d5e0</key>
      <component>groupid:projectid:directory/filename.cpp</component>
      <project>groupid:projectid</project>
      <rule>cxxexternal:cpplint.whitespace/parens-2</rule>
      <status>OPEN</status>
      <severity>MINOR</severity>
      <message>Extra space before ( in function call</message>
      <line>377</line>
      <creationDate>2013-07-03T09:27:50+0300</creationDate>
      <updateDate>2013-07-03T09:27:50+0300</updateDate>
    </issue>
    <issue>
      <key>cfa6370e-3bc7-4d8b-8131-8177ec360c74</key>
      <component>groupid:projectid:directory/filename.cpp</component>
      <project>groupid:projectid</project>
      <rule>cxxexternal:cpplint.whitespace/parens-2</rule>
      <status>OPEN</status>
      <severity>MINOR</severity>
      <message>Extra space before ( in function call</message>
      <line>375</line>
      <creationDate>2013-07-03T09:27:50+0300</creationDate>
      <updateDate>2013-07-03T09:27:50+0300</updateDate>
    </issue>
    <issue>
      <key>d580b795-c1e8-4fef-833d-500cd420733d</key>
      <component>groupid:projectid:directory/filename.cpp</component>
      <project>groupid:projectid</project>
      <rule>cxxexternal:cpplint.whitespace/parens-2</rule>
      <status>OPEN</status>
      <severity>MINOR</severity>
      <message>Extra space before ( in function call</message>
      <line>282</line>
      <creationDate>2013-07-03T09:27:50+0300</creationDate>
      <updateDate>2013-07-03T09:27:50+0300</updateDate>
    </issue>
    <issue>
      <key>d5fdb30d-653e-44bc-b65f-e1b26417f0cf</key>
      <component>groupid:projectid:directory/filename.cpp</component>
      <project>groupid:projectid</project>
      <rule>cxxexternal:cpplint.whitespace/parens-2</rule>
      <status>OPEN</status>
      <severity>MINOR</severity>
      <message>Extra space before ( in function call</message>
      <line>354</line>
      <creationDate>2013-07-03T09:27:50+0300</creationDate>
      <updateDate>2013-07-03T09:27:50+0300</updateDate>
    </issue>
    <issue>
      <key>dbc7fae4-de40-4e5b-9ab2-f3c87d44e32d</key>
      <component>groupid:projectid:directory/filename.cpp</component>
      <project>groupid:projectid</project>
      <rule>cxxexternal:cpplint.whitespace/semicolon-3</rule>
      <status>OPEN</status>
      <severity>MINOR</severity>
      <message>Extra space before last semicolon. If this should be an empty statement, use {} instead.</message>
      <line>250</line>
      <creationDate>2013-07-03T09:27:50+0300</creationDate>
      <updateDate>2013-07-03T09:27:50+0300</updateDate>
    </issue>
    <issue>
      <key>e61b1736-95e4-4784-b29c-388d031dd7b0</key>
      <component>groupid:projectid:directory/filename.cpp</component>
      <project>groupid:projectid</project>
      <rule>cxxexternal:cpplint.whitespace/parens-2</rule>
      <status>OPEN</status>
      <severity>MINOR</severity>
      <message>Extra space before ( in function call</message>
      <line>352</line>
      <creationDate>2013-07-03T09:27:50+0300</creationDate>
      <updateDate>2013-07-03T09:27:50+0300</updateDate>
    </issue>
    <issue>
      <key>efad1eab-33c3-4798-bd10-3f4f7b69d71a</key>
      <component>groupid:projectid:directory/filename.cpp</component>
      <project>groupid:projectid</project>
      <rule>cxxexternal:cpplint.readability/namespace-0</rule>
      <status>OPEN</status>
      <severity>INFO</severity>
      <message>Namespace should be terminated with // namespace geometry</message>
      <line>917</line>
      <creationDate>2013-07-03T09:27:50+0300</creationDate>
      <updateDate>2013-07-03T09:27:50+0300</updateDate>
    </issue>
    <issue>
      <key>f542a20a-c63f-4cfd-a09d-5d40c93d9026</key>
      <component>groupid:projectid:directory/filename.cpp</component>
      <project>groupid:projectid</project>
      <rule>cxxexternal:cpplint.whitespace/parens-2</rule>
      <status>OPEN</status>
      <severity>MINOR</severity>
      <message>Extra space before ( in function call</message>
      <line>345</line>
      <creationDate>2013-07-03T09:27:50+0300</creationDate>
      <updateDate>2013-07-03T09:27:50+0300</updateDate>
    </issue>
    <issue>
      <key>f92c62d1-1ede-4cbc-8796-5f8897786d7a</key>
      <component>groupid:projectid:directory/filename.cpp</component>
      <project>groupid:projectid</project>
      <rule>cxxexternal:cpplint.whitespace/parens-2</rule>
      <status>OPEN</status>
      <severity>MINOR</severity>
      <message>Extra space before ( in function call</message>
      <line>360</line>
      <creationDate>2013-07-03T09:27:50+0300</creationDate>
      <updateDate>2013-07-03T09:27:50+0300</updateDate>
    </issue>
    <issue>
      <key>fd0b3d20-796a-4425-a7fb-1aed4c12bf31</key>
      <component>groupid:projectid:directory/filename.cpp</component>
      <project>groupid:projectid</project>
      <rule>cxxexternal:cpplint.whitespace/parens-2</rule>
      <status>OPEN</status>
      <severity>MINOR</severity>
      <message>Extra space before ( in function call</message>
      <line>242</line>
      <creationDate>2013-07-03T09:27:50+0300</creationDate>
      <updateDate>2013-07-03T09:27:50+0300</updateDate>
    </issue>
  </issues>
  <components>
    <component>
      <key>groupid:projectid:directory/filename.cpp</key>
      <qualifier>FIL</qualifier>
      <name>vector_utilities.cpp</name>
      <longName>directory/filename.cpp</longName>
    </component>
  </components>
  <projects>
    <project>
      <key>groupid:projectid</key>
      <qualifier>TRK</qualifier>
      <name>Common</name>
      <longName>Common</longName>
    </project>
  </projects>
  <rules>
    <rule>
      <key>cxxexternal:cpplint.readability/namespace-0</key>
      <name>Namespace should be terminated with "// namespace %s"  %self.name</name>
      <desc>Namespace should be terminated with "// namespace %s"  %self.name</desc>
      <status>READY</status>
    </rule>
    <rule>
      <key>common-c++:InsufficientLineCoverage</key>
      <name>Insufficient line coverage by unit tests</name>
      <desc>&lt;p&gt;A violation is created on a file as soon as the line coverage on this file is less than the required threshold. It gives the number of lines to be covered in order to reach the required threshold.&lt;/p&gt;</desc>
      <status>READY</status>
    </rule>
    <rule>
      <key>cxxexternal:cpplint.whitespace/parens-2</key>
      <name>Extra space before ( in function call</name>
      <desc>Extra space before ( in function call</desc>
      <status>READY</status>
    </rule>
    <rule>
      <key>common-c++:InsufficientBranchCoverage</key>
      <name>Insufficient branch coverage by unit tests</name>
      <desc>&lt;p&gt;A violation is created on a file as soon as the branch coverage on this file is less than the required threshold.It gives the number of lines to be covered in order to reach the required threshold.&lt;/p&gt;</desc>
      <status>READY</status>
    </rule>
    <rule>
      <key>cxxexternal:cpplint.whitespace/semicolon-3</key>
      <name>Extra space before last semicolon. If this should be an empty   statement, use {} instead.</name>
      <desc>Extra space before last semicolon. If this should be an empty   statement, use {} instead.</desc>
      <status>READY</status>
    </rule>
    <rule>
      <key>cppcheck:variableScope</key>
      <name>The scope of the variable can be reduced</name>
      <desc>The scope of the variable can be reduced.</desc>
      <status>READY</status>
    </rule>
    <rule>
      <key>cppcheck:unusedFunction</key>
      <name>Unused function</name>
      <desc>The function is never used.</desc>
      <status>READY</status>
    </rule>
    <rule>
      <key>cxxexternal:cpplint.build/include-0</key>
      <name>"%s" already included at %s:%s  % (include, filename, include_state[include])</name>
      <desc>"%s" already included at %s:%s  % (include, filename, include_state[include])</desc>
      <status>READY</status>
    </rule>
  </rules>
  <users>
    <user>
      <login>loginname</login>
      <name>Real Name</name>
      <active>true</active>
      <email>user.name@organization.com</email>
    </user>
    <user>
      <login>loginname1</login>
      <name></name>
      <active>true</active>
    </user>
  </users>
</issues>
""">