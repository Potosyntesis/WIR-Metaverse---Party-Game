using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
//using UnityEngine.UIElements;

public class PlayerRagdollInput : MonoBehaviour
{
    //public static PlayerRagdollInputManager Instance;
    [SerializeField] public Transform cameraTransform;
    [SerializeField] Rigidbody root;
    private ConfigurableJoint BodyRotation;


    [SerializeField] float speedMultiplier = 1f;
    [SerializeField] float characterSmooth = 0.1f;

    [SerializeField] private bool isGrounded;
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float gravity;
    [SerializeField] private float jumpHeight;
    private Vector3 velocity;
    private bool isJump = false;
    private Animator animator, aniLeft, aniRight;
    public GameObject mainObject, leftObject, rightObject;

    //public bool isOwner = false;

    private float turnSmoothVelocity;
    private PlayerControlBindings controlBindings;


    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;

        animator = mainObject.GetComponent<Animator>();
        aniLeft = leftObject.GetComponent<Animator>();
        aniRight = rightObject.GetComponent<Animator>();

        BodyRotation = root.GetComponent<ConfigurableJoint>();

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

        if (isGrounded && isJump)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2 * gravity);
            root.AddForce(velocity, ForceMode.Impulse);

            isJump = false;
        }
        float targetAngle = Mathf.Atan2(-inputVector.x, -inputVector.y) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, characterSmooth);
        BodyRotation.targetRotation = Quaternion.Euler(0f, -targetAngle, 0f);

        if (inputVector.magnitude >= 0.1f)
        {
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            root.AddForce(moveDir * -speedMultiplier);

            animator.SetBool("IsWalk", true);
            aniLeft.SetBool("IsWalk", true);
            aniRight.SetBool("IsWalk", true);
        }
        else
        {
            animator.SetBool("IsWalk", false);
            aniLeft.SetBool("IsWalk", false);
            aniRight.SetBool("IsWalk", false);
        }
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
