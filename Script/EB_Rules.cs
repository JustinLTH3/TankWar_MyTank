using System.Collections.Generic;

public class EB_Rules
{
    public List<EB_Rule> RuleList { get; } = new List<EB_Rule>();
    
    public void AddRule(EB_Rule rule)
    {
        RuleList.Add(rule);
    }
}