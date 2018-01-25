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

		private Matcher SetupMatcher(string pattern)
		{
			return new Matcher(pattern);
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
		public void RejectNullRuleArgument()
		{
			Check.ThatCode(() => SetupMatcher((Rule)null))
				.Throws<ArgumentNullException>();
		}

		[Fact]
		public void RejectNullPatternArgument()
		{
			Check.ThatCode(() => SetupMatcher((string)null))
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
		public void DoNotMatchEmptyTokens()
		{
			var tokens = new List<Token>();
			var sut = SetupMatcher(new Rule(TokenType.Word));

			var result = sut.Match(tokens);

			Check.That(result).IsEmpty();
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

		[Fact]
		public void SkipOptionalRules()
		{
			var tokens = SetupTokens("The thing is round.");
			var match = Match(tokens, 2, 1, 1, 1);
			match.SetName(0, "var");
			match.SetName(2, "value");

			var sut = SetupMatcher(
				new Rule(TokenType.Word, true, "var"),
				Rule.Optional(
					new Rule(TokenType.Punctuation, "("),
					new Rule(TokenType.Word, "called"),
					new Rule(TokenType.Word, true, "name"),
					new Rule(TokenType.Punctuation, ")")
				),
				new Rule(TokenType.Word, "is"),
				new Rule(TokenType.Word, true, "value"),
				new Rule(TokenType.Punctuation, ".")
			);

			var result = sut.Match(tokens);

			Check.That(result).HasOneElementOnly().Which.IsEqualTo(match);
		}


		[Fact]
		public void IncludeOptionalRules()
		{
			var tokens = SetupTokens("The thing (called X) is round.");
			var match = Match(tokens, 2, 1, 1, 1, 1, 1, 1, 1);
			match.SetName(0, "var");
			match.SetName(3, "name");
			match.SetName(6, "value");

			var sut = SetupMatcher(
				new Rule(TokenType.Word, true, "var"),
				Rule.Optional(
					new Rule(TokenType.Punctuation, "("),
					new Rule(TokenType.Word, "called"),
					new Rule(TokenType.Word, true, "name"),
					new Rule(TokenType.Punctuation, ")")
				),
				new Rule(TokenType.Word, "is"),
				new Rule(TokenType.Word, true, "value"),
				new Rule(TokenType.Punctuation, ".")
			);

			var result = sut.Match(tokens);

			Check.That(result).HasOneElementOnly().Which.IsEqualTo(match);
		}

		[Fact]
		public void MatchWithPatternString()
		{
			var tokens = SetupTokens("The variable is twelve.");
			var match = Match(tokens, 2, 1, 1, 1);
			match.SetName(0, "var");
			match.SetName(2, "value");

			var sut = SetupMatcher("$var is $value.");

			var result = sut.Match(tokens);

			Check.That(result).HasOneElementOnly().Which.IsEqualTo(match);
		}

		[Fact]
		public void SkipOptionalInPatternString()
		{
			var tokens = SetupTokens("The thing is round.");
			var match = Match(tokens, 2, 1, 1, 1);
			match.SetName(0, "var");
			match.SetName(2, "value");

			var sut = SetupMatcher("$var [(called $name)] is $value.");

			var result = sut.Match(tokens);

			Check.That(result).HasOneElementOnly().Which.IsEqualTo(match);
		}

		[Fact]
		public void IncludeOptionalInPatternString()
		{
			var tokens = SetupTokens("The thing (called X) is round.");
			var match = Match(tokens, 2, 1, 1, 1, 1, 1, 1, 1);
			match.SetName(0, "var");
			match.SetName(3, "name");
			match.SetName(6, "value");

			var sut = SetupMatcher("$var [(called $name)] is $value.");

			var result = sut.Match(tokens);

			Check.That(result).HasOneElementOnly().Which.IsEqualTo(match);
		}

		[Fact]
		public void MatchTooManyTokens()
		{
			var tokens = SetupTokens("The thing is round. The other thing is angular.");
			var match = Match(tokens, 2, 1, 1, 1);
			match.SetName(0, "var");
			match.SetName(2, "value");

			var sut = SetupMatcher("$var is $value.");

			var result = sut.Match(tokens);

			Check.That(result).HasOneElementOnly().Which.IsEqualTo(match);
		}


		[Fact]
		public void MatchCaseInvariant()
		{
			var tokens = SetupTokens("The thing IS round.");
			var match = Match(tokens, 2, 1, 1, 1);
			match.SetName(0, "var");
			match.SetName(2, "value");

			var sut = SetupMatcher("$var is $value.");

			var result = sut.Match(tokens);

			Check.That(result).HasOneElementOnly().Which.IsEqualTo(match);
		}
		
		// todo: invalid patterns
		// todo: pattern escape char
    }
}
