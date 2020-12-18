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

    private float startTime;

    public bool falling = true;

    private bool clickable = false;

    private GameObject PieceHolder;

    private Socket nwController;

    private GameController gameController;

    private Vector3 initialpos;

    private Vector3 pushpullForce;

    [SerializeField] float fallSpeed = 0.01f;

    [SerializeField] float onTouchFriction = 0.1f;

    [SerializeField] float pushPullForceMultiplier = 0.1f;

    void Awake(){
        startTime = Time.time;
    }

    void Start() {
        PieceHolder = GameObject.Find("PieceHolder");
        nwController = GameObject.Find("NetworkController").GetComponent<Socket>();
        gameController = GameObject.Find("GameController").GetComponent<GameController>();
        rigidbody = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();
        startDynFriction = collider.material.dynamicFriction;
        startStaticFriction = collider.material.staticFriction;
    }

    void FixedUpdate() {
        if(falling && (nwController.myTurn || !gameController.inGame)) {
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
        if(!checkIfClickable()){
            Debug.Log("not clickable");
            return;
        }

        screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);

        initialpos = transform.position;

        //offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
        offset = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
        collider.material.dynamicFriction = onTouchFriction;
        collider.material.staticFriction = onTouchFriction;
        pushpullForce = Vector3.zero;
        if(Input.GetKey(KeyCode.LeftControl)){
            pushpullForce = -Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z + 1f)) + offset;
        } else if(Input.GetKey(KeyCode.LeftShift)){
            pushpullForce = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z + 1f)) - offset;
        } else {
            //rigidbody.useGravity= false;
        }
    }

    void OnMouseDrag()
    {
        if(!checkIfClickable()){
            return;
        }
        if(pushpullForce != Vector3.zero){
            float initialmass = rigidbody.mass;
            rigidbody.mass = 0.1f;
            rigidbody.AddForce(pushpullForce);
            pushpullForce *= 1.01f;
            rigidbody.mass = initialmass;
            return;
        }
        Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
        Vector3 mousePosOffset = Camera.main.ScreenToWorldPoint(curScreenPoint) - offset;
        rigidbody.velocity = (initialpos + mousePosOffset - transform.position) * 500f * Time.deltaTime;
    }

    void OnMouseUp() {
        cakeslice.Outline script = GetComponent<cakeslice.Outline>();
        script.color = 2;
        script.enabled = false;

        pushpullForce = Vector3.zero;
        if(!clickable || falling || !gameController.canClickPieces){
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
        if(Time.time - startTime >= 0.05f) {
            rigidbody.useGravity = true;
            falling = false;
        }
    }

    private void OnTriggerEnter(Collider other) {
        if(tag == "DeadPiece" || (!nwController.myTurn && gameController.inGame)){
            tag = "DeadPiece";
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
                gameController.HandleLose();
            }
        }
        if(Time.time - startTime >= 0.05f) {
            rigidbody.useGravity = true;
            falling = false;
        }
    }

    public void SetClickable(bool value){
        clickable = value;
    }

    private void EndTurn(){
        tag = "DeadPiece";
        clickable = false;
        gameController.EndTurn();
    }

    private bool checkIfClickable(){
        return (clickable && !falling && (gameController.canClickPieces || !gameController.inGame));
    }
}