using GlobalIO.Infrastructure.Interfaces;
using GlobalIO.Infrastructure.Models;
using Google.Api.Gax;
using Google.Api.Gax.Grpc;
using Google.Api.Gax.ResourceNames;
using Google.Cloud.PubSub.V1;
using Google.Protobuf;
using Microsoft.Extensions.Options;
using Spectre.Console;

namespace GlobalIO.Infrastructure.Services
{
    public class PubSubService : IPubSubService
    {
        private readonly GooglePubSubSettings _settings;
        private readonly PublisherServiceApiClient _publisherService;
        private readonly SubscriberServiceApiClient _subscriberService;

        public PubSubService(IOptions<GooglePubSubSettings> settings)
        {
            _settings = settings.Value;

            ConfigureEmulator();

            _publisherService = CreatePublisherClient();
            _subscriberService = CreateSubscriberClient();
        }

        private void ConfigureEmulator()
        {
            if (_settings.UseEmulator)
            {
                Console.WriteLine($"{Emoji.Known.HammerAndWrench} Configuring emulator: {_settings.EmulatorHost}");
                Console.WriteLine($"{Emoji.Known.SadButRelievedFace} Credentials disabled");

                Environment.SetEnvironmentVariable("PUBSUB_EMULATOR_HOST", _settings.EmulatorHost);
            }
        }

        private PublisherServiceApiClient CreatePublisherClient()
        {
            var builder = new PublisherServiceApiClientBuilder();

            if (_settings.UseEmulator)
            {
                builder.EmulatorDetection = EmulatorDetection.EmulatorOnly;
                builder.GoogleCredential = null;

                builder.Settings = new PublisherServiceApiSettings
                {
                    CallSettings = CallSettings.FromExpiration(Expiration.FromTimeout(TimeSpan.FromSeconds(_settings.TimeoutSeconds)))
                };
            }

            return builder.Build();
        }

        private SubscriberServiceApiClient CreateSubscriberClient()
        {
            var builder = new SubscriberServiceApiClientBuilder();

            if (_settings.UseEmulator)
            {
                builder.EmulatorDetection = EmulatorDetection.EmulatorOnly;
                builder.GoogleCredential = null;

                builder.Settings = new SubscriberServiceApiSettings
                {
                    CallSettings = CallSettings.FromExpiration(Expiration.FromTimeout(TimeSpan.FromSeconds(_settings.TimeoutSeconds)))
                };
            }

            return builder.Build();
        }

        public string GetProjectId() => _settings.ProjectId;

        public async Task<List<string>> GetAllTopicsAsync()
        {
            try
            {
                var projectName = ProjectName.FromProject(_settings.ProjectId);
                var topics = _publisherService.ListTopics(projectName);

                var resultado = new List<string>();
                foreach (var topic in topics)
                {
                    resultado.Add(topic.TopicName.TopicId);
                }
                return resultado;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error listing topics: {ex.Message}", ex);
            }
        }

        public async Task<List<Subscription>> GetAllSubscriptionsAsync()
        {
            try
            {
                var projectName = ProjectName.FromProject(_settings.ProjectId);
                var subscriptions = _subscriberService.ListSubscriptions(projectName);

                var resultado = new List<Subscription>();
                if (subscriptions != null)
                {
                    foreach (var subscription in subscriptions)
                    {
                        resultado.Add(subscription);
                    }
                }
                return resultado;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error listing subscriptions: {ex.Message}", ex);
            }
        }

        public async Task<List<string>> GetAllSubscriptionsByTopicAsync(string topicId)
        {
            try
            {
                var topicName = TopicName.FromProjectTopic(_settings.ProjectId, topicId);
                var subscriptions = _publisherService.ListTopicSubscriptions(topicName);

                var resultado = new List<string>();
                foreach (var subscription in subscriptions)
                {
                    var subscriptionName = SubscriptionName.Parse(subscription);
                    resultado.Add(subscriptionName.SubscriptionId);
                }
                return resultado;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error listing subscriptions from topic {topicId}: {ex.Message}", ex);
            }
        }

