using System.Text;
namespace BTree
{
    public class Service : ElementBase
    {
        protected System.Action onEvaluateFn;
        public Service(string name, System.Action evaluate)
        {
            Name = name;
            onEvaluateFn = evaluate;
        }
        /// <summary>
        /// Run the service.
        /// </summary>
        public void Evaluate()
        {
            if (onEvaluateFn != null)
            {
                onEvaluateFn();
            }
        }
        /// <summary>
        /// Get information about the service.
        /// </summary>
        /// <param name="debug">StringBuilder instance.</param>
        /// <param name="indentlevel">The level of indentation 
        /// we should apply.</param>
        public override void GetDebugTextInternal(StringBuilder
            debug, int indentlevel = 0)
        {
            // apply the indent
            for (int index = 0; index < indentlevel; ++index)
            {
                debug.Append(' ');
            }
            debug.Append($"S: {Name}");
        }
    }
}