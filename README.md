VSSonarQubeExtension
====================
[![Issue Stats](http://issuestats.com/github/TrimbleSolutionsCorporation/VSSonarQubeExtension/badge/issue)](http://issuestats.com/github/TrimbleSolutionsCorporation/VSSonarQubeExtension)
[![Issue Stats](http://issuestats.com/github/TrimbleSolutionsCorporation/VSSonarQubeExtension/badge/pr)](http://issuestats.com/github/TrimbleSolutionsCorporation/VSSonarQubeExtension)

[![Build status](https://ci.appveyor.com/api/projects/status/w03onktfvppbimow/branch/master?svg=true)](https://ci.appveyor.com/project/TrimbleSolutionsCorporation/vssonarqubeextension/branch/master)

This is a Extension for Visual Studio to interact wiht SonarQube (TM).

# Installation
Use the Visual Studio Extension Manager and search for VSSonarExtension, Visual Studio 2012, 2013, 2015 and 2017 are supported.

# Alternatives
SonarLint is the official extension provided by SonarSource. It support C# and VB.Net. 

# Why this extension
There are several reasons why we use this extension, first this extension existed before SonarLint and it has been maitained since its inception. Currently it provides some features that SonarLint does not provide so we will continue its support until we find there is use for it.

Now the features that are provided:
- it supports C++ open source plugin, https://github.com/SonarOpenCommunity/sonar-cxx. This is a free alternative to the official c++ plugin provided by SonarSource. 
- Its less invasive then SonarLint. In our context we have to handle multiple solutions that share or not a similar quality profile, this extension allow checking of issues without the need of installing any diagnostics into the projects the solutions. Upgrades are non existent, since everything is handled by the extension  assas sada  sa 



# Getting Started
All information on configuration and usage will be found in the wiki pages. [Home](https://github.com/TeklaCorp/VSSonarQubeExtension/wiki)

# Getting Involved

Versions of Visual Studio 2012, 2013 and 2015 can be used. There is one solution for each version. VS Extensability part is kept in separe project for each version. 

Submit pull request if you have bug fixes or improvements.

# License

This program is free software; you can redistribute it and/or modify it under the terms of the GNU Lesser General Public License
as published by the Free Software Foundation; either version 3 of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details. 
You should have received a copy of the GNU Lesser General Public License along with this program; if not, write to the Free
Software Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
