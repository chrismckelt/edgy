 

# Edgy - an Azure IOT Edge Demo

This repo contains an Azure IoT Edge solution that will deploy the below modules to an IOT Edge device

A walkthrough series on how this was setup is available here:

https://dev.to/chris_mckelt/azure-iot-edge-who-is-cooler-dotnet-node-or-python-369m

# Setup instructions

https://github.com/chrismckelt/edgy/wiki
 
## Demo Goal

This solution demonstrates an air-conditioning monitoring system where 3 room sensors are publishing their temperature over time.   When a room gets too hot the air conditioner for that room is turned on. Once the room is cooled it is turned off.

Three ‘data generator’ modules publish a message with the following properties.

*   <span style="font-family: calibri light; font-size: small;">_Timestamp_ </span>
*   <span style="font-family: calibri light; font-size: small;">_Temperature_ –   room temp in Celsius</span>
*   <span style="font-family: calibri light; font-size: small;">_IsAirConditionerOn_ – true/false</span>
*   <span style="font-family: calibri light; font-size: small;">_TagKey_ – room name (in this case  dotnet, node, python)</span>

Three ‘data recorder’ modules subscribe to the published temperature messages and save the data in a time series database.

A custom module will listen to all temperature messages and analyse when a room is too hot. Sending a message to turn the rooms air conditioner on.

![](https://user-images.githubusercontent.com/662868/76138797-6afb6a80-6085-11ea-93dd-2a8fda17583a.png)

## Demo Focus Areas

1.  show local debug/development options &  remote/real deployment
2.  how to create and configure an Azure IOT Hub environment in Azure [using Azure CLI scripts](https://github.com/chrismckelt/edgy/tree/master/scripts/environment)
3.  coding custom modules in .Net, Python, NodeJS (sorry Java)
4.  using existing [Azure IoT Edge marketplace](https://aka.ms/iot-edge-marketplace) modules
5.  using non-edge marketplace modules (docker images) to save data with [Timescale](https://www.timescale.com/)
6.  connecting a data flow engine ( [Apache Nifi](https://nifi.apache.org/)) to the Edge [MQTT Broker](https://docs.microsoft.com/en-us/azure/iot-hub/iot-hub-mqtt-support)
7.  viewing the data through a [Grafana](https://grafana.com/) dashboard.

# Getting started

In order to develop solutions for the edge:

1.  Read [Developing custom modules](https://docs.microsoft.com/en-us/azure/iot-edge/how-to-vs-code-develop-module)
2.  Setup your machine using the [Azure IoT EdgeHub Dev Tool](https://github.com/Azure/iotedgedev)
3.  I recommend installing these [VS code extensions](https://marketplace.visualstudio.com/items?itemName=vsciot-vscode.azure-iot-edge)
4.  I recommend using [Portainer](https://www.portainer.io/) for docker management both locally & on the deployed edge solution

###### Portainer running on [http://localhost:9000/](http://localhost:9000/)

![](https://user-images.githubusercontent.com/662868/76701501-ae487f80-66fc-11ea-861a-2f04c19bdf56.png)

* * *

## Solution Overview

## Azure Setup

Scripts found [here](https://github.com/chrismckelt/edgy/tree/master/scripts/environment) will create out the below environment in a given resource group.

![](https://user-images.githubusercontent.com/662868/75736348-ed0f2a80-5d37-11ea-9aac-0c43c68b7911.png)

![](https://user-images.githubusercontent.com/662868/75735359-75d89700-5d35-11ea-8b46-9e5be2274d46.png)

## Modules

The code contains the docker build files , code & scripts to create the following modules

![](https://user-images.githubusercontent.com/662868/75736364-fa2c1980-5d37-11ea-99f9-42eb41fb7ea1.png)

## Code

#### Descriptions of folders and files

<table>

<tbody>

<tr>

<th>Folder /  File</th>

<th>Description</th>

</tr>

</tbody>

<tbody>

<tr>

<td>config</td>

<td>automatically generate files from the deployment.templates.json (debug or prod) that are used to deploy the solution</td>

</tr>

<tr>

<td>modules</td>

<td>custom code, docker images for your IOT Edge solution</td>

</tr>

<tr>

<td>scripts</td>

<td>code to create the environment, build the code/docker images and deploy the solution</td>

</tr>

<tr>

<td>tools</td>

<td>certificate generator and other tools for solution support</td>

</tr>

<tr>

<td>.env</td>

<td>holds environment variables that populate the generated config files from the templates</td>

</tr>

<tr>

<td>deployment.debug.template.json</td>

<td>  creates a file in the /config folder called 'deployment.debug.json' that populates environment variables, used for local development</td>

</tr>

<tr>

<td>deployment.prod.template.json</td>

<td>creates a file in the /config folder called 'deployment.prod.json' that populates environment variables, used for production like deployment</td>

</tr>

</tbody>

</table>

#### Solution Structure Overview

![](https://user-images.githubusercontent.com/662868/75339444-81f2cd80-58cb-11ea-8c08-eb485e8b5e4b.png)

## Azure IOT Edge Devices

The solution used 3 devices which will be setup in a future post.

![](https://user-images.githubusercontent.com/662868/76172689-62b14580-61d3-11ea-8dd5-26fb9c1f4d40.png)

### 1\. Local Simulator

##### A simulated local environment using the [Azure IoT Edge Hub Development simulator to run against the IOT Hub.](https://github.com/Azure/iotedgehubdev)   When developing for the edge it is recommended <span style="font-weight: bold;">not</span> to install [the real ‘IOT edge runtime’](https://docs.microsoft.com/bs-latn-ba/Azure/iot-edge/how-to-install-iot-edge-linux) on your machine but instead use the simulator.

### 2\. Local Device

##### Linux Ubuntu machine hosted in VMWare on my local machine using Hyper V

[https://docs.microsoft.com/bs-latn-ba/Azure/iot-edge/how-to-install-iot-edge-linux](https://docs.microsoft.com/bs-latn-ba/Azure/iot-edge/how-to-install-iot-edge-linux)

![](https://user-images.githubusercontent.com/662868/76173281-1d901200-61d9-11ea-9a9c-bdceacf476c9.png)

### 3\. Cloud Device

##### Linux Ubuntu hosted on Azure in our resource group [created using this script](https://github.com/chrismckelt/edgy/blob/master/scripts/environment/init.ps1)

This uses the pre-existing [Linux Ubuntu image from the Azure Marketplace](https://azuremarketplace.microsoft.com/en-us/marketplace/apps/microsoft_iot_edge.iot_edge_vm_ubuntu?tab=overview)  with the runtime installed.

Once up and running VS Code will show the devices below.

[ ![](https://user-images.githubusercontent.com/662868/76172706-870d2200-61d3-11ea-8c02-eb29f5813075.png)](https://user-images.githubusercontent.com/662868/76172706-870d2200-61d3-11ea-8c02-eb29f5813075.png "https://user-images.githubusercontent.com/662868/76172706-870d2200-61d3-11ea-8c02-eb29f5813075.png")


