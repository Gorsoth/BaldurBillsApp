using System;
using System.Collections.Generic;

namespace BaldurBillsApp.Models;

public partial class AppUser
{
    public int UserId { get; set; }

    public string UserName { get; set; } = null!;

    public byte[] PasswordHash { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public DateTime? LastLogin { get; set; }
}
