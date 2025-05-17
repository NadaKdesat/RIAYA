using System;
using System.Collections.Generic;

namespace RIAYA.Models;

public partial class Contact
{
    public int Id { get; set; }

    public string? FullName { get; set; }

    public string? Email { get; set; }

    public string? Subject { get; set; }

    public string? Message { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool? IsRead { get; set; }

    public bool? IsReplied { get; set; }

    public string? ReplyMessage { get; set; }

    public DateTime? RepliedAt { get; set; }
}
