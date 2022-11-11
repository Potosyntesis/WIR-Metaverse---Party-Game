using UnityEngine;
using UnityEngine.InputSystem;


public class CameraControl : MonoBehaviour
{
    public static CameraControl Instance;

    [SerializeField] public Transform player;
    [SerializeField] float camHeight = 5f;
    [SerializeField] float camDistance = -5f;

    private Transform camTransform;
    private PlayerControlBindings controlBindings;

    private Vector2 turn;

    void Awake()
    {
        //Destroy self if the same game object exsists within the same scene
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        //Set the instance to this game object
        Instance = this;

        //creating object of unity input system
        controlBindings = new PlayerControlBindings();
        controlBindings.PlayerInput.Enable();
        controlBindings.PlayerInput.Look.performed += Look;

        camTransform = transform;
    }

    void FixedUpdate()
    {
        if (player == null) return;
        //set the camera Rotation 
        Quaternion camRotation = Quaternion.Euler(turn.y, turn.x, 0);

        //setting the camera offset to view the player
        Vector3 camOffset = new Vector3(0, camHeight, camDistance);

        //set the camera to follow the player
        camTransform.position = player.position + camRotation * camOffset;

        //bounds for camera
        turn.y = Mathf.Clamp(turn.y, -40, 50);

        //set target for camera to look at
        Vector3 AdjustedPos = player.position;
        //offset for the camera
        AdjustedPos.y += 0.5f;

        camTransform.LookAt(AdjustedPos);

    }

    public void Look(InputAction.CallbackContext context)
    {
        //Taking input vector of mouse using unity input system
        Vector2 mouseVector = context.ReadValue<Vector2>();
        turn.x += mouseVector.x;
        turn.y += mouseVector.y;
    }
}
