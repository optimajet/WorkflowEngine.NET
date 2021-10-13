
# ASP.NET Boilerplate Angular version & WorkflowEngine

# Get Started

## Download

Create & download your project from https://aspnetboilerplate.com/Templates

## Настройка Asp.Net Core решения

Все изменения, нужные для бекенд части приложения, вы можете найти в следующем [коммите]( https://github.com/optimajet/WorkflowEngine.NET/commit/fe1671d68e5c1aca3a1e9f391529e6079498df09). Все изменения в коде выделены комментарием ```WorkflowEngineSampleCode```. Вы можете использовать ключевое слово ```WorkflowEngineSampleCode``` в сочетании с глобальным поиском по проекту  для нахождения всех нужных изменений.

Для воспроизведения нашего семпла вам нужно сделать следующее:

- Добавить в проект c Application следующие нугет пакеты: WorkflowEngine.NetCore-Core, WorkflowEngine.NetCore-ProviderForMSSQL
- Добавить ```WorkflowRuntimeManager``` вместе с ```ActionProvider``` (расположение Application/Workflow)
- Добавить в DI ```IWorkflowActionProvider``` и ```WorkflowRuntime``` (расположение Web.Host/Startup/Startup.cs)
- Добавить сущность ```Document``` (расположение Core/Documents)
- Настроить контекст EF Core и произвести миграцию для добавления таблицы ```AppDocuments```  (расположение EntityFrameworkCore/YourSolutionNameDbContext.cs)
- Настроить контекст EF Core и произвести миграцию для добавления таблицы ```AppDocuments```  (расположение EntityFrameworkCore/YourSolutionNameDbContext.cs)
- Добавить ```DocumentAppService``` и ```WorkflowSchemeAppService``` вместе со всеми необходимыми Dto (расположение Application/Documents и Application/WorkflowSchemes)
- Добавить ```DesignerController``` (расположение Web.Host/Controllers)

Готово!

## Настройка Angular решения

Все изменения, нужные для фронтенд части приложения, вы можете найти в следующем [коммите](https://github.com/optimajet/WorkflowEngine.NET/commit/ede24d4b02641afa86970c963bc946880fcd8eff). Все изменения в коде выделены комментарием ```WorkflowEngineSampleCode```, так же как и в бекенд части.

Для воспроизведения нашего семпла вам нужно сделать следующее:

- Добавить пакет ```Workflow Designer``` для ангуляра и ```JQuery```

```shell
npm i @optimajet/workflow-designer-angular jquery
```

- Реализовать ```WorkflowSchemeServiceProxy``` и ```DocumentServiceProxy``` и добавить их провайдеры (@shared/service-proxies/)
- Реализовать ангуляровские компоненты для documents и workflow (@app/documents и @app/workflow)
- Добавить ранее реализованные компоненты и модули в ```app.module.ts``` (@app/app.module.ts)
- Добавить endpoints в роутинге  (@app/app-routing.module.ts)
- Добавить разделы меню для новых компонентов (@app/layout/sidebar-menu.component.ts)
- Добавить стили ```Workflow Designer``` в ```core.less``` (@shared/core.less)

Готово!

# Screenshots

#### Workflow Designer

![](_screenshots/workflow.png)

#### Documents

![](_screenshots/documents.png)

# Documentation

* [ASP.NET Core MVC & jQuery version.](https://aspnetboilerplate.com/Pages/Documents/Zero/Startup-Template-Core)
* [ASP.NET Core & Angular  version.](https://aspnetboilerplate.com/Pages/Documents/Zero/Startup-Template-Angular)
* [Workflow Engine](https://workflowengine.io/documentation/)
* [Workflow Designer Angular](https://www.npmjs.com/package/@optimajet/workflow-designer-angular)

# License

[MIT](LICENSE).
