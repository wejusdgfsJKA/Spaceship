using System;
public class LeafNode : Node
{
    //action node
    protected Func<NodeState> onEvaluate;
    public LeafNode(string _name, Func<NodeState> _evaluate, Action _enter = null,
        Action _exit = null) : base(_name, _enter, _exit)
    {
        onEvaluate = _evaluate;
    }
    public override bool Evaluate()
    {
        if (base.Evaluate())
        {
            if (onEvaluate != null)
            {
                State = onEvaluate();
                return false;
            }
        }
        return false;
    }
}
