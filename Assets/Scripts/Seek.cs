using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seek : MonoBehaviour
{
    
    public GameObject floor;
    public float moveSpeed = 2f, rotSpeed = 3f;
    [SerializeField] Vector3 destination;
    [SerializeField] bool destinationSet = false, checking = false, gotAngle = false;
    [SerializeField] float timer;
    [SerializeField] float randomAngle = 0f;

    void Start(){
    }

    void Update(){
        float maxCheck = 5.0f;
        
        // Set a random destination
        if (!destinationSet){
            timer = 0.0f;
            destination = SetDestination(floor);
            destinationSet = true;
        }
        
        if (destinationSet && !checking){
            MoveToDestination(destination);
        }
        
        if (destinationSet && CheckPosition(destination)){
            checking = true;
            if (!gotAngle){
                randomAngle = Random.Range(-180f, 180f);
                gotAngle = true;
            }
            else{
                Quaternion desiredRotation = Quaternion.Euler(0.0f, randomAngle, 0.0f);
                transform.rotation = Quaternion.Lerp(transform.rotation, desiredRotation, 5f * Time.deltaTime);
            }
            timer += Time.deltaTime;
            if (timer >= maxCheck){
                checking = false;
                gotAngle = false;
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

    //public void CheckAround(){
        // Seeker will look around at a random spot over 5 seconds
        
    //}
}
