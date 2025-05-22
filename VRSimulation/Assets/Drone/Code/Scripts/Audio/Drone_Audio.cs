using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DroneSim
{
    public class Drone_Audio : MonoBehaviour
    {
        #region Variables
        [Header("Airplane Audio Properties")]
        public Drone_Base_Input input;
        public AudioSource idleSource;
        public AudioSource fullThrottleSource;
        public float maxPitchValue = 1.2f;

        private float finalPitchValue;
        private float finalVolumeValue;
        #endregion

        #region BuiltIn Methods
        // Start is called before the first frame update
        void Start()
        {
            if(fullThrottleSource)
            {
                fullThrottleSource.volume = 0f;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (input)
            {
                HandleAudio();
            }
        }
        #endregion

        #region CustomMethods
        protected virtual void HandleAudio()
        {
            finalVolumeValue = Mathf.Lerp(0f, 1f, input.StickyThrottle);
            finalPitchValue = Mathf.Lerp(1f, maxPitchValue, input.StickyThrottle);

            if(fullThrottleSource)
            {
                fullThrottleSource.volume = finalVolumeValue;
                fullThrottleSource.pitch = finalPitchValue;
            }
        }
        #endregion
    }
}
