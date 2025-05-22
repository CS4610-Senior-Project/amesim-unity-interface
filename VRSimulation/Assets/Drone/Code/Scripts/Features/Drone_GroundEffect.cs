using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DroneSim
{
    public class Drone_GroundEffect : MonoBehaviour
    {
        #region Variables
        public float maxGroundDistance = 3f;
        public float liftForce = 100f;
        public float maxSpeed = 15f;

        private Rigidbody rb;
        #endregion

        #region BuiltIn Methods
        // Start is called before the first frame update
        void Start()
        {
            rb = GetComponent<Rigidbody>();
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if(rb)
            {
                HandleGroundEffect();
            }
        }
        #endregion

        #region CustomMethods
        //This code runs if you are landing or taking off from the runway or "ground"
        protected virtual void HandleGroundEffect()
        {
            RaycastHit hit;
            if(Physics.Raycast(transform.position, Vector3.down, out hit))
            {
                if(hit.transform.tag == "ground" && hit.distance < maxGroundDistance)
                {
                    float currentSpeed = rb.velocity.magnitude;
                    float normalizedSpeed = currentSpeed / maxSpeed;

                    float distance = maxGroundDistance - hit.distance;
                    float finalForce = liftForce * distance * normalizedSpeed;
                    rb.AddForce(Vector3.up * finalForce);
                }
            }
        }
        #endregion
    }
}