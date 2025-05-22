using UnityEngine;

namespace DroneSim
{
    // This script acts as the input source for Drone_Controller,
    // providing values derived from the CSV via the PID controller.
    // It effectively replaces Drone_Base_Input for CSV-driven control.
    [RequireComponent(typeof(OrientationPidController))]
    public class CsvInputProvider : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private OrientationPidController pidController;
        [SerializeField] private CsvReader csvReader; // Added CsvReader reference
        private ManualInputProvider manualInput;

        // Public properties mimicking Drone_Base_Input for compatibility
        public float Pitch { get; private set; }
        public float Roll { get; private set; }
        public float RawCsvTargetRoll { get; private set; } // Added for raw CSV roll target
        public float Yaw { get; private set; }
        public float Throttle { get; private set; } // Raw throttle input (not used directly by Drone_Controller)
        public float StickyThrottle { get; private set; } // Used by Drone_Controller
        public int Flaps { get; private set; } // Not controlled by CSV
        public float Brake { get; private set; } // Not controlled by CSV
        public bool CameraSwitch { get; private set; } // Not controlled by CSV

        [Header("Manual Input")]
        public bool useManualInput = false; // Flag to switch to manual input
        public KeyCode manualInputKey = KeyCode.UpArrow; // Key to activate manual input

        void Awake()
        {
            manualInput = GetComponent<ManualInputProvider>();
            if (!manualInput)
            {
                manualInput = gameObject.AddComponent<ManualInputProvider>();
            }

            if (pidController == null)
            {
                pidController = GetComponent<OrientationPidController>();
            }

            if (csvReader == null) // Initialize CsvReader
            {
                csvReader = FindObjectOfType<CsvReader>();
                if (csvReader == null)
                {
                    Debug.LogError("CsvReader dependency not found on CsvInputProvider!");
                    // enabled = false; // Decide if this is critical enough to disable
                }
            }

            if (pidController == null)
            {
                Debug.LogError("OrientationPidController dependency not found on CsvInputProvider!");
                enabled = false; // Disable if dependency is missing
            }
        }

        void Update() // Update can be used here as PID controller uses FixedUpdate
        {
            if (pidController == null) return;

            // Get calculated inputs from PID controller
            Pitch = pidController.PitchOutput;
            Roll = pidController.RollOutput;

            // Set fixed values as per instructions
            Yaw = 0f;
            StickyThrottle = 1.0f; // Full throttle
            Throttle = 0f; // Raw throttle isn't directly used, but set to 0 for clarity

            // Keep other inputs at default values
            Flaps = 0;
            Brake = 0f;
            CameraSwitch = false; // Or handle camera switching separately if needed

            // Check for ANY arrow key input to activate manual control
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow) ||
                Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                useManualInput = true; // Enable manual input mode
            }

            if (useManualInput && manualInput != null)
            {
                // Get manual input values
                Pitch = manualInput.Pitch;
                Roll = manualInput.Roll;
                Yaw = manualInput.Yaw;
                StickyThrottle = manualInput.Throttle; // Use direct throttle
                Throttle = manualInput.Throttle;
                Flaps = manualInput.Flaps;
                Brake = manualInput.Brake;
                CameraSwitch = manualInput.CameraSwitch;
            }
            else
            {
                // Get raw CSV target roll if CsvReader is available and initialized
                if (csvReader != null && csvReader.IsInitialized())
                {
                    TargetData currentTarget = csvReader.GetTargetData(Time.time); // Consider if Time.fixedTime is better if PID uses FixedUpdate
                    RawCsvTargetRoll = currentTarget.TargetRoll;
                }
                else
                {
                    RawCsvTargetRoll = 0f; // Default if CsvReader isn't ready
                }

                // Get calculated inputs from PID controller
                Pitch = pidController.PitchOutput;
                Roll = pidController.RollOutput;

                // Set fixed values as per instructions
                // Yaw will now be driven by the raw CSV target roll for coordinated turns, and inverted for correct direction
                Yaw = -RawCsvTargetRoll; 
                StickyThrottle = 1.0f; // Full throttle
                Throttle = 0f; // Raw throttle isn't directly used, but set to 0 for clarity

                // Keep other inputs at default values
                Flaps = 0;
                Brake = 0f;
                CameraSwitch = false; // Or handle camera switching separately if needed
            }
        }
    }
}
