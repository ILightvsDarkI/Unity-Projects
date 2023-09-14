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
    public float wallrunSpeed;
    public float slideSpeed;

    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;


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
    public bool grounded;

    [Header ("Slope Handling")]
    
    public float groundRayLen = 0.3f;
    public float onSlopeRayLen = 0.3f;
    public float maxSlopeAngle;
    public RaycastHit slopeHit;
    private bool exitingSlope;
    public bool onSlope;


    public TMP_Text speedText; 
    public TMP_Text horizontalSpeedText; 

   

    public Transform orientation;

    float scrollValue;
    float horizontalInput;
    float verticalIInput;

    Vector3 moveDirection;

    Rigidbody rb;

    public MovemenState state;
    
    public enum MovemenState {
        walking,
        sprinting,
        crouching,
        wallrunning,
        sliding,
        air
    }

    public bool sliding;
    public bool wallrunning;

    private void Start() {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        startYScale = transform.localScale.y;
    }

    private void Update() {

        //ground check
        float rayLen = playerHeight * 0.5f + groundRayLen;
        grounded = Physics.Raycast(transform.position,Vector3.down, rayLen, whatIsGround);

        
        Debug.DrawRay(transform.position, Vector3.down * rayLen, Color.blue, 2);


        MyInput();
        SpeedControl();
        StateHandler();

        //handle drag
        if(grounded) {
            rb.drag = groundDrag;
        }
        else {
            rb.drag = 0;
        }

        speedText.text = rb.velocity.magnitude.ToString("F2");
        horizontalSpeedText.text = new Vector3(rb.velocity.x, 0, rb.velocity.z).magnitude.ToString("F2");
    }

    private void FixedUpdate() {
        onSlope = OnSlope();
        MovePlayer();
    }

    private void MyInput() { 

        scrollValue = Input.GetAxis("Mouse ScrollWheel");
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

      //  Mode - WallRunning
        if(wallrunning) {
            state = MovemenState.wallrunning;
            
            moveSpeed =  wallrunSpeed;
        //  desiredMoveSpeed =  wallrunSpeed; 
        }

        // Mode - Sliding
        if(sliding) {
            state = MovemenState.sliding;

            if(OnSlope() && rb.velocity.y < 0.1f) {
                desiredMoveSpeed = moveSpeed;
            }
            else {
                desiredMoveSpeed =  sprintSpeed;
            }
        }

        // Mode - crouching
        else if(grounded && Input.GetKey(crouchKey)) {
            state = MovemenState.crouching;
            desiredMoveSpeed = croucSpeed;
        }

        // Mode - Sprinting
        else if(grounded && Input.GetKey(sprintKey)) {
            state = MovemenState.sprinting;
            desiredMoveSpeed =  sprintSpeed;
        }

        // Mode - Walking;
        else if (grounded) {
            state = MovemenState.walking;
            desiredMoveSpeed =  walkSpeed;
        }

        // Mode - air
        if (!grounded){
            state = MovemenState.air;
        }

        // check if desiredMoveSpeed has changer drastically
        // if(Mathf.Ads(desiredMoveSpeed - lastDesiredMoveSpeed) > 4f && moveSpeed != 0f) {
        //     StopAllCoroutines();
        //     StartCorontines(SmoothlyLerpMoveSpeed());
        // }
         else {
            moveSpeed = desiredMoveSpeed;
        }

        lastDesiredMoveSpeed = desiredMoveSpeed;
    }


    private void MovePlayer() {

        // calulate movement direction
        moveDirection = orientation.forward * verticalIInput + orientation.right * horizontalInput;

        // on slope
        if(OnSlope() && !exitingSlope) {
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * moveSpeed * 20f, ForceMode.Force);
            
            // if(rb.velocity.y >= 0) {
            //     rb.AddForce(Vector3.down * 800f, ForceMode.Force);
            // }
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
        if (!wallrunning) {
            rb.useGravity = !grounded;
        }
    }   

    private void SpeedControl() {

        // limit speed on slope (1)
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
            if(flatVel.magnitude > moveSpeed){
                Vector3 limitedVel =  flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x,rb.velocity.y,limitedVel.z);
            }
        }
    }

    private void Jump() {

        exitingSlope = true;

        //reset velosity
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump() {

        readyToJump = true;

        exitingSlope = false;
    }

    public bool OnSlope() {
        float rayLen = playerHeight * 0.5f + onSlopeRayLen;
     //   Debug.DrawRay(transform.position, Vector3.down * rayLen, Color.red, 2);

        if(Physics.Raycast(transform.position,Vector3.down, out slopeHit, rayLen)) {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
        
            return angle <= maxSlopeAngle && angle != 0;
        }
        return false;
    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction) {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }
}
