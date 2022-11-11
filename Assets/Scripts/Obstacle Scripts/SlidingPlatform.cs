using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlidingPlatform : MonoBehaviour
{
    float timer = 0f;
    float speed = 0.25f;
    int phase = 0;
    Vector3 platformPos;
    private void FixedUpdate()
    {
        timer += Time.fixedDeltaTime;
        if (timer > 1f)
        {
            phase++;
            phase %= 4;
            timer = 0f;
        }

        switch (phase)
        {
            case 0:
                platformPos = new Vector3(transform.position.x + speed * (1 - timer), transform.position.y, transform.position.z);
                transform.position = platformPos;
                break;
            case 1:
                platformPos = new Vector3(transform.position.x + -speed * timer, transform.position.y, transform.position.z);
                transform.position = platformPos;

                break;
            case 2:
                platformPos = new Vector3(transform.position.x + -speed * (1 - timer), transform.position.y, transform.position.z);
                transform.position = platformPos;

                break;
            case 3:
                platformPos = new Vector3(transform.position.x + speed * timer,transform.position.y, transform.position.z);
                transform.position = platformPos;
                break;
        }
    }
}
