# Refactoring To-Do List

This document outlines the tasks required for the data-import-utility refactoring, focusing on improving extensibility and addressing build issues.

## Phase 1: Address Initial Build Errors & Setup

1.  **Resolve `IsExternalInit` Compiler Errors (CS0518)**
    *   **Issue:** The build fails in the `AutomatedRealms.DataImportUtility.Abstractions` project because the predefined type `System.Runtime.CompilerServices.IsExternalInit` is not found. This is typically required when using init-only properties with older .NET target frameworks.
    *   **Investigation Points:**
        *   There appear to be two `IsExternalInit.cs` files:
            *   `src/DataImportUtility/AutomatedRealms.DataImportUtility.Abstractions/Compatibility/IsExternalInit.cs` (namespace `AutomatedRealms.DataImportUtility.Abstractions.Compatibility`)
            *   `src/DataImportUtility/AutomatedRealms.DataImportUtility.Abstractions/IsExternalInit.cs` (potentially with namespace `System.Runtime.CompilerServices`)
        *   The `ProjectStructure.md` only lists the file in the `Compatibility` subfolder.
    *   **Action:**
        *   Clarify which `IsExternalInit.cs` file should be used. (DONE - User confirmed `Compatibility/IsExternalInit.cs` is the one to keep)
        *   Ensure the chosen file is correctly placed in the `System.Runtime.CompilerServices` namespace. (DONE - Namespace updated)
        *   Remove the redundant or incorrectly namespaced file. (DONE - User deleted the other copy)
        *   Verify it's correctly included in the compilation of the `Abstractions` project. (DONE - Error CS0518 resolved by build)
        *   Update `ProjectStructure.md` if the location or existence of these files changes.

2.  **Resolve Missing Compiler-Required Members (CS0656) in `ValueMap.cs`** (DONE)
    *   **Issue:** The build fails in `AutomatedRealms.DataImportUtility.Abstractions\Models\ValueMap.cs` because compiler-required members are missing:
        *   `System.Runtime.CompilerServices.RequiredMemberAttribute..ctor`
        *   `System.Runtime.CompilerServices.CompilerFeatureRequiredAttribute..ctor`
    *   **Cause:** Likely due to using C# 11 'required' members feature while targeting an older .NET framework.
    *   **Action:**
        *   Utilized existing definitions in `CompilerServicesPolyfills.cs`.
        *   Moved `CompilerServicesPolyfills.cs` from `Helpers` to `Compatibility` folder. (DONE)
        *   Corrected the namespace in `CompilerServicesPolyfills.cs` to `System.Runtime.CompilerServices`. (DONE)
        *   Removed redundant `IsExternalInit` from `CompilerServicesPolyfills.cs`. (DONE)
        *   Ensured `CompilerServicesPolyfills.cs` (now in `Compatibility` folder) is correctly included in the compilation of the `Abstractions` project. (DONE - Errors resolved by build)
        *   User to delete original file from `Abstractions/Helpers/CompilerServicesPolyfills.cs`. (PENDING - User action)

