using NFluent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TigeR.Inform7.Tokens;
using Xunit;

namespace TigeR.Inform7.Ast
{
    public class MatcherTest
    {
		private List<Token> SetupTokens(string text)
		{
			var tokenizer = new Tokenizer();
			return tokenizer.Tokenize(text).ToList();
		}

		private Matcher SetupMatcher(Rule firstRule, params Rule[] otherRules)
		{
			return new Matcher(firstRule, otherRules);
		}

		private Match Match(List<Token> tokens, params int[] length)
		{
			var result = new Match();
			var offset = 0;
			foreach (var len in length)
			{
				result.Add(tokens.Skip(offset).Take(len).ToList());
				offset += len;
			}

			return result;
		}

		[Fact]
		public void RejectNullArgument()
		{
			Check.ThatCode(() => SetupMatcher(null))
				.Throws<ArgumentNullException>();
		}

		[Fact]
		public void RejectNullRule()
		{
			Check.ThatCode(() => SetupMatcher(new Rule(TokenType.Word), null))
				.Throws<ArgumentNullException>();
		}

		[Fact]
		public void RejectNullTokens()
		{
			Check.ThatCode(() => SetupMatcher(new Rule(TokenType.Word)).Match(null))
				.Throws<ArgumentNullException>();
		}

		[Fact]
		public void RejectEmptyTokens()
		{
			Check.ThatCode(() => SetupMatcher(new Rule(TokenType.Word)).Match(new List<Token>()))
				.Throws<ArgumentException>();
		}

		[Fact]
		public void MatchSingle()
		{
			var tokens = SetupTokens("Word");
			var sut = SetupMatcher(new Rule(TokenType.Word, "Word"));

			var result = sut.Match(tokens);

			Check.That(result).HasOneElementOnly().Which.IsEqualTo(Match(tokens, 1));
		}

		[Fact]
		public void MatchMultiple()
		{
			var tokens = SetupTokens("A sentence.");
			var sut = SetupMatcher(
				new Rule(TokenType.Word, "A"),
				new Rule(TokenType.Word),
				new Rule(TokenType.Punctuation));

			var result = sut.Match(tokens);

			Check.That(result).HasOneElementOnly().Which.IsEqualTo(Match(tokens, 1, 1, 1));
		}

		[Fact]
		public void MatchRepeatable()
		{
			var tokens = SetupTokens("A sentence.");
			var sut = SetupMatcher(
				new Rule(TokenType.Word, true),
				new Rule(TokenType.Punctuation));

			var result = sut.Match(tokens);

			Check.That(result).HasOneElementOnly().Which.IsEqualTo(Match(tokens, 2, 1));
		}

		[Fact]
		public void MatchAllPossible()
		{
			var tokens = SetupTokens("Apples and Oranges and Stuff.");
			var sut = SetupMatcher(
				new Rule(TokenType.Word, true),
				new Rule(TokenType.Word, "and"),
				new Rule(TokenType.Word, true),
				new Rule(TokenType.Punctuation));

			var result = sut.Match(tokens);
			
			Check.That(result).Contains(
				Match(tokens, 1, 1, 3, 1),
				Match(tokens, 3, 1, 1, 1)
			);
		}

		[Fact]
		public void DontMatchNotMatching()
		{
			var tokens = SetupTokens("A sentence.");
			var sut = SetupMatcher(
				new Rule(TokenType.Word, true),
				new Rule(TokenType.Word, "by"),
				new Rule(TokenType.Word, true),
				new Rule(TokenType.Punctuation));

			var result = sut.Match(tokens);

			Check.That(result).IsEmpty();
		}

		[Fact]
		public void AssignNamedMatches()
		{
			var tokens = SetupTokens("The variable is twelve.");
			var match = Match(tokens, 2, 1, 1, 1);
			match.SetName(0, "var");
			match.SetName(2, "value");

			var sut = SetupMatcher(
				new Rule(TokenType.Word, true, "var"),
				new Rule(TokenType.Word, "is"),
				new Rule(TokenType.Word, true, "value"),
				new Rule(TokenType.Punctuation));

			var result = sut.Match(tokens);

			Check.That(result).HasOneElementOnly().Which.IsEqualTo(match);
		}
    }
}
