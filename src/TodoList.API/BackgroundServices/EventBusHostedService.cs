using Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Services;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BackgroundServices
{
  public class EventBusHostedService : IHostedService, IDisposable
  {
    private readonly IConnection connection;
    private readonly IModel channel;
    private readonly IServiceProvider serviceProvider;
    private readonly EventBusOptions eventBusOptions;

    public EventBusHostedService(IServiceProvider serviceProvider, IOptions<EventBusOptions> eventBusOptions)
    {
      ConnectionFactory connectionFactory = new ConnectionFactory
      {
        HostName = eventBusOptions.Value.Connection,
        UserName = eventBusOptions.Value.UserName,
        Password = eventBusOptions.Value.Password,
        DispatchConsumersAsync = true
      };

      this.connection = connectionFactory.CreateConnection();
      this.channel = connection.CreateModel();
      this.serviceProvider = serviceProvider;
      this.eventBusOptions = eventBusOptions.Value;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
      channel.ExchangeDeclare(exchange: eventBusOptions.BrokerName, type: ExchangeType.Direct);

      channel.QueueDeclare(queue: eventBusOptions.ClientName, durable: true, exclusive: false, autoDelete: false, arguments: null);

      channel.QueueBind(queue: eventBusOptions.ClientName, exchange: eventBusOptions.BrokerName, routingKey: nameof(UserCreatedIntegrationEvent));

      AsyncEventingBasicConsumer consumer = new AsyncEventingBasicConsumer(channel);

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
      UserCreatedIntegrationEvent userCreatedIntegrationEvent = JsonConvert.DeserializeObject<UserCreatedIntegrationEvent>(Encoding.UTF8.GetString(args.Body.ToArray()));

      using IServiceScope scope = serviceProvider.CreateScope();

      await scope.ServiceProvider.GetRequiredService<IUserService>().SaveAsync(userCreatedIntegrationEvent.UserId);

      channel.BasicAck(deliveryTag: args.DeliveryTag, multiple: false);
    }
  }
}
