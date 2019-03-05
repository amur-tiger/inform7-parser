using System;
using System.Collections.Generic;
using System.Text;
using TigeR.Inform7.Ast.Nodes;
using TigeR.Inform7.Tokens;

namespace TigeR.Inform7.Ast
{
	internal class HeaderParser : ISubParser
	{
		private readonly Matcher titleMatcher = new Matcher("[Version $version of] '$title' [(for $platform only)] by $author [begins here][.]");
		private readonly Matcher modestyMatcher = new Matcher("Use authorial modesty.");

		public IEnumerable<Node> Parse(IEnumerator<Token> tokens)
		{
			var line = new List<Token>();
			while (tokens.MoveNext())
			{
				line.Add(tokens.Current);
				if (tokens.Current.Type == TokenType.Newline)
				{
					break;
				}
			}

			var matches = titleMatcher.Match(line);
			if (matches.Count > 0)
			{
				var match = matches[0];

				var title = new TitleStatement();

				var titleString = new StringExpressionNode();
				titleString.Token.AddRange(match["title"]);

				var author = new NOTDONENODE();
				author.Token.AddRange(match["author"]);

				title.Children.Add(titleString);
				title.Children.Add(author);

				yield return title;
			}


			// first intro
			// then description (before anything else)
			// may be multiple descs
			// authorial modesty

			// return


		}
	}
}
