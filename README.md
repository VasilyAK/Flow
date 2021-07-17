# Библиотека Flow <img src="https://github.com/VasilyAK/Flow/workflows/Flow-CI/badge.svg?branch=master"/>

## Введение

Одна из самых распространенных проблем в программировании - это проблема организации кода. Код должен быть написан так, чтобы его было удобно читать и вносить изменения. Однако в процессе разработки далеко не всегда есть возможность наперед предусмотреть какого рода функционал потребуется реализовать и как разместить его в уже существующем коде.

### Цели

- Иметь возможность быстро и удобно встраивать новую логику в уже существующий код
- Контролировать последовательность исполнения кода в удобной форме
- Просто и лаконично писать тесты

### Соглашения о терминах

- Flow: Поток - последовательность выполняемых операций, каждая из которых идентифицируется уникальным кодом

- FlowNode: Нода потока - атомарная структура потока, отвечающая за исполнение назначенной операции.

- FlowMap: Карта потока - набор инструкций, описывающий последовательность выполнения операций и зависимости между нодами потока.

- FlowContext: Контекст потока - объект для передачи команд потоку, а так же для передачи результатов выполнения операций между нодами потока.

### Ссылки на источники

- [Спецификация требований программного обеспечения][ref1]

## Общее описание

### Видение продукта

Flow - это C# библиотека. Внутренняя логика библиотеки основана на теории графов.

### Функциональность продукта

### Документация для пользователей

#### Схема выполнения операций

        -----| Index1 | -----
        |                   |
    | Index2 |          | Index4 |
        |                   |
    | Index3 |          | Index5 |

#### Пример реализации

```c#
    public enum IndexExample
    {
        FirstStep,
        SecondStep,
        ThirdStep,
        FourthStep,
        FifthStep,
    }

    public class FlowContextExample : FlowContext
    {
        public bool IsFirstBranchSelected { get; set; } = false;
        public bool IsSecondBranchSelected { get; set; } = false;
        public int FirstValue { get; set; }
        public int SecondValue { get; set; }
        public int ThirdValue { get; set; }
        public object NonControlResource { get; set; }

        public new void Dispose() => NonControlResource = null;
    }

    public class FlowExample : Flow<FlowContextExample>
    {
        public (string branch, int summ) GetResult() => ProcessContext(RunFlow());

        public async Task<(string branch, int summ)> GetResultAsync() => ProcessContext(await RunFlowAsync());

        protected override void BuildFlowMap()
        {
            flowMap
                .AddRoot(IndexExample.FirstStep, FirstStepAction)
                .AddNext(IndexExample.SecondStep, SecondStepAction)
                .AddNext(IndexExample.ThirdStep, ThirdStepAction);

            flowMap
                .GetNode(IndexExample.FirstStep)
                .AddNext(IndexExample.FourthStep, FourthStepAction)
                .AddNext(IndexExample.FifthStep, FirthStepAction);
        }

        private static void FirstStepAction(FlowContextExample ctx)
        {
            ctx.FirstValue = new Random().Next(10);
            if (ctx.FirstValue < 5)
            {
                ctx.IsFirstBranchSelected = true;
                ctx.SetNext(IndexExample.SecondStep);
            }
            else
            {
                ctx.IsSecondBranchSelected = true;
                ctx.SetNext(IndexExample.FourthStep);
            }
        }

        private static void SecondStepAction(FlowContextExample ctx)
        {
            ctx.SecondValue = 10;
        }

        private static Task ThirdStepAction(FlowContextExample ctx)
        {
            ctx.ThirdValue = 20;
            return Task.CompletedTask;
        }

        private static Task FourthStepAction(FlowContextExample ctx)
        {
            ctx.SecondValue = 100;
            return Task.CompletedTask;
        }

        private static void FirthStepAction(FlowContextExample ctx)
        {
            ctx.ThirdValue = 200;
        }

        private (string branch, int summ) ProcessContext(FlowContextExample context)
        {
            var summ = context.FirstValue + context.SecondValue + context.ThirdValue;
            var branchSelected = context.IsFirstBranchSelected
                ? "First branch"
                : context.IsSecondBranchSelected ? "Second branch" : "No branch";
            return (branchSelected, summ);
        }
```

