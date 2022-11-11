using System;
using System.Collections;
using System.Collections.Generic;
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
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        controlBindings = new PlayerControlBindings();
        controlBindings.PlayerInput.Enable();
        controlBindings.PlayerInput.Look.performed += Look;

        camTransform = transform;
    }

    void FixedUpdate()
    {
        Quaternion camRotation = Quaternion.Euler(turn.y, turn.x, 0);

        Vector3 camOffset = new Vector3(0, camHeight, camDistance);

        camTransform.position = player.position + camRotation * camOffset;

        //bounds for camera
        turn.y = Mathf.Clamp(turn.y, -40, 50);

        Vector3 AdjustedPos = player.position;
        AdjustedPos.y += 0.5f;

        camTransform.LookAt(AdjustedPos);
    }

    public void Look(InputAction.CallbackContext context)
    {
        Vector2 mouseVector = context.ReadValue<Vector2>();
        turn.x += mouseVector.x;
        turn.y += mouseVector.y;
    }
}
