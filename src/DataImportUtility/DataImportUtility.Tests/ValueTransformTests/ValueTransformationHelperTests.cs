using System.Data;

using DataImportUtility.Helpers;
using DataImportUtility.Tests.SampleData;

namespace DataImportUtility.Tests.ValueTransformTests;

public class ValueTransformationHelperTests
{
    // Test the ToObject<T>(this DataRow) method with some null values that get set to DBNull.Value in place of string?
    [Fact]
    public void ToObject_WithNullValues_ReturnsObjectWithDBNullValues()
    {
        // Arrange
        var dataTable = new DataTable();

        dataTable.Columns.Add(new DataColumn("LabAnalysisId", typeof(string)) { AllowDBNull = false });
        dataTable.Columns.Add(new DataColumn("Result", typeof(string)) { AllowDBNull = true });
        dataTable.Columns.Add(new DataColumn("retention_time", typeof(double)) { AllowDBNull = false });
        dataTable.Columns.Add(new DataColumn("FinalAmount", typeof(double)) { AllowDBNull = true });

        dataTable.Rows.Add(string.Empty, DBNull.Value, 0.0d, DBNull.Value);

        // Act
        var result = dataTable.Rows[0].ToObject<FakeTargetType>();

        // Assert
        Assert.Equal(string.Empty, result.LabAnalysisId);
        Assert.Null(result.Result);
        Assert.Equal(0.0d, result.retention_time);
        Assert.Null(result.FinalAmount);
    }
}
