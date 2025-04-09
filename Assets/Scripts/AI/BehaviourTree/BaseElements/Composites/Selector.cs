namespace BTree
{/// <summary>
 /// Evaluate first possible node.
 /// If found successfull child node, state is SUCCESS.<br />
 /// If found child node which can be ran, state is RUNNING.<br />
 /// Otherwise state is FAILURE.
 /// </summary>
    public class Selector : Composite
    {
        public Selector(string name) : base(name) { }
        /// <summary>
        /// Start with leftmost child which can be evaluated. If leftmost not running,
        /// continue rightward.<br/>
        /// </summary>
        /// <returns>True if found child to evaluate, false otherwise.</returns>
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
        /// <summary>
        /// The selector has a new child which it can run. If this child is at the 
        /// left of the leftmost node which can be currently evaluated, abort leftmost
        /// and set its value to the new child. Notify parent if necessary.
        /// </summary>
        /// <param name="child">The child which can be executed.</param>
        public override void NewLeftmost(Node child)
        {
            int index;
            if (childrenIndexes.TryGetValue(child, out index))
            {
                if (index < leftmost)
                {
                    //we have a new valid node, so we abort the previous running node
                    if (children[leftmost].State == NodeState.RUNNING)
                    {
                        children[leftmost].Abort();
                    }
                    leftmost = index;
                    if (state != NodeState.RUNNING)
                    {
                        if (Parent != null)
                        {
                            Parent.NewLeftmost(this);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Find the first node we can run and set it as the leftmost child which we can evaluate.
        /// </summary>
        public override void UpdateLeftmost()
        {
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
}