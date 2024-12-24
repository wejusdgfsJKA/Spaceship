using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(SphereCheck))]
public class PilotBT : BTree
{
    public Transform target;
    public float speed;
    public float minDistToTarget;
    public SphereCheck collisionSensor;
    public float distCheck;
    public float extendedDistCheck;
    protected Rigidbody rb;
    protected override Composite SetupTree()
    {
        collisionSensor = GetComponentInChildren<SphereCheck>();
        rb = transform.root.gameObject.GetComponent<Rigidbody>();

        localMemory.SetData<Vector3?>(BlackboardData.Destination, target.position);

        LeafNode _idle = new LeafNode("Idle",
        () =>
        {
            return NodeState.RUNNING;
        });

        root = new Selector("Root");

        AddGetToDestSubtree(root);
        root.AddChild(_idle);
        return root;
    }
    protected void AddGetToDestSubtree(Composite _parent)
    {
        Selector _sel = new Selector("Get to destination");
        _sel.AddDecorator(new Decorator("Has dest", () =>
            localMemory.GetData<Vector3?>(BlackboardData.Destination) != null)).
            MonitorValue(localMemory, BlackboardData.Destination);
        _parent.AddChild(_sel);

        //first we try to go straight towards the target
        AddStraightFlightSubtree(_sel);
        _sel.AddService(new Service("Check for obstacles", () =>
        {
            Vector3? _v = localMemory.GetData<Vector3?>(BlackboardData.Destination);
            if (_v != null)
            {
                bool _prevClear = localMemory.GetData<bool>(BlackboardData.ClearPath);
                bool _clear;
                if (_prevClear)
                {
                    _clear = !collisionSensor.PerformLineCheck((Vector3)_v, distCheck);
                }
                else
                {
                    _clear = (!collisionSensor.PerformLineCheck((Vector3)_v,
                        distCheck)) && (!collisionSensor.
                        PerformLineCheck((Vector3)_v, extendedDistCheck));
                }
                localMemory.SetData(BlackboardData.ClearPath, _clear);
                if (!_clear)
                {
                    //we need to find alternative directions
                    Vector3? _v1 = localMemory.GetData<Vector3?>(BlackboardData.AlternateDirection);
                    if (_v1 == null || collisionSensor.PerformRayCheck((Vector3)_v1))
                    {
                        localMemory.SetData(BlackboardData.AlternateDirection, collisionSensor.
                            GetValidDirection(distCheck));
                    }
                }
                if (_clear)
                {
                    Debug.DrawLine(transform.position, (Vector3)_v, Color.green, .1f);
                }
                else
                {
                    Debug.DrawLine(transform.position, (Vector3)_v, Color.red, .1f);
                }
            }
        }));

        AddObstacleAvoidanceSubtree(_sel);
    }
    protected void AddStraightFlightSubtree(Composite _parent)
    {
        localMemory.SetData(BlackboardData.ClearPath, true);
        LeafNode _goStraight = new LeafNode("Go straight",
        () =>
        {
            if (Vector3.SqrMagnitude(transform.position - localMemory.
                GetData<Vector3>(BlackboardData.Destination)) <= minDistToTarget)
            {
                localMemory.SetData<Vector3?>(BlackboardData.Destination, null);
                return NodeState.SUCCESS;
            }
            transform.root.LookAt(localMemory.GetData<Vector3>(BlackboardData.Destination));
            rb.velocity = transform.forward * speed;
            return NodeState.RUNNING;
        },
        null,
        () =>
        {
            rb.velocity = Vector3.zero;
        });
        _goStraight.AddDecorator(new Decorator("ClearPath",
            () => localMemory.GetData<bool>(BlackboardData.ClearPath))).
            MonitorValue(localMemory, BlackboardData.ClearPath);
        _parent.AddChild(_goStraight);
    }
    protected void AddObstacleAvoidanceSubtree(Composite _parent)
    {
        localMemory.SetData<Vector3?>(BlackboardData.AlternateDirection, null);
        //we have an obstacle in front of us
        LeafNode _findNewPath = new LeafNode("Find new path",
        () =>
        {
            transform.root.LookAt(transform.position + localMemory.
                GetData<Vector3>(BlackboardData.AlternateDirection));
            rb.velocity = transform.forward * speed;
            return NodeState.RUNNING;
        },
        null,
        () =>
        {
            rb.velocity = Vector3.zero;
        });
        _findNewPath.AddDecorator(new Decorator("Has altdir",
            () => localMemory.GetData<Vector3?>(BlackboardData.
            AlternateDirection) != null)).MonitorValue(localMemory,
            BlackboardData.AlternateDirection);
        _parent.AddChild(_findNewPath);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(PilotBT))]
public class PilotBTDebug : Editor
{
    private void OnSceneGUI()
    {
        PilotBT pilotBT = (PilotBT)target;

        Handles.color = Color.white;
        Handles.DrawWireArc(pilotBT.transform.position + pilotBT.
            transform.forward * pilotBT.distCheck, pilotBT.transform.forward,
            pilotBT.transform.up, 360, pilotBT.collisionSensor.radius);
        Handles.DrawWireArc(pilotBT.transform.position + pilotBT.
            transform.forward * pilotBT.distCheck, pilotBT.transform.up,
            pilotBT.transform.forward, 360, pilotBT.collisionSensor.radius);
        Handles.DrawWireArc(pilotBT.transform.position + pilotBT.
            transform.forward * pilotBT.distCheck, pilotBT.transform.right,
            pilotBT.transform.forward, 360, pilotBT.collisionSensor.radius);

        Handles.color = Color.magenta;
        Handles.DrawWireArc(pilotBT.transform.position + pilotBT.
            transform.forward * pilotBT.extendedDistCheck, pilotBT.transform.forward,
            pilotBT.transform.up, 360, pilotBT.collisionSensor.radius);
        Handles.DrawWireArc(pilotBT.transform.position + pilotBT.
            transform.forward * pilotBT.extendedDistCheck, pilotBT.transform.up,
            pilotBT.transform.forward, 360, pilotBT.collisionSensor.radius);
        Handles.DrawWireArc(pilotBT.transform.position + pilotBT.
            transform.forward * pilotBT.extendedDistCheck, pilotBT.transform.right,
            pilotBT.transform.forward, 360, pilotBT.collisionSensor.radius);
    }
}
#endif
