 

# Edgy - an Azure IOT Edge POC

## Work in progress

This repo contains an Azure IoT Edge solution that will deploy the below modules to an IOT Edge device

More info 

http://blog.mckelt.com/2020/02/13/azure-iot-edge-creating-an-edge-reporting-solution/
 
### Modules

The code contains the docker build files , code & scripts to create the following modules  

•DotNetDataGenerator     C#    
•DotNetDataRecorder      C#  
•NodeDataGenerator       Node          
•NodeDataRecorder        Node     
•PythonDataGenerator     Python     
•PythonDataRecorder      Python     
•WebApp                  C#  to see the messages      
•TimeScale DB            Database  
•Apache Nifi             Workflow      
•Grafana                 Dashboards/ Reporting    
•Apache Nifi  -- https://nifi.apache.org/  
•TimeScaleDB -- https://www.timescale.com/  
•Blob Storage – Edge marketplace  

![demo](https://user-images.githubusercontent.com/662868/75931447-f6bb9e00-5eaf-11ea-8586-bffd117a86cb.png)  

![code](https://user-images.githubusercontent.com/662868/75339444-81f2cd80-58cb-11ea-8c08-eb485e8b5e4b.png)  


![edge](https://user-images.githubusercontent.com/662868/74130071-2b736700-4c1c-11ea-938f-59df0adb6f27.png)  


####  For Self Signed Certicates use the docker image here --> [here](https://docs.microsoft.com/en-us/azure/iot-edge/tutorial-machine-learning-edge-05-configure-edge-device)
 