using GlobalIO.Infrastructure.Interfaces;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalIO.Infrastructure.HealthChecks
{
    public class PubSubHealthCheck : IHealthCheck
    {
        private readonly IPubSubService _pubSubService;

        public PubSubHealthCheck(IPubSubService pubSubService)
        {
            _pubSubService = pubSubService;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var topicos = await _pubSubService.GetAllTopicsAsync();
                return HealthCheckResult.Healthy(
                    $"Connected Pub/Sub. {topicos.Count} topics found.");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy(
                    "Pub/Sub connection failed", ex);
            }
        }
    }
}