3.  **Resolve Type Not Found Errors (CS0246) in Abstractions Project Helpers** (To be addressed by Task #5)
    *   **Issue:** Various concrete rule and transformation types (e.g., `CopyRule`, `IgnoreRule`, `CalculateTransformation`) cannot be found in files like `MappingRuleTypeExtensions.cs`, `ValueTransformationTypeExtensions.cs` within the `AutomatedRealms.DataImportUtility.Abstractions\Helpers` folder.
    *   **Cause:** These helper/extension methods in the `Abstractions` project attempt to directly reference or instantiate concrete types from the `Core` project using an enum-based factory pattern.
    *   **Resolution:** These errors are symptomatic of the old extensibility model. The problematic helper files and the enums they rely on for instantiation will be removed as their functionality is replaced by the new registration and `TypeId`-based mechanism detailed in Task #5.

4.  **Resolve `JsonConverter<>` Not Found Errors (CS0246)**
    *   **Issue:** The type or namespace name `JsonConverter<>` cannot be found in:
        *   `AutomatedRealms.DataImportUtility.Abstractions/CustomConverters/MappingRuleBaseConverter.cs`
        *   `AutomatedRealms.DataImportUtility.Abstractions/CustomConverters/ValueTransformationBaseConverter.cs`
    *   **Action:**
        *   Add a package reference to `System.Text.Json` (or `Newtonsoft.Json` if that's the preferred library) in the `AutomatedRealms.DataImportUtility.Abstractions.csproj` file.
        *   Add the necessary `using` directive (e.g., `using System.Text.Json.Serialization;`) in the affected files.

## Phase 2: Core Refactoring for Extensibility

5.  **Refactor Extensible Types (`MappingRuleBase`, `ValueTransformationBase`, etc.) for a Registration-Based, `TypeId`-Driven Extensibility Model**
    *   **Goal:** Transition from an enum-driven approach to a robust, performant, and developer-friendly registration-based model. This model will use a string `TypeId` for serialization and deserialization, allowing consumers to define and use their own custom implementations seamlessly, with a strong emphasis on minimizing startup performance impact.
    *   **Tasks:**
        *   **A. Define `TypeId` Property:**
            *   Add a non-nullable string property (e.g., `public string TypeId { get; /* internal set; */ }`) to base classes (`MappingRuleBase`, `ValueTransformationBase`, `ComparisonOperationBase`). This property will store a unique, human-readable identifier for the concrete type (e.g., "Core.CopyRule", "MyCompany.CustomRule").
            *   The `TypeId` should be assigned by the concrete class, possibly in its constructor or as a constant/static readonly field that the base class constructor can access.
        *   **B. Update Custom JSON Converters:**
            *   Modify `MappingRuleBaseConverter`, `ValueTransformationBaseConverter`, etc., to:
                *   **Serialization:** Write the `TypeId` string to the JSON.
                *   **Deserialization:** Read the `TypeId` from JSON. Use a central type registry (see point C) to resolve this `TypeId` to a `System.Type`. Deserialize the JSON to an instance of this resolved type.
        *   **C. Implement Type Registration Mechanism & Registry (Performance-Focused):**
            *   Design a central registry (likely managed by DI, e.g., `Dictionary<string, Type>`) that maps `TypeId` strings to `System.Type`.
            *   **Core Library Registration (Explicit & Performant):** The `.Core` library will provide DI extension methods (e.g., `services.AddDataImportUtilityCore()`) that *explicitly* register all its default implementations with their predefined `TypeId`s (e.g., `registry.Register<CopyRule>("Core.CopyRule")`). This avoids any runtime scanning for core types, ensuring fast startup.
            *   **Consumer Registration Strategies (Balancing Ease-of-Use & Performance):**
                1.  **Explicit Registration (Recommended for Control & Performance):** Provide DI extension methods for consumers to explicitly register their custom types (e.g., `services.AddMappingRule<MyCustomRule>("MyCompany.MyRule")`). This gives consumers full control and is the most performant.
                2.  **(Future/Investigate) Source Generator for Consumer Registration:** Investigate creating a source generator that runs in the *consumer's* project. When a consumer defines a class inheriting from `MappingRuleBase` (etc.), the source generator could automatically generate the explicit DI registration call for that type (e.g., into a partial class method `services.AddMyGeneratedRules()`). This would offer the ease of "declare and it works" with the performance of explicit registration, avoiding runtime scanning in consumer code.
                3.  **Opt-in Assembly Scanning (Convenience, with Caveats):** As a lower priority or alternative, optionally provide a way for consumers to register all types from a specific assembly. Clearly document the potential performance implications of assembly scanning at startup.
        *   **D. Decouple Abstractions & Remove Obsolete Enum-Based Logic:**
            *   Remove the old enum properties (e.g., `MappingRuleType` from `MappingRuleBase`) if their primary purpose was type discrimination for instantiation/serialization.
            *   Remove the enum definition files (`MappingRuleType.cs`, `ValueTransformationType.cs`, etc.) from the `Abstractions` project.
            *   Remove the associated `...Extensions.cs` files from `Abstractions/Helpers` (e.g., `MappingRuleTypeExtensions.cs`) that contain the enum-based `CreateNewInstance` logic.
        *   **E. Update UI Components:**
            *   Adapt UI components (e.g., in `AutomatedRealms.DataImportUtility.Components`) that currently rely on enums to populate selection lists. These should now dynamically query the registered types (via their `TypeId` and perhaps a display name obtained from an attribute or a descriptive property on the type) from the central registry.
        *   **F. Evaluate `AutomatedRealms.DataImportUtility.SourceGenerator` Project:**
            *   The current source generator (if its purpose was enum-related or creating the problematic `CreateNewInstance` extensions) will likely be deprecated in its current form.
            *   Its purpose could pivot to assisting with the new consumer registration model (as per point C.2 - Source Generator for Consumer Registration).

6.  **Review and Update Dependent Components**
    *   **Impact:** Changes to `MappingRuleBase` and its handling will affect other parts of the system.
    *   **Tasks:**
        *   Identify all code currently using `MappingRuleType` enum (e.g., for switch statements, factory logic).
        *   Update UI components in `AutomatedRealms.DataImportUtility.Components` that are used for configuring mapping rules to work with the new extensible system.
        *   Update core processing logic that applies these rules.
        *   Ensure test projects are updated to reflect these changes.

## Phase 3: Further Development and Cleanup

7.  **Address Subsequent Build Errors**
    *   **Action:** After resolving the initial errors in the `Abstractions` project, perform a full solution build.
    *   Systematically identify and fix any further build errors that appear in other projects (`Core`, `DataReader`, `Components`, `Tests`, `SampleApp`). These might be cascading errors from the initial issues or new problems related to the refactoring.

8.  **Update Documentation**
    *   **Action:**
        *   Keep `Docs/ProjectStructure.md` accurate as files are moved, added, or removed.
        *   Update `Docs/ImplementationPlan.md` with more detailed steps and decisions as the refactoring progresses.
        *   Create new documentation explaining how to create and use custom `MappingRuleBase` implementations with the refactored library.

## Ongoing Tasks

*   Refer to `DO_NOT_MODIFY_OriginalImplementation/` for understanding the original behavior when needed.
*   Commit changes frequently with clear messages.
*   Write unit tests for new and refactored logic.
