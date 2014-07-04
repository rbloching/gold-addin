using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using MonoDevelop.Core;
using MonoDevelop.Projects;

namespace GoldAddin
{

    /// <summary>
	/// Represents an IDE project for developing languages with GOLD
    /// </summary>
	public class GoldProject : Project
	{        
		ICompiler goldCompiler;
		BuildResult currentBuildResult;
		IProgressMonitor monitor; 
		List<string> projectTypes;

		/// <summary>
		/// Initializes a new instance of the <see cref="GoldAddin.GoldProject"/> class.
		/// </summary>
		/// <remarks>
		/// this constructor is invoked when an existing project is opened in the IDE.
		/// configuration settings will be loaded from .mdproj file
		/// </remarks>
		public GoldProject()
		{
			init ();
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="GoldAddin.GoldProject"/> class.
		/// </summary>
		/// <remarks>this constructor is invoked when a new project is created in the IDE</remarks>
		public GoldProject(ProjectCreateInformation info, XmlElement projectOptions)
		{
			init ();

			//a project will not build unless at least 1 configuration is set
			var config = (GoldProjectConfiguration)(CreateConfiguration ("Release"));
			config.DebugMode = false;
			config.OutputFormat = GrammarTableFormat.CompiledGrammarTable;
			config.GrammarTableName = info.ProjectName;
			config.OutputDirectory = info.BinPath;
			Configurations.Add(config);
		}

		void init ()
		{
			projectTypes = new List<string> ();
			projectTypes.Add (GoldLanguageBinding.LanguageName);

			goldCompiler = new GoldCompiler ();
			goldCompiler.MessageOutput += compilerOutputHandler;
			currentBuildResult = new BuildResult ();
		}


        /// <summary>
        /// Gets the project type
        /// </summary>        
		[Obsolete]
		public override string ProjectType 
		{            
			get { return GoldLanguageBinding.LanguageName; }
		}

		/// <summary>
		/// Gets the project type and its base types.
		/// </summary>
		public override IEnumerable<string> GetProjectTypes ()
		{
			return projectTypes;
		}

		protected override BuildResult DoBuild (IProgressMonitor monitor, ConfigurationSelector configuration)
		{
            currentBuildResult.ClearErrors();            
			var config = (GoldProjectConfiguration)(GetConfiguration (configuration));

			//check the number of grammar files in the project set to compile
			//a gold project can have only a single grammar file set to compile
			int grmFileCount = 0;
			ProjectFile file = null;
			foreach(ProjectFile projectFile in Files)
			{
				if(isGrammarWithCompileAction(projectFile))
				{
					grmFileCount+=1;
					file = projectFile;
				}
			}

			if (grmFileCount == 0)
			{
				currentBuildResult.AddWarning ("Nothing to build: there is no grammar file associated with this project with the build action set to compile");
			}
			else if (grmFileCount > 1)
			{
				currentBuildResult.AddError ("Cannot build project: this project contains multiple grammar files set to compile. Select a single grammar file to build for the project.");
			}
			else
			{
				//there was 1 grammar file with build action set to compile
				//variable 'file' is set to that file
				compileGrammarFile (file.Name, config, monitor);
			}
			return currentBuildResult;                       
		}

		
		protected override void DoClean (IProgressMonitor monitor, ConfigurationSelector configuration)
		{
			base.DoClean (monitor, configuration);

			var config = (GoldProjectConfiguration)(GetConfiguration (configuration));
			foreach (ProjectFile file in Files)
			{
				if (IsCompileable (file.Name))
				{
					string str = Path.GetFileNameWithoutExtension (file.Name);
					str = config.OutputDirectory.Combine (str);
					File.Delete (str + ".cgt");
					File.Delete (str + ".egt");
					File.Delete (str + ".log");
				}
			}
		}	

		public override bool IsCompileable(string fileName)
		{
			return GoldLanguageBinding.IsSourceFile (fileName);
		}

		//true if the given file is a grammar file that is set to compile build action
		static bool isGrammarWithCompileAction(ProjectFile file)
		{
			if (file == null)
				return false;
			return GoldLanguageBinding.IsSourceFile(file.Name) && file.BuildAction == BuildAction.Compile;
		}

        		      
        public override SolutionItemConfiguration CreateConfiguration(string name)
        {            
			var config = new GoldProjectConfiguration();
			config.Name = name;
			return config;
        }
			
		//compiles the given grammar file		                       
		void compileGrammarFile(string fileName, GoldProjectConfiguration config, IProgressMonitor progressMonitor)
		{
			progressMonitor.Log.WriteLine("compiling " + fileName + ":");
			monitor = progressMonitor; 
			string inputFile = fileName;
			string outputFile = getOutputFileName (config);
			goldCompiler.Compile (inputFile, outputFile);
		}

		static string getOutputFileName(GoldProjectConfiguration config)
		{
			string extension = "cgt";
			if (config.OutputFormat == GrammarTableFormat.EnhancedGrammarTable)
				extension = "egt";				
			return config.OutputDirectory+"/"+config.GrammarTableName + "." + extension;
		}
	

        void compilerOutputHandler(CompilerMessage message)
        {
            //write to Build Output Window
			if (message.RawText.Length > 0)
				monitor.Log.WriteLine (message.RawText);

			//write to Message Window
			if (message.Severity == MessageSeverity.Error)
				currentBuildResult.AddError (message.File, message.Line, message.Column, "", message.Text);
            else if (message.Severity == MessageSeverity.Warning)
				currentBuildResult.AddWarning(message.File, message.Line, message.Column, "", message.Text);

        }	

		/// <summary>
		/// Gets the default build action for a file
		/// </summary>
		/// <returns>The default build action.</returns>
		/// <param name="fileName">File name.</param>
		public override string GetDefaultBuildAction (string fileName)
		{
			//A gold project should have only 1 grammar file set to compile
			//so the default build action is none, unless there are no other
			// .grm files set to compile

			string result = BuildAction.None;
			if (GoldLanguageBinding.IsSourceFile (fileName))
			{
				if (countOfGrammarFiles () == 0)
					result = BuildAction.Compile;
			}
			return result;
		}
			
		int countOfGrammarFiles()
		{
			int count = 0;
			foreach (ProjectFile file in Files)
			{
				if(isGrammarWithCompileAction(file))
				{
					count += 1;
				}
			}
			return count;
		}
	}
}

