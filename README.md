# Overview
Application-Insights-To-Teams-Function is an http triggered Azure Function (V2) that posts a notification using message card in a designated Microsoft Teams channel when an Application Insights log alert is fired:

![GitHub Logo](/assets/alert-message.png)

The design of the notification is flexible and is based on a [template](#creating-a-template).

## Getting started

The steps involved are:

- Deploy the Azure Function
- Configuring the Microsoft Teams channel connector
- Configure Application Insights
- Provisioning of a Managed Identity
- Configure the storage account
- Create template for the Teams message
- Configure the Azure Function

### Deploy the Azure Function

Deploy the Azure Function contained in this repository. Since it is a http triggered function make note of the function url as it is needed in another step.

### Configuring the Microsoft Teams channel connector

Create and configure an incoming webhook connector as outlined [in the documentation](https://docs.microsoft.com/en-us/microsoftteams/platform/concepts/connectors/connectors-using#setting-up-a-custom-incoming-webhook). Make note of the webhook url as it is needed in another step.

### Configure Application Insights

There are two steps involved, the first one is to create an api key used to perform a query against the Application Insights data and the second one involves creating an alert that will be send to Microsoft Teams.

**Create Api Key**

Create an api key using the Azure Portal and make note of the api key and the application ID as they are used to configure the Azure Function. See [the documentation](https://dev.applicationinsights.io/documentation/Authorization/API-key-and-App-ID) for details.

**Create an Application Insights log alert**

Create a new application insights log alert based on a custom Kusto query as outlined [in the documentation](https://docs.microsoft.com/en-us/azure/azure-monitor/platform/alerts-log).

It is important to [create a webhook action](https://docs.microsoft.com/en-us/azure/azure-monitor/platform/alerts-log-webhook) for the log alert rule, based on the [*common alert schema*](https://docs.microsoft.com/en-us/azure/azure-monitor/platform/alerts-common-schema). Use the [url of the azure function](#deploy-the-azure-function) as url of the webhook.

### Provision a Managed Identity

The Azure Function needs to be able to connect to an azure storage account to access a json file containing the message card definition that will be posted to the configured Microsoft Teams channel. In order to do that withouth having to configure secrets in the code or in the configuration this function makes use of [Azure Managed Identities](https://docs.microsoft.com/en-us/azure/active-directory/managed-identities-azure-resources/overview). 

Assign a managed identity to the Azure Function. You can either use a new or existing user-assigned managed identity or a system managed identity. See [the documentation](https://docs.microsoft.com/en-us/azure/app-service/overview-managed-identity) for a how-to. In case of a user-assigned managed identity note the client id as it will be used in another step.  

### Configure the storage account

The Azure Function uses a storage account to download a template that will represent a message card that is send to Microsoft Teams. Give the managed identity assigned in the previous step read permissions to the blob storage. Make sure the storage account has a blob storage container the Azure Function can use.

### Create template for the Teams message

Create a message card template and store it in the container of the blob storage account configured in the previous step. A guide creating the template is found [here](#create-message-template).

### Configure the Azure Function

It is now time to configure the Azure Function deployed [previously](#Deploy-the-Azure-Function) using the application settings. The settings are prefixed with the name of the Azure Function. Create the following [application settings](https://docs.microsoft.com/en-us/azure/azure-functions/functions-how-to-use-azure-function-app-settings):

|Application Setting|Description|
|---|---|
|AppInsightsAlertsToTeams-KeyVaultUrl|The url of an Azure KeyVault. If specified other settings like the api key can be retrieved from the Key Vault|
|AppInsightsAlertsToTeams-ApiKey-xxxx-xxxx-xxxx-xxxx|The API key of the Application Insights Resource used for the alerts where xxx is the Application Insights application id.|
|AppInsightsAlertsToTeams-MessageCardTemplateBaseUrl|The url of the Azure Blob Storage container.|
|AppInsightsAlertsToTeams-PostToUrl-xxxx-xxxx-xxxx-xxxx|The url of the Microsoft Teams webhook connector where xxx is the Application Insights application id.|
|AppInsightsAlertsToTeams-IdentityClientId|The client id of the user-assigned Managed Identity used to access blob storage if applicable.|

An example configuration looks like this:

```json
  ...
  {
    "name": "AppInsightsAlertsToTeams-ApiKey-47120b41-e034-41d7-8c48-a411eb07b366",
    "value": "vwnubeek5gqf3buonnwfpdgcpaalhymlrzw7subs",
    "slotSetting": false
  },
  {
    "name": "AppInsightsAlertsToTeams-IdentityClientId",
    "value": "1250a484-3b89-48a9-bba7-6213fe72f3b4",
    "slotSetting": false
  },
  {
    "name": "AppInsightsAlertsToTeams-MessageCardTemplateBaseUrl",
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

## Creating a template

Unfortunately the webhook integration of Microsoft Teams does not yet support Adaptive Cards. Therefore, the message that is posted to Application Insights is based on [message cards](https://docs.microsoft.com/en-us/outlook/actionable-messages/message-card-reference). To visually create a template there is a [website](https://messagecardplayground.azurewebsites.net/) you can use.

In the template the following texts will be replaced with actual values of the Application Insights Alerts:

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

An example incoming alert looks like this:

```json
{
  "schemaId": "azureMonitorCommonAlertSchema",
  "data": {
    "essentials": {
      "alertId": "/subscriptions/xxxxxxxxxxxxx/providers/Microsoft.AlertsManagement/alerts/xxxxxxxxxx",
      "alertRule": "MyAlertRuleName",
      "severity": "Sev3",
      "signalType": "Log",
      "monitorCondition": "Fired",
      "monitoringService": "Application Insights",
      "alertTargetIDs": [
        "/subscriptions/xxxxxxxxxxxx/resourcegroups/xxxxxx/providers/microsoft.insights/components/xxxxxxxxxxxx"
      ],
      "originAlertId": "6460b992-863b-47ea-82f6-5bbbdd27c5f2",
      "firedDateTime": "2019-05-06T14:18:10.2539211Z",
      "description": "",
      "essentialsVersion": "1.0",
      "alertContextVersion": "1.0"
    },
    "alertContext": {
      "SearchQuery": "exceptions\n| extend customer = \"Customer\"\n| extend env = \"Production\"\n| order by timestamp desc \n| project timestamp , env, customer, problemId , type , outerMessage   , details",
      "SearchIntervalStartTimeUtc": "5/6/2019 2:13:05 PM",
      "SearchIntervalEndtimeUtc": "5/6/2019 2:18:05 PM",
      "ResultCount": 2,
      "LinkToSearchResults": "https://portal.azure.com#@80274f8xxxxxxxxxx",
      "SearchIntervalDurationMin": "5",
      "SearchIntervalInMinutes": "5",
      "Threshold": 0,
      "Operator": "Greater Than",
      "ApplicationId": "xxxxxxxxxxxxxxxxxxxx"
    }
  }
}
```

Given the example above, a template may look like this:

```json
{
  "@type": "MessageCard",
  "@context": "https://schema.org/extensions",
  "summary": "Alert fired for rule [[alert.data.essentials.alertRule]]",
  "themeColor": "0078D7",
  "title": "[[searchResult.problemId]]",
  "sections": [
    {
      "facts": [
        {
          "name": "Environment:",
          "value": "[[searchResult.env]]"
        },
        {
          "name": "Customer:",
          "value": "[[searchResult.customer]]"
        }
      ],
      "text": "[[searchResult.outerMessage]]"
    }
  ],
  "potentialAction": [
    {
      "@type": "OpenUri",
      "name": "View in Portal",
      "targets": [
        {
          "os": "default",
          "uri": "[[alert.alertContext.LinkToSearchResults]]"
        }
      ]
    }
  ]
}
```
 A special text substitution is done for the result of the Application Insights query. Any template text with the format [[searchResult.ColumnName}]] will be replaced with the actual result value. Given the above examples the query will result in a table with the columns timestamp, env, customer, problemId, type, outerMessage, details as defined by the query triggering the alert:
 
 > "exceptions\n| extend customer = \"Customer\"\n| extend env = \"Production\"\n| order by timestamp desc \n| project timestamp, env, customer, problemId, type, outerMessage, details"
 
For example, the text "[[searchResult.outerMessage]]" will be replaced with the actual value of the `outerMessage` column of the query result.

For each row of the result a message is posted to Microsoft Teams.
