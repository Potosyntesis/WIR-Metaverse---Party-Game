using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformOverTime : MonoBehaviour
{
    [SerializeField] private Vector3 rotationVelocity;

    // Update is called once per frame
    void Update()
    {
        if (rotationVelocity != Vector3.zero)
        {
            transform.Rotate(rotationVelocity * Time.deltaTime);
        }
    }

}
