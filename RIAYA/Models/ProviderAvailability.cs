using System;
using System.Collections.Generic;

namespace RIAYA.Models;

public partial class ProviderAvailability
{
    public int ProviderAvailabilityId { get; set; }

    public int ProviderId { get; set; }

    public int DayOfWeek { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }
}
