public class Sequence : Composite
{
    public Sequence(string _name) : base(_name) { }
    public override bool Evaluate()
    {
        if (base.Evaluate())
        {
            for (int i = leftmost; i < children.Count; i++)
            {
                children[i].Evaluate();
                switch (children[i].State)
                {
                    case NodeState.RUNNING:
                        state = NodeState.RUNNING;
                        leftmost = i;
                        return false;
                    case NodeState.SUCCESS:
                        state = NodeState.RUNNING;
                        continue;
                    case NodeState.FAILURE:
                        state = NodeState.FAILURE;
                        return false;
                }
            }
            state = NodeState.SUCCESS;
            return true;
        }
        return false;
    }
    public override void NewLeftmost(Node _child)
    {
        //we don't need to do anything here
    }
    public override void UpdateLeftmost()
    {
        //if our current node is no longer valid we need to abort
        if (state == NodeState.RUNNING)
        {
            Abort();
        }
    }
}
