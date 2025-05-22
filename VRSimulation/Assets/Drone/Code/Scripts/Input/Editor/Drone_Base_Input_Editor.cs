using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace DroneSim
{
    [CustomEditor(typeof(Drone_Base_Input))]
    public class Drone_Base_Input_Editor : Editor
    {
        #region Variables
        private Drone_Base_Input targetInput;
        #endregion


        #region Builtin Methods
        void OnEnable()
        {
            targetInput = (Drone_Base_Input)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            string debugInfo = "";
            debugInfo += "Pitch = " + targetInput.Pitch + "\n";
            debugInfo += "Roll = " + targetInput.Roll + "\n";
            debugInfo += "Yaw = " + targetInput.Yaw + "\n";
            debugInfo += "Throttle = " + targetInput.Throttle + "\n";
             debugInfo += "Sticky Throttle = " + targetInput.StickyThrottle + "\n";
            debugInfo += "Brake = " + targetInput.Brake + "\n";
            debugInfo += "Flaps = " + targetInput.Flaps + "\n";
             debugInfo += "Camera = " + targetInput.CameraSwitch + "\n";

            //custom editor code
            GUILayout.Space(20);
            EditorGUILayout.TextArea(debugInfo, GUILayout.Height(120));
            GUILayout.Space(20);

            Repaint();
        }
        #endregion
    }
}
