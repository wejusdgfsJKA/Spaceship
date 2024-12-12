using UnityEngine;

public class PilotBT : BTree
{
    public Transform target;
    public float speed;
    public float minDistToTarget;
    public SphereCheck collisionSensor;
    public float distcheck;
    protected Rigidbody rb;
    protected override Composite SetupTree()
    {
        collisionSensor = GetComponentInChildren<SphereCheck>();
        rb = transform.root.gameObject.GetComponent<Rigidbody>();

        localMemory.SetData<Vector3?>("Dest", target.position);

        LeafNode _idle = new LeafNode("Idle", () =>
        {
            Debug.Log("Idle");
            return NodeState.RUNNING;
        }, () =>
        {
            rb.velocity = Vector3.zero;
        });

        root = new Selector("Root");

        AddGetToDestSubtree(root);
        root.AddChild(_idle);
        return root;
    }
    void AddGetToDestSubtree(Composite _parent)
    {
        localMemory.SetData("ClearPath", true);
        Selector _sel1 = new Selector("Get to destination");
        _parent.AddChild(_sel1);
        _sel1.AddDecorator(new Decorator("Destination exists", () =>
        {
            return localMemory.GetData<Vector3?>("Dest") != null;
        })).MonitorValue(localMemory, "Dest");

        //if path is clear, head that way
        AddStraightFlightSubtree(_sel1);

        //find a clear path if obstacles are encountered
        AddObstacleAvoidanceSubtree(_sel1);
    }
    void AddStraightFlightSubtree(Composite _parent)
    {
        localMemory.SetData("ClearPath", true);
        Sequence _seq = new Sequence("Head to destination");
        _parent.AddService(new Service("Check path to dest", () =>
        {
            var _v = localMemory.GetData<Vector3?>("Dest");
            if (_v != null)
            {
                var b = ValidVector((Vector3)_v);
                if (b)
                {
                    Debug.DrawLine(transform.position, (Vector3)_v, Color.green, .1f);
                }
                else
                {
                    Debug.DrawLine(transform.position, (Vector3)_v, Color.red, .1f);
                }
                localMemory.SetData("ClearPath", ValidVector((Vector3)_v));
            }
        }));
        _seq.AddDecorator(new Decorator("ClearPath", () =>
        {
            return localMemory.GetData<bool>("ClearPath");
        })).MonitorValue(localMemory, "ClearPath");

        //face target
        _seq.AddChild(new LeafNode("Face destination",
        () =>
        {
            var _dot = Vector3.Dot(transform.forward, (localMemory.
                GetData<Vector3>("Dest") - transform.position).normalized);
            if (_dot == 1)
            {
                //we are facing the target
                return NodeState.SUCCESS;
            }
            Vector3 _directionToTarget = target.position - transform.position;
            // Calculate the target rotation
            Quaternion _targetRotation = Quaternion.LookRotation(_directionToTarget);
            // Smoothly rotate towards the target rotation
            transform.rotation = Quaternion.RotateTowards(transform.rotation,
                _targetRotation, 10);
            Debug.DrawRay(transform.position, transform.forward, Color.white, .1f);
            return NodeState.RUNNING;
        }));
        //move forward
        _seq.AddChild(new LeafNode("Move forward",
        () =>
        {
            if (Vector3.Distance(transform.position, localMemory.GetData<Vector3?>("Dest").Value) <= minDistToTarget + speed)
            {
                return NodeState.SUCCESS;
            }
            return NodeState.RUNNING;
        },
        () =>
        {
            rb.velocity = transform.forward * speed;
        },
        () =>
        {
            rb.velocity = Vector3.zero;
        }));
        //slow down
        _seq.AddChild(new LeafNode("Slow down",
        () =>
        {
            if (Vector3.Distance(transform.position,
                localMemory.GetData<Vector3?>("Dest").Value) <=
                minDistToTarget)
            {
                localMemory.SetData<Vector3?>("Dest", null);
                rb.velocity = Vector3.zero;
                return NodeState.SUCCESS;
            }
            float currentspd = rb.velocity.magnitude;
            float dist = Vector3.Distance(transform.position,
                localMemory.GetData<Vector3>("Dest"));
            if (currentspd > dist)
            {
                if (currentspd / 2 > dist)
                {
                    rb.velocity = transform.forward * dist;
                }
                else
                {
                    rb.velocity = transform.forward * currentspd / 2;
                }
            }
            return NodeState.RUNNING;
        },
        () =>
        {
            rb.velocity = transform.forward * speed;
        },
        () =>
        {
            rb.velocity = Vector3.zero;
        }));

        _parent.AddChild(_seq);
    }
    void AddObstacleAvoidanceSubtree(Composite _parent)
    {
        //if we reach this subtree we know there are obstacles directly
        //in front of us, so we check for valid directions

        //basically a modified version of the straight flight subtree
        //we just pick a clear direction and fly that way until the path to the
        //destination is clear
        localMemory.SetData("Direction", Vector3.zero);
        Selector _sel = new Selector("Find new path");

        //move forward
        _sel.AddChild(new LeafNode("Move forward",
        () =>
        {
            if (ValidVector(transform.position + transform.forward))
            {
                return NodeState.FAILURE;
            }
            return NodeState.RUNNING;
        },
        () =>
        {
            rb.velocity = transform.forward * speed;
        },
        () =>
        {
            rb.velocity = Vector3.zero;
        }));
        //face in the new direction
        _sel.AddChild(new LeafNode("Face destination",
        () =>
        {
            Vector3 _v = localMemory.GetData<Vector3>("Direction");
            if (_v == Vector3.zero)
            {
                return NodeState.FAILURE;
            }
            var _dot = Vector3.Dot(transform.forward, _v);
            if (_dot == 1)
            {
                //we are facing the target
                return NodeState.SUCCESS;
            }
            // Calculate the target rotation
            Quaternion targetRotation = Quaternion.LookRotation(_v);
            // Smoothly rotate towards the target rotation
            transform.rotation = Quaternion.RotateTowards(transform.rotation,
                targetRotation, 10);
            Debug.DrawRay(transform.position, transform.forward, Color.white, .1f);
            return NodeState.RUNNING;
        }, () =>
        {
            Vector3 v = GetValidDirection();
            Debug.DrawRay(transform.position, v, Color.green, 5);
            localMemory.SetData("Direction", v);
        }));

        _parent.AddChild(_sel);
    }
    protected Vector3 LocalConverter(float _x, float _y, float _z)
    {
        return (transform.forward * _z + transform.right * _x +
            transform.up * _y).normalized;
    }
    protected bool ValidVector(Vector3 _v)
    {
        if ((_v - transform.position).sqrMagnitude == 0) return false;
        return collisionSensor.PerformCheck((_v - transform.position * .9f).normalized) < 2;
    }
    protected Vector3 GetValidDirection()
    {
        //we will likely only have stuff like asteroids to worry about

        //prioritize checking forward
        for (int z = 1; z >= -1; z--)
        {
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (z == 1)
                    {
                        if (j != 0 || i != 0)
                        {
                            Vector3 _v = LocalConverter(i, j, z);
                            //avoid checking directly forward
                            if (ValidVector(_v))
                            {
                                return _v;
                            }
                        }
                    }
                    else
                    {
                        Vector3 _v = LocalConverter(i, j, z);
                        if (ValidVector(_v))
                        {
                            return _v;
                        }
                    }
                }
            }
        }
        return Vector3.zero;
    }
}
