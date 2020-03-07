"use strict";

var Transport = require("azure-iot-device-mqtt").Mqtt;
var Client = require("azure-iot-device").ModuleClient;
var Message = require("azure-iot-device").Message;
var Chance = require("chance");

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

        var chance = new Chance();
        var counter = 1;
        var currentTemp = 20;

        var interval = setInterval(()=>{
          // const eventData =        '{"TimeStamp":"2020-02-26T03:38:07.2354044Z","IsAirConditionerOn":1,"Temperature":0.76241135306768648,"TagKey":"node"}';
            var now = new Date();
            var add = chance.floating({ min: 0, max: 1,fixed: 2  });
            currentTemp = currentTemp + add;

            if (!currentTemp) currentTemp = 1;

            const data = {
              TimeStamp : now,
              Temperature : currentTemp,
              IsAirConditionerOn:  1,
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

  if (inputName === "input1") {
    var message = msg.getBytes().toString("utf8");
    if (message) {
      var outputMsg = new Message(message);
      client.sendOutputEvent(
        "output1",
        outputMsg,
        printResultFor("Sending received message")
      );
    }
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
