using System;
using System.Collections.Generic;

namespace RIAYA.Models;

public partial class HealthBlog
{
    public int Id { get; set; }

    public string? Title { get; set; }

    public string? Content { get; set; }

    public string? Type { get; set; }

    public string? ContentUrl { get; set; }

    public string? Category { get; set; }

    public DateTime? PublishDate { get; set; }

    public DateTime? CreatedAt { get; set; }
}
