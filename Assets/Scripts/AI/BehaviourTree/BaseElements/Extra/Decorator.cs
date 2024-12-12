using System;
using System.Text;
public class Decorator : ElementBase
{
    public Func<bool> OnEvaluate;
    protected bool result = true;
    public bool Result
    {
        get
        {
            return Result;
        }
        protected set
        {
            if (result != value)
            {
                //the decorator's state changes, we need to notify our listeners
                result = value;
                if (result)
                {
                    OnPass();
                }
                else
                {
                    OnFail();
                }
            }
        }
    }
    protected Action onDataChanged;

    public Action OnPass, OnFail;
    public Decorator(string _name, Func<bool> _onevaluate)
    {
        Name = _name;
        OnEvaluate = _onevaluate;
        onDataChanged += () =>
        {
            //re-evaluate everytime the value of the data changes
            Result = OnEvaluate();
        };
    }
    public override void GetDebugTextInternal(StringBuilder _debug, int _indentlevel = 0)
    {
        // apply the indent
        for (int _index = 0; _index < _indentlevel; ++_index)
        {
            _debug.Append(' ');
        }
        _debug.Append($"D: {Name} [{(result ? "PASS" : "FAIL")}]");
    }
    public void MonitorValue(BlackBoard _data, string _key)
    {
        //subscribe to relevant data
        _data.AddListener(onDataChanged, _key);
    }
    public void StopMonitoringValue(BlackBoard _data, string _key)
    {
        //unsubscribe from data which is no longer relevant
        _data.AddListener(onDataChanged, _key);
    }
}