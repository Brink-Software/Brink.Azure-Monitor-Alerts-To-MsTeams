# Overview
WizWiz is an http triggered Azure Function (V2) that posts a notification using message card in a designated Microsoft Teams channel when an Application Insights log alert is fired:



The design of the notification is flexible and is based on a template.

## Getting started

The steps involved are:

- Deploy the Azure Function
- Configuring the Microsoft Teams channel connector
- Create an Application Insights log alert
- Provisioning of a Managed Identity
- Configure the storage account
- Create template for the Teams message
- Configure the Azure Function

#### Deploy the Azure Function

Deploy the Azure Function contained in this repository. Since it is a http triggered function make note of the function url as it is needed in another step.

#### Configuring the Microsoft Teams channel connector

Create and configure an incoming webhook connector as outlined [in the documentation](https://docs.microsoft.com/en-us/microsoftteams/platform/concepts/connectors/connectors-using#setting-up-a-custom-incoming-webhook). Make note of the webhook url as it is needed in another step.

#### Create an Application Insights log alert

Create a new application insights log alert based on a custom Kusto query as outlined [in the documentation](https://docs.microsoft.com/en-us/azure/azure-monitor/platform/alerts-log).

It is important to [create a webhook action](https://docs.microsoft.com/en-us/azure/azure-monitor/platform/alerts-log-webhook) for the log alert rule, based on the [*common alert schema*](https://docs.microsoft.com/en-us/azure/azure-monitor/platform/alerts-common-schema). Use the [url of the azure function](#deploy-the-azure-function) as url of the webhook.

#### Provision a Managed Identity

The Azure Function needs to be able to connect to an azure storage account to access a json file containing the message card definition that will be posted to the configured Microsoft Teams channel. In order to do that withouth having to configure secrets in the code or in the configuration this function makes use of [Azure Managed Identities](https://docs.microsoft.com/en-us/azure/active-directory/managed-identities-azure-resources/overview). 

Assign a managed identity to the Azure Function. You can either use a new or existing user-assigned managed identity or a system managed identity. See [the documentation](https://docs.microsoft.com/en-us/azure/app-service/overview-managed-identity) for a how-to. In case of a user-assigned managed identity note the client id as it will be used in another step.  

#### Configure the storage account

The Azure Function uses a storage account to download a template that will represent a message card that is send to Microsoft Teams. Give the managed identity assigned in the previous step read permissions to the blob storage. Make sure the storage account has a blob storage container the Azure Function can use.

#### Create template for the Teams message

Create a message card template and store it in the container of the blob storage account configured in the previous step. A guide creating the template is found [here](#create-message-template).

#### Configure the Azure Function

It is now time to configure the Azure Function deployed [previously](#Deploy-the-Azure-Function). Create the following [application settings](https://docs.microsoft.com/en-us/azure/azure-functions/functions-how-to-use-azure-function-app-settings):

|Application Setting|Description|
|---|---|
|AppInsightsAlertsToTeams.KeyVaultUrl|   |
|AppInsightsAlertsToTeams.ApplicationInsightsApiKey|The API key of the Application Insights Resource used for the alerts.|
|AppInsightsAlertsToTeams.ApplicationInsightsAppId|The App Id  of the Application Insights Resource used for the alerts.|
|AppInsightsAlertsToTeams.MessageCardTemplateBaseUrl|The url of the Azure Blob Storage container.|
|AppInsightsAlertsToTeams.PostToUrl|The url of the Microsoft Teams webhook connector.|
|AppInsightsAlertsToTeams.IdentityClientId|The client id of the user-assigned Managed Identity if applicable.|

An example configuration looks like this:

```
  ...
  {
    "name": "ApplicationInsightsApiKey",
    "value": "vwnubeek5gqf3buonnwfpdgcpaalhymlrzw7subs",
    "slotSetting": false
  },
  {
    "name": "ApplicationInsightsAppId",
    "value": "47120b41-e034-41d7-8c48-a411eb07b366",
    "slotSetting": false
  },
  {
    "name": "IdentityClientId",
    "value": "1250a484-3b89-48a9-bba7-6213fe72f3b4",
    "slotSetting": false
  },
  {
    "name": "MessageCardTemplateBaseUrl",
    "value": "https://myblobstorage.blob.core.windows.net/templates",
    "slotSetting": false
  },
  {
    "name": "PostToUrl",
    "value": "https://outlook.office.com/webhook/88b6bcb7-8d51-4e83-877c-e72b1250456d@4d2324cf4-5d8c-4b4f-8e15-3b6026f4518c/IncomingWebhook/edfecdaaf410354ab5c65f5f7aea253a/7f4b9854-9bcc-46cf-9a53-60fa2842dce9",
    "slotSetting": false
  },
  ...
```




https://messagecardplayground.azurewebsites.net/



[[alert.data.essentials.alertRule]]  
[[alert.data.essentials.alertRule]]  
[[alert.data.essentials.description]]  
[[alert.data.essentials.severity]]  
[[alert.alertContext.LinkToSearchResults]]  
[[alert.alertContext.Threshold]]  
[[alert.alertContext.Operator]]  
[[alert.alertContext.SearchIntervalDurationMin]]  
[[alert.alertContext.SearchIntervalInMinutes]]  
[[alert.alertContext.SearchIntervalStartTimeUtc]]  
[[alert.alertContext.SearchIntervalEndtimeUtc]]  

[[searchResult.{(string)column.name}]]
