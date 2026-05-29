# Radiopeaks.HSM

A lightweight and extensible **Hierarchical State Machine (HSM)** framework for Unity using C# generics.

This system supports:

* Hierarchical states
* State caching
* Nested substates
* Context-driven architecture
* Unity lifecycle integration (`Update`, `FixedUpdate`, `LateUpdate`)
* Clean and reusable state transitions

---

# Features

* ✅ Generic context support
* ✅ Hierarchical state architecture
* ✅ Parent/Substate relationships
* ✅ Cached state instances
* ✅ Unity-friendly lifecycle methods
* ✅ Easy state transitions
* ✅ Lightweight and allocation-free during runtime transitions

---

# Architecture Overview

The system is composed of three main parts:

## `IState<TContext>`

Defines the contract for all states.

```csharp
public interface IState<TContext>
{
    void Enter();
    void Exit();
    void Update();
    void FixedUpdate();
    void LateUpdate();

    void Initialize(
        TContext context,
        StateMachine<TContext> machine,
        IState<TContext> superState
    );
}
```

Every state can:

* Enter and exit
* Receive Unity update callbacks
* Access the shared context
* Know its parent state (`SuperState`)

---

## `State<TContext>`

Base abstract implementation for all states.

Provides:

* Access to the shared context
* State machine reference
* Substate management
* State transitions
* Hierarchy handling

```csharp
public abstract class State<TContext> : IState<TContext>
```

### Important Methods

#### Switch to another state

```csharp
SwitchState<NewState>();
```

If the current state has a parent state, the transition happens inside the hierarchy.

Otherwise, it becomes the new root state.

---

#### Set a substate

```csharp
SetSubState<MovementState>();
```

This creates a hierarchical relationship:

```text
PlayerState
 └── MovementState
```

---

## `StateMachine<TContext>`

Controls the active state hierarchy.

```csharp
var machine = new StateMachine<PlayerContext>(context);
```

Responsibilities:

* Starting states
* Updating hierarchy
* Managing transitions
* Caching state instances
* Rebuilding active hierarchy

---

# How Hierarchical Updates Work

The state machine stores the active hierarchy internally:

```text
RootState
 └── ChildState
      └── SubChildState
```

During updates:

```csharp
machine.Update();
```

The machine executes updates from top to bottom:

```text
RootState.Update()
ChildState.Update()
SubChildState.Update()
```

If a transition happens during execution, the hierarchy reloads safely and stops the current iteration.

---

# Basic Example

## Context

```csharp
public class PlayerContext
{
    public Rigidbody Rigidbody;
    public Animator Animator;
    public bool IsGrounded;
}
```

---

## Create a Root State

```csharp
public class PlayerState : State<PlayerContext>
{
    public override void Enter()
    {
        SetSubState<IdleState>();
    }
}
```

---

## Idle State

```csharp
public class IdleState : State<PlayerContext>
{
    public override void Enter()
    {
        Context.Animator.Play("Idle");
    }

    public override void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            SwitchState<MoveState>();
        }
    }
}
```

---

## Move State

```csharp
public class MoveState : State<PlayerContext>
{
    public override void Enter()
    {
        Context.Animator.Play("Run");
    }

    public override void Update()
    {
        if (!Input.GetKey(KeyCode.W))
        {
            SwitchState<IdleState>();
        }
    }
}
```

---

## Using the State Machine

```csharp
public class PlayerController : MonoBehaviour
{
    private StateMachine<PlayerContext> _machine;

    private void Awake()
    {
        var context = new PlayerContext
        {
            Rigidbody = GetComponent<Rigidbody>(),
            Animator = GetComponent<Animator>()
        };

        _machine = new StateMachine<PlayerContext>(context);
        _machine.Start<PlayerState>();
    }

    private void Update()
    {
        _machine.Update();
    }

    private void FixedUpdate()
    {
        _machine.FixedUpdate();
    }

    private void LateUpdate()
    {
        _machine.LateUpdate();
    }
}
```

---

# Hierarchical State Example

One of the main strengths of this system is nested states.

Example hierarchy:

```text
PlayerState
 └── GroundedState
      ├── IdleState
      └── MoveState
```

## Grounded State

```csharp
public class GroundedState : State<PlayerContext>
{
    public override void Enter()
    {
        SetSubState<IdleState>();
    }

    public override void Update()
    {
        if (!Context.IsGrounded)
        {
            SwitchState<AirState>();
        }
    }
}
```

Now `IdleState` and `MoveState` live under `GroundedState`.

This allows:

* Shared grounded logic
* Cleaner transitions
* Better separation of responsibilities

---

# State Caching

States are automatically cached:

```csharp
GetOrCreateState<T>()
```

This means:

* States are instantiated only once
* No garbage allocations during transitions
* Better runtime performance

---

# Debugging Hierarchy

Inside the Unity Editor:

```csharp
Debug.Log(machine.HierarchyString());
```

Example output:

```text
 -> PlayerState
       -> GroundedState
              -> MoveState
```

---

# Why Use This System?

This HSM architecture is useful for:

* Character controllers
* Enemy AI
* Gameplay flow
* Menus
* Combat systems
* Ability systems
* Animation logic

Compared to traditional FSMs, hierarchical state machines reduce duplication and improve scalability.

---

# Advantages Over Traditional FSM

| Traditional FSM     | Hierarchical FSM       |
| ------------------- | ---------------------- |
| Flat structure      | Nested structure       |
| Duplicated logic    | Shared parent behavior |
| Hard to scale       | Easy to extend         |
| Complex transitions | Cleaner transitions    |

---

# License

MIT License

Feel free to use, modify, and distribute.
