using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectGrabbing : MonoBehaviour
{
    [SerializeField] GameObject attachedArm, leftObject, rightObject;
    [SerializeField] bool alreadyGrabbing = false;
    [SerializeField] float pushStrength = 5f;

    private GameObject grabbedObj;
    private PlayerControlBindings controlBindings;
    private Animator aniLeft, aniRight;
    private Rigidbody rb;
    private bool isPush = false;

    void Start()
    {
        rb = attachedArm.GetComponent<Rigidbody>();
        aniLeft = leftObject.GetComponent<Animator>();
        aniRight = rightObject.GetComponent<Animator>();

        controlBindings = new PlayerControlBindings();
        controlBindings.PlayerInput.Enable();

        controlBindings.PlayerInput.Grab.performed += Grab_Performed;
        controlBindings.PlayerInput.Grab.canceled += Grab_Canceled;
        controlBindings.PlayerInput.Push.performed += Push_Performed;
        controlBindings.PlayerInput.Push.canceled += Push_Canceled;
    }

    void Update()
    {
        //Debug.Log(isPush);

        if (!alreadyGrabbing && grabbedObj != null)
        {
            FixedJoint fj = grabbedObj.GetComponent<FixedJoint>();
            fj.connectedBody = null;
            fj.breakForce = 0;
            Destroy(fj);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Item"))
        {
            grabbedObj = other.gameObject;
        }



        if (other.gameObject.CompareTag("Item") && isPush)
        {
            Rigidbody otherRB = grabbedObj.GetComponent<Rigidbody>();

            Vector3 forceDir = other.gameObject.transform.position - transform.position;
            forceDir.y = 0;
            forceDir.Normalize();

            otherRB.AddForceAtPosition(forceDir * pushStrength, transform.position, ForceMode.Impulse);

            isPush = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        grabbedObj = null;
    }

    public void Grab_Performed(InputAction.CallbackContext context)
    {
        alreadyGrabbing = true;

        aniLeft.SetBool("IsLeftUp", true);
        aniRight.SetBool("IsRightUp", true);

        FixedJoint fj = grabbedObj.AddComponent<FixedJoint>();
        fj.connectedBody = rb;
        fj.breakForce = 10000;
    }

    public void Grab_Canceled(InputAction.CallbackContext context)
    {
        alreadyGrabbing = false;

        aniLeft.SetBool("IsLeftUp", false);
        aniRight.SetBool("IsRightUp", false);

        FixedJoint fj = grabbedObj.GetComponent<FixedJoint>();
        fj.connectedBody = null;
        fj.breakForce = 0;
        Destroy(fj);
        grabbedObj = null;
    }

    public void Push_Performed(InputAction.CallbackContext context)
    {
        isPush = true;
    }

    public void Push_Canceled(InputAction.CallbackContext context)
    {
        isPush = false;
    }
}
