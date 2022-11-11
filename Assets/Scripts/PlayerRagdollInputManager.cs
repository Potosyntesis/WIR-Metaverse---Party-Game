using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerRagdollInputManager : MonoBehaviour
{
    public static PlayerRagdollInputManager Instance;
    private Transform cameraTransform;
    private PlayerControlBindings controlBindings;
    private ConfigurableJoint BodyRotation;
    private Vector3 velocity;
    private Animator animator, aniLeft, aniRight;
    [SerializeField] public GameObject mainObject, leftObject, rightObject;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] Rigidbody root;

    [SerializeField] private float jumpHeight;
    [SerializeField] private float jumpSpeed = 1f;
    [SerializeField] float speedMultiplier = 1f;
    [SerializeField] private float distToGround = 0.6f;
    private float gravity = -9.81f;

    private Rigidbody platformRBody;
    private bool isOnPlatform;

    [SerializeField] public bool isOwner = false;
    private bool isJump = true;
    private bool jumpPressed = false;
    private bool isGrounded = false;

    private void Awake()
    {
        if (!isOwner) return;
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

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
        if (!isOwner) return;
        cameraTransform = Camera.main.transform;
    }

    private void Update()
    {
        if (!isOwner) return;

        if (Physics.Raycast(transform.position, Vector3.down, distToGround + 0.1f))
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }

        Vector2 inputVector = controlBindings.PlayerInput.Movement.ReadValue<Vector2>();

        if (isGrounded && isJump && jumpPressed)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2 * gravity);
            root.AddForce(velocity, ForceMode.Impulse);
            jumpPressed = false;
            isJump = false;
            StartCoroutine(JumpDelay());
        }
        else if (!isJump)
        {
            jumpPressed = false;
        }

        float targetAngle = Mathf.Atan2(-inputVector.x, -inputVector.y) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
        Quaternion rotationToSend = Quaternion.Euler(0f, -targetAngle, 0f);
        BodyRotation.targetRotation = Quaternion.Euler(0f, -targetAngle, 0f);

        if (inputVector.magnitude >= 0.1f)
        {
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            if (isGrounded)
            {
                root.AddForce(moveDir * -speedMultiplier, ForceMode.Force);
            }
            else
            {
                root.AddForce(moveDir * -jumpSpeed, ForceMode.Force);

            }

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

        if (isOnPlatform)
        {
            root.velocity = root.velocity + platformRBody.velocity;
        }

        //Debug.Log(rotationToSend);
        //PositionRotationPayload positionPayload = new PositionRotationPayload(transform.position, rotationToSend);
        //ClientManager.Instance.SendMessage<PositionRotationPayload>(Tags.Position, positionPayload, DarkRift.SendMode.Reliable);
    }

    public void CollisionDetected(CollisionDectection detection)
    {
        Debug.Log("Child Collision");
    }

    //private void OnCollisionEnter(Collision collision)
    //{
    //    Collider collider = collision.GetContact(0).thisCollider;
    //    if(collider.gameObject.tag == "Platform")
    //    {
    //        platformRBody = collider.gameObject.GetComponent<Rigidbody>();
    //        isOnPlatform = true;
    //    }
    //}

    //void OnCollisionExit(Collision collision)
    //{
    //    Collider collider = collision.GetContact(0).thisCollider;
    //    if (collision.gameObject.tag == "Platform")
    //    {
    //        isOnPlatform = false;
    //        platformRBody = null;
    //    }
    //}

    IEnumerator JumpDelay()
    {
        yield return new WaitForSeconds(1.25f);
        isJump = true;
        Debug.Log("Coroutine Finished");
    }

    public void Jump(InputAction.CallbackContext context)
    {
        jumpPressed = true;
    }

    public void Movement(InputAction.CallbackContext context)
    {
        Vector2 inputVector = context.ReadValue<Vector2>();
    }
}
