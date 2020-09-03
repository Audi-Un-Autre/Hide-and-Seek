using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seek : MonoBehaviour
{
    
    public Grid grid;
    public GameObject floor;
    public float moveSpeed = 2f, rotSpeed = 3f;
    [SerializeField] Vector3 destination;
    [SerializeField] bool destinationSet, pathfinding, moving, checkingDone;
    [SerializeField] float timer;
    [SerializeField] float randomAngle = 0f;
    public List<Node> path = new List<Node>();
    Coroutine movement;

    void Start(){
        // Stay in Scene View
        UnityEditor.SceneView.FocusWindowIfItsOpen(typeof(UnityEditor.SceneView));

        // Set initial destination for Gizmos
        destination = SetDestination(floor);
        destinationSet = true;
        pathfinding = true;
        checkingDone = false;
        moving = true;
    }

    void Update(){
        
        // Set a random destination
        if (!destinationSet){
            timer = 0.0f;
            destination = SetDestination(floor);
            destinationSet = true;
            pathfinding = true;
        }

        // Pathfind
        if (pathfinding){
            pathfinding = Pathfind(transform.position, destination);
            if (!pathfinding)
                moving = false;
        }

        // Trace path made
        if (!moving){
            movement = StartCoroutine(MoveToDestination(grid.path));
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

    private Vector3 SetDestination(GameObject plane){
        // Get a random point on the floor as a destination
        MeshCollider planeCollider = plane.GetComponent<MeshCollider>();
        float planeBoundsX = Random.Range(planeCollider.bounds.min.x, planeCollider.bounds.max.x);
        float planeBoundsZ = Random.Range(planeCollider.bounds.min.z, planeCollider.bounds.max.z);

        // Create vector towards random point
        Vector3 randomPoint = new Vector3(planeBoundsX, transform.position.y, planeBoundsZ);
        return randomPoint;
    }

    IEnumerator MoveToDestination(List<Node> _path){
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
        }
    }

    private bool CheckPosition(Vector3 point){
        // If at destination, send true, else false
        if (transform.position == point)
            return true;
        else{
            return false;
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
}
