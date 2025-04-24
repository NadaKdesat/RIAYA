using System;
using System.Collections.Generic;

namespace RIAYA.Models;

public partial class JoinU
{
    public int Id { get; set; }

    public string? FullName { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? InterestedIn { get; set; }

    public string? Message { get; set; }

    public DateTime? CreatedAt { get; set; }
}
