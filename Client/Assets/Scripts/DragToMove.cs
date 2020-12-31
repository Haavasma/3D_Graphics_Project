using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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

    public AudioClip[] collisions;

    private Vector3 initialMousePos;

    private bool clickable = false;

    private GameObject PieceHolder;

    private Socket nwController;

    private GameController gameController;

    private UIController uIController;

    private Vector3 initialpos;

    private Vector3 pushpullForce;

    private AudioSource audioSource;

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
        uIController = GameObject.Find("UIController").GetComponent<UIController>();
        rigidbody = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();
        startDynFriction = collider.material.dynamicFriction;
        startStaticFriction = collider.material.staticFriction;
        audioSource = GetComponent<AudioSource>();
    }

    void FixedUpdate() {
        if(falling && (nwController.myTurn || !gameController.inGame)) {
            rigidbody.MovePosition(new Vector3(transform.position.x, transform.position.y - fallSpeed, transform.position.z));
        }
        //gameObject.AddComponent<HingeJoint>();
    }

    private void OnMouseEnter() {
        if(!Input.GetMouseButton(0) && checkIfClickable()){
            GetComponent<cakeslice.Outline>().enabled = true;
        }
    }

    private void OnMouseExit() {
        if(!Input.GetMouseButton(0) && checkIfClickable())
        {
            GetComponent<cakeslice.Outline>().enabled = false;
        }
    }

    void OnMouseDown()
    {
        Debug.Log("clicked " + name);
        if(!checkIfClickable()){
            Debug.Log("not clickable");
            return;
        }
        GetComponent<cakeslice.Outline>().color = 1;

        screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);

        initialpos = transform.position;

        //offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
        offset = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
        collider.material.dynamicFriction = onTouchFriction;
        collider.material.staticFriction = onTouchFriction;
        pushpullForce = Vector3.zero;
        if(Input.GetKey(KeyCode.LeftShift)){
            initialMousePos = Input.mousePosition;
            pushpullForce = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z + 1f)) - offset;

            rigidbody.AddForce(pushpullForce*200.0f * rigidbody.mass);
            pushpullForce = pushpullForce*1000.0f * rigidbody.mass;
            uIController.SetPointCursor();
        }
        else
        {
            uIController.SetDragCursor();
        }
    }

    void OnMouseDrag()
    {
        if(!checkIfClickable()){
            return;
        }
        if(pushpullForce != Vector3.zero /*&& Input.GetKey(KeyCode.LeftShift)*/){
             //rigidbody.velocity = Camera.main.ScreenToWorldPoint(new Vector3(initialMousePos.x, initialMousePos.y, 
            //screenPoint.z + ((Input.mousePosition.y - initialMousePos.y) - (initialpos.z - transform.position.z))) - offset) * 0.02f;
            rigidbody.AddForce(pushpullForce*Time.deltaTime);
            pushpullForce *= 1 + Time.deltaTime;
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
        if(!clickable || falling || !gameController.GetCanClickPieces()){
            return;
        }
        uIController.SetHandCursor();
        rigidbody.useGravity = true;
        collider.material.dynamicFriction = startDynFriction;
        collider.material.staticFriction = startStaticFriction;
        tag = "MovedByPlayer";
    }

    void OnCollisionEnter(Collision collision){
        if (collision.relativeVelocity.magnitude > 2)
        {
            System.Random random = new System.Random();
            float volume = 0.05f + collision.relativeVelocity.magnitude / 50.0f;
            Debug.Log("volume: " + volume);
            audioSource.volume =  volume >= 0.5f ? 0.5f : volume;
            audioSource.PlayOneShot(collisions[random.Next(collisions.Length)]);
        }
        if(tag == "DeadPiece" || tag == "BottomPiece"){
            falling = false;
            return;
        }
        if(tag == "MovedByPlayer" && collision.transform.tag == "DeadPiece"){
            EndTurn();
            return;
        }
        tag = "Untagged";
        if(Time.time - startTime >= 0.05f) {
            rigidbody.useGravity = true;
            falling = false;
        }
    }

    private void OnTriggerEnter(Collider other) {
        if(tag == "BottomPiece"){
            return;
        }
        if(tag == "DeadPiece" || (!nwController.myTurn && gameController.inGame)){
            tag = "DeadPiece";
            return;
        }
        if(other.gameObject.tag=="Ground")
        {
            Debug.Log("hit gO with tag Ground");
            if(tag == "MovedByPlayer"|| tag == "DeadPiece") {
                EndTurn();
            }
            else if(tag=="Untagged"){
                Debug.Log("sending handleLose");
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
        return (clickable && !falling && (gameController.GetCanClickPieces()) && (!gameController.inGame || nwController.myTurn));
    }
}