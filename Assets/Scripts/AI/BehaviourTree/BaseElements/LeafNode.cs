using System;
namespace BTree
{
    /// <summary>
    /// This node will always be at the bottom of the tree.
    /// </summary>
    public class LeafNode : Node
    {
        protected Func<NodeState> onEvaluate;
        public LeafNode(string name, Func<NodeState> evaluate, Action enter = null,
            Action exit = null) : base(name, enter, exit)
        {
            onEvaluate = evaluate;
        }
        /// <summary>
        /// If possible, run onEvaluate.
        /// </summary>
        /// <returns></returns>
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
}