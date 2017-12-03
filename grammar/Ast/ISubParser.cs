using System;
using System.Collections.Generic;
using System.Text;
using TigeR.Inform7.Ast.Nodes;
using TigeR.Inform7.Tokens;

namespace TigeR.Inform7.Ast
{
    internal interface ISubParser
    {
		IEnumerable<Node> Parse(IEnumerator<Token> tokens);
    }
}
