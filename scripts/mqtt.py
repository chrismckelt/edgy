from paho.mqtt import client as mqtt
import ssl

path_to_root_cert = "c:/config/local/certs/edge-device-ca/cert/edge-device-ca-root.cert.pem"
device_id = "LocalSimulator"
sas_token = "device SAS"
iot_hub_name = "auaze-edgex-dev"

def on_connect(client, userdata, flags, rc):
    print("Device connected with result code: " + str(rc))


def on_disconnect(client, userdata, rc):
    print("Device disconnected with result code: " + str(rc))

def on_publish(client, userdata, mid):
    print("Device sent message")


client = mqtt.Client(client_id=device_id, protocol=mqtt.MQTTv311)

client.on_connect = on_connect
client.on_disconnect = on_disconnect
client.on_publish = on_publish

client.username_pw_set(username=iot_hub_name+".azure-devices.net/" +
                       device_id + "/?api-version=2018-06-30", password=sas_token)

client.tls_set(ca_certs=path_to_root_cert, certfile=None, keyfile=None,
               cert_reqs=ssl.CERT_REQUIRED, tls_version=ssl.PROTOCOL_TLSv1_2, ciphers=None)
client.tls_insecure_set(False)

client.connect(iot_hub_name+".azure-devices.net", port=8883)

client.publish("devices/" + device_id + "/messages/events/", "{id=123}", qos=1)
client.loop_forever()


# # Set the username but not the password on your client
# client.username_pw_set(username=iot_hub_name+".azure-devices.net/" +
#                        device_id + "/?api-version=2018-06-30", password=None)

# # Set the certificate and key paths on your client
# cert_file = ""
# key_file = ""
# client.tls_set(ca_certs=path_to_root_cert, certfile=cert_file, keyfile=key_file,
#                cert_reqs=ssl.CERT_REQUIRED, tls_version=ssl.PROTOCOL_TLSv1_2, ciphers=None)

# # Connect as before
# client.connect(iot_hub_name+".azure-devices.net", port=8883)

