# Design Decisions for Component Library Refactoring

This document outlines the detailed design decisions for refactoring the `AutomatedRealms.DataImportUtility.Components` library away from enum discriminators toward a more extensible interface-based approach.

## 1. Core Interface Design

### 1.1 File Type Handling

Instead of `DataFileType` enum, we'll implement a file type provider system:

```csharp
public interface IFileTypeProvider
{
    string Name { get; }
    string DisplayName { get; }
    string[] SupportedExtensions { get; }
    string[] MimeTypes { get; }
    bool CanHandleFile(string fileName, string? mimeType = null);
}

public interface IFileTypeRegistry
{
    IReadOnlyCollection<IFileTypeProvider> RegisteredFileTypes { get; }
    void RegisterFileType(IFileTypeProvider fileTypeProvider); IFileTypeProvider? GetFileTypeForFile(string fileName, string? mimeType = null); IFileTypeProvider? GetFileTypeByName(string name);
}
```

### 1.2 Import Process Flow

Replace `FileImportType` enum with a workflow interface:

```csharp
public interface IImportWorkflow
{
    string Name { get; }
    string DisplayName { get; }
    int StepCount { get; }
    Type ComponentType { get; } // The component to render for this workflow
    bool SupportsFileType(IFileTypeProvider fileType);
}

public interface IImportWorkflowRegistry
{
    IReadOnlyCollection<IImportWorkflow> RegisteredWorkflows { get; }
    void RegisterWorkflow(IImportWorkflow workflow); IImportWorkflow? GetWorkflowByName(string name); IEnumerable<IImportWorkflow> GetWorkflowsForFileType(IFileTypeProvider fileType);
}
```

### 1.3 Mapping Strategy

Replace `ColumnMappingType` enum with an interface:

```csharp
public interface IMappingStrategy
{
    string Name { get; }
    string DisplayName { get; }
    Type ComponentType { get; } // The mapping component to render
    bool SupportsFileType(IFileTypeProvider fileType);
}

public interface IMappingStrategyRegistry
{
    IReadOnlyCollection<IMappingStrategy> RegisteredStrategies { get; }
    void RegisterStrategy(IMappingStrategy strategy); IMappingStrategy? GetStrategyByName(string name); IEnumerable<IMappingStrategy> GetStrategiesForFileType(IFileTypeProvider fileType);
}
```

### 1.4 Validation Framework

Replace validation enums with interfaces:

```csharp
public interface IValidationSeverity
{
    string Name { get; }
    string DisplayName { get; }
    int SeverityLevel { get; } // For ordering/filtering (higher = more severe)
    string CssClass { get; } // For styling messages
}

public interface IValidationTrigger
{
    string Name { get; }
    string DisplayName { get; }
}

// Replace flag-based enums with a rule registry
public interface IValidationRule
{
    string Name { get; }
    string DisplayName { get; }
    IValidationSeverity DefaultSeverity { get; }
    bool IsApplicableToProperty(Type modelType, string propertyName);
}
```

## 2. Factory Pattern Implementation

### 2.1 Component Factory

```csharp
public interface IComponentFactory
{
    RenderFragment CreateComponent(Type componentType, Dictionary<string, object> parameters);
    RenderFragment<TItem> CreateComponent<TItem>(Type componentType, Dictionary<string, object> parameters);
}
```

### 2.2 Dynamic Component Registration

```csharp
public interface IComponentResolver 
{ 
    Type? ResolveComponentType(string componentKey); 
    void RegisterComponent(string componentKey, Type componentType); 
}
```


## 3. JavaScript Interop Architecture

Replace enum integer values with string constants:

```csharp
public static class FileValidationConstants
{
    public const string Success = "success";
    public const string InvalidFormat = "invalid-format";
    public const string TooLarge = "too-large";
    public const string Empty = "empty";
    // etc.
}
public static class ImportStatusConstants
{
    public const string Idle = "idle";
    public const string Uploading = "uploading";
    public const string Processing = "processing";
    public const string Mapping = "mapping";
    public const string Validating = "validating";
    public const string Completed = "completed";
    public const string Error = "error";
    // etc.
}
```

## 4. Class Relationships Diagram

