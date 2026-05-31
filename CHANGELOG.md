## [0.2.4-alpha] - [31/05/26]
### Added PlayerInteraction.InputRouter
* Added Input Router component to mimic most of the functionality of Unity's EventSystem + InputSystemUIInputModule components with a more general focus (Unlike EventSystem which is heavily linked with UI). This also allows us to split event input (ie, UI button navigation) from general player interaction input.
* Added IRaycastStrategy and basic Raycast Strategy components to allow us to filter hits.

## [0.2.3-alpha] - [29/05/26]
### Some minor StateMachine tweaks, added FlaggedValue
* Changed IState interface to require full interface implementation.
* Exposed events in the StateMachine for switching/entering/exiting states
* Added FlaggedValue for easy in-inspector value flagging 

## [0.2.2-alpha] - [24/05/26]
### Added VectorInt extensions

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
### Added Event Channels

## [0.1.6-alpha] - [04/05/26]
### Many minor changes to support project development
* Formalised versioning
* Ported some code from existing projects
* New IO functions
* Serialisation changes to support broader use

## [0.1.5-alpha] - [24/07/25]
### Added UILineGraph, Improved Editor Experience
* Added new UILineGraph component and game object resource. Addable by a right-click in the heriarchy.
* Added more editor utilities for adding resources from a path into the scene in editor and play mode.
* Hooked new menu items for the UILineRenderer, UIGridRenderer, and UILineGraph

## [0.1.4-alpha] - [19/07/25]
### Improvements to UILineRenderer Access
* Added some accessors and a new editor for the UILineRenderer
* Added some more Utilities relating to the Rect structure
* Fixed an issue with the Vector3 RelativeToCameraForward extension that would cause oscillation when used

## [0.1.3-alpha] - [17/07/25]
### Added Simple UILineRenderer
* Added new UILineRenderer MaskableGraphic. At the moment this is a simple UI implementation of something like Unity's LineRenderer. Some basic optimisation in there, but will probably revisit to improve further.

## [0.1.2-alpha] - [11/07/25]
### More Housekeeping
* Organised more utilities into their own utilities scripts
* Subdivided the extensions.cs script into categorised extensions scripts
* Ensured that extensions scripts handle as little logic as possible, passed to utilities scripts

## [0.1.0-alpha] - [10/07/25]
### Housekeeping
* Tidying of namespaces mainly focussing on the Utilities