# Blazor Component Library Refactoring Plan: Replacing Enum Discriminators

This plan outlines the steps to refactor the `AutomatedRealms.DataImportUtility.Components` library to move away from enum discriminators in favor of a more extensible approach, consistent with the refactoring already completed in the core libraries.

## Analysis Phase

- ✅ Review all existing enum usage in the Components library
- ✅ Map dependencies between Components and core libraries
- ✅ Identify components that need modification based on enum usage
- ✅ Document any JavaScript interop that relies on enum values

### Analysis Phase Results

See the [Analysis Document](AnalysisPhaseResults.md) for a detailed breakdown of enum usage and dependencies.

---

## Design Phase

- ✅ Design component interfaces that align with the new approach in core libraries
  -	Created smart enum patterns (FileType, ImportWorkflow, MappingStrategy)
  -	Designed validation framework with appropriate extensibility
  -	Maintained consistency with the core libraries' TypeId pattern
- ✅ Create class diagrams showing the relationship between components and core classes
  -	Included a comprehensive ASCII diagram showing inheritance and composition relationships
  -	Demonstrated how components interact with the new type system
  -	Showed extension patterns through inheritance
- ✅ Define extension points for consumer customization
  -	Static Register() methods in each smart enum class
  -	DI extension methods for service registration
  -	Component registration interfaces
  -	Runtime component discovery mechanisms
- ✅ Design generic type parameters to replace enum-based type constraints where applicable
  -	Showed transition from enum parameters to type-safe class parameters
  -	Demonstrated how to update component parameter definitions
  -	Provided compatibility layers for transition period

### Design Phase Results

See the [Design Decisions Document](DesignDecisions.md) for detailed designs of the new component interfaces and their relationships.

## Implementation Phase - Core Changes

- ✅ Create base component classes/interfaces that align with core library patterns
  -	Created FileType, ImportWorkflow, MappingStrategy smart enum patterns
  -	Implemented ValidationSeverity, ValidationTrigger, ValidationRule hierarchy
  -	All follow the TypeId pattern established in the core libraries
- ✅ Implement factory classes for component creation where needed
  -	Created IComponentFactory interface and ComponentFactory implementation
  -	Implemented methods for creating components from smart enum patterns
  -	Added support for generic component types
- ✅ Add DI registration extensions for components
  -	Extended ServiceCollectionExtensions with registration methods
  -	Added methods for all smart enum types (AddFileType, AddImportWorkflow, etc.)
  -	Created specialized registration for validation rules
- [ ] Update any existing TypeScript code to accommodate the new type system

## Implementation Phase - Immediate Priorities

To restore functionality to the component library, we need to focus on these specific priorities:

1. **Address Critical Enum Dependencies**
   - [ ] ValueTransformationType - Used extensively in transformation configuration components
   - [ ] ComparisonOperationType - Used in conditional transformations
   - [ ] MappingRuleType - Used across multiple mapping components

2. **Create TypeId-based Extensions**
   - [ ] Create extension methods that bridge between TypeId strings and the former enum values
   - [ ] Implement ValueTransformationTypeExtensions with GetClassType() and CreateNewInstance() methods
   - [ ] Implement GetEnumValue() extension methods for backward compatibility

3. **Update Component Parameter Types**
   - [ ] Update parameter types in FieldTransformationConfiguration
   - [ ] Update parameter types in ValueTransformationConfiguration
   - [ ] Update parameter types in ConditionalValueTransformationInput

This focused approach will ensure we restore functionality first while implementing our extensibility design.

## Implementation Phase - Component Updates

- [ ] Refactor file import components to use the new type system
- [ ] Update data mapping components to work with extensible type definitions
- [ ] Modify validation components to use interface-based validation instead of enum flags
- [ ] Adjust any component parameters that previously used enums

## Testing Phase

- [ ] Create unit tests for new component interfaces
- [ ] Test extensibility by creating sample extensions
- [ ] Verify backward compatibility where possible
- [ ] Test performance impacts of the new architecture

## Documentation and Examples

- [ ] Update component documentation to reflect the new architecture
- [ ] Create example implementations showing how to extend the components
- [ ] Document migration path for existing consumers
- [ ] Update any XML documentation comments in the codebase

## Final Verification

- [ ] Perform compatibility testing against the original implementations
- [ ] Verify that all JavaScript interop features work correctly
- [ ] Ensure all public APIs are appropriately documented
- [ ] Review for any remaining enum usages that should be replaced

This plan will be our guide for refactoring the Components library to better align with the core libraries and provide more extensibility for consumers.
