using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    private float forceMultiplier = 250;
    private Rigidbody rb;

    private void Awake()
    {
        RandomizeZedAngle();
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        rb.AddForce(-forceMultiplier, 0f, 0f);
    }

    private void RandomizeZedAngle()
    {
        Debug.Log("starting rotation = " + transform.rotation);
        float randomZedAngle = Random.Range(-30f, 30f);
        Debug.Log("randomZedAngle = " + randomZedAngle);

        transform.localEulerAngles = new Vector3(0f, 0f, randomZedAngle); // ref: https://answers.unity.com/questions/208447/set-rotation-of-a-transform.html
        Debug.Log("ending rotation = " + transform.rotation);
    }
}
