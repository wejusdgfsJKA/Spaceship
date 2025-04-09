namespace BTree
{
    /// <summary>
    /// Only has two children. Evaluate left child node. If not failure, evaluate 
    /// right child node. State is state of left child node.
    /// </summary>
    public class ParallelNode : Composite
    {
        public ParallelNode(string name, Node leftChild, Node rightChild) : base(name)
        {
            children.Add(leftChild);
            children.Add(rightChild);
        }
        /// <summary>
        /// Evaluate the left child. If not failure, evaluate right child.
        /// </summary>
        /// <returns></returns>
        public override bool Evaluate()
        {
            if (base.Evaluate())
            {
                children[0].Evaluate();
                state = children[0].State;
                if (state != NodeState.FAILURE)
                {
                    children[1].Evaluate();
                }
                return true;
            }
            return false;
        }
        /// <summary>
        /// Does nothing.
        /// </summary>
        /// <param name="child"></param>
        public override void NewLeftmost(Node child) { }
        /// <summary>
        /// Does nothing.
        /// </summary>
        public override void UpdateLeftmost() { }
    }
}