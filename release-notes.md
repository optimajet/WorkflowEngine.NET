<!--Stay on the edge of our innovations and learn about the changes made to Workflow Engine with each of our releases.-->
# Release Notes

## 2.3

- A 'Refresh' button and its functionality have been added to Designer
- A 'Full Screen' button and its functionality have been added to Designer
- Scroll-based scaling has been added to Designer
- `BulkCreateInstance` and `TimerManager` performance has been enhanced

---

## 2.2 

- Now it is possible to create asynchronous Actions and Conditions. You can call asynchronous methods from Actions and Conditions using the `await` keyword. Such methods will be the most effective if you use asynchronous methods of the `WorkflowRuntime` object, for instance, `ExecuteCommandAsync` instead of `ExecuteCommand`, or `SetStateAsync` instead of `SetState`, etc. You can create asynchronous Actions in Designer. To do that you simply need to check the Async checkbox in the Action or Condition where you're going to call asynchronous methods from. If you use `IWorkflowActionProvider`, then you will need to implement 4 additional methods. `bool IsActionAsync(string name)` and `bool IsConditionAsync(string name)` should return true so that the Action or Condition are called asynchronously. The execution of an asynchronous Action or Condition is done in the `Task ExecuteActionAsync(string name, ProcessInstance processInstance, WorkflowRuntime runtime, string actionParameter, CancellationToken token)` and `Task<bool> ExecuteConditionAsync(string name, ProcessInstance processInstance, WorkflowRuntime runtime, string actionParameter, CancellationToken token)` methods. 
- Parameters conveyed to the process with the command no longer need to be described as command parameters. Iа such a parameter is described in the scheme, it will be a Temporary or a Persistence one, depending on which Purpose is specified in the scheme. If the parameter is not described in the scheme, it will be a Temporary one.
- The `ExecuteCommand` and `ExecuteCommandAsync` methods return information on whether the command has been executed (it may not be executed under certain conditions) and the `ProcessInstance` state after the execution of a command.

**The following additional actions must be taken to uprgade to Workflow Engine 2.2:**

- If you use `IWorkflowActionProvider`, you will need to add 4 new methods to it: `IsActionAsync`, `IsConditionAsync`, `ExecuteActionAsync`, `ExecuteConditionAsync`. If you do not yet intend to use asynchronous Actions, then the `IsActionAsync` and `IsConditionAsync` methods should always return false, whereas `ExecuteActionAsync` and `ExecuteConditionAsync` can throw a NotImplementedException. 
 
```csharp
public bool IsActionAsync(string name)
{
	return false;
}

public bool IsConditionAsync(string name)
{
	return false;
}

public async Task ExecuteActionAsync(string name, ProcessInstance processInstance, WorkflowRuntime runtime, string actionParameter, CancellationToken token)
{
	throw new NotImplementedException();
}

 public async Task<bool> ExecuteConditionAsync(string name, ProcessInstance processInstance, WorkflowRuntime runtime, string actionParameter, CancellationToken token)
{
	throw new NotImplementedException();
}
```
---

## 2.1

- Workflow Engine for .NET Core App 1.1 is released. All Workflow Engine features are supported. This version will be updated simultaneously along the .NET Framework version. 2 persistence providers are currently supported: MS SQL Server and PostgreSQL. Links to NuGet packages and samples can be found [here](/downloads).
- Workflow Engine scheme import/export to/from BPMN2 has been added.
- Bulk process creation has been added. Now you can create a large amount of processes (100 - 100,000) in significantly less time than when using the `CreateInstance` method. Use the `_runtime.BulkCreateInstance(..)` method to do that. Currently, the feature is available in the .NET Framework version of Workflow Engine and supports only MS SQL Server. The list of supported databases will be expanded.

---

## 2.0

