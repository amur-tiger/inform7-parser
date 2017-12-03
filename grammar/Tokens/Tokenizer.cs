using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace TigeR.Inform7.Tokens
{
    public class Tokenizer
    {
		public IEnumerable<Token> Tokenize(String text)
		{
			if (text == null) throw new ArgumentNullException(nameof(text));

			var line = 0;
			var indentation = 0;
			var state = new State(text);
			while (state.Advance())
			{
				if (state.Line > line)
				{
					yield return new Token("", state.Line, 0, TokenType.Newline);
					line = state.Line;
				}

				if (Char.IsWhiteSpace(state.Character))
				{
					continue;
				}

				if (state.Indentation > indentation)
				{
					yield return new Token("", state.Line, 0, TokenType.Indent);
					indentation = state.Indentation;
				}
				else if (state.Indentation < indentation)
				{
					yield return new Token("", state.Line, 0, TokenType.Dedent);
					indentation = state.Indentation;
				}

				switch (state.Character)
				{
					case '[':
						foreach (var token in ReadComment(state))
						{
							yield return token;
						}
						break;
					case '"':
						foreach (var token in ReadString(state))
						{
							yield return token;
						}
						break;
					case '(':
						if (state.Peek == '-')
						{
							foreach (var token in ReadInform6Block(state))
							{
								yield return token;
							}
							break;
						}
						goto default;
					default:
						foreach (var token in ReadWord(state))
						{
							yield return token;
						}
						break;
				}
			}
		}

		private IEnumerable<Token> ReadWord(State state)
		{
			Debug.Assert(!Char.IsWhiteSpace(state.Character));

			if (!Char.IsLetterOrDigit(state.Character))
			{
				yield return new Token(new String(state.Character, 1), state.Line, state.Column, TokenType.Punctuation);
				yield break;
			}

			var startPosition = state.Position;
			var startLine = state.Line;
			var startColumn = state.Column;

			while (Char.IsLetterOrDigit(state.Peek) || state.Peek == '\'')
			{
				if (!state.Advance())
				{
					break;
				}
			}

			var surface = state.Text.Substring(startPosition, state.Position - startPosition + 1);

			yield return new Token(surface, startLine, startColumn, TokenType.Word);
		}

		private IEnumerable<Token> ReadComment(State state)
		{
			Debug.Assert(state.Character == '[');

			var startPosition = state.Position;
			var startLine = state.Line;
			var startColumn = state.Column;
			var commentStack = 1;

			while (state.Advance())
			{
				switch (state.Character)
				{
					case '[':
						commentStack += 1;
						break;
					case ']':
						commentStack -= 1;
						if (commentStack == 0)
						{
							yield return new Token(state.Text.Substring(startPosition, state.Position - startPosition + 1),
								startLine, startColumn, TokenType.Comment);
							yield break;
						}
						break;
				}
			}

			throw new SyntaxException("Unterminated comment", state.Line, state.Column);
		}

		private IEnumerable<Token> ReadString(State state)
		{
			Debug.Assert(state.Character == '"');

			yield return new Token("\"", state.Line, state.Column, TokenType.QuotesStart);

			var startPosition = state.Position + 1;
			int? startLine = null;
			int? startColumn = null;

			while (state.Advance())
			{
				if (startLine == null) startLine = state.Line;
				if (startColumn == null) startColumn = state.Column;

				switch (state.Character)
				{
					case '[':
						if (state.Position > startPosition)
						{
							yield return new Token(state.Text.Substring(startPosition, state.Position - startPosition), startLine.Value, startColumn.Value, TokenType.StringLiteral);
						}

						foreach (var token in ReadStringVariables(state))
						{
							yield return token;
						}

						Debug.Assert(state.Character == ']');
						startPosition = state.Position + 1;
						startLine = null;
						startColumn = null;

						break;
					case '"':
						if (state.Position > startPosition)
						{
							yield return new Token(state.Text.Substring(startPosition, state.Position - startPosition), startLine.Value, startColumn.Value, TokenType.StringLiteral);
						}

						yield return new Token("\"", state.Line, state.Column, TokenType.QuotesEnd);
						yield break;
				}
			}

			throw new SyntaxException("Unterminated string literal", state.Line, state.Column);
		}

		private IEnumerable<Token> ReadStringVariables(State state)
		{
			Debug.Assert(state.Character == '[');

			yield return new Token("[", state.Line, state.Column, TokenType.StringVariableStart);

			var startPosition = state.Position + 1;
			int? startLine = null;
			int? startColumn = null;

			while (state.Advance())
			{
				if (startLine == null) startLine = state.Line;
				if (startColumn == null) startColumn = state.Column;

				if (Char.IsWhiteSpace(state.Character))
				{
					var diff = state.Position - startPosition;
					if (diff > 0)
					{
						yield return new Token(state.Text.Substring(startPosition, state.Position - startPosition), startLine.Value, startColumn.Value, TokenType.Word);
					}

					startPosition = state.Position + 1;
					startLine = null;
					startColumn = null;
				}
				else if (state.Character == ']')
				{
					var diff = state.Position - startPosition;
					if (diff > 0)
					{
						yield return new Token(state.Text.Substring(startPosition, state.Position - startPosition), startLine.Value, startColumn.Value, TokenType.Word);
					}

					yield return new Token("]", state.Line, state.Column, TokenType.StringVariableEnd);
					yield break;
				}
			}

			throw new SyntaxException("Unterminated string literal", state.Line, state.Column);
		}

		private IEnumerable<Token> ReadInform6Block(State state)
		{
			Debug.Assert(state.Character == '(' && state.Peek == '-');

			var startPosition = state.Position;
			var startLine = state.Line;
			var startColumn = state.Column;

			while (state.Advance())
			{
				if (state.Character == '-' && state.Peek == ')')
				{
					state.Advance();
					yield return new Token(state.Text.Substring(startPosition + 2, state.Position - startPosition - 3).Trim(),
						startLine, startColumn, TokenType.Inform6);
					yield break;
				}
			}

			throw new SyntaxException("Unterminated Inform6 Block", state.Line, state.Column);
		}
	}
}
