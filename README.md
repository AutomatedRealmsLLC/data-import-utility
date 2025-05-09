[Demo](https://AutomatedRealmsLLC.github.io/data-import-utility)

# data-import-utility
A library for importing data from CSV, Excel, and other structured files, and turning it into C# objects.

## Overview

The `data-import-utility` provides a flexible and extensible way to handle data import tasks in .NET applications. Key features include:

*   **Core Logic:** A robust core library (`DataImportUtility`) for handling the underlying data parsing, mapping, and transformation.
*   **Blazor Components:** A rich set of Blazor components (`DataImportUtility.Components`) for easily integrating data import functionality into your web applications. This includes a `DataFileMapper` component for a comprehensive UI experience.
*   **Extensibility:** Designed with abstractions for data reading services, comparison operations, mapping rules, and value transformations, allowing for customization and extension.
*   **Source Generator:** Includes a source generator (`DataImportUtility.SourceGenerator`) to help create mapping rules.

## Getting Started with Blazor WebApp

This guide will walk you through setting up the `data-import-utility` in a new Blazor WebApp project.

### 1. Install NuGet Packages

In your Blazor WebApp project, install the necessary NuGet packages:

```powershell
Install-Package AutomatedRealms.DataImportUtility.Components
```

This will also bring in `AutomatedRealms.DataImportUtility` as a dependency.

### 2. Configure Services

In your `Program.cs` file, you need to register the services required by the `data-import-utility`.

```csharp
// Program.cs
using DataImportUtility.Components.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents(); // Or .AddInteractiveWebAssemblyComponents() or both

// Add DataReaderService for handling file reading
builder.Services.AddDataReaderServices();

// Optionally, configure UI options for the DataFileMapper component
builder.Services.ConfigureDataFileMapperUiOptions(options =>
{
    // Example: Customize the field mapper editor component
    // options.FieldMapperEditorComponentType = typeof(MyCustomFieldMapperEditor);
});

var app = builder.Build();

// ... rest of your Program.cs
```

### 3. Add Imports

Add the following using statements to your main `_Imports.razor` file to make the components and services available throughout your Blazor app:

```razor
// _Imports.razor
// ... other usings
@using DataImportUtility.Components
@using DataImportUtility.Components.Models
@using DataImportUtility.Models
```

### 4. Use the DataFileMapper Component

You can now use the `DataFileMapper` component in your Blazor pages or components.

Here's a basic example of how to use it in a Razor component:

```razor
@page "/import-data"
@using DataImportUtility.Models // For ImportedDataFile, FieldMapping etc. if needed directly
@using System.Collections.ObjectModel // For ObservableCollection if needed

<h3>Import Data</h3>

<DataFileMapper TTargetType="MyTargetModel" OnDataImported="HandleDataImported" />

@code {
    public class MyTargetModel
    {
        public string? Name { get; set; }
        public int Age { get; set; }
        // Add other properties that match your expected data
    }

    private void HandleDataImported(IEnumerable<MyTargetModel> importedData)
    {
        // Process the imported data
        Console.WriteLine($"Successfully imported {importedData.Count()} items.");
        foreach (var item in importedData)
        {
            Console.WriteLine($"Name: {item.Name}, Age: {item.Age}");
        }
        // You might want to update UI state or save to a database here
    }

    // Optional: If you want to interact with the DataFileMapper's state more directly
    // you can provide a cascading IDataFileMapperState
    // private DataFileMapperState _dataFileMapperState = new();
}
```

**Explanation:**

*   **`TTargetType="MyTargetModel"`**: This generic parameter specifies the C# class (`MyTargetModel` in this example) that the imported data should be mapped to. You'll need to define this class with properties corresponding to the columns in your data files.
*   **`OnDataImported="HandleDataImported"`**: This event callback is triggered when the user has successfully mapped and imported data. The `HandleDataImported` method will receive an `IEnumerable<TTargetType>` containing the imported and mapped objects.

### 5. Styling

The `DataImportUtility.Components` library comes with default styling. If you want to customize the appearance, you can override the CSS. The components are designed to be themable. You can inspect the generated HTML and CSS to identify the classes to target.

The library uses SASS for its internal styling. If your project also uses SASS, you might be able to integrate more deeply.

This should give you a good starting point for integrating the `data-import-utility` into your Blazor WebApp. Refer to the `SampleApp` in this repository for a more detailed example.
