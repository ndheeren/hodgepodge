using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Apthorpe.Pong
{
    public class PlayerController : MonoBehaviour
    {
        public float upperBound; // code location suspect
        public float lowerBound; // code location suspect
        public float moveSpeed;

        private void Start()
        {
            upperBound = 9f;
            lowerBound = -9f;
            moveSpeed = 0.5f;
        }

        private void FixedUpdate()
        {
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            {
                if (transform.position.y < upperBound)
                {
                    transform.Translate(0f, moveSpeed, 0f);
                }
            }
            else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            {
                if (transform.position.y > lowerBound)
                {
                    transform.Translate(0f, -moveSpeed, 0f);
                }
            }
        }
    }
}
