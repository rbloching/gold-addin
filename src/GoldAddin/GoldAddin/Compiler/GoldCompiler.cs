using System.Diagnostics;
using System.ComponentModel;
using System.IO;

namespace GoldAddin
{
    /// <summary>
	/// Compiler for GOLD grammar files 
	/// </summary>        
    public class GoldCompiler : ICompiler
    {        
	    ProcessStartInfo startInfo;
        string inputFileName = string.Empty;
		const string goldCmdPath = "GOLDbuild.exe";	
		IParser parser;
		GoldBuildOutputParser outputParser;

		/// <summary>
		/// Initializes a new instance of the <see cref="GoldAddin.GoldCompiler"/> class.
		/// </summary>
		public GoldCompiler()
		{
			startInfo = new ProcessStartInfo ();
			startInfo.FileName = goldCmdPath;
			startInfo.CreateNoWindow = true;
			startInfo.UseShellExecute = false;
			startInfo.ErrorDialog = false;
			startInfo.RedirectStandardOutput = true;
			startInfo.RedirectStandardError = true;

			parser = new GrammarParser ();
			parser.ErrorFound += parseErrorFound;
		}




		/// <summary>
		/// Compile the specified grammar 
		/// </summary>
		/// <param name="grammarFile">Grammar file to compile</param>
		/// <param name="outputFile">The name of the binary file output</param>
		/// <remarks>The GOLD compiler outputs 2 versions of binary grammar tables:
		/// CGT and EGT. The format generated depends on the file extension of the 
		/// output file (.cgt or .egt)</remarks>
		public void Compile(string grammarFile, string outputFile)
		{
			//create a helper that will classify the error messages emitted by GOLD
			inputFileName = grammarFile;
			string text = slurpFile (inputFileName);
			outputParser = new GoldBuildOutputParser (inputFileName);

			//perform an initial parse on the file.
			//this will get the error locations for us,
			//since we can't get this info from the gold compiler
			parser.Parse (text);

			//launch the GOLD command line compiler
			startInfo.Arguments = constructArguments (grammarFile, outputFile);
			startInfo.WorkingDirectory = Path.GetDirectoryName (outputFile);
			var p = new Process();
			p.StartInfo = startInfo;
			p.OutputDataReceived += outputHandler;            
			try
			{
				p.Start();
				p.BeginOutputReadLine();
				p.BeginErrorReadLine();
				p.WaitForExit();
				p.Close();
			}
			catch(Win32Exception) 
			{
				var message = new CompilerMessage (MessageSeverity.Error,
					goldCmdPath + " was not found on path",				                                  
					string.Empty, 0, 0);
				onCompileStepComplete (message);
			}
		}


		static string slurpFile(string file)
		{
			if (string.IsNullOrEmpty(file) || !File.Exists (file))
			{
				return string.Empty;
			}
			return File.ReadAllText (file);
		}


		static string fileExtensionFromFormat(GrammarTableFormat format)
		{
			string ext = "egt";
			if (format == GrammarTableFormat.CompiledGrammarTable)
				ext = "cgt";
			return ext;
		}

		static string constructArguments(string inputFile, string outputFile)
		{
			string logFileName = Path.GetFileNameWithoutExtension (outputFile) + ".log";

			//quote file names in case they contain spaces
			string args = "\"" + inputFile + "\"" + " \"" + outputFile + "\"" +" \""+logFileName+"\"";
			return args;
		}

					
	    public event CompilerEventHandler MessageOutput;



		void parseErrorFound(CompilerMessage message)
		{
			//file information should be included for outgoing message
			var completeMessage = new CompilerMessage (message.Severity,
			                                          message.Text,
			                                          inputFileName,
			                                          message.Line,
			                                          message.Column);
			onCompileStepComplete (completeMessage);
		}
				
		void outputHandler(object sender, DataReceivedEventArgs e)
		{
            if (e.Data != null)
            {               
				var message = outputParser.ParseRawMessage(e.Data);

				//some messages need to be suppressed because they were already checked
				//by the parser. Otherwise, duplicates will show in the Errors Pad
				string rawText = message.RawText;
				if(rawText.Contains("Grammar") && (rawText.Contains("Syntax") || rawText.Contains("Lexical") || rawText.Contains("Comment")))
				{
					message = new CompilerMessage (MessageSeverity.Info, string.Empty, string.Empty, 0, 0, rawText);
				}


				onCompileStepComplete (message);
			}
		}
					               			
		void onCompileStepComplete(CompilerMessage message)
		{
			CompilerEventHandler copy = MessageOutput;
            if (copy != null)            
                copy(message);            
		}						             
    }				        
}
