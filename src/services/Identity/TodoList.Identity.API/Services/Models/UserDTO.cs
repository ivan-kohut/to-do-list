using System.Collections.Generic;

namespace TodoList.Identity.API.Services.Models
{
    public class UserDTO
    {
        public required int Id { get; init; }

        public required string? UserName { get; init; }

        public required string? Email { get; init; }

        public required bool IsRegisteredInSystem { get; init; }

        public required bool IsEmailConfirmed { get; init; }

        public required IEnumerable<string> LoginProviders { get; init; }

        public required IEnumerable<string?> Roles { get; init; }
    }
}
