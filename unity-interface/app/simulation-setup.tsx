"use client";

import { useState, useRef, ChangeEvent } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Accordion, AccordionContent, AccordionItem, AccordionTrigger } from "@/components/ui/accordion";
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import { Checkbox } from "@/components/ui/checkbox";
import { ScrollArea } from "@/components/ui/scroll-area";
import { FileUp, Search, Settings, Download } from "lucide-react"; // Removed RefreshCw

// Define parameter type
type ParameterMap = { [key: string]: string };
// Define type for parameters with numeric values
type NumericParameterMap = { [key: string]: number };


export default function SimulationSetup() {
  const [configPreview, setConfigPreview] = useState<string | false>(false);
  const [modelFile, setModelFile] = useState<File | null>(null);
  const [modelFileName, setModelFileName] = useState<string>(""); // To display in input
  const [csvFile, setCsvFile] = useState<File | null>(null); // State for CSV file
  const [csvFileName, setCsvFileName] = useState<string>(""); // State for CSV file name display
  // Removed script file state
  const [extractedParams, setExtractedParams] = useState<ParameterMap>({});
  const [userParams, setUserParams] = useState<ParameterMap>({}); // To store user overrides
  // Removed isRunning and runOutput state

  // Refs for hidden file inputs
  const modelFileInputRef = useRef<HTMLInputElement>(null);
  const csvFileInputRef = useRef<HTMLInputElement>(null);

  // --- Helper Functions ---

  const validateInputs = (): boolean => {
    if (!modelFile) {
      alert("Please select a model file.");
      return false;
    }
    if (!csvFile) {
      alert("Please select a CSV file for time-varying inputs.");
      return false;
    }
    // Removed script file validation
    const startTimeInput = document.getElementById("start-time") as HTMLInputElement;
    const endTimeInput = document.getElementById("end-time") as HTMLInputElement;
    const intervalInput = document.getElementById("interval") as HTMLInputElement;

    if (!startTimeInput?.value) {
        alert("Please enter a Start Time.");
        return false;
    }
    if (!endTimeInput?.value) {
        alert("Please enter an End Time.");
        return false;
    }
     if (!intervalInput?.value) {
        alert("Please enter an Interval.");
        return false;
    }
    return true;
  };

  const generateConfig = (): object | null => {
    if (!validateInputs()) {
      return null;
    }

    // Collect and parse runtime parameters (ensure they are numbers)
    const startTime = parseFloat((document.getElementById("start-time") as HTMLInputElement)?.value || "0");
    const endTime = parseFloat((document.getElementById("end-time") as HTMLInputElement)?.value || "100");
    const interval = parseFloat((document.getElementById("interval") as HTMLInputElement)?.value || "0.01");

    // Convert static parameters to numbers
    const numericParams: NumericParameterMap = {};
    for (const [key, value] of Object.entries(userParams)) {
      const numValue = parseFloat(value);
      numericParams[key] = isNaN(numValue) ? 0 : numValue; // Default to 0 if parsing fails
    }

    // Construct time series data using the uploaded CSV filename
    const timeSeriesData = {
      "dynamic_time_table": {
        "file": `data/${csvFileName}` // Use the state variable for CSV filename
      }
    };

    // Hardcoded outputs and generate flag as per request
    const outputVariables = ["eulerangles_1@aero_fd_6dof_body"];
    const generateOutputFiles = true;

    // Construct the final configuration object in the desired format
    const config = {
      model_file: `models/${modelFileName}`, // Assuming "models/" prefix
      start_time_s: startTime,
      end_time_s: endTime,
      interval_s: interval,
      time_series_data: timeSeriesData,
      outputs: outputVariables, // Hardcoded outputs
      generate_output_files: generateOutputFiles, // Hardcoded flag
    };

    return config;
  }

  const parseModelParameters = (content: string): ParameterMap => {
    const params: ParameterMap = {};
    // Corrected regex to match ss.set_model_parameter("param", "value") format
    const regex = /ss\.set_model_parameter\("([^"]+)", "([^"]+)"\)/g;
    let match;
    while ((match = regex.exec(content)) !== null) {
      // parameter_name@component_instance = value
      params[match[1]] = match[2];
    }
    // console.log("Extracted Params:", params); // Keep for potential debugging
    return params;
  };

  const handleFileChange = (event: ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (file && file.name.endsWith(".py")) {
      setModelFile(file);
      setModelFileName(file.name); // Update display name

      const reader = new FileReader();
      reader.onload = (e) => {
        const content = e.target?.result as string;
        if (content) {
          const defaults = parseModelParameters(content);
          setExtractedParams(defaults);
          setUserParams(defaults); // Initialize user params with defaults
        }
      };
      reader.onerror = (e) => {
        console.error("Error reading model file:", e);
        // Consider adding user feedback (e.g., toast notification)
        setModelFile(null);
        setModelFileName("");
        setExtractedParams({});
        setUserParams({});
      };
      reader.readAsText(file);
    } else {
      console.warn("Invalid model file selected or no file chosen.");
      setModelFile(null);
      setModelFileName("");
      setExtractedParams({});
      setUserParams({});
      // Consider adding user feedback
    }
     // Reset file input value
     if (event.target) {
        event.target.value = '';
     }
  };

  const handleCsvFileChange = (event: ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (file && file.name.endsWith(".csv")) {
      setCsvFile(file);
      setCsvFileName(file.name);
    } else {
      console.warn("Invalid CSV file selected or no file chosen.");
      setCsvFile(null);
      setCsvFileName("");
      // Consider adding user feedback
    }
    // Reset file input value
    if (event.target) {
      event.target.value = '';
    }
  };

  const handleModelButtonClick = () => {
    modelFileInputRef.current?.click();
  };

  const handleCsvButtonClick = () => {
    csvFileInputRef.current?.click();
  };

  // Removed script file handlers


  // --- Event Handlers ---

  const handleGenerateAndDownload = () => {
    const config = generateConfig();
    if (!config) {
      return; // Validation failed
    }

    try {
      const configString = JSON.stringify(config, null, 2);
      const blob = new Blob([configString], { type: 'application/json' });
      const url = URL.createObjectURL(blob);

      // Create a temporary anchor element to trigger download
      const a = document.createElement('a');
      a.href = url;
      a.download = 'plane_config.json'; // Set the desired filename
      a.setAttribute('download', 'plane_config.json'); // Ensure overwrite
      document.body.appendChild(a); // Append to body to make it clickable
      a.click();

      // Clean up: remove anchor and revoke object URL
      document.body.removeChild(a);
      URL.revokeObjectURL(url);

      console.log('Config file download triggered.');
      // alert('Configuration file download initiated.'); // Removed alert

    } catch (error) {
      console.error('Error generating or downloading config:', error);
      alert(`Error generating configuration file: ${error instanceof Error ? error.message : String(error)}`);
    }
  };


  return (
    <div className="space-y-6 py-4">
      <div className="grid gap-6 md:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle>Model Selection</CardTitle>
            <CardDescription>Select the Amesim-generated model file</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              <div className="grid w-full items-center gap-1.5">
                <Label htmlFor="model-file">Model File</Label>
                <div className="flex w-full items-center space-x-2">
                  {/* Hidden actual file input */}
                   <input
                     type="file"
                     ref={modelFileInputRef} // Use correct ref
                     onChange={handleFileChange}
                     accept=".py"
                     style={{ display: "none" }}
                     id="model-file-input" // Ensure ID is present
                   />
                  {/* Display input (read-only) */}
                  <Input
                    id="model-file"
                    placeholder="Select model.py file..."
                    value={modelFileName}
                    readOnly
                  />
                  {/* Button to trigger model file selection */}
                  <Button variant="outline" size="icon" onClick={handleModelButtonClick}>
                    <FileUp className="h-4 w-4" />
                  </Button>
                </div>
              </div>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Runtime Parameters</CardTitle>
            <CardDescription>Set simulation runtime parameters</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              <div className="grid w-full items-center gap-1.5">
                <Label htmlFor="start-time">Start Time (s)</Label>
                <Input id="start-time" type="number" placeholder="0.0" />
              </div>
              <div className="grid w-full items-center gap-1.5">
                <Label htmlFor="end-time">End Time (s)</Label>
                <Input id="end-time" type="number" placeholder="100.0" />
              </div>
              <div className="grid w-full items-center gap-1.5">
                <Label htmlFor="interval">Interval (s)</Label>
                <Input id="interval" type="number" placeholder="0.01" />
              </div>
            </div>
          </CardContent>
        </Card>
      </div>


      <Card>
        <CardHeader>
          <CardTitle>Time-Varying Inputs</CardTitle>
          <CardDescription>Configure dynamic inputs that change over time</CardDescription>
        </CardHeader>
        <CardContent>
          {/* Removed Tabs component, only CSV upload remains */}
          <div className="space-y-4">
              <div className="grid w-full items-center gap-1.5">
                <Label htmlFor="csv-file">CSV File</Label>
                <div className="flex w-full items-center space-x-2">
                   {/* Hidden actual CSV file input */}
                   <input
                     type="file"
                     ref={csvFileInputRef} // Use correct ref
                     onChange={handleCsvFileChange}
                     accept=".csv"
                     style={{ display: "none" }}
                     id="csv-file-input"
                   />
                  {/* Display input (read-only) for CSV filename */}
                  <Input
                    id="csv-file"
                    placeholder="Select time_series.csv file..."
                    value={csvFileName} // Display selected CSV filename
                    readOnly
                  />
                  {/* Button to trigger CSV file selection */}
                  <Button variant="outline" size="icon" onClick={handleCsvButtonClick}>
                    <FileUp className="h-4 w-4" />
                  </Button>
                </div>
              </div>
              <div className="rounded-md border">
                <div className="p-4 text-center text-sm text-muted-foreground">
                  CSV preview will appear here after file upload
                </div>
              </div>
              {/* Removed CSV preview placeholder */}
          </div>
        </CardContent>
      </Card>

      {/* Output Options Card Removed */}

      {/* Script Upload Card Removed */}


      {/* Action Buttons */}
      <div className="flex justify-end">
        {/* Generate Config Button Removed */}
        <div className="space-x-2">
          {/* Reset Button Removed */}
          {/* Changed to Generate & Download Button */}
          <Button onClick={handleGenerateAndDownload}>
            <Download className="mr-2 h-4 w-4" />
            Generate & Download Config
          </Button>
        </div>
      </div>

       {/* Removed Display Area for Run Output/Error */}

      {/* Config Preview Modal Removed */}
    </div>
  )
}
