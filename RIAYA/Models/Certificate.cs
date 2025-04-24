using System;
using System.Collections.Generic;

namespace RIAYA.Models;

public partial class Certificate
{
    public int Id { get; set; }

    public int? ProviderId { get; set; }

    public string? Title { get; set; }

    public string? Institution { get; set; }

    public int? YearObtained { get; set; }

    public string? CertificateUrl { get; set; }

    public virtual Provider? Provider { get; set; }
}
