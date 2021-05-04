using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TD.Server {
    public class MovementBehaviour : MonoBehaviour {
        public float range;
        public float speed;

        public Transform target;

        Rigidbody rb;

        public bool canMove = true;

        float slowTimer;
        float movementModifier = 1;

        private void Start() {
            rb = GetComponent<Rigidbody>();
        }

        private void FixedUpdate() {
            if (target == null || !canMove) {
                rb.velocity = Vector3.zero;
                return;
            }

            if (slowTimer <= 0)
                movementModifier = 1;

            RotateToTarget();

            rb.velocity = transform.forward * speed * Time.fixedDeltaTime * movementModifier;
        }

        void RotateToTarget() {
            Vector3 dir = target.transform.position - transform.position;

            float angle = Mathf.Lerp(transform.localEulerAngles.y, Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg, 90 * Time.deltaTime);

            transform.localEulerAngles = new Vector3(0, angle, 0);
        }

        public void AddSlow(int slowAmount, float time = 1.5f) {
            movementModifier = 1 * (1 - slowAmount / 100f);
            slowTimer = time;
        }
    }
}