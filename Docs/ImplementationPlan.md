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

- [ ] Design component interfaces that align with the new approach in core libraries
- [ ] Create class diagrams showing the relationship between components and core classes
- [ ] Define extension points for consumer customization
- [ ] Design generic type parameters to replace enum-based type constraints where applicable

### Design Phase Results

See the [Design Decisions Document](DesignDecisions.md) for detailed designs of the new component interfaces and their relationships.

## Implementation Phase - Core Changes

- [ ] Create base component classes/interfaces that align with core library patterns
- [ ] Implement factory classes for component creation where needed
- [ ] Add DI registration extensions for components
- [ ] Update any existing TypeScript code to accommodate the new type system

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