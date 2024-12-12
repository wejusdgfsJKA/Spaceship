using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
public class SphereCheck : MonoBehaviour
{
    HashSet<Collider> exceptions;
    [SerializeField] protected LayerMask obstructionMask;
    [field: SerializeField] public float radius { get; protected set; }
    private void Awake()
    {
        //this method adds to the set of exceptions all colliders of this object
        AddColliders(transform.root);
    }
    void AddColliders(Transform _t)
    {
        var _c = _t.GetComponent<Collider>();
        if (_c != null)
        {
            exceptions.Add(_c);
            for (int i = 0; i < _t.childCount; i++)
            {
                AddColliders(_t.GetChild(i));
            }
        }
    }
    public bool PerformCheck(Vector3 _direction)
    {
        Collider[] buffer = new Collider[10];
        int l = Physics.OverlapSphereNonAlloc(transform.position + _direction,
            radius, buffer, obstructionMask);
        for (int i = 0; i < buffer.Length; i++)
        {
            if (exceptions.Contains(buffer[i]))
            {
                l--;
                if (l == 0)
                {
                    return false;
                }
            }
        }
        return true;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(SphereCheck))]
public class SphereCheckDebug : Editor
{
    private void OnSceneGUI()
    {
        SphereCheck check = (SphereCheck)target;

        Handles.DrawWireArc(check.transform.position, check.transform.forward,
            check.transform.up, 360, check.radius);
        Handles.DrawWireArc(check.transform.position, check.transform.up,
            check.transform.forward, 360, check.radius);
        Handles.DrawWireArc(check.transform.position, check.transform.right,
            check.transform.forward, 360, check.radius);
    }
}
#endif
