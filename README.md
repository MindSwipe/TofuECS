![tofuecs_logo](https://user-images.githubusercontent.com/8916588/139094266-3e2db942-4842-4f0d-b1da-8e694ee3578c.png)

This is an entity component system (ECS) framework written in C# that can be easily added to a Unity project as a managed plugin (dll) — although I suppose there's no reason you couldn't use it for some other non-Unity purpose. Licensed under MIT.

This repo contains a solution with three projects: TofuECS, TofuECS.Utilities, and TofuECS.Tests. TofuECS is the main dll you will want to include in your project, while TofuECS.Utilities contains some other useful classes that aren't strictly necessary. TofuECS.Tests should not be included, but showcases how to start and run a simulation and how you can use this framework.

ECS frameworks are fun to code in and offer performance benefits against the typical GameObject/MonoBehaviour Unity workflow, all while presenting a clear separation of logic from views (for example: your GameObjects, Meshes, Sprites, etc.). They solve a problem that is very common in game development: messy class hierarchies that make it difficult to share code between two unrelated classes. Essentially, an ECS is a data structure containing the entire state of your game (or simulation) at every moment, with rules on how to alter that data over time.

## Entities
There is no "Entity" class in TofuECS. They're just integers. Literally, they are keys for dictionaries when looking for component indexes in `ComponentBuffers`. There is no extra data associated with them whatsoever. The integer `3` can be a key that points to multiple components, and that is how you can associate components together. `CreateEntity()` just ticks up and returns an integer value, and is simply useful to ensure the same number is not used twice.

## Components
Components contain the state of the `Simulation`. They are stored in a managed array and accessed via the `Simulation`. Currently, components must be `unmananaged` structs (see [MS docs for the Unmanaged type constraint](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-7.3/blittable)), i.e, structs with only fields of types `int`, `bool`, etc. This does require some creativity on the part of the developer in order to inject data from Unity (or some other engine) into the sim, as common types like `string` or managed arrays are not allowed.

[Use the `fixed` keyword in your component structs when arrays are necessary](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/unsafe-code#fixed-size-buffers). A caveat here is that there is a limit to how big your structs can be, which is a limitation of the mono runtime. For example, a component containing a fixed bool array of more than 1,000,000 elements is sketchy. Additionally, fixed buffers may not be resized at runtime.

## Systems
Systems are ***stateless***  classes that implement `ISystem` and are passed into the constructor of a `Simulation` instance. They are initialized once when the sim is initialized, and processed once each time `Tick()` is called on the sim. This is where all logic for your ECS should exist (*it is possible to put logic in functions on your components, but that seems messy in my opinion*).

It is extremely important to remember the term ***stateless***. While there's nothing stopping you from adding fields (state) to an implementation of `ISystem`, doing so will likely lead to inconsistent results when re-simulating frames during a rollback or replay, and goes against the spirit of an ECS. *Remember to store all data in components*.

All functions in an `ISystem` implementation, besides the required ones (`Initialize` and `Process`), ought to be `static` ([see the docs](https://docs.microsoft.com/en-us/previous-versions/visualstudio/visual-studio-2015/code-quality/ca1822-mark-members-as-static?view=vs-2015&redirectedfrom=MSDN)). This gives minor performance improvements and also reminds the developer to keep the class ***stateless***. 

# Other Notes...

This ECS is about as bare-bones as it could be. It is intended to be compatible with multi-threaded applications, physics engines, and rollback engines. When using Unity, I recommend putting your ECS code inside an assembly definition that does not allow engine references (and allows unsafe code). The ECS ought to be Unity-agnostic, and because of the type constraints on your components, there's not any use for MonoBehaviours and GameObjects in your system logic anyway. 


The utilities included in `TofuECS.Utilities` are simply there because I thought they'd be helpful for game developers:
- `ArrayQuickSort`: An implementation of QuickSort that can be used for arrays.
- `XorShiftRandom`: A very-lightly modified implementation of a super-fast RNG. It can, for example, be used as a singleton component when pseudo-RNG is necessary.

`ILogService` is a boarder-line utility that is there to pass logs from the simulation to whatever your implementation of it might be. I thought it would be easier to just write `s.Debug("wtf why is this happening????");`.

- Q: *"How do I inject configuration data into the Simulation?"*  A: Use `Simulation.RegisterSingletonComponent<T>(T myConfigData)` and from there you'll probably want to just access it via `s.GetSingletonComponent<T>(out T myConfigData);` in the `Initialize` method of one of your `ISystem` implementations.


- Q: *"How do I respond to state changes inside the Simulation (in Unity, for example)?"* A: Raise a regular C# `event` inside of an `ISystem` instance. You might want to consider queuing data and processing it after the simulation finishes the tick, since the state could still change if the view is updated mid-tick. Just a suggestion.


- Q: *"What does the update loop look like for the Simulation?"* A:
    ```
    private void Update()
    {
        _simulation.SystemEvent<MyInput>(_myInput); // not necessary if no input exists
        _simulation.Tick();
    }
    ```
    Note here that system events are useful both for processing simulation input and for communicating between systems. Every instance of `ISystemEventListener<MyInput>` in your systems array will immediately receive a callback that allows you to respond to the data.


- Q: *"How do I rollback my simulation to a previous state?"* A: Use `GetState<TComponent>` for tracking your state and `SetState<TComponent>` when going back in time to some other state. Look at `RollbackTest()` in TofuECS.Tests.  


- Q: *"I keep getting `BufferFullException` when running my simulation, how do I resize my buffer when I run out of space?"* A: You don't, sorry. Increase the size of your buffer when registering the component or consider some fundamental change to the structure of your `Simulation`.

*TofuECS is in development! Vegan friendly.*
