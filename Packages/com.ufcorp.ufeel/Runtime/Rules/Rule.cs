using System;

namespace UFeel
{
    public class Rule
    {
        public Func<bool> Condition;
        public Action Action;
        public bool OnlyOnce;
        public int Id;

        public Rule(int id, Func<bool> condition, Action action, bool onlyOnce)
        {
            Id = id;
            Condition = condition;
            Action = action;
            OnlyOnce = onlyOnce;
        }
    }
}