using UnityEngine;

public class PilotBT : BTree
{
    public Transform target;
    public float speed;
    public float minDistToTarget;
    protected SphereCheck collisionSensor;
    public float distcheck;
    protected Rigidbody rb;
    protected override Composite SetupTree()
    {
        collisionSensor = GetComponentInChildren<SphereCheck>();
        rb = transform.root.gameObject.GetComponent<Rigidbody>();

        localMemory.SetData<Vector3?>("Dest", target.position);

        LeafNode _idle = new LeafNode("Idle",
        () =>
        {
            Debug.Log("Idle");
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
            localMemory.GetData<Vector3?>("Dest") != null)).
            MonitorValue(localMemory, "Dest");
        _parent.AddChild(_sel);

        //first we try to go straight towards the target
        AddStraightFlightSubtree(_sel);
        _sel.AddService(new Service("Check for obstacles", () =>
        {
            Vector3 _v = localMemory.GetData<Vector3>("Dest");
            bool _clear = !collisionSensor.PerformLineCheck(_v);
            localMemory.SetData("ClearPath", _clear);
            if (!_clear)
            {
                //we need to find alternative directions
                Vector3? _v1 = localMemory.GetData<Vector3?>("AltDir");
                if (_v1 == null || collisionSensor.PerformRayCheck((Vector3)_v1))
                {
                    localMemory.SetData("AltDir", collisionSensor.
                        GetValidDirection());
                }
            }
            if (_clear)
            {
                Debug.DrawLine(transform.position, _v, Color.green, .1f);
            }
            else
            {
                Debug.DrawLine(transform.position, _v, Color.red, .1f);
            }
        }));

        AddObstacleAvoidanceSubtree(_sel);
    }
    protected void AddStraightFlightSubtree(Composite _parent)
    {
        localMemory.SetData("ClearPath", true);
        LeafNode _goStraight = new LeafNode("Go straight",
        () =>
        {
            if (Vector3.SqrMagnitude(transform.position - localMemory.
                GetData<Vector3>("Dest")) <= minDistToTarget)
            {
                return NodeState.SUCCESS;
            }
            transform.root.LookAt(localMemory.GetData<Vector3>("Dest"));
            rb.velocity = transform.forward * speed;
            return NodeState.RUNNING;
        },
        null,
        () =>
        {
            rb.velocity = Vector3.zero;
        });
        _goStraight.AddDecorator(new Decorator("ClearPath",
            () => localMemory.GetData<bool>("ClearPath"))).
            MonitorValue(localMemory, "ClearPath");
        _parent.AddChild(_goStraight);
    }
    protected void AddObstacleAvoidanceSubtree(Composite _parent)
    {
        localMemory.SetData<Vector3?>("AltDir", null);
        //we have an obstacle in front of us
        LeafNode _findNewPath = new LeafNode("Find new path",
        () =>
        {
            transform.root.LookAt(transform.position + localMemory.
                GetData<Vector3>("AltDir"));
            rb.velocity = transform.forward * speed;
            return NodeState.RUNNING;
        },
        null,
        () =>
        {
            rb.velocity = Vector3.zero;
        });
        _findNewPath.AddDecorator(new Decorator("Has altdir",
            () => localMemory.GetData<Vector3?>("AltDir") != null));
        _parent.AddChild(_findNewPath);
    }
}
