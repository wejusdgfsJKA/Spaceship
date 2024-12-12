using System.Collections.Generic;
using System.Text;

public abstract class Composite : Node
{
    protected List<Node> children = new();
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
    public Node AddChild(Node _node)
    {
        _node.Parent = this;
        childrenIndexes.Add(_node, children.Count);
        children.Add(_node);
        return _node;
    }
    public abstract void NewLeftmost(Node _child);
    public abstract void UpdateLeftmost();
    public void ChildInvalid(Node _child)
    {
        //this child is no longer valid
        if (childrenIndexes[_child] == leftmost)
        {
            UpdateLeftmost();
        }
    }
    public override void GetDebugTextInternal(StringBuilder _debug, int _indentlevel = 0)
    {
        base.GetDebugTextInternal(_debug, _indentlevel);
        _debug.AppendLine();
        _debug.Append("Leftmost: " + leftmost);
        foreach (var _child in children)
        {
            _debug.AppendLine();
            _child.GetDebugTextInternal(_debug, _indentlevel + 2);
        }
    }
}
