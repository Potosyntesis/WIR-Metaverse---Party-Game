using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Balance : MonoBehaviour
{
    public Transform body;

    void Update()
    {
        transform.position = body.position;
    }
}
