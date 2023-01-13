# Overview

This repo contains an http triggered Azure Function (V4/.Net 6.0) that posts a notification using message cards in a designated Microsoft Teams channel when an Azure Monitor alert is fired.

![GitHub Logo](/assets/alert-message.png)

How a notification is presented in Microsoft Teams is flexible and based on a [template](#creating-a-template).

## How it works

Alert data is sent using the [common schema](https://docs.microsoft.com/en-us/azure/azure-monitor/platform/alerts-common-schema) to an instance of the Azure Function contained in this repository the data is parsed using a template and send as a [message card](https://docs.microsoft.com/en-us/outlook/actionable-messages/message-card-reference) to a Microsoft Teams channel.

![Flow](/assets/flow.png)

## Supported Azure Monitor alerts:

- [Log Alerts V2](https://docs.microsoft.com/en-us/azure/azure-monitor/alerts/alerts-common-schema-definitions#monitoringservice--log-alerts-v2)
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

| Application Setting            | Description                                     | Example value                                                                                             |
| ------------------------------ | ----------------------------------------------- | --------------------------------------------------------------------------------------------------------- |
| ConfigurationStorageConnection | A connection string to an Azure Storage account | DefaultEndpointsProtocol=https; AccountName=azuremonitor; AccountKey=xxx; EndpointSuffix=core.windows.net |
| ConfigurationFilename | Filename of the JSON configuration file | configuration.json, See the [relevant section](#Creating-a-configuration-file) |
| ContainerName | Name of the container containing the configuration file in the storage account defined by the ConfigurationStorageConnection setting. | mycontainer |

## Create Azure Monitor Alerts

Create Azure Monitor Alerts and define an Action Group that sends alerts to the configured Azure Function in the previous step as outlined in [the documentation](https://docs.microsoft.com/en-us/azure/azure-monitor/platform/action-groups#function). Make sure that using the Common alert schema is enabled!

## Creating a configuration file

The function operates based on a configuration file in json format. The json contains an array of alert definitions. Each alert is defined like this:

```json
{
  "Context": {},
  "AlertRule": "{Name of the alert rule in Azure Monitor}",
  "AlertTargetID": "{Id of the resource generating the alert}",
  "TeamsChannelConnectorWebhookUrl": "{Url of the microsoft teams channel}",
  "TeamsMessageTemplate": {},
}
```

Thre alert types need additional information, Application Insights, Log Alerts V2 and Log Analytics. This information is defined by the object value of the `Context` property:

### Application Insights specific context

```json
"Context":  {
              "ApiKey": "{api key from Application Insights}"
            }
```

ApiKey refers to an App Insights [api key](https://docs.microsoft.com/en-us/azure/bot-service/bot-service-resources-app-insights-keys?view=azure-bot-service-4.0#api-key). That key is used to execute the log query and use the results to populate the template that is used as a message to Microsoft Teams. The key must have permissions to read data.

If the context is an empty object the results won't be populated in the template.

### Log Analytics specific context

```json
"Context":  {
              "ClientSecret": "{AAD Client Secret of the app registration}",
              "ClientId": "{AAD Client Id}",
              "TenantId": "{AAD Tenant Id}",
              "RedirectUrl": "{Redirect url of the app registration}"
            },
```

Client Secret, Client Id, Tenant Id and Redirect Url refer to values retrieved by creating an App Registration in the Azure Active Directory (AAD) that allows the function to execute the log query and use the results to populate the template that is used as a message to Microsoft Teams. See [the docs](https://dev.loganalytics.io/documentation/Authorization/AAD-Setup) on how to create and link the App Registration.

If the context is an empty object the results won't be populated in the template.

### Log Alerts V2 specific context

This type of alert has the property `linkToSearchResultsAPI` point to either a log analytics api endpoint or an application insights api endpoint, based on the type of resource that is used to create the alert rule. Based on this the context should contain the api key to access the application insights api endpoint or the credentials to connect to the log analytics api endpoint.

### Message templates

The `TeamsMessageTemplate` property of an alert configuration contains a json object that defines a [Message Card](https://docs.microsoft.com/en-us/outlook/actionable-messages/message-card-reference). Unfortunately the webhook integration of Microsoft Teams does not yet support [Adaptive Cards](https://docs.microsoft.com/en-us/outlook/actionable-messages/adaptive-card). Cards can be visually designed using the [Message Card playground](https://messagecardplayground.azurewebsites.net/).

![Message Card playground](/assets/alert-message.png)

Placeholders can be defined using double squares ([[a.placeholder]]). When the function is triggered placeholders are replaced with actual values from the alert data. Placeholders are linked to the alert data using the [JSONPath](https://restfulapi.net/json-jsonpath/) . notation.

All alerts must be enabled to use the common schema as found [here](https://docs.microsoft.com/en-us/azure/azure-monitor/platform/alerts-common-schema-definitions). Use this to build the template. Each alert type will also have data specific for that type. Those can be found [here](https://docs.microsoft.com/en-us/azure/azure-monitor/platform/alerts-common-schema-definitions#alert-context).

### Example template

Suppose a template is defined this:

```json
"TeamsMessageTemplate": {
            "@type": "MessageCard",
            "@context": "https://schema.org/extensions",
            "summary": "Alert fired for rule [[$.data.essentials.alertRule]]",
            "themeColor": "0078D7",
            "title": "[[$.data.essentials.monitorCondition]]",
            "sections": [
                {
                    "facts": [
                        {
                            "name": "Name:",
                            "value": "[[$.data.alertContext.Condition.AllOf[1].MetricName]]"
                        },
                        {
                            "name": "Value:",
                            "value": "[[$.data.alertContext.Condition.AllOf[1].MetricValue]]"
                        },
                        {
                            "name": "Severity:",
                            "value": "[[$.data.essentials.severity]]"
                        }
                    ],
                    "text": "Alert fired for rule [[$.data.essentials.alertRule]]"
                }
            ]
        }
```

and an alert like this:

````json
{
    "schemaId": "azureMonitorCommonAlertSchema",
    "data": {
        "essentials": {
            "alertId": "/subscriptions/rb1658c6-4bc0-4cd9-b4fd-14ec986cceww/providers/Microsoft.AlertsManagement/alerts/1e0c65b5-9c9d-4481-9a0c-dedcbfa2b8c4",
            "alertRule": "My Metric Rule",
            "severity": "Sev3",
            "signalType": "Metric",
            "monitorCondition": "Resolved",
            "monitoringService": "Platform",
            "alertTargetIDs": [
                "/subscriptions/rb1658c6-4bc0-4cd9-b4fd-14ec986cceww/resourcegroups/default-applicationinsights-eastus/providers/microsoft.insights/components/webapp"
            ],
            "originAlertId": "rb1658c6-4bc0-4cd9-b4fd-14ec986cceww_Default-ApplicationInsights-EastUS_microsoft.insights_metricalerts_Metric_-1730944160",
            "firedDateTime": "2020-07-13T22:14:28.2359102Z",
            "resolvedDateTime": "2020-07-13T22:21:28.3200719Z",
            "description": "Metric",
            "essentialsVersion": "1.0",
            "alertContextVersion": "1.0"
        },
        "alertContext": {
            "properties": null,
            "conditionType": "SingleResourceMultipleMetricCriteria",
            "condition": {
                "windowSize": "PT5M",
                "allOf": [
                    {
                        "metricName": "requests/count",
                        "metricNamespace": "microsoft.insights/components",
                        "operator": "GreaterThan",
                        "threshold": "0",
                        "timeAggregation": "Count",
                        "dimensions": [
                            {
                                "name": "ResourceId",
                                "value": "3f429b17-443b-46b7-acba-f087713b873a"
                            }
                        ],
                        "metricValue": 0,
                        "webTestName": null
                    }
                ],
                "windowStartTime": "2020-07-13T22:13:14.654Z",
                "windowEndTime": "2020-07-13T22:18:14.654Z"
            }
        }
    }
}
````

the rendered template will look like this:

```json
"TeamsMessageTemplate": {
            "@type": "MessageCard",
            "@context": "https://schema.org/extensions",
            "summary": "Alert fired for rule My Metric Rule",
            "themeColor": "0078D7",
            "title": "Resolved",
            "sections": [
                {
                    "facts": [
                        {
                            "name": "Name:",
                            "value": "requests/count"
                        },
                        {
                            "name": "Value:",
                            "value": "0"
                        },
                        {
                            "name": "Severity:",
                            "value": "Sev3"
                        }
                    ],
                    "text": "Alert fired for rule My Metric Rule"
                }
            ]
        }
```

![Rendered Message Card](/assets/rendered-template.png)

### Example configuration file

A configuration file including multiple alert may look like this:

```json
[
    {
        "Context": {
            "ApiKey": "sdbtmr9a51rvfsrv9erqyu3bk2y5r52k1n36m5rpul"
        },
        "AlertRule": "AI alert log result",
        "AlertTargetID": "/subscriptions/rb1658c6-4bc0-4cd9-b4fd-14ec986cceww/resourcegroups/default-applicationinsights-eastus/providers/microsoft.insights/components/webapp",
        "TeamsChannelConnectorWebhookUrl": "https://outlook.office.com/webhook/99b6bcb6-9d52-5d93-966c-e62b1251556d@5d325cf5-5d9c-5b5f-9e15-3b6126f5519c/IncomingWebhook/6f51c9cf1b995a51a1cacd931ca5551a/6f5b9955-9abb-56cf-9a53-61fa2952dce9",
        "TeamsMessageTemplate": {
            "@type": "MessageCard",
            "@context": "https://schema.org/extensions",
            "summary": "Alert fired for rule [[$.data.essentials.alertRule]]",
            "themeColor": "1169D6",
            "title": "[[$.data.alertContext.SearchResults.Tables[1].Rows[1].innermostType]]",
            "sections": [
                {
                    "facts": [
                        {
                            "name": "Operation Id:",
                            "value": "[[$.data.alertContext.SearchResults.Tables[1].Rows[1].operation_Id]]"
                        }
                    ],
                    "text": "[[$.data.alertContext.SearchResults.Tables[1].Rows[1].innermostMessage]]"
                }
            ],
            "potentialAction": [
                {
                    "@type": "OpenUri",
                    "name": "View in Portal",
                    "targets": [
                        {
                            "os": "default",
                            "uri": "[[$.data.alertContext.LinkToSearchResults]]"
                        }
                    ]
                }
            ]
        }
    },
    {
        "Context": null,
        "AlertRule": "Metric",
        "AlertTargetID": "/subscriptions/rb1658c6-4bc0-4cd9-b4fd-14ec986cceww/resourcegroups/default-applicationinsights-eastus/providers/microsoft.insights/components/webapp",
        "TeamsChannelConnectorWebhookUrl": "https://outlook.office.com/webhook/99b6bcb6-9d52-5d93-966c-e62b1251556d@5d325cf5-5d9c-5b5f-9e15-3b6126f5519c/IncomingWebhook/6f51c9cf1b995a51a1cacd931ca5551a/6f5b9955-9abb-56cf-9a53-61fa2952dce9",
        "TeamsMessageTemplate": {
            "@type": "MessageCard",
            "@context": "https://schema.org/extensions",
            "summary": "Alert fired for rule [[$.data.essentials.alertRule]]",
            "themeColor": "1169D6",
            "title": "[[$.data.essentials.monitorCondition]]",
            "sections": [
                {
                    "facts": [
                        {
                            "name": "Name:",
                            "value": "[[$.data.alertContext.Condition.AllOf[1].MetricName]]"
                        },
                        {
                            "name": "Value:",
                            "value": "[[$.data.alertContext.Condition.AllOf[1].MetricValue]]"
                        },
                        {
                            "name": "Severity:",
                            "value": "[[$.data.essentials.severity]]"
                        }
                    ],
                    "text": "[[$.data.essentials.firedDateTime]]"
                }
            ]
        }
    },
    {
        "Context": {
            "ClientSecret": "gtkhf9Rld_~vryE~h~HW-616E~RIc91AS5",
            "ClientId": "d91b6cd1-9c6f-516c-b1f2-ca21c9996a6b",
            "TenantId": "91265f95-65f9-5955-95fb-e111a5c9abac",
            "RedirectUrl": "https://loganalytics"
        },
        "AlertRule": "Analytics",
        "AlertTargetID": "/subscriptions/rb1658c6-4bc0-4cd9-b4fd-14ec986cceww/resourcegroups/default-applicationinsights-eastus/providers/microsoft.insights/components/webapp-anal",
        "TeamsChannelConnectorWebhookUrl": "https://outlook.office.com/webhook/99b6bcb6-9d52-5d93-966c-e62b1251556d@5d325cf5-5d9c-5b5f-9e15-3b6126f5519c/IncomingWebhook/6f51c9cf1b995a51a1cacd931ca5551a/6f5b9955-9abb-56cf-9a53-61fa2952dce9",
        "TeamsMessageTemplate": {
            "@type": "MessageCard",
            "@context": "https://schema.org/extensions",
            "summary": "Alert fired for rule [[$.data.essentials.alertRule]]",
            "themeColor": "1169D6",
            "title": "[[$.data.alertContext.SearchResults.Tables[1].Rows[1].Resource]]",
            "sections": [
                {
                    "facts": [
                        {
                            "name": "Metric Name:",
                            "value": "[[$.data.alertContext.SearchResults.Tables[1].Rows[1].MetricName]]"
                        }
                    ],
                    "text": "[[$.data.alertContext.SearchResults.Tables[1].Rows[1].UnitName]]"
                }
            ],
            "potentialAction": [
                {
                    "@type": "OpenUri",
                    "name": "View in Portal",
                    "targets": [
                        {
                            "os": "default",
                            "uri": "[[$.data.alertContext.LinkToSearchResults]]"
                        }
                    ]
                }
            ]
        }
    }
]
```
