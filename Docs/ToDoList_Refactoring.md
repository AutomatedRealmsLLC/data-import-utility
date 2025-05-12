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

3.  **Resolve Type Not Found Errors (CS0246) in Abstractions Project Helpers**
    *   **Issue:** Various concrete rule and transformation types (e.g., `CopyRule`, `IgnoreRule`, `CalculateTransformation`) cannot be found in files like `MappingRuleTypeExtensions.cs`, `ValueTransformationTypeExtensions.cs` within the `AutomatedRealms.DataImportUtility.Abstractions\Helpers` folder.
    *   **Cause:** These helper/extension methods in the `Abstractions` project are attempting to directly reference or instantiate concrete types defined in the `AutomatedRealms.DataImportUtility.Core` project. This is problematic because:
        *   It creates a tight coupling where `Abstractions` knows about `Core` implementations.
        *   It can lead to circular dependencies if `Core` already depends on `Abstractions`.
        *   This is directly tied to the enum-based factory logic we want to refactor for better extensibility.
    *   **Action (Part of larger refactoring effort - Phase 2):**
        *   These errors highlight the core issue with the current enum-based instantiation. The long-term fix is to remove these extension methods or change their approach as part of refactoring `MappingRuleBase` (Task #5).
        *   For now, we might need to temporarily comment out the problematic code in these helper files to allow the build to proceed with other projects, or add `using` directives for the `Core` project's namespaces if a temporary direct reference is acceptable to see other errors.

4.  **Resolve `JsonConverter<>` Not Found Errors (CS0246)**
    *   **Issue:** The type or namespace name `JsonConverter<>` cannot be found in:
        *   `AutomatedRealms.DataImportUtility.Abstractions/CustomConverters/MappingRuleBaseConverter.cs`
        *   `AutomatedRealms.DataImportUtility.Abstractions/CustomConverters/ValueTransformationBaseConverter.cs`
    *   **Action:**
        *   Add a package reference to `System.Text.Json` (or `Newtonsoft.Json` if that's the preferred library) in the `AutomatedRealms.DataImportUtility.Abstractions.csproj` file.
        *   Add the necessary `using` directive (e.g., `using System.Text.Json.Serialization;`) in the affected files.

## Phase 2: Core Refactoring for Extensibility

5.  **Refactor `MappingRuleBase` for Extensibility**
    *   **Goal:** Move away from the current enum-based approach (`MappingRuleType`, `MappingRuleTypeExtensions`) to allow consumers of the library to define and use their own custom `MappingRuleBase` implementations easily.
    *   **Tasks:**
        *   Analyze the existing `MappingRuleBase` (in `Abstractions`) and its current implementations (in `Core`, e.g., `CopyRule`, `CombineFieldsRule`).
        *   Design a new discovery and instantiation mechanism for `MappingRuleBase` implementations. Options include:
            *   Reflection-based scanning of assemblies for derived types.
            *   A manual registration pattern.
            *   Integration with a Dependency Injection container if one is planned.
        *   Remove reliance on `Enums/MappingRuleType.cs` and `Helpers/MappingRuleTypeExtensions.cs` for rule identification and creation.
        *   Update `CustomConverters/MappingRuleBaseConverter.cs` to support serialization and deserialization of the new extensible rule types. This will likely require a way to store type information for custom rules in the JSON.

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
