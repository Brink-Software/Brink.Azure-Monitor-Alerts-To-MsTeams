# Overview
This repo contains an http triggered Azure Function (V3) that posts a notification using message card in a designated Microsoft Teams channel when an Azure Monitor alert is fired:

![GitHub Logo](/assets/alert-message.png)

How a notification is presented in Microsoft Teams is flexible and based on a [template](#creating-a-template).

## How it works
Alert data is sent using the [common schema](https://docs.microsoft.com/en-us/azure/azure-monitor/platform/alerts-common-schema) to an instance of the Azure Function contained in this repository the data is parsed using a template and send as a [message card](https://docs.microsoft.com/en-us/outlook/actionable-messages/message-card-reference) to a Microsoft Teams channel.

## Supported Azure Monitor alerts:

- [Application Insights](https://docs.microsoft.com/en-us/azure/azure-monitor/platform/alerts-common-schema-definitions#monitoringservice--application-insights)
- [Activity Log - Administrative](https://docs.microsoft.com/en-us/azure/azure-monitor/platform/alerts-common-schema-definitions#monitoringservice--activity-log---administrative)
- [Activity Log - Policy](https://docs.microsoft.com/en-us/azure/azure-monitor/platform/alerts-common-schema-definitions#monitoringservice--activity-log---policy)
- [Activity Log - Autoscale](https://docs.microsoft.com/en-us/azure/azure-monitor/platform/alerts-common-schema-definitions#monitoringservice--activity-log---autoscale)
- [Activity Log - Security](https://docs.microsoft.com/en-us/azure/azure-monitor/platform/alerts-common-schema-definitions#monitoringservice--activity-log---security)
- [Log Analytics](https://docs.microsoft.com/en-us/azure/azure-monitor/platform/alerts-common-schema-definitions#monitoringservice--log-analytics)
- [Metric](https://docs.microsoft.com/en-us/azure/azure-monitor/platform/alerts-common-schema-definitions#metric-alerts)
- [Resource Health](https://docs.microsoft.com/en-us/azure/azure-monitor/platform/alerts-common-schema-definitions#monitoringservice--resource-health)
- [Service Health](https://docs.microsoft.com/en-us/azure/azure-monitor/platform/alerts-common-schema-definitions#monitoringservice--servicehealth)

# Getting started

The steps involved are:

- Deploy the Azure Function
- Configuring the Microsoft Teams channel connector
- Configure the Azure Function
- Create Azure Monitor Alerts
- Create a configuration file

## Deploy the Azure Function

Deploy the Azure Function contained in this repository. Since it is a http triggered function make note of the function url as it is needed in another step.

## Configuring the Microsoft Teams channel connector

Create and configure an incoming webhook connector as outlined [in the documentation](https://docs.microsoft.com/en-us/microsoftteams/platform/concepts/connectors/connectors-using#setting-up-a-custom-incoming-webhook). Make note of the webhook url as it is needed in another step.

## Configure the Azure Function

It is now time to configure the Azure Function deployed [previously](#Deploy-the-Azure-Function) using the application settings. Create the following [application setting](https://docs.microsoft.com/en-us/azure/azure-functions/functions-how-to-use-azure-function-app-settings):

|Application Setting|Description|Example value
|---|---|---|
|ConfigurationStorageConnection|A connection string to an Azure Storage account|DefaultEndpointsProtocol=https; AccountName=azuremonitor; AccountKey=xxx; EndpointSuffix=core.windows.net

The blob storage must have a container named "azuremonitoralerttoteams" containing a json file named "configuration.json". See the [relevant section](#Creating-a-configuration-file)

## Create Azure Monitor Alerts

Create Azure Monitor Alerts and define an Action Group that sends alerts to the configured Azure Function in the previous step as outlined in [the documentation](https://docs.microsoft.com/en-us/azure/azure-monitor/platform/action-groups#function). Make sure that using the Common alert schema is enabled!

## Creating a configuration file

The function operates based on a configuration file in json format.

https://docs.microsoft.com/en-us/azure/azure-monitor/platform/alerts-common-schema-definitions

https://docs.microsoft.com/en-us/azure/azure-monitor/platform/alerts-common-schema-definitions#alert-context
