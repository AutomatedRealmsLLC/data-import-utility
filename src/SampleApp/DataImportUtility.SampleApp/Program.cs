using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using DataImportUtility.SampleApp;
using DataImportUtility.Components.Extensions;
using DataImportUtility.SampleApp.OverriddenComponents;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Configure the data file mapper UI options.
builder.Services
    .ConfigureDataFileMapperUiOptions(options =>
    {
        options.FieldMapperEditorComponentType = typeof(MyFieldMapperEditor);
    });

builder.Services
    .AddDataReaderServices();

await builder.Build().RunAsync();
