using System;

public class DecisionActionNode : IDecisionNode
{
    private Action action;

    public DecisionActionNode(Action action)
    {
        this.action = action;
    }

    // Ejecuta la accion que recibe
    public void Execute()
    {
        action.Invoke();
    }
}

