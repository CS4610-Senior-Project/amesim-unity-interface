# Integrating Amesim with Unity for Flight Path Visualization

## Overview

This project provides a system for visualizing flight paths simulated in Simcenter Amesim within a 3D environment created in Unity. It consists of three main components: a web-based user interface for defining simulation parameters, a Python service to run the Amesim simulation and process the output data, and a Unity project to visualize the results.


The user interacts with the Web UI to set up simulation parameters and generate a configuration file. The Python Simulation Service takes this configuration, runs the Amesim simulation, and processes the output data into a format suitable for Unity. The Unity project then reads this processed data to drive the 3D visualization of the flight path.

## Features

*   Web-based interface for easy simulation parameter input.
*   Automated execution of Simcenter Amesim simulations via a Python service.
*   Processing and normalization of simulation output data.
*   3D visualization of aircraft flight paths in a Unity environment.
*   PID-controlled drone movement in Unity based on Amesim simulation data.

## Directory Structure

*   `/simulation-service`: Contains the Python scripts for running Amesim simulations and processing data.
*   `/unity-interface`: Contains the Next.js web application for user input and configuration.
*   `/VRSimulation`: Contains the Unity project for 3D visualization.

## Prerequisites

To set up and run this project, you will need the following installed:

*   **Simcenter Amesim:** Specify the version you used (e.g., 2310). A valid license is required.
*   **Python:** A compatible version with your Amesim installation (e.g., Python 3.x). Ensure it's accessible from your terminal.
*   **pip:** Python package installer.
*   **Node.js and pnpm:** For the web user interface. Node.js version 18 or higher is recommended. Install pnpm globally (`npm install -g pnpm`).
*   **Unity Hub and Unity Editor:** Specify the Unity version used (e.g., 2022.3.14f1).
*   **Git:** For cloning the repository.

## Setup and Installation

1.  **Clone the repository:**
    ```bash
    git clone <repository_url>
    cd amesim-unity-interface
    ```
2.  **`simulation-service` Setup:**
    *   Navigate to the simulation service directory:
        ```bash
        cd simulation-service
        ```

    *   Install Python dependencies:
        ```bash
        pip install pandas
        # Or if you have a requirements.txt:
        # pip install -r requirements.txt
        ```
3.  **`unity-interface` (Web UI) Setup:**
    *   Navigate to the web interface directory:
        ```bash
        cd ../unity-interface
        ```
    *   Install Node.js dependencies using pnpm:
        ```bash
        pnpm install
        ```
4.  **`VRSimulation` (Unity Project) Setup:**
    *   Open **Unity Hub**.
    *   Click on **"Add"** and select the `VRSimulation` folder.
    *   Ensure you select the correct Unity Editor version (e.g., 2022.3.14f1) when prompted or in the project settings.
    *   Open the project in the Unity Editor.

## Running the System

Follow these steps to run an end-to-end simulation and visualization:

1.  **Start the Web UI:**
    It can be accessed from here: https://v0-custom-unity-ui-design.vercel.app

2.  **Run the Python Simulation Service:**
    *   Open a **new terminal** and navigate to the `simulation-service` directory:
        ```bash
        cd simulation-service
        ```
    *   Place the `plane_config.json` file you downloaded from the Web UI into the `simulation-service/example/` directory.
    *   Execute the simulation script, making sure to use the correct Python executable if Amesim requires it:
        ```bash
        # Example for Windows:
        "C:\Program Files\Simcenter\2310\Amesim\python.bat" script.py -c example/plane_config.json

        # Example for macOS/Linux (adjust path to Amesim's python.bat/python executable):
        /path/to/Simcenter/2310/Amesim/python.bat script.py -c example/plane_config.json
        ```
    *   This script will run the Amesim simulation and generate the `pid_targets.csv` (or `pid_targets_normalized.csv`) file in the `simulation-service/output/` directory (or the directory specified in your config).

3.  **Visualize in Unity:**
    *   Copy the generated `pid_targets.csv` file from the output directory of the simulation service to the `VRSimulation/Assets/StreamingAssets/` folder in your Unity project. You might need to create the `StreamingAssets` folder if it doesn't exist.
    *   In the Unity Editor, open the main simulation scene (e.g., located in `Assets/Scenes/`).
    *   Press the **Play** button in the Unity Editor.
    *   The 3D aircraft model should now visualize the flight path based on the data from the `pid_targets.csv` file.

## Key Files and Scripts

*   `simulation-service/script.py`: The main Python script for orchestrating the simulation.
*   `simulation-service/example/plane_config.json`: An example configuration file for the simulation service.
*   `simulation-service/plane.ame` (or `AMESIM/6DOF Simulation/6dof_flight.ame`): The core Amesim model file.
*   `VRSimulation/Assets/Drone/Code/Scripts/Input/CsvReader.cs`: Unity script responsible for reading the processed CSV data.
*   `VRSimulation/Assets/Drone/Code/Scripts/Controller/Drone_Controller.cs`: Unity script controlling the drone's movement and orientation based on simulation data.
*   `unity-interface/app/page.tsx` (or similar): The main page component for the web user interface.
