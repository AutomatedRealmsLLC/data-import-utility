## Data Import Utility Refactoring Plan

**Goal:** Improve extensibility and modularity by separating abstractions, implementations, and UI components into different libraries.

**Phase 1: Project Structure and Core Abstractions**

*   [Completed] **Create `AutomatedRealms.DataImportUtility.Abstractions` project:**
    *   [X] Move all interfaces (e.g., `IDataReaderService`, `IImportDataFileRequest`) to this project.
    *   [X] Move base classes (e.g., `MappingRuleBase`, `ValueTransformationBase`, `ComparisonOperationBase`) to this project.
    *   [X] Move core models/DTOs used by abstractions (e.g., `FieldMapping`, `FieldTransformation`, `TransformationResult`, `ImportedRecordFieldDescriptor`, `ImportTableDefinition`, `ValueMap`) to this project.
    *   [X] Move relevant enums (e.g., `MappingRuleType`, `ValueTransformationType`, `ComparisonOperationType`) to this project. (Assuming these are now in `AutomatedRealms.DataImportUtility.Abstractions`)
    *   [X] Ensure this project has minimal dependencies, ideally none outside of standard .NET libraries.
*   [In Progress] **Create `AutomatedRealms.DataImportUtility.Core` project:**
    *   [X] This project will contain the implementations of the abstractions.
    *   [Partially Addressed] Move current rule implementations (from `DataImportUtility/Rules/`) here.
    *   [Partially Addressed] Move value transformation implementations (from `DataImportUtility/ValueTransformations/`) here. (`CalculateTransformation.cs`, `SubstringTransformation.cs`, `RegexMatchTransformation.cs`, `MapTransformation.cs`, `InterpolateTransformation.cs`, `ConditionalTransformation.cs`, `CombineFieldsTransformation.cs` moved and refactored, `ValueTransformationBase.ApplyTransformationAsync` added, all currently known concrete implementations updated)
    *   [X] Move comparison operation implementations (from `DataImportUtility/ComparisonOperations/`) here. (All core operations created and refactored)
    *   [ ] Move custom converters (from `DataImportUtility/CustomConverters/`) here.
    *   [X] Move custom exceptions (from `DataImportUtility/CustomExceptions/`) here.
    *   [X] Move helper classes (from `DataImportUtility/Helpers/`, including `ModelValidation`) here.
    *   [X] Move `ApplicationConstants.cs` (from `DataImportUtility/`) here.
    *   [X] Move compatibility classes (e.g., `MathCompatibility.cs`) here.
    *   [X] This project will reference `AutomatedRealms.DataImportUtility.Abstractions`.
    *   [In Progress] The `Jace` dependency will likely reside here if `CalculateTransformation.cs` is moved here. (Moved `CalculateTransformation.cs`, Jace package to be verified/added to Core project)
*   [ ] **Create `AutomatedRealms.DataImportUtility.DataReader` project:**
    *   Move `IDataReaderService.cs` from `AutomatedRealms.DataImportUtility.Abstractions` to this new project's `Abstractions` folder (or directly if it's the only abstraction).
    *   Move the `DataReaderService.cs` (currently in `DataImportUtility.Components/Services/`) to this project.
    *   This project will handle all file reading logic (CSV, Excel, etc.).
    *   This project will reference `AutomatedRealms.DataImportUtility.Abstractions` (for `ImportedDataFile`, `IImportDataFileRequest` etc.).
    *   Any specific dependencies for file reading (e.g., Excel libraries) will be contained within this project.
*   [In Progress] **Update `DataImportUtility` (original core project `AutomatedRealms.DataImportUtility`):**
    *   This project might become a thin wrapper or be merged into `AutomatedRealms.DataImportUtility.Core`. For now, let's assume it will be significantly slimmed down. It will reference `AutomatedRealms.DataImportUtility.Abstractions`, `AutomatedRealms.DataImportUtility.Core`, and `AutomatedRealms.DataImportUtility.DataReader`.
    *   [Partially Addressed] `ApplicationConstants.cs` - Moved to `AutomatedRealms.DataImportUtility.Core`.
    *   [Partially Addressed] `Helpers` - Relevant parts moved to `AutomatedRealms.DataImportUtility.Core`.
*   [ ] **Update `AutomatedRealms.DataImportUtility.Components` project:**
    *   This project will now reference `AutomatedRealms.DataImportUtility.Abstractions`, `AutomatedRealms.DataImportUtility.Core`, and `AutomatedRealms.DataImportUtility.DataReader`.
    *   Update service registrations and usages to reflect the new project structure.
    *   `DataFileMapper.razor.cs` and other components will need to be updated to use the new service locations and potentially adjusted namespaces.
*   [ ] **Update `AutomatedRealms.DataImportUtility.SourceGenerator` project:**
    *   This project will likely need significant changes or might be deprecated if we move away from enum-based discovery for rules and transformations.
    *   If kept, it will need to reference `AutomatedRealms.DataImportUtility.Abstractions` to understand the base types.
*   [ ] **Update `AutomatedRealms.DataImportUtility.Tests` project:**
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
    *   Implement a robust service registration mechanism. For example, `AutomatedRealms.DataImportUtility.Core` could have an extension method like `services.AddDataImportCoreImplementations()` which registers all its rules and transformations.
    *   `AutomatedRealms.DataImportUtility.DataReader` would have `services.AddDataReaderServices()`.
*   [ ] **Dependency Management:**
    *   Carefully review and manage NuGet package dependencies for each new project to ensure they are minimal and appropriate.
*   [ ] **Testing:**
    *   Ensure all existing tests pass after refactoring.
    *   Add new tests for the new service registration and extensibility points.
