using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;


public class Movement : MonoBehaviour
{
    public float maxSpeed = 50f;
    public float angularSpeed = 20f;
    public bool disableControllerInput = false;
    public bool fieldOriented = true;
    private InputAction moveAction;
    private InputAction rotateLeft; 
    private InputAction rotateRight;
    private Rigidbody rigidBody;

    public void ToggleFieldOriented()
    {
        fieldOriented = !fieldOriented;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        moveAction = InputSystem.actions.FindAction("Move", true);
        rotateLeft = InputSystem.actions.FindAction("RotateLeft", true);
        rotateRight = InputSystem.actions.FindAction("RotateRight", true);
    }

    public void RobotRelativeMove(float forward, float strafe, float rotation)
    {
        
        //var linearMovement = rigidBody.rotation * new Vector3(forward, 0, -strafe) * maxSpeed * Time.deltaTime;
        //rigidBody.MovePosition(rigidBody.position + linearMovement);
        //var angularMovment = new Vector3(0f, rotation * -angularSpeed) * angularSpeed * Time.deltaTime;
        //rigidBody.MoveRotation(rigidBody.rotation * Quaternion.Euler(angularMovment));
        rigidBody.angularVelocity = new Vector3(0f, -rotation * angularSpeed * Time.deltaTime, 0f);
        rigidBody.linearVelocity = rigidBody.rotation * new Vector3(forward, 0, strafe) * maxSpeed * Time.deltaTime;
    }

    public void FieldOrientedMove(float forward, float strafe, float rotation)
    {
        //rb.MovePosition(rb.position + direction * movementSpeed * Time.fixedDeltaTime);
        rigidBody.angularVelocity = new Vector3(0f, -rotation * angularSpeed * Time.deltaTime, 0f);
        rigidBody.linearVelocity = maxSpeed * Time.deltaTime * new Vector3(forward, 0, strafe);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (disableControllerInput == true) return;

        Vector2 moveValue = moveAction.ReadValue<Vector2>();
        bool left = rotateLeft.ReadValue<float>() == 1f;
        bool right = rotateRight.ReadValue<float>() == 1f;
        float rotation = left ? 1 : (right ? -1 : 0);

        if (fieldOriented) FieldOrientedMove(moveValue.y, -moveValue.x, rotation);
        else RobotRelativeMove(moveValue.y, -moveValue.x, rotation);
    }

    
}
