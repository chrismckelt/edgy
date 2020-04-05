"use strict";

var Transport = require("azure-iot-device-mqtt").Mqtt;
var Client = require("azure-iot-device").ModuleClient;
var Message = require("azure-iot-device").Message;
var Chance = require("chance");

const _airconActive = false
var _tempChange = 0;

Client.fromEnvironment(Transport, function(err, client) {
  if (err) {
    throw err;
  } else {
    client.on("error", function(err) {
      throw err;
    });

    // connect to the Edge instance
    client.open(function(err) {
      if (err) {
        throw err;
      } else {
        console.log("IoT Hub module client initialized");

        var currentTemp = 20;
        var chance = new Chance();
        var counter = 1;
       
         // Act on input messages to the module.
         client.on("inputMessage", function(inputName, msg) {
          console.log(`message received ${msg}`);
          pipeMessage(client, inputName, msg, pool);
        });

        var interval = setInterval(()=>{
          // const eventData =        '{"TimeStamp":"2020-02-26T03:38:07.2354044Z","IsAirConditionerOn":1,"Temperature":0.76241135306768648,"TagKey":"node"}';
            var utc = new Date().getUTCDate();
            _tempChange = chance.floating({ min: 0, max: 1.5,fixed: 2  });
            currentTemp = currentTemp + _tempChange;

            if (_airconActive)
            {
                // air con is active - cool down
                if (currentTemp >= 18)
                {
                    currentTemp = currentTemp - _tempChange;
                    console.log(`aircon on. descreasing by ${_tempChange} Temp: ${currentTemp}`);
                }
                else
                {
                    // fix the tempat 18 if it goes below
                    currentTemp = 18;
                    _airconActive = false;// turn off
                    console.log(`aircon too cold. turning off`);
                }

            }
            else
            {
                // air con OFF increase the heat 
                currentTemp = currentTemp + chance.Double(1, _tempChange);
                console.log(`aircon off. increasing by ${_tempChange} Temp: ${currentTemp}`);
            }

            const data = {
              TimeStamp : utc,
              Temperature : currentTemp,
              IsAirConditionerOn:  _airconActive,
              TagKey : "node"
            }
            var json = JSON.stringify(data);

            console.log(`Sending message: ${json}`);
            var outputMsg = new Message(json);

            client.sendOutputEvent(
              "output1",
              outputMsg,
              printResultFor("Sending " + outputMsg)
            );

            if (counter === 1000)   clearInterval(interval);

        }, 1000)
      }
    });
  }
});

// Helper function to print results in the console
function printResultFor(op) {
  return function printResult(err, res) {
    if (err) {
      console.log(op + " error: " + err.toString());
    }
    if (res) {
      console.log(op + " status: " + res.constructor.name);
    }
  };
}

// This function just pipes the messages without any change.
function pipeMessage(client, inputName, msg) {
  client.complete(msg, printResultFor("Receiving message"));
  var message = msg.getBytes().toString("utf8");
  if (inputName === "input1") {
     console.log("################# ActivateAirCon #################")
     console.log(message);
  }
}

// Helper function to print results in the console
function printResultFor(op) {
  return function printResult(err, res) {
    if (err) {
      console.log(op + " error: " + err.toString());
    }
    if (res) {
      console.log(op + " status: " + res.constructor.name);
    }
  };
}
