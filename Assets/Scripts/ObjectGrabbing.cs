using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectGrabbing : MonoBehaviour
{
    public static ObjectGrabbing Instance;
    [SerializeField] GameObject mainRoot, attachedArm, leftObject, rightObject;
    [SerializeField] bool isGrabbing = false;
    [SerializeField] float pushStrength = 5f;

    private GameObject grabbedObj;
    private PlayerControlBindings controlBindings;
    private Animator aniLeft, aniRight;
    private Rigidbody rb;
    private bool isPush = false, isAttached = false;

    void Start()
    {
        //will return if the client is not owner of game object
        if (!mainRoot.GetComponent<PlayerRagdollInputManager>().isOwner) return;

        //set the rigid body for the object to get attached to
        rb = attachedArm.GetComponent<Rigidbody>();
        aniLeft = leftObject.GetComponent<Animator>();
        aniRight = rightObject.GetComponent<Animator>();

        //creating object of unity input system
        controlBindings = new PlayerControlBindings();
        controlBindings.PlayerInput.Enable();
        controlBindings.PlayerInput.Grab.performed += Grab_Performed;
        controlBindings.PlayerInput.Grab.canceled += Grab_Canceled;
        controlBindings.PlayerInput.Push.performed += Push_Performed;
        controlBindings.PlayerInput.Push.canceled += Push_Canceled;
    }

    void FixedUpdate()
    {
        //will return if the client is not owner of game object
        if (!mainRoot.GetComponent<PlayerRagdollInputManager>().isOwner) return;

        GrabAnimation grabAnimation = new GrabAnimation(isGrabbing, isPush, 0);
        ClientManager.Instance.SendMessage<GrabAnimation>(Tags.GrabAnimation, grabAnimation, DarkRift.SendMode.Unreliable);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (ServerManager.Instance != null && other.gameObject.CompareTag("Item"))
        {
            Debug.Log("Collision");
            grabbedObj = other.gameObject;
        }

        //set the game object with item tag to attach


        ////set the game object with item tag to push
        //if (other.gameObject.CompareTag("Item") && isPush)
        //{
        //    Rigidbody otherRB = grabbedObj.GetComponent<Rigidbody>();

        //    //add force to rigid body if pressed
        //    Vector3 forceDir = other.gameObject.transform.position - transform.position;
        //    forceDir.y = 0;
        //    forceDir.Normalize();

        //    otherRB.AddForceAtPosition(forceDir * pushStrength, transform.position, ForceMode.Impulse);

        //    isPush = false;
        //}
    }

    private void OnTriggerExit(Collider other)
    {
        grabbedObj = null;
    }

    public void Grab_Performed(InputAction.CallbackContext context)
    {
        isGrabbing = true;

        aniLeft.SetBool("IsLeftUp", true);
        aniRight.SetBool("IsRightUp", true);
    }

    public void Grab_Canceled(InputAction.CallbackContext context)
    {
        isGrabbing = false;

        aniLeft.SetBool("IsLeftUp", false);
        aniRight.SetBool("IsRightUp", false);
    }

    public void Push_Performed(InputAction.CallbackContext context)
    {
        isPush = true;
    }

    public void Push_Canceled(InputAction.CallbackContext context)
    {
        isPush = false;
    }

    public GameObject GetObject()
    {
        if(grabbedObj != null)
        {
            return grabbedObj;
        }
        else
        {
            return null;
        }
    }
}
