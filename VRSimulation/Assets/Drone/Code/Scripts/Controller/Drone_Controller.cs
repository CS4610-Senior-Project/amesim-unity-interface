using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DroneSim {
    [RequireComponent(typeof(Drone_Characteristics))]
    public class Drone_Controller : Drone_BaseRigidbody_Controller
    {
        #region Variables
        [Header("Base Airplane Properties")]
        public CsvInputProvider input; // Changed from Drone_Base_Input
        public Drone_Characteristics characteristics;
        public Transform centerOfGravity;

        [Tooltip("Weight is in pounds")]
        public float droneWeight = 800f;

        [Header("Engines")]
        public List<Drone_Engine> engines = new List<Drone_Engine>();

        [Header("Wheels")]
        public List<Drone_Wheels> wheels = new List<Drone_Wheels>();

        [Header("Control Surfaces")]
        public List<Drone_ControlSurface> controlSurfaces = new List<Drone_ControlSurface>();
        #endregion

        #region Constants
        const float poundsToKilos = 0.453592f;
        #endregion


        #region Builtin Methods
        public override void Start()
        {
            base.Start();

            float finalMass = droneWeight * poundsToKilos;
            if(rb)
            {
                rb.mass = finalMass;
                if(centerOfGravity)
                {
                    rb.centerOfMass = centerOfGravity.localPosition;
                }

                characteristics = GetComponent<Drone_Characteristics>();
                if(characteristics)
                {
                    characteristics.InitCharacteristics(rb, input);
                }
            }

            if(wheels != null)
            {
                if(wheels.Count > 0)
                {
                    foreach (Drone_Wheels wheel in wheels)
                    {
                        wheel.InitWheel();
                    }
                }
            }
        }
        #endregion

        #region Custom Methods
        protected override void HandlePhysics()
        {
            if(input)
            {
                HandleEngines();
                HandleCharacteristics();
                HandleControlSurfaces();
                HandleWheel();
                HandleAltitude();
            }
        }
        #endregion

        void HandleEngines()
        {
            if (engines != null)
            {
                if(engines.Count > 0)
                {
                    foreach(Drone_Engine engine in engines)
                    {
                        rb.AddForce(engine.CalculateForce(input.StickyThrottle));
                    }
                }
            }
        }

        void HandleCharacteristics() 
        {
            if(characteristics)
            {
                characteristics.UpdateCharacteristics();
            }
        }

        void HandleWheel()
        {
            if(wheels.Count > 0)
            {
               foreach(Drone_Wheels wheel in wheels)
                {
                    wheel.HandleWheel(input);
                }
            }
        }

        void HandleAltitude() 
        {

        }

        void HandleControlSurfaces()
        {
            if(controlSurfaces.Count > 0)
            {
                foreach(Drone_ControlSurface controlSurface in controlSurfaces)
                {
                    controlSurface.HandleControlSurface(input);
                }
            }
        }

    }
}
