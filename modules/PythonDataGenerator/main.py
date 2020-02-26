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

import ptvsd
#ptvsd.enable_attach(('127.0.0.1',  5678))

async def main():
    try:
        if not sys.version >= "3.5.3":
            raise Exception( "PythonDataGenerator requires python 3.5.3+. Current version of Python: %s" % sys.version )
        print ( "IoT Hub Client for Python" )

        module_client = None    # The client object is used to interact with your Azure IoT hub.

        connection_string = None #os.environ.get('EdgeHubConnectionString')
        print(connection_string)
        if not connection_string:
            module_client = IoTHubModuleClient.create_from_edge_environment()
        else:
            module_client = IoTHubModuleClient.create_from_connection_string(connection_string)

 #       module_client = IoTHubModuleClient.create_from_edge_environment()
        # connect the client.
        await module_client.connect()
        
        HOST = os.environ.get('TimeScaleDB.Host')

        if not HOST:
             HOST = "auazexedgexxdev.australiaeast.cloudapp.azure.com"

        #HOST = "auazexedgexxdev.australiaeast.cloudapp.azure.com"
        print(HOST)
        breaker = 1
        while breaker < 100 :
            try:
                data =  '{"TimeStamp":"2020-02-26T03:38:07.2354044Z","IsAlive":1,"Confidence":0.76241135306768648,"TagKey":"python"}'
                print(data)
                await module_client.send_message_to_output(data, "output1")
                breaker = breaker + 1
            except:
                breaker = breaker + 1
                time.sleep(10)
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