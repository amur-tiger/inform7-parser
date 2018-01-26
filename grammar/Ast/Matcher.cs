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
			// "The description is '$description'."

			var tokenizer = new Tokenizer();
			var tokens = tokenizer.Tokenize(pattern);
			var variable = false;
			var strlit = false;

			rules = new List<Rule>();
			foreach (var token in tokens)
			{
				if (token.Surface == "$")
				{
					variable = true;
				}
				else if (token.Surface == "'")
				{
					strlit = !strlit;
					if (strlit)
					{
						rules.Add(new Rule(TokenType.QuotesStart));
					}
					else
					{
						rules.Add(new Rule(TokenType.QuotesEnd));
					}
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
						variable = false;
						if (strlit)
						{
							rules.Add(new Rule(TokenType.Unknown, null, true, token.Surface, TokenType.QuotesEnd));
						}
						else
						{
							rules.Add(new Rule(TokenType.Word, true, token.Surface));
						}
					}
					else
					{
						// todo string variables may end up here, check strlit
						rules.Add(new Rule(token.Type, token.Surface));
					}
				}
			}
		}

		public List<Match> Match(IReadOnlyList<Token> tokens)
		{
			if (tokens == null)
			{
				throw new ArgumentNullException(nameof(tokens));
			}

			if (tokens.Count == 0)
			{
				return new List<Match>();
			}

			var result = new List<Match>();
			var branch = new Match();
			branch.Add(new List<Token>());

			Recursive(new Queue<Token>(tokens), new Queue<Rule>(rules), branch, result);

			return result;
		}

		private void Recursive(Queue<Token> tokens, Queue<Rule> rules, Match branch, List<Match> result)
		{
			if (rules.Peek() is Rule.RuleGroup)
			{
				var unrolled = new Queue<Rule>((rules.Dequeue() as Rule.RuleGroup).Rules);
				foreach (var r in rules)
				{
					unrolled.Enqueue(r);
				}

				Recursive(new Queue<Token>(tokens), unrolled, branch.Duplicate(), result);
				Recursive(tokens, rules, branch, result);

				return;
			}

			var rule = rules.Peek();
			var token = tokens.Dequeue();

			if (!MatchRule(token, rule))
			{
				return;
			}

			if (rules.Count > 1 && tokens.Count == 0)
			{
				return;
			}

			if (rule.Name != null)
			{
				branch.SetName(branch.Count - 1, rule.Name);
			}

			branch.Last().Add(token);

			if (rule.Repeatable)
			{
				Recursive(new Queue<Token>(tokens), new Queue<Rule>(rules), branch.Duplicate(), result);
			}

			rules.Dequeue();

			if (rules.Count == 0)
			{
				result.Add(branch);
				return;
			}
			
			branch.Add(new List<Token>());
			Recursive(tokens, rules, branch, result);
		}

		private bool MatchRule(Token token, Rule rule)
		{
			if (rule.WantedType != TokenType.Unknown && token.Type != rule.WantedType)
			{
				return false;
			}

			if (rule.WantedSurface != null && !rule.WantedSurface.Equals(token.Surface, StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}

			if (rule.WantedType == TokenType.Unknown && rule.ExceptType != TokenType.Unknown && token.Type == rule.ExceptType)
			{
				return false;
			}

			return true;
		}
    }
}
