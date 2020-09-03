using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node{

    [SerializeField] public bool isObstacle;
    [SerializeField] public Vector3 worldPosition;
    public int gCost, hCost;
    public int gridX, gridZ;
    public Node parent;

    public Node(bool _isObstacle, Vector3 _worldPosition, int _gridX, int _gridZ){
        isObstacle = _isObstacle;
        worldPosition = _worldPosition;
        gridX = _gridX;
        gridZ = _gridZ;
    }

    public int fCost{
        get {
            return gCost + hCost;
            }
    }
}
