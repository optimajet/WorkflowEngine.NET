var WorkflowDesignerConstants = {
    SelectColor: 'yellow',
    ActivityColor: '#F2F2F2',
    ActivityInitialColor: '#CCCC99',
    ActivityFinalColor: '#CC9966',
    ActivityCurrentColor: '#00CCCC',

    DeleteConfirm: 'Вы уверены, что хотите удалить выбранные элементы?',
    FieldIsRequired: 'Поле обязательно!',
    FieldMustBeUnique: 'Значение должно быть уникально!',
    ButtonTextDelete: 'Удалить',
    ButtonTextCreate: 'Создать',

    InfoBlockLabel:{
        Activity: 'Состояния: ',
        Transition: 'Переходы: ',
        Command: 'Команды: ',
    },

    ActivityNamePrefix: 'Activity_',
    ActivityFormLabel: {
        Title: 'Состояние',
        Name: 'Наименование',
        State: 'Статус',
        IsInitial: 'Начальный',
        IsFinal: 'Конечный',
        IsForSetState: 'Доступно для установки состояния',
        IsAutoSchemeUpdate: 'Автоматическое обновление схемы',
        Implementation: 'Выполнение',
        PreExecutionImplementation: 'PreExecution реализация',
        ImpOrder: 'Порядок',
        ImpAction: 'Действие',
        ImpActionParameter: 'Параметр',
        AlwaysConditionShouldBeSingle: 'Always условие должн быть одно',
        OtherwiseConditionShouldBeSingle: 'Otherwise условие должно быть одно'
        
    },

    TransitionFormLabel: {
        Title: 'Переход',
        Name: 'Наименование',
        From: 'Из состояния',
        To: 'В состояние',
        Classifier: 'Тип перехода',
        Restrictions: 'Ограничения',
        RestrictionsType: 'Тип',
        RestrictionsActor: 'Актер',
        Condition: 'Условия',
        ConditionType: 'Тип',
        ConditionAction: 'Действие',
        ResultOnPreExecution: 'Результат для PreExecution',
        Trigger: 'Триггер',
        TriggerType: 'Тип',
        TriggerCommand: 'Команда',
        TriggerTimer: 'Таймер',
        ConditionActionParameter: 'Параметр',
        ConditionInversion: 'Инвертировать результат',
        ConditionsConcatenationType: 'Тип сложения для условий',
        AllowConcatenationType: 'Сложение разрешений',
        RestrictConcatenationType: 'Сложение запретов',
        ConditionsListShouldNotBeEmpty: 'Список условий не может быть пустым'

    },
    LocalizationFormLabel: {
        Title: 'Локализация',
        ObjectName: 'Объект',
        Type: 'Тип',
        IsDefault: 'По умолчанию',
        Culture: 'Язык',
        Value: 'Значение',
        Types: ['Command', 'State', 'Parameter'],
    },

    TimerFormLabel: {
        Title: 'Таймеры',
        Name: 'Наименование',
        Type: 'Тип',
        Value: 'Значение',
        Types: ['Command', 'State', 'Parameter'],
        NotOverrideIfExists : "Не сбрасывать таймер, если он существует"
    },

    ParameterFormLabel: {
        Title: 'Parameters',
        Name: 'Наименование',
        Type: 'Тип',
        Purpose: 'Назначение',
        Value: 'Значение',
        DefaultValue: 'Значение по умолчанию'
    },

    ActorFormLabel: {
        Title: 'Актеры',
        Name: 'Наименование',
        Rule: 'Правило',
        Value: 'Параметры'
    },

    CommandFormLabel: {
        Title: 'Команды',
        Name: "Наименование",
        InputParameters: "Входящие параметры",
        InputParametersName: 'Наименование',
        InputParametersParameter: 'Параметр'
    },

    AdditionalParamsFormLabel: {
        Title: 'Дополнительные параметры',
        IsObsolete: "Устаревший",
        DefiningParameters: 'Установленные параметры',
        ProcessParameters: 'Параметры процесса',
        ProcessParametersName: 'Наименование',
        ProcessParametersValue: 'Значение'
    },
     CodeActionsFormLabel: {
        Title: 'Действия',
        Name: 'Наименование',
        ActionCode: 'Код действия',
        IsGlobal: 'Глобальный',
        Type: 'Тип'
    },

    ToolbarLabel: {
        CreateActivity: 'Создать состояние',
        CopySelected: 'Копированить выбранные элементы',
        Undo: 'Отменить',
        Redo: 'Повторить',
        Move: 'Режим перемещения',
        ZoomIn: 'Увеличение масштаба',
        ZoomOut: 'Уменьшение масштаба',
        ZoomPositionDefault: 'Установить масштабирование и позиционирование в значения по умолчанию',
        AutoArrangement: 'Упрядочить элементы',
        Actors: 'Актеры',
        Commands: 'Команды',
        Parameters: 'Параметры',
        Localization: 'Локализация',
        Timers: 'Таймеры',
        AdditionalParameters: 'Дополнительные парамтеры',
        CodeActions: 'Действия'
    },
    ErrorActivityIsInitialCountText: "Один элемент должен быть отмечен флагом Начальный",
    ErrorReadOnlySaveText: "Дизайнер в режиме только для чтения, вы не можете сохранить схему.",
    FormMaxHeight: 500,
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
        Title: "Редактирование кода действия",
        EditCodeButton: 'Редактировать код',
        Usings: 'Пространства имен',
        Compile: "Компилировать",
        CompileSucceeded: "Компиляция успешна.",
        Success: "Успешно",
        Error: "Ошибка",
        OK: "Закрыть окно"
    }
};