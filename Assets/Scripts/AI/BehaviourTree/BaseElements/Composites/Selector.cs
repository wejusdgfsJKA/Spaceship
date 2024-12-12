public class Selector : Composite
{
    public Selector(string _name) : base(_name) { }
    public override bool Evaluate()
    {
        if (base.Evaluate())
        {
            for (int i = leftmost; i < children.Count; i++)
            {
                children[i].Evaluate();
                switch (children[i].State)
                {
                    case NodeState.FAILURE:
                        continue;
                    case NodeState.SUCCESS:
                        state = NodeState.SUCCESS;
                        if (i < children.Count - 1)
                        {
                            leftmost = i + 1;
                        }
                        else
                        {
                            leftmost = i;
                        }
                        return true;
                    case NodeState.RUNNING:
                        state = NodeState.RUNNING;
                        if (leftmost < i)
                        {
                            //we have a new valid node
                            leftmost = i;
                        }
                        return true;
                }
            }
            state = NodeState.FAILURE;
            return true;
        }
        return false;
    }
    public override void NewLeftmost(Node _child)
    {
        int _index = childrenIndexes[_child];
        if (_index >= 0 && _index < children.Count && _index < leftmost)
        {
            //we have a new valid node
            children[leftmost].Abort();
            leftmost = _index;
            if (state != NodeState.RUNNING)
            {
                if (Parent != null)
                {
                    Parent.NewLeftmost(this);
                }
            }
        }
    }
    public override void UpdateLeftmost()
    {
        //find the first node we can run and go from there
        int i = 0;
        while (i < children.Count)
        {
            if (children[i].BlockingDecorators == 0)
            {
                leftmost = i;
                if (state != NodeState.RUNNING)
                {
                    if (Parent != null)
                    {
                        Parent.NewLeftmost(this);
                    }
                }
                return;
            }
            i++;
        }
        // we have no valid nodes
        Abort();
    }
}
