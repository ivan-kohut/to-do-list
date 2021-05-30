using TodoList.Identity.API.Events;

namespace TodoList.Identity.API.Services
{
  public interface IEventBusService
  {
    void Publish(IIntegrationEvent integrationEvent);
  }
}
