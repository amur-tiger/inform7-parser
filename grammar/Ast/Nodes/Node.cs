using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TigeR.Inform7.Tokens;

namespace TigeR.Inform7.Ast.Nodes
{
    public abstract class Node
    {
		public List<Node> Children { get; } = new List<Node>();
		public List<Token> Token { get; } = new List<Token>();

		public string Surface => String.Join(' ', Token.Select(token => token.Surface));

		public Node(params Node[] nodes)
		{
			Children.AddRange(nodes);
		}

		public override string ToString()
		{
			return Surface;
		}
	}
}
