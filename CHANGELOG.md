## [0.2.10-alpha] - [21/07/26]
### Added Serialisable XorShift random
* Added JSON Serialisable BeltainsTools.Maths.XSRandom class for seeded random game features.

## [0.2.9-alpha] - [13/07/26]
### Some more Hierarchical State Machine love
* Added additional API for streamlined access of ancestors per state
* Added suspend/resume logic to states, to allow some form of interruption
* Fixed transitions sometimes ignoring Activities when transitioning to parent states and not directly to initial substates.

## [0.2.8-alpha] - [10/07/26]
### Fixes surrounding Coroutine Runner, Hierarchical State Machine

## [0.2.7-alpha] - [22/06/26]
### Improvements to debugging Telemetry
* Cleaned up telemetry tracking API to clarify usage
* Added handles for easier and more robust message management and updating

## [0.2.6-alpha] - [20/06/26]
### Fixes to support Unity 6000.5's new stricter serialization requirements
* Added some System.NonSerialized attributes across BEvent usages
* Also some minor tweaks PlayerInteraction events

## [0.2.5-alpha] - [05/06/26]
### Hierarchical State Machines
* Added HSMs under BeltainsTools.StateMachines.HSM namespace
* Added Transition Sequencer to HSMs, along with activities and two sequencing modes
* Moved FSMs under BeltainsTools.StateMachines.FSM namespace

## [0.2.4-alpha] - [31/05/26]
### PlayerInteraction.InputRouter
* Added Input Router component to mimic most of the functionality of Unity's EventSystem + InputSystemUIInputModule components with a more general focus (Unlike EventSystem which is heavily linked with UI). This also allows us to split event input (ie, UI button navigation) from general player interaction input.
* Added IRaycastStrategy and basic Raycast Strategy components to allow us to filter hits.

## [0.2.3-alpha] - [29/05/26]
### Some minor StateMachine tweaks, FlaggedValue
* Changed IState interface to require full interface implementation.
* Exposed events in the StateMachine for switching/entering/exiting states
* Added FlaggedValue for easy in-inspector value flagging 

## [0.2.2-alpha] - [24/05/26]
### VectorInt extensions

## [0.2.1-alpha] - [22/05/26]
### Coroutine Sugar
* Added CoroutineRunner with static calls to allow execution of coroutines with management handles from anywhere in code.
* Added yield instruction for async Tasks
* Improved FrameDelayed and TimeDelayed actions to not require an executor monobehaviour.

## [0.2.0-alpha] - [21/05/26]
### Finite State Machines
* Added code-side finite state machine classes
* Added a TimeDelayedAction alongside existing FrameDelayedAction
* Added EventChannelInvoker component for editor level access to invocation of EventChannels

## [0.1.7-alpha] - [10/05/26]
### Event Channels

## [0.1.6-alpha] - [04/05/26]
### Many minor changes to support project development
* Formalised versioning
* Ported some code from existing projects
* New IO functions
* Serialisation changes to support broader use

## [0.1.5-alpha] - [24/07/25]
### UILineGraph, Improved Editor Experience
* Added new UILineGraph component and game object resource. Addable by a right-click in the heriarchy.
* Added more editor utilities for adding resources from a path into the scene in editor and play mode.
* Hooked new menu items for the UILineRenderer, UIGridRenderer, and UILineGraph

## [0.1.4-alpha] - [19/07/25]
### Improvements to UILineRenderer Access
* Added some accessors and a new editor for the UILineRenderer
* Added some more Utilities relating to the Rect structure
* Fixed an issue with the Vector3 RelativeToCameraForward extension that would cause oscillation when used

## [0.1.3-alpha] - [17/07/25]
### Simple UILineRenderer
* Added new UILineRenderer MaskableGraphic. At the moment this is a simple UI implementation of something like Unity's LineRenderer. Some basic optimisation in there, but will probably revisit to improve further.

## [0.1.2-alpha] - [11/07/25]
### More Housekeeping
* Organised more utilities into their own utilities scripts
* Subdivided the extensions.cs script into categorised extensions scripts
* Ensured that extensions scripts handle as little logic as possible, passed to utilities scripts

## [0.1.0-alpha] - [10/07/25]
### Housekeeping
* Tidying of namespaces mainly focussing on the Utilities