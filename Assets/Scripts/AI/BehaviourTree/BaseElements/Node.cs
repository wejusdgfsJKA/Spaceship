using System;
using System.Collections.Generic;
using System.Text;
public enum NodeState
{
    RUNNING,
    SUCCESS,
    FAILURE
}

public abstract class Node : ElementBase
{
    public Action Abort;
    protected NodeState state = NodeState.FAILURE;
    public NodeState State
    {
        get
        {
            return state;
        }
        set
        {
            if (state != value)
            {
                if (value == NodeState.RUNNING)
                {
                    //this node should stop executing if the parent does
                    if (Parent != null)
                    {
                        Parent.Abort += Abort;
                    }
                }
                else if (state == NodeState.RUNNING)
                {
                    if (onExit != null)
                    {
                        onExit();
                    }
                    if (Parent != null)
                    {
                        Parent.Abort -= Abort;
                    }
                }
                state = value;
            }
        }
    }
    public Composite Parent { get; set; }
    protected List<Service> services = new();
    protected List<Decorator> decorators = new();
    protected Action onEnter, onExit;
    public int BlockingDecorators { get; protected set; } = 0;
    public Node(string _name, Action _enter = null, Action _exit = null)
    {
        Name = _name;
        onEnter = _enter;
        onExit = _exit;
        Abort += () =>
        {
            if (Parent != null)
            {
                Parent.ChildInvalid(this);
            }
            State = NodeState.FAILURE;
        };
    }
    public virtual bool Evaluate()
    {
        RunServices();
        if (BlockingDecorators > 0)
        {
            //we have invalid decorators
            State = NodeState.FAILURE;
            return false;
        }
        if (State != NodeState.RUNNING && onEnter != null)
        {
            //run the enter function if the node was not running before
            onEnter();
            State = NodeState.RUNNING;
        }
        return true;
    }
    public Service AddService(Service _service)
    {
        services.Add(_service);
        return _service;
    }
    public Decorator AddDecorator(Decorator _decorator)
    {
        decorators.Add(_decorator);
        _decorator.OnPass += OnDecoratorPass;
        _decorator.OnFail += OnDecoratorFail;
        return _decorator;
    }
    protected void OnDecoratorPass()
    {
        //this decorator passes its condition
        if (BlockingDecorators > 0)
        {
            BlockingDecorators--;
        }
        if (Parent != null && BlockingDecorators == 0)
        {
            //we must notify the parent composite
            Parent.NewLeftmost(this);
        }
    }
    protected void OnDecoratorFail()
    {
        //this decorator fails its condition
        if (State == NodeState.RUNNING && BlockingDecorators == 0)
        {
            //abort if we were previously running
            Abort();
        }
        BlockingDecorators++;
    }
    protected void RunServices()
    {
        //run all available services
        for (int i = 0; i < services.Count; i++)
        {
            services[i].Evaluate();
        }
    }
    public override void GetDebugTextInternal(StringBuilder _debug, int _indentlevel = 0)
    {
        // apply the indent
        for (int _index = 0; _index < _indentlevel; ++_index)
            _debug.Append(' ');

        _debug.Append($"{Name} [{state}]");
        if (Parent != null)
        {
            _debug.AppendLine();
        }
        if (BlockingDecorators > 0)
        {
            _debug.AppendLine();
            _debug.Append(BlockingDecorators + " blocking decorators.");
        }

        foreach (var _service in services)
        {
            _debug.AppendLine();
            _debug.Append(_service.GetDebugText(_indentlevel + 1));
        }

        foreach (var _decorator in decorators)
        {
            _debug.AppendLine();
            _debug.Append(_decorator.GetDebugText(_indentlevel + 1));
        }
    }
}