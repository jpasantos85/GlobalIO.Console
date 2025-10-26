using GlobalIO.Infrastructure.Behaviours.Constants;
using GlobalIO.Infrastructure.Interfaces;
using Google.Cloud.PubSub.V1;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GlobalIO.Infrastructure.PubSub
{
    public class PubSubCommander
    {
        private readonly IPubSubService _pubSubService;

        public PubSubCommander(IPubSubService pubSubService)
        {
            _pubSubService = pubSubService;
        }

        public async Task ExecuteAsync()
        {
            while (true)
            {
                AnsiConsole.Clear();

                await ShowHeaderAsync();
                await ShowMenuAsync();
            }
        }

        private async Task ShowHeaderAsync()
        {
            AnsiConsole.Write(new FigletText("Google Pub/Sub Emulator").LeftJustified().Color(Color.Blue));
            AnsiConsole.MarkupLine($"[green]Connected to Emulator[/]");
            AnsiConsole.MarkupLine($"[grey]Project: {_pubSubService.GetProjectId()}[/]");
            AnsiConsole.MarkupLine($"[grey]Host: {GetEmulatorHost()}[/]\n");
            
            try
            {
                var topicos = await _pubSubService.GetAllTopicsAsync();
                var subscriptions = await _pubSubService.GetAllSubscriptionsAsync();
                AnsiConsole.MarkupLine($"[grey]Status: {Emoji.Known.CheckMarkButton} Connected - {topicos.Count} topics, {subscriptions.Count} subscriptions[/]\n");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Status: {Emoji.Known.CrossMark} Connection error - {ex.Message}[/]\n");
            }
        }

        private string GetEmulatorHost()
        {
            return Environment.GetEnvironmentVariable("PUBSUB_EMULATOR_HOST") ?? throw new ArgumentNullException("GetEmulatorHost", "Is Null Or Empty");
        }

        private async Task ShowMenuAsync()
        {
            while (true)
            {
                var command = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[yellow] Main Menu - Pub/Sub Emulator[/]")
                        .PageSize(12)
                        .AddChoices(new[] {
                        Variable.Menu.ListTopics,
                        Variable.Menu.ListSubscriptions,
                        Variable.Menu.ViewMessages,
                        Variable.Menu.CreateTopic,
                        Variable.Menu.CreateSubscription,
                        Variable.Menu.PublishMessage,
                        Variable.Menu.DeleteTopic,
                        Variable.Menu.DeleteSubscription,
                        Variable.Menu.SubscriptionDetails,
                        Variable.Menu.ClearAllMessages,
                        Variable.Menu.HealthCheck,
                        Variable.Menu.Exit
                        }));

                try
                {
                    switch (command)
                    {
                        case Variable.Menu.ListTopics:
                            await ListTopicsAsync();
                            break;

                        case Variable.Menu.ListSubscriptions:
                            await ListSubscriptionsAsync();
                            break;

                        case Variable.Menu.ViewMessages:
                            await ViewMessageAsync();
                            break;

                        case Variable.Menu.CreateTopic:
                            await CreateTopicAsync();
                            break;

                        case Variable.Menu.CreateSubscription:
                            await CreateSubscriptionAsync();
                            break;

                        case Variable.Menu.PublishMessage:
                            await PublishMessageAsync();
                            break;

                        case Variable.Menu.DeleteTopic:
                            await DeleteTopicAsync();
                            break;

                        case Variable.Menu.DeleteSubscription:
                            await DeleteSubscriptionAsync();
                            break;

                        case Variable.Menu.SubscriptionDetails:
                            await SubscriptionDetailsAsync();
                            break;

                        case Variable.Menu.ClearAllMessages:
                            await ClearAllMessagesAsync();
                            break;

                        case Variable.Menu.HealthCheck:
                            await HealthCheckAsync();
                            break;

                        case Variable.Menu.Exit:
                            await ExitAsync();
                            return;
                    }
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
                }

                if (command != Variable.Menu.Exit)
                {
                    WaitPressKey();
                }
            }
        }

        private void WaitPressKey()
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[grey]Press any key to continue...[/]");
            Console.ReadKey();
        }

        private async Task ListTopicsAsync()
        {
            AnsiConsole.MarkupLine($"[yellow]{Emoji.Known.Clipboard} Listing Topics:[/]\n");

            var table = new Table();
            table.AddColumn("Topic");
            table.AddColumn("Subscription");

            var topicos = await _pubSubService.GetAllTopicsAsync();
            foreach (var topico in topicos)
            {
                var subscriptions = await _pubSubService.GetAllSubscriptionsByTopicAsync(topico);

                table.AddRow(
                    topico,
                    subscriptions.Count > 0 ? string.Join(", ", subscriptions) : "[grey]None[/]"
                );
            }

            AnsiConsole.Write(table);
        }

        private async Task ListSubscriptionsAsync()
        {
            AnsiConsole.MarkupLine($"[yellow]{Emoji.Known.Clipboard} Listing Subscriptions:[/]\n");

            var table = new Table();
            table.AddColumn("Subscription");
            table.AddColumn("Topic");
            table.AddColumn("Ack Deadline");
            table.AddColumn("Push Endpoint");

            var subscriptions = await _pubSubService.GetAllSubscriptionsAsync();

            foreach (var subscription in subscriptions)
            {
                table.AddRow(
                    subscription.SubscriptionName.SubscriptionId,
                    subscription.TopicAsTopicName.TopicId,
                    $"{subscription.AckDeadlineSeconds}s",
                    subscription.PushConfig?.PushEndpoint ?? "[grey]Pull[/]"
                );
            }

            AnsiConsole.Write(table);
        }

        private async Task ViewMessageAsync()
        {
            var subscriptionIds = new List<string>();
            var subscriptions = await _pubSubService.GetAllSubscriptionsAsync();

            foreach (var sub in subscriptions)
            {
                subscriptionIds.Add(sub.SubscriptionName.SubscriptionId);
            }

            if (subscriptionIds.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No subscriptions found.[/]");
                return;
            }

            var subscriptionId = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select one subscription:")
                    .AddChoices(subscriptionIds));

            var maxMessages = AnsiConsole.Ask<int>("Number of messages to view:", 10);
            var confirm = AnsiConsole.Confirm("Do you want to acknowledge (ACK) messages after viewing?", false);

            AnsiConsole.MarkupLine($"[yellow]{Emoji.Known.Eyes} Viewing up to {maxMessages} messages from '{subscriptionId}':[/]\n");

            try
            {
                var resultMessage = await _pubSubService.GetMessagesAsync(subscriptionId, maxMessages, confirm);

                if (resultMessage == null)
                {
                    AnsiConsole.MarkupLine("[red]Error getting messages.[/]");
                    return;
                }

                var messages = resultMessage.Messages;

                if (messages.Count == 0)
                {
                    AnsiConsole.MarkupLine("[grey]No messages found.[/]");
                    return;
                }

                foreach (var message in messages)
                {
                    var datas = System.Text.Encoding.UTF8.GetString(message.Message.Data.ToByteArray());
                    var panel = new Panel(datas)
                    {
                        Header = new PanelHeader($"Message ID: {message.Message.MessageId}"),
                        Border = BoxBorder.Rounded
                    };

                    if (message.Message.Attributes.Count > 0)
                    {
                        var atributos = string.Join("\n",
                            message.Message.Attributes.Select(a => $"{a.Key}: {a.Value}"));
                        panel.Header = new PanelHeader($"Message ID: {message.Message.MessageId} 📎");
                        panel.Expand = true;
                    }

                    AnsiConsole.Write(panel);

                    if (!confirm)
                    {
                        AnsiConsole.MarkupLine($"[grey]ACK ID: {message.AckId}[/]");
                        AnsiConsole.WriteLine();
                    }
                }

                if (confirm)
                {
                    AnsiConsole.MarkupLine($"[green]{Emoji.Known.CheckMarkButton} {resultMessage.Total} messages viewed and confirmed![/]");
                }
                else
                {
                    AnsiConsole.MarkupLine($"[yellow]{Emoji.Known.Warning} {resultMessage.Total} viewed messages (NOT confirmed)[/]");

                    if (resultMessage.Total > 0 && AnsiConsole.Confirm("Would you like to confirm these messages now?"))
                    {
                        var ackIds = resultMessage.Messages.Select(m => m.AckId).ToList();
                        await _pubSubService.ConfirmMessagesAsync(subscriptionId, ackIds);
                        AnsiConsole.MarkupLine($"[green]{Emoji.Known.CheckMarkButton} {ackIds.Count} messages confirmed![/]");
                    }
                }

            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]{Emoji.Known.CrossMark} Error Viewing Messages: {ex.Message}[/]");
            }
        }

        private async Task CreateTopicAsync()
        {
            AnsiConsole.MarkupLine($"[yellow]{Emoji.Known.Plus} Create New Topic[/]\n");

            var topicId = AnsiConsole.Ask<string>("Topic name:");

            try
            {
                var resultTopic = await _pubSubService.CreateTopicAsync(topicId);
                AnsiConsole.MarkupLine($"[green]{Emoji.Known.CheckMarkButton} Topic '{resultTopic}' successfully created![/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]{Emoji.Known.CrossMark} Error Creating Topic: {ex.Message}[/]");
            }
        }

        private async Task CreateSubscriptionAsync()
        {
            AnsiConsole.MarkupLine($"[yellow]{Emoji.Known.Plus} Create New Subscription[/]\n");

            var topicos = await _pubSubService.GetAllTopicsAsync();
            if (topicos?.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No topics found. Create a topic first.[/]");
                return;
            }

            var topicId = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select topic:")
                    .AddChoices(topicos!));

            var subscriptionId = AnsiConsole.Ask<string>("Subscription name:");

            try
            {
                var resultado = await _pubSubService.CreateSubscriptionAsync(subscriptionId, topicId);
                AnsiConsole.MarkupLine($"[green]{Emoji.Known.CheckMarkButton} Subscription '{resultado}' successfully created![/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]{Emoji.Known.CrossMark} Error Creating Subscription: {ex.Message}[/]");
            }
        }

        private async Task DeleteTopicAsync()
        {
            AnsiConsole.MarkupLine($"[yellow]{Emoji.Known.Wastebasket} Delete Topic[/]\n");

            var topicos = await _pubSubService.GetAllTopicsAsync();
            if (topicos?.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No topics found.[/]");
                return;
            }

            var topicId = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select the topic to delete:")
                    .AddChoices(topicos!));

            if (AnsiConsole.Confirm($"[red]Are you sure you want to delete the topic '{topicId}'?[/]", false))
            {
                try
                {
                    await _pubSubService.DeleteTopicAsync(topicId);
                    AnsiConsole.MarkupLine($"[green]{Emoji.Known.CheckMarkButton} Topic '{topicId}' successfully deleted![/]");
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]{Emoji.Known.CrossMark} Error Deleting Topic: {ex.Message}[/]");
                }
            }
        }

        private async Task DeleteSubscriptionAsync()
        {
            AnsiConsole.MarkupLine($"[yellow]{Emoji.Known.Wastebasket} Delete Subscription[/]\n");

            var subscriptionIds = new List<string>();
            var subscriptions = await _pubSubService.GetAllSubscriptionsAsync();

            foreach (var sub in subscriptions)
            {
                subscriptionIds.Add(sub.SubscriptionName.SubscriptionId);
            }

            if (subscriptionIds.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No subscriptions found.[/]");
                return;
            }

            var subscriptionId = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select the subscription to delete:")
                    .AddChoices(subscriptionIds));

            if (AnsiConsole.Confirm($"[red]Are you sure you want to delete the subscription '{subscriptionId}'?[/]", false))
            {
                try
                {
                    await _pubSubService.DeleteSubscriptionAsync(subscriptionId);
                    AnsiConsole.MarkupLine($"[green]{Emoji.Known.CheckMarkButton} Subscription '{subscriptionId}' successfully deleted![/]");
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]{Emoji.Known.CrossMark} Error Deleting Subscription: {ex.Message}[/]");
                }
            }
        }

        private async Task SubscriptionDetailsAsync()
        {
            var subscriptionIds = new List<string>();
            var subscriptions = await _pubSubService.GetAllSubscriptionsAsync();

            foreach (var sub in subscriptions)
            {
                subscriptionIds.Add(sub.SubscriptionName.SubscriptionId);
            }

            if (subscriptionIds.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No subscriptions found.[/]");
                return;
            }

            var subscriptionId = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select subscription:")
                    .AddChoices(subscriptionIds));

            try
            {
                var subscription = await _pubSubService.GetSubscriptionAsync(subscriptionId);

                var table = new Table();
                table.Border = TableBorder.Rounded;

                table.AddColumn("Property");
                table.AddColumn("Value");

                table.AddRow($"{Emoji.Known.Newspaper} Subscription ID", subscription.SubscriptionName.SubscriptionId);
                table.AddRow($"{Emoji.Known.BarChart} Topic", subscription.TopicAsTopicName.TopicId);
                table.AddRow($"{Emoji.Known.AlarmClock} Ack Deadline", $"{subscription.AckDeadlineSeconds} seconds");
                table.AddRow($"{Emoji.Known.FloppyDisk} Retention", subscription.MessageRetentionDuration?.ToString() ?? "Default");
                table.AddRow($"{Emoji.Known.IncomingEnvelope} Push Endpoint", subscription.PushConfig?.PushEndpoint ?? "[grey]Pull Mode[/]");
                table.AddRow($"{Emoji.Known.MagnifyingGlassTiltedLeft} Filter", subscription.Filter ?? "[grey]None[/]");

                AnsiConsole.Write(table);
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error getting details: {ex.Message}[/]");
            }
        }

        private async Task PublishMessageAsync()
        {
            var topicos = await _pubSubService.GetAllTopicsAsync();

            if (topicos.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No topics found. Create a topic first.[/]");
                return;
            }

            var topicId = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select topic:")
                    .AddChoices(topicos));

            var mensagem = AnsiConsole.Ask<string>("Enter the message:");

            try
            {
                var messageId = await _pubSubService.PublishMessageAsync(topicId, mensagem);
                AnsiConsole.MarkupLine($"[green]{Emoji.Known.CheckMarkButton} Posted message with ID: {messageId}[/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]{Emoji.Known.CrossMark} Error Publish Message: {ex.Message}[/]");
            }
        }

        private async Task ClearAllMessagesAsync()
        {
            var subscriptions = await _pubSubService.GetAllSubscriptionsAsync();

            if (subscriptions.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No subscriptions found to clear.[/]");
                return;
            }

            if (AnsiConsole.Confirm("[red]Are you sure you want to Clear All Messages from ALL subscriptions?[/]", false))
            {
                foreach (var subscription in subscriptions)
                {
                    try
                    {
                        var mensagemResult = await _pubSubService.GetMessagesAsync(
                            subscription.SubscriptionName.SubscriptionId, 1000);

                        if (mensagemResult == null)
                        {
                            AnsiConsole.MarkupLine($"[red]Error when cleaning {subscription.SubscriptionName.SubscriptionId}.[/]");
                            continue;
                        }

                        var mensagens = mensagemResult.Messages;

                        if (mensagens.Count > 0)
                        {
                            AnsiConsole.MarkupLine($"[yellow]Cleaning {mensagens.Count} messages from {subscription.SubscriptionName.SubscriptionId}[/]");
                        }
                    }
                    catch (Exception ex)
                    {
                        AnsiConsole.MarkupLine($"[red]Error to clean {subscription.SubscriptionName.SubscriptionId}: {ex.Message}[/]");
                    }
                }
                AnsiConsole.MarkupLine($"[green]{Emoji.Known.CheckMarkButton} Cleaning completed![/]");
            }
        }

        private async Task HealthCheckAsync()
        {
            try
            {
                var topicos = await _pubSubService.GetAllTopicsAsync();
                var subscriptions = await _pubSubService.GetAllSubscriptionsAsync();

                var panel = new Panel($"{Emoji.Known.CheckMarkButton} Connection to Pub/Sub Emulator established successfully!\n\n" +
                                     $"{Emoji.Known.ChartIncreasing} Estatísticas:\n" +
                                     $"   {Emoji.Known.BackhandIndexPointingRight} Topics: {topicos.Count}\n" +
                                     $"   {Emoji.Known.BackhandIndexPointingRight} Subscriptions: {subscriptions.Count}\n" +
                                     $"   {Emoji.Known.BackhandIndexPointingRight} Project: {_pubSubService.GetProjectId()}\n" +
                                     $"   {Emoji.Known.BackhandIndexPointingRight} Host: {GetEmulatorHost()}")
                {
                    Header = new PanelHeader($"{Emoji.Known.BeatingHeart} Health Check - Status from Pub/Sub"),
                    Border = BoxBorder.Double,
                    BorderStyle = new Style(Color.Green)
                };

                AnsiConsole.Write(panel);
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]{Emoji.Known.CrossMark} Health check failure: {ex.Message}[/]");
            }
        }

        private async Task ExitAsync()
        {
            if (AnsiConsole.Confirm("[red]Are you sure you want to Exit??[/]"))
            {
                AnsiConsole.MarkupLine($"[green]{Emoji.Known.HandWithFingersSplayed} See you later![/]");
                Environment.Exit(0);
            }
        }
    }
}
