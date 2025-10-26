using GlobalIO.Infrastructure.Services;
using Google.Cloud.PubSub.V1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalIO.Infrastructure.Interfaces
{
    public interface IPubSubService
    {
        Task<List<string>> GetAllTopicsAsync();
        Task<List<Subscription>> GetAllSubscriptionsAsync();
        Task<List<string>> GetAllSubscriptionsByTopicAsync(string topicId);
        Task<MensagensResult> GetMessagesAsync(string subscriptionId, int? maxMessages = null, bool confirmarMensagens = false, TimeSpan? timeout = null);
        Task<Subscription> GetSubscriptionAsync(string subscriptionId);
        Task<string> CreateTopicAsync(string topicId);
        Task<string> CreateSubscriptionAsync(string subscriptionId, string topicId);
        Task ConfirmMessagesAsync(string subscriptionId, List<string> ackIds);
        Task DeleteTopicAsync(string topicId);
        Task DeleteSubscriptionAsync(string subscriptionId);
        Task<string> PublishMessageAsync(string topicId, string message);
        string GetProjectId();
    }
}
