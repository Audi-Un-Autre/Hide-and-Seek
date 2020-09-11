using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seek : MonoBehaviour
{
    public enum SeekerState{Wander, Chase, Hide}
    public Material mSeek, mHide;
    public Grid grid;
    public GameObject floor;
    public float moveSpeed = 2f, rotSpeed = 3f;
    public bool moving, hide;
    public List<Node> path = new List<Node>();
    public Coroutine movement;

    private SeekerState _currentState;
    private Plane[] geoPlanes;
    private Vector3 hiddenDestination;

    [SerializeField] Vector3 destination;
    [SerializeField] bool destinationSet, pathfinding, checkingDone, atSpot;
    [SerializeField] float timer;
    [SerializeField] float randomAngle = 0f;
    [SerializeField] Vector3 targetPosition;

    void Start(){
        // Stay in Scene View
        UnityEditor.SceneView.FocusWindowIfItsOpen(typeof(UnityEditor.SceneView));

        // Set initial destination for Gizmos
        destination = SetDestination(floor);
        destinationSet = false;
        checkingDone = false;
        moving = true;
        hide = false;

        gameObject.GetComponent<Renderer>().material.color = mSeek.color;
    }

    void Update(){
        // Basic State Machine
        switch(_currentState){
            case SeekerState.Wander:{
                Debug.Log("CURRENT STATE : WANDER.");
                if (CheckForHidden()){
                    StopCoroutine(movement);
                    pathfinding = true;
                    destinationSet = false;
                    moving = false;
                    checkingDone = false;
                    _currentState = SeekerState.Chase;
                } else {

                    // set destination
                    if (!destinationSet){
                        timer = 0.0f;
                        destination = SetDestination(floor);
                        Node tempDestination = grid.PointOnGrid(destination);
                        if (tempDestination.isObstacle){
                            destination = SetDestination(floor);
                        }
                        else{
                            destinationSet = true;
                            pathfinding = true;
                        }
                    }

                    // Pathfind to destination
                    if (pathfinding){
                        pathfinding = Pathfind(transform.position, destination);
                        if (!pathfinding)
                            moving = false;
                    }

                    // Trace path made
                    if (!moving){
                        movement = StartCoroutine(MoveToDestination(grid.path, _currentState));
                        moving = true;
                    }
                    
                    // At last node of path and finished checking around, reset all bools and start new destination
                    if (checkingDone){
                        StopCoroutine(movement);
                        pathfinding = false;
                        destinationSet = false;
                        checkingDone = false;
                    }
                }
                break;
            }

            // In this state, go to target until collision, once colliding this seeker will 
            case SeekerState.Chase:{
                moveSpeed = 3f;
                Debug.Log("CURRENT STATE : CHASE.");
                if (hide){
                    _currentState = SeekerState.Hide;
                } else {

                    //pathfind to hidden player
                    if (pathfinding){
                        pathfinding = Pathfind(transform.position, ChaseDestination(targetPosition));
                        if (!pathfinding){
                            moving = false;
                        }
                    }

                    //trace path made
                    if (!moving){
                        movement = StartCoroutine(MoveToDestination(grid.path, _currentState));
                        moving = true;
                    }
                }
                break;
            }

            // In this state, go to a random location, become hidden and remove the properties of a seeker
            case SeekerState.Hide:{
                moveSpeed = 3.5f;
                Debug.Log("CURRENT STATE : HIDE");
                if (!destinationSet){
                        destination = SetDestination(floor);
                        Node tempDestination = grid.PointOnGrid(destination);
                        if (tempDestination.isObstacle){
                            destination = SetDestination(floor);
                        }
                        else{
                            destinationSet = true;
                            pathfinding = true;
                        }
                    }
                    
                // Pathfind
                if (pathfinding){
                    pathfinding = Pathfind(transform.position, destination);
                    if (!pathfinding)
                        moving = false;
                }

                // Trace path made
                if (!moving){
                    movement = StartCoroutine(MoveToDestination(grid.path, _currentState));
                    moving = true;
                }

                if (atSpot){
                    gameObject.GetComponent<Renderer>().material.color = mHide.color;
                    Destroy(GetComponent<Seek>());
                }

                break;
            }
        }
    }

    public bool Pathfind(Vector3 start, Vector3 end){
        Node startNode = grid.PointOnGrid(start);
        Node endNode = grid.PointOnGrid(end);

        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode);

        while(openSet.Count > 0){
            Node currentNode = openSet[0];
            for (int i = 0; i < openSet.Count; i++){
                if (openSet[i].fCost < currentNode.fCost || openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost){
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == endNode){
                CreatePath(startNode, endNode);
                return false;
            }
            
            foreach (Node neighbour in grid.GetNeighbours(currentNode)){
                if (neighbour.isObstacle || closedSet.Contains(neighbour)){
                    continue;
                }

                int newCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                if (newCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour)){
                    neighbour.gCost = newCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, endNode);
                    neighbour.parent = currentNode;

                    if (!openSet.Contains(neighbour)){
                        openSet.Add(neighbour);
                    }
                }
            }
        }
        Debug.Log("FATAL ERROR: Destination is an obstacle.");
        return true;
    }

    private int GetDistance(Node nodeA, Node nodeB){
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridZ);
        int dstZ = Mathf.Abs(nodeA.gridZ - nodeB.gridX);

        if (dstX > dstZ)
            return 14 * dstZ + 10 * (dstX - dstZ);
        else
            return 14 * dstX + 10 * (dstZ - dstX);
    }

    public void CreatePath(Node startNode, Node endNode){
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while(currentNode != startNode){
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Reverse();

        grid.path = path;
    }

    private bool CheckForHidden(){

        // Put a sphere net around the seeker to sense hidden players
        Collider[] colliders = Physics.OverlapSphere(transform.position, 10f);
        foreach(var c in colliders){

            // If a hidden player is near, raycast towards it to see if it's blocked by a wall or if its in line of sight
            if (c.gameObject.tag == "Hidden"){
                RaycastHit hit;
                Vector3 direction = (c.transform.position - transform.position).normalized;
                if (Physics.Raycast(transform.position, direction, out hit)){
                    if (hit.transform.gameObject.tag == "Hidden"){
                        targetPosition = hit.transform.position;
                        Debug.Log("Found a hidden player.");
                        return true;
                    } else {
                        //Debug.Log("Hidden player is near, but I can't see it.");
                        return false;
                    }
                }
            }
        }
        return false;
    }

    private Vector3 SetDestination(GameObject plane){
        // Get a random point on the floor as a destination
        MeshCollider planeCollider = plane.GetComponent<MeshCollider>();
        float planeBoundsX = Random.Range(planeCollider.bounds.min.x, planeCollider.bounds.max.x);
        float planeBoundsZ = Random.Range(planeCollider.bounds.min.z, planeCollider.bounds.max.z);

        // Create vector towards random point
        Vector3 randomPoint = new Vector3(planeBoundsX, transform.position.y, planeBoundsZ);
        return randomPoint;
    }

    private Vector3 ChaseDestination(Vector3 hiddenPlayer){
        return new Vector3(hiddenPlayer.x, hiddenPlayer.y, hiddenPlayer.z);
    }

    IEnumerator MoveToDestination(List<Node> _path, SeekerState state){
        // Face and move towards point
        float maxTimer = 10.0f;
        float twoSeconds = 0.0f;
        Node lastNode = _path[_path.Count - 1];
        bool init = true;

        foreach(Node n in _path){
            Vector3 truePoint = new Vector3(n.worldPosition.x, transform.position.y, n.worldPosition.z);

            while (Vector3.Distance(transform.position, truePoint) > 0f){
                Quaternion rotation = Quaternion.LookRotation(truePoint - transform.position);
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(truePoint - transform.position), rotSpeed * Time.deltaTime);
                transform.position = Vector3.MoveTowards(transform.position, truePoint, moveSpeed * Time.deltaTime);
                yield return null;
            }

            if (state == SeekerState.Wander){
                // look around when we reach the last node
                while (n == lastNode){
                    // Initalize finding a random angle to check
                    if (init)
                        init = CheckAround(init);
                    else
                        CheckAround(init);

                    timer += Time.deltaTime;
                    // After 10 seconds, go to a new destination
                    if (timer > maxTimer){
                        checkingDone = true;
                        
                    // Look around every 2 seconds
                    } else if (Mathf.Floor(timer) % 2.0f != twoSeconds){
                        init = true;
                    }
                    yield return null;
                }
            } else if (state == SeekerState.Hide && n == lastNode){
                atSpot = true;
            }
        }
    }

    public bool CheckAround(bool init){
        // Seeker will look around at a random spot
        if (init){
                randomAngle = Random.Range(-180f, 180f);
        } else{
            Quaternion desiredRotation = Quaternion.Euler(0.0f, randomAngle, 0.0f);
            transform.rotation = Quaternion.Lerp(transform.rotation, desiredRotation, 5f * Time.deltaTime);
        }
        return false;
    }

    // OVERLAYSPHERE VISUAL DEBUGGING
    private void OnDrawGizmos(){
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, 10f);
    }
}
