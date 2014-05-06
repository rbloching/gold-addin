using System;
using System.Collections.Generic;
using bsn.GoldParser.Grammar;
using bsn.GoldParser.Parser;

namespace GoldAddin
{
	/// <summary>
	/// Token utility functions
	/// </summary>
	public static class TokenUtil
	{
		/// <summary>
		/// Determines if is new line the specified token.
		/// </summary>
		/// <returns><c>true</c> if is new line the specified token; otherwise, <c>false</c>.</returns>
		/// <param name="token">Token.</param>
		public static bool IsNewLine(Token token)
		{
			return token != null && token.NameIs ("Newline");
		}

		/// <summary>
		/// True if the given token represents the End Of File
		/// </summary>
		/// <returns><c>true</c> if is end the specified token; otherwise, <c>false</c>.</returns>
		/// <param name="token">Token.</param>
		public static bool IsEnd(Token token)
		{
			return token!=null && (token.Symbol.Kind == SymbolKind.End);
		}

		public static bool IsTerminalLiteral(Token token)
		{
			return token != null && token.Symbol.Name == "Terminal" && token.Text.StartsWith ("\'", StringComparison.Ordinal);
		}

		/// <summary>
		/// Determines if the specified token is a grammar definition (property, set, terminal, or nonterminal)
		/// </summary>
		/// <returns><c>true</c> if is user definition token the specified token; otherwise, <c>false</c>.</returns>
		/// <param name="token">Token.</param>
		public static bool IsUserDefinitionToken(Token token)
		{
			return token != null &&
				(token.Symbol.Name == "ParameterName" ||
				 token.Symbol.Name == "SetName" ||
				 (token.Symbol.Name == "Terminal" && !IsTerminalLiteral(token))||
				 token.Symbol.Name == "Nonterminal");
		}

		/// <summary>
		/// Determines if the specified token is noise
		/// </summary>
		/// <returns><c>true</c> if the token is noise (whitespace, comment, etc); otherwise, <c>false</c>.</returns>
		/// <param name="token">Token to examine</param>		
		public static bool IsNoise(Token token)
		{
			SymbolKind kind = token.Symbol.Kind;
			return kind != SymbolKind.Error && kind != SymbolKind.Terminal && kind != SymbolKind.End;
		}



		static readonly Dictionary<string, DefinitionType> symbolTypeMap;
		static TokenUtil()
		{
			symbolTypeMap = new Dictionary<string, DefinitionType>();
			symbolTypeMap.Add("ParameterName", DefinitionType.Property);
			symbolTypeMap.Add("Nonterminal", DefinitionType.NonTerminal);
			symbolTypeMap.Add("Terminal", DefinitionType.Terminal);
			symbolTypeMap.Add("SetName", DefinitionType.SetName);
		}

		/// <summary>
		/// Gets the grammar definition type of the given token
		/// </summary>
		/// <returns>The definition type.</returns>
		/// <param name="token">Token.</param>
		public static DefinitionType GetDefinitionType(Token token)
		{
			DefinitionType symbolType;
			bool there = symbolTypeMap.TryGetValue(token.Symbol.Name, out symbolType);
			if (!there)
				symbolType = DefinitionType.None;
			return symbolType;
		}


	}
}

