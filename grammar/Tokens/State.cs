using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TigeR.Inform7.Tokens
{
	internal class State
	{
		private int indentationSpaces = 0;

		/// <summary>
		/// The text to save the state for.
		/// </summary>
		public string Text { get; }

		/// <summary>
		/// The current cursor position on the text.
		/// </summary>
		public int Position { get; private set; } = -1;

		/// <summary>
		/// The current line the cursor position is on in the text. Starts at zero.
		/// </summary>
		public int Line { get; private set; } = 0;

		/// <summary>
		/// The current text column the cursor position is on in the text. Starts at zero.
		/// </summary>
		public int Column { get; private set; } = -1;

		/// <summary>
		/// The current indentation level of the line at the cursor position.
		/// </summary>
		public int Indentation => (indentationSpaces + 3) / 4;

		/// <summary>
		/// The current character the cursor position points to. Returns the zero-character on empty texts.
		/// </summary>
		public char Character => Position < 0 || Position >= Text.Length ? '\0' : Text[Position];

		/// <summary>
		/// The next character the cursor position will point to. Returns the zero-character if no characters follow.
		/// </summary>
		public char Peek => Position >= Text.Length - 1 ? '\0' : Text[Position + 1];

		/// <summary>
		/// Creates a new state with the given text.
		/// </summary>
		/// <param name="text">The text to save.</param>
		public State(string text)
		{
			Text = text ?? throw new ArgumentNullException(nameof(text));
		}

		/// <summary>
		/// Resets the cursor position back to the start of the text.
		/// </summary>
		public void Reset()
		{
			Position = -1;
			Line = 0;
			Column = -1;
			indentationSpaces = 0;
		}

		/// <summary>
		/// Advances the cursor position one character, if there are more characters in the text. Returns
		/// a bool on whether the cursor position was actually moved.
		/// </summary>
		/// <returns></returns>
		public bool Advance()
		{
			if (Position < Text.Length - 1)
			{
				Column += 1;
				if (Character == '\t')
				{
					Column += 4 - Column % 4;
				}

				if (Character == '\n' || Character == '\r' && Peek != '\n')
				{
					Line += 1;
					Column = 0;
					indentationSpaces = 0;
					
					for (int i = Position + 1; i < Text.Length; i++)
					{
						if (Text[i] == '\n' || Text[i] == '\r' || !Char.IsWhiteSpace(Text[i]))
						{
							break;
						}
				
						if (Text[i] == '\t')
						{
							indentationSpaces += 4 - indentationSpaces % 4;
						}
						else
						{
							indentationSpaces += 1;
						}
					}
				}

				Position += 1;
				return true;
			}

			return false;
		}
	}
}
