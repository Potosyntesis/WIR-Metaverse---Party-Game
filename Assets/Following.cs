using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Following : MonoBehaviour
{
    [SerializeField] GameObject gameObjectToFollow;
    void Update()
    {
        transform.position = gameObjectToFollow.transform.position;        
    }
}
