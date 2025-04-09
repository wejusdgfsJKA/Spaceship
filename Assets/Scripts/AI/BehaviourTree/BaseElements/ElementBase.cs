using System.Text;

namespace BTree
{
    public abstract class ElementBase
    {
        public string Name { get; protected set; }
        /// <summary>
        /// Get debug text regarding the state of this element.
        /// </summary>
        /// <param name="indentlevel">How much we should indent.</param>
        /// <returns>Text regarding the state of this element.</returns>
        public string GetDebugText(int indentlevel = 0)
        {
            StringBuilder debugtextbuilder = new StringBuilder();

            GetDebugTextInternal(debugtextbuilder, indentlevel);

            return debugtextbuilder.ToString();
        }
        public abstract void GetDebugTextInternal(StringBuilder debugTextBuilder, int indentLevel = 0);
    }
}