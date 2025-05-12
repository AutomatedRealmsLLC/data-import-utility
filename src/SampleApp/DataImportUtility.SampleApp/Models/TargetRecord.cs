using System.ComponentModel.DataAnnotations;

namespace DataImportUtility.SampleApp.Models;

public class TargetRecord
{
    [Required]
    public string? LabSampleId { get; set; }
    [MaxLength(3, ErrorMessage = "Result cannot be longer than 3 characters.")]
    public string? SampleType { get; set; }
    public int DilutionFactor { get; set; }
    public string? CAS { get; set; }
    public string? IgnoreField1 { get; set; }
    public string? IgnoreField2 { get; set; }
}
