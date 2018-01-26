using System;
using System.Collections.Generic;
using System.Text;
using TigeR.Inform7.Tokens;

namespace TigeR.Inform7.Ast
{
	internal class Rule
	{
		internal class RuleGroup : Rule
		{
			public List<Rule> Rules { get; } = new List<Rule>();

			public RuleGroup() : base(TokenType.Unknown) { }

			public override string ToString()
			{
				var builder = new StringBuilder();
				builder.Append("RuleGroup[");

				foreach (var rule in Rules)
				{
					builder
						.Append(rule.ToString())
						.Append(", ");
				}

				builder.Length -= 2;
				builder.Append("]");

				return builder.ToString();
			}
		}

		public TokenType WantedType { get; }
		public String WantedSurface { get; }
		public bool Repeatable { get; }
		public String Name { get; }
		public TokenType ExceptType { get; }

		public Rule(TokenType tokenType) : this(tokenType, null, false, null, TokenType.Unknown) { }

		public Rule(TokenType tokenType, string surface) : this(tokenType, surface, false, null, TokenType.Unknown) { }

		public Rule(TokenType tokenType, string surface, string name) : this(tokenType, surface, false, name, TokenType.Unknown) { }

		public Rule(TokenType tokenType, bool repeat) : this(tokenType, null, repeat, null, TokenType.Unknown) { }

		public Rule(TokenType tokenType, bool repeat, string name) : this(tokenType, null, repeat, name, TokenType.Unknown) { }

		public Rule(TokenType tokenType, string surface, bool repeat, string name, TokenType exceptType)
		{
			WantedType = tokenType;
			WantedSurface = surface;
			Repeatable = repeat;
			Name = name;
			ExceptType = exceptType;
		}

		public static Rule Optional(params Rule[] rules)
		{
			var group = new RuleGroup();
			group.Rules.AddRange(rules);
			return group;
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

			if (WantedType == TokenType.Unknown && ExceptType != TokenType.Unknown)
			{
				builder
					.Append(", Not ")
					.Append(ExceptType);
			}

			builder.Append("]");

			return builder.ToString();
		}
	}
}
