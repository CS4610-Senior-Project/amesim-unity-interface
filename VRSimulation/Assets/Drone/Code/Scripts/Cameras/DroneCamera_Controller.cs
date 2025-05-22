using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DroneSim 
{
    public class DroneCamera_Controller : MonoBehaviour
    {
        #region Variables
        [Header("Camera Controller Poperties")]
        public Drone_Base_Input input;
        public int startCameraIndex = 0;
        public List<Camera> cameras = new List<Camera>();

        private int cameraIndex = 0;
        #endregion

        #region BuiltIn Methods
        // Start is called before the first frame update
        void Start()
        {
            if(startCameraIndex >= 0 && startCameraIndex < cameras.Count)
            {
                DisableAllCameras();
                cameras[startCameraIndex].enabled = true;
                cameras[startCameraIndex].GetComponent<AudioListener>().enabled = true; 
            }
        }

        // Update is called once per frame
        void Update()
        {
            if(input)
            {
                if(input.CameraSwitch)
                {
                    SwitchCamera();
                }
            }
        }
        #endregion

        #region CustomMethods
        protected virtual void SwitchCamera()
        {
            DisableAllCameras();
            if(cameras.Count > 0)
            {
                cameraIndex++;

                if(cameraIndex >= cameras.Count)
                {
                    cameraIndex = 0;
                }

                cameras[cameraIndex].enabled = true;
                cameras[cameraIndex].GetComponent<AudioListener>().enabled = true;
            }
        }

        void DisableAllCameras()
        {
            if(cameras.Count > 0)
            {
                foreach(Camera cam in cameras)
                {
                    cam.enabled = false;
                    cam.GetComponent<AudioListener>().enabled = false;
                }
            }
        }
        #endregion
    }
}
