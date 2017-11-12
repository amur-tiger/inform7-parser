using System;
using System.Collections.Generic;
using System.Text;

namespace TigeR.Inform7
{
	public class SyntaxException : Exception
	{
		public int Line { get; }
		public int Column { get; }

		public SyntaxException(string message, int line, int column) : base(message)
		{
			Line = line;
			Column = column;
		}

		public SyntaxException(string message, int line, int column, Exception inner) : base(message, inner)
		{
			Line = line;
			Column = column;
		}
    }
}
