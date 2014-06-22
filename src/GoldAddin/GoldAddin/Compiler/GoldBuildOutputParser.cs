using System;
using bsn.GoldParser.Parser;

namespace GoldAddin
{
	/// <summary>
	/// Resposnsible for parsing the raw messages output by the GOLDBuild 
	/// command line compiler
	/// </summary>
	public class GoldBuildOutputParser
	{
		readonly string fileName;		
		readonly GoldParsedDocument parsedDoc;

		/// <summary>
		/// Initializes a new instance of the <see cref="GoldAddin.GoldBuildOutputParser"/> class.
		/// </summary>
		/// <param name="fileName">File name.</param>
		public GoldBuildOutputParser(string fileName)
		{
			this.fileName = fileName;
			parsedDoc = new GoldParsedDocument ();
			parsedDoc.ParseFromFile (fileName);
		}

		/// <summary>
		/// Parses the raw message and returns a compiler 
		/// message object representing the raw message from the compiler
		/// </summary>
		/// <param name="data">A message provided by the GOLDbuild compiler. 
		/// Assumed to be a singe line</param>
		/// <returns>A CompilerMessage representation of the given message string</returns>
		public CompilerMessage ParseRawMessage(string data)
		{
			int line = 0;
			int column = 0;
			string text = data;
			MessageSeverity severity = getSeverity (data);

			bool isNewProblem = (!isContinuationMessage (data)) &&
			(severity == MessageSeverity.Warning || severity == MessageSeverity.Error);


			if (isNewProblem)
			{
				string problemText = getProblemText (data);
				text = problemText;
				var location = locateError (problemText);
				line = location.Line;
				column = location.Column;
			}
			else
			{
				//dont want to report an existing problem again, 
				// so downgrade the message
				severity = MessageSeverity.Info;
			}

			return new CompilerMessage(severity, text, fileName, line, column, data);
		}

		static readonly char[] fieldDelimiters = {':'};

		LineInfo locateError (string problemText)
		{
			//text will be in the form:
			// TYPE : DESCRIPTION : OPTIONAL
			// optional field may or may not be present
			// in at least one case, description field is not present
			string[] fields = problemText.Split (fieldDelimiters, StringSplitOptions.RemoveEmptyEntries);

			string type = string.Empty;
			string descr = string.Empty;

			if (fields.Length > 0)
				type = fields [0];
			if (fields.Length > 1)
				descr = fields [1];

			if (type.Contains ("Undefined"))
			{
				//use the first location of the token in question
				string name = getTokenText (type, descr);
				return getLocationOfToken (name);
			}

			if (type.Contains ("Unused") || type.Contains ("Unreachable"))
			{
				//search for declaration start
				string name = getTokenText (type, descr);
				return getLocationOfDefinition (name);
			}

			if (type.Contains ("start symbol"))
			{
				//search for start symbol declaration. may or may not be there
				return getLocationOfDefinition ("\"Start Symbol\"");
			}

			if (type.Contains ("Duplicate") || type.Contains ("redefined"))
			{
				string name = getTokenTextForDuplicateMessage (type, descr);
				return getLocationOfReDefinition (name);
			}
			
			//handle DFA error where multiple terminals can accept the same text
			//this particular error actually has four fields:
			// TYPE:DESCRIPTION:TERMINAL-LIST:DETAILS
			if (type.Contains("DFA") && descr.Contains ("Cannot distinguish between"))
			{
				//this error will contain a space seperated list of terminals
				//use the first terminal from the list for location info				
				string terminals = fields[2];
				string firstTerminal = terminals.Split(new char[]{' '},StringSplitOptions.RemoveEmptyEntries)[0];
				return getLocationOfDefinition(firstTerminal);
			}
			

			return new LineInfo (0, 0, 0);
		}




