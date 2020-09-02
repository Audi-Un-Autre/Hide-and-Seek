using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node{

    [SerializeField] public bool isObstacle;
    [SerializeField] public Vector3 worldPosition;

    public Node(bool _isObstacle, Vector3 _worldPosition){
        isObstacle = _isObstacle;
        worldPosition = _worldPosition;
    }
}
