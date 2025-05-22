using UnityEngine;

namespace DroneSim
{
    // Basic PID controller implementation
    [System.Serializable] // Added this attribute to make it visible in Inspector
    public class PidController
    {
        public float Kp = 1f; // Proportional gain
        public float Ki = 0.1f; // Integral gain
        public float Kd = 0.01f; // Derivative gain

        private float integral = 0f;
        private float lastError = 0f;

        public void Reset()
        {
            integral = 0f;
            lastError = 0f;
        }

        public float Update(float targetValue, float currentValue, float deltaTime)
        {
            float error = targetValue - currentValue;

            // Handle angle wrapping for shortest path (e.g., -180 to 180 degrees)
            if (Mathf.Abs(error) > 180f)
            {
                error = Mathf.Sign(error) * (Mathf.Abs(error) - 360f);
            }

            integral += error * deltaTime;
            float derivative = (error - lastError) / deltaTime;
            lastError = error;

            float output = Kp * error + Ki * integral + Kd * derivative;

            // Clamp output to typical control range (-1 to 1)
            return Mathf.Clamp(output, -1f, 1f);
        }
    }

    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CsvReader))] // Expects CsvReader on the same GameObject or accessible
    public class OrientationPidController : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private CsvReader csvReader;
        private Rigidbody rb;

        [Header("PID Settings")]
        public PidController pitchPid = new PidController { Kp = 2f, Ki = 0.5f, Kd = 0.1f };
        public PidController rollPid = new PidController { Kp = 2f, Ki = 0.5f, Kd = 0.1f };
        // Yaw PID is not needed as per instructions
        public float targetAngleMultiplier = 30f; // Scale factor for normalized targets

        [Header("Output")]
        [SerializeField] [Range(-1f, 1f)] private float pitchOutput = 0f;
        [SerializeField] [Range(-1f, 1f)] private float rollOutput = 0f;
        // Yaw output is always 0

        public float PitchOutput => pitchOutput;
        public float RollOutput => rollOutput;
        public float YawOutput => 0f; // Always 0 as per instructions

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            if (csvReader == null)
            {
                csvReader = GetComponent<CsvReader>(); // Try to get it if not assigned
            }

            if (csvReader == null)
            {
                Debug.LogError("CsvReader dependency not found on OrientationPidController!");
                enabled = false; // Disable script if dependency is missing
            }
        }

        void OnEnable()
        {
            // Reset PID controllers when enabled
            pitchPid.Reset();
            rollPid.Reset();
        }

        void FixedUpdate() // Use FixedUpdate for physics-related calculations
        {
            if (!csvReader || !csvReader.IsInitialized())
            {
                pitchOutput = 0f;
                rollOutput = 0f;
                return; // Wait for CsvReader to be ready
            }

            // Get target data for the current time
            TargetData currentTarget = csvReader.GetTargetData(Time.time); // Using Time.time as simulation time

            // Get current orientation (Euler angles)
            Vector3 currentEulerAngles = GetCurrentEulerAngles();

            // Update PID controllers
            // Note: Target angles from CSV might need conversion if they aren't in the same range/coordinate system as Unity's Euler angles.
            // Assuming target angles from CSV are normalized (-1 to 1), scale them to degrees
            float scaledTargetPitch = currentTarget.TargetPitch * targetAngleMultiplier;
            float scaledTargetRoll = currentTarget.TargetRoll * targetAngleMultiplier;

            pitchOutput = pitchPid.Update(scaledTargetPitch, currentEulerAngles.x, Time.fixedDeltaTime);
            rollOutput = rollPid.Update(scaledTargetRoll, currentEulerAngles.z, Time.fixedDeltaTime); // Unity uses Z for Roll in Euler angles

            // --- DEBUG LOGGING ---
            if (Time.frameCount % 60 == 0) // Log roughly once per second
            {
                Debug.Log($"Time: {Time.time:F2}, Target(P:{scaledTargetPitch:F2}, R:{scaledTargetRoll:F2}), Current(P:{currentEulerAngles.x:F2}, R:{currentEulerAngles.z:F2}), Output(P:{pitchOutput:F2}, R:{rollOutput:F2})"); // Log scaled targets
            }
            // --- END DEBUG LOGGING ---
        }

        // Helper to get Euler angles in a consistent -180 to 180 range
        Vector3 GetCurrentEulerAngles()
        {
            Vector3 euler = rb.rotation.eulerAngles;
            // Normalize angles to -180 to 180 range for consistent PID calculation
            float pitch = (euler.x > 180f) ? euler.x - 360f : euler.x;
            // float yaw = (euler.y > 180f) ? euler.y - 360f : euler.y; // Yaw not used
            float roll = (euler.z > 180f) ? euler.z - 360f : euler.z;
            return new Vector3(pitch, 0, roll); // Only returning pitch and roll
        }
    }
}