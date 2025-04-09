using System;
using System.Text;

namespace BTree
{
    public class Decorator : ElementBase
    {
        /// <summary>
        /// Returns true if all conditions have been passed, 
        /// false otherwise.
        /// </summary>
        public Func<bool> OnEvaluate;
        protected bool result = true;
        /// <summary>
        /// The result of OnEvaluate.
        /// </summary>
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
        /// <summary>
        /// Fires when the value of a monitored resource changes.
        /// </summary>
        protected Action onDataChanged;
        /// <summary>
        /// Fires when the Decorator has passed all conditions.
        /// </summary>
        public Action OnPass;
        /// <summary>
        /// Fires when the Decorator is no longer passing all conditions.
        /// </summary>
        public Action OnFail;
        public Decorator(string name, Func<bool> onevaluate)
        {
            Name = name;
            OnEvaluate = onevaluate;
            onDataChanged += () =>
            {
                //re-evaluate everytime the value of the data changes
                Result = OnEvaluate();
            };
        }
        /// <summary>
        /// Get info about this Decorator(Whether it has passed or not).
        /// </summary>
        /// <param name="debug">StringBuilder instance.</param>
        /// <param name="indentlevel">Level of indentation we should apply.</param>
        public override void GetDebugTextInternal(StringBuilder
            debug, int indentlevel = 0)
        {
            // apply the indent
            for (int index = 0; index < indentlevel; ++index)
            {
                debug.Append(' ');
            }
            debug.Append($"D: {Name} [{(result ? "PASS" : "FAIL")}]");
        }
        /// <summary>
        /// Begin monitoring a relevant resource.
        /// </summary>
        /// <param name="data">Blackboard being monitored.</param>
        /// <param name="key">The key of the resource we want monitored.</param>
        public void MonitorValue(BlackBoard data, string key)
        {
            //subscribe to relevant data
            data.AddListener(onDataChanged, key);
        }
        /// <summary>
        /// Stop monitoring a resource.
        /// </summary>
        /// <param name="data">The Blackboard we were monitoring.</param>
        /// <param name="key">The key of the resource we were monitoring</param>
        public void StopMonitoringValue(BlackBoard data, string key)
        {
            //unsubscribe from data which is no longer relevant
            data.RemoveListener(onDataChanged, key);
        }
    }
}