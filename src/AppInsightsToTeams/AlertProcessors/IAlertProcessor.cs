using System.Threading.Tasks;
using AzureMonitorAlertToTeams.Models;

namespace AzureMonitorAlertToTeams.AlertProcessors
{
    public interface IAlertProcessor
    {
        ValueTask<string> CreateTeamsMessageTemplateAsync(string teamsMessageTemplate, AlertConfiguration alertConfiguration, Alert alert);
    }
}