        public async Task<MensagensResult> GetMessagesAsync(string subscriptionId, int? maxMessages = null, bool confirmarMensagens = false, TimeSpan? timeout = null)
        {
            try
            {
                var subscriptionName = SubscriptionName.FromProjectSubscription(_settings.ProjectId, subscriptionId);
                var maxMsg = maxMessages ?? _settings.MaxMessages;

                var request = new PullRequest
                {
                    SubscriptionAsSubscriptionName = subscriptionName,
                    MaxMessages = maxMsg
                };

                var response = await _subscriberService.PullAsync(request);
                var mensagens = response.ReceivedMessages.ToList();

                if (confirmarMensagens && mensagens.Count > 0)
                {
                    var ackIds = mensagens.Select(m => m.AckId).ToList();
                    await _subscriberService.AcknowledgeAsync(subscriptionName, ackIds);
                }

                return new MensagensResult
                {
                    Messages = mensagens,
                    Total = mensagens.Count,
                    Confirmadas = confirmarMensagens
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error viewing subscription messages {subscriptionId}: {ex.Message}", ex);
            }
        }

        public async Task<Subscription> GetSubscriptionAsync(string subscriptionId)
        {
            try
            {
                var subscriptionName = SubscriptionName.FromProjectSubscription(_settings.ProjectId, subscriptionId);
                return await _subscriberService.GetSubscriptionAsync(subscriptionName);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting subscription {subscriptionId}: {ex.Message}", ex);
            }
        }

        public async Task<string> CreateTopicAsync(string topicId)
        {
            try
            {
                var topicName = TopicName.FromProjectTopic(_settings.ProjectId, topicId);
                var topic = await _publisherService.CreateTopicAsync(topicName);
                return topic.TopicName.TopicId;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating topic {topicId}: {ex.Message}", ex);
            }
        }

        public async Task<string> CreateSubscriptionAsync(string subscriptionId, string topicId)
        {
            try
            {
                var topicName = TopicName.FromProjectTopic(_settings.ProjectId, topicId);
                var subscriptionName = SubscriptionName.FromProjectSubscription(_settings.ProjectId, subscriptionId);

                var subscription = new Subscription
                {
                    SubscriptionName = subscriptionName,
                    TopicAsTopicName = topicName,
                    AckDeadlineSeconds = 60
                };

                var result = await _subscriberService.CreateSubscriptionAsync(subscription);
                return result.SubscriptionName.SubscriptionId;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating subscription {subscriptionId} to the topic {topicId}: {ex.Message}", ex);
            }
        }

        public async Task DeleteTopicAsync(string topicId)
        {
            try
            {
                var topicName = TopicName.FromProjectTopic(_settings.ProjectId, topicId);
                await _publisherService.DeleteTopicAsync(topicName);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting topic {topicId}: {ex.Message}", ex);
            }
        }

        public async Task DeleteSubscriptionAsync(string subscriptionId)
        {
            try
            {
                var subscriptionName = SubscriptionName.FromProjectSubscription(_settings.ProjectId, subscriptionId);
                await _subscriberService.DeleteSubscriptionAsync(subscriptionName);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting subscription {subscriptionId}: {ex.Message}", ex);
            }
        }

        public async Task ConfirmMessagesAsync(string subscriptionId, List<string> ackIds)
        {
            try
            {
                var subscriptionName = SubscriptionName.FromProjectSubscription(_settings.ProjectId, subscriptionId);
                await _subscriberService.AcknowledgeAsync(subscriptionName, ackIds);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error confirming subscription messages {subscriptionId}: {ex.Message}", ex);
            }
        }

        public async Task<string> PublishMessageAsync(string topicId, string message)
        {
            try
            {
                var topicName = TopicName.FromProjectTopic(_settings.ProjectId, topicId);
                var pubsubMessage = new PubsubMessage
                {
                    Data = ByteString.CopyFromUtf8(message)
                };

                var result = await _publisherService.PublishAsync(topicName, new[] { pubsubMessage });
                return result.MessageIds[0];
            }
            catch (Exception ex)
            {
                throw new Exception($"Error publishing message to topic {topicId}: {ex.Message}", ex);
            }
        }
    }

    public class MensagensResult
    {
        public List<ReceivedMessage> Messages { get; set; } = new();
        public int Total { get; set; }
        public bool Confirmadas { get; set; }
    }
}
