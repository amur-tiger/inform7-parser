using NFluent;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace TigeR.Inform7.Tokens
{
    public class StateTest
    {
		private State Setup(string text)
		{
			return new State(text);
		}

		[Fact]
		public void RejectNullArgument()
		{
			Check.ThatCode(() => new State(null)).Throws<ArgumentNullException>();
		}

		[Fact]
		public void InitializeWithSaneStartParameters()
		{
			var sut = Setup("A text.");

			Check.That(sut.Text).IsEqualTo("A text.");
			Check.That(sut.Position).IsEqualTo(-1);
			Check.That(sut.Character).IsEqualTo('\0');
			Check.That(sut.Peek).IsEqualTo('A');
			Check.That(sut.Column).IsEqualTo(-1);
			Check.That(sut.Line).IsEqualTo(0);
			Check.That(sut.Indentation).IsEqualTo(0);
		}

		[Fact]
		public void AdvanceShouldAdvanceCursor()
		{
			var sut = Setup("A text.");

			Check.That(sut.Advance()).IsEqualTo(true);
			Check.That(sut.Position).IsEqualTo(0);
			Check.That(sut.Character).IsEqualTo('A');
			Check.That(sut.Peek).IsEqualTo(' ');
		}

		[Fact]
		public void ColumnAndLineShouldBeTracked()
		{
			var sut = Setup("A.\nB.\nC.");

			Check.That(sut.Advance()).IsEqualTo(true);
			Check.That(sut.Position).IsEqualTo(0);
			Check.That(sut.Column).IsEqualTo(0);
			Check.That(sut.Line).IsEqualTo(0);

			Check.That(sut.Advance()).IsEqualTo(true);
			Check.That(sut.Position).IsEqualTo(1);
			Check.That(sut.Column).IsEqualTo(1);
			Check.That(sut.Line).IsEqualTo(0);

			Check.That(sut.Advance()).IsEqualTo(true);
			Check.That(sut.Position).IsEqualTo(2);
			Check.That(sut.Column).IsEqualTo(2);
			Check.That(sut.Line).IsEqualTo(0);

			Check.That(sut.Advance()).IsEqualTo(true);
			Check.That(sut.Position).IsEqualTo(3);
			Check.That(sut.Column).IsEqualTo(0);
			Check.That(sut.Line).IsEqualTo(1);

			Check.That(sut.Advance()).IsEqualTo(true);
			Check.That(sut.Position).IsEqualTo(4);
			Check.That(sut.Column).IsEqualTo(1);
			Check.That(sut.Line).IsEqualTo(1);

			Check.That(sut.Advance()).IsEqualTo(true);
			Check.That(sut.Position).IsEqualTo(5);
			Check.That(sut.Column).IsEqualTo(2);
			Check.That(sut.Line).IsEqualTo(1);

			Check.That(sut.Advance()).IsEqualTo(true);
			Check.That(sut.Position).IsEqualTo(6);
			Check.That(sut.Column).IsEqualTo(0);
			Check.That(sut.Line).IsEqualTo(2);

			Check.That(sut.Advance()).IsEqualTo(true);
			Check.That(sut.Position).IsEqualTo(7);
			Check.That(sut.Column).IsEqualTo(1);
			Check.That(sut.Line).IsEqualTo(2);

			Check.That(sut.Advance()).IsEqualTo(false);
			Check.That(sut.Position).IsEqualTo(7);
			Check.That(sut.Column).IsEqualTo(1);
			Check.That(sut.Line).IsEqualTo(2);
		}

		[Fact]
		public void CorrectlyHandleMixedLineBreaks()
		{
			var sut = Setup("\n\r\n\r");

			Check.That(sut.Advance()).IsEqualTo(true);
			Check.That(sut.Line).IsEqualTo(0);
			Check.That(sut.Column).IsEqualTo(0);

			Check.That(sut.Advance()).IsEqualTo(true);
			Check.That(sut.Line).IsEqualTo(1);
			Check.That(sut.Column).IsEqualTo(0);

			Check.That(sut.Advance()).IsEqualTo(true);
			Check.That(sut.Line).IsEqualTo(1);
			Check.That(sut.Column).IsEqualTo(1);

			Check.That(sut.Advance()).IsEqualTo(true);
			Check.That(sut.Line).IsEqualTo(2);
			Check.That(sut.Column).IsEqualTo(0);

			Check.That(sut.Advance()).IsEqualTo(false);
		}

		[Fact]
		public void HandleIndentation()
		{
			var sut = Setup("A\n\tB");

			Check.That(sut.Advance()).IsEqualTo(true);
			Check.That(sut.Column).IsEqualTo(0);
			Check.That(sut.Indentation).IsEqualTo(0);

			Check.That(sut.Advance()).IsEqualTo(true);
			Check.That(sut.Column).IsEqualTo(1);
			Check.That(sut.Indentation).IsEqualTo(0);

			Check.That(sut.Advance()).IsEqualTo(true);
			Check.That(sut.Column).IsEqualTo(0);
			Check.That(sut.Indentation).IsEqualTo(1);

			Check.That(sut.Advance()).IsEqualTo(true);
			Check.That(sut.Column).IsEqualTo(4);
			Check.That(sut.Indentation).IsEqualTo(1);

			Check.That(sut.Advance()).IsEqualTo(false);
			Check.That(sut.Column).IsEqualTo(4);
			Check.That(sut.Indentation).IsEqualTo(1);
		}

		[Fact]
		public void HandleNestedIndentation()
		{
			var sut = Setup("A\n\tB\n\t\tC");

			Check.That(sut.Advance()).IsEqualTo(true);
			Check.That(sut.Indentation).IsEqualTo(0);

			Check.That(sut.Advance()).IsEqualTo(true);
			Check.That(sut.Indentation).IsEqualTo(0);

			Check.That(sut.Advance()).IsEqualTo(true);
			Check.That(sut.Indentation).IsEqualTo(1);

			Check.That(sut.Advance()).IsEqualTo(true);
			Check.That(sut.Indentation).IsEqualTo(1);

			Check.That(sut.Advance()).IsEqualTo(true);
			Check.That(sut.Indentation).IsEqualTo(1);

			Check.That(sut.Advance()).IsEqualTo(true);
			Check.That(sut.Indentation).IsEqualTo(2);

			Check.That(sut.Advance()).IsEqualTo(true);
			Check.That(sut.Indentation).IsEqualTo(2);

			Check.That(sut.Advance()).IsEqualTo(true);
			Check.That(sut.Indentation).IsEqualTo(2);

			Check.That(sut.Advance()).IsEqualTo(false);
			Check.That(sut.Indentation).IsEqualTo(2);
		}

		[Fact]
		public void HandleDedentation()
		{
			var sut = Setup("A\n\tB\nC");

			Check.That(sut.Advance()).IsEqualTo(true);
			Check.That(sut.Indentation).IsEqualTo(0);

			Check.That(sut.Advance()).IsEqualTo(true);
			Check.That(sut.Indentation).IsEqualTo(0);

			Check.That(sut.Advance()).IsEqualTo(true);
			Check.That(sut.Indentation).IsEqualTo(1);

			Check.That(sut.Advance()).IsEqualTo(true);
			Check.That(sut.Indentation).IsEqualTo(1);

			Check.That(sut.Advance()).IsEqualTo(true);
			Check.That(sut.Indentation).IsEqualTo(1);

			Check.That(sut.Advance()).IsEqualTo(true);
			Check.That(sut.Indentation).IsEqualTo(0);

			Check.That(sut.Advance()).IsEqualTo(false);
			Check.That(sut.Indentation).IsEqualTo(0);
		}

		[Fact]
		public void HandleMixedIndentation()
		{
			var sut = Setup("A\n  \tB");

			Check.That(sut.Advance()).IsEqualTo(true);
			Check.That(sut.Indentation).IsEqualTo(0);

			Check.That(sut.Advance()).IsEqualTo(true);
			Check.That(sut.Indentation).IsEqualTo(0);

			Check.That(sut.Advance()).IsEqualTo(true);
			Check.That(sut.Indentation).IsEqualTo(1);

			Check.That(sut.Advance()).IsEqualTo(true);
			Check.That(sut.Indentation).IsEqualTo(1);

			Check.That(sut.Advance()).IsEqualTo(true);
			Check.That(sut.Indentation).IsEqualTo(1);

			Check.That(sut.Advance()).IsEqualTo(true);
			Check.That(sut.Indentation).IsEqualTo(1);

			Check.That(sut.Advance()).IsEqualTo(false);
			Check.That(sut.Indentation).IsEqualTo(1);
		}

		[Fact]
		public void HandleShortIndentation()
		{
			var sut = Setup("A\n  B");

			Check.That(sut.Advance()).IsEqualTo(true);
			Check.That(sut.Indentation).IsEqualTo(0);

			Check.That(sut.Advance()).IsEqualTo(true);
			Check.That(sut.Indentation).IsEqualTo(0);

			Check.That(sut.Advance()).IsEqualTo(true);
			Check.That(sut.Indentation).IsEqualTo(1);

			Check.That(sut.Advance()).IsEqualTo(true);
			Check.That(sut.Indentation).IsEqualTo(1);

			Check.That(sut.Advance()).IsEqualTo(true);
			Check.That(sut.Indentation).IsEqualTo(1);

			Check.That(sut.Advance()).IsEqualTo(false);
			Check.That(sut.Indentation).IsEqualTo(1);
		}

		[Fact]
		public void ResetProperly()
		{
			var sut = Setup("A text.");

			sut.Advance();
			sut.Advance();

			Check.That(sut.Position).IsStrictlyGreaterThan(0);

			sut.Reset();
			
			Check.That(sut.Text).IsEqualTo("A text.");
			Check.That(sut.Position).IsEqualTo(-1);
			Check.That(sut.Character).IsEqualTo('\0');
			Check.That(sut.Peek).IsEqualTo('A');
			Check.That(sut.Column).IsEqualTo(-1);
			Check.That(sut.Line).IsEqualTo(0);
			Check.That(sut.Indentation).IsEqualTo(0);
		}
	}
}
