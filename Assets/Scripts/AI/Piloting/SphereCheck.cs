using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
public class SphereCheck : MonoBehaviour
{
    HashSet<Collider> exceptions = new();
    [SerializeField] protected LayerMask obstructionMask;
    [field: SerializeField] public float radius { get; protected set; }
    protected void Awake()
    {
        //this method adds to the set of exceptions all colliders of this object
        //and its children
        AddColliders(transform.root);
    }
    protected void AddColliders(Transform _t)
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
    public bool PerformRayCheck(Vector3 _direction)
    {
        Collider[] buffer = new Collider[10];
        int l = Physics.OverlapSphereNonAlloc(transform.position + _direction,
            radius, buffer, obstructionMask);
        //Debug.DrawLine(transform.position, transform.position + _direction, Color.magenta, 1);
        for (int i = 0; i < buffer.Length; i++)
        {
            if (exceptions.Contains(buffer[i]))
            {
                //ignore all contacts in the exceptions set
                l--;
                if (l == 0)
                {
                    return false;
                }
            }
        }
        return true;
    }
    public bool PerformLineCheck(Vector3 _endpoint)
    {
        return PerformRayCheck((_endpoint - transform.position).normalized);
    }
    protected Vector3 LocalConverter(float _x, float _y, float _z)
    {
        return (transform.forward * _z + transform.right * _x +
            transform.up * _y).normalized;
    }
    public Vector3? GetValidDirection()
    {
        //we will likely only have simple stuff like the
        //occasional asteroid to worry about

        //first check forward, then to the sides, then to the rear
        int _z = 1;
        Vector3 _v;
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (j != 0 || i != 0)
                {
                    //avoid checking directly forward
                    _v = LocalConverter(i, j, _z);
                    if (!PerformRayCheck(_v))
                    {
                        Debug.DrawRay(transform.position, _v, Color.green, 1);
                        return _v;
                    }
                    Debug.DrawRay(transform.position, _v, Color.red, 1);
                }
            }
        }

        _z = 0;
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (j != 0 || i != 0)
                {
                    //avoid checking for the zero vector
                    _v = LocalConverter(i, j, _z);
                    if (!PerformRayCheck(_v))
                    {
                        Debug.DrawRay(transform.position, _v, Color.green, 1);
                        return _v;
                    }
                    Debug.DrawRay(transform.position, _v, Color.red, 1);
                }
            }
        }

        _z = -1;
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (j != 0 || i != 0)
                {
                    //avoid checking directly behind us,
                    //that might create a loop
                    _v = LocalConverter(i, j, _z);
                    if (!PerformRayCheck(_v))
                    {
                        Debug.DrawRay(transform.position, _v, Color.green, 1);
                        return _v;
                    }
                    Debug.DrawRay(transform.position, _v, Color.red, 1);
                }
            }
        }

        _v = LocalConverter(0, 0, -1);
        //if we have exhausted all other options try to go directly bakwards
        if (!PerformRayCheck(_v))
        {
            Debug.DrawRay(transform.position, _v, Color.green, 1);
            return _v;
        }

        Debug.DrawRay(transform.position, _v, Color.red, 1);

        return null;
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
