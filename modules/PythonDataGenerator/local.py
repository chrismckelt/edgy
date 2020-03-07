import datetime
import random

data =  f'"TimeStamp":"{datetime.datetime.now()}","IsAirConditionerOn":1,"Temperature":{random.randint(1, 100)},"TagKey":"python"'
                #obj = Payload(datetime.datetime.now(), 1,0.6,"python")