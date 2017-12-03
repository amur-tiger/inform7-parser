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

		public Matcher(string pattern)
		{
			// "$variable is $value."
			// "$relationshipName relates $lhs [(called $lhsName)] to $rhs [(called $rhsName)]"

			var tokenizer = new Tokenizer();
			var tokens = tokenizer.Tokenize(pattern);
			var variable = false;

			rules = new List<Rule>();
			foreach (var token in tokens)
			{
				if (token.Surface == "$")
				{
					variable = true;
				}
				else if (token.Type == TokenType.Comment)
				{
					var content = token.Surface.Substring(1, token.Surface.Length - 2);
					var optionalTokens = tokenizer.Tokenize(content);
					var group = new Rule.RuleGroup();
					foreach (var optionalToken in optionalTokens)
					{
						if (optionalToken.Surface.StartsWith("$"))
						{
							variable = true;
						}
						else
						{
							if (variable)
							{
								group.Rules.Add(new Rule(TokenType.Word, true, optionalToken.Surface));
								variable = false;
							}
							else
							{
								group.Rules.Add(new Rule(optionalToken.Type, optionalToken.Surface));
							}
						}
					}
					rules.Add(group);
				}
				else
				{
					if (variable)
					{
						rules.Add(new Rule(TokenType.Word, true, token.Surface));
						variable = false;
					}
					else
					{
						rules.Add(new Rule(token.Type, token.Surface));
					}
				}
			}
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
			
			void Recurse(int tokenOffset, int ruleOffset, int groupRuleOffset, Match branch)
			{
				var currentToken = tokens[tokenOffset];
				var currentRule = rules[ruleOffset];

				if (currentRule is Rule.RuleGroup)
				{
					if (groupRuleOffset < 0)
					{
						Recurse(tokenOffset, ruleOffset + 1, -1, branch);
						var newBranch = branch.Duplicate();
						Recurse(tokenOffset, ruleOffset, 0, newBranch);
						return;
					}
					else
					{
						if (groupRuleOffset == (currentRule as Rule.RuleGroup).Rules.Count)
						{
							Recurse(tokenOffset, ruleOffset + 1, -1, branch);
							return;
						}

						currentRule = (currentRule as Rule.RuleGroup).Rules[groupRuleOffset];
					}
				}

				if (!MatchRule(currentToken, currentRule))
				{
					return;
				}

				if (currentRule.Name != null)
				{
					branch.SetName(branch.Count - 1, currentRule.Name);
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
					Recurse(tokenOffset + 1, ruleOffset, groupRuleOffset, newBranch);
				}
				
				branch.Add(new List<Token>());
				if (groupRuleOffset < 0)
				{
					Recurse(tokenOffset + 1, ruleOffset + 1, -1, branch);
				}
				else
				{
					Recurse(tokenOffset + 1, ruleOffset, groupRuleOffset + 1, branch);
				}
			}
			
			var initialMatch = new Match();
			initialMatch.Add(new List<Token>());

			Recurse(0, 0, -1, initialMatch);

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
