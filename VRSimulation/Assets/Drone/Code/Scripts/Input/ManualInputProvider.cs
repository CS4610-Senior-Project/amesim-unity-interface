using UnityEngine;

namespace DroneSim
{
    public class ManualInputProvider : MonoBehaviour
    {
        #region Variables
        private float pitch = 0f;
        private float roll = 0f;
        private float yaw = 0f;
        private float throttle = 0f;

        [SerializeField]
        private KeyCode brakeKey = KeyCode.Space;
        private float brake = 0f;

        [SerializeField]
        private KeyCode cameraKey = KeyCode.C;
        private bool cameraSwitch = false;

        public int maxFlapIncrements = 2;
        private int flaps = 0;
        #endregion


        #region Properties
        public float Pitch { get { return pitch; } }
        public float Roll { get { return roll; } }
        public float Yaw { get { return yaw; } }
        public float Throttle { get { return throttle; } }
        public float StickyThrottle { get { return throttle; } } // Direct throttle control
        public int Flaps { get { return flaps; } }
        public float Brake { get { return brake; } }
        public bool CameraSwitch { get { return cameraSwitch; } }
        #endregion


        #region Builtin Methods
        void Update()
        {
            HandleInput();
            ClampInputs();
        }
        #endregion


        #region Custom Methods
        private void HandleInput()
        {
            //Process Main Control Input
            pitch = Input.GetAxis("Vertical");
            roll = -Input.GetAxis("Horizontal"); // Invert roll direction
            yaw = Input.GetAxis("Yaw");
            throttle = Input.GetAxis("Throttle");

            //Process Brake inputs
            brake = Input.GetKey(brakeKey) ? 1f : 0f;

            //Process Flaps Inputs
            if (Input.GetKeyDown(KeyCode.F))
            {
                flaps += 1;
            }

            if (Input.GetKeyDown(KeyCode.G))
            {
                flaps -= 1;
            }

            flaps = Mathf.Clamp(flaps, 0, maxFlapIncrements);

            //Camera Switch Key
            cameraSwitch = Input.GetKeyDown(cameraKey);
        }

        private void ClampInputs()
        {
            pitch = Mathf.Clamp(pitch, -1f, 1f);
            roll = Mathf.Clamp(roll, -1f, 1f);
            yaw = Mathf.Clamp(yaw, -1f, 1f);
            throttle = Mathf.Clamp(throttle, -1f, 1f);
            brake = Mathf.Clamp(brake, 0f, 1f);

            flaps = Mathf.Clamp(flaps, 0, maxFlapIncrements);
        }
        #endregion
    }
}