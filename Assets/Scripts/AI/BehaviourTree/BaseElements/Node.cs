using System;
using System.Collections.Generic;
using System.Text;
namespace BTree
{
    public enum NodeState
    {
        RUNNING,
        SUCCESS,
        FAILURE
    }

    public abstract class Node : ElementBase
    {
        /// <summary>
        /// Fires when the node is interrupted from running (NOT ON LOCAL SUCCESS!).
        /// </summary>
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
        /// <summary>
        /// Fires when entering the node.
        /// </summary>
        protected Action onEnter;
        /// <summary>
        /// Fires when exiting the node, either on success or failure.
        /// </summary>
        protected Action onExit;
        /// <summary>
        /// The number of decorators which are blocking the execution of this node.
        /// </summary>
        public int BlockingDecorators { get; protected set; } = 0;
        public Node(string name, Action enter = null, Action exit = null)
        {
            Name = name;
            onEnter = enter;
            onExit = exit;
            Abort += () =>
            {
                if (Parent != null)
                {
                    Parent.ChildInvalid(this);
                }
                State = NodeState.FAILURE;
            };
        }
        /// <summary>
        /// Evaluate the node: <br />
        /// Run services. <br />
        /// If we have blocking decorators return false, node state is failure. <br />
        /// If node was not running previously, run onEnter.<br />
        /// Return true.
        /// </summary>
        /// <returns>True if the execution so far was successfull. This is necessary because children need to know if they should continue execution or not.</returns>
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
        /// <summary>
        /// Add a new service to this node.
        /// </summary>
        /// <param name="service">The service to be added.</param>
        /// <returns>The service that was added.</returns>
        public Service AddService(Service service)
        {
            services.Add(service);
            return service;
        }
        /// <summary>
        /// Add a new decorator to this node.
        /// </summary>
        /// <param name="decorator">Decorator to be added.</param>
        /// <returns>Returns the added decorator.</returns>
        public Decorator AddDecorator(Decorator decorator)
        {
            decorators.Add(decorator);
            decorator.OnPass += OnDecoratorPass;
            decorator.OnFail += OnDecoratorFail;
            return decorator;
        }
        /// <summary>
        /// A decorator has passed and is no longer blocking the execution. If we can 
        /// now execute, notify the parent.
        /// </summary>
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
        /// <summary>
        /// A decorator has failed its condition, abort if currently running.
        /// </summary>
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
        /// <summary>
        /// Run all services.
        /// </summary>
        protected void RunServices()
        {
            //run all available services
            for (int i = 0; i < services.Count; i++)
            {
                services[i].Evaluate();
            }
        }
        /// <summary>
        /// Return state of this node, services and decorators.
        /// </summary>
        /// <param name="debug">StringBuilder object.</param>
        /// <param name="indentLevel">The level of indentation that we should apply.</param>
        public override void GetDebugTextInternal(StringBuilder debug, int indentLevel = 0)
        {
            // apply the indent
            for (int index = 0; index < indentLevel; ++index)
            {
                debug.Append(' ');
            }

            debug.Append($"{Name} [{state}]");
            if (Parent != null)
            {
                debug.AppendLine();
            }
            if (BlockingDecorators > 0)
            {
                debug.AppendLine();
                debug.Append(BlockingDecorators + " blocking decorators.");
            }

            foreach (var service in services)
            {
                debug.AppendLine();
                debug.Append(service.GetDebugText(indentLevel + 1));
            }

            foreach (var decorator in decorators)
            {
                debug.AppendLine();
                debug.Append(decorator.GetDebugText(indentLevel + 1));
            }
        }
    }
}