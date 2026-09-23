using System;

public class DecisionQuestionNode : IDecisionNode
{
    private Func<bool> question;
    private IDecisionNode trueNode;
    private IDecisionNode falseNode;

    public DecisionQuestionNode(Func<bool> question, IDecisionNode trueNode, IDecisionNode falseNode)
    {
        this.question = question;
        this.trueNode = trueNode;
        this.falseNode = falseNode;
    }

    public void Execute()
    {
        if(question.Invoke())
            trueNode.Execute();
        else
            falseNode.Execute();
    }
}
