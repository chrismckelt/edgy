import datetime
import random

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
import payload
import pytz

from datetime import datetime, timezone

# data = '{ "TimeStamp": "2020-04-07 09:22:05.807444", "IsAirConditionerOn": "False", "Temperature": 29.413214813914912, "TagKey": "python" }'

# #obj = Payload(datetime.datetime.now(), 1,0.6,"python")
# sdata = json.loads(data)
# print(sdata["IsAirConditionerOn"])
 

aircon_active = True
current_temp = 21
 
 

# data = json.load(sdata)
# #sdata = '{"TimeStamp":"{dt}","IsAirConditionerOn":"{aircon_active}","Temperature":{current_temp},"TagKey":"python"}'

data =  '{"TimeStamp":"{dt}","IsAirConditionerOn":{b},"Temperature":{current_temp},"TagKey":"python"}'.format(dt=datetime.utcnow().strftime('%B %d %Y - %H:%M:%S'), b=current_temp, current_temp=current_temp)


print(sdata)