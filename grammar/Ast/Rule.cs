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

		public Rule(TokenType tokenType) : this(tokenType, null) { }

		public Rule(TokenType tokenType, String surface) : this(tokenType, surface, false) { }

		public Rule(TokenType tokenType, bool repeat) : this(tokenType, null, repeat) { }

		public Rule(TokenType tokenType, String surface, bool repeat)
		{
			WantedType = tokenType;
			WantedSurface = surface;
			Repeatable = repeat;
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

			builder.Append("]");

			return builder.ToString();
		}
	}
}
