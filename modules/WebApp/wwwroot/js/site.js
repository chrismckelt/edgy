/*
Overview: site.js 
1) establishes the signalR connection and controls the response to it.
2) Inserts the layout for the rectangles drawn over areas of the 
    shelf on load into the html
3) Dynamically updates the index.html file based on incoming data
  a) Updates rectangle colors around the products
  b) Updates the text within the rectangle to "Void" or blank

*/

const TABLE_ID = '#shelf_table';
const ID_TEXT = "Text"; //Text appended to SHELF_PRODUCTS name to show/remove Void text in rectangle

const DATA_API_ENDPOINT = "/api/data/live";

const DOWN_JUSTIFY = 50;
const LEFT_JUSTIFY = 75;

const SVG = document.getElementById('dataSvg');


$(document).ready(function () {
  /*Establish signalR connection, see Startup.cs, ClientUpdateHub.cs,
    ClientNotifier.cs for more information on the signalR connection 
    and processes that are invoked.
  */
  const ws_connection = new signalR.HubConnectionBuilder().withUrl("/clientUpdateHub").build();
  /*on signalR "NewData" begin this proess*/
  ws_connection.on("NewData", function () {
        
    const route = DATA_API_ENDPOINT;

    /*jQuery .getJSON based on API call in Controllers/ShelfController.cs*/
    $.getJSON(route, function (result) {
      const data = JSON.parse(result);
      console.log(data);
     $('#data_result').append(result);
     $('#data_result').append('<br/>'); 
    });
  });

  /*If signalR cannot connect display error message in console.*/
  ws_connection.start().catch(function (err) {
    return console.error(err.toString());
  });

});