### Допущения и зависимости

- Удалить, если будет нечего написать

## Функциональность системы

### Программные интерфейсы

``` c#
public abstract class Flow
{
    public virtual void Dispose();

    // Запустить поток на исполнение
    public TFlowContext RunFlow();
    public async Task<TFlowContext> RunFlowAsync();
    
    // Построить карту потока
    protected virtual void BuildFlowMap() { };
}
```

``` c#
public interface IFlowContext
{
    IReadOnlyFlowNode[] CompletedNodes { get ; }
    IReadOnlyFlowNode CurrentFlowNode { get ; }
    IReadOnlyFlowNode NextFlowNode { get ; }
    IReadOnlyFlowNode PreviousFlowNode { get ; }
    
    void Dispose();

    // Назначить следующую исполняемую ноду
    void SetNext(string flowNodeIndex);
    void SetNext<TIndex>(TIndex flowNodeIndex) where TIndex : struct;
}
```

``` c#
public interface IFlowMap<TFlowContext> where TFlowContext : IFlowContext
{
    bool IsValid { get; }
    FlowMapValidationError[] ValidationErrors { get; }

    // Добавить в карту корневую ноду
    IFlowNode<TFlowContext> AddRoot(string flowNodeIndex);
    IFlowNode<TFlowContext> AddRoot<TIndex>(TIndex flowNodeIndex) where TIndex : struct;

    // Добавить в карту корневую ноду
    IFlowNode<TFlowContext> AddRoot(string flowNodeIndex, Action<TFlowContext> flowNodeAction);
    IFlowNode<TFlowContext> AddRoot<TIndex>(TIndex flowNodeIndex, Action<TFlowContext> flowNodeAction) where TIndex : struct;

    // Добавить в карту корневую ноду
    IFlowNode<TFlowContext> AddRoot(string flowNodeIndex, Func<TFlowContext, Task> flowNodeAction);
    IFlowNode<TFlowContext> AddRoot<TIndex>(TIndex flowNodeIndex, Func<TFlowContext, Task> flowNodeAction) where TIndex : struct;

    // Получить ноду по ее ключу
    IFlowNode<TFlowContext> GetNode(string flowNodeIndex);
    IFlowNode<TFlowContext> GetNode<TIndex>(TIndex flowNodeIndex) where TIndex : struct;

    // Получить корневую ноду
    IFlowNode<TFlowContext> GetRoot();
}
```

```c#
public interface IReadOnlyFlowNode
{
    string Index { get; }
    bool HasAction { get; }
    bool IsValid { get; }
    FlowNodeType Type { get; }
}
```

```c#
public interface IFlowNode<TFlowContext> : IReadOnlyFlowNode where TFlowContext : IFlowContext
{
    FlowNodeValidationError[] ValidationErrors { get; }

    // Назначить ноде исполняемое действие
    IFlowNode<TFlowContext> AddAction(Action<TFlowContext> flowNodeAction);

    // Назначить ноде исполняемое действие
    IFlowNode<TFlowContext> AddAction(Func<TFlowContext, Task> flowNodeAction);

    // Добавить ссылку на следующую исполняемую ноду
    IFlowNode<TFlowContext> AddNext(string flowNodeIndex);
    IFlowNode<TFlowContext> AddNext<TIndex>(TIndex flowNodeIndex) where TIndex : struct;

    // Добавить ссылку на следующую исполняемую ноду
    IFlowNode<TFlowContext> AddNext(string flowNodeIndex, Action<TFlowContext> flowNodeAction);
    IFlowNode<TFlowContext> AddNext<TIndex>(TIndex flowNodeIndex, Action<TFlowContext> flowNodeAction) where TIndex : struct;

    // Добавить ссылку на следующую исполняемую ноду
    IFlowNode<TFlowContext> AddNext(string flowNodeIndex, Func<TFlowContext, Task> flowNodeAction);
    IFlowNode<TFlowContext> AddNext<TIndex>(TIndex flowNodeIndex, Func<TFlowContext, Task> flowNodeAction) where TIndex : struct;
}
```

