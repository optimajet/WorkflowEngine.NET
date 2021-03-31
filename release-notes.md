# Workflow Engine: Release Notes

## 5.1 {#5.1}

### Designer

- Improved designer’s usability.
- Added creation of commands from the transition’s edit form.
- It is possible to customize displayed activities and transitions, their templates are located in the *templates/elements* folder and are .svg files that can be changed.
- It became possible to choose a color for displaying activity and transition.
- For custom activities added on the server, it is now possible to specify the template for the edit form and .svg to be displayed on graph.
- A new type of activity *Decision* has been added to the elements panel – it is used for convenience and to improve the visual perception of processes with conditional branches. It allows you to make a branch with one condition.
- A new type of activity *Decision Table* has been added to the elements panel – it is used for convenience and to improve the visual perception of processes with conditional branches. It allows you to make a branch with any number of conditions.
- The design of editing usings in Code Actions has been changed.

### Plugins

- Extended the functionality of the *Approval Plugin*, now it fully allows you to implement the functionality of Inbox, Outbox, and the history of document changes. All [examples](/downloads/net-core/) implement this functionality using the *Approval Plugin*.
- Methods for manipulating Inbox, Outbox and the history of document changes are added to *Persistence Providers*. The history of document changes, Inbox and Outbox are only filled when *Approved Plugin* is connected.
- The ability to register Actors predefined on the server is added in the Basic Plugin. The `basicPlugin.WithActors` method and two delegates: `basicPlugin.CheckPredefinedActorAsync` and `basicPlugin.GetPredefinedIdentitiesAsync` are used for this.
- You can now specify a delegate to update the status(state) of the document - `basicPlugin.UpdateDocumentStateAsync` in the *Basic Plugin*.
- A *DeleteSubprocesess* action is added to the *Basic Plugin*, this action deletes all subprocesses.
- Added the ability to customize HTTP requests headers in the *Basic Plugin’s* *CheckHTTPRequest and HTTPRequest* methods.
- The ability to specify a username and password for all the methods that make HTTP requests has been added to the *Basic Plugin*.  
- The ability to download files by HTTP has been added to the *File Plugin*.
- It is now possible to specify the ID of the created process in the *CreateProcess* method in the *Basic Plugin*.

### Core

