using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// script that controls the movement of the gameobject based on user input
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

    // fetches variables needed from the scene
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

    // Keeps the piece falling slowly for steady tower setup
    void FixedUpdate() {
        if(falling && (nwController.myTurn || !gameController.inGame)) {
            rigidbody.MovePosition(new Vector3(transform.position.x, transform.position.y - fallSpeed, transform.position.z));
        }
        //gameObject.AddComponent<HingeJoint>();
    }
    // highlights the piece if clickable when mouse enters gameobject
    private void OnMouseEnter() {
        if(!Input.GetMouseButton(0) && checkIfClickable()){
            GetComponent<cakeslice.Outline>().enabled = true;
            uIController.PlayHighlightSound();
        }
    }
    // removes highlight when mouse exits the gameobject
    private void OnMouseExit() {
        if(!Input.GetMouseButton(0))
        {
            GetComponent<cakeslice.Outline>().enabled = false;
        }
    }

    // Adds forces and saves vectors used for drag to move mechanic when mouse is clicked down
    void OnMouseDown()
    {
        if(!checkIfClickable()){
            return;
        }
        GetComponent<cakeslice.Outline>().color = 1;

        uIController.PlayClickSound();

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

    // Drags the gameobject toward the mouse. If left shift is held, a pushing force is added to the gameobject
    void OnMouseDrag()
    {
        if(!checkIfClickable()){
            return;
        }
        tag = "MovedByPlayer";
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
    
    // resets variable values and removes highlight when mouse button is released after clicking the gameobject
    void OnMouseUp() {
        if(!checkIfClickable()){
            return;
        }
        cakeslice.Outline script = GetComponent<cakeslice.Outline>();
        script.color = 2;
        script.enabled = false;

        pushpullForce = Vector3.zero;
        uIController.SetHandCursor();
        rigidbody.useGravity = true;
        collider.material.dynamicFriction = startDynFriction;
        collider.material.staticFriction = startStaticFriction;
        tag = "MovedByPlayer";
    }

    // Called when the gameobject collides
    void OnCollisionEnter(Collision collision){

        // Play sound if collision has high enough velocity
        if (collision.relativeVelocity.magnitude > 2)
        {
            System.Random random = new System.Random();
            float volume = 0.05f + collision.relativeVelocity.magnitude / 50.0f;
            Debug.Log("volume: " + volume);
            audioSource.volume =  volume >= 0.5f ? 0.5f : volume;
            audioSource.PlayOneShot(collisions[random.Next(collisions.Length)]);
        }
        // turns of falling if gameobject is dead or is a bottom piece
        if(tag == "DeadPiece" || tag == "BottomPiece"){
            falling = false;
            return;
        }
        // Ends turn if a piece moved by player collides with a dead piece
        if(tag == "MovedByPlayer" && collision.transform.tag == "DeadPiece"){
            EndTurn();
            return;
        }
        // resets tag
        tag = "Untagged";
        // Turns on gravity and sets falling to false if 0.05s has passed since spawn
        if(Time.time - startTime >= 0.05f) {
            rigidbody.useGravity = true;
            falling = false;
        }
    }

    // Called as the gameobject enters a trigger
    private void OnTriggerEnter(Collider other) {
        if(tag == "BottomPiece"){
            return;
        }
        // return if ingame and not the user's turn or if the piece entering a trigger is dead.
        if(tag == "DeadPiece" || (!nwController.myTurn && gameController.inGame)){
            tag = "DeadPiece";
            return;
        }
        // Ends turn if gameobject entering ground-trigger is moved by player, tells the gamecontroller to lose if piece was not moved by player
        if(other.gameObject.tag=="Ground")
        {
            Debug.Log("hit gO with tag Ground");
            if(tag == "MovedByPlayer") {
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

    // Sets clickable variable to given value
    public void SetClickable(bool value){
        clickable = value;
    }

    // tags the piece as not clickable and dead, tells gamecontroller to end the turn
    private void EndTurn(){
        tag = "DeadPiece";
        clickable = false;
        gameController.EndTurn();
    }

    // Returns true if the gameobject should be clickable
    private bool checkIfClickable(){
        return (clickable && !falling && (gameController.GetCanClickPieces()) && (!gameController.inGame || nwController.myTurn));
    }
}