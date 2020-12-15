using System.Collections;
using System.Collections.Generic;
using UnityEngine;

 [RequireComponent(typeof(MeshCollider))]
public class DragToMove : MonoBehaviour
{
    private Vector3 screenPoint;
    private Vector3 offset;

    private Rigidbody rigidbody;

    private Collider collider;

    private float startDynFriction;

    private float startStaticFriction;

    public bool falling = true;

    private bool clickable = false;

    private GameObject PieceHolder;

    private Vector3 initialpos;

    [SerializeField] float fallSpeed = 0.01f;

    [SerializeField] float onTouchFriction = 0.1f;

    void Start() {
        PieceHolder = GameObject.Find("PieceHolder");
        //PieceHolder.SetActive(false);
        rigidbody = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();
        startDynFriction = collider.material.dynamicFriction;
        startStaticFriction = collider.material.staticFriction;
    }

    void FixedUpdate() {
        if(falling) {
            rigidbody.MovePosition(new Vector3(transform.position.x, transform.position.y - fallSpeed, transform.position.z));
        }
        //gameObject.AddComponent<HingeJoint>();
    }

    private void OnMouseEnter() {
        if(!Input.GetMouseButton(0)){
            GetComponent<cakeslice.Outline>().enabled = true;
        }
    }

    private void OnMouseExit() {
        if(!Input.GetMouseButton(0))
        {
            GetComponent<cakeslice.Outline>().enabled = false;
        }
    }

    void OnMouseDown()
    {
        GetComponent<cakeslice.Outline>().color = 1;
        Debug.Log("clicked " + name);
        if(!clickable || falling){
            return;
        }
        screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);

        initialpos = transform.position;

        //offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
        offset = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
        rigidbody.useGravity= false;
        collider.material.dynamicFriction = onTouchFriction;
        collider.material.staticFriction = onTouchFriction;

    }

    void OnMouseDrag()
    {
        if(!clickable || falling){
            return;
        }
        /*
        Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
        Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint) + offset;  
        Vector3 pointerToObjectVector = curPosition - transform.position;
        pointerToObjectVector *= pointerToObjectVector.sqrMagnitude/pointerToObjectVector.magnitude;
        rigidbody.AddForce(pointerToObjectVector);
        */
        Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
        Vector3 mousePosOffset = Camera.main.ScreenToWorldPoint(curScreenPoint) - offset;
        rigidbody.velocity = (initialpos + mousePosOffset - transform.position) * 500f * Time.deltaTime;
    }

    void OnMouseUp() { 
        cakeslice.Outline script = GetComponent<cakeslice.Outline>();
        script.color = 2;
        script.enabled = false;

        if(!clickable || falling){
            return;
        }
        rigidbody.useGravity = true;
        collider.material.dynamicFriction = startDynFriction;
        collider.material.staticFriction = startStaticFriction;
        tag = "MovedByPlayer";
    }

    void OnCollisionEnter(Collision collision){
        if(tag == "DeadPiece"){
            return;
        }
        if(tag == "MovedByPlayer" && collision.transform.tag == "DeadPiece"){
            EndTurn();
        }
        else if(tag == "MovedByPlayer"){
            tag = "Untagged";
        }
        if(falling && Time.time >= 0.05f) {
            rigidbody.useGravity = true;
            falling = false;
        }
    }

    private void OnTriggerEnter(Collider other) {
        if(tag == "DeadPiece"){
            return;
        }
        if(other.gameObject.tag=="Ground")
        {
            if(tag == "BottomPiece"){
                return;
            }
            else if(tag == "MovedByPlayer"|| tag == "DeadPiece") {
                EndTurn();
            }
            else if(gameObject.tag!="BottomPiece"){
                GameObject.Find("GameController").GetComponent<GameController>().HandleLose();
            }
        }
    }

    public void SetClickable(bool value){
        clickable = value;
    }

    private void EndTurn(){
        tag = "DeadPiece";
        clickable = false;
        GameObject.Find("GameController").GetComponent<GameController>().EndTurn();
    }
}