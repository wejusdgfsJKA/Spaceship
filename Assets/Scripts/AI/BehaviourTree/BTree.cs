using System.Collections;
using UnityEngine;

public abstract class BTree : MonoBehaviour
{
    [SerializeField] protected float updateinterval = .1f;
    [field: SerializeField]
    public bool ShouldRun { get; set; } = true;
    protected Composite root = null;
    protected Coroutine coroutine;
    protected BlackBoard localMemory = new();
    protected WaitForSeconds waitInterval;
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
    protected abstract Composite SetupTree();
    public void SetData<T>(string _blackboard, string _key, T _value)
    {
        localMemory.SetData<T>(_key, _value);
    }
    public string GetDebugText()
    {
        return root.GetDebugText();
    }
}