using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerMovement : MonoBehaviour
{
    [Header ("Movement")]
    private float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;

    public float groundDrag;
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump = true;

    [Header("Crouching")]
    public float croucSpeed;
    public float crouchYcale;
    float startYScale;


    [Header ("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.Mouse3;

    [Header ("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    [Header ("Slope Handling")]
    public float maxSlopeAngle;
    public RaycastHit slopeHit;
    private bool exitingSlope;


    public TMP_Text speedText; 
    public TMP_Text horizontalSpeedText; 

   

    public Transform orientation;

    float horizontalInput;
    float verticalIInput;

    Vector3 moveDirection;

    Rigidbody rb;

    public MovemenState state;

    public enum MovemenState {
        walking,
        sprinting,
        crouching,
        air
    }

    private void Start() {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        startYScale = transform.localScale.y;
    }

    private void Update() {

        //ground check
        grounded = Physics.Raycast(transform.position,Vector3.down,playerHeight * 0.5f + 0.2f, whatIsGround);

        MyInput();
        SpeedControl();
        StateHandler();

        //handle drag
        if(grounded){
          rb.drag = groundDrag;
        }
        else{
            rb.drag = 0;
        }

        speedText.text = rb.velocity.magnitude.ToString("F2");
        horizontalSpeedText.text = new Vector3(rb.velocity.x, 0, rb.velocity.z).magnitude.ToString("F2");
    }

    private void FixedUpdate() {
        MovePlayer();
    }

    private void MyInput() { 

        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalIInput = Input.GetAxisRaw("Vertical");

        // when to Jump
        if (Input.GetKey(jumpKey) && readyToJump && grounded) {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        // start crouch 
        if(Input.GetKeyDown(crouchKey)) {
            transform.localScale = new  Vector3(transform.localScale.x,crouchYcale,transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }

        // stop crouch
        if(Input.GetKeyUp(crouchKey)) {
            transform.localScale = new  Vector3(transform.localScale.x,startYScale,transform.localScale.z);
        }
    }

    private void StateHandler(){

        // Mode - crouching
        if(grounded && Input.GetKey(crouchKey)) {
            state = MovemenState.crouching;
            moveSpeed = croucSpeed;
        }

        // Mode - Sprinting
        else if(grounded && Input.GetKey(sprintKey)) {
            state = MovemenState.sprinting;
            moveSpeed =  sprintSpeed;
        }

        // Mode - Walking;
        else if (grounded) {
            state = MovemenState.walking;
            moveSpeed =  walkSpeed;
        }

        // Mode - air
        else {
            state = MovemenState.air;
        }
    }


    private void MovePlayer() {

        // calulate movement direction
        moveDirection = orientation.forward * verticalIInput + orientation.right * horizontalInput;

        // on slope
        if(OnSlope() && !exitingSlope) {
            rb.AddForce(GetSlopeMoveDirection() * moveSpeed * 20f, ForceMode.Force);
            
            if(rb.velocity.y > 0) {
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }

        // on ground
        if(grounded) {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        // in air    
        else if (!grounded){
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);    
        }

        // turn gravity off  while on slope
        rb.useGravity = !OnSlope();
    }   

    private void SpeedControl() {

        // limit speed on slope
        if(OnSlope() && !exitingSlope) {
            if(rb.velocity.magnitude > moveSpeed)
            {
                rb.velocity = rb.velocity.normalized * moveSpeed;
            }
        }

        // limiit speed on graund or in air
        else {

            Vector3 flatVel  = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            //limit velocity if needed
            if(flatVel.magnitude > moveSpeed) {
                Vector3 limitedVel =  flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x,rb.velocity.y,limitedVel.z);
            }
        }
    }

    private void Jump() {

        exitingSlope = true;

        //reset velosity
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);


        if(state == MovemenState.crouching) {
            rb.AddForce(transform.up * jumpForce + orientation.forward * 100f, ForceMode.Impulse);

        }
        else {
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        }
    }

    private void ResetJump() {

        readyToJump = true;

        exitingSlope = false;
    }

    private bool OnSlope() {
        if(Physics.Raycast(transform.position,Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f)) {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }
        return false;
    }

    private Vector3 GetSlopeMoveDirection() {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }
}
