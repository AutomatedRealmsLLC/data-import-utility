using DataImportUtility.SampleApp.Client.Models;

namespace DataImportUtility.SampleApp.Pages;

public partial class Home
{
    // We don't currently have anything to do here since all of the logic
    // is contained in the data file mapper component. We will add some
    // logic specific to lab data files in the next step.

    private static readonly string[] _ignoreFields =
    [
        nameof(TargetRecord.IgnoreField1),
        nameof(TargetRecord.IgnoreField2)
    ];
}