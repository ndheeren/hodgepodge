using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Apthorpe.Chess
{
    public class CamController : MonoBehaviour
    {
        // public static ChessManager Instance { get; set; }
        public static CamController Instance { get; set; }

        private Camera cam;
        public Camera Cam
        {
            get
            {
                return cam;
            }
        }


        private void Awake()
        {
            UpholdSingleton();

            if (Instance == this)
            {
                Initialize();
            }
        }

        private void UpholdSingleton()
        {
            if (Instance == null)
            {
                //DontDestroyOnLoad(gameObject);
                Instance = this;
            }
            else if (Instance != this)
            {
                Debug.Log($"{GetType().Name} instance already exists! Destroying duplicate.");
                Destroy(gameObject);
            }
        }

        private void Initialize()
        {
            cam = GetComponent<Camera>();
        }
    }
}