- Designer UI has been improved, transitions are now linear for better usability
- Scheme rendering library [KineticJS](https://github.com/ericdrowell/KineticJS) has been replaced with [Konva.js](https://konvajs.github.io/)
- `WorkflowDesigner.readonlymode()`, `WorkflowDesigner.printablemode()` and `WorkflowDesigner.ediatblemode()` methods have been added to `WorkflowDesigner` object in the client. `readonlymode` makes Designer uneditable. `printablemode` removes the toolbar, the grid and all controls, zooming designer so that the scheme occupies the entire canvas. This mode can be used to print Designer's viewport to a PDF within a browser. `editablemode` makes Designer editable.
- Designer now allows users to make an autocomplete list for the following fields: `Actor.Value`, `Activity.Implementation.Action parameter`, `Transition.Condition.ActionParameter`. The autocomplete list is being populated server-side via the `IDesignerAutocompleteProvider` interface and by transferring implementation to the `WorkflowRuntime` object via `_runtime.WithDesignerAutocompleteProvider(provider)`.
- The `GetConditions` method has been added to the `IWorkflowActionProvider` interface. Now, the choice of conditions and actions in Designer can be split.
- Parameters of any type not mentioned in the scheme can now be used in Actions' code. The type should be serialized in JSON. Three methods are available: `_processInstance.SetParameter("parameterName",parameterValue,ParameterPurpose.Persistence)`, `_processInstance.GetParameter<ParameterType>("parameterName")` and `_processInstance.RemoveParameter("parameterName")`.
- Ability to create Timers with a value which is undefined at the time of creation of a process has been added. `Timer.Value == -1` means that the current transition will not be executed until the respective timer has a defined value.  `Timer.Value == 0` means that in case the timer connected to a transition does not have a value, this transition shall be processed automatically. Initial value of a timer can be set with the `_runtime.NeedTimerValue` event.
- Ability to modify execution time for timers in running processes has been added. Use methods `_runtime.SetTimerValue(processId,timerName,newValue)` and `_runtime.ResetTimerValue(processId, timerName)` to change and reset timer values outside the process.  Use methods `_runtime.SetTimerValue(processInstance,timerName,newValue)` and `_runtime.ResetTimerValue(processInstance, timerName) ` to change and reset timer values from within your Actions. 
- Newtonsoft.Json version has been changed from 7.0.1 to 9.0.0.

**The following additional actions must be taken to uprgade to Workflow Engine 2.0:**

- Replace the link to KineticJS with the link to konva.min.js. Konva.js library is included in the ZIP archive and in `nuget package WorkflowEngine.NET-Designer`.
- Update Newtonsoft.Json.dll to 9.0.0. The library is included in the ZIP archive and in `nuget package WorkflowEngine.NET-Core`.
- If you use `implementation` of `IWorkflowActionProvider` you should add a `public List<string> GetConditions()` method to it. It is advisable that the `GetActions` returns only the list of available Actions, whereas the `GetConditions` method should return the list of available Conditions. In this case they should be filtered properly in the Designer. However, if you do not want to modify your code, make sure that the `GetConditions` method throws a `NotImplementedException`. In this case, everything should work as it used to.
- IF you use Oracle, you should run an `update_1_5_to_2_0.sql` script from the Oracle provider ZIP archive or a NuGet package.

---
## 1.5.6

- Two new persistence providers were added to WorkflowEngine.NET Redis Provider for [Redis](http://redis.io/) and Ignite Provider for [Apache Ignite](https://ignite.apache.org/)
- notrendertoolbar property was added to the Designer object configuration (client side javascript). You can hide toobar in the workflow designer for end users.
- Second parameter which allow ignore AutoSchemeUpdate sign of Activity was added to the WorkflowRuntime.UpdateSchemeIfObsolete method.
- Several redundat operations related to subprocess features were removed to increase performance.
- Conditions are checked for transitions that creating subprocesses.

---
## 1.5.5

In this release several features have been added to simplify the generation of forms based ot commands. 
- The sign `IsRequired` for command parameter. You can take it into account when generating forms, also it is used in the command valudation before the execution of command.
- The `Default value` for command parameter. You can access it using `CommandParameter.DefaultValue` property, after you have received the list of available commands. You can set all command parameters to default value using `WorkflowCommand.SetAllParametersToDefault` or `WorkflowCommand.SetParameterToDefault` functions. The Default value must be a valid JSON (wich can be deserialized to type which is specified in the bond Parameter) or will be interpreted as a string.
- An `autocompete` was added in the field `Type of Parameter` (editing window Parameters). It makes a suugestions about types which can be used such as primitive types (Int32, String etc) or your custom types. Types from assemblies which was registerd using `_runtime.RegisterAssemblyForCodeActions` function are added in the autocomplete list. To prevent registration or filter the list you can use the last two optional parameters of _runtime.RegisterAssemblyForCodeActions function - ignoreForDesigner and designerTypeFilter.
- The `Initial values` were added for Parameters. You can yse Initial value only for Parameters which have Purpose = Persistence. This values must be a valid JSON (wich can be deserialized to type which is specified in the bond Parameter) or will be interpreted as a string. These values will be set to the process when it is created.
- The JSON editor was added for edit `Parameter.InitialValue` (Parameters window), `Command.InputParameters.DefaultValue` (Commands window), `Actor.Value` (Actors window),
  `Activity.Implementation.ActionParameter` (Activity window), `Transition.Condition.ActionParameter` (Transition window). The JSON editor includes a syntax highlight and Format button. The Format button can be used to format your JSON. Please note that if you use JSON with unquoted property names, you must add reference on `json5.js` library on Designer page. For Parameter.InitialValue (Parameters window) and Command.InputParameters.DefaultValue (Commands window) the JSON editor aso includes Create button. This button can be used to create an empty object based on Parameter.Type. Designer (on server) uses only parameterless constructor to create the empty object.
- There are two new events was added to `WorkflowRuntime`. Both are occured only in normal execution mode (not in pre-execution). `BeforeActivityExecution` - is occured after the Runtime has chosen an appropriate transition for execution but before Actions in an Activity were executed. `ProcessActivityChanged` - is occured after some Activity was executed. You can use `ProcessActivityChanged.TransitionalProcessWasCompleted` property to ensure that the command (or timer) execution was finished. Using of these events could be more convenient than `ProcessStatusChanged` in some cases.
- `GetAllActorsForCommandTransitions(ProcessInstance)` method was added to `WorkflowRuntime`. You can use this method to get next potential approvers for current state.
- You can change the JSON Serializer settings for Parameters derialization using SetParameterSerializerSettings configuration method.

---
## 1.5.4

- Schema was added in constructors of all SQL providers. For MsSQL the default schema is "dbo". For PostgreSql the default schema is "public". For Oracle the default schema is null.
- You are able to specify an interval timer value with milliseconds, seconds, minutes and days. For example "1d 2h 4m 30s" - one day two hours four minutes thirty seconds or "5m 30s", "14d", "12h", "10m", "10s" etc. You are able to use following names. d, day or days to specify interval in days. h, hour or hours to specify interval in hours. m, minute or minutes to specify interval in minutes.  s, second or seconds to specify interval in seconds. ms, millisecond or milliseconds to specify interval in milliseconds. If the interval is specified as just a numeric value, it  is interpreted as the interval in milliseconds.
- You are able to pass initial parameters of a process by using `_runtime.CreateInstance(CreateInstanceParams createInstanceParams)` method. Parameters passed in the createInstanceParams.InitialProcessParameters property will be used as initial for parameters of a process.

---
## 1.5.3

- The JSON serializer used by the engine was changed from `ServiceStack.Text to Newtonsoft.JSON`. 1.5.3 version of WFE was built with the `verion 7.0 of Newtonsoft.JSON`.
- *MSSQL provider* was rewritten from LINQtoSQL to ordinary SQL queries.
- The *Parameter* window in the designer was implroved. Now you can hide System parameters. They are hidden by default.
- Working with types in the *Parameter* window  in the designer was improved. Now you are able to use short type's names such as String, Guid, Int32 etc for primitive types. The type names including namespace for custom types (OptimaJet.Workflow.Core.Model.ActivityDefinition, Business.Approvers etc)- Also you are able to use <> for generic types and [] for arrays.
- Fixed a bug in the timers for their correct restart after restart of the workflow runtime.

---
## 1.5.2

- `ExecutedActivity` and `ExecutedTransition` properties added in `ProcessInstance` class. Via them, you can access executed transition and activity during a transitional process.
- A scheme of a process downloaded from the designer with not ecoded *Code Actions*. You can upload a scheme with encoded or not encoded code actions.
- The algorithm of sub-process scheme separation improved for it to work properly with the different schemes.
- `GetAvailableCommands` method returns distinct commands.
- Fixed parsing of the values of the *Timers* in different cultures
- While merging the processes you can access the parameters of a subprocess using the property `MergedSubprocessParameters` of the `ProcessInstance` class.