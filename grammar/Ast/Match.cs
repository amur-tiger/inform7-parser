using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TigeR.Inform7.Tokens;

namespace TigeR.Inform7.Ast
{
    internal class Match : List<List<Token>>, IEquatable<Match>
    {
		// todo: update when removing stuff
		private readonly Dictionary<String, Int32> matchNames = new Dictionary<String, Int32>();

		public Match() : base() { }

		public Match(IEnumerable<List<Token>> list) : base(list) { }

		public Match Duplicate()
		{
			var result = new Match();
			foreach (var group in this)
			{
				result.Add(new List<Token>(group));
			}

			foreach (var name in matchNames)
			{
				result.SetName(name.Value, name.Key);
			}

			return result;
		}

		public List<Token> this[String name]
		{
			get
			{
				if (!matchNames.ContainsKey(name))
				{
					return null;
				}

				var index = matchNames[name];
				return this[index];
			}
		}

		public void SetName(int index, string name)
		{
			matchNames[name] = index;
		}

		public void Add(string name, List<Token> group)
		{
			Add(group);
			matchNames.Add(name, Count - 1);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as Match);
		}

		public bool Equals(Match other)
		{
			if (other == null)
			{
				return false;
			}

			if (Count != other.Count)
			{
				return false;
			}

			if (!matchNames.SequenceEqual(other.matchNames))
			{
				return false;
			}

			for (int i = 0; i < Count; i++)
			{
				if (this[i].Count != other[i].Count)
				{
					return false;
				}

				for (int j = 0; j < this[i].Count; j++)
				{
					if (!this[i][j].Equals(other[i][j]))
					{
						return false;
					}
				}
			}

			return true;
		}

		public override string ToString()
		{
			return "[" + String.Join("] [", this.Select(group => String.Join(' ', group.Select(token => token.Surface)))) + "]";
		}
	}
}
