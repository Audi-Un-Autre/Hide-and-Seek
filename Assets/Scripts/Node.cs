using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node{

    [SerializeField] public bool isObstacle;
    [SerializeField] public bool isHidden; 
    [SerializeField] public Vector3 worldPosition;
    public int gCost, hCost;
    public int gridX, gridZ;
    public Node parent;

    public Node(bool _isObstacle, bool _isHidden, Vector3 _worldPosition, int _gridX, int _gridZ){
        isObstacle = _isObstacle;
        isHidden = _isHidden;
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
