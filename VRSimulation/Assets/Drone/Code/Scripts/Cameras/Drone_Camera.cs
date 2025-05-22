using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DroneSim {
    public class Drone_Camera : Drone_Basic_Follow_Camera
    {
        #region variables
        [Header("Airplane Camera Properties")]
        public float minHeightFromGround = 2f;
        #endregion

        protected override void HandleCamera()
        {
            //Airplane camera Code
            RaycastHit hit;
            if(Physics.Raycast(transform.position, Vector3.down, out hit))
            {
                if(hit.distance < minHeightFromGround && hit.transform.tag == "ground")
                {
                    float wantedHeight = origHeight + (minHeightFromGround - hit.distance);
                    height = wantedHeight;
                }
            }

            base.HandleCamera();

        }
    }
}