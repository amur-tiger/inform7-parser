using NFluent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace TigeR.Inform7.Tokens
{
	public class TokenizerTest
	{
		private Tokenizer sut;

		public TokenizerTest()
		{
			sut = new Tokenizer();
		}

		[Fact]
		public void RejectNull()
		{
			Check.ThatCode(() => sut.Tokenize(null).ToList())
				.Throws<ArgumentNullException>();
		}

		[Fact]
		public void HandleEmptyString()
		{
			var result = sut.Tokenize("").ToList();

			Check.That(result).IsEmpty();
		}

		[Fact]
		public void TokenizeSentence()
		{
			var result = sut.Tokenize("This is a sentence.").ToList();

			Check.That(result).ContainsExactly(
				new Token("This", 0, 0, TokenType.Word),
				new Token("is", 0, 5, TokenType.Word),
				new Token("a", 0, 8, TokenType.Word),
				new Token("sentence", 0, 10, TokenType.Word),
				new Token(".", 0, 18, TokenType.Punctuation)
			);
		}

		[Fact]
		public void DetectIndentation()
		{
			var result = sut.Tokenize("Block:\n\tThis is indented;").ToList();

			Check.That(result).ContainsExactly(
				new Token("Block", 0, 0, TokenType.Word),
				new Token(":", 0, 5, TokenType.Punctuation),
				new Token("", 1, 0, TokenType.Newline),
				new Token("", 1, 0, TokenType.Indent),
				new Token("This", 1, 4, TokenType.Word),
				new Token("is", 1, 9, TokenType.Word),
				new Token("indented", 1, 12, TokenType.Word),
				new Token(";", 1, 20, TokenType.Punctuation)
			);
		}

		[Fact]
		public void DetectDedentation()
		{
			var result = sut.Tokenize("A:\n\tI;\n\nB.").ToList();

			Check.That(result).ContainsExactly(
				new Token("A", 0, 0, TokenType.Word),
				new Token(":", 0, 1, TokenType.Punctuation),
				new Token("", 1, 0, TokenType.Newline),
				new Token("", 1, 0, TokenType.Indent),
				new Token("I", 1, 4, TokenType.Word),
				new Token(";", 1, 5, TokenType.Punctuation),
				new Token("", 2, 0, TokenType.Newline),
				new Token("", 3, 0, TokenType.Newline),
				new Token("", 3, 0, TokenType.Dedent),
				new Token("B", 3, 0, TokenType.Word),
				new Token(".", 3, 1, TokenType.Punctuation)
			);
		}

		[Fact]
		public void DetectNestedIndent()
		{
			var result = sut.Tokenize("A:\n\tI:\n\t\tI2;\n").ToList();

			Check.That(result).ContainsExactly(
				new Token("A", 0, 0, TokenType.Word),
				new Token(":", 0, 1, TokenType.Punctuation),
				new Token("", 1, 0, TokenType.Newline),
				new Token("", 1, 0, TokenType.Indent),
				new Token("I", 1, 4, TokenType.Word),
				new Token(":", 1, 5, TokenType.Punctuation),
				new Token("", 2, 0, TokenType.Newline),
				new Token("", 2, 0, TokenType.Indent),
				new Token("I2", 2, 8, TokenType.Word),
				new Token(";", 2, 10, TokenType.Punctuation)
			);
		}

		[Fact]
		public void DetectString()
		{
			var result = sut.Tokenize("\"A string.\"").ToList();

			Check.That(result).ContainsExactly(
				new Token("\"", 0, 0, TokenType.QuotesStart),
				new Token("A string.", 0, 1, TokenType.StringLiteral),
				new Token("\"", 0, 10, TokenType.QuotesEnd)
			);
		}

		[Fact]
		public void DetectStringVariables()
		{
			var result = sut.Tokenize("\"Insert [a variable].\"").ToList();

			Check.That(result).ContainsExactly(
				new Token("\"", 0, 0, TokenType.QuotesStart),
				new Token("Insert ", 0, 1, TokenType.StringLiteral),
				new Token("[", 0, 8, TokenType.StringVariableStart),
				new Token("a", 0, 9, TokenType.Word),
				new Token("variable", 0, 11, TokenType.Word),
				new Token("]", 0, 19, TokenType.StringVariableEnd),
				new Token(".", 0, 20, TokenType.StringLiteral),
				new Token("\"", 0, 21, TokenType.QuotesEnd)
			);
		}

		[Fact]
		public void DoNotSkipWhitespaceAroundStringVariables()
		{
			var result = sut.Tokenize("\"[Our] [Home]\"").ToList();

			Check.That(result).ContainsExactly(
				new Token("\"", 0, 0, TokenType.QuotesStart),
				new Token("[", 0, 1, TokenType.StringVariableStart),
				new Token("Our", 0, 2, TokenType.Word),
				new Token("]", 0, 5, TokenType.StringVariableEnd),
				new Token(" ", 0, 6, TokenType.StringLiteral),
				new Token("[", 0, 7, TokenType.StringVariableStart),
				new Token("Home", 0, 8, TokenType.Word),
				new Token("]", 0, 12, TokenType.StringVariableEnd),
				new Token("\"", 0, 13, TokenType.QuotesEnd)
			);
		}

		[Fact]
		public void DetectComments()
		{
			var result = sut.Tokenize("[This is a comment]").ToList();

			Check.That(result).ContainsExactly(
				new Token("[This is a comment]", 0, 0, TokenType.Comment)
			);
		}

		[Fact]
		public void DetectNestedComments()
		{
			var result = sut.Tokenize("[This is a [nested] comment]").ToList();

			Check.That(result).ContainsExactly(
				new Token("[This is a [nested] comment]", 0, 0, TokenType.Comment)
			);
		}

		[Fact]
		public void ParseMixedIndentation()
		{
			var result = sut.Tokenize("B:\n  \tI;\n    J;\n\tK;").ToList();

			Check.That(result).ContainsExactly(
				new Token("B", 0, 0, TokenType.Word),
				new Token(":", 0, 1, TokenType.Punctuation),
				new Token("", 1, 0, TokenType.Newline),
				new Token("", 1, 0, TokenType.Indent),
				new Token("I", 1, 4, TokenType.Word),
				new Token(";", 1, 5, TokenType.Punctuation),
				new Token("", 2, 0, TokenType.Newline),
				new Token("J", 2, 4, TokenType.Word),
				new Token(";", 2, 5, TokenType.Punctuation),
				new Token("", 3, 0, TokenType.Newline),
				new Token("K", 3, 4, TokenType.Word),
				new Token(";", 3, 5, TokenType.Punctuation)
			);
		}

		[Fact]
		public void ParseIrregularIndentation()
		{
			var result = sut.Tokenize("B:\n   A;\n\tB;\n \tC;").ToList();

			Check.That(result).ContainsExactly(
				new Token("B", 0, 0, TokenType.Word),
				new Token(":", 0, 1, TokenType.Punctuation),
				new Token("", 1, 0, TokenType.Newline),
				new Token("", 1, 0, TokenType.Indent),
				new Token("A", 1, 3, TokenType.Word),
				new Token(";", 1, 4, TokenType.Punctuation),
				new Token("", 2, 0, TokenType.Newline),
				new Token("B", 2, 4, TokenType.Word),
				new Token(";", 2, 5, TokenType.Punctuation),
				new Token("", 3, 0, TokenType.Newline),
				new Token("C", 3, 4, TokenType.Word),
				new Token(";", 3, 5, TokenType.Punctuation)
			);
		}

		[Fact]
		public void ParseExcessiveWhitespace()
		{
			var result = sut.Tokenize("This    is  a sentence.").ToList();

			Check.That(result).ContainsExactly(
				new Token("This", 0, 0, TokenType.Word),
				new Token("is", 0, 8, TokenType.Word),
				new Token("a", 0, 12, TokenType.Word),
				new Token("sentence", 0, 14, TokenType.Word),
				new Token(".", 0, 22, TokenType.Punctuation)
			);
		}

		// todo error input (tokenizer should put best effort)
		// badly formatted input (indents, newlines)
    }
}
