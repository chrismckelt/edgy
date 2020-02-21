ssh azureuser@$ip 

# create config dir and give access   --> see ftp.ps1 to send files to /config
sudo mkdir /config

# certs
# https://docs.microsoft.com/en-us/azure/iot-edge/tutorial-machine-learning-edge-05-configure-edge-device
# build docker image in C:\dev\IoTEdgeAndMlSample
# docker run --name createcertificates --rm -v c:\config\certs:/edgeCertificates createcertificates /edgeCertificates

# setup permissions for directions and files

sudo chmod 777 -R /config
sudo touch /var/lib/iotedge/mgmt.sock
sudo touch /var/lib/iotedge/workload.sock
sudo chown iotedge:iotedge /var/run/iotedge/mgmt.sock
sudo chown iotedge:iotedge /var/run/iotedge/workload.sock
sudo chmod 660 /var/run/iotedge/mgmt.sock
sudo chmod 666 /var/run/iotedge/workload.sock

# COPY certs to NIFI keystore and restart

## LOCAL SIMULATOR
cd /config/local/nifi
cp -R * /opt/nifi/nifi-current/
keytool -import -noprompt -alias iot-edge1 -file /config/local/certs/edge-device-ca/cert/edge-device-ca-root.cert.pem -keystore /usr/local/openjdk-8/lib/security/cacerts -storepass changeit
keytool -import -noprompt -alias iot-edge2 -file /config/local/certs/edge-device-ca/cert/edge-device-ca.cert.pem -keystore /usr/local/openjdk-8/lib/security/cacerts -storepass changeit
cd /opt/nifi/nifi-current/bin
bash ./nifi.sh restart

## PRODUCTION  / local device / cloud device
unset EdgeModuleCACertificateFile
unset EdgeHubConnectionString
cd /config/nifi
cp -r * /opt/nifi/nifi-current/
keytool -import -noprompt -alias iot-edge1 -file /config/certs/azure-iot-test-only.root.ca.cert.pem -keystore /usr/local/openjdk-8/lib/security/cacerts -storepass changeit
keytool -import -noprompt -alias iot-edge2 -file /config/certs/new-edge-device.cert.pem -keystore /usr/local/openjdk-8/lib/security/cacerts -storepass changeit
keytool -import -noprompt -alias local -file /config/certs/new-edge-device-full-chain.cert.pem -keystore /usr/local/openjdk-8/lib/security/cacerts -storepass changeit
keytool -import -noprompt -alias balt -file /config/certs/BaltimoreCyberTrustRoot.crt -keystore /usr/local/openjdk-8/lib/security/cacerts -storepass changeit
cd /opt/nifi/nifi-current/bin
bash ./nifi.sh restart



