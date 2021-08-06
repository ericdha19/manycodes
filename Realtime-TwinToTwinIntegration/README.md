# Introduction 
This project is focused on providing a connection between the data collected by Devices Connected to IoT and creating an instance of Digital Twins in Azure. Many times we have concentrated a series of many data in IoT and to have greater control and create a network or data graph we can use Digital Twins.
Of course, for the above, Digital Twins requires us to create a generic model depending on the purpose of our data and how it is going to be stacked by certain characteristics.

This project aims to base ourselves on an architecture in which data is consumed from Cosmos DB that is acquired from IoT and said data goes through a transformation to be digested by Azure Digital Twins. This component can be modified to adapt several models or create a new one depending on the requirement, but it provides the bases to Create, Update a Digital Twin and also the Examples of how to create Models.

# Getting Started
We review in deep with the next topics
1.	Architecture
2.	Azure Resources
3.	Code
4.	Configuration of Azure Digital Twins
5.- Extras

# 1. Architecture

![Architecture](../Documentation/Architecture.jpg)

In the image we have the next elements:

    1.- Cosmos DB Change Feed

    This is the data that we process when one property change in that DB, the trigger of change is recived for the point 2.

    2.- Azure Function 1

    This Functions have the responsability to recive the data from the feed and validate that is valid against the model that we need. If is Valid we create a event in the event hub with the model we need to procees.

    3.- Event Hub

    This component is the core that recive events and subscriptions to process the data of devices that acompplish the criteria to Digital Twins. 

    4.- White List Cache

    This component is an Azure Redis Cache Component that contains a white-list with ids of devices that we can procees. This resource is reading by Azure Funtion 2

    5.- Azure Function 2

    This components is the end of the line, this function cath the evenst from the Event Hub and transform to the final model to Digital Twins, after some validations this Create or Update the data.

This segmentation provide the way to change and adapt enay component and many cases reutilize the code in diferents points if the flow.

# 2. Azure Resources

For the implementation and delivery of this project, we use a enviroment in Azure in a only one resource group.

We use the next type of Resources

1.- Azure Function

2.- Azure Storage

3.- Event Hub

4.- Application Insigths

5.- Key Vault

6.- Azure Digital Twins

7.- App Service Plan


![Resources](../Documentation/AzureResources.png)