## Нефункциональные требования

### Требования к производительности

- Приложить результаты тестов на производительность

### Требования к качеству программного обеспечения

- FlowNode

  - При запуске на исполнение

    1. Нода должна содержать инструкцию на исполнение, иначе - ошибка.
    2. Нода должна поддерживать возможность исполнения синхронной и асинхронной операции.

  - При инициализации

    1. Корневая нода не должна иметь родительских нод, иначе - нода не валидна.
    2. Нода не должна иметь двух дочерних нод с одинаковыми ключами, иначе - нода не валидна.
    3. Нода не должна иметь циклических ссылок на саму себя (быть родителем n-го порядка для самой.себя), иначе - нода не валидна
    4. Связи между каждыми двумя нодами должны быть одинарными
    5. Нода должна идентифицироваться уникальным строковым ключом
    6. Нода должна содержать отчет о всех ошибках, полученных при инициализации
    7. Сравнение эквивалентности должно быть реализовано через ReferenceEquals

- FlowMap

  - При запуске на исполнение

    1. Карта потока используется только для описания связей между нодами потока и не используется в момент запуска потока на исполнение.
    2. Не должно быть возможности изменять карту потока вне конструктора контекста потока.

  - При инициализации

    1. Карта потока должна содержать единственную корневую ноду.
    2. Все ноды карты потока должны быть валидными.
    3. Карта потока должна предоставлять доступ к любой ноде потока.
    4. Карта потока должна содержать отчет о всех ошибках, полученных при инициализации
    5. Сравнение эквивалентности должно быть реализовано через ReferenceEquals

- FlowContext

  - При запуске на исполнение

    1. Контекст должен предоставить возможность задавать порядок выполнения операций.
    2. Контекст должен предоставлять информацию о процессе исполнения
        - Предыдущая нода
        - Текущая нода
        - Следующая нода
        - Завершенные ноды
    3. Во время асинхронного выполнения ноды, контекст должен быть заблокирован для других потоков, за исключением дочерних.
    4. Контекст должен реализовывать интерфейс System.IDisposable

  - При инициализации

    1. Карта потока должна быть валидной, иначе - ошибка.
    2. По умолчанию на исполнение назначается корневая нода
    
- Flow

  - При запуске на исполнение

    1. Поток должен предоставлять возможность задавать порядок выполнения операций.
    2. Поток должен предоставлять возможность назначить действие на ноду
    3. Поток должен предоставлять возможность запускать поток на исполнение.

  - При инициализации

    1. Карта потока должна быть валидной, иначе - ошибка.
    2. По умолчанию на исполнение назначается корневая нода.
    3. Поток должен реализовывать интерфейс System.IDisposable.

## Прочее

### Приложение А: Глоссарий

### Приложение Б: Модели процессов и предметной области и другие диаграммы

### Приложение В: Список ключевых задач

[ref1]: https://ru.wikipedia.org/wiki/%D0%A1%D0%BF%D0%B5%D1%86%D0%B8%D1%84%D0%B8%D0%BA%D0%B0%D1%86%D0%B8%D1%8F_%D1%82%D1%80%D0%B5%D0%B1%D0%BE%D0%B2%D0%B0%D0%BD%D0%B8%D0%B9_%D0%BF%D1%80%D0%BE%D0%B3%D1%80%D0%B0%D0%BC%D0%BC%D0%BD%D0%BE%D0%B3%D0%BE_%D0%BE%D0%B1%D0%B5%D1%81%D0%BF%D0%B5%D1%87%D0%B5%D0%BD%D0%B8%D1%8F
