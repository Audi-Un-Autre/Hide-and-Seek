using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeekerCollision : MonoBehaviour{
    Seek seekScript;

    void Start(){
        seekScript = GetComponent<Seek>();
    }

    // On collision this object will end all coroutines, go into hiding, and then destroy this script to prevent further collision detection
    private void OnTriggerEnter(Collider o){
        if (o.GetComponent<Collider>().tag == "Hidden"){
            Debug.Log("Tagged a hidden player. Time to hide!");
            if (seekScript.moving){
                StopCoroutine(seekScript.movement);
            }
            o.gameObject.AddComponent<Seek>();
            seekScript.hide = true;
            Destroy(GetComponent<SeekerCollision>());
        }
    }
}
