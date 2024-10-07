// src/App.js

import React, { useState } from "react";
import { BlobServiceClient } from "@azure/storage-blob";
import "./App.css";

function App() {
  const [file, setFile] = useState(null);
  const [uploadStatus, setUploadStatus] = useState("");

  // SAS URI for Azure Blob Storage
  const sasUri =
    "http://127.0.0.1:10000/devstoreaccount1/temp?sv=2024-11-04&se=2024-10-08T05%3A27%3A50Z&sr=c&sp=w&sig=h%2BS73B0Dnfw9ZqdCiUWaaclyJRYOBaK1mgNQju3HOKM%3D";

  // Function to handle file upload
  const uploadFile = async () => {
    if (!file) {
      setUploadStatus("Please select a file first.");
      return;
    }

    try {
      // Create a BlobServiceClient using the SAS URI
      const blobServiceClient = new BlobServiceClient(sasUri);

      // Get the container client (assume container name is 'temp')
      const containerClient = blobServiceClient.getContainerClient("temp");

      // Create blob client with the file name
      const blobClient = containerClient.getBlockBlobClient(file.name);

      // Upload file as a blob
      setUploadStatus("Uploading...");
      await blobClient.uploadData(file); 

      setUploadStatus("File uploaded successfully!");
    } catch (error) {
      console.error("Error uploading file:", error);
      setUploadStatus("Error uploading file.");
    }
  };

  return (
    <div className="App">
      <header className="App-header">
        <h1>Upload File to Azure Blob Storage using SAS URI</h1>
        <input
          type="file"
          onChange={(e) => setFile(e.target.files[0])}
        />
        <button onClick={uploadFile}>Upload File</button>
        <p>{uploadStatus}</p>
      </header>
    </div>
  );
}

export default App;
