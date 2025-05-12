using System.ComponentModel.DataAnnotations;

namespace DataImportUtility.Tests.SampleData;

public class FakeTargetType
{
    public string LabAnalysisId { get; set; } = default!;
    public double? FinalAmount { get; set; }
    public string? Result { get; set; }
    public string? AnalysisId { get; set; }
    public string? RandomFieldName { get; set; }
    public double retention_time { get; set; }
    public string? SpikeAmount { get; set; }
    public string? Unit { get; set; }
    public string? Flag { get; set; }

}

public class FakeTargetTypeWithValidation
{
    [Required]
    public string LabAnalysisId { get; set; } = default!;
    public double? FinalAmount { get; set; }
    [StringLength(10)]
    public string? Result { get; set; }
    public string? AnalysisId { get; set; }
    public string? RandomFieldName { get; set; }
    public double retention_time { get; set; }
    public string? SpikeAmount { get; set; }
    public string? Unit { get; set; }
    public string? Flag { get; set; }

}
