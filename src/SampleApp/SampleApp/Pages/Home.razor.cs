using SampleApp.Models;

namespace SampleApp.Pages;

public partial class Home
{
    private static readonly string[] _ignoreFields =
    [
        nameof(TargetRecord.IgnoreField1),
        nameof(TargetRecord.IgnoreField2)
    ];
}
