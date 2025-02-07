﻿namespace DataImportUtility.Tests.SampleData;

public record ImportedRecord
{
    public string? LabSampleId { get; set; }
    public string? ClientSampleId { get; set; }
    public string? Matrix { get; set; }
    public string? SampleType { get; set; }
    public string? CollectionDate { get; set; }
    public string? PercentMoisture { get; set; }
    public string? AnalysisMethod { get; set; }
    public long DilutionFactor { get; set; }
    public string? AnalysisDate { get; set; }
    public string? Cas { get; set; }
    public string? Analyte { get; set; }
    public long Result { get; set; }
    public string? Unit { get; set; }
    public string? Flag { get; set; }
    public double HighLimit { get; set; }
    public string? HighLimitType { get; set; }
    public double LowLimit { get; set; }
    public string? LowLimitType { get; set; }
    public string? PercentRecovery { get; set; }
    public string? LowerRecoveryLimit { get; set; }
    public string? UpperRecoveryLimit { get; set; }
    public string? Rpd { get; set; }
    public string? RpdLimit { get; set; }
    public string? ReceiptLab { get; set; }
    public string? ProjectDescription { get; set; }
    public string? Sdg { get; set; }
    public string? LabJobId { get; set; }
    public string? CocId { get; set; }
    public string? SampleTypeDesc { get; set; }
    public string? ReceiveDate { get; set; }
    public string? TotalSampleAmount { get; set; }
    public string? TotalSampleAmountUnits { get; set; }
    public string? LeachBatch { get; set; }
    public string? LeachMethod { get; set; }
    public string? LeachDate { get; set; }
    public string? PrepBatch { get; set; }
    public long PrepMethod { get; set; }
    public string? PrepDate { get; set; }
    public string? PrepType { get; set; }
    public double InitialAmount { get; set; }
    public string? InitialAmountUnit { get; set; }
    public double FinalAmount { get; set; }
    public string? FinalAmountUnit { get; set; }
    public string? ReAnalysisType { get; set; }
    public string? AnalysisBatch { get; set; }
    public string? AnalysisLab { get; set; }
    public string? InstrumentId { get; set; }
    public string? ColumnDetectorId { get; set; }
    public string? Basis { get; set; }
    public string? AnalyteType { get; set; }
    public string? ResultStatus { get; set; }
    public string? Tpu { get; set; }
    public string? TpuSigma { get; set; }
    public string? DecisionLevel { get; set; }
    public double RetentionTime { get; set; }
    public string? SpikeAmount { get; set; }
    public string? ExpectedAmount { get; set; }
    public string? Rer { get; set; }
    public string? RerLimit { get; set; }
    public string? LowerBreechLimit { get; set; }
    public string? UpperBreechLimit { get; set; }
}
