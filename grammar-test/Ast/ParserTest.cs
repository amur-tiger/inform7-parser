using NFluent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TigeR.Inform7.Ast.Nodes;
using TigeR.Inform7.Tokens;
using Xunit;

namespace TigeR.Inform7.Ast
{
	public class ParserTest
	{
		private Parser sut;

		public ParserTest()
		{
			sut = new Parser();
		}

		private IEnumerable<Token> Setup(string text)
		{
			var tokenizer = new Tokenizer();
			return tokenizer.Tokenize(text);
		}

		[Fact]
		public void RejectNull()
		{
			Check.ThatCode(() => sut.Parse(null)).Throws<ArgumentNullException>();
		}

		[Fact]
		public void ParseAssignment()
		{
			var tokens = Setup("The number is twelve.");

			var result = sut.Parse(tokens);

			Check.That(result).IsInstanceOf<StatementListNode>();

			Check.That(result.Children[0]).IsInstanceOf<AssignmentNode>();
			var assign = result.Children[0] as AssignmentNode;
			Check.That(assign.Surface).IsEqualTo("is"); // todo: necessary? maybe either skip this, or make surface over children as well

			Check.That(assign.LHS).IsInstanceOf<IdentifierNode>();
			var ident = assign.LHS;

			Check.That(ident.Surface).IsEqualTo("The number");

			Check.That(assign.RHS).IsInstanceOf<IdentifierNode>();
			var val = assign.RHS;

			Check.That(val.Surface).IsEqualTo("twelve");
		}

		/*[Fact]
		public void ParseDefaultValues()
		{
			var tokens = Setup("It is usually scenery.");

		    var result = sut.Parse(tokens);

			Check.That(result).IsInstanceOf<StatementListNode>();

			Check.That(result.Children[0]).IsInstanceOf<DefaultPropertyValueNode>();
			var defprop = result.Children[0] as DefaultPropertyValueNode();
			Check.That(defprop.Surface).IsEqualTo("is usually");

			Check.That()
		}*/
    }
}
