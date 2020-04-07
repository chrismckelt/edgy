# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for
# full license information.

import time
import datetime
import os
import sys
import json
import uuid
import asyncio
from six.moves import input
import threading
from azure.iot.device.aio import IoTHubModuleClient
from azure.iot.device import Message
import psycopg2
import random
from chance import chance
import ptvsd
#ptvsd.enable_attach(('127.0.0.1',  5678))
#https://github.com/Azure/azure-iot-sdk-python/tree/master/azure-iot-device/samples/async-edge-scenarios

messages_to_send = 1000
aircon_active = False
current_temp = 20
temp_change = 0.5

async def main():
    
    aircon_active = False

    try:
        # Inputs/Ouputs are only supported in the context of Azure IoT Edge and module client
        # The module client object acts as an Azure IoT Edge module and interacts with an Azure IoT Edge hub
        module_client = IoTHubModuleClient.create_from_edge_environment() # The client object is used to interact with your Azure IoT hub.
        #conn_str = os.getenv("EdgeHubConnectionString")
        #module_client = IoTHubModuleClient.create_from_connection_string(conn_str)
        await module_client.connect()

        # define behavior for receiving an input message on input1
        async def send_test_message(i):
            print("sending message #" + str(i))
            aa = aircon_active
            try:
                #data =  '{"TimeStamp":"2020-02-26T03:38:07.2354044Z","IsAirConditionerOn":1,"Temperature":0.76241135306768648,"TagKey":"python"}'
                temp_change = chance.random.randrange(0.5, 1.5)

                if aa == True:
                    if current_temp > 18:
                        current_temp = current_temp - temp_change
                        print(f"aircon on. descreasing by {temp_change} Temp: {current_temp}")
                    else:
                        current_temp = 18
                        aircon_active = False
                        print('aircon too cold. turning off')
                else:        
                    current_temp = current_temp + temp_change
                    print(f"aircon off. increasing by {temp_change} Temp: {current_temp}")
                
                data = {
                    "TimeStamp": f"{datetime.datetime.utcnow()}",
                    "IsAirConditionerOn": aircon_active,
                    "Temperature":  current_temp,
                    "TagKey": "python"
                }

                print(f'current temp: {current_temp}')
                print(f'air con active: {aircon_active}')
                print(f'timestamp: "{datetime.datetime.utcnow()}')

                sdata = json.dumps(data)
                print(sdata)
                msg = Message(sdata)
                msg.message_id = uuid.uuid4()

                await module_client.send_message_to_output(msg, "output1")
                time.sleep(30)
                print("done sending message #" + str(i))
            except:
                print("Unexpected error:", sys.exc_info()[0])
                raise

        
                 # define behavior for receiving an input message on input1
        async def input1_listener(module_client):
            while True:
                input_message = await module_client.receive_message_on_input("input1")  # blocking call
                print("################# ActivateAirCon #################")
                print("the data in the message received on input1 was ")
                print(input_message.data)
                print("######################")
                print(input_message.custom_properties)

        listeners = await asyncio.gather(input1_listener(module_client),*[send_test_message(i) for i in range(1, messages_to_send)])

        listeners.cancel()
        # Finally, disconnect
        await module_client.disconnect()

    except Exception as e:
        print ( "Unexpected error %s " % e )
        raise

if __name__ == "__main__":
    asyncio.run(main())
    # loop = asyncio.get_event_loop()
    # loop.run_until_complete(main())
    # loop.close()

    # If using Python 3.7 or above, you can use following code instead:
    # asyncio.run(main())