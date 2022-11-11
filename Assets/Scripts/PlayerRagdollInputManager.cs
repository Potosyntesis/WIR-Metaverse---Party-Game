using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerRagdollInputManager : MonoBehaviour
{
    public static PlayerRagdollInputManager Instance;
    private Transform cameraTransform;
    private PlayerControlBindings controlBindings;
    private ConfigurableJoint BodyRotation;
    public Animator animator, aniLeft, aniRight;
    private Vector3 velocity;
    [SerializeField] public GameObject mainObject, leftObject, rightObject;
    [SerializeField] Rigidbody root;
    [SerializeField] private LayerMask groundMask;

    [SerializeField] private float jumpHeight, jumpSpeed = 1f, speedMultiplier = 1f, distToGround = 0.6f;
    private float gravity = -9.81f;

    [SerializeField] public bool isOwner = false;
    private bool isJump = true, jumpPressed = false, isGrounded = false, isWalkNetwork = false;

    private void Awake()
    {
        //Hide cursor when game is running
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Start()
    {
        //will return if the client is not owner of game object
        if (!isOwner) return;
        //retrieving compontents
        animator = mainObject.GetComponent<Animator>();
        aniLeft = leftObject.GetComponent<Animator>();
        aniRight = rightObject.GetComponent<Animator>();

        BodyRotation = root.GetComponent<ConfigurableJoint>();

        //creating object of unity input system
        controlBindings = new PlayerControlBindings();
        controlBindings.PlayerInput.Enable();
        controlBindings.PlayerInput.Jump.performed += Jump;
        
        //set camera tranform to this game object
        cameraTransform = Camera.main.transform;
    }

    private void FixedUpdate()
    {
        //will return if the client is not owner of game object
        if (!isOwner) return;
        //using raycast to do ground check
        if (Physics.Raycast(transform.position, Vector3.down, distToGround + 0.1f))
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }

        //Using unity input system for input controls 
        Vector2 inputVector = controlBindings.PlayerInput.Movement.ReadValue<Vector2>();

        //Jump check
        if (isGrounded && isJump && jumpPressed)
        {
            //adding force to player to jump
            velocity.y = Mathf.Sqrt(jumpHeight * -2 * gravity);
            root.AddForce(velocity, ForceMode.Impulse);
            jumpPressed = false;
            isJump = false;
            //Coroutine to add cooldown between each jump
            StartCoroutine(JumpDelay());
        }
        else if (!isJump)
        {
            jumpPressed = false;
        }

        //setting player rotation to look where the camera is facing
        float targetAngle = Mathf.Atan2(-inputVector.x, -inputVector.y) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
        BodyRotation.targetRotation = Quaternion.Euler(0f, -targetAngle, 0f);

        //if input key is pressed
        if (inputVector.magnitude >= 0.1f)
        {
            //set the variable to move the player where the camera is facing
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            if (isGrounded)
            {
                //add force to move the player when on the ground
                root.AddForce(moveDir * -speedMultiplier, ForceMode.Force);
            }
            else
            {
                //add force to move the player when in the air
                root.AddForce(moveDir * -jumpSpeed, ForceMode.Force);
            }
            //set the animations to play
            animator.SetBool("IsWalk", true);
            aniLeft.SetBool("IsWalk", true);
            aniRight.SetBool("IsWalk", true);
            //variable to send over the network
            isWalkNetwork = true;
        }
        else
        {
            animator.SetBool("IsWalk", false);
            aniLeft.SetBool("IsWalk", false);
            aniRight.SetBool("IsWalk", false);
            isWalkNetwork = false;
        }
        //assigning data to send over the network
        PositionRotationPayload positionPayload = new PositionRotationPayload(transform.position, BodyRotation.targetRotation, 0, isWalkNetwork);
        ClientManager.Instance.SendMessage<PositionRotationPayload>(Tags.Position, positionPayload, DarkRift.SendMode.Reliable);
    }

    IEnumerator JumpDelay()
    {
        //coroutine to add jump cooldown
        yield return new WaitForSeconds(1.25f);
        isJump = true;
    }

    public void Jump(InputAction.CallbackContext context)
    {
        //Jump button pressed
        jumpPressed = true;
    }
}
