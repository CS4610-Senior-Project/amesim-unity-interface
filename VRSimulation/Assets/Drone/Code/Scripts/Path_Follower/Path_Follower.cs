using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace DroneSim
{
    public class PathFollower : MonoBehaviour
    {
        public string csvFilePath = "Assets/Data/PlanePath.csv"; // Path to the CSV file
        public string controlFilePath = "Assets/Data/control.txt"; // Path to the control file
        private List<Vector3> pathPositions = new List<Vector3>();
        private List<Vector3> pathRotations = new List<Vector3>();
        private int currentPointIndex = 0;
        public float moveSpeed = 5f; // Speed at which the plane moves
        public float rotationSpeed = 5f; // Speed at which the plane rotates
        public bool startMoving = false; // Start moving when this is true

        private Rigidbody rb;
        private bool isPlayerControlled = false;

        void Awake() {
            rb = GetComponent<Rigidbody>();
        }

        void Start() {
            ReadCSVFile();
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            startMoving = true;
        }

        void Update() {
            CheckControlFile();

            if (startMoving && !isPlayerControlled) {
                if (currentPointIndex < pathPositions.Count) {
                    MoveAndRotateToNextPoint();
                }
            }

            if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0) {
                EnableManualControl();
            }
        }

        void CheckControlFile() {
            if (!File.Exists(controlFilePath))
                return;

            string command = File.ReadAllText(controlFilePath).Trim();
            Debug.Log("Read command: " + command); // Debugging output

            switch (command)
            {
                case "RELOAD":
                    Debug.Log("Reloading CSV file."); // Confirm reload is triggered
                    ReadCSVFile();
                    currentPointIndex = 0; // Reset to start of the new path
                    startMoving = true;
                    File.WriteAllText(controlFilePath, ""); // Clear command
                    break;
                case "RESET":
                    Debug.Log("Resetting to start.");
                    currentPointIndex = 0;
                    startMoving = true;
                    File.WriteAllText(controlFilePath, ""); // Clear command
                    break;
            }
        }


        void ReadCSVFile() {
            string[] lines = File.ReadAllLines(csvFilePath);
            pathPositions.Clear();
            pathRotations.Clear();
            for (int i = 1; i < lines.Length; i++) { // Start from 1 to skip header
                string[] values = lines[i].Split(',');
                if (values.Length == 6) {
                    if (float.TryParse(values[0], out float x) &&
                        float.TryParse(values[1], out float y) &&
                        float.TryParse(values[2], out float z) &&
                        float.TryParse(values[3], out float rx) &&
                        float.TryParse(values[4], out float ry) &&
                        float.TryParse(values[5], out float rz)) {
                        Vector3 position = new Vector3(x, y, z);
                        Vector3 rotation = new Vector3(rx, ry, rz);
                        pathPositions.Add(position);
                        pathRotations.Add(rotation);
                    }
                }
            }
        }

        void MoveAndRotateToNextPoint() {
            if (currentPointIndex < pathPositions.Count) {
                Vector3 targetPosition = pathPositions[currentPointIndex];
                Vector3 targetRotation = pathRotations[currentPointIndex];
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
                Quaternion currentRotation = transform.rotation;
                Quaternion targetQuaternion = Quaternion.Euler(targetRotation);
                transform.rotation = Quaternion.RotateTowards(currentRotation, targetQuaternion, rotationSpeed * Time.deltaTime);

                if (Vector3.Distance(transform.position, targetPosition) < 0.1f && Quaternion.Angle(transform.rotation, targetQuaternion) < 0.1f) {
                    currentPointIndex++;
                }
            } else {
                startMoving = false; // Stop moving once all points are reached
            }
        }

        void EnableManualControl() {
            isPlayerControlled = true;
            rb.constraints = RigidbodyConstraints.None;
        }
    }
}
