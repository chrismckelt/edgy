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

    print('PythonDataGenerator started...')
    for i in range(1,60):
        print('waiting for nifi')
        time.sleep(1) # wait for nifi
    
    try:
        # Inputs/Ouputs are only supported in the context of Azure IoT Edge and module client
        # The module client object acts as an Azure IoT Edge module and interacts with an Azure IoT Edge hub
        module_client = IoTHubModuleClient.create_from_edge_environment() # The client object is used to interact with your Azure IoT hub.
        #conn_str = os.getenv("EdgeHubConnectionString")
        #module_client = IoTHubModuleClient.create_from_connection_string(conn_str)
        await module_client.connect()

        aircon_active = False
        current_temp = 20
        temp_change = 1.5
        state = [0, 20] # mutable object used via async funcs

        # define behavior for receiving an input message on input1
        async def send_message(module_client, state):

            i = 1
            while i < 100 :
                aircon_active = bool(state[0])
                current_temp = int(state[1])
                try:
                #data =  '{"TimeStamp":"2020-02-26T03:38:07.2354044Z","IsAirConditionerOn":1,"Temperature":0.76241135306768648,"TagKey":"python"}'
                    temp_change = random.uniform(0, 1.5)  # randomise the temp change
                    #print("sending message #" + str(i))
                    if aircon_active == True:
                        if current_temp > 18:
                            current_temp = current_temp - temp_change
                            print(f"aircon on. descreasing by {temp_change} Temp: {current_temp}")
                        else:
                            current_temp = 18
                            print('aircon too cold. turning off')
                            state[0] = 0;
                    else:        

                        if current_temp > 30:
                            print('OVERHEATING')

                        current_temp = current_temp + temp_change
                        print(f"aircon off. increasing by {temp_change} Temp: {current_temp}")
                    
                    #sdata =  '{"TimeStamp":"{dt}","IsAirConditionerOn":{b},"Temperature":{current_temp},"TagKey":"python"}'.format(dt=datetime.utcnow().strftime('%B %d %Y - %H:%M:%S'), b=current_temp, current_temp=current_temp)

                    sdata = '{"TimeStamp":"AAA","IsAirConditionerOn" : "BBB","Temperature": CCC,"TagKey":"python"}' 
                    sdata = sdata.replace("AAA", datetime.datetime.utcnow().isoformat())
                    sdata = sdata.replace("BBB", str(aircon_active))
                    sdata = sdata.replace("CCC", str(round(current_temp, 2)))
                    state[1] = current_temp
                    #sdata = json.dumps(data)
                 
                    msg = Message(sdata)
                    msg.message_id = uuid.uuid4()
                    
                    await module_client.send_message_to_output(msg, "output1")
                    print('sent ' + sdata)
                    #print("done sending message #" + str(i))
                    i = i + 1
                    time.sleep(10)
                except:
                    print("Unexpected error:", sys.exc_info()[0])
                    raise

        
                 # define behavior for receiving an input message on input1
        async def input1_listener(module_client,state):
            while True:
                input_message = await module_client.receive_message_on_input("input1")  # blocking call
               
                print("message received from nifi")
                print(input_message.data)
                
                payload = json.loads(input_message.data)
                print(payload["IsAirConditionerOn"])
                print(payload["Temperature"])

                if  bool(payload["IsAirConditionerOn"]) and float(payload["Temperature"])> 25:
                    state[0] = 1
                    print("################# ActivateAirCon #################")

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