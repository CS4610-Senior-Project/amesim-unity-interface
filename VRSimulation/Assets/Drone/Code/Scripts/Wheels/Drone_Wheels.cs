using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DroneSim 
{
    [RequireComponent(typeof(WheelCollider))]
    public class Drone_Wheels : MonoBehaviour
    {
        #region Variables
        [Header("Wheel Properties")]
        public Transform wheelGraphic;
        public bool isBraking = false;
        public float brakePower = 5f;
        public bool isSteering = false;
        public float steerAngle = 20f;
        public float steerSmoothSpeed = 2f;

        private WheelCollider WheelCol;
        private Vector3 worldPos;
        private Quaternion worldRot;
        private float finalBrakeForce;
        private float finalSteerAngle;
        #endregion

        #region Builtin Methods
        void Start()
        {
            WheelCol = GetComponent<WheelCollider>();
        }
        #endregion

        #region Custom Methods
        //Initialize the wheel torque to something very small
        public void InitWheel()
        {
            if(WheelCol)
            {
                WheelCol.motorTorque = 0.000000000001f;
            }
        }

        //This method controls the wheel graphics animation
        public void HandleWheel(CsvInputProvider input) // Changed parameter type
        {
            if(WheelCol)
            {
                //Place the Wheel in the correct position
                WheelCol.GetWorldPose(out worldPos, out worldRot);
                if(wheelGraphic)
                {
                    wheelGraphic.rotation = worldRot;
                    wheelGraphic.position = worldPos;
                }
                if (isBraking)
                {
                    if(input.Brake > 0.1f)
                    {
                        finalBrakeForce = Mathf.Lerp(finalBrakeForce, input.Brake * brakePower, Time.deltaTime);
                        WheelCol.brakeTorque = finalBrakeForce;
                    }
                    else
                    {
                        finalBrakeForce = 0f;
                        WheelCol.brakeTorque = 0f;
                        WheelCol.motorTorque = 0.000000000001f;
                    }
                }

                if (isSteering)
                {
                    finalSteerAngle = Mathf.Lerp(finalSteerAngle, -input.Yaw * steerAngle, Time.deltaTime * steerSmoothSpeed);
                    WheelCol.steerAngle = finalSteerAngle;
                }
            } 
        }
        #endregion

        
    }
}