using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KyanPersonalProject2025.CharacterController6
{
    public class OrientationHelper : MonoBehaviour
    {
        public Transform player;
        public LayerMask groundMask;
        public float rayLength = 5f;

        public float rotSpeed = 2f;
        public float rot;

        private void FixedUpdate()
        {
            bool hit = Physics.Raycast(transform.position, Vector3.down, out RaycastHit info, rayLength, groundMask);
            if (hit)
            {
                transform.up = info.normal;
            }

            transform.Rotate(Vector3.up, player.transform.eulerAngles.y - transform.eulerAngles.y, Space.Self);

            Debug.DrawLine(transform.position, transform.position + transform.forward * 5f, Color.red);
        }
    }
}
