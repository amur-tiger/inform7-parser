using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TigeR.Inform7.Ast.Nodes;
using TigeR.Inform7.Tokens;

namespace TigeR.Inform7.Ast
{
    public class Parser
    {
		private readonly Matcher matchAssignment = new Matcher("$var is $val.");
		private readonly Matcher matchInclude = new Matcher("Include $title by $author.");

		private readonly Matcher matchRelationShipRule = new Matcher("$relationshipName relates $lhs [(called $lhsName)] to $rhs [(called $rhsName)]");

		public Node Parse(IEnumerable<Token> stream)
		{
			if (stream == null)
			{
				throw new ArgumentNullException(nameof(stream));
			}

			var result = new StatementListNode();

			stream = FilterComments(stream);


			var headerParser = new HeaderParser();
			foreach (var node in headerParser.Parse(stream.GetEnumerator()))
			{
				result.Children.Add(node);
			}




			var sentences = SplitSentences(stream);


			// first, expect a header:
			// (version?) title "by" author.
			// a string description

			// then, general statements: definitions, rules...

			// inside rules: general rule statements (only the last statement may have a dot, all others must end on semicolon

			// matching may depend on previous user-defined rules: maybe make all matching user-definable (in contrast to defining new rules in code)
			// example: Inclusion relates a thing (called X) to a thing (called Y) when Y is part of X. -> One oven is a part of every stove.


			foreach (var sentence in sentences)
			{
				var includeMatches = matchInclude.Match(sentence);
				if (includeMatches.Count > 0)
				{
					var match = includeMatches.First();

					var title = new IdentifierNode();
					title.Token.AddRange(match["title"]);

					var author = new IdentifierNode();
					author.Token.AddRange(match["author"]);

					var inc = new IncludeNode();
					inc.Token.AddRange(match[0]);
					inc.Children.Add(title);
					inc.Children.Add(author);

					result.Children.Add(inc);

					continue;
				}

				var matches = matchAssignment.Match(sentence);
				if (matches.Count > 0)
				{
					var match = matches.First();

					var lhs = new IdentifierNode();
					lhs.Token.AddRange(match["var"]);

					var rhs = new IdentifierNode();
					rhs.Token.AddRange(match["val"]);

					var assign = new AssignmentNode();
					assign.Token.AddRange(match[1]);
					assign.Children.Add(lhs);
					assign.Children.Add(rhs);

					result.Children.Add(assign);

					continue;
				}

				var other = new NOTDONENODE();
				other.Token.AddRange(sentence);
				result.Children.Add(other);
			}



			return result;


			// start with all builders that can be top level
			// inside these, continue with builders that can occur there, and so on...
		}

		private IEnumerable<Token> FilterComments(IEnumerable<Token> tokens)
		{
			foreach (var token in tokens)
			{
				if (token.Type != TokenType.Comment)
				{
					yield return token;
				}
			}
		}

		private IEnumerable<List<Token>> SplitSentences(IEnumerable<Token> tokens)
		{
			var list = new List<Token>();
			foreach (var token in tokens)
			{
				if (token.Type == TokenType.Newline)
				{
					continue;
				}

				list.Add(token);

				if (token.Type == TokenType.Punctuation && (token.Surface == "." || token.Surface == ";"))
				{
					yield return list;
					list = new List<Token>();
				}
			}

			if (list.Count > 0)
			{
				yield return list;
			}
		}

		public string DumpNode(Node root)
		{
			void Recurse(StringBuilder sb, Node node, int level)
			{
				sb.Append("".PadRight(level * 2));
				sb.Append(node.GetType().Name.ToString());

				var str = node.ToString();
				if (str != null && str.Length > 0)
				{
					sb.Append(" >");
					sb.Append(str);
					sb.Append("<");
				}

				sb.Append("\n");

				foreach (var child in node.Children)
				{
					Recurse(sb, child, level + 1);
				}
			}

			var builder = new StringBuilder();
			Recurse(builder, root, 0);
			return builder.ToString();
		}

		private Node TitleBuilder(List<Token> tokens, ref int offset)
		{
			var title = StringBuilder(tokens, ref offset);
			if (title == null)
			{
				return null;
			}

			return null;
		}

		private StringExpressionNode StringBuilder(List<Token> tokens, ref int offset)
		{
			if (tokens[offset].Type != TokenType.QuotesStart)
			{
				return null;
			}

			for (int i = offset + 1; i < tokens.Count; i++)
			{
				if (tokens[i].Type == TokenType.QuotesEnd)
				{
					var result = new StringExpressionNode();
					// todo string vars as nodes
					result.Token.AddRange(tokens.Skip(offset).Take(i - offset));
					return result;
				}
			}

			throw new SyntaxException("Unclosed string", tokens[offset].Line, tokens[offset].Column);
		}
    }
}
