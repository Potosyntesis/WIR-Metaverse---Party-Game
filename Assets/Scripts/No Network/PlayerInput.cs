using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
//using UnityEngine.UIElements;

public class PlayerInput : MonoBehaviour
{
    //public static PlayerInputManager Instance;

    [SerializeField] float speedMultiplier = 1f;
    [SerializeField] float characterSmooth = 0.1f;

    [SerializeField] private bool isGrounded;
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float gravity;
    [SerializeField] private float jumpHeight;
    [SerializeField] public Transform cameraTransform;

    private Vector3 velocity;
    private bool isJump = false;

    //public bool isOwner = false;

    private float turnSmoothVelocity;
    private CharacterController controller;
    private PlayerControlBindings controlBindings;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;

        controller = GetComponent<CharacterController>();

        controlBindings = new PlayerControlBindings();
        controlBindings.PlayerInput.Enable();
        controlBindings.PlayerInput.Jump.performed += Jump;
        controlBindings.PlayerInput.Movement.performed += Movement;
    }

    private void Start()
    {
        cameraTransform = Camera.main.transform;
    }

    private void Update()
    {
        //if (!isOwner) return;

        isGrounded = Physics.CheckSphere(transform.position, groundCheckDistance, groundMask);

        Vector2 inputVector = controlBindings.PlayerInput.Movement.ReadValue<Vector2>();

        float targetAngle = Mathf.Atan2(inputVector.x, inputVector.y) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, characterSmooth);
       
        transform.rotation = Quaternion.Euler(0, angle, 0);

        if (isGrounded && isJump)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2 * gravity);
            controller.Move(velocity * Time.deltaTime);
            isJump = false;
        }

        if (inputVector.magnitude >= 0.1f)
        {

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            controller.Move(moveDir * speedMultiplier * Time.deltaTime);

        }
        //Gravity force on character
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        //Debug.Log(targetAngle);
        //Debug.Log(Quaternion.Euler(0, angle, 0));
        //Debug.Log(transform.rotation);

        //PositionRotationPayload positionPayload = new PositionRotationPayload(transform.position, transform.rotation);
        //ClientManager.Instance.SendMessage<PositionRotationPayload>(Tags.Position, positionPayload, DarkRift.SendMode.Reliable);

    }

    public void Jump(InputAction.CallbackContext context)
    {
        isJump = true;
    }

    public void Movement(InputAction.CallbackContext context)
    {
        Vector2 inputVector = context.ReadValue<Vector2>();
    }
}
