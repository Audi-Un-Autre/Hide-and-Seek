using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seek : MonoBehaviour
{
    
    public GameObject floor;
    public float moveSpeed = 2f, rotSpeed = 3f;
    [SerializeField] Vector3 destination;
    [SerializeField] bool destinationSet = false, checking = false, init = true;
    [SerializeField] float timer;
    [SerializeField] float randomAngle = 0f;

    void Start(){
    }

    void Update(){
        float maxCheck = 10.0f;
        
        // Set a random destination
        if (!destinationSet){
            timer = 0.0f;
            destination = SetDestination(floor);
            destinationSet = true;
            init = true;
        }
        
        // Move to destination
        if (destinationSet && !checking){
            MoveToDestination(destination);
        }
        
        // Check surroundings for 10 seconds and then find new destination
        if (destinationSet && CheckPosition(destination)){
            checking = true;

            // Initalize finding a random angle to check
            if (init)
                init = CheckAround(init);
            else
                CheckAround(init);

            timer += Time.deltaTime;
            // After 10 seconds, go to a new destination
            if (timer > maxCheck){
                checking = false;
                destinationSet = false;
                
            // Look around every 2 seconds
            } else if (Mathf.Floor(timer) % 2.0f != 0.0f){
                init = true;
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
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(point - transform.position), rotSpeed * Time.deltaTime);
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
