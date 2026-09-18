using System;
using UnityEngine;

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

    public void Excecute()
    {
        if(question.Invoke())
            trueNode.Excecute();
        else
            falseNode.Excecute();
    }
}
