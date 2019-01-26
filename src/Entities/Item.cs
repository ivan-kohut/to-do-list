﻿namespace Entities
{
  public class Item
  {
    public int Id { get; set; }
    public int UserId { get; set; }
    public ItemStatus Status { get; set; }
    public string Text { get; set; }
    public int Priority { get; set; }

    public User User { get; set; }
  }
}
