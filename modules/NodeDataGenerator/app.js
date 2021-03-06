"use strict";

var Transport = require("azure-iot-device-mqtt").Mqtt;
var Client = require("azure-iot-device").ModuleClient;
var Message = require("azure-iot-device").Message;
var Chance = require("chance");

var _airconActive = false;
var _tempChange = 0.5;

Client.fromEnvironment(Transport, function (err, client) {
  if (err) {
    throw err;
  } else {
    client.on("error", function (err) {
      throw err;
    });

    // connect to the Edge instance
    client.open(function (err) {
      if (err) {
        throw err;
      } else {
        console.log("IoT Hub module client initialized");

        // Act on input messages to the module.
        client.on("inputMessage", function (inputName, msg) {
          console.log(`message received ${msg}`);
          pipeMessage(client, inputName, msg);
        });

        // block until nifi ready
        console.log("waiting for nifi", 1000);
        setTimeout(() => start(client), 1000);

        start(client);
      }
    });
  }
});

function start(client) {
  var currentTemp = 20;
  var counter = 1;
  var chance = new Chance();

  var interval = setInterval(() => {
    // const eventData =        '{"TimeStamp":"2020-02-26T03:38:07.2354044Z","IsAirConditionerOn":1,"Temperature":0.76241135306768648,"TagKey":"node"}';
    var utc = new Date();
    _tempChange = chance.floating({ min: 0, max: 1.5, fixed: 2 });

    if (_airconActive) {
      // air con is active - cool down
      if (currentTemp >= 18) {
        currentTemp = currentTemp - _tempChange;
        console.log(
          `aircon on. descreasing by ${_tempChange} Temp: ${currentTemp}`
        );
      } else {
        // fix the temp at 18 if it goes below
        currentTemp = 18;
        _airconActive = false; // turn off
        console.log(`aircon too cold. turning off`);
      }
    } else {

      if (currentTemp > 30){
        console.warn('OVERHEATING');
      }
      // air con OFF increase the heat
      currentTemp = currentTemp + _tempChange;
      console.log(
        `aircon off. increasing by ${_tempChange} Temp: ${currentTemp}`
      );
    }
    
    currentTemp = Math.round(currentTemp * 100) / 100;

    const data = {
      TimeStamp: "'" + utc.toISOString() + "'",
      Temperature: currentTemp,
      IsAirConditionerOn: _airconActive,
      TagKey: "node",
    };
    var json = JSON.stringify(data);

    var outputMsg = new Message(json);

    client.sendOutputEvent(
      "output1",
      outputMsg,
      printResultFor("sent " + JSON.stringify(outputMsg))
    );

    if (counter >= 100) clearInterval(interval);
  }, 10000);
}

// Helper function to print results in the console
function printResultFor(op) {
  return function printResult(err, res) {
    if (err) {
      console.log(op + " error: " + err.toString());
    }
    if (res) {
      console.log(op);
    }
  };
}

// This function just pipes the messages without any change.
function pipeMessage(client, inputName, msg) {
  client.complete(msg, printResultFor("receiving message from nifi"));
  var message = msg.getBytes().toString("utf8");
  if (inputName === "input1") {
    var payload = JSON.parse(message);

    if (payload.TagKey != "node") {
      return;
    }

    if (
      payload.IsAirConditionerOn == false &&
      payload.Temperature > 25
    ) {
      console.log("################# ActivateAirCon #################");
      console.log(message);
      _airconActive = true;
    }
  }
}
