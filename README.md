# Overview
WizWiz is an Azure Function (V2) 

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
|AppInsightsAlertsToTeams.identityClientId|The client id of the user-assigned Managed Identity if applicable.|

https://messagecardplayground.azurewebsites.net/

AppInsightsAlertsToTeams.KeyVaultUrl  
AppInsightsAlertsToTeams.ApplicationInsightsApiKey  
AppInsightsAlertsToTeams.ApplicationInsightsAppId  
AppInsightsAlertsToTeams.MessageCardTemplateBaseUrl  
AppInsightsAlertsToTeams.PostToUrl  

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
