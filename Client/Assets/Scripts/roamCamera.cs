using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class roamCamera : MonoBehaviour
{
    private bool alignWithMainCamera = false;

    private GameObject MainCamera;
    // Start is called before the first frame update
    void Start()
    {
        MainCamera = GameObject.FindGameObjectWithTag("MainCamera");   
    }

    // Update is called once per frame
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

    public void StopRoam(){
        alignWithMainCamera = true;
        Debug.Log("stopping roam");
        GetComponent<Animator>().enabled = false;

    }
}
