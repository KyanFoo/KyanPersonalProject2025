<<--- Development Notes for [CameraController_README] --->>
-----------------------------------------------------------

---------------------------------------------------------------------------
Date: 21/10/2025
---------------------------------------------------------------------------
Overview:
---------
- Serves as the base for [CameraController] in [PersonalCharacterControl].

- Combining reference from [CharacterControls] into one unified prototype aligned with my design goals.

- [Problems] will occur and [Solution] will arise to further develop the prototype script.

===========================================================
[CameraController1]_Script
===========================================================
Reference From:
---------------
1. [CharacterControl1]_Script
2. [PersonalEdit]_ChatGPT

Overview:
---------
- The hierarchy of the [Player] reference from [CharacterControl1].
	- Following the Gameobjects from [Player], [CameraHolder], [PlayerObj and etc.

Player (Rigidbody, MovementScript)
 └─ PlayerObj (Mesh/Visuals, CapsuleCollider)
 └─ CameraPos (Virtual Camera, Follow)
 └─ Orientation
CameraHolder (Rigidbody, position 0,0,0, no rotation)
 └─ Camera (Main Camera)
 └─ VirtualCamera (Cinemachine Virtual Camera)

- Establishes the baseline structure for player-camera relationships.

Problem Errors:
---------------
Issue Observed (X):
- When applying [CameraMovement], the Player's [Transform.Position] flickers.
	- This occurs in most reference [CharacterControl] implementations.

===========================================================
[CameraController2]_Script
===========================================================
[PersonalEdit]:
---------------
Solution (O):
- Use a parent GameObject structure.
	- Keep the Rigidbody parent object fixed at (0,0,0) with no rotation (handles physics).
 	- Create a child GameObject (mesh/visuals) that handles rotation for visuals and camera alignment.

Overview:
---------
- I going towards the idea of having multiple Scripts for [PersonalCharacterControl].
	- Seperating [CameraController]_Function and [InputManagement()]_Function.
	- Putting everything into one script will be very long.

Reference From:
---------------
1. [CharacterControl1]_Script -> [PlayerCam]_Script  
2. [CharacterControl2]_Script -> [PlayerController]_Script -> [Turn()]_Function  
3. [PersonalEdit]_ChatGPT

From [CharacterControl1]:
-------------------------
- Gather [Mouse Input] using [GetAxisRaw].
	- Returns the exact raw input immediately (-1, 0, or 1). No smoothing -> instant response. [GetAxisRaw]
	- Returns a smoothed value between -1 and 1. Creates a gradual acceleration/deceleration effect. [GetAxis]

- Calculate [Rotation] values for [xRotation] and [yRotation]. 
	- Clamp [xRotation] to simulate the natural head rotation limit of a human.

From [CharacterControl2]:
-------------------------
- Compatible with [Cinemachine]'s [VirtualCamera].  
	- Intended for use with [Third-Person-Perspective] Camera in [PersonalCharacterControl].  
	- Benefits from using [VirtualCamera]'s built-in camera controls within the [Inspector].
	- Reducing the need to build a fully custom camera system.
	  
From [PersonalEdit]:
--------------------
- Rotate both [PlayerMesh] and [Camera] using [xRotation] and [yRotation].
	- To ensure synchronized and consistent facing direction.

- Rotate the variable's [Transform.Rotation] rather than the entire Player.  
	- Use [playerBody.rotation] instead of [transform.rotation].  
	- Prevents unwanted rotation of the Player when applying [yRotation].

Problem Errors:
---------------
Issue Observed (X):
- When [CameraMovement] is applied, the Player’s [Transform.Position] correctly stays at (0,0,0). 
 
- However, when spawning the Player above floor level:
	- [Transform.Position] is not numerically (0,0,0), despite appearing correctly aligned.  
	- This causes the Player to appear "below zero" even when visually positioned on the floor.

===========================================================
[CameraController3]_Script
===========================================================
[PersonalEdit]:
---------------
Solution (O):
- Create [ResetPlayerState()]_Function
	- Pressing [SpaceKey] resets the Player’s [Transform.Position] to (0,0,0) after spawning from a height.

- Forcibly resetting the Player’s [Transform.Position] to (0,0,0) is generally unsafe.

- As directly modifying the transform can cause flickering.
	- However, it is acceptable when executed before gameplay begins (e.g., during initialization or spawn setup).

Problem Errors:
---------------
Issue Observed (X):
- This method works effectively for prototype testing.

- However, using a [Key Press] trigger (e.g., [SpaceKey]) for reset functionality is not suitable for final gameplay implementation.

- Resetting the [Transform.Position] should be handled cleanly and occur without interfering with gameplay flow.

===========================================================
[CameraController4]_Script
===========================================================
[PersonalEdit]:
---------------
Solution (O):
- Implement a proper Spawn System
	- Introduce a variable [Vector3 spawnPosition] to represent the Player’s designated spawn point.
	- This allows controlled spawning at various locations, such as the center of a plane, a mountain top, or a building’s upper floor.

From [Problem Errors]
---------------------
- The automatic reset of the Player’s [Transform.Position] has not yet been implemented.  
- The necessary functions are already in place; they only need to be properly linked to complete the system.