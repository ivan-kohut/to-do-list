using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Text;
using TodoList.Identity.API.Events;
using TodoList.Identity.API.Options;

namespace TodoList.Identity.API.Services
{
  public class RabbitMQEventBusService : IEventBusService, IDisposable
  {
    private readonly string brokerName;
    private readonly IConnection connection;

    public RabbitMQEventBusService(IOptions<EventBusOptions> eventBusOptions)
    {
      this.brokerName = eventBusOptions.Value.BrokerName;
      this.connection = new ConnectionFactory { HostName = eventBusOptions.Value.Connection }.CreateConnection();
    }

    public void Publish(IIntegrationEvent integrationEvent)
    {
      using IModel channel = connection.CreateModel();

      channel.ExchangeDeclare(exchange: brokerName, type: ExchangeType.Direct);

      channel.BasicPublish(exchange: brokerName, routingKey: integrationEvent.GetType().Name, basicProperties: null, body: Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(integrationEvent)));
    }

    public void Dispose()
    {
      if (connection != null)
      {
        connection.Dispose();
      }
    }
  }
}
