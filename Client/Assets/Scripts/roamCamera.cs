using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Used on a roaming camera
public class roamCamera : MonoBehaviour
{
    private bool alignWithMainCamera = false;

    private GameObject MainCamera;
    // Start is called before the first frame update
    void Start()
    {
        MainCamera = GameObject.FindGameObjectWithTag("MainCamera");   
    }

    // Moves the roaming camera toward the main camera if alignwithcamera is true
    void Update()
    {
        if(alignWithMainCamera) {
            if((transform.position - MainCamera.transform.position).magnitude <= 0.1f && Quaternion.Angle(transform.rotation, MainCamera.transform.rotation) <= 0.1f)
            {
                gameObject.SetActive(false);
            }
            transform.position = Vector3.MoveTowards(transform.position, MainCamera.transform.position, Time.deltaTime* 50f);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, MainCamera.transform.rotation, Time.deltaTime*100f);
        }
    }

    // Stops the animator and sets alignwithcamera to true
    public void StopRoam(){
        alignWithMainCamera = true;
        Debug.Log("stopping roam");
        GetComponent<Animator>().enabled = false;

    }
}
