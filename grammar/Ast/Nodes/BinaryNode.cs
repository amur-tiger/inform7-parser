using System;
using System.Collections.Generic;
using System.Text;

namespace TigeR.Inform7.Ast.Nodes
{
    public class BinaryNode : StatementNode
	{
		public Node LHS => Children[0];
		public Node RHS => Children[1];
	}
}
