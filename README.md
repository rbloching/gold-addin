gold-addin
==========

GOLD parser builder addin for MonoDevelop / Xamarin Studio

The GOLD meta-language binding makes it possible to edit, compile, and (someday) test grammars using Devin Cook's Grammar Oriented Langauage Development (GOLD) within MonoDevelop and Xamarin Studio. For information on GOLD, see [www.goldparser.org](http://www.goldparser.org)


Building Project
----------------
Use Xamarin Studio 4.2 (or later) to build the project. Addin Maker add-in should be installed (this can be done from the Add-in Manager of the IDE).


Installation
------------
Download the GOLD command line builder 5.2 from [http://goldparser.org/builder/index.htm](http://goldparser.org/builder/index.htm). Currently this is only available for Windows. Add the location of GOLDbuild.exe to the environment PATH.


Features
--------
###Editing Support
*	Syntax highlighting
*	Rename-refactoring for terminals, non-terminals, and sets
*	Code completion, including built-in sets and properties

###Compilation Support
*	Performs all analysis phases (Grammar, LALR, DFA) and generates parse table with a single step
*	Supports version 1 (CGT) and version 5 (EGT) output
*	File location highlighted for all errors found in Grammar Analysis phase


Art Credit
--------
Some icons by [Yusuke Kamiyamane](http://p.yusukekamiyamane.com). Licensed under a [Creative Commons Attribution 3.0 License.](http://creativecommons.org/licenses/by/3.0)