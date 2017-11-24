using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TigeR.Inform7.Tokens;

namespace TigeR.Inform7.Ast
{
    internal class Matcher
    {
		private readonly List<Rule> rules;

		public Matcher(Rule firstRule, params Rule[] otherRules)
		{
			if (firstRule == null)
			{
				throw new ArgumentNullException(nameof(firstRule));
			}

			if (otherRules == null)
			{
				throw new ArgumentNullException(nameof(otherRules));
			}

			foreach (var rule in otherRules)
			{
				if (rule == null)
				{
					throw new ArgumentNullException(nameof(otherRules));
				}
			}

			rules = new List<Rule>(otherRules.Length + 1);
			rules.Add(firstRule);
			rules.AddRange(otherRules);
		}

		public List<Match> Match(List<Token> tokens)
		{
			if (tokens == null)
			{
				throw new ArgumentNullException(nameof(tokens));
			}

			if (tokens.Count == 0)
			{
				throw new ArgumentException("Tokens cannot be empty", nameof(tokens));
			}

			var result = new List<Match>();
			
			void Recurse(int tokenOffset, int ruleOffset, Match branch)
			{
				var currentToken = tokens[tokenOffset];
				var currentRule = rules[ruleOffset];

				if (!MatchRule(currentToken, currentRule))
				{
					return;
				}

				branch.Last().Add(currentToken);
				if (tokenOffset == tokens.Count - 1 && ruleOffset == rules.Count - 1)
				{
					result.Add(branch);
					return;
				}

				if (currentRule.Repeatable)
				{
					var newBranch = branch.Duplicate();
					Recurse(tokenOffset + 1, ruleOffset, newBranch);
				}
				
				branch.Add(new List<Token>());
				Recurse(tokenOffset + 1, ruleOffset + 1, branch);
			}
			
			var initialMatch = new Match();
			initialMatch.Add(new List<Token>());

			Recurse(0, 0, initialMatch);

			return result;
		}

		private bool MatchRule(Token token, Rule rule)
		{
			if (token.Type != rule.WantedType)
			{
				return false;
			}

			if (rule.WantedSurface != null && token.Surface != rule.WantedSurface)
			{
				return false;
			}

			return true;
		}
    }
}