- You can now set common usings for all *Code Actions* of the scheme, at the same time you can configure usings individually for each *Code Action*. This makes usings managing easier.
- Added process log. The sequence of actions that occurs during the execution of the process is now added to a process log. The log can be enabled for all processes created for a specific scheme or for a specific process. **Attention: The Workflow Engine package includes a logger that stores the log in the memory. This is enough for debugging processes but if you want to make the log persistent, you have to implement the `IProcessLogProvider` and connect it to the Workflow Runtime by calling the `runtime.WithProcessLogger(…)` method**.
- `GetProcessInstancesAsync(...)` and `GetSchemesAsync(...)` methods that accept sorting and paging are added  to the Persistence Provider. With their help you can access the list of schemes and processes.
- The *Activity* in *Expert mode* in the *Designer* now has the ability to set the *Execution Timeout*, meaning it limits the execution time of all *Actions of this Activity*. **Attention: This timeout with only work for asynchronous Actions that process the Cancellation Token passed to them.** The timeout value is the same as the [interval timer value](/documentation/scheme/timers/#general). Possible reactions to timeout: *Set Activity*, *Set State*, *Retry*.
- The *Activity* in *Expert mode* in the *Designer* now has the ability to set *Idle Timeout*, meaning it limits the time a process can be in this *Activity* without doing anything (i.e Idled or Finalized status of the process). The timeout value is the same as the [interval timer value](/documentation/scheme/timers/#general). Possible reactions to timeout: *Set Activity*, *Set State*.
- The *Activity* in *Expert mode* in the *Designer* now has the ability to set *Error handling* by listing the names of the exceptions that need to be handled. Possible reactions to the exception: *Set Activity*, *Set State*, *Retry*, *Ignore*.
- The *Activity* in *Expert mode* in the *Designer* now has the ability to disable saving process state. This setting is called *Disable persist*.
- Process Instance is passed to the `HasExternalParameter`,` IsGetExternalParameterAsync`, `IsSetExternalParameterAsync` of `IWorkflowExternalParametersProvider` methods.
- Two time-stamps were added to Process Instance: *CreationDate* – date and time of process creation and *LastTransitionDate* – date and time of the last change of the process state.
- *StartTransitionTime* – date and time of the beginning of the transition and *TransitionDuration* – the duration of the transition in milliseconds - were added to the Process Transition History.
- An additional parameter `NamesSearchType.All` or `NamesSearchType.NotExcluded` is passed in the `IWorkflowActionProvider.GetActions`,` IWorkflowActionProvider.GetConditions` and `IWorkflowRuleProvider.GetRules` methods, this allows adding *Actions, Conditions or Rules* that are not seen the Designer but that are executed in the process.

### Update instruction {#5.1_update}

**The following additional actions must be taken to upgrade to Workflow Engine 5.1:**

- **Warning. If using Redis, please, contact our support for update instructions.**
- Run the SQL script update_5_0_to_5_1 for all relative databases and MongoDB.
  - [MSSQL](https://github.com/optimajet/WorkflowEngine.NET/blob/master/Providers/OptimaJet.Workflow.MSSQL/Scripts/update_5_0_to_5_1.sql)
  - [PostgreSQL](https://github.com/optimajet/WorkflowEngine.NET/blob/master/Providers/OptimaJet.Workflow.PostgreSQL/Scripts/update_5_0_to_5_1.sql)
  - [Oracle](https://github.com/optimajet/WorkflowEngine.NET/blob/master/Providers/OptimaJet.Workflow.Oracle/Scripts/update_5_0_to_5_1.sql)
  - [MySQL](https://github.com/optimajet/WorkflowEngine.NET/blob/master/Providers/OptimaJet.Workflow.MySQL/Scripts/update_5_0_to_5_1.sql)
  - [MongoDB](https://github.com/optimajet/WorkflowEngine.NET/blob/master/Providers/OptimaJet.Workflow.MongoDB/Scripts/update_5_0_to_5_1.js)
- Update all files related to the Designer. They are available [here](https://github.com/optimajet/WorkflowEngine.NET/tree/master/Designer). Be mindful, that thу *elements* subfolder must be added to the *templates* folder.
- If you are using *Action or Rule Providers* in the `IWorkflowActionProvider.GetActions`,` IWorkflowActionProvider.GetConditions` and `IWorkflowRuleProvider.GetRules` methods add the last `NamesSearchType namesSearchType` parameter. You don't need to change the code of these methods.
- If you are using *External Parameters Provider* in the `HasExternalParameter`, `IsGetExternalParameterAsync` and `IsSetExternalParameterAsync` methods add the last `ProcessInstance processInstance` parameter. You don't need to change the code of these methods.

## 5.0 {#5.0}

### Designer

- Designer windows have been reworked to behave as non-modal which allows:
  - to keep them open while working with a scheme.
  - to open multiple windows, change their size and behavior.
- Customization of Designer windows is now simplified; each window is represented by a Vue.js template which can be now altered independently from the rest of the Designer.
- Customization of Designer toolbars simplified.
- A Designer library of typical schemes added which represent most frequently used blocks which can be dragged onto your schemas.
- A library of custom activities added; these are single action activities whose settings are adjustable within the form.
- Forms to edit properties of activities and transitions now have two viewing modes - Default and Expert. The Default mode represents only the essential and most used settings, while the Expert mode combines all the settings.

### Sub-processes

- Sub-processes can now be launched in a separate thread to allow for physical parallelism at the scheme level.
- Methods to copy parameters into a sub-process can now be carried out using the following options:
  - copy all parameters (default option, previously available)
  - not to copy parameters
  - only copy specified parameters
  - ignore specified parameters.
- New feature to specify a method to transfer sub-process parameters on to its parent process when merging. The following options are available:
  - transfer only the parameters absent in the parent process (default option, previously available)
  - transfer all parameters
  - only copy indicated parameters
  - ignore indicated parameters.
- New feature to cleary indicate whether a particular transition starts a sub-process or finalizes it; previously identified automatically, the transition can be now obviously set.
- New feature to specify a sub-process name which can be represented as a simple string or a calculated expression; expressions can consume parameters (this is syntactically similar to conditional expressions). In this way, new sub-processes can be created simply by changing paramater values used when a sub-process name is generated.  As an example, consider creating an invoice approval scheme where the invoice will contain several product items. It is now possible to design a scheme in which approval of the items will be represented by a sub-process. Furthermore, by combining loops (in the plugin) and sub-process naming calculations a sub-process for each product item can be created.
- New feature to specify a sub-process id, or calculate it based on the parameters; herein, substitutions are used - not expressions.

### Persistence Providers

- All persistence providers are implemented as completely asynchronous. As a result, the WFE core operates in a completely asynchronous fashion suitable for scalability.
- .NET Core provider for MsSql utilizes Microsoft.Data.SqlClient instead of System.Data.SqlClient.

### Workflow Runtime

- Method *Resume* is added to API Workflow Runtime. Contrary to the *SetSatate* method, it does not execute a current activity, or a set one, but rather attempts to continue execution of the process.
This method can be used to go on with a process execution after a failure, or in the case of changes in external conditions.
- Restorer Restore Decision *Resume* added.
- New in the ProcessInstance class:
  - Indexer for getting and setting paramater values
  - Methods to manipulate root process parameters from within a sub-process.
  - For all standard events, now exist their asynchronous counterparts.

### Plugins

- FTP и SFTP support added to the File Plugin. **Attention. File Plugin is supplied as a seperate Nuget package/dll**
- New plugin, *Loops Plugin*, added for simple implementation of the *for* and *foreach*.
- Within the action *BasicPlugin.HttpRequest* you can specify the name of the paramater which is to hold the result.
- Within the action *BasicPlugin.SetParameter*, and the *BasicPlugin.CheckParameter* condition, you can set and verify the root process' paramaters.
- Many new conditions added to *BasicPlugin.CheckParameter* и *BasicPlugin.CheckHTTPRequest*.
- New plugins added for interaction with Slack, Telegram, Twilio, Nexmo are provided as Nuget packages (dlls).

### Update instruction {#5.0_update}

**The following additional actions must be taken to upgrade to Workflow Engine 5.0:**

- **Warning. If using Redis, please, contact our support for update instructions.**
- Run the SQL script update_4_2_to_5_0 for all relative databases and MongoDB.
  - [MSSQL](https://github.com/optimajet/WorkflowEngine.NET/blob/master/Providers/OptimaJet.Workflow.MSSQL/Scripts/update_4_2_to_5_0.sql)
  - [PostgreSQL](https://github.com/optimajet/WorkflowEngine.NET/blob/master/Providers/OptimaJet.Workflow.PostgreSQL/Scripts/update_4_2_to_5_0.sql)
  - [Oracle](https://github.com/optimajet/WorkflowEngine.NET/blob/master/Providers/OptimaJet.Workflow.Oracle/Scripts/update_4_2_to_5_0.sql)
  - [MySQL](https://github.com/optimajet/WorkflowEngine.NET/blob/master/Providers/OptimaJet.Workflow.MySQL/Scripts/update_4_2_to_5_0.sql)
  - [MongoDB](https://github.com/optimajet/WorkflowEngine.NET/blob/master/Providers/OptimaJet.Workflow.MongoDB/Scripts/update_4_2_to_5_0.js)
- Update all files related to the Designer. They are available [here](https://github.com/optimajet/WorkflowEngine.NET/tree/master/Designer).
- Be mindful, that these files must be linked to the designer page:
  - workflowdesigner.min.css
  - workflowdesigner.min.js
  - jquery

  All the other javascript libraries previously required, do not need a separate attachement for they are now compiled inside workflowdesigner.min.js.
- Pay attention to the new way of initializating a WorkflowDesigner object instance.

  ```javascript
  wfdesigner = new WorkflowDesigner({
    name: 'simpledesigner',
    apiurl: '/Designer/API',
    renderTo: 'wfdesigner',
    templatefolder: '/templates/',
    graphwidth: graphwidth,
    graphheight: graphheight
  });
  ```

  - Indicating the path to the images folder is no longer required and the folder can be deleted.
  - Yet, the path to the templates folder must be specified as it contains templates for all the forms and the library of schemes.
  - It is advised to reimplement WorkflowDesigner initializations, as is shown [here]().
- If you are using .NET Framework you may need to use `Request.Unvalidated[key]` instead of `context.Request.Form[key]` in the DesignerConroller.cs.

- **Attention. Event handlers OnSchemaWasChanged and OnSchemaWasChangedAsync are now initialized inside a locked process. That is, prior to releasing the *Running* status. The code, [as described in the documentation](https://workflowengine.io/documentation/execution/scheme-update/#manual) will continue to work as expected without need to change it. Yet, if SetActivityWithoutExecution[Async] or SetActivityWithExecution[Async] are called in these handlers, it should be done using this flag *doNotSetRunningStatus = true*. In turn, if you utilized methods ExecuteCommand, SetSatate and similar, be aware that there methods which lock the process. Therefore, is might be best to use methods: SetActivityWithoutExecution[Async] or SetActivityWithExecution[Async]. If you have implemented a complicated logic for updating schemes which leads to a failed attempt at updating you WFE version because of this change, reach us at support@optimajet.com and we'll help you.**

## 4.2 {#4.2}

- Support for the timers in multi-server mode has been added. You can configure `WorkflowRuntime` to work in a single or multi-server environment. This affects the timers and the recovering from failure. You can configure the runtime with the following code:
  
  ```csharp
  _runtime = new WorkflowRuntime()
    ...
    .AsSingleServer() //.AsMultiServer()
   .Start();
  ```

  **WARNING. `AsMultiServer ()` - works for Ultimate licenses only.**

- If an abnormal server shutdown has occurred, then part of the processes may remain frozen in the *Running* status. This status will be changed by the automatic recovery procedure; one of the two statuses - either *Error* or *Terminated* - will be set. You can also change the recovery procedure using Process Restorer. For example, as follows:

  ```csharp
  public class ProcessRestorer : IProcessRestorer
  {
    ...
  }
  runtime.WithProcessRestorer(new ProcessRestorer())
  ```

  **Please, note that Process Restorer is only used in a case of server shutdown. For the workflow errors, use the `OnWorkflowError` event handler.**

- The `runtime.Shutdown()` and `runtime.ShutdownAsync()` methods has been added. These methods provide the correct shutdown of the `WorkflowRuntime`. Using this method is very important for a multi-server environment.
- The approach to the workflow execution has been changed. Now, the process is executed using a run loop. Therefore, you can create an endless workflow. One iteration of the loop is equivalent to one *Activity*. By default, the maximum number of iterations is limited by `Int64.MaxValue`. However, you can change it as follows:

  ```csharp
  _runtime.WithRuntimeSettings(new WorkflowRuntimeSettings()
  {
    MaxNumberOfPerformedActivities = -1 //Infinite loop allowed
  });
  ```

- Now, you can safely change the workflow state from the workflow itself during its operation. Use the following code:

  ```csharp
  processInstance.SetActivityAfterActionExecution("ActivityName");
  processInstance.SetActivityAfterActivityExecution("ActivityName");
  processInstance.SetStateAfterActionExecution("StateName");
  processInstance.SetStateAfterActivityExecution("StateName");
  ```

- If the complex object is stored in the process parameters, you can receive and set its properties on the fly, using partial parameters:
  
  ```csharp
  processInstance.GetParameter<T>("ObjectParameter.Object.Value");
  ```

- A new type of *Expressions* has been added. This is a simple expression that returns true or false. You can use them instead of *Conditions*. Here, you can use the process parameters values, as well as string formatting of these parameters. The syntax of these expressions is as follows:

  ```csharp
  @ObjectParameter.BooleanProperty and @IntParameter <> 1"
  ```

  Here the 'ObjectParameter.BooleanProperty' and 'IntParameter' are *Process parameters*.

- The parameters values ​​can be substituted into the values ​​of the *Actions*, *Conditions*, or *Rules*. The syntax is similar to the expressions:

  ```javascript
  {
     JsonObject = "@ObjectParameter:json",
     Int = "@IntParameter:json",
     IntString = "@IntParameter",
     Guid = "@GuidParameter:json",
     GuidString = "@(GuidParameter:N)",
     DateTime = "@(DateTimeParameter:json)",
     DateTimeString = "Today is @(DateTimeParameter:dd-MM-yyyy HH:mm:ss)",
  }
  ```

- You can connect the external parameter provider - `IWorkflowExternalParametersProvider` - to `WorkflowRuntime`. This allows you to access external data (for example, the document record which the process is linked to) as usual process parameters. It can be used in Expressions.

- A plugin to work with files has been added - *File Plugin*.
- A plugin to work with Inbox/Outbox has been added - *Approval Plugin*.
- HTTPRequest and CheckHTTPRequest methods added by *Basic Plugin* can now send POST.
- Methods to simplify work with parallel processes have been added to *Basic Plugin*.
- In the *Parameter edit forms* you can use the *Multiselect Dropdown*.

**The following additional actions must be taken to upgrade to Workflow Engine 4.2:**

- **Warning. RavenDB provider is no longer supported.**
- **Warning. If using Redis, please, contact our support for update instructions.**
- **Warning. If you are using MongoDB. The multi-server feature will work only with MongoDB version > 4.0**
- Run the SQL script update_4_1_to_4_2 for all relative databases and MongoDB.
  - [MSSQL](https://github.com/optimajet/WorkflowEngine.NET/blob/master/Providers/OptimaJet.Workflow.MSSQL/Scripts/update_4_1_to_4_2.sql)
  - [PostgreSQL](https://github.com/optimajet/WorkflowEngine.NET/blob/master/Providers/OptimaJet.Workflow.PostgreSQL/Scripts/update_4_1_to_4_2.sql)
  - [Oracle](https://github.com/optimajet/WorkflowEngine.NET/blob/master/Providers/OptimaJet.Workflow.Oracle/Scripts/update_4_1_to_4_2.sql)
  - [MySQL](https://github.com/optimajet/WorkflowEngine.NET/blob/master/Providers/OptimaJet.Workflow.MySQL/Scripts/update_4_1_to_4_2.sql)
  - [MongoDB](https://github.com/optimajet/WorkflowEngine.NET/blob/master/Providers/OptimaJet.Workflow.MongoDB/Scripts/update_4_1_to_4_2.js)
- Update all files related to the Designer. They are available [here](https://github.com/optimajet/WorkflowEngine.NET/tree/master/Designer).
- Update packages or dll to version 4.2. **If you connect the WFE with DLLs you must reference all DLLs from [this archive](https://workflowengine.io/downloads/assets/workflow-engine-net-4.2-core.zip) in your solution.
- Remove `runtime.WithBus(...)` call from your `WorkflowRuntime` configuration.
- At the point, where you configure `WorkflowRuntime` remove the `.WithTimerManager (...)` call. And add a call of  `.AsSingleServer()` or `.AsMultiServer()`, depending on the mode you want to run the `WorkflowRuntime`. **WARNING. `AsMultiServer ()` - works for Ultimate licenses only.**
- The signature of the `UsersInRoleAsync` delegate for *Basic Plugin* has changed.
  The way, it was:

  ```csharp
  public delegate Task<IEnumerable<string>> UsersInRoleAsyncDelegate(string roleName, Guid? processId = null);
  ```

  The way, it is now:

  ```csharp
   public delegate Task<IEnumerable<string>> UsersInRoleAsyncDelegate(string roleName, ProcessInstance processInstance);
  ```

- If you are using *.Net Framework* for your web application most likely you will must add the following lines in your *web.config*.

  ```xml
  <system.web>
    <compilation debug="true" targetFramework="4.5">
        <assemblies>
            <add assembly="System.Runtime, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" />
        </assemblies>
    </compilation>
    ...
  </system.web>
  ```

## 4.1 {#4.1}

- Support for multi-tenant applications.
  - *TenantId* has been added to processes. When creating a process, one can specify *TenantId* and use its value when working with the process. *TenantId* is stored in the *WorkflowProcessInstance* table in the *TenantId* column.
  Passing `TenantId` to a process:

    ```csharp
    var createInstanceParams = new CreateInstanceParams(schemeCode, processId) { TenantId = "tenantId" };
    workflowRuntime.CreateInstance(createInstanceParams);
    ```

    After creating a process with *TenantId* indicated, the access to it inside *Actions*, *Conditions* and *Rules* can be arranged as follows.

    ```csharp
    string tenantId = processInstance.TenantId;
    ```

  - For schemes one can specify tags, and then, search for schemes where these tags are indicated. Tags are set in the scheme designer by clicking on the *Process Info* button. Tags are stored in the *WorkflowScheme* table in the *Tags* column. A list of codes for the schemes where the corresponding tags are indicated can be received using the following code:

     ```csharp
     List<string> schemeCodes =  workflowRuntime.Builder.SearchSchemesByTags(new List<string> {"Tag1","Tag2"});
     ```

   The search is performed using an OR expression.

- Plugin System and the Basic Plugin. The plugin for WorkflowEngine.NET is a class that is necessary to implement the `IWorkflowPlugin` interface, and optional to implement the `IWorkflowActionProvider`, `IWorkflowRuleProvider`, `IDesignerParameterFormatProvider`, `IDesignerAutocompleteProvider` interfaces (in any combination). In fact, the plugin is a class that adds functionality to be used when creating process schemes. The plugin connects to WorkflowEngine.NET when configuring `WorkflowRuntime`.
  
  ```csharp
  WorkflowRuntime workflowRuntime = workflowRuntime.WithPlugin(new BasicPlugin());
  ```

  Simultaneously, any number of plugins can be connected to WorkflowEngine.NET. The Basic Plugin `OptimaJet.Workflow.Core.Plugins.BasicPlugin` has been added to the WorkflowEngine.NET package; it implements the following basic functions:
  - Actions:
    - *SendEmail* - sending email.
    - *CreateProcess* - creating a process from a process.
    - *HTTPRequest* - sending a request to a third-party web service.
    - *SetParameter* - setting a process parameter.
  - Conditions:
    - *IsProcessFinalized* - checking the finalization of the current process or a process with the Id specified.
    - *CheckAllSubprocessesCompleted* - checking the finalization (completion) of all the subprocesses.
    - *CheckParameter* - checking if the parameter is consistent to the given value (so far, only strings are supported).
    - *IsArrovedByUsers* - checking if the specified process was processed by all of the listed users.
    - *IsArrovedByRoles* - checking if the specified process was processed by all of the listed roles.
    - *CheckHTTPRequest* - conditional transition based on the result of a request to a third-party web service.
  - Authorization Rules (Security):
    - *CheckRole* - checking access to the command for a specific role.
  **Warning: to perform operations related to the roles checking, `BasicPlugin` must have the delegate handler `basicPlugin.UsersInRoleAsync` installed.**

- Now, implicit (that is, not explicitly specified in the scheme) parameters passed when creating a process, executing a command, or setting a new state to a process can be persistent.
  - When creating a process, use the following code:
  
    ```csharp
    var createInstanceParams = new CreateInstanceParams(schemeCode, processId)
      .AddPersistentParameter("Parameter1Name", 100)
      .AddPersistentParameter("ParameterName2", parameterValue);
    workflowRuntime.CreateInstance(createInstanceParams);
    ```

  - When passing parameters with a command, use the following code:

    ```csharp
    WorkflowCommand command = ...
    command.SetParameter("ParameterName", parameterValue, persist: true);
    workflowRuntime.ExecuteCommand(command, ... );
    ```
  
  - When passing a parameter with a command, use the following code:
  
    ```csharp
    var setStateParams = new SetStateParams(processId,"StateName")
      .AddPersistentParameter("Parameter1Name", 100)
      .AddPersistentParameter("ParameterName2", parameterValue);
    workflowRuntime.SetState(setStateParams);
    ```

- Support for dynamic parameters has been added. To perform the task, the `DynamicParameter` class, which can be cast into dynamic, has been developed. For example:  
  
  Creating a parameter:
  
  ```csharp
  var dynamicParameter = new
  {
    Name = "Dynamic",
    ObjectValue = new
    {
        Name = "Object",
        Value = 100
    },
    ListValue = new List<object> {
        new {Id = 1, Name = "ObjectInList1"},
        new {Id = 2, Name = "ObjectInList2"}
    }
  }
  processInstance.SetParameter("Dynamic", dynamicParameter, ParameterPurpose.Persistence);
  ```

  Getting a parameter:
  
  ```csharp
  var dynamicParameter = processInstance.GetParameter<DynamicParameter>("Dynamic") as dynamic;
  string name = dynamicParameter.Name;
  string objectValueName = dynamicParameter.ObjectValue.Name;
  string firstItemName = (dynamicParameter.ListValue as List<dynamic>).First().Name;
  ```

- The following aggregating providers are available: `AggregatingActionProvider`, `AggregatingRuleProvider`, `AggregatingDesignerAutocompleteProvider`, `AggregatingDesignerParameterFormatProvider`. An aggregating provider is a provider to which other providers can be added.
- `IWorkflowRuleProvider` - supports asynchronous authorization (security) rules.
- The scheme code is passed to all methods of all providers. Here are these methods
  - `IWorkflowActionProvider.GetActions`
  - `IWorkflowActionProvider.GetConditions`
  - `IWorkflowActionProvider.IsActionAsync`
  - `IWorkflowActionProvider.IsConditionAsync`
  - `IWorkflowRuleProvider.GetRules`
  - `IWorkflowRuleProvider.IsCheckAsync`
  - `IWorkflowRuleProvider.IsGetIdentitiesAsync`
  - `IDesignerParameterFormatProvider.GetFormat`
  - `IDesignerAutocompleteProvider.GetAutocompleteSuggestions`

  `string schemeCode` has been added as the last parameter to all these methods.
- A unified and correct error output when accessing the Designer API has been added.
- Intellisense has been added in the Code Actions (code in schemes) editor.
- A new type *TextArea* has been added to the forms where parameter (for Actions, Conditions or rules) values are edited.
- In any of the persistence providers, one can optionally turn off the history of transitions and set the history of subprocesses to be written in the history of the main process. For example:
  
  ```csharp
  var provider = new MSSQLProvider(connectionString, writeToHistory:false);
  var provider = new MSSQLProvider(connectionString, writeSubProcessToRoot:true);
  ```

## Update guide to WFE4.1 {#update4.1}

**The following additional actions must be taken to upgrade to Workflow Engine 4.1:**

- Run the following SQL script for all relative databases.
  - [MSSQL](https://github.com/optimajet/WorkflowEngine.NET/blob/master/Providers/OptimaJet.Workflow.MSSQL/Scripts/update_4_0_to_4_1.sql)
  - [PostgreSQL](https://github.com/optimajet/WorkflowEngine.NET/blob/master/Providers/OptimaJet.Workflow.PostgreSQL/Scripts/update_4_1_to_4_2.sql)
  - [Oracle](https://github.com/optimajet/WorkflowEngine.NET/blob/master/Providers/OptimaJet.Workflow.Oracle/Scripts/update_4_1_to_4_2.sql)
  - [MySQL](https://github.com/optimajet/WorkflowEngine.NET/blob/master/Providers/OptimaJet.Workflow.MySQL/Scripts/update_4_0_to_4_1.sql)
- Update all files related to the Designer. They are available [here](https://github.com/optimajet/WorkflowEngine.NET/tree/master/Designer).
- Update packages or dll to version 4.1.
- If the `IWorkflowActionProvider` interface is implemented in your project, the last parameter`string schemeCode` shall be added to the following methods:
  - `IWorkflowActionProvider.GetActions`
  - `IWorkflowActionProvider.GetConditions`
  - `IWorkflowActionProvider.IsActionAsync`
  - `IWorkflowActionProvider.IsConditionAsync`
- If the `IWorkflowRuleProvider` interface is implemented in your project, the following methods shall be added to your provider:
  
  ```csharp
  public Task<bool> CheckAsync(ProcessInstance processInstance, WorkflowRuntime runtime, string identityId, string ruleName,string parameter, CancellationToken token)
  {
      throw new NotImplementedException();
  }

  public Task<IEnumerable<string>> GetIdentitiesAsync(ProcessInstance processInstance, WorkflowRuntime runtime, string ruleName, string parameter, CancellationToken token)
  {
      throw new NotImplementedException();
  }

  public bool IsCheckAsync(string ruleName, string schemeCode)
  {
      return false;
  }

  public bool IsGetIdentitiesAsync(string ruleName, string schemeCode)
  {
      return false;
  }
  ```

  The last parameter `string schemeCode` shall be also added to the following method:
  - `IWorkflowRuleProvider.GetRules`

- If the `IDesignerParameterFormatProvider` interface is implemented in your project, the last parameter `string schemeCode` shall be added to the following method:
  - `IDesignerParameterFormatProvider.GetFormat`
- If the `IDesignerAutocompleteProvider` interface is implemented in your project, the last parameter `string schemeCode` shall be added to the following method:
  - `IDesignerAutocompleteProvider.GetAutocompleteSuggestions`
- **IMPORTANT! Incorrect behavior was fixed when the subprocess was merged in the parent process via the set state of the parent process mechanism. Previously, the parent process parameters were OVERWRITTEN. Now, the parent process parameters won't be changed. Only new parameters from the subprocess will be written to the parent process automatically. The same way the merge via calculating conditions always works. If you consciously exploited this behavior, then the best way to get parameters from the subprocess is to use a property `processInstance.MergedSubprocessParameters` when merge occurs.**
- **IMPORTANT! If in your project the Action Provider has changed (after the first initialization) using the method `workflowRuntime.WithActionProvider(...)` replace this code with the following call `workflowRuntime.ClearActionProvider().WithActionProvider(...)`**
- **IMPORTANT! If in your project the Rule Provider has changed (after the first initialization) using the method `workflowRuntime.WithRuleProvider(...)` replace this code with the following call `workflowRuntime.ClearRuleProvider().WithRuleProvider(...)`**
- **IMPORTANT! If in your project the Designer Autocomplete Provider has changed (after the first initialization) using the method `workflowRuntime.WithDesignerAutocompleteProvider(...)` replace this code with the following call `workflowRuntime.ClearDesignerAutocompleteProvider().WithDesignerAutocompleteProvider(...)`**
- **IMPORTANT! If in your project the Designer Parameter Format Provider has changed (after the first initialization) using the method `workflowRuntime.WithDesignerParameterFormatProvider(...)` replace this code with the following call `workflowRuntime.ClearDesignerParameterFormatProvider().WithDesignerParameterFormatProvider(...)`**
- It is not necessary but suggested to change the Designer Controller the following way

  ```csharp
  public ActionResult API()
  {
    ...
    var res = WorkflowInit.Runtime.DesignerAPI(pars, out bool hasError, filestream, true);
    var operation = pars["operation"].ToLower();

    if (operation == "downloadscheme" && !hasError)
      return File(Encoding.UTF8.GetBytes(res), "text/xml", "scheme.xml");
    else if (operation == "downloadschemebpmn" &&  !hasError)
      return File(UTF8Encoding.UTF8.GetBytes(res), "text/xml", "scheme.bpmn");

    return Content(res);
  }
  ```

  See complete controller code
  - [ASP.NET MVC Core](https://github.com/optimajet/WorkflowEngine.NET/blob/master/Samples/ASP.NET%20Core/MSSQL/WF.Sample/Controllers/DesignerController.cs)
  - [ASP.NET MVC](https://github.com/optimajet/WorkflowEngine.NET/blob/master/Samples/ASP.NET%20MVC/MSSQL/WF.Sample/Controllers/DesignerController.cs)
  - [Web Forms](https://github.com/optimajet/WorkflowEngine.NET/blob/master/Samples/ASP.NET%20WebForms/WebFormsMSSQL/WF.Sample/Pages/Designer/WFEDesigner.ashx.cs)

- It is not necessary but suggested to pass the scheme code when calling `wfdesigner.create()` method on the Designer page.

  ```javascript
  wfdesigner.create(schemecode);
  ```

  See complete view code
  - [ASP.NET MVC Core](https://github.com/optimajet/WorkflowEngine.NET/blob/master/Samples/ASP.NET%20Core/MSSQL/WF.Sample/Views/Designer/Index.cshtml)
  - [ASP.NET MVC](https://github.com/optimajet/WorkflowEngine.NET/blob/master/Samples/ASP.NET%20MVC/MSSQL/WF.Sample/Views/Designer/Index.cshtml)
  - [Web Forms](https://github.com/optimajet/WorkflowEngine.NET/blob/master/Samples/ASP.NET%20WebForms/WebFormsMSSQL/WF.Sample/Pages/Designer/Index.aspx)

## 4.0 {#4.0}

- Designer usability improvement. Transition info will now be displayed in a fuller, more comprehensive form. You can now switch between full screen and normal edit window display modes. Toolbars design has been changed.
- you can customize Activity и Transition rendering in the Designer. 
- you can customize Designer windows. 
- Designer performance has been optimized. 
- Scheme inlining. Now you can check a scheme as a scheme that can be inlined and embed it into another scheme. Thus you can re-use typical parts of your processes many times, without copying them between schemes. Multi-layered inlining is supported.  
- Process Info window has been added into the specific process view mode. It allows you to view this process parameters, transition history, launched timers. Here full information on subprocesses is also displayed.
- You can specify annotations for Activity and Transition. Annotations are a dictionary (key - value) which you can set in the Designer individually for each Activity ot Transition. You can read annotation value in the code, using the following methods: `activityDefinition.GetAnnotation<T>(name)`, `transitionDefinition.GetAnnotation<T>(name)`, `processInstance.ProcessScheme.GetActivityAnnotation<T>(activityName, name)`, `processInstance.ProcessScheme.GetTransitionAnnotation<T>(transitionName, name)`
- For the string parameter, which is transferred into Actions, Conditions and Rules, you can specify the structure which will define the form in which this parameter will be displayed in edit mode in the Designer. Form field contents can be specified in the Designer in the `CodeActions` section. Or you can create a class implementing `IDesignerParameterFormatProvider` interface on the server and configure your  `WorkflowRuntime` in the following way: `workflowRuntime.WithDesignerParameterFormatProvider(new YourDesignerParameterFormatProvider())`. Thus you can specify the appearance of the string parameter which is transferred into Action, Condition or Rule.
- In the event handler `workflowRuntime.OnWorkflowError` you can now cancel exception throwing, using event arguments `args.SuppressThrow = true;`. Also you can specify the Activity, which will be set after error processing. For example, it can be initial activity: `args.ActivityToSet = args.ProcessInstance.ProcessScheme.InitialActivity;`
- For simple execution of complex business cases in `WorkflowRuntime` use two of the following methods: `workflowRuntime.GetAvailableCommandsWithConditionCheck(...)` - get the list of available commands with additional conditions check, and `workflowRuntime.ExecuteCommandWithRestrictionCheck(...)` - execution of the command with additional restrictions check.
- Correct merging of the subprocess и parent process has been added, when a subprocess is merged with its parent process immediately after launch. In other words, if a subprocess contains only Auto triggered transitions. Now merge will be correct, and the subprocess will wait till the parent process is unlocked. 
- Process execution can be cancelled using `CancellationToken`. Such cancellation will be activated automatically if you configure your `WorkflowRuntime` in the following way: `workflowRuntime.SetCancellationTokenHandling(CancellationTokenHandling.Throw)`.
 
**The following additional actions must be taken to upgrade to Workflow Engine 4.0:**

- Don't forget to update packages or dlls, and the Designer javascript, css and all designer related images.
- Run the SQL  script `update_4_0.sql` for all relative databases. You will find this script in your provider's archive.  
- If you have used process status change (for example `args.ProcessStatus = ProcessStatus.Idled;`) to cancel exception release after the event has been processed `workflowRuntime.OnWorkflowError`, you will need to use the following code `args.SuppressThrow = true;`. Status change hack won't work, custom status will be installed, but the exception will still be thrown. 

## 3.5 {#3.5}

- Moving the canvas (in the Move mode) with arrows was added to the designer.
- Explicit passing of CultureInfo was added to methods `GetInitialCommands`, `GetInitialState`, `GetCurrentState` and `GetAvailableCommands` of the `WorkflowRuntime` class
- Full samples (Vacation request approval) for all supported databases for ASP.NET Core and ASP.NET MVC. Full samples for MSSQL, Postgres and Oracle database for Web Forms.

## 3.4 {#3.4}

- Added automatic size increase for background in designer.
- Added designer localization files for German, French, Spanish, Italian, Portuguese, Turkish, and Russian languages.
- .NET Core NuGet packages come with .NET Standard 1.6 and .NET Standard 2.0 libraries. This allows you to use these packages with any .NET Core version, including 2.1.
- The `OptimaJet.Workflow.Core.Logging.ILogger` interface was added; it can be initializes with your own logger in the `WorkflowRuntime` object. It is used to integrate with the Workflow Server logging system, but you can use it to integrate with your own system. Bear in mind that the logger in the `WorkflowRuntime` object is not initialized by anything by default.

## 3.3 {#3.3}

- MongoDB provider for Workflow Engine .NET Core was added.
- MongoDB driver was updated to version 2.7. You can now connect to CosmosDB through a MongoDB connection.
- The following hotkeys were added to the Workflow Designer:
  - Ctrl + A - Select all
  - Ctrl + C - Copy selected items
  - Ctrl + E - New Activity
  - Ctrl + I - Extended info
  - Ctrl + Y - Redo
  - Ctrl + Z - Undo
  - Arrows - Move selected items
  - Delete - Delete
  - Alt + Enter - Full-screen mode
  - Ctrl + M - Move mode

**The following additional actions must be taken to upgrade to Workflow Engine 3.3 if you are using MongoDB:**

- Only for MongoDB users: it is necessary to apply the _update_3_2_to_3_3.js_ script to your database.

## 3.2 {#3.2}

- Added providers for MySQL and Oracle, running under .NET Core
- Added class `AggregatingRuleProvider`, designed to combine several `IWorkflowRuleProvider` into one `IWorkflowRuleProvider`.
- Added class `AggregatingActionProvider`, designed to combine several `IWorkflowActionProvider` into one `IWorkflowActionProvider`.

## 3.1 {#3.1}

- Workflow Engine's relational database storage system has been optimized. Additional indices have been built; obsolete indices have been removed; the size of some columns has been changed. Generally, these changes should result in the improvement of Workflow Engine's performance, especially for Microsoft SQL Server. All these changes have already been included into installation scripts; use the _update_3_1.sql_ script to update the existing databases.
- BulkCreateInstance now works for Microsoft SQL Server and .NET Core (version >= 2).

**Warning**

The _update_3_1.sql_ script contains a change of index and size of certain columns. Be particularly careful when applying it to the production database.

## 3.0 {#3.0}

- The interface of Workflow Designer has been revamped to improve usability
  - The look and feel of the scheme has been changed
  - The library that renders popup windows and controls has been changed from jQuery UI to [Semantic-UI](https://github.com/Semantic-Org/Semantic-UI). jQuery UI has been removed from the project completely. Autocomplete for lists has been implemented with [jQuery-autoComplete](https://github.com/Pixabay/jQuery-autoComplete)
  - The [Konva.js](https://konvajs.github.io/) version has been updated to 2.0.2
  - The 'Extended Info' mode has been added to provide additional information needed when creating a workflow scheme
  - Undo and redo have been added
  - Current activity of subprocesses is now highlighted
  - Global CodeActions have been simplified
  - Scheme legend has been added
- Builds for .NET Core 2.0 and .NET Standard 2.0 have been included to .NET Core packages
- The order of search for Action, Condition and Rule in Code Actions and `IWorkflowActionProvider`(`IWorkflowRuleProvider`) has been changed. Earlier on, the order was as follows (highest to lowest priority): Global CodeAction, CodeAction in the scheme, `IWorkflowActionProvider`(`IWorkflowRuleProvider`). Now this order is the following by default: CodeAction in the scheme, Global CodeAction, `IWorkflowActionProvider`(`IWorkflowRuleProvider`). Thus, CodeAction in the scheme has the highest priority. Search priority can be set with the `runtime.SetExecutionSearchOrder(ExecutionSearchOrder order)` setting.
- Parameter type names used to be stored as an assembly qualified name (by specifying the version of the build and the public key token), which resulted in troubles when migrating schemes from the .NET Framework environment to the .NET Core environment. Now a simplified type name - the one that is displayed in Designer - is stored in the scheme. Old schemes are loaded without changes; type names will be replaced after the first save of the scheme in the Designer.
- Errors that occur when there's a '-' in CodeAction names have been fixed.

**The following additional actions must be taken to uprgade to Workflow Engine 3.0:**

- It is not necessary to update to the new version of the Designer; however, we strongly advise it. The old version of the Designer shall work with the new versions of the Workflow Engine at least within the next half a year. If you are updating to the new version of the Designer, introduce the following changes to the pages where Designer is displayed.
  - Delete links to **jquery-ui.min.css** and **jquery-ui.js**
  ````html
  		<link href="/Content/themes/base/jquery-ui.min.css" rel="stylesheet" type="text/css"/>
  		<script src="/Scripts/jquery-ui.js" type="text/javascript"/>
  		```
  - Add links to **semantic.min.css**, **semantic.min.js**, **jquery.auto-complete.min.js**
  		```html
  		<link href="/Content/semantic.min.css" rel="stylesheet" type="text/css"/>
  		<script src="/Scripts/semantic.min.js" type="text/javascript"/>
  		<script src="/Scripts/jquery.auto-complete.min.js" type="text/javascript"/>
  ````
- If the new order of search for Action, Condition and Rule does not suit you, change it with the following setting:

```csharp
runtime = runtime.SetExecutionSearchOrder(ExecutionSearchOrder.GlobalLocalProvider);
```

Then, everything shall work the same way it did in the previous versions.

- Workflow Engine's reaction to the scenario where it could not find an Action, Condition or Rule in CodeActions or `IWorkflowActionProvider`(`IWorkflowRuleProvider`) has been changed. Earlier on, Workflow Engine ignored this scenario which made it difficult to debug schemes. Now, the `NotImplementedException` exception is thrown, specifying the name of the object which was not found. If this behavior does not suit you, use the following setting:

```csharp
runtime = runtime.SetIgnoreMissingExecutionItems(true);
```

## 2.3 {#2.3}

- A 'Refresh' button and its functionality have been added to Designer
- A 'Full Screen' button and its functionality have been added to Designer
- BulkCreateInstance and TimerManager performance has been enhanced
- Scroll-based scaling has been added to Designer

## 2.2 {#2.2}

- Now it is possible to create asynchronous Actions and Conditions. You can call asynchronous methods from Actions and Conditions using the `await` keyword. Such methods will be the most effective if you use asynchronous methods of the `WorkflowRuntime` object, for example, `ExecuteCommandAsync` instead of `ExecuteCommand`, or `SetStateAsync` instead of `SetState`, etc. You can create asynchronous Actions in Designer. To do that you simply need to check the Async checkbox in the Action or Condition where you're going to call asynchronous methods from. If you use `IWorkflowActionProvider`, then you will need to implement 4 additional methods. `bool IsActionAsync(string name)` and `bool IsConditionAsync(string name)` should return true so that the Action or Condition are called asynchronously. The execution of an asynchronous Action or Condition is done in the `Task ExecuteActionAsync(string name, ProcessInstance processInstance, WorkflowRuntime runtime, string actionParameter, CancellationToken token)` and `Task<bool> ExecuteConditionAsync(string name, ProcessInstance processInstance, WorkflowRuntime runtime, string actionParameter, CancellationToken token)` methods.
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

## 2.1 {#2.1}

- Workflow Engine for .NET Core App 1.1 is released. All Workflow Engine functions are supported. This version will be updated simultaneously along the .NET Framework version. 2 persistence providers are currently supported: MS SQL Server and PostgreSQL. Links to NuGet packages and samples can be found [here](/downloads/net/).
- Workflow Engine scheme import/export to/from BPMN2 has been added.
- Bulk process creation has been added. Now you can create a large amount of processes (100 - 100,000) in significantly less time than when using the `CreateInstance` method. Use the `_runtime.BulkCreateInstance(..)` method to do that. Currently, the feature is available in the .NET Framework version of Workflow Engine and supports only MS SQL Server. The list of supported databases will be expanded.

## 2.0 {#2.0}

- Designer UI has been improved, transitions are now linear for better usability
- Scheme rendering library [KineticJS](https://github.com/ericdrowell/KineticJS) has been replaced with [Konva.js](https://konvajs.github.io/)
- `WorkflowDesigner.readonlymode()`, `WorkflowDesigner.printablemode()` and `WorkflowDesigner.ediatblemode()` methods have been added to `WorkflowDesigner` object in the client. `readonlymode` makes Designer uneditable. `printablemode` removes the toolbar, the grid and all controls, zooming designer so that the scheme occupies the entire canvas. This mode can be used to print Designer's viewport to a PDF within a browser. `editablemode` makes Designer editable.
- Designer now allows users to make an autocomplete list for the following fields: `Actor.Value`, `Activity.Implementation.Action parameter`, `Transition.Condition.ActionParameter`. The autocomplete list is being populated server-side via the `IDesignerAutocompleteProvider` interface and by transferring implementation to the `WorkflowRuntime` object via `_runtime.WithDesignerAutocompleteProvider(provider)`.
- The `GetConditions` method has been added to the `IWorkflowActionProvider` interface. Now, the choice of conditions and actions in Designer can be split.
- Parameters of any type not mentioned in the scheme can now be used in Actions' code. The type should be serialized in JSON. Three methods are available: `_processInstance.SetParameter("parameterName",parameterValue,ParameterPurpose.Persistence)`, `_processInstance.GetParameter<ParameterType>("parameterName")` and `_processInstance.RemoveParameter("parameterName")`.
- Ability to create Timers with a value which is undefined at the time of creation of a process has been added. `Timer.Value == -1` means that the current transition will not be executed until the respective timer has a defined value. `Timer.Value == 0` means that in case the timer connected to a transition does not have a value, this transition shall be processed automatically. Initial value of a timer can be set with the `_runtime.NeedTimerValue` event.
- Ability to modify execution time for timers in running processes has been added. Use methods `_runtime.SetTimerValue(processId,timerName,newValue)` and `_runtime.ResetTimerValue(processId, timerName)` to change and reset timer values outside the process. Use methods `_runtime.SetTimerValue(processInstance,timerName,newValue)` and `_runtime.ResetTimerValue(processInstance, timerName)` to change and reset timer values from within your Actions.
- Newtonsoft.Json version has been changed from 7.0.1 to 9.0.0.

**The following additional actions must be taken to uprgade to Workflow Engine 2.0:**

- Replace the link to KineticJS with the link to konva.min.js. Konva.js library is included in the ZIP archive and in `nuget package WorkflowEngine.NET-Designer`.
- Update Newtonsoft.Json.dll to 9.0.0. The library is included in the ZIP archive and in `nuget package WorkflowEngine.NET-Core`.
- If you use `implementation` of `IWorkflowActionProvider` you should add a `public List<string> GetConditions()` method to it. It is advisable that the `GetActions` returns only the list of available Actions, whereas the `GetConditions` method should return the list of available Conditions. In this case they should be filtered properly in the Designer. However, if you do not want to modify your code, make sure that the `GetConditions` method throws a `NotImplementedException`. In this case, everything should work as it used to.
- IF you use Oracle, you should run an `update_1_5_to_2_0.sql` script from the Oracle provider ZIP archive or a NuGet package.

## 1.5.6 {#1.5.6}

- Two new persistence providers were added to WorkflowEngine.NET Redis Provider for [Redis](http://redis.io/) and Ignite Provider for [Apache Ignite](https://ignite.apache.org/)
- notrendertoolbar property was added to the Designer object configuration (client side javascript). You can hide toobar in the workflow designer for end users.
- Second parameter which allow ignore AutoSchemeUpdate sign of Activity was added to the WorkflowRuntime.UpdateSchemeIfObsolete method.
- Several redundat operations related to subprocess features were removed to increase performance.
- Conditions are checked for transitions that creating subprocesses.

## 1.5.5 {#1.5.5}

In this release several features have been added to simplify the generation of forms based on commands.

- The sign `IsRequired` for command parameter. You can take it into account when generating forms, also it is used in the command validation before the execution of command.
- The `Default value` for command parameter. You can access it using `CommandParameter.DefaultValue` property, after you have received the list of available commands. You can set all command parameters to default value using `WorkflowCommand.SetAllParametersToDefault` or `WorkflowCommand.SetParameterToDefault` functions. The Default value must be a valid JSON (wich can be deserialized to type which is specified in the bond Parameter) or will be interpreted as a string.
- An `autocompete` was added in the field `Type of Parameter` (editing window Parameters). It makes a suugestions about types which can be used such as primitive types (Int32, String etc) or your custom types. Types from assemblies which was registerd using `_runtime.RegisterAssemblyForCodeActions` function are added in the autocomplete list. To prevent registration or filter the list you can use the last two optional parameters of \_runtime.RegisterAssemblyForCodeActions function - ignoreForDesigner and designerTypeFilter.
- The `Initial values` were added for Parameters. You can yse Initial value only for Parameters which have Purpose = Persistence. This values must be a valid JSON (wich can be deserialized to type which is specified in the bond Parameter) or will be interpreted as a string. These values will be set to the process when it is created.
- The JSON editor was added for edit `Parameter.InitialValue` (Parameters window), `Command.InputParameters.DefaultValue` (Commands window), `Actor.Value` (Actors window),
  `Activity.Implementation.ActionParameter` (Activity window), `Transition.Condition.ActionParameter` (Transition window). The JSON editor includes a syntax highlight and Format button. The Format button can be used to format your JSON. Please note that if you use JSON with unquoted property names, you must add reference on `json5.js` library on Designer page. For Parameter.InitialValue (Parameters window) and Command.InputParameters.DefaultValue (Commands window) the JSON editor aso includes Create button. This button can be used to create an empty object based on Parameter.Type. Designer (on server) uses only parameterless constructor to create the empty object.
- There are two new events was added to `WorkflowRuntime`. Both are occured only in normal execution mode (not in pre-execution). `BeforeActivityExecution` - is occured after the Runtime has chosen an appropriate transition for execution but before Actions in an Activity were executed. `ProcessActivityChanged` - is occured after some Activity was executed. You can use `ProcessActivityChanged.TransitionalProcessWasCompleted` property to ensure that the command (or timer) execution was finished. Using of these events could be more convenient than `ProcessStatusChanged` in some cases.
- `GetAllActorsForCommandTransitions(ProcessInstance)` method was added to `WorkflowRuntime`. You can use this method to get next potential approvers for current state.
- You can change the JSON Serializer settings for Parameters derialization using SetParameterSerializerSettings configuration method.

## 1.5.4 {#1.5.4}

- Schema was added in constructors of all SQL providers. For MsSQL the default schema is "dbo". For PostgreSql the default schema is "public". For Oracle the default schema is null.
- You are able to specify an interval timer value with milliseconds, seconds, minutes and days. For example "1d 2h 4m 30s" - one day two hours four minutes thirty seconds or "5m 30s", "14d", "12h", "10m", "10s" etc. You are able to use following names. d, day or days to specify interval in days. h, hour or hours to specify interval in hours. m, minute or minutes to specify interval in minutes. s, second or seconds to specify interval in seconds. ms, millisecond or milliseconds to specify interval in milliseconds. If the interval is specified as just a numeric value, it is interpreted as the interval in milliseconds.
- You are able to pass initial parameters of a process by using `_runtime.CreateInstance(CreateInstanceParams createInstanceParams)` method. Parameters passed in the createInstanceParams.InitialProcessParameters property will be used as initial for parameters of a process.

## 1.5.3 {#1.5.3}

- The JSON serializer used by the engine was changed from `ServiceStack.Text to Newtonsoft.JSON`. 1.5.3 version of WFE was built with the `verion 7.0 of Newtonsoft.JSON`.
- _MSSQL provider_ was rewritten from LINQtoSQL to ordinary SQL queries.
- The _Parameter_ window in the designer was implroved. Now you can hide System parameters. They are hidden by default.
- Working with types in the _Parameter_ window in the designer was improved. Now you are able to use short type's names such as String, Guid, Int32 etc for primitive types. The type names including namespace for custom types (OptimaJet.Workflow.Core.Model.ActivityDefinition, Business.Approvers etc)- Also you are able to use <> for generic types and [] for arrays.
- Fixed a bug in the timers for their correct restart after restart of the workflow runtime.

## 1.5.2 {#1.5.2}

- `ExecutedActivity` and `ExecutedTransition` properties added in `ProcessInstance` class. Via them, you can access executed transition and activity during a transitional process.
- A scheme of a process downloaded from the designer with not ecoded _Code Actions_. You can upload a scheme with encoded or not encoded code actions.
- The algorithm of sub-process scheme separation improved for it to work properly with the different schemes.
- `GetAvailableCommands` method returns distinct commands.
- Fixed parsing of the values of the _Timers_ in different cultures
- While merging the processes you can access the parameters of a subprocess using the property `MergedSubprocessParameters` of the `ProcessInstance` class.
