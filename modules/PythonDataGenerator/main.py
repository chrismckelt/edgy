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
global aircon_active
global current_temp
global temp_change
global state 

async def main():

    
    try:
        # Inputs/Ouputs are only supported in the context of Azure IoT Edge and module client
        # The module client object acts as an Azure IoT Edge module and interacts with an Azure IoT Edge hub
        module_client = IoTHubModuleClient.create_from_edge_environment() # The client object is used to interact with your Azure IoT hub.
        #conn_str = os.getenv("EdgeHubConnectionString")
        #module_client = IoTHubModuleClient.create_from_connection_string(conn_str)
        await module_client.connect()

        aircon_active = False
        current_temp = 20
        temp_change = 0.5
        state = [0, 20]

        # define behavior for receiving an input message on input1
        async def send_message(module_client, state):
            
            aircon_active = bool(state[0])
            current_temp = int(state[1])

            i = 1
            while i < 1000 :
                try:
                #data =  '{"TimeStamp":"2020-02-26T03:38:07.2354044Z","IsAirConditionerOn":1,"Temperature":0.76241135306768648,"TagKey":"python"}'
                    temp_change = 5 #chance.random.randrange(0.5, 1.5)
                    print("sending message #" + str(i))
                    if aircon_active == True:
                        if current_temp > 18:
                            current_temp = current_temp - temp_change
                            print(f"aircon on. descreasing by {temp_change} Temp: {current_temp}")
                        else:
                            current_temp = 18
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

                    state = [int(aircon_active), current_temp]
                    print(f'current temp: {current_temp}')
                    print(f'air con active: {aircon_active}')
                    print(f'timestamp: "{datetime.datetime.utcnow()}')

                    sdata = json.dumps(data)
                    print(sdata)
                    msg = Message(sdata)
                    msg.message_id = uuid.uuid4()
                    
                    await module_client.send_message_to_output(msg, "output1")
                    print("done sending message #" + str(i))
                    i = i + 1
                except:
                    print("Unexpected error:", sys.exc_info()[0])
                    raise

        
                 # define behavior for receiving an input message on input1
        async def input1_listener(module_client,state):
            while True:
                input_message = await module_client.receive_message_on_input("input1")  # blocking call
                print("################# ActivateAirCon #################")
                print("message received from nifi ")
                print(input_message.data)
                print("######################")
                print(input_message.custom_properties)
                state[0] = 1

        listeners = await asyncio.gather(input1_listener(module_client,state), send_message(module_client, state))
        
         # *[send_message(i, state) for i in range(1, messages_to_send)]

        
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