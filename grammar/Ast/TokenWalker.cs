using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TigeR.Inform7.Tokens;

namespace TigeR.Inform7.Ast
{
	internal class TokenWalker : IEnumerator<Token>
	{
		private IEnumerator<Token> enumerator;

		private Queue<Token> cache = new Queue<Token>();

		private Token cacheNext;

		public IEnumerable<Token> Tokens { get; }

		public Token Current
		{
			get
			{
				if (cacheNext != null)
				{
					return cacheNext;
				}
				else
				{
					return enumerator.Current;
				}
			}
		}

		object IEnumerator.Current => Current;

		public TokenWalker(IEnumerable<Token> tokens)
		{
			Tokens = tokens;
			Reset();
		}

		public void Mark()
		{
			// cache everything, keep in cache even if enumerated, only release items if mark called again
		}

		public void Rewind()
		{
			// use up cache before others
		}

		public bool MoveNext()
		{
			if (cache.Count > 0)
			{
				cacheNext = cache.Dequeue();
				return true;
			}

			return enumerator.MoveNext();
		}

		public void Dispose()
		{
			enumerator.Dispose();
		}

		public void Reset()
		{
			enumerator = Tokens.GetEnumerator();
		}
	}
}
