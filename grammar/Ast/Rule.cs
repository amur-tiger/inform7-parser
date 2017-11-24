using System;
using System.Collections.Generic;
using System.Text;
using TigeR.Inform7.Tokens;

namespace TigeR.Inform7.Ast
{
	internal class Rule
	{
		public TokenType WantedType { get; }
		public String WantedSurface { get; }
		public bool Repeatable { get; }
		public String Name { get; }

		public Rule(TokenType tokenType) : this(tokenType, null, false, null) { }

		public Rule(TokenType tokenType, string surface) : this(tokenType, surface, false, null) { }

		public Rule(TokenType tokenType, string surface, string name) : this(tokenType, surface, false, name) { }

		public Rule(TokenType tokenType, bool repeat) : this(tokenType, null, repeat, null) { }

		public Rule(TokenType tokenType, bool repeat, string name) : this(tokenType, null, repeat, name) { }

		public Rule(TokenType tokenType, string surface, bool repeat, string name)
		{
			WantedType = tokenType;
			WantedSurface = surface;
			Repeatable = repeat;
			Name = name;
		}

		public override string ToString()
		{
			var builder = new StringBuilder();
			builder
				.Append("Rule[")
				.Append(WantedType);

			if (WantedSurface != null)
			{
				builder
					.Append(", \"")
					.Append(WantedSurface)
					.Append("\"");
			}

			if (Repeatable)
			{
				builder.Append(", Repeatable");
			}

			if (Name != null)
			{
				builder
					.Append(", Name=\"")
					.Append(Name)
					.Append("\"");
			}

			builder.Append("]");

			return builder.ToString();
		}
	}
}
