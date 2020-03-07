import json

class Payload:
  def __init__(self, timeStamp, isAirConditionerOn, temperature,tagKey):
    self.TimeStamp = timeStamp
    self.IsAirConditionerOn = isAirConditionerOn
    self.Temperature = temperature
    self.IsAirConditionerOn = isAirConditionerOn
    self.TagKey = tagKey

def toJson(self):
    return json.dumps(self.__dict__)
         