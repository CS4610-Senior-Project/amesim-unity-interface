import csv
import json
import matplotlib.pyplot as plt
import os
from typing import List, Tuple

try:
    from amesim import *
except ImportError:
    print('Unable to import Simcenter Amesim module.\nCheck the AME environment variable.')
else:
    print('Simcenter Amesim module is imported')

try:
  from ame_apy import *
except ImportError:
  print('Unable to import Simcenter Amesim API module.\nCheck the AME environment variable.')

##############################################################################################

class SimulationService:
    def __init__(self):
        self._initialize_amesim()
        self.temp_files = []

    def _initialize_amesim(self) -> None:
        AMEInitAPI(False)
        AMEGetAPIVersion()

    def _create_temporary_file(self, time_col, data_col, file_name):
        file_path = os.path.join(os.getcwd(), file_name)
        self.temp_files.append(file_path)
        with open(file_path, 'w', newline='') as temp_data_file:
            csv_writer = csv.writer(temp_data_file, delimiter=' ')
            for row in zip(time_col, data_col):
                csv_writer.writerow(row)

    def _delete_temporary_files(self):
        for file_path in self.temp_files:
            os.remove(file_path)
        self.temp_files = []

    def _trim_amesim_model(self, code: str) -> str:
        lines = code.split('\n')
        index_create_circuit = next(
            (i for i, line in enumerate(lines) if line.startswith('AMECreateCircuit')),
            None
        )
        index_generate_code = next(
            (i for i, line in enumerate(lines) if line.startswith('AMEGenerateCode')),
            None
        )
        if not index_create_circuit or not index_generate_code:
            raise ValueError("Error: Unable to parse file. Please use file generated by Amesim")
        lines = lines[index_create_circuit : index_generate_code]
        trimmed_code = '\n'.join(lines)
        return trimmed_code

    def load_model(self, model_file: str) -> None:
        print(f"Loading model")
        file_extension = model_file.split('.')[-1]
        if file_extension.lower() != "py":
            raise ValueError("Error: Model file must have correct file extension: .py")
        with open(model_file, "r") as file:
            code = file.read()
        try:
            exec(self._trim_amesim_model(code))
        except Exception as e:
            print(f"Error loading model: {e}")
            raise

    def set_model_parameter(self, param_name: str, param_value: str) -> None:

        try:
            AMESetParameterValue(param_name, param_value)
        except Exception as e:
            print(f"Error setting parameter {param_name}: {e}")
            try:
                # Get all parameters in the circuit
                parameters = AMEGetParameters()
                print("\nValid parameters in the circuit:")
                for param in parameters:
                    print(f"- {param.name}")  # Extract the parameter name
            except Exception as e:
                print(f"\nFailed to retrieve parameters: {e}")
            raise ValueError(f"Invalid parameter: {param_name}")

    def set_model_parameter_timeseries(self, table_name: str, data_file: str) -> None:
        file_extension = os.path.splitext(data_file)[1].lower()
        if file_extension not in [".csv", ".txt", ".data"]:
            raise ValueError(f"Data file '{data_file}' must have one of the following extensions: .csv, .txt, .data")
        param_name = f"filename@{table_name}"
        self.set_model_parameter(param_name, data_file)

    def set_runtime_parameters(self, start_time_s: str, stop_time_s: str, interval_s: str) -> None:
        print(f"Setting runtime parameters")
        try:
            AMESetRunParameter("start_time_s", start_time_s)
            AMESetRunParameter("stop_time_s", stop_time_s)
            AMESetRunParameter("interval_s", interval_s)
        except Exception as e:
            print(f"Error setting runtime parameters: {e}")
            raise

    def _parse_config_file(self, config_file: str) -> dict:
        with open(config_file, 'r') as file:
            data = json.load(file)
            required_keys = [
                "model_file", "start_time_s", "end_time_s",
                "interval_s", "parameters", "outputs",
                "generate_output_files"
            ]
            for key in required_keys:
                if key not in data:
                    raise RuntimeError(f"Error: '{key}' is missing in the JSON config file")
            return data

    def run_from_config_file(self, config_file: str) -> None:
        print(f"Running from config file")
        data = self._parse_config_file(config_file)
        # Construct absolute path for model file relative to config file location
        config_dir = os.path.dirname(os.path.abspath(config_file))
        model_path_relative = data["model_file"]
        model_path_absolute = os.path.join(config_dir, model_path_relative)
        self.load_model(model_path_absolute)
        for param_name, value in data["parameters"].items():
            self.set_model_parameter(param_name, str(value))
        # config_dir was defined earlier when handling model_file path
        # Process time series data only if the key exists in the config
        if "time_series_data" in data:
            for table_name, table_info in data["time_series_data"].items():
                # Assuming config structure like: { "table_name": { "file": "relative/path/to/data.csv", ... } }
                if "file" in table_info:
                    data_file_relative = table_info["file"]
                    data_file_absolute = os.path.join(config_dir, data_file_relative)
                    # Check if the data file exists before setting the parameter
                    if os.path.exists(data_file_absolute):
                        self.set_model_parameter_timeseries(table_name, data_file_absolute)
                    else:
                        print(f"Warning: Time series data file not found at {data_file_absolute}")
                else:
                    # Fallback or error handling if 'file' key is missing?
                    # For now, let's just print a warning.
                    print(f"Warning: 'file' key missing for time_series_data table '{table_name}' in config.")
        self.set_runtime_parameters(
            str(data["start_time_s"]),
            str(data["end_time_s"]),
            str(data["interval_s"]),
        )
        self.run_simulation()
        for output_param in data["outputs"]:
            self.plot_variable(output_param)
        if data["generate_output_files"]:
            self.save_all_output_files(data["outputs"])
        self.quit()

    def run_simulation(self) -> None:
        print("Running system simulation...")
        try:
            AMERunSimulation()
        except Exception as e:
            print(f"Error running simulation: {e}")
            raise

    def get_output_values(self, variable_name: str) -> Tuple[List[float], List[float]]:
        print(f"Getting output data for variable: {variable_name}")
        try:
            pairs = AMEGetVariableValues(variable_name)
        except Exception as e:
            print(f"Error retrieving output values for {variable_name}: {e}")
            try:
                # Get all variables in the circuit
                variables = AMEGetVariables()
                print("\nValid variables in the circuit:")
                for var in variables:
                    print(f"- {var.name}")  # Extract the variable name
            except Exception as e:
                print(f"\nFailed to retrieve variables: {e}")
            raise ValueError(f"Invalid variable: {variable_name}")
        time_list, data_list = zip(*pairs)
        return time_list, data_list

    def plot_variable(self, variable_name: str) -> None:
        time_values, variable_values = self.get_output_values(variable_name)
        plt.plot(time_values, variable_values, label=variable_name)
        plt.legend(loc="upper left")
        plt.xlabel("Time")
        plt.ylabel(variable_name)
        plt.grid(True)
        #plt.show()

    def save_all_output_files(self, variable_names: List[str], output_path: str = None) -> None:
        print(f"Saving all output files...")
        self.save_output_data_csv(variable_names, output_path)
        for variable_name in variable_names:
            self.save_plot_pdf(variable_name, output_path)

    def save_output_data_csv(self, variable_names: List[str], output_path: str = None) -> None:
        if output_path is None:
            output_path = os.path.join(os.getcwd(), "output", "data.csv")
        else:
            output_path = os.path.join(output_path, "data.csv")
        output_dir = os.path.dirname(output_path)
        if not os.path.exists(output_dir):
            os.makedirs(output_dir)
        print(f"Saving output data")
        output_data = {}
        for i, variable_name in enumerate(variable_names):
            variable_output = self.get_output_values(variable_name)
            output_data[variable_name] = variable_output[1]
            if i == 0:
                output_data["time"] = variable_output[0]
        with open(output_path, 'w', newline='') as csvfile:
            fieldnames = ["time"] + variable_names
            writer = csv.DictWriter(csvfile, fieldnames=fieldnames)
            writer.writeheader()
            for i in range(len(output_data["time"])):
                row = {field: output_data[field][i] for field in fieldnames}
                writer.writerow(row)

    def save_plot_pdf(self, variable_name: str, output_path: str = None) -> None:
        time_values, variable_values = self.get_output_values(variable_name)
        plt.plot(time_values, variable_values, label=variable_name)
        plt.legend(loc="upper left")
        plt.xlabel("Time")
        plt.ylabel(variable_name)
        plt.grid(True)
        if output_path is None:
            output_path = os.path.join(os.getcwd(), "output", f"{variable_name}.pdf")
        else:
            output_path = os.path.join(output_path, f"{variable_name}.pdf")
        output_dir = os.path.dirname(output_path)
        if not os.path.exists(output_dir):
            os.makedirs(output_dir)
        plt.savefig(output_path)

    def quit(self):
        print(f"Quitting Simulation Service...")
        self._delete_temporary_files()
        AMECloseCircuit(True)
        AMECloseAPI(False)
