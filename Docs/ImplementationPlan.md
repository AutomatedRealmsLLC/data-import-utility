## Data Import Utility Refactoring Plan

**Goal:** Improve extensibility and modularity by separating abstractions, implementations, and UI components into different libraries.

**Phase 1: Project Structure and Core Abstractions**

*   [ ] **Create `DataImportUtility.Abstractions` project:**
    *   Move all interfaces (e.g., `IDataReaderService`, `IImportDataFileRequest`) to this project.
    *   Move base classes (e.g., `MappingRuleBase`, `ValueTransformationBase`, `ComparisonOperationBase`) to this project.
    *   Move core models/DTOs used by abstractions (e.g., `FieldMapping`, `FieldTransformation`, `TransformationResult`, `ImportedRecordFieldDescriptor`, `ImportTableDefinition`, `ValueMap`) to this project.
    *   Move relevant enums (e.g., `MappingRuleType`, `ValueTransformationType`, `ComparisonOperationType`) to this project.
    *   Ensure this project has minimal dependencies, ideally none outside of standard .NET libraries.
*   [ ] **Create `DataImportUtility.Core` project:**
    *   This project will contain the implementations of the abstractions.
    *   Move current rule implementations (from `DataImportUtility/Rules/`) here.
    *   Move value transformation implementations (from `DataImportUtility/ValueTransformations/`) here.
    *   Move comparison operation implementations (from `DataImportUtility/ComparisonOperations/`) here.
    *   This project will reference `DataImportUtility.Abstractions`.
    *   The `Jace` dependency will likely reside here if `CalculateTransformation.cs` is moved here.
*   [ ] **Create `DataImportUtility.DataReader` project:**
    *   Move `IDataReaderService.cs` from `Abstractions` to this new project's `Abstractions` folder (or directly if it's the only abstraction).
    *   Move the `DataReaderService.cs` (currently in `DataImportUtility.Components/Services/`) to this project.
    *   This project will handle all file reading logic (CSV, Excel, etc.).
    *   This project will reference `DataImportUtility.Abstractions` (for `ImportedDataFile`, `IImportDataFileRequest` etc.).
    *   Any specific dependencies for file reading (e.g., Excel libraries) will be contained within this project.
*   [ ] **Update `DataImportUtility` (original core project):**
    *   This project might become a thin wrapper or be merged into `DataImportUtility.Core`. For now, let's assume it will be significantly slimmed down. It will reference `DataImportUtility.Abstractions`, `DataImportUtility.Core`, and `DataImportUtility.DataReader`.
    *   `ApplicationConstants.cs` might need to be re-evaluated. Some constants might move to `Abstractions` or `Core`.
    *   `Helpers` might be split across the new projects based on their usage.
*   [ ] **Update `DataImportUtility.Components` project:**
    *   This project will now reference `DataImportUtility.Abstractions`, `DataImportUtility.Core`, and `DataImportUtility.DataReader`.
    *   Update service registrations and usages to reflect the new project structure.
    *   `DataFileMapper.razor.cs` and other components will need to be updated to use the new service locations and potentially adjusted namespaces.
*   [ ] **Update `DataImportUtility.SourceGenerator` project:**
    *   This project will likely need significant changes or might be deprecated if we move away from enum-based discovery for rules and transformations.
    *   If kept, it will need to reference `DataImportUtility.Abstractions` to understand the base types.
*   [ ] **Update `DataImportUtility.Tests` project:**
    *   Update references to reflect the new project structure.
    *   Adjust namespaces and test setups.
*   [ ] **Update `SampleApp` project:**
    *   Update references.
    *   Update `Program.cs` for service registration.
    *   Adjust namespaces.
*   [ ] **Update Solution Files (`.sln`):**
    *   Add new projects to the main solution (`DataImportUtility.sln`).
    *   Ensure project dependencies are correctly configured in the solution.
*   [ ] **Address Namespace Changes:**
    *   Systematically update namespaces across all moved files and their usages.
*   [ ] **Refactor Enum-based extensibility (Post Phase 1):**
    *   Investigate replacing enum-driven discovery (e.g., in `ApplicationConstants.cs` for `MappingRuleType`, `ValueTransformationType`) with a registration-based mechanism (e.g., using dependency injection and service collection scanning for types implementing `MappingRuleBase`, `ValueTransformationBase`). This will truly enhance extensibility.

**Phase 2: Implementation Details & Refinements**

*   [ ] **Service Registration:**
    *   Implement a robust service registration mechanism. For example, `DataImportUtility.Core` could have an extension method like `services.AddDataImportCoreImplementations()` which registers all its rules and transformations.
    *   `DataImportUtility.DataReader` would have `services.AddDataReaderServices()`.
*   [ ] **Dependency Management:**
    *   Carefully review and manage NuGet package dependencies for each new project to ensure they are minimal and appropriate.
*   [ ] **Testing:**
    *   Ensure all existing tests pass after refactoring.
    *   Add new tests for the new service registration and extensibility points.
