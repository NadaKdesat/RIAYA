using System;
using System.Collections.Generic;

namespace RIAYA.Models;

public partial class ServiceCategory
{
    public int Id { get; set; }

    public string? CategoryName { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Provider> Providers { get; set; } = new List<Provider>();

    public virtual ICollection<Service> Services { get; set; } = new List<Service>();
}
