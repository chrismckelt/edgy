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
 
data = {
    "TimeStamp": f"{datetime.datetime.now()}",
    "IsAirConditionerOn": 1,
    "Temperature": 40,
    "TagKey": "python"
}

#obj = Payload(datetime.datetime.now(), 1,0.6,"python")
sdata = json.dumps(data)
print(sdata)
HOST = "timescaledb"

print(data["TimeStamp"])
""" insert data into table """
sql = """insert into Table_001 VALUES ('{TimeStamp}', '{IsAirConditionerOn}','{Temperature}','{TagKey}')""".format(TimeStamp=data["TimeStamp"],IsAirConditionerOn=data["IsAirConditionerOn"],Temperature=data["Temperature"],TagKey=data["TagKey"])
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