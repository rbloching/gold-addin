using MonoDevelop.Projects;

namespace GoldAddin
{
    /// <summary>
    /// Responsible for instantiating a GOLD project 
    /// </summary>
	public class GoldProjectBinding : IProjectBinding
	{		
		/// <summary>
		/// Returns an object representing a GOLD project. Used when creating a new project in the IDE
		/// </summary>		
		public Project CreateProject (ProjectCreateInformation info, System.Xml.XmlElement projectOptions)
		{
			return new GoldProject(info, projectOptions);
		}

		
        /// <summary>
        /// Generates a GOLD project that contains just the given file
        /// </summary>        
        /// <returns>null if the file is not valid for a GOLD project</returns>
        public Project CreateSingleFileProject (string sourceFile)
		{
			if (CanCreateSingleFileProject(sourceFile))
                return new GoldProject();
			return null;
		}

        /// <summary>
        /// returns true if the given source file is a GOLD grammar definition.
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <returns></returns>
        /// <remarks>grammars written in GOLD are defined with a single file, so 
        /// it makes sense to allow a single file project in the IDE.</remarks>
		public bool CanCreateSingleFileProject (string sourceFile)
		{            
            return GoldLanguageBinding.IsSourceFile(sourceFile);
		}

        /// <summary>
        /// Get the project type
        /// </summary>
        /// <remarks>
        /// this must match the project binding id used in the .addin.xml manifest
        /// and the project templates
        /// </remarks>
		public string Name 
		{            
			get {return "GoldProject";}
		}
        
		
	}
}

