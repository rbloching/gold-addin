using System.Text.RegularExpressions;

namespace GoldAddin
{
	/// <summary>
	/// Helper functions for working with grammar text
	/// </summary>
	public static class GrammarText
	{
		/// <summary>
		/// Finds the name of the non terminal.
		/// </summary>
		/// <returns>The non terminal name.</returns>
		/// <param name="str">String.</param>
		public static string FindNonTerminalName(string str)
		{
			string pattern = @"<([a-zA-Z0-9]|[\._\- ])+>";
			return findPatternIn (pattern, str);
		}

		/// <summary>
		/// Gets the first set name found in the given string
		/// </summary>
		/// <returns>The set name.</returns>
		/// <param name="str">String.</param>
		public static string FindSetName(string str)
		{
			string pattern = @"{[^{}]+}";
			return findPatternIn (pattern, str);
		}


		public static string FindPropertyName(string str)
		{
			//double double quotes escapes double quote in verbatim string.
			string pattern = @"""[^""']+"""; 
			return findPatternIn (pattern, str);
		}

		/// <summary>
		/// Gets the first terminal name that is delimited with single quotes
		/// </summary>
		/// <returns>The terminal name without the quotes</returns>
		/// <param name="str">String.</param>
		public static string FindTerminalName(string str)
		{
			string pattern = @"'([a-zA-Z0-9]|[._\-])+'";
			string matchResult =  findPatternIn (pattern, str);
			string name = string.Empty;

			//result still has single quotes. remove those

			if (matchResult.Length >= 2) //with the quotes, length is at least 2
			{
				name = matchResult.Substring (1, matchResult.Length - 2);
			}
			return name;
		}

		static string findPatternIn(string pattern, string str)
		{
			Regex regex = new Regex (pattern, RegexOptions.IgnoreCase);
			MatchCollection matches = regex.Matches (str);
			string result = string.Empty;
			if(matches.Count>0)
			{
				result = matches[0].ToString();
			}
			return result;
		}
	}
}

