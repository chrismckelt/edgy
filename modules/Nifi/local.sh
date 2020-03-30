### cp -R /config/local/nifi/* /opt/nifi/nifi-current/

cp -R /config/local/nifi/flowfile_repository/* /opt/nifi/nifi-current/flowfile_repository
cp -R /config/local/nifi/database_repository/* /opt/nifi/nifi-current/database_repository
cp -R /config/local/nifi/content_repository/* /opt/nifi/nifi-current/content_repository


keytool -import -noprompt -alias iot-edge1 -file /config/local/certs/edge-device-ca/cert/edge-device-ca-root.cert.pem -keystore /usr/local/openjdk-8/lib/security/cacerts -storepass changeit
keytool -import -noprompt -alias iot-edge2 -file /config/local/certs/edge-device-ca/cert/edge-device-ca.cert.pem -keystore /usr/local/openjdk-8/lib/security/cacerts -storepass changeit
cd /opt/nifi/nifi-current/bin
bash ./nifi.sh restart
