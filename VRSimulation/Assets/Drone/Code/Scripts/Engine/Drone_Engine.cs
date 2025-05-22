using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DroneSim
{    
    public class Drone_Engine : MonoBehaviour
    {
        #region Variables
        [Header("Engine Properties")]
        public float maxForce = 200f;
        public float maxRPM = 2550f;
        
        public AnimationCurve powerCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        
        [Header("Propellers")]
        public Drone_Propellor propellor;
        #endregion

        #region Builtin Methods
        #endregion

        #region Custom Methods
        public Vector3 CalculateForce(float throttle)
        {
            //Calculate Power
            float finalThrottle = Mathf.Clamp01(throttle);
            finalThrottle = powerCurve.Evaluate(finalThrottle);

            //calculate RPM's
            float currentRPM = finalThrottle * maxRPM;
            if(propellor)
            {
                propellor.HandlePropeller(currentRPM);
            }

            //Create Force
            float finalPower = finalThrottle * maxForce;
            Vector3 finalForce = transform.forward * finalPower;

            return finalForce;
        }
        #endregion
    }
}