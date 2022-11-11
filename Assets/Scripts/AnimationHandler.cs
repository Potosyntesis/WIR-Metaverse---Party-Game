using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationHandler : MonoBehaviour
{
    public Transform target;

    private ConfigurableJoint joint;
    private Quaternion startingRotation;

    void Start()
    {
        joint = GetComponent<ConfigurableJoint>();
        startingRotation = transform.rotation;
    }

    void FixedUpdate()
    {
        joint.SetTargetRotationLocal(target.rotation, startingRotation);
    }
}
