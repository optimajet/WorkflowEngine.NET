var WorkflowDesignerConstants = {
    SelectColor: '#CCCC00',
    ActivityColor: '#F2F3F5',
    ActivityInitialColor: '#CBF4B2',
    ActivityFinalColor: '#DDEAFA',
    ActivityCurrentColor: '#FFCC99',
    DeleteConfirm: 'Are you sure you want to delete selected item(s)?',
    FieldIsRequired: 'Field is required!',
    FieldMustBeUnique: 'Field must be unique!',
    ButtonTextDelete: 'Delete',
    ButtonTextCreate: 'Create',
    ButtonTextSave: 'Save',
    ButtonTextCancel: 'Cancel',
    ButtonTextClose: 'Close',
    SaveConfirm: 'Save changes?',
    DialogConfirmText: "Question",

    InfoBlockLabel:{
        Activity: 'Activity: ',
        Transition: 'Transition: ',
        Command: 'Command: ',
    },

    ActivityNamePrefix: 'Activity_',
    ActivityFormLabel: {
        Title: 'Activity',
        Name: 'Name',
        State: 'State',
        IsInitial: 'Initial',
        IsFinal: 'Final',
        IsForSetState: 'For set state',
        IsAutoSchemeUpdate: 'Auto scheme update',
        Implementation: 'Implementation',
        PreExecutionImplementation: 'PreExecution Implementation',
        ImpOrder: 'Order',
        ImpAction: 'Action',
        ImpActionParameter: 'Action parameter',
        AlwaysConditionShouldBeSingle: 'Always condition should be single',
        OtherwiseConditionShouldBeSingle: 'Otherwise condition should be single'
        
    },

    TransitionFormLabel: {
        Title: 'Transition',
        Name: 'Name',
        From: 'From activity',
        To: 'To activity',
        Classifier: 'Classifier',
        Restrictions: 'Restrictions',
        RestrictionsType: 'Type',
        RestrictionsActor: 'Actor',
        Condition: 'Condition',
        ConditionType: 'Type',
        ConditionAction: 'Action',
        ResultOnPreExecution: 'Result on PreExecution',
        Trigger: 'Trigger',
        TriggerType: 'Type',
        TriggerCommand: 'Command',
        TriggerTimer: 'Timer',
        ConditionActionParameter: 'Action parameter',
        ConditionInversion: 'Invert action result',
        ConditionsConcatenationType: 'Conditions concatenation type',
        AllowConcatenationType: 'Concat allow as',
        RestrictConcatenationType: 'Concat restrict as',
        ConditionsListShouldNotBeEmpty: 'Conditions list should not be empty',
        IsFork: 'Is fork',
        MergeViaSetState: 'Merge subprocess via set state',
        DisableParentStateControl: 'Disable parent process control'

    },
    LocalizationFormLabel: {
        Title: 'Localization',
        ObjectName: 'ObjectName',
        Type: 'Type',
        IsDefault: 'IsDefault',
        Culture: 'Culture',
        Value: 'Value',
        Types: ['Command', 'State', 'Parameter'],
    },

    TimerFormLabel: {
        Title: 'Timers',
        Name: 'Name',
        Type: 'Type',
        Value: 'Value',
        Types: ['Command', 'State', 'Parameter'],
        NotOverrideIfExists : "Do not override timer if exists"
    },

    ParameterFormLabel: {
        Title: 'Parameters',
        Name: 'Name',
        Type: 'Type',
        Purpose: 'Purpose',
        Value: 'Value',
        InitialValue: 'InitialValue',
        ShowSystemParameters : 'Show system parameters'
    },

    ActorFormLabel: {
        Title: 'Actors',
        Name: 'Name',
        Rule: 'Rule',
        Value: 'Value'
    },

    CommandFormLabel: {
        Title: 'Command',
        Name: "Name",
        InputParameters: "Input Parameters",
        InputParametersName: 'Name',
        InputParametersIsRequired: 'Required',
        InputParametersParameter: 'Parameter',
        InputParametersDefaultValue: 'Default'
    },

    AdditionalParamsFormLabel: {
        Title: 'Additional Parameters',
        IsObsolete: "IsObsolete",
        DefiningParameters: 'Defining parameters',
        ProcessParameters: 'Process parameters',
        ProcessParametersName: 'Name',
        ProcessParametersValue: 'Value'
    },
     CodeActionsFormLabel: {
        Title: 'Code actions',
        Name: 'Name',
        ActionCode: 'Action code',
        IsGlobal: 'Is global',
        IsAsync: 'Async',
        Type: 'Type'
    },

    ToolbarLabel: {
        CreateActivity: 'Create activity',
        CopySelected: 'Copy selected',
        Undo: 'Undo',
        Redo: 'Redo',
        Move: 'Move',
        ZoomIn: 'Zoom In',
        ZoomOut: 'Zoom Out',
        ZoomPositionDefault: 'Zoom and position default set',
        AutoArrangement: 'Auto arrangement',
        Actors: 'Actors',
        Commands: 'Commands',
        Parameters: 'Parameters',
        Localization: 'Localization',
        Timers: 'Timers',
        AdditionalParameters: 'Additional Parameters',
        CodeActions: 'Code actions'
    },
    ErrorActivityIsInitialCountText: "One element must be marked flag Initial",
    ErrorReadOnlySaveText: "The Designer in ReadOnly mode, you can't save it.",
    FormMaxHeight: 700,
    EditCodeSettings: {
        Height: 600,
        Width: 1000,
        CodeHeight: 390,
        MessageBoxHeight: 400,
        MessageBoxWidth: 600,
        SuccessBoxHeight: 150,
        SuccessBoxWidth: 300
    },
    EditCodeLabel: {
        Title: "Edit code",
        EditCodeButton: 'Edit code',
        Usings: 'Usings',
        Compile: "Compile",
        CompileSucceeded: "Compilation succeeded.",
        Success: "Success",
        Error: "Error",
        OK: "OK"
    },
    EditJSONSettings: {
        Height: 600,
        Width: 1000,
        CodeHeight: 480
    },
     EditJSONLabel: {
        Title: "Edit value in JSON",
        CreateEmptyType: "Create",
        Format: "Format"       
     },
    isjava: false
};