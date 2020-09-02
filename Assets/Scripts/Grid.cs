using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour{

    public Transform seeker;
    public GameObject plane;
    public LayerMask ObstacleMask;
    [SerializeField] Vector3 gridSize;
    [SerializeField] float nodeRadius;
    Node[,] grid;

    [SerializeField] float nodeDiameter;
    int gridX, gridZ;

    void Start(){
        gridSize = plane.GetComponent<MeshCollider>().bounds.size;
        nodeDiameter = 1f;
        nodeRadius = nodeDiameter / 2f;

        gridX = Mathf.RoundToInt(gridSize.x / nodeDiameter);
        gridZ = Mathf.RoundToInt(gridSize.z / nodeDiameter);
        CreateGrid();
    }

    void CreateGrid(){
        grid = new Node[gridX, gridZ];
        
        // Get bottom left of grid by subtraction this object's position of 0,0,0 from Vector3.right (gridSize.x/2,0,0) and Vector3.forward (0,0,gridSize.y/2)
        Vector3 gridBottomLeft = transform.position - Vector3.right * gridSize.x / 2 - Vector3.forward * gridSize.z / 2;

        for (int x = 0; x < gridX; x++){
            for (int z = 0; z < gridZ; z++){
                // Point on grid that represents a 'node'. From the bottom left of the grid we move 1 unit to the right per loop and then 1 unit up after hitting x limit.
                Vector3 worldPoint = gridBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (z * nodeDiameter + nodeRadius);
                bool isObstacle = Physics.CheckSphere(worldPoint, nodeRadius, ObstacleMask);
                grid[x,z] = new Node(isObstacle, worldPoint);
            }
        }
    }

    public Node PointOnGrid(Vector3 point){
        float percentX = (point.x + gridSize.x/2) / gridSize.x;
        float percentZ = (point.z + gridSize.z/2) / gridSize.z;

        percentX = Mathf.Clamp01(percentX);
        percentZ = Mathf.Clamp01(percentZ);

        int x = Mathf.RoundToInt((gridX - 1) * percentX);
        int z = Mathf.RoundToInt((gridZ - 1) * percentZ);
        return grid[x,z];
    }

    void OnDrawGizmos(){
        Gizmos.DrawWireCube(transform.position, new Vector3(gridSize.x, 1, gridSize.z));
        if (grid != null){
            Node player = PointOnGrid(seeker.position);
            foreach (Node n in grid){
                Gizmos.color = n.isObstacle?Color.red:Color.white;
                if (player == n)
                    Gizmos.color = Color.blue;
                Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter - .1f));
            }
        }
    }
}
