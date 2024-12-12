using System.Text;

public abstract class ElementBase
{
    public string Name { get; protected set; }
    public string GetDebugText(int _indentlevel = 0)
    {
        StringBuilder _debugtextbuilder = new StringBuilder();

        GetDebugTextInternal(_debugtextbuilder, _indentlevel);

        return _debugtextbuilder.ToString();
    }
    public abstract void GetDebugTextInternal(StringBuilder _debugTextBuilder, int _indentLevel = 0);
}
