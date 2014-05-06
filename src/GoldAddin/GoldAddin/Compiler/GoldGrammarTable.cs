using System.Reflection;
using System.IO;
using bsn.GoldParser.Grammar;


namespace GoldAddin
{
	/// <summary>
	/// Provides access to the compiled grammar tables for the GOLD meta-language
	/// </summary>
	public static class GoldGrammarTable
	{
		static readonly CompiledGrammar goldGrammarV1;
		static readonly CompiledGrammar goldGrammarV5;

		//TODO: create a compiled table for version 5 of the meta-language
		static readonly string v1Resource = "GoldAddin.Resources.GOLD Meta-Language (2.6.0).egt";
		static readonly string v5Resource = "GoldAddin.Resources.GOLD Meta-Language (2.6.0).egt";

		/// <summary>
		/// Gets the compiled grammar table for the associated GOLD grammar version.
		/// Version 5 is the default
		/// </summary>
		/// <returns>The compiled grammar table</returns>
		/// <param name="version">GOLD meta-langauge version</param>
		public static CompiledGrammar GetCompiledGrammar(CgtVersion version)
		{
			CompiledGrammar grammar = goldGrammarV5;
			if (version == CgtVersion.V1_0)
			{
				grammar = goldGrammarV1;
			}
			return grammar;
		}

		static GoldGrammarTable()
		{
			goldGrammarV1 = loadFromResource (v1Resource);
			goldGrammarV5 = loadFromResource (v5Resource);
		}

		static CompiledGrammar loadFromResource(string resourceName)
		{
			Assembly assembly = Assembly.GetExecutingAssembly();
			Stream stream = assembly.GetManifestResourceStream(resourceName);
			CompiledGrammar grammar = CompiledGrammar.Load(stream);
			return grammar;
		}
	}
}

