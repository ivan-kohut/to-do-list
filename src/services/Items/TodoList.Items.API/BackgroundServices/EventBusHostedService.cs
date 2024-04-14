using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TodoList.Items.API.Application.Commands;
using TodoList.Items.API.Application.IntegrationEvents;
using TodoList.Items.API.Options;

namespace TodoList.Items.API.BackgroundServices
{
    public class EventBusHostedService(IServiceProvider serviceProvider, IOptions<EventBusOptions> eventBusOptions) : IHostedService, IDisposable
    {
        private readonly EventBusOptions eventBusOptions = eventBusOptions.Value;

        private IConnection? connection;
        private IModel? channel;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            RetryPolicy retryPolicy = Policy
              .Handle<BrokerUnreachableException>()
              .WaitAndRetry(eventBusOptions.RetryCount!.Value, retryNumber => TimeSpan.FromSeconds(Math.Pow(2, retryNumber)), (exception, sleepDuration) => Console.WriteLine($"RabbitMQ connection retry, sleep duration: {sleepDuration}"));

            ConnectionFactory connectionFactory = new()
            {
                HostName = eventBusOptions.Connection,
                UserName = eventBusOptions.UserName,
                Password = eventBusOptions.Password,
                DispatchConsumersAsync = true
            };

            retryPolicy.Execute(() =>
            {
                connection = connectionFactory.CreateConnection();
            });

            channel = connection!.CreateModel();

            channel.ExchangeDeclare(exchange: eventBusOptions.BrokerName, type: ExchangeType.Direct);

            channel.QueueDeclare(queue: eventBusOptions.ClientName, durable: true, exclusive: false, autoDelete: false, arguments: null);

            channel.QueueBind(queue: eventBusOptions.ClientName, exchange: eventBusOptions.BrokerName, routingKey: nameof(UserCreatedIntegrationEvent));

            AsyncEventingBasicConsumer consumer = new(channel);

            consumer.Received += HandleIntegrationEvent;

            channel.BasicConsume(queue: eventBusOptions.ClientName, autoAck: false, consumer: consumer);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            channel?.Dispose();
            connection?.Dispose();
        }

        private async Task HandleIntegrationEvent(object sender, BasicDeliverEventArgs args)
        {
            UserCreatedIntegrationEvent userCreatedIntegrationEvent = JsonConvert.DeserializeObject<UserCreatedIntegrationEvent>(Encoding.UTF8.GetString(args.Body.ToArray()))
                ?? throw new Exception("UserCreated integration event is null after deserialization");

            using IServiceScope scope = serviceProvider.CreateScope();

            await scope.ServiceProvider.GetRequiredService<IMediator>().Send(new CreateUserCommand(userCreatedIntegrationEvent.UserId));

            channel!.BasicAck(deliveryTag: args.DeliveryTag, multiple: false);
        }
    }
}
