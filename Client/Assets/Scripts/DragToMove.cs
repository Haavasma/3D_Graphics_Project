using System.Collections;
using System.Collections.Generic;
using UnityEngine;


 [RequireComponent(typeof(MeshCollider))]
public class DragToMove : MonoBehaviour
{
    private Vector3 screenPoint;
    private Vector3 offset;

    private Rigidbody rigidbody;

    void Start() {
        rigidbody = GetComponent<Rigidbody>();
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

}