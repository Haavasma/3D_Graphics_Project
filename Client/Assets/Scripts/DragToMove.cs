using System.Collections;
using System.Collections.Generic;
using UnityEngine;


 [RequireComponent(typeof(MeshCollider))]
public class DragToMove : MonoBehaviour
{
    private Vector3 screenPoint;
    private Vector3 offset;

    private Rigidbody rigidbody;

    private bool falling = true;

    [SerializeField] float fallSpeed = 0.01f;

    void Start() {
        rigidbody = GetComponent<Rigidbody>();
    }

    void FixedUpdate() {
        if(falling) {
            rigidbody.MovePosition(new Vector3(transform.position.x, transform.position.y - fallSpeed, transform.position.z));
        }
    }

    void OnMouseDown()
    {
        screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);

        offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
        rigidbody.useGravity= false;
    }

    void OnMouseDrag()
    {
        Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);

        Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint) + offset;
        Vector3 pointerToObjectVector = curPosition - transform.position;
        pointerToObjectVector *= pointerToObjectVector.sqrMagnitude/pointerToObjectVector.magnitude;
        rigidbody.AddForce(pointerToObjectVector);
    }

    void OnMouseUp() { 
        rigidbody.useGravity = true;
    }

    void OnCollisionEnter(Collision collision){
        if(falling && Time.time >= 0.05f) {
            Debug.Log(Time.time);
            Debug.Log("Collided");
            rigidbody.useGravity = true;
            falling = false;
        }
    }

}