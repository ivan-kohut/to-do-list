namespace TodoList.Items.Domain.Aggregates.ItemAggregate
{
  public class Item
  {
    private int _id;
    private int _userId;
    private ItemStatus _status;
    private string _text;
    private int _priority;

    public Item(int userId, ItemStatus status, string text, int priority)
    {
      this._userId = userId;
      this._status = status;
      this._text = text;
      this._priority = priority;
    }
  }
}
