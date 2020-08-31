using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seek : MonoBehaviour
{
    
    public GameObject floor;
    public float moveSpeed = 2f, rotSpeed = 3f;
    [SerializeField] Vector3 destination;
    [SerializeField] bool destinationSet = false, checking = false;
    [SerializeField] float time;

    void Start(){
    }

    void Update(){
        float maxCheck = 5.0f;

        if (!destinationSet){
            time = 0.0f;
            destination = SetDestination(floor);
            destinationSet = true;
        } else if (destinationSet){
            MoveToDestination(destination);
            if (CheckPosition(destination)){
                CheckAround();
                time += Time.deltaTime;
                if (time >= maxCheck)
                    destinationSet = false;
            }
        }
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

    private void MoveToDestination(Vector3 point){
        // Face and move towards point
        Quaternion rotation = Quaternion.LookRotation(point - transform.position);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, rotSpeed * Time.deltaTime);
        transform.position = Vector3.MoveTowards(transform.position, point, moveSpeed * Time.deltaTime);
    }

    private bool CheckPosition(Vector3 point){
        // If at destination, send true, else false
        if (transform.position == point)
            return true;
        else{
            return false;
        }
    }

    public void CheckAround(){
        // Seeker will look around at a random spot over 5 seconds
        float tempRotSpeed = 2f;
        float timer = 0f;
        float maxTimer = 4.5f;
        Quaternion endRot = Quaternion.Euler(new Vector3(0.0f, Random.Range(-180f, 180f), 0.0f));
        //Vector3 newRot = new Vector3(0.0f, Random.Range(-180.0f, 180.0f), 0.0f);

        //timer += Time.deltaTime;
        //if (timer > maxTimer)
            //endRot = Quaternion.Euler(newRot);

        transform.rotation = Quaternion.Slerp(transform.rotation, endRot, tempRotSpeed * Time.deltaTime);
    }
}
