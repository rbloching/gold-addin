using System;
using MonoDevelop.Projects;


namespace GoldAddin
{
	public class GoldLanguageBinding : ILanguageBinding
	{		
		public const string LanguageName = "GOLD";

		public bool IsSourceCodeFile (MonoDevelop.Core.FilePath fileName)
		{
			return GoldLanguageBinding.IsSourceFile(fileName);
		}

		public MonoDevelop.Core.FilePath GetFileName (MonoDevelop.Core.FilePath fileNameWithoutExtension)
		{
			return fileNameWithoutExtension + ".grm";
		}

		public string Language 
		{
			get{ return LanguageName;}
		}

		public string SingleLineCommentTag 
		{
			get { return "!";}				
		}

		public string BlockCommentStartTag 
		{
			get {return "!*";}
		}

		public string BlockCommentEndTag 
		{
			get {return "*!";}
		}
		
		public static bool IsSourceFile(string fileName)
		{
			return System.IO.Path.GetExtension(fileName).CompareTo(".grm") == 0;
		}

	}

}
