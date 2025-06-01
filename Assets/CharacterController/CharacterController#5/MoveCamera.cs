using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KyanPersonalProject2025.CharacterController5
{
    public class MoveCamera : MonoBehaviour
    {
        [SerializeField] Transform cameraPosition;

        // Update is called once per frame
        void Update()
        {
            transform.position = cameraPosition.position;
        }
    }
}
