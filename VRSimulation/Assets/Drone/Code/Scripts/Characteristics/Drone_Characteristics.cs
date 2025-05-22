using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DroneSim
{
    public class Drone_Characteristics : MonoBehaviour
    {
        #region Variables
        [Header("Characteristics Properties")]
        public float forwardSpeed;
        public float mph;
        public float maxMPH = 110f;
        public float rbLerpSpeed = 0.01f;

        [Header("Lift Properties")]
        public float maxLiftPower = 800f;
        // Changed default curve to generate less lift at low speeds
        public AnimationCurve liftCurve = new AnimationCurve(
            new Keyframe(0f, 0f, 0f, 0f),    // Start at zero lift, zero slope
            new Keyframe(0.6f, 0.1f, 1f, 1f), // Start generating noticeable lift later (around 60% max speed)
            new Keyframe(1f, 1f, 2f, 0f)     // Ramp up quickly to full lift at max speed
        );
        
        [Header("Drag Properties")]
        public float dragFactor = 0.01f; //how much extra drag to add on as you go faster
        public float flapDragFactor = 0.005f;

        [Header("Control Properties")]
        public float pitchSpeed = 1000f;
        [Tooltip("Speed for CSV-driven roll adjustments.")]
        public float rollSpeed = 3000f; // Speed for CSV roll
        [Tooltip("Speed/Responsiveness for Manual roll control.")]
        public float manualRollSpeed = 3000f; // Separate speed for Manual roll
        public float yawSpeed = 1000f;

        private CsvInputProvider input; // Changed from Drone_Base_Input
        private Rigidbody rb;
        private float startDrag;
        private float startAngularDrag;

        private float maxMPS;
        private float normalizeMPH;


        private float angleOfAttack;
        private float pitchAngle;
        private float rollAngle;
        #endregion

        #region BuiltIn Methods
        #endregion

        #region Constants
        const float mpsToMph = 2.23694f;
        #endregion

        #region CustomMethods
        public void InitCharacteristics(Rigidbody currentRB, CsvInputProvider currentInput) // Changed parameter type
        {
            //basic Initialization
            input = currentInput;
            rb = currentRB;
            startDrag = rb.drag;
            startAngularDrag = rb.angularDrag;

            
            //Find the max Meters per second
            maxMPS = maxMPH / mpsToMph;
        }

        //This will calculate speed, lift, and drag
        public void UpdateCharacteristics()
        {
            if(rb)
            {
                //process the flight physics
                CalculateForwardSpeed();
                CalculateLift();
                CalculateDrag();

                //control the drone
                HandlePitch();
                HandleRoll();
                HandleYaw();
                //HandleBanking(); // Disabled to prevent interference with manual roll

                //handle rigidbody
                HandleRigidbodyTransform();
            }

        }

        // This calculates the speed that the plane is going and converts from mps to mph
        void CalculateForwardSpeed()
        {
            Vector3 localVelocity = transform.InverseTransformDirection(rb.velocity);
            forwardSpeed = Mathf.Max(0f, localVelocity.z);

            //Make sure forward speed doesnt get larger than maxMPS
            forwardSpeed = Mathf.Clamp(forwardSpeed, 0f, maxMPS);

            mph = forwardSpeed * mpsToMph;
            //Make sure MPH doesnt get faster than the max MPS
            mph = Mathf.Clamp(mph, 0f, maxMPH);
            //normalize MPH for the lift animation curve
            normalizeMPH = Mathf.InverseLerp(0f, maxMPH, mph);

        }

        //Lift is what makes the airplane fly in an upward direction
        void CalculateLift()
        {
            //Get the angle of attack
            angleOfAttack = Vector3.Dot(rb.velocity.normalized, transform.forward);
            angleOfAttack *= angleOfAttack;

            Vector3 liftDir = transform.up;
            //Normalized MPH makes lift much more realistic and also controlled 
            float liftPower = liftCurve.Evaluate(normalizeMPH) * maxLiftPower;

            Vector3 finalLiftForce = liftDir * liftPower * angleOfAttack;
            rb.AddForce(finalLiftForce);  //add an upward lift force to the plane

        }

        //resistance force that pulls you back as you move faster
        void CalculateDrag()
        {
            float speedDrag = forwardSpeed * dragFactor;

            //Flap Drag, helps plane have a smoother landing
            float flapDrag = input.Flaps * flapDragFactor;

            //Add all the drag together
            float finalDrag = startDrag + speedDrag + flapDrag;

            rb.drag = finalDrag;
            rb.angularDrag = startAngularDrag * forwardSpeed;
        }

        //Handle rigid body velocity
        void HandleRigidbodyTransform()
        {
            if(rb.velocity.magnitude > 1f)
            {
                Vector3 updatedVelocity = Vector3.Lerp(rb.velocity, transform.forward * forwardSpeed, forwardSpeed * angleOfAttack * Time.deltaTime * rbLerpSpeed);
                rb.velocity = updatedVelocity;

                Quaternion updatedRotation = Quaternion.Slerp(rb.rotation, Quaternion.LookRotation(rb.velocity.normalized, transform.up), Time.deltaTime * rbLerpSpeed);
                // Temporarily commented out for testing roll leveling conflict
                // rb.MoveRotation(updatedRotation); 
            }
        }

        //Pitch is the rotation of the aircraft around a side-to-side axis, aka up or down
        void HandlePitch()
        {
            Vector3 flatForward = transform.forward;
            flatForward.y = 0f;
            flatForward = flatForward.normalized;
            pitchAngle = Vector3.Angle(transform.forward, flatForward);
            Vector3 pitchTorque = input.Pitch * pitchSpeed * transform.right;
            rb.AddTorque(pitchTorque);
            
        }
        
        //Handle left and right movement of the wings
        //Directly set rotation for instant response in manual mode
        void HandleRoll()
        {
            if (input is CsvInputProvider csvInputProvider && csvInputProvider.useManualInput)
            {
                // Manual input logic
                float targetManualRollAngle = input.Roll * 45f; // Max 45 degree bank for manual (Removing negation again)
                Quaternion targetManualRotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, targetManualRollAngle);
                // Smoothly rotate towards the target manual roll angle using manualRollSpeed
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetManualRotation, manualRollSpeed * Time.deltaTime);
            }
            // Check if 'input' can be cast to CsvInputProvider for safety,
            // and that we are NOT in manual mode.
            else if (input is CsvInputProvider csvDrivenProvider && !csvDrivenProvider.useManualInput)
            {
                // CSV-driven roll based on raw CSV target
                float rawCsvTargetRoll = csvDrivenProvider.RawCsvTargetRoll;
                if (Mathf.Approximately(rawCsvTargetRoll, 0f)) {
                }
                float desiredBankAngleZ = 0f;
                const float FULL_ROLL_DEG = 35f; // Changed to 35-degree bank for full roll

                if (Mathf.Approximately(rawCsvTargetRoll, 1f))
                {
                    desiredBankAngleZ = FULL_ROLL_DEG;
                }
                else if (Mathf.Approximately(rawCsvTargetRoll, -1f))
                {
                    desiredBankAngleZ = -FULL_ROLL_DEG;
                }
                // If rawCsvTargetRoll is 0 or any other value, desiredBankAngleZ remains 0 (wings level)

                // Get current world pitch and yaw to maintain them
                // Applying roll in world Z might be too simplistic if drone has arbitrary pitch/yaw.
                // For a more robust solution, one would typically convert the desired bank angle
                // into the drone's local coordinate system or use more advanced rotation logic.
                // However, for a simple target bank while generally flying forward, this can work:
                
                Quaternion currentRotation = transform.rotation;
                // We want to achieve a world Z euler angle of desiredBankAngleZ
                // while keeping current world X (pitch) and Y (yaw)
                // This is a simplification. True aerodynamic roll is around the longitudinal axis.
                Vector3 currentEuler = currentRotation.eulerAngles;
                Quaternion targetWorldRotation = Quaternion.Euler(currentEuler.x, currentEuler.y, desiredBankAngleZ);

                // Log right before applying rotation
                float step = rollSpeed * Time.deltaTime;
                 Debug.Log($"HandleRoll (CSV): RawTgt={rawCsvTargetRoll:F2}, DesiredAngle={desiredBankAngleZ:F1}, CurrentAngle={currentEuler.z:F1}, Step={step:F3}");

                // The 'rollSpeed' variable will control how fast it banks.
                // It was originally for torque strength, so its value might need significant tuning.
                transform.rotation = Quaternion.RotateTowards(currentRotation, targetWorldRotation, step);
            }
            // If 'input' is not CsvInputProvider or some other case, the drone won't roll via this method.
        } // Closing brace for HandleRoll method

        //Handle turning the actual plane
        void HandleYaw()
        {
            Vector3 yawTorque = input.Yaw * yawSpeed * transform.up;
            rb.AddTorque(yawTorque);
        }

        //Handle plane turning when you roll
        //DISABLED - Interferes with manual roll control
        //void HandleBanking()
        //{
        //    float bankSide = Mathf.InverseLerp(-90f, 90f, rollAngle);
        //    float bankAmount = Mathf.Lerp(-1f, 1f, bankSide);
        //    Vector3 bankTorque = bankAmount * rollSpeed * transform.up;
        //    rb.AddTorque(bankTorque);
        //}
        #endregion
    }
}
