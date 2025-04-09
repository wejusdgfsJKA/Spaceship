using System.Collections;
using UnityEngine;

namespace BTree
{
    public abstract class BTree : MonoBehaviour
    {
        /// <summary>
        /// The period of time between updates.
        /// </summary>
        [SerializeField] protected float updateinterval = .1f;
        /// <summary>
        /// When false, the tree will pause execution until set to true.
        /// </summary>
        [field: SerializeField]
        public bool ShouldRun { get; set; } = true;
        /// <summary>
        /// Root node.
        /// </summary>
        protected Composite root = null;
        /// <summary>
        /// Main coroutine responsible for running the whole tree.
        /// </summary>
        protected Coroutine coroutine;
        /// <summary>
        /// This tree's Blackboard.
        /// </summary>
        protected BlackBoard localMemory = new();
        /// <summary>
        /// This governs how often the BT updates.
        /// </summary>
        protected WaitForSeconds waitInterval;
        /// <summary>
        /// Secondary condition, used with ShouldRun.
        /// </summary>
        protected WaitUntil waitForPermission;
        protected virtual void Awake()
        {
            waitInterval = new(updateinterval);
            waitForPermission = new(() => { return ShouldRun; });
            root = SetupTree();
        }
        protected virtual void OnEnable()
        {
            ShouldRun = true;
            coroutine = StartCoroutine(UpdateLoop());
        }
        protected void OnDisable()
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
        }
        protected IEnumerator UpdateLoop()
        {
            yield return new WaitUntil(new(() => { return localMemory != null; }));
            while (true)
            {
                yield return waitInterval;
                yield return waitForPermission;
                root?.Evaluate();
            }
        }
        /// <summary>
        /// Called on Awake, builds the BT.
        /// </summary>
        /// <returns>The root node.</returns>
        protected abstract Composite SetupTree();
        /// <summary>
        /// Set the value of a resource in the Blackboard.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetData<T>(string key, T value)
        {
            localMemory.SetData<T>(key, value);
        }
        /// <summary>
        /// Get information about the BT.
        /// </summary>
        /// <returns>Text containing information about the root and its subtrees.</returns>
        public string GetDebugText()
        {
            return root.GetDebugText();
        }
    }
}