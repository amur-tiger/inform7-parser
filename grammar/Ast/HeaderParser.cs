using System;
using System.Collections.Generic;
using System.Text;
using TigeR.Inform7.Ast.Nodes;
using TigeR.Inform7.Tokens;

namespace TigeR.Inform7.Ast
{
	internal class HeaderParser : ISubParser
	{
		private readonly Matcher introMatcher = new Matcher("[Version $version of] '$title' [(for $platform only)] by $author [begins here][.]");
		private readonly Matcher modestyMatcher = new Matcher("Use authorial modesty.");

		public IEnumerable<Node> Parse(IEnumerator<Token> tokens)
		{
			var simpleIntroMatcher = new Matcher(
				new Rule(TokenType.QuotesStart),
				new Rule(TokenType.StringLiteral, false, "title"),
				new Rule(TokenType.QuotesEnd),
				new Rule(TokenType.Word, "by"),
				new Rule(TokenType.Word, true, "author"),
				new Rule(TokenType.Newline)
			);

			var line = new List<Token>();
			while (tokens.MoveNext())
			{
				line.Add(tokens.Current);
				if (tokens.Current.Type == TokenType.Newline)
				{
					break;
				}
			}

			var matches = simpleIntroMatcher.Match(line);
			if (matches.Count > 0)
			{
				var intro = new TitleStatement();

				var title = new StringExpressionNode();
				title.Token.AddRange(matches[0][1]);

				var author = new NOTDONENODE();
				author.Token.AddRange(matches[0][4]);

				intro.Children.Add(title);
				intro.Children.Add(author);

				yield return intro;
			}


			// first intro
			// then description (before anything else)
			// may be multiple descs
			// authorial modesty

			// return

			
		}
	}
}
