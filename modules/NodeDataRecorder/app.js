"use strict";

var Transport = require("azure-iot-device-mqtt").Mqtt;
var Client = require("azure-iot-device").ModuleClient;
var Message = require("azure-iot-device").Message;
const { Pool } = require('pg');

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

        const pool = new Pool({
          user: "postgres",
          host: "timescaledb",
          database: "postgres",
          password: "m5asuFHqBE",
          port: "5432"
        });

        console.log(`PG connection pool ok ${pool}`);

        // Act on input messages to the module.
        client.on("inputMessage", function(inputName, msg) {
          console.log(`message received ${msg}`);
          pipeMessage(client, inputName, msg, pool);
        });
 
      }
    });
  }
});

// This function just pipes the messages without any change.
function pipeMessage(client, inputName, msg, pool) {
  client.complete(msg, printResultFor("Receiving message"));

  var message = msg.getBytes().toString("utf8");
  if (message) {
    var m = new Message(message);
    //  '{"TimeStamp":"2020-02-26T03:38:07.2354044Z","IsAlive":1,"Temperature":0.76241135306768648,"TagKey":"node"}';
    var sql = `insert into Table_001 VALUES ('2020-02-26T03:38:07.2354044Z', 0,0.76241135306768648,'node')`
    console.log(sql);
    pool.query(
      sql,
      (err, res) => {
        console.error(err, res);
        //if (pool) pool.end();
      }
    );
  }else{
    console.log(`not utf8 ${msg}`);
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