```
                               ┌───────────────────┐
                               │                   │
                        ┌─────►│ IFileTypeProvider │
                        │      │                   │
                        │      └───────────────────┘
                        │               ▲
                   uses │               │implements
┌──────────────┐        │        ┌───────────────┐ 
│              │        │        │               │ 
│ FileUploader ├────────┘        │ ExcelFileType │ 
│  Component   │                 │               │ 
│              │                 └───────────────┘ 
└──────────────┘                        ▲
       ▲                                │contains
       │implements               ┌──────────────┐    
┌──────────────┐                 │              │
│              │                 │  CSVFileType │
│              │                 │              │ 
│ ImportWizard ├─────┐           └──────────────┘ 
│  Component   │     │            
│              │     │ uses           
└──────────────┘     │          ┌─────────────────┐     
                     │          │                 │
                     └─────────►│ IImportWorkflow │ 
                                │                 │ 
                                └─────────────────┘ 
                                        ▲ 
                                        │implements 
                                 ┌──────────────┐ 
                                 │              │ 
                                 │StandardImport│ 
                                 │   Workflow   │ 
                                 └──────────────┘
```

## 5. Generic Type Parameters

Replace enum-based type constraints with interface constraints:

```csharp
// Before:
public class ValidationMessage<TModel> where TModel : class
{
    [Parameter] public ValidationSeverity MinimumSeverity { get; set; } 
    // ...
}

// After:
public class ValidationMessage<TModel> where TModel : class 
{ 
    [Parameter] public IValidationSeverity MinimumSeverity { get; set; } 
    // ...
}
```


## 6. Extension Points for Consumers

### 6.1 Registration Extensions

```csharp
public static class DataImportComponentsServiceCollectionExtensions
{
    public static IServiceCollection AddDataImportComponents(this IServiceCollection services)
    { 
        // Register core services
        services.AddSingleton<IFileTypeRegistry, FileTypeRegistry>(); 
        services.AddSingleton<IImportWorkflowRegistry, ImportWorkflowRegistry>(); 
        services.AddSingleton<IMappingStrategyRegistry, MappingStrategyRegistry>(); 
        services.AddSingleton<IComponentFactory, ComponentFactory>(); 
        services.AddSingleton<IComponentResolver, ComponentResolver>();
        
        // Register built-in implementations
        services.AddSingleton<IFileTypeProvider, ExcelFileTypeProvider>();
        services.AddSingleton<IFileTypeProvider, CsvFileTypeProvider>();
        services.AddSingleton<IImportWorkflow, StandardImportWorkflow>();
        services.AddSingleton<IMappingStrategy, HeaderBasedMappingStrategy>();

        return services;
    }

    // Extension method for adding custom file types
    public static IServiceCollection AddFileType<T>(this IServiceCollection services)
        where T : class, IFileTypeProvider
    {
        services.AddSingleton<IFileTypeProvider, T>();
        return services;
    }

    // Similar extension methods for workflows, mapping strategies, etc.
}
```


### 6.2 Runtime Component Registration

```csharp
// Allow registration of components at runtime 
public interface IComponentRegistration 
{ 
    void RegisterComponent<TComponent>(string componentKey)
        where TComponent : IComponent;
    void RegisterValidator<TValidator>(string validatorKey)
        where TValidator : IComponent;
    void RegisterTransformer<TTransformer>(string transformerKey)
        where TTransformer : IComponent;
}
```

## 7. Backward Compatibility Layer

For a transitional period, provide adapter classes that convert between the new interface-based system and the old enum-based system:

```csharp
public static class FileTypeProviderExtensions 
{ 
    // Convert from old enum to new interface (for existing code) 
    public static IFileTypeProvider ToFileTypeProvider(this DataFileType fileType) 
        => fileType switch 
        { 
            DataFileType.Excel => new ExcelFileTypeProvider(),
            DataFileType.Csv => new CsvFileTypeProvider(), 
            _ => throw new ArgumentOutOfRangeException(nameof(fileType))
        };

    // Convert from new interface to old enum (for backward compatibility)
    public static DataFileType ToLegacyEnum(this IFileTypeProvider fileTypeProvider) 
        => fileTypeProvider switch
        {
            ExcelFileTypeProvider => DataFileType.Excel,
            CsvFileTypeProvider => DataFileType.Csv,
            _ => DataFileType.Unknown
        };
}
```

## 8. Conclusion

This design provides a flexible, extensible foundation for the component library without relying on enum discriminators, while still offering backward compatibility during the transition period.