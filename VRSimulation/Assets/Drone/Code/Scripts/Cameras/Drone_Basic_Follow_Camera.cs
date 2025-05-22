using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DroneSim {
    public class Drone_Basic_Follow_Camera : MonoBehaviour
    {
        #region Variables
        [Header("Basic Follow Camera Properties")]
        public Transform target;
        public float distance = 5f;
        public float height = 2f;
        public float smoothSpeed = 0.5f;

        private Vector3 smoothVelocity;
        protected float origHeight;
        #endregion

        #region BuiltIn Methods
        // Start is called before the first frame update
        void Start()
        {
            origHeight = height;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if(target) 
            {
                HandleCamera();
            }
        }
        #endregion

        #region CustomMethods
        
        //This method tells the camera what position to be at in, currently it is located behind the plane
        protected virtual void HandleCamera()
        {
            //Follow target
            Vector3 wantedPosition = target.position + (-target.forward * distance) + (Vector3.up * height);
            Debug.DrawLine(target.position, wantedPosition, Color.blue);

            transform.position = Vector3.SmoothDamp(transform.position, wantedPosition, ref smoothVelocity, smoothSpeed);
            transform.LookAt(target);

        }
        #endregion
    }
}
