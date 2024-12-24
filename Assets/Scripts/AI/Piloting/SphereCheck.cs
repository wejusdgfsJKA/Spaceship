using System;
using System.Collections.Generic;
using UnityEngine;
public class SphereCheck : MonoBehaviour
{
    protected HashSet<Collider> exceptions = new();
    [SerializeField] protected LayerMask obstructionMask;
    protected System.Random random;
    [field: SerializeField] public float radius { get; protected set; }
    protected void Awake()
    {
        //this method adds to the set of exceptions all colliders of this object
        //and its children
        random = new System.Random(DateTime.Now.Second);
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
        Collider[] _buffer = new Collider[10];
        int _l = Physics.OverlapSphereNonAlloc(transform.position + _direction,
            radius, _buffer, obstructionMask);
        if (_l == 0)
        {
            return false;
        }
        int _ll = _l;
        //Debug.DrawLine(transform.position, transform.position + _direction, Color.magenta, 1);
        for (int i = 0; i < _ll; i++)
        {
            if (exceptions.Contains(_buffer[i]))
            {
                //ignore all contacts in the exceptions set
                _l--;
                if (_l <= 0)
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
    public bool PerformLineCheck(Vector3 _endpoint, float _multiplier)
    {
        return PerformRayCheck((_endpoint - transform.position).normalized * _multiplier);
    }
    protected Vector3 LocalConverter(float _x, float _y, float _z)
    {
        return (transform.forward * _z + transform.right * _x +
            transform.up * _y).normalized;
    }
    public Vector3? GetValidDirection(float _multiplier = 1)
    {
        //we will likely only have simple stuff like the
        //occasional asteroid to worry about

        //first check forward, then to the sides, then to the rear
        int _z = 1;
        Vector3 _v;
        var a = GetOne();
        var b = GetOne();
        int i = -a;
        int j;
        while (i != a)
        {
            i += a;
            j = -b;
            while (j != b)
            {
                j += b;
                if (j != 0 || i != 0)
                {
                    //avoid checking directly forward
                    _v = LocalConverter(i, j, _z) * _multiplier;
                    if (!PerformRayCheck(_v))
                    {
                        Debug.DrawRay(transform.position, _v, Color.green, 1);
                        return _v;
                    }
                    Debug.DrawRay(transform.position, _v, Color.red, 1);
                }
            }
        }

        i = -a;
        _z = 0;
        while (i != a)
        {
            i += a;
            j = -b;
            while (j != b)
            {
                j += b;
                if (j != 0 || i != 0)
                {
                    //avoid checking for the zero vector
                    _v = LocalConverter(i, j, _z) * _multiplier;
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
        i = -a;
        while (i != a)
        {
            i += a;
            j = -b;
            while (j != b)
            {
                j += b;
                if (j != 0 || i != 0)
                {
                    //avoid checking directly behind us
                    _v = LocalConverter(i, j, _z) * _multiplier;
                    if (!PerformRayCheck(_v))
                    {
                        Debug.DrawRay(transform.position, _v, Color.green, 1);
                        return _v;
                    }
                    Debug.DrawRay(transform.position, _v, Color.red, 1);
                }
            }
        }
        _v = LocalConverter(0, 0, -1) * _multiplier;
        //if we have exhausted all other options try to go directly bakwards
        /*if (!PerformRayCheck(_v))
        {
            Debug.DrawRay(transform.position, _v, Color.green, 1);
            return _v;
        }

        Debug.DrawRay(transform.position, _v, Color.red, 1);
        */
        return null;
    }
    protected int GetOne()
    {
        return random.Next(0, 2) * 2 - 1;
    }
}