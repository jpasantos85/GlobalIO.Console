using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalIO.Infrastructure.Behaviours.Constants
{
    public static class Variable
    {
        public static class Menu
        {
            public const string ListTopics = $"{Emoji.Known.Clipboard} List Topics";
            public const string ListSubscriptions = $"{Emoji.Known.Clipboard} List Subscriptions";
            public const string ViewMessages = $"{Emoji.Known.Eyes} View Messages";
            public const string CreateTopic = $"{Emoji.Known.Plus} Create Topic";
            public const string CreateSubscription = $"{Emoji.Known.Plus} Create Subscription";
            public const string PublishMessage = $"{Emoji.Known.EnvelopeWithArrow} Publish Message";
            public const string DeleteTopic = $"{Emoji.Known.Wastebasket} Delete Topic";
            public const string DeleteSubscription = $"{Emoji.Known.Wastebasket} Delete Subscription";
            public const string SubscriptionDetails = $"{Emoji.Known.MagnifyingGlassTiltedLeft} Subscription Details";
            public const string ClearAllMessages = $"{Emoji.Known.Broom} Clear All Messages";
            public const string HealthCheck = $"{Emoji.Known.BeatingHeart} Health Check";
            public const string Exit = $"{Emoji.Known.StopSign} Exit";
        }
    }
}
