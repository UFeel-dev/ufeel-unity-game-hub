using System;
using System.Collections.Generic;

namespace UFeel
{
    internal static class RulesProcessor
    {
        private static int _nextRuleId = 0;
        private static readonly List<Rule> _rules = new();
        private static readonly List<Rule> _rulesToAdd = new();
        private static readonly HashSet<int> _rulesToRemove = new();


        // TODO: 
        internal static void UpdateRules()
        {
            foreach (Rule rule in _rules)
            {
                if (!rule.Condition())
                    continue;

                rule.Action?.Invoke();

                if (rule.OnlyOnce)
                    _rulesToRemove.Add(rule.Id);
            }

            if (_rulesToRemove.Count > 0)
            {
                for (int i = _rules.Count - 1; i >= 0; i--)
                {
                    if (_rulesToRemove.Remove(_rules[i].Id))
                    {
                        _rules.RemoveAt(i);
                    }
                }
                _rulesToRemove.Clear();
            }

            if (_rulesToAdd.Count > 0)
            {
                _rules.AddRange(_rulesToAdd);
                _rulesToAdd.Clear();
            }
        }

        internal static RuleKey AddRule(Func<bool> condition, Action action, bool onlyOnce = false)
        {
            Rule rule = new(
                id: _nextRuleId++,
                condition: condition,
                action: action,
                onlyOnce: onlyOnce
            );

            _rulesToAdd.Add(rule);
            return new RuleKey(rule.Id);
        }

        public static void RemoveRule(RuleKey key)
        {
            _rulesToRemove.Add(key.Id);
        }
    }
}