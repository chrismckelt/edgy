"use strict";

var Transport = require("azure-iot-device-mqtt").Mqtt;
var Client = require("azure-iot-device").ModuleClient;
var Message = require("azure-iot-device").Message;

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

        // Act on input messages to the module.
        client.on("inputMessage", function(inputName, msg) {
          pipeMessage(client, inputName, msg);
        });

        var Chance = require("chance");
        var chance = new Chance();

        for (let i = 0; i < 10; i++) {
          sleep(60000, () => {
            const eventData =
              '{"TimeStamp":"2020-02-02T10:31:15.5831884Z","ProcessedTimestamp":"2020-02-02T10:31:15.5831894Z","ValueVarchar":"276","ValueNumeric":276.0,"Confidence":90,"TagKey":"58418"}';

              const num = chance.integer({ min: 250, max: 300 });
              const data = {
                TimeStamp : chance.date(),
                ProcessedTimestamp :  chance.date(),
                ValueNumeric : num,
                ValueVarchar : num.toString(),
                Confidence : chance.integer({ min: 250, max: 300 }),
                TagKey : "58418"
              }

              var json = JSON.stringify(data);

            console.log(`Sending message: ${json}`);
            var outputMsg = new Message(eventData);

            client.sendOutputEvent(
              "output1",
              outputMsg,
              printResultFor("Sending " + outputMsg)
            );
          });
        }
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

function sleep(time, callback) {
  var stop = new Date().getTime();
  while (new Date().getTime() < stop + time) {}
  callback();
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
