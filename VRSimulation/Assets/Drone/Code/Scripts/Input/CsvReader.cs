using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DroneSim
{
    // Structure to hold the data for a single time point from the CSV
    public struct TargetData
    {
        public float Time;
        public float TargetPitch;
        public float TargetRoll;
        // Yaw and Thrust are omitted as per instructions

        public TargetData(float time, float pitch, float roll)
        {
            Time = time;
            TargetPitch = pitch;
            TargetRoll = roll;
        }
    }

    public class CsvReader : MonoBehaviour
    {
        [Tooltip("Path to the CSV file (e.g., ../../../Library/CloudStorage/OneDrive-CalPolyPomona/simulation-service/pid_targets.csv)")]
        public string csvFilePath = "../../../Library/CloudStorage/OneDrive-CalPolyPomona/simulation-service/pid_targets.csv";

        private List<TargetData> targetDataList = new List<TargetData>();
        private bool isInitialized = false;

        void Awake()
        {
            LoadCsvData();
        }

        void LoadCsvData()
        {
            string fullPath = Path.Combine(Application.dataPath, "..", csvFilePath); // Go up one level from Assets
            Debug.Log($"CsvReader: Attempting to load CSV from resolved path: {fullPath}");

            try
            {
                var lines = File.ReadAllLines(fullPath); // Directly try to read

                // Skip header row (assuming first line is header)
                for (int i = 1; i < lines.Length; i++)
                {
                    var values = lines[i].Split(',');

                    if (values.Length >= 3) // Ensure we have at least Time, Pitch, Roll
                    {
                        if (float.TryParse(values[0].Trim(), out float time) &&
                            float.TryParse(values[1].Trim(), out float pitch) &&
                            float.TryParse(values[2].Trim(), out float roll))
                        {
                            targetDataList.Add(new TargetData(time, pitch, roll));
                        }
                        else
                        {
                            Debug.LogWarning($"Could not parse line {i + 1}: {lines[i]}");
                        }
                    }
                    else
                    {
                         Debug.LogWarning($"Skipping line {i + 1} due to insufficient columns: {lines[i]}");
                    }
                }

                // Sort by time just in case the CSV isn't ordered
                targetDataList = targetDataList.OrderBy(td => td.Time).ToList();

                if (targetDataList.Count > 0)
                {
                    isInitialized = true;
                    Debug.Log($"Successfully loaded {targetDataList.Count} data points from {csvFilePath} (resolved: {fullPath})");
                }
                else
                {
                    Debug.LogError($"No valid data points loaded from {csvFilePath} (resolved: {fullPath})");
                    isInitialized = false;
                }
            }
            catch (System.IO.DirectoryNotFoundException dnfEx)
            {
                Debug.LogError($"CsvReader: Directory not found for path: {fullPath}. Details: {dnfEx.ToString()}");
                isInitialized = false;
            }
            catch (System.IO.FileNotFoundException fnfEx)
            {
                Debug.LogError($"CsvReader: File not found at path: {fullPath}. Details: {fnfEx.ToString()}");
                isInitialized = false;
            }
            catch (System.UnauthorizedAccessException uaEx)
            {
                Debug.LogError($"CsvReader: Unauthorized access to path: {fullPath}. Details: {uaEx.ToString()}");
                isInitialized = false;
            }
            catch (System.IO.IOException ioEx)
            {
                Debug.LogError($"CsvReader: IO Error accessing path: {fullPath}. Details: {ioEx.ToString()}");
                isInitialized = false;
            }
            catch (System.Exception ex) // General catch-all
            {
                Debug.LogError($"Error loading CSV file: {fullPath}. Details: {ex.ToString()}");
                isInitialized = false;
            }
        }

        // Gets the target data for the given simulation time, interpolating between points
        public TargetData GetTargetData(float currentTime)
        {
            if (!isInitialized || targetDataList.Count == 0)
            {
                Debug.LogWarning("CsvReader not initialized or has no data.");
                return new TargetData(currentTime, 0f, 0f); // Return default if not ready
            }

            // Handle cases before the first data point or after the last
            if (currentTime <= targetDataList[0].Time)
            {
                return targetDataList[0];
            }
            if (currentTime >= targetDataList[targetDataList.Count - 1].Time)
            {
                return targetDataList[targetDataList.Count - 1];
            }

            // Find the two data points surrounding the current time
            int index = targetDataList.FindIndex(td => td.Time >= currentTime);
            TargetData prevData = targetDataList[index - 1];
            TargetData nextData = targetDataList[index];

            // Interpolate
            float t = Mathf.InverseLerp(prevData.Time, nextData.Time, currentTime);
            float interpolatedPitch = Mathf.Lerp(prevData.TargetPitch, nextData.TargetPitch, t);
            float interpolatedRoll = Mathf.Lerp(prevData.TargetRoll, nextData.TargetRoll, t);

            return new TargetData(currentTime, interpolatedPitch, interpolatedRoll);
        }

        public bool IsInitialized()
        {
            return isInitialized;
        }
    }
}
