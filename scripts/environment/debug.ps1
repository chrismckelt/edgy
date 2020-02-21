# https://docs.microsoft.com/en-us/azure/iot-edge/troubleshoot
# iot helpers
sudo systemctl status iotedge
sudo systemctl restart docker
sudo systemctl restart iotedge
sudo journalctl -u iotedge --no-pager --no-full

journalctl --no-pager

sudo iotedge check
sudo iotedge list
sudo iotedged -c config.yaml

#  config file
sudo cat /etc/iotedge/config.yaml
sudo nano /etc/iotedge/config.yaml

#llogs
sudo journalctl -u iotedge -f 
journalctl -u iotedge


# open ssl connect 
openssl s_client -CAfile /config/certs/new-edge-device-full-chain.cert.pem -connect edgevm:8883
openssl x509 -in azure-iot-test-only.root.ca.cert.pem -text -fingerprint | sed 's/[:]//g'
openssl s_client -connect $ip:8883 -CAfile azure-iot-test-only.root.ca.cert.pem -showcerts
openssl s_client -cert ./new-edge-device.cert.pem -key ./new-edge-device.key.pem -CApath /etc/ssl/certs/ -connect 127.0.0.1:8883

