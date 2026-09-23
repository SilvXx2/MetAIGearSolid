using System;

public class DecisionActionNode : IDecisionNode
{
    private Action action;

    public DecisionActionNode(Action action)
    {
        this.action = action;
    }
    public void Execute()
    {
        action.Invoke();
    }
}