		static string getTokenText(string problemType, string description)
		{
			if (problemType.Contains ("terminal"))
			{
				//description is already terminal name!
				return description.Trim();
			}

			if (problemType.Contains ("rule"))
			{
				return GrammarText.FindNonTerminalName (description);
			}

			if (problemType.Contains ("Set"))
			{
				return GrammarText.FindSetName (description);
			}

			if (problemType.Contains ("property"))
			{
				return GrammarText.FindPropertyName (description);
			}

			return string.Empty;

		}


		//unfortunately duplicate errors must be handled slightly differently because 
		// duplicate terminal messages from GOLD do not match the pattern of all the
		// other messages. In those cases, the description field is empty!
		static string getTokenTextForDuplicateMessage(string problemType, string description)
		{
			if (problemType.Contains ("terminal"))
			{
				return GrammarText.FindTerminalName (problemType);
			}

			return getTokenText (problemType, description);
		}
					

		//name is already delimited. ie: {set name}, <non terminal>, ...
		// the location of the first matching token is returned
		LineInfo getLocationOfToken(string tokenName)
		{
			var enumerator = parsedDoc.FindUsesOf (tokenName).GetEnumerator();
			var location = new LineInfo (0, 0, 0);
			if (enumerator.MoveNext ())
			{
				location= enumerator.Current.Position;
			}
			return location;
		}

		//assumes tokenName is already delimited. eg: {set name}, <non terminal>, ...
		//returns the location where the given name is first defined
		LineInfo getLocationOfDefinition(string tokenName)
		{
			var enumerator = parsedDoc.FindDefinitionsByName (tokenName).GetEnumerator ();
			var location = new LineInfo (0, 0, 0);
			if (enumerator.MoveNext ())
				location =  enumerator.Current.Location;

			if (locationNotFound (location))
			{
				//some error messages could report unused, even though the symbol was not
				// defined formally (such as a terminal), which can be confusing. 
				// In those cases, just find the first use
				location = getLocationOfToken (tokenName);
			}

			return location;
		}

		//name is already delimited. ie: {set name}, <non terminal>, ...
		//returns the location of the second definition of the given name
		LineInfo getLocationOfReDefinition(string name)
		{
			var enumerator = parsedDoc.FindDefinitionsByName (name).GetEnumerator ();
			if (enumerator.MoveNext ())
			{
				if (enumerator.MoveNext ())
				{
					return enumerator.Current.Location;
				}
			}
			return new LineInfo (0, 0, 0);
		}
		

		//assumes that the given string is already in the form SEVERITY:PHASE:PROBLEM
		static string getProblemText (string data)
		{
			//first field is severity, second is phase
			const int FieldsToRemove = 2; //fields are separated by colons
			string messageText = data;


			//get the index of the 2nd field delimiter
			int dataLength = data.Length;
			int colonCount = 0;
			int index = 0;
			for (int i=0; i<dataLength; i++)
			{
				if (data [i] == ':')
				{				
					colonCount += 1;
					if (colonCount == FieldsToRemove)
					{
						index = i;
						break;
					}
				}

			}

			int startIndex = index + 1;
			if (startIndex < dataLength)
				messageText = data.Substring (startIndex).Trim();
			return messageText;
		}
		
		static MessageSeverity getSeverity (string rawMessage)
		{
			var severity = MessageSeverity.Info;
			if(startsWithIgnoreCase(rawMessage,"ERROR") || isContinuationMessage(rawMessage))
				severity = MessageSeverity.Error;
			else if(startsWithIgnoreCase(rawMessage,"WARNING"))			
				severity= MessageSeverity.Warning;
			return severity;
		}

		static bool startsWithIgnoreCase(string str, string substr)
		{
			return str.StartsWith (substr, StringComparison.OrdinalIgnoreCase);
		}

		static bool isContinuationMessage(string data)
		{
			return startsWithIgnoreCase (data, "Expecting");
		}

		static bool locationNotFound(LineInfo lineInfo)
		{
			return lineInfo.Line == 0 && lineInfo.Column == 0;
		}
	}
}

