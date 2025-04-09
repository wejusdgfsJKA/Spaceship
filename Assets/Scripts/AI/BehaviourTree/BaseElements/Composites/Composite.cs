using System.Collections.Generic;
using System.Text;

namespace BTree
{
    public abstract class Composite : Node
    {
        protected List<Node> children = new();
        /// <summary>
        /// Contains the indexes of all children in the children list.
        /// </summary>
        protected Dictionary<Node, int> childrenIndexes = new();
        protected int leftmost;
        public int Leftmost
        {
            get
            {
                return leftmost;
            }
        }
        public Composite(string _name) : base(_name)
        {
            //reset our starting point everytime we enter
            onEnter += () =>
            {
                leftmost = 0;
            };
        }
        /// <summary>
        /// Add a child node to this composite node.
        /// </summary>
        /// <param name="node">The child node to be added.</param>
        /// <returns>The child node that was added.</returns>
        public Node AddChild(Node node)
        {
            node.Parent = this;
            childrenIndexes.Add(node, children.Count);
            children.Add(node);
            return node;
        }
        /// <summary>
        /// A new child can be evaluated.
        /// </summary>
        /// <param name="child">The child that can now be evaluated.</param>
        public abstract void NewLeftmost(Node child);
        /// <summary>
        /// Reconsider what child to evaluate.
        /// </summary>
        public abstract void UpdateLeftmost();
        /// <summary>
        /// This child node is no longer valid. If he was the leftmost child we 
        /// could evaluate, update leftmost.
        /// </summary>
        /// <param name="child">The child node which is now invalid.</param>
        public void ChildInvalid(Node child)
        {
            //this child is no longer valid
            if (childrenIndexes[child] == leftmost)
            {
                UpdateLeftmost();
            }
        }
        /// <summary>
        /// Return state of this node, services and decorators, as well as its children.
        /// </summary>
        /// <param name="debug">StringBuilder object.</param>
        /// <param name="indentLevel">The level of indentation that we should apply.</param>
        public override void GetDebugTextInternal(StringBuilder debug, int indentlevel = 0)
        {
            base.GetDebugTextInternal(debug, indentlevel);
            debug.AppendLine();
            debug.Append("Leftmost: " + leftmost);
            foreach (var child in children)
            {
                debug.AppendLine();
                child.GetDebugTextInternal(debug, indentlevel + 2);
            }
        }
    }
}