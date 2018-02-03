VSSonarQubeExtension
====================
[![Issue Stats](http://issuestats.com/github/TrimbleSolutionsCorporation/VSSonarQubeExtension/badge/issue)](http://issuestats.com/github/TrimbleSolutionsCorporation/VSSonarQubeExtension)
[![Issue Stats](http://issuestats.com/github/TrimbleSolutionsCorporation/VSSonarQubeExtension/badge/pr)](http://issuestats.com/github/TrimbleSolutionsCorporation/VSSonarQubeExtension)

[![Build status](https://ci.appveyor.com/api/projects/status/w03onktfvppbimow/branch/master?svg=true)](https://ci.appveyor.com/project/TrimbleSolutionsCorporation/vssonarqubeextension/branch/master)

This is a Extension for Visual Studio to interact wiht SonarQube (TM).

# Installation
Use the Visual Studio Extension Manager and search for VSSonarExtension, Visual Studio 2012, 2013, 2015 and 2017 are supported.

## Issues with Resharper
In case you cant see anything in the options dialog, first uninstall Resharper and open the options dialog. After this you can install Resharper again (go figure)!. 

# Alternatives
SonarLint is the official extension provided by SonarSource. It support C# and VB.Net. 

# Why this extension
There are several reasons why we use this extension, first this extension existed before SonarLint and it has been maintained since its inception. Currently it provides some features that SonarLint does not provide so we will continue its support until we find there is use for it. And in the good spirit of open source, as long as, someone is willing to maintain a piece of software that others use its worth keeping.

Now the features that are provided
- It supports C++ open source plugin, https://github.com/SonarOpenCommunity/sonar-cxx. This is a free alternative to the official c++ plugin provided by SonarSource. 
- It's less invasive then SonarLint. In our context we have to handle multiple solutions that share or not a similar quality profile, this extension allows checking of issues without the need of installing any diagnostics into the projects the solutions. Upgrades are nonexistent, since everything is handled by the extension you don't need to worry about updating those diagnostics.
- F# is supported, we include FSharplint runner and with sonar-fsharp-plugin (https://github.com/jmecsoftware/sonar-fsharp-plugin) you can start analyzing F# in SonarQube
- Coverage, you can display sonar coverage in visual studio
- Custom Roslyn Runner (https://github.com/jmecsoftware/sonarqube-roslyn-plugin), in conjunction with this plugin the extension can run your custom diagnostics automatically. Again in nonintrusive way. And no Sdk needed.
- SonarLing Roslyn diagnostics are supported (https://github.com/jmecsoftware/DotNetAndFSharpPlugin)
- we have many other plugins that integrate with other systems, like git, testtrack.


# Getting Started
All information on configuration and usage will be found in the wiki pages. [Home](https://github.com/TeklaCorp/VSSonarQubeExtension/wiki)

# Getting Involved

Versions of Visual Studio 2012, 2013 and 2015 can be used. There is one solution for each version. VS Extensibility part is kept in separe project for each version. 

Submit pull request if you have bug fixes or improvements.

# License

This program is free software; you can redistribute it and/or modify it under the terms of the GNU Lesser General Public License
as published by the Free Software Foundation; either version 3 of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details. 
You should have received a copy of the GNU Lesser General Public License along with this program; if not, write to the Free
Software Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
