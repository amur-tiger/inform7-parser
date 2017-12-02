using System;
using System.Collections.Generic;
using System.Text;

namespace TigeR.Inform7.Tokens
{
	public enum TokenType
	{
		Unknown,
		Word,
		Punctuation,
		Newline,
		Indent,
		Dedent,
		QuotesStart,
		QuotesEnd,
		StringLiteral,
		StringVariableStart,
		StringVariableEnd,
		Comment,
		Inform6
	}
}
