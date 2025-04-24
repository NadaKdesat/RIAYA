using System;
using System.Collections.Generic;

namespace RIAYA.Models;

public partial class SessionRating
{
    public int Id { get; set; }

    public string? AppointmentType { get; set; }

    public int AppointmentId { get; set; }

    public int Rating { get; set; }

    public string? Feedback { get; set; }

    public DateTime? CreatedAt { get; set; }
}
