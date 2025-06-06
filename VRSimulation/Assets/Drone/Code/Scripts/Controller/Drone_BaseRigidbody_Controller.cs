using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DroneSim 
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(AudioSource))]
    public class Drone_BaseRigidbody_Controller : MonoBehaviour
    {
        #region 
        protected Rigidbody rb;
        protected AudioSource aSource;
        #endregion



        #region Builtin Methods
        // Start is called before the first frame update
        public virtual void Start()
        {
            rb = GetComponent<Rigidbody>();
            aSource = GetComponent<AudioSource>();
            if(aSource)
            {
                aSource.playOnAwake = false;
            }
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if(rb)
            {
                HandlePhysics();
            }
        }
        #endregion


        #region Custome Methods
        protected virtual void HandlePhysics() {}
        #endregion
    }
}