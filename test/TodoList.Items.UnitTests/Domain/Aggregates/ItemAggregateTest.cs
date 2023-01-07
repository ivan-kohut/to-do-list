using FluentAssertions;
using System;
using System.Collections.Generic;
using TodoList.Items.Domain.Aggregates.ItemAggregate;
using Xunit;

using static FluentAssertions.FluentActions;

namespace TodoList.Items.UnitTests.Domain.Aggregates
{
    public class ItemAggregateTest
    {
        public static IEnumerable<object[]> TestIsDoneData => new List<object[]>
        {
            new object[] { ItemStatus.Todo, false },
            new object[] { ItemStatus.Done, true }
        };

        [Fact]
        public void When_CreateWithStatus_Expect_ItemCreated()
        {
            int userId = 1;
            string text = "test_text";
            int priority = 2;

            // Act
            Item actualItem = new(userId, text, priority, ItemStatus.Done);

            actualItem.Id.Should().Be(default);
            actualItem.UserId.Should().Be(userId);
            actualItem.Text.Should().BeEquivalentTo(text);
            actualItem.Priority.Should().Be(priority);
            actualItem.IsDone.Should().BeTrue();
        }

        [Fact]
        public void When_CreateWithoutStatus_Expect_ItemCreated()
        {
            int userId = 1;
            string text = "test_text";
            int priority = 2;

            // Act
            Item actualItem = new(userId, text, priority);

            actualItem.Id.Should().Be(default);
            actualItem.UserId.Should().Be(userId);
            actualItem.Text.Should().BeEquivalentTo(text);
            actualItem.Priority.Should().Be(priority);
            actualItem.IsDone.Should().BeFalse();
        }

        [Fact]
        public void When_CreateWithEmptyText_Expect_ArgumentNullException()
        {
            // Act
            Invoking(() => { Item item = new(1, string.Empty, 2); })
                .Should()
                .ThrowExactly<ArgumentNullException>();
        }

        [Theory]
        [MemberData(nameof(TestIsDoneData))]
        public void Expect_IsDone(ItemStatus itemStatus, bool isDone)
        {
            new Item(1, "test_text", 2, itemStatus).IsDone.Should().Be(isDone);
        }

        [Fact]
        public void Expect_ItemUpdated()
        {
            Item item = new(1, "test_text", 2);

            string newText = "new_test_text";
            int newPriority = 12;

            item.Update(true, newText, newPriority);

            item.Text.Should().BeEquivalentTo(newText);
            item.Priority.Should().Be(newPriority);
            item.IsDone.Should().BeTrue();
        }

        [Fact]
        public void When_UpdateWithEmptyText_Expect_ArgumentNullException()
        {
            Item itemToUpdate = new(1, "test_text", 2);

            // Act
            itemToUpdate.Invoking(i => i.Update(true, string.Empty, 12))
                .Should()
                .ThrowExactly<ArgumentNullException>();
        }
    }
}
