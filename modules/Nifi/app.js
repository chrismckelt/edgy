"use strict";

var Transport = require("azure-iot-device-mqtt").Mqtt;
var Client = require("azure-iot-device").ModuleClient;
var moduleTwin;
const { exec, execSync } = require("child_process");

// Blob storage
const azureStorage = require("azure-storage");
const path = require("path");

const azureStorageConfig = {
  accountName: "",
  accountKey: "",
  containerName: ""
};

// Initiate the module client
Client.fromEnvironment(Transport, function(err, client) {
  if (err) {
    throw err;
  } else {
    client.on("error", function(err) {
      throw err;
    });

    // connect to the Edge instance
    client.open(function(err) {
      if (err) {
        throw err;
      } else {
        console.log("[INFO] IoT Hub module client initialized");

        // Act on twin change of the module.
        client.getTwin(function(err, twin) {
          if (err) {
            console.error("[ERROR] Error getting twin: " + err.message);
          } else {
            moduleTwin = twin;
            moduleTwin.on("properties.desired", function(data) {
              console.log("Module Twin: %s", JSON.stringify(data));
              // get the twin information
              var flowversion = data.flowversion;
              var certificatefile = data.certificatefile;
              azureStorageConfig.accountName = data.accountname;
              azureStorageConfig.accountKey = data.accountkey;
              azureStorageConfig.containerName = data.containername;

              // Process the files
              setCertificatefile(certificatefile);
              setFlowfile(flowversion);

              // Restart NiFi
              restartNifi();

              // Report back status
              var reported = { flowversion: flowversion };
              sendReportedProperties(reported);
            });
          }
        });

        // Register handler for 'BackupFlow'
        client.onMethod("BackupFlow", function(request, response) {
          console.log("[INFO] Received a request for BackupFlow()");
          console.log("[INFO] " + JSON.stringify(request.payload, null, 2));
          var responsePayload = {
            message: "Unknown"
          };

          if (request.payload.version) {
            // Copy the flow file and upload it to blob storage
            responsePayload.message = backupFlowfile(request.payload.version);
          } else {
            responsePayload.message = "[ERROR] Flow file version not set.";
          }

          // Send repsonse
          response.send(200, responsePayload, function(err) {
            if (err) {
              console.error(
                "[ERROR] Unable to send method response: " + err.toString()
              );
            } else {
              console.log("[INFO] Response to BackupFlow() sent.");
            }
          });
        });
      }
    });
  }
});

// Send the reported properties patch to the hub
function sendReportedProperties(data) {
  moduleTwin.properties.reported.update(data, function(err) {
    if (err) throw err;
    console.log("[INFO] Twin state reported");
    console.log("[INFO] " + JSON.stringify(data, null, 2));
  });
}

function setCertificatefile(file) {
  // Laoding certificate file
  console.log("[INFO] Loading certificate file.");
  importCert("rootca", "/config/certs/azure-iot-test-only.root.ca.cert.pem");
  importCert("device", "/config/certs/new-edge-device.cert.pem");
  importCert("fullchain", "/config/certs/new-edge-device-full-chain.cert.pem");
 
  return;
  if (file) {
    var download = downloadBlob(file, "/config/");
    download
      .then(function() {
        try {
          execSync("keytool -import -noprompt -alias iot-edge-production-full -file /config/" + file + " -keystore /usr/local/openjdk-8/lib/security/cacerts -storepass changeit");
          console.log("[INFO] Loading certificate DOWNLOADED successful. " + file);
        } catch (err) {
          console.warn(
            "[WARN] Error loading DOWNLOADED certificate: " + file + err.message
          );
          console.warn(err);
        }
      })
      .catch(function(err) {
        console.warn("[ERROR] Error downloading flow file: " + err.message);
        console.warn(err);
      });
  }
}

function importCert(alias, certname) {
  var dir1 = '/usr/lib/jvm/java-8-openjdk-amd64/jre/lib/security/cacerts'; // ubuntu
  var dir2 = '/usr/local/openjdk-8/lib/security/cacerts'; // other -- docker.io/apache/nifi

  try {
    // make sure this matches the linux HOST machines config.yaml
    // this is for ubuntu
    execSync(
      "keytool -import -noprompt -alias " + alias + " -file " + certname + " -keystore " + dir2 + " -storepass changeit"
    );
    console.log("[INFO] Loading certificate successful." + certname);
  } catch (err) {
    console.warn("[WARN] Error loading certificate: " + certname + err.message);
    console.warn(err);
  }


}

