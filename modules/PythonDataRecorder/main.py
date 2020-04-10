# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for
# full license information.

import time
import os
import sys
import json
import asyncio
from six.moves import input
import threading
from azure.iot.device.aio import IoTHubModuleClient
import psycopg2
#from double import Double

import ptvsd
#ptvsd.enable_attach(('127.0.0.1',  5678))

async def main():
    try:
        if not sys.version >= "3.5.3":
            raise Exception( "PythonDataRecorder requires python 3.5.3+. Current version of Python: %s" % sys.version )
        print ( "IoT Hub Client for Python" )

        module_client = IoTHubModuleClient.create_from_edge_environment() # The client object is used to interact with your Azure IoT hub.

 #       module_client = IoTHubModuleClient.create_from_edge_environment()
        # connect the client.
        await module_client.connect()
        
        HOST = os.environ.get('TimeScaleDB.Host')

        if not HOST:
            HOST = "auazexedgexxdev.australiaeast.cloudapp.azure.com"

        HOST = "timescaledb"

        print(HOST)
        # define behavior for receiving an input message on input1
        async def input1_listener(module_client):
          #  ptvsd.break_into_debugger()
            while True:
                payload = await module_client.receive_message_on_input("input1")  # blocking call
                print("Message received on input1")
                print( "    Data: <<{}>>".format(payload.data) )
                print( "    Properties: {}".format(payload.custom_properties))
                data = json.loads(payload.data)
                number = 0

                if bool(data["TimeStamp"]):
                    number = 1
                
                print(data["TimeStamp"])
                """ insert data into table """
                sql = """insert into Table_001 VALUES ('{TimeStamp}', '{IsAirConditionerOn}','{Temperature}','{TagKey}')""".format(TimeStamp=data["TimeStamp"],IsAirConditionerOn=number,Temperature=data["Temperature"],TagKey=data["TagKey"])
                print(sql)
                conn = None
                try:
                    conn = psycopg2.connect(host=HOST,database="postgres", user="postgres", password="m5asuFHqBE",port=5432)  #8881
                    #conn = psycopg2.connect(host=HOST,database="postgres", user="postgres", password="m5asuFHqBE",port=8081)  #8881
                    cur = conn.cursor()
                    cur.execute(sql)
                    conn.commit()
                    cur.close()
                except (Exception, psycopg2.DatabaseError) as error:
                    print(error)
                finally:
                    if conn is not None:
                        conn.close()

        #define behavior for halting the application
        def stdin_listener():
            while True:
                try:
                    selection = input("")
                    if selection == "Q" or selection == "q":
                        print("Quitting...")
                        break
                except:
                    time.sleep(10)

        # Schedule task for C2D Listener
        listeners = asyncio.gather(input1_listener(module_client))

        print ( "PythonDataRecorder is now waiting for messages. ")

        # Run the stdin listener in the event loop
        loop = asyncio.get_event_loop()
        user_finished = loop.run_in_executor(None, stdin_listener)

        # Wait for user to indicate they are done listening for messages
        await user_finished

        # Cancel listening
        listeners.cancel()

        # Finally, disconnect
        await module_client.disconnect()

    except Exception as e:
        print ( "Unexpected error %s " % e )
        raise

if __name__ == "__main__":
    loop = asyncio.get_event_loop()
    loop.run_until_complete(main())
    loop.close()

    # If using Python 3.7 or above, you can use following code instead:
    # asyncio.run(main())