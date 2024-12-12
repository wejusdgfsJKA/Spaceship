using System.Text;
public class Service : ElementBase
{
    protected System.Action onEvaluateFn;
    public Service(string _name, System.Action _evaluate)
    {
        Name = _name;
        onEvaluateFn = _evaluate;
    }
    public void Evaluate()
    {
        if (onEvaluateFn != null)
        {
            onEvaluateFn();
        }
    }
    public override void GetDebugTextInternal(StringBuilder _debug, int _indentlevel = 0)
    {
        // apply the indent
        for (int _index = 0; _index < _indentlevel; ++_index)
        {
            _debug.Append(' ');
        }
        _debug.Append($"S: {Name}");
    }
}