function setFlowfile(version) {
  
  // try {
  //   execSync(
  //     "cp /config/flow.xml.gz /opt/nifi/nifi-current//conf/flow.xml.gz"
  //   );
  //   console.log("[INFO] Loading flow file successful.");
  // } catch (err) {
  //   console.warn("[WARN] Error loading flow file: " + err.message);
  // }
  
  return;

  // Load flow file from /config directory (can also be done from blob storage online)
  if (version) {
    console.log("[INFO] Loading Nifi flow file.");
    var download = downloadBlob(version + "flow.xml.gz", "/config/");
    download
      .then(function() {
        try {
          execSync(
            "cp /config/" +
              version +
              ".flow.xml.gz /opt/nifi/nifi-current//conf/flow.xml.gz"
          );
          console.log("[INFO] Loading flow file successful.");
        } catch (err) {
          console.warn("[WARN] Error loading flow file: " + err.message);
        }
      })
      .catch(function(err) {
        console.warn("[WARN] Error downloading flow file: " + err.message);
      });
  }
}

function backupFlowfile(version) {
  // Load flow file from /config directory (can also be done from blob storage online)
  if (version) {
    console.log("[INFO] Saving Nifi flow file.");
    try {
      execSync(
        "cp /opt/nifi/nifi-current/conf/flow.xml.gz /config/" + version + ".flow.xml.gz"
      );
      console.log("[INFO] Saving flow file successful.");
      var upload = uploadBlob(version + ".flow.xml.gz", "/config/");
      upload
        .then(function() {
          console.log("[INFO] Uploading flow file successful.");
        })
        .catch(function(err) {
          console.log("[INFO] Uploading flow file failed. " + err.message);
        });
    } catch (err) {
      console.error("[ERROR] Error backup flow file: " + err.message);
    }
  }
}

function restartNifi() {
  // (re)start NiFi
  console.log("[INFO] (Re)Starting NiFi.");
  try {
    exec("/opt/nifi/nifi-current/bin/nifi.sh restart", (err, stdout, stderr) => {
      if (err) console.error("[ERROR] Error starting Nifi: " + err.message);
      else console.log("[INFO] Started Nifi successful.");
    });
  } catch (err) {
    console.error("[ERROR] Error starting Nifi: " + err.message);
  }
}

function downloadBlob(blobName, downloadFilePath) {
  // only download if storage information is set
  return new Promise(function(resolve, reject) {
    if (
      azureStorageConfig.accountName &&
      azureStorageConfig.accountKey &&
      azureStorageConfig.containerName
    ) {
      const name = path.basename(blobName);
      console.log("[INFO] File to download: %s", name);
      const blobService = azureStorage.createBlobService(
        azureStorageConfig.accountName,
        azureStorageConfig.accountKey
      );
      blobService.getBlobToLocalFile(
        azureStorageConfig.containerName,
        blobName,
        `${downloadFilePath}${name}`,
        function(error, serverBlob) {
          if (error) {
            reject(error);
          } else console.log("[INFO] File downloaded: %s", name);
          resolve();
        }
      );
    } else {
      console.log(
        "[WARN] Azure storage credentials are not set. Please update module twin desired properties."
      );
      resolve();
    }
  });
}

function uploadBlob(blobName, uploadFilePath) {
  // only upload if storage information is set
  return new Promise(function(resolve, reject) {
    if (
      azureStorageConfig.accountName &&
      azureStorageConfig.accountKey &&
      azureStorageConfig.containerName
    ) {
      const name = path.basename(blobName);
      console.log("[INFO] File to upload: %s", name);
      const blobService = azureStorage.createBlobService(
        azureStorageConfig.accountName,
        azureStorageConfig.accountKey
      );
      blobService.createBlockBlobFromLocalFile(
        azureStorageConfig.containerName,
        blobName,
        `${uploadFilePath}${name}`,
        function(error) {
          if (error) {
            reject(error);
          } else {
            resolve();
          }
        }
      );
    } else {
      console.log(
        "[WARN] Azure storage credentials are not set. Please update module twin desired properties."
      );
      resolve();
    }
  });
}
