# Data Import Utility Refactoring: Extensibility Implementation Plan

**Overarching Goal:** Refactor the data import utility to allow consumers to easily define and use their own custom implementations of core extensible types (e.g., `MappingRuleBase`, `ValueTransformationBase`, `ComparisonOperationBase`) without relying on enums, ensuring good performance and a clean developer experience.

## Phase 1: Core Abstraction Changes & Registration Foundation

This phase focuses on modifying the base abstractions and setting up the core infrastructure for the new `TypeId`-driven registration and serialization model.

- [X] **1. Modify Base Abstraction Classes (`MappingRuleBase`, `ValueTransformationBase`, etc.):**
    - [X] **Task:** Add `TypeId` Property.
        - **Details:** Add a `public string TypeId { get; protected set; }` (or `init;`) property to each base class.
        - The `TypeId` should be a unique string identifier (e.g., "Core.CopyRule", "MyCompany.CustomRule").
        - Concrete implementations will be responsible for setting this `TypeId` in their constructors (e.g., `public CopyRule() : base("Core.CopyRule") { ... }`) or by overriding an abstract property.
    - [X] **Task:** Remove Obsolete Enum Properties.
        - **Details:** Remove properties like `public MappingRuleType RuleType { get; }` if their sole purpose was for the old enum-based discrimination.

- [ ] **2. Develop Central Type Registry & DI Integration:**
    - [ ] **Task:** Create `TypeRegistryService` (or similar).
        - **Details:** This service will internally hold a mapping (e.g., `Dictionary<string, Type>`) from `TypeId` strings to `System.Type`.
        - It will expose methods like `RegisterType(string typeId, Type type)` and `ResolveType(string typeId)`. It should also provide a way to get all registered types (or their `TypeId`s and display names) for UI population. Make thread-safe if necessary.
    - [ ] **Task:** Implement DI Extension for Core Types.
        - **Details:** In `AutomatedRealms.DataImportUtility.Core`, create a public static class (e.g., `DataImportCoreServiceCollectionExtensions`) with an extension method like `public static IServiceCollection AddDataImportUtilityCore(this IServiceCollection services)`.
        - This method will explicitly register all default implementations from the `.Core` project with the `TypeRegistryService` (e.g., `typeRegistry.RegisterType("Core.CopyRule", typeof(CopyRule));`).
        - It will also register the `TypeRegistryService` itself as a singleton.
    - [ ] **Task:** Implement DI Extensions for Consumer Registration (Explicit Method).
        - **Details:** In `AutomatedRealms.DataImportUtility.Abstractions` or a new dedicated DI helper library, provide generic extension methods for consumers, e.g., `services.AddMappingRule<TImplementation>(string typeId)` where `TImplementation : MappingRuleBase`.
        - This allows consumers to explicitly register their types: `services.AddMappingRule<MyCustomRule>("MyCompany.MyRule");`.

- [ ] **3. Update Custom JSON Converters (`MappingRuleBaseConverter`, etc.):**
    - [ ] **Task:** Modify Converters to Use `TypeId` and `TypeRegistryService`.
        - **Details (Serialization):**
            - Ensure the `TypeId` property of the object being serialized is written to the JSON (e.g., as a `"$typeId"` property, or a clearly named one like `"RuleTypeId"`).
        - **Details (Deserialization):**
            - Read the `TypeId` property from the JSON.
            - Use the injected `TypeRegistryService` to resolve this `typeId` to a `System.Type`.
            - If resolved, deserialize the JSON object into an instance of this specific `System.Type`.
            - Handle cases where `typeId` is missing or cannot be resolved (throw informative exception).

## Phase 2: Removing Old Enum-Based System & UI Updates

- [X] **4. Decouple and Remove Obsolete Enum-Based Logic:**
    - [X] **Task:** Remove Enum Definitions.
        - **Details:** Delete enum files like `MappingRuleType.cs`, `ValueTransformationType.cs` from `Abstractions/Enums/`.
    - [X] **Task:** Remove Enum-Based Helper Extensions.
        - **Details:** Delete files like `MappingRuleTypeExtensions.cs` from `Abstractions/Helpers/` that contained the old `CreateNewInstance` logic.
    - [ ] **Task:** Clean up any remaining direct usages of these enums for type discrimination logic if not already covered.

- [ ] **5. Update UI Components (e.g., Blazor Components):**
    - [ ] **Task:** Modify UI for Dynamic Type Selection.
        - **Details:** Components that allowed users to select a rule/transformation type via an enum-populated dropdown will need to be changed.
        - They should now fetch the list of available types from the `TypeRegistryService` (e.g., get all registered `TypeId`s, potentially with associated display names if we add that feature - perhaps a `DisplayName` property on the base class or an attribute).
        - The UI will then bind to the `TypeId` string.

## Phase 3: Consumer Experience & Advanced Features (Future Considerations)

- [ ] **6. Develop Source Generator for Consumer Type Registration (Performance & DX Enhancement):**
    - [ ] **Task:** Design and Implement Source Generator.
        - **Details:** Create/Update the `AutomatedRealms.DataImportUtility.SourceGenerator` project.
        - This generator would scan the consumer's project for classes inheriting `MappingRuleBase`, etc. (potentially marked with a specific attribute if needed to narrow scope or provide metadata like a display name).
        - It would then auto-generate a partial class with an extension method (e.g., `services.AddMyProjectDataImportExtensions()`) that contains explicit registration calls for all discovered custom types, using their self-defined `TypeId`.
        - This combines ease of use (developer just creates the class) with the performance of explicit registration.
    - [ ] **Task:** Provide clear documentation on how to use the source generator.

- [ ] **7. Documentation & Examples:**
    - [ ] **Task:** Update all relevant documentation.
        - **Details:** Explain the new extensibility model, how to create custom types (including setting their `TypeId`), how to register them (explicitly and via source generator if implemented), and how `TypeId`s work in serialization.
    - [ ] **Task:** Create comprehensive examples in `SampleApp` demonstrating custom type creation and registration.

## Ongoing Tasks (Throughout all Phases):

- [ ] **Testing:** Write unit and integration tests for the new registration system, JSON converters, and any modified components.
- [ ] **Error Handling:** Ensure robust error handling (e.g., for missing `TypeId`s during deserialization, registration conflicts, `TypeId` uniqueness if not enforced by convention).
- [ ] **Project References:** Adjust project references as needed (e.g., `Core` will reference `Abstractions`, `Components` will reference `Abstractions` and potentially `Core` for DI setup).
- [ ] **Address Build Errors:** Systematically resolve any build errors that arise from these changes.

This plan provides a structured approach. We will iterate on these tasks and add more detail as we progress.
