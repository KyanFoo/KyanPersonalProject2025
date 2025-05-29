using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KyanPersonalProject2025.CharacterController1
{
    public class MoveCamera : MonoBehaviour
    {
        public Transform cameraPosition;

        // Update is called once per frame
        private void Update()
        {
            transform.position = cameraPosition.position;

        }
    }
}
