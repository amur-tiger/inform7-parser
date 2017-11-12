using System;
using System.Collections.Generic;
using System.Text;

namespace TigeR.Inform7.Tokens
{
    public class Token : IEquatable<Token>
    {
		public String Surface { get; }
		public int Line { get; }
		public int Column { get; }

		public TokenType Type { get; }

		public Token(String surface, int line, int column, TokenType type)
		{
			Surface = surface;
			Line = line;
			Column = column;
			Type = type;
		}

		public bool Equals(Token other)
		{
			return other != null &&
				other.Surface == Surface &&
				other.Line == Line &&
				other.Column == Column &&
				other.Type == Type;
		}

		public override bool Equals(Object other)
		{
			return Equals(other as Token);
		}

		public override int GetHashCode()
		{
			var hash = 23;
			hash = hash * 31 + Surface.GetHashCode();
			hash = hash * 31 + Line.GetHashCode();
			hash = hash * 31 + Column.GetHashCode();
			hash = hash * 31 + Type.GetHashCode();
			return hash;
		}

		public override string ToString()
		{
			return $"Token[{Type}, Line={Line}, Column={Column}, Surface=\"{Surface}\"]";
		}
	}
}
