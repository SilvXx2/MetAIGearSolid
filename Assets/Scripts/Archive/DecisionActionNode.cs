using System;
using UnityEngine;

public class DecisionActionNode : IDecisionNode
{
    private Action action;

    public DecisionActionNode(Action action)
    {
        this.action = action;
    }
    public void Excecute()
    {
        action.Invoke();
    }
}
