using System.Collections.Generic;

namespace GoldAddin
{
	/// <summary>
	/// Container object for an unordered set of non-repeating, case-insensitive strings
	/// </summary>
	public class AnyCaseStringSet
	{
		HashSet<string> normalizedStrings;

		/// <summary>
		/// Initializes a new instance of the <see cref="GoldAddin.AnyCaseStringSet"/> class.
		/// </summary>
		public AnyCaseStringSet ()
		{
			normalizedStrings = new HashSet<string> ();
		}

		/// <summary>
		/// Add the specified string to the set
		/// </summary>
		/// <param name="str">String.</param>
		public void Add(string str)
		{
			normalizedStrings.Add (str.ToUpper ());
		}

		/// <summary>
		/// Returns true if the given string is already in the set (ignoring case)
		/// </summary>
		/// <param name="str">String.</param>
		public bool Contains(string str)
		{
			return normalizedStrings.Contains (str.ToUpper ());
		}
	}
}

