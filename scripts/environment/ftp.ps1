# ftp 

sudo apt-get install vsftpd
sudo service vsftpd restart

# edit the settings file and allow uploads
sudo nano /etc/vsftpd.conf  
##    set --> write_enabled=YES
sudo service vsftpd stop
sudo chown iotedge /var/lib/iotedge/
# permissions
sudo chmod -R -f 777 /config


sudo chmod -R -f 755 /etc/iotedge
sudo chmod -R 777 /etc/iotedge
sudo chmod +rw /etc/iotedge


sudo chmod -R 777 /var/lib/iotedge.mgmt.sock

docker exec container bash -c 'echo "$ENV_VAR"'