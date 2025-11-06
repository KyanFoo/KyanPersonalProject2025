using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KyanPersonalProject2025.CharacterControl3
{
    public class MoveCamera : MonoBehaviour
    {
        [SerializeField] Transform cameraPosition;

        // Avoid the jitters and lags when the player is moving or looking around.
        // Move the Camera away from the player object.
        private void Update()
        {
            transform.position = cameraPosition.position;
        }
    }
}
