.                                                                  # Workspace root (d:\git\AutomatedRealms\data-import-utility)
├─ build.cake                                                      # Cake build script
├─ LICENSE                                                         # License file for the project
├─ README.md                                                       # Project README file
├─ Docs/                                                           # Documentation files
│  ├─ ImplementationPlan.md                                        # Plan for refactoring and implementation
│  └─ ProjectStructure.md                                          # This file: describes the project structure
├─ src/                                                            # Source code
│  ├─ DataImportUtility/                                           # Main solution folder for the Data Import Utility library
│  │  ├─ DataImportUtility.sln                                     # Visual Studio Solution File
│  │  ├─ Directory.Build.props                                     # Common MSBuild properties for projects in this solution
│  │  ├─ AutomatedRealms.DataImportUtility.Abstractions/           # Project for common abstractions and interfaces
│  │  │  ├─ AutomatedRealms.DataImportUtility.Abstractions.csproj  # C# project file
│  │  │  ├─ ApplicationConstants.cs
│  │  │  ├─ ComparisonOperationBase.cs
│  │  │  ├─ IComparisonOperation.cs
│  │  │  ├─ IDataReaderService.cs
│  │  │  ├─ IImportDataFileRequest.cs
│  │  │  ├─ IMappingRule.cs
│  │  │  ├─ ITransformationContext.cs
│  │  │  ├─ MappingRuleBase.cs
│  │  │  ├─ ValueTransformationBase.cs
│  │  │  ├─ Compatibility/
│  │  │  │  ├─ CompilerServicesPolyfills.cs
│  │  │  │  └─ IsExternalInit.cs
│  │  │  ├─ CustomConverters/
│  │  │  │  ├─ MappingRuleBaseConverter.cs
│  │  │  │  └─ ValueTransformationBaseConverter.cs
│  │  │  ├─ CustomExceptions/
│  │  │  │  └─ MissingFieldMappingException.cs
│  │  │  ├─ Enums/
│  │  │  │  ├─ ComparisonOperationType.cs
│  │  │  │  ├─ MappingRuleType.cs
│  │  │  │  ├─ RuleType.cs
│  │  │  │  └─ ValueTransformationType.cs
│  │  │  ├─ Helpers/
│  │  │  │  ├─ DataTableExtensions.cs
│  │  │  │  ├─ FileReaderHelpers.cs
│  │  │  │  ├─ MappingRulesHelpers.cs
│  │  │  │  ├─ MappingRuleTypeExtensions.cs
│  │  │  │  ├─ ModelValidatorHelpers.cs
│  │  │  │  ├─ ReflectionHelpers.cs
│  │  │  │  ├─ RuleTypeExtensions.cs
│  │  │  │  ├─ StringExtensions.cs
│  │  │  │  ├─ StringHelpers.cs
│  │  │  │  ├─ TransformationResultHelpers.cs
│  │  │  │  ├─ ValueTransformationHelper.cs
│  │  │  │  └─ ValueTransformationTypeExtensions.cs
│  │  │  ├─ Interfaces/
│  │  │  │  ├─ IComparisonOperation.cs
│  │  │  │  └─ ITransformationContext.cs
│  │  │  └─ Models/
│  │  │     ├─ ConditionalRule.cs
│  │  │     ├─ FieldMapping.cs
│  │  │     ├─ FieldTransformation.cs
│  │  │     ├─ ImportedDataFile.cs
│  │  │     ├─ ImportedRecordFieldDescriptor.cs
│  │  │     ├─ ImportTableDefinition.cs
│  │  │     ├─ MappingRuleBase.cs
│  │  │     ├─ TransformationResult.cs
│  │  │     └─ ValueMap.cs
│  │  ├─ AutomatedRealms.DataImportUtility.Core/                   # Project for core data processing logic
│  │  │  ├─ AutomatedRealms.DataImportUtility.Core.csproj          # C# project file
│  │  │  ├─ ApplicationConstants.cs
│  │  │  ├─ ComparisonOperations/
│  │  │  │  ├─ BetweenOperation.cs
│  │  │  │  ├─ ComparisonOperationFactory.cs
│  │  │  │  ├─ ContainsOperation.cs
│  │  │  │  ├─ EndsWithOperation.cs
│  │  │  │  ├─ EqualsOperation.cs
│  │  │  │  ├─ GreaterThanOperation.cs
│  │  │  │  ├─ GreaterThanOrEqualOperation.cs
│  │  │  │  ├─ InOperation.cs
│  │  │  │  ├─ IsFalseOperation.cs
│  │  │  │  ├─ IsNotNullOperation.cs
│  │  │  │  ├─ IsNotNullOrEmptyOperation.cs
│  │  │  │  ├─ IsNotNullOrWhiteSpaceOperation.cs
│  │  │  │  ├─ IsNullOperation.cs
│  │  │  │  ├─ IsNullOrEmptyOperation.cs
│  │  │  │  ├─ IsNullOrWhiteSpaceOperation.cs
│  │  │  │  ├─ IsTrueOperation.cs
│  │  │  │  ├─ LessThanOperation.cs
│  │  │  │  ├─ LessThanOrEqualOperation.cs
│  │  │  │  ├─ NotBetweenOperation.cs
│  │  │  │  ├─ NotContainsOperation.cs
│  │  │  │  ├─ NotEqualOperation.cs
│  │  │  │  ├─ NotInOperation.cs
│  │  │  │  ├─ RegexMatchOperation.cs
│  │  │  │  └─ StartsWithOperation.cs
│  │  │  ├─ Compatibility/
│  │  │  │  └─ MathCompatibility.cs
│  │  │  ├─ CustomConverters/
│  │  │  │  ├─ MappingRuleBaseConverter.cs
│  │  │  │  └─ ValueTransformationBaseConverter.cs
│  │  │  ├─ CustomExceptions/
│  │  │  │  └─ MissingFieldMappingException.cs
│  │  │  ├─ Helpers/
│  │  │  │  ├─ CompilerServicesPolyfills.cs
│  │  │  │  ├─ DataTableExtensions.cs
│  │  │  │  ├─ FileReaderHelpers.cs
│  │  │  │  ├─ MappingRulesHelpers.cs
│  │  │  │  ├─ MappingRuleTypeExtensions.cs
│  │  │  │  ├─ ModelValidation/
│  │  │  │  │  └─ ModelValidatorHelpers.cs
│  │  │  │  ├─ ReflectionHelpers.cs
│  │  │  │  ├─ RuleTypeExtensions.cs
│  │  │  │  ├─ StringExtensions.cs
│  │  │  │  ├─ StringHelpers.cs
│  │  │  │  ├─ TransformationResultHelpers.cs
│  │  │  │  ├─ ValueTransformationHelper.cs
│  │  │  │  └─ ValueTransformationTypeExtensions.cs
│  │  │  ├─ Models/
│  │  │  │  ├─ FieldMapping.cs
│  │  │  │  ├─ FieldTransformation.cs
│  │  │  │  ├─ ImportedDataFile.cs
│  │  │  │  ├─ ImportedRecordFieldDescriptor.cs
│  │  │  │  ├─ ImportTableDefinition.cs
│  │  │  │  ├─ TransformationResult.cs
│  │  │  │  └─ ValueMap.cs
│  │  │  ├─ Rules/
│  │  │  │  ├─ CombineFieldsRule.cs
│  │  │  │  ├─ ConstantValueRule.cs
│  │  │  │  ├─ CopyRule.cs
│  │  │  │  ├─ CopyRuleCore.cs
│  │  │  │  ├─ CustomFieldlessRule.cs
│  │  │  │  ├─ FieldAccessRule.cs
│  │  │  │  ├─ IgnoreRule.cs
│  │  │  │  └─ StaticValueRule.cs
│  │  │  └─ ValueTransformations/
│  │  │     ├─ CalculateTransformation.cs
│  │  │     ├─ CombineFieldsTransformation.cs
│  │  │     ├─ ConditionalTransformation.cs
│  │  │     ├─ InterpolateTransformation.cs
│  │  │     ├─ MapTransformation.cs
│  │  │     ├─ RegexMatchTransformation.cs
│  │  │     └─ SubstringTransformation.cs
│  │  ├─ AutomatedRealms.DataImportUtility.DataReader/             # Project for data reading implementations
│  │  │  ├─ AutomatedRealms.DataImportUtility.DataReader.csproj    # C# project file
│  │  │  ├─ Abstractions/
│  │  │  │  └─ IDataReaderService.cs
│  │  │  ├─ Helpers/
│  │  │  │  └─ FileReaderHelpers.cs
│  │  │  └─ Services/
│  │  │     └─ DataReaderService.cs
│  │  ├─ AutomatedRealms.DataImportUtility.Components/             # New Blazor component library for UI elements
│  │  │  ├─ AutomatedRealms.DataImportUtility.Components.csproj    # C# project file
│  │  │  ├─ _Imports.razor
│  │  │  ├─ compilerconfig.json
│  │  │  ├─ compilerconfig.json.defaults
│  │  │  ├─ DataFileMapper.razor
│  │  │  ├─ DataFileMapper.razor.cs
│  │  │  ├─ DataFileMapper.razor.css
│  │  │  ├─ DataFileMapper.razor.min.css
│  │  │  ├─ DataFileMapper.razor.scss
│  │  │  ├─ GlobalUsings.cs
│  │  │  ├─ Abstractions/
│  │  │  │  ├─ BaseStateEventHandler.cs
│  │  │  │  ├─ FileImportUtilityComponentBase.cs
│  │  │  │  └─ IDataFileMapperState.cs
│  │  │  ├─ ButtonComponents/
│  │  │  │  ├─ CheckButton.razor
│  │  │  │  ├─ CheckButton.razor.cs
│  │  │  │  ├─ CheckButton.razor.css
│  │  │  │  ├─ CheckButton.razor.min.css
│  │  │  │  └─ CheckButton.razor.scss
│  │  │  ├─ CustomErrorBoundaryComponent/
│  │  │  │  ├─ DefaultErrorContextContent.razor
│  │  │  │  └─ DefaultErrorContextContent.razor.cs
│  │  │  ├─ DataSetComponents/
│  │  │  │  ├─ DataSetDisplay.razor
│  │  │  │  ├─ DataSetDisplay.razor.cs
│  │  │  │  ├─ DataSetDisplay.razor.css
│  │  │  │  ├─ DataSetDisplay.razor.min.css
│  │  │  │  ├─ DataSetDisplay.razor.scss
│  │  │  │  ├─ DataTableDisplay.razor
│  │  │  │  ├─ DataTableDisplay.razor.cs
│  │  │  │  ├─ DataTableDisplay.razor.css
│  │  │  │  ├─ DataTableDisplay.razor.min.css
│  │  │  │  ├─ DataTableDisplay.razor.scss
│  │  │  │  └─ _Imports.razor
│  │  │  ├─ Extensions/
│  │  │  │  ├─ DataFileMapperUiOptions.cs
│  │  │  │  └─ ServiceExtensions.cs
│  │  │  ├─ FieldMappingComponents/
│  │  │  │  ├─ FieldMapperEditor.razor
│  │  │  │  ├─ FieldMapperEditor.razor.css
│  │  │  │  ├─ FieldMapperEditor.razor.min.css
│  │  │  │  ├─ FieldMapperEditor.razor.scss
│  │  │  │  ├─ FieldMapperEditorBase.razor
│  │  │  │  ├─ FieldMapperEditorBase.razor.cs
│  │  │  │  ├─ FieldTransformationComponents/
│  │  │  │  │  ├─ FieldTransformationConfiguration.razor
│  │  │  │  │  ├─ FieldTransformationConfiguration.razor.cs
│  │  │  │  │  ├─ FieldTransformationConfiguration.razor.css
│  │  │  │  │  ├─ FieldTransformationConfiguration.razor.min.css
│  │  │  │  │  └─ FieldTransformationConfiguration.razor.scss
│  │  │  │  ├─ OperandComponents/
│  │  │  │  │  ├─ MappingRuleConfigPanel.razor
│  │  │  │  │  ├─ MappingRuleConfigPanel.razor.cs
│  │  │  │  │  ├─ MappingRuleConfigPanel.razor.css
│  │  │  │  │  ├─ MappingRuleConfigPanel.razor.min.css
│  │  │  │  │  └─ MappingRuleConfigPanel.razor.scss
│  │  │  │  ├─ ValueTransformationComponents/
│  │  │  │  │  ├─ BasicValueTransformationInput.razor
│  │  │  │  │  ├─ BasicValueTransformationInput.razor.css
│  │  │  │  │  ├─ BasicValueTransformationInput.razor.min.css
│  │  │  │  │  ├─ BasicValueTransformationInput.razor.scss
│  │  │  │  │  ├─ CalculateValueTransformationInput.razor
│  │  │  │  │  ├─ CombineFieldsTransformationInput.razor
│  │  │  │  │  ├─ ConditionalValueTransformationInput.razor
│  │  │  │  │  ├─ ConditionalValueTransformationInput.razor.cs
│  │  │  │  │  ├─ ConditionalValueTransformationInput.razor.css
│  │  │  │  │  ├─ ConditionalValueTransformationInput.razor.min.css
│  │  │  │  │  ├─ ConditionalValueTransformationInput.razor.scss
│  │  │  │  │  ├─ MapTransformationInput.razor
│  │  │  │  │  ├─ MapTransformationInput.razor.cs
│  │  │  │  │  ├─ SubstringValueTransformationInput.razor
│  │  │  │  │  ├─ ValueTransformationConfiguration.razor
│  │  │  │  │  ├─ ValueTransformationConfiguration.razor.cs
│  │  │  │  │  ├─ ValueTransformationConfiguration.razor.css
│  │  │  │  │  ├─ ValueTransformationConfiguration.razor.min.css
│  │  │  │  │  ├─ ValueTransformationConfiguration.razor.scss
│  │  │  │  │  ├─ ValueTransformationInputBase.cs
│  │  │  │  │  ├─ ValueTransformationRuleDetails.razor
│  │  │  │  │  ├─ ValueTransformationRuleDetails.razor.cs
│  │  │  │  │  ├─ ValueTransformationRuleDetails.razor.css
│  │  │  │  │  ├─ ValueTransformationRuleDetails.razor.min.css
│  │  │  │  │  └─ ValueTransformationRuleDetails.razor.scss
│  │  │  │  └─ Wrappers/
│  │  │  │     ├─ FieldMapperDialog.razor
│  │  │  │     ├─ FieldMapperDialog.razor.css
│  │  │  │     ├─ FieldMapperDialog.razor.min.css
│  │  │  │     ├─ FieldMapperDialog.razor.scss
│  │  │  │     ├─ FieldMapperDisplayMode.cs
│  │  │  │     ├─ FieldMapperFlyout.razor
│  │  │  │     ├─ FieldMapperFlyout.razor.css
│  │  │  │     ├─ FieldMapperFlyout.razor.min.css
│  │  │  │     ├─ FieldMapperFlyout.razor.scss
│  │  │  │     ├─ FieldMapperWrapperBase.cs
│  │  │  │     ├─ _FieldMapperEditorMixins.scss
│  │  │  │     └─ _Imports.razor
│  │  │  ├─ FilePickerComponent/
│  │  │  │  ├─ DataFilePicker.razor
│  │  │  │  ├─ DataFilePicker.razor.cs
│  │  │  │  ├─ DataFilePicker.razor.css
│  │  │  │  ├─ DataFilePicker.razor.min.css
│  │  │  │  └─ DataFilePicker.razor.scss
│  │  │  ├─ IconComponent/
│  │  │  │  └─ Icons.razor
│  │  │  ├─ JsInterop/
│  │  │  │  ├─ FileMapperJsModule.cs
│  │  │  │  └─ JsModuleBase.cs
│  │  │  ├─ Models/
│  │  │  │  ├─ FileReadState.cs
│  │  │  │  └─ ImportDataFileRequest.cs
│  │  │  ├─ Sass/
│  │  │  │  ├─ _AccessibilityStyles.scss
│  │  │  │  ├─ _AnimationMixins.scss
│  │  │  │  ├─ _ButtonMixins.scss
│  │  │  │  ├─ _ColorVariables.scss
│  │  │  │  ├─ _ControlStyles.scss
│  │  │  │  ├─ _FlyoutMixins.scss
│  │  │  │  ├─ _ModalMixins.scss
│  │  │  │  ├─ _ScrollbarMixins.scss
│  │  │  │  └─ _TableMixins.scss
│  │  │  ├─ Services/
│  │  │  ├─ State/
│  │  │  │  └─ DataFileMapperState.cs
│  │  │  └─ wwwroot/
│  │  │     ├─ js/
│  │  │     │  ├─ file-mapper-funcs.js
│  │  │     │  └─ file-mapper-funcs.js.map
│  │  │     └─ scripts/
│  │  │        └─ file-mapper-funcs.ts
│  │  └─ AutomatedRealms.DataImportUtility.Tests/                  # Project for unit tests
│  │     ├─ AutomatedRealms.DataImportUtility.Tests.csproj         # C# project file
│  │     ├─ ExtensionTests/
│  │     ├─ MappingRuleTests/
│  │     ├─ ModelTests/
│  │     ├─ SampleData/
│  │     ├─ TestHelpers/
│  │     └─ ValueTransformTests/
│  └─ SampleApp/                                                   # Sample Blazor application demonstrating the library's usage
│     ├─ DataImportUtility.SampleApp.sln                           # Separate solution file for the sample app
│     └─ DataImportUtility.SampleApp/                              # Sample Blazor WebAssembly application project
│        ├─ DataImportUtility.SampleApp.csproj                     # C# project file
│        ├─ DataImportUtility.SampleApp.csproj.user
│        ├─ _Imports.razor
│        ├─ App.razor
│        ├─ compilerconfig.json
│        ├─ compilerconfig.json.defaults
│        ├─ Program.cs
│        ├─ Layout/
│        ├─ Models/
│        ├─ OverriddenComponents/
│        ├─ Pages/
│        ├─ Properties/
│        ├─ Sass/
│        └─ wwwroot/
└─ tools/                                                          # Tools used in the build process
   └─ Addins/                                                      # Cake Addins

**Note:** Standard .NET build output folders (e.g., `bin/`, `obj/`), IDE-specific folders (e.g., `.vs/`), and some project subfolders (like `Properties/` within projects unless they contain unique configuration) have been omitted for brevity.