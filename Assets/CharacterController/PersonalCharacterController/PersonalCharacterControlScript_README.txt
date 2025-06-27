<<-----Final----->>
<<----------CharacterController#1---------->>
=============================================
FIRST PERSON MOVEMENT in 10 MINUTES - Unity Tutorial (Dave / GameDevelopment)


------OVERALL CONCEPT-------->
==============================
- Uses [RigidBody].
- The controller separates camera and movement logic into two scripts: [PlayerCam] and [PlayerMovement1].
- [PlayerCam] handles first-person camera rotation using mouse input.
- [PlayerMovement1] manages physics-based movement using Rigidbody including sprinting, jumping, slope detection, and gravity control.
- Slope movement is handled with raycasting and projected force to simulate realistic walking on sloped surfaces.
- Gravity is customized to make falling feel faster and less floaty.

[PlayerCam] Script:
===================
------PROS----------------->
- Simple and clear implementation of mouse look.
- Uses Clamp to limit vertical rotation and prevent over-rotation.
- Separates camera rotation and player orientation cleanly.
- Cursor lock improves immersion for first-person view.

------CONS----------------->
- Script is attached separately to the camera, which can clutter the Player hierarchy.
- No smoothing or damping on camera movement; may feel jittery on fast mouse movement.
- No support for inverting Y-axis or customizing via UI.

[PlayerMovement1] Script:
=========================
------PROS----------------->
- Modular design with well-separated functions for input, movement, jumping, and state control.
- Slope detection and handling is implemented.
- Air control with customizable multiplier is a nice touch.
- Gravity adjustment using additional downward force gives more control over feel.
- Has sprinting, jumping cooldown, drag, and slope-based speed limiting.
- MovementState enum improves clarity in managing different states.
- SpeedControl logic ensures the player does not exceed movement speed.

------CONS----------------->
- Gravity override is manually added, which may conflict with Unity physics or other forces.
- `OnSlope()` is called multiple times per frame (performance hit), could be cached.
- Jump logic uses `Invoke`, which can be harder to debug or cancel mid-jump.
- Rigidbody physics with direct AddForce can feel floaty or less responsive without tuning.
- No check for head collisions or ceiling impact after jumping.

<<----------CharacterController#2---------->>
=============================================
How to Create Player Movement in UNITY (Rigidbody & Character Controller) (Rytech)


------OVERALL CONCEPT-------->
==============================
- Both scripts aim to achieve first-person player movement and camera control.
- Movement is based on user input from keyboard (WASD) and mouse (for looking around).
- Supports jumping.
- Mouse movement rotates the player horizontally and adjusts camera pitch vertically.
- Camera is a child of the player and rotates only on the X-axis (pitch).
- Gravity is manually handled in both approaches.

[PlayerMovement2RB] Script:
===================
------PROS----------------->
- Rigidbody provides more realistic physics and external force interaction (e.g., knockback, collisions).
- Easy to apply impulses like jump using AddForce.
- Movement uses velocity directly for cleaner speed control.
- Clean and compact script structure.

------CONS----------------->
- Lacks smoothing/damping for mouse movement and movement transitions.
- Jump check relies on `Physics.CheckSphere` which may not always be reliable (e.g., on steep terrain).
- Doesn't clamp camera rotation vertically, can lead to camera flipping over.
- Doesn't manually control gravity; relies on physics system + quick check.

[PlayerMovement2CC] Script:
===================
------PROS----------------->
- Uses built-in CharacterController which simplifies collision and slope handling.
- Ground check via `controller.isGrounded` is reliable and efficient.
- Movement handled with `controller.Move()` feels smoother and avoids physics glitches.
- No need to manually manage Rigidbody behavior or collisions.

------CONS----------------->
- Double `controller.Move()` calls per frame may create unexpected behavior.
- Custom gravity can become inconsistent if jumpForce and gravity aren’t tuned properly.
- Like RB version, lacks clamping for vertical camera rotation.
- No separation of ground and air movement behavior (e.g., air control).
- No drag or momentum – feels more 'arcade' than physics-based.

<<----------CharacterController#3---------->>
FIRST PERSON MOVEMENT in Unity - FPS Controller (Brackeys)


------OVERALL CONCEPT-------->
==============================
- [PlayerMovement3] handles movement, jumping, and gravity using Unity’s CharacterController.
- [MouseLook] handles first-person camera control using mouse input.
- Player can move, jump, and rotate view smoothly.
- Ground detection is handled by a physics sphere cast around the player’s feet.

[PlayerMovement3] Script:
===================
------PROS----------------->
- Simple and easy to follow structure for beginners.
- Ground detection using `Physics.CheckSphere` is clear and configurable.
- Gravity and jump implemented using realistic physics formulas (based on `v = sqrt(2gh)`).
- Uses CharacterController for smooth movement without physics glitches.
- Commented well with clear explanations of logic.

------CONS----------------->
- No air control mechanics – movement in air behaves the same as on ground.
- Uses `velocity.y = -2f` to stick to ground, which is a workaround, not a clean solution.
- Ground check sphere may give inaccurate results on uneven terrain or slopes.


[MouseLook] Script:
=========================
------PROS----------------->
- Separates camera control from movement (clean and modular).
- Locks and hides the cursor for FPS-style control.
- Uses `Mathf.Clamp` to prevent over-rotation (no head-spin bug).
- Sensitivity is adjustable for user control feel.

------CONS----------------->
- No smoothing or damping on mouse movement (instant, can feel jerky).
- No option for vertical inversion or user-friendly sensitivity config.
- Rotates only X-axis for camera and Y-axis for player, but doesn't handle roll or other view axes.

<<----------CharacterController#4---------->>
FPS controller tutorial in Unity | Part 01 (Ash Dev)


------OVERALL CONCEPT-------->
==============================
 [PlayerMovement4] is a first-person player controller script using Unity’s CharacterController.
- Adds smooth sprinting, camera bobbing (using Cinemachine), jump physics, and gravity.
- Separates input, movement, and camera logic for clarity.
- Uses CinemachineVirtualCamera and Perlin noise for immersive screen shake.
- Allows for adjustable mouse sensitivity, sprint transitions, and movement responsiveness.

[PlayerMovement4] Script:
=========================
------PROS----------------->
- Smooth sprint transition using Lerp gives natural speed change instead of instant shift.
- Clean separation of concerns: input, movement, turning, and camera bobbing are in separate functions.
- Mouse input is handled in a way that avoids gimbal lock via clamping vertical rotation (xRotation).
- Incorporates camera bobbing using Cinemachine’s Perlin noise, which increases immersion during movement.
- Modular and easy to expand with features like crouch or stamina due to organized layout.
- Configurable via Unity Inspector: speeds, jump height, camera settings, sensitivity.
- Physics-based jump using `Mathf.Sqrt(height * gravity * 2)` is realistic and correct.
- Uses virtual camera’s transform for directional movement, giving correct forward motion based on camera orientation.

------CONS----------------->
- `gravity` is serialized with default 0f — can cause division by zero or faulty jumping if not properly set in the Inspector.
- `verticalVelocity = -1f` to "stick" the player to the ground is a bit hacky and may be unstable on moving platforms or slopes.
- Mouse rotation directly modifies virtual camera rotation instead of using a dedicated camera rig—could cause camera desync in complex setups.
- Uses GetButtonDown for jumping which may miss inputs if frames drop (GetKeyDown would be safer in fixed update for physics).
 No fall damage or slope limiting logic.
- No player state management (idle, running, jumping), so can be tricky to link with animations.

<<----------CharacterController#5---------->>
Rigidbody FPS Controller Tutorial #1-4 (Plai)


------OVERALL CONCEPT-------->
==============================
- [PlayerMovement5] is a physics-based player controller using a Rigidbody component.
- Supports walking, sprinting, jumping, and basic slope detection with projected movement.
- [PlayerLook] handles first-person camera look with clamped vertical rotation and smooth turning based on sensitivity.
- Orientation object is used for forward/right movement directions, allowing separation between view and movement.
- Together, these scripts create a responsive, grounded FPS controller suited for parkour-like or physics-reliant gameplay.

[[PlayerLook] Script:
=========================
------PROS----------------->
- Simple and efficient mouse look implementation.
- Separates horizontal and vertical look via yRotation and xRotation.
- Uses `Quaternion.Euler` for smooth camera rotation.
- Orientation transform allows direction-based movement logic (good separation from camera).
- Multiplier allows fine-tuning mouse sensitivity.

------CONS----------------->
- No mouse smoothing or acceleration dampening, may feel too "snappy."
- No vertical rotation limit for camera roll or pitch beyond 90/-90 clamp.
- Doesn't account for time delta in `Input.GetAxisRaw`, can cause issues with inconsistent framerates.
- Cursor lock is one-time; lacks toggle logic for pause menus.
- No support for controller/gamepad input.

[PlayerMovement5] Script:
=========================
------PROS----------------->
- Physically based movement using Rigidbody gives natural behavior.
- Includes sprinting, jumping, and drag adjustment for air/ground.
- Slope detection adds realism and prevents unwanted vertical launches.
- Slope movement uses `Vector3.ProjectOnPlane` for accurate motion.
- Separation of input, movement, speed, drag, and jumping improves readability and modularity.
- Drag control improves physics realism (ground drag vs. air drag).
- Movement direction is based on `orientation`, allowing camera-independent control.

------CONS----------------->
- Constantly recalculating slopeMoveDirection even when not on slope is unnecessary.
- No clamp or max velocity cap — could result in speed buildup if physics act up.
- Using `AddForce` without checking velocity magnitude may lead to jitter or overspeeding.
- MovementMultiplier is hardcoded — better to expose for tweaking.
- No air control limiting — currently allows full directional air control unless refined.
- Rigidbody freezeRotation may cause odd behavior on moving platforms or external physics interactions.
- Doesn’t include animation or state transitions (idle, walk, run).
- No movement smoothing or friction logic, may feel floaty depending on settings.

<<----------CharacterController#6---------->>
Unity 2021 - Rigidbody Player Controller Full Explanation - Part 1-3 (Gigabyte)


------OVERALL CONCEPT-------->
==============================
- [MouseLook] provides simple yet effective first-person camera control with vertical clamping and optional smoothing via `smoothCam`.
- [OrientationHelper] aligns the player orientation to surface normals (like slopes or curved terrain), enhancing grounded navigation and realistic character alignment.
- [PlayerMovement6] is a complex physics-based controller that includes:
  - Ground checking via `SphereCast`
  - Multi-jump handling
  - Walk force/velocity clamping
  - Gravity modifiers (standard and slope-specific)
  - Slope detection with angle clamping
  - Friction forces for responsive stopping
- Combined, this trio forms a grounded FPS controller that can handle various terrains while still feeling responsive and controlled.

[MouseLook] Script:
=========================
------PROS----------------->
+ Clean and minimal structure, easily adjustable with `mouseSens`.
+ Separates horizontal and vertical rotation clearly.
+ Inverted Y-axis implemented manually (common for FPS).
+ Locks cursor for immersive first-person control.
+ Offers external access to horizontal rotation through `GetHorizontalRotation()`.

------CONS----------------->
- Doesn't use clamping for yaw (horizontalRotation) — could lead to overflow over time.
- Camera shake or motion smoothing is not implemented — very raw input feel.
- Uses `transform.eulerAngles` directly, which can sometimes lead to gimbal lock or interpolation issues.
- No separation between camera tilt and body rotation if expanded to third-person or head-bob mechanics.

[OrientationHelper] Script:
=========================
------PROS----------------->
+ Smart use of `transform.up = info.normal` allows slope-based orientation for natural-feeling ground traversal.
+ Separates orientation from camera, enabling more flexibility (e.g., aiming vs. walking direction).
+ Uses `Debug.DrawLine` for visual debugging (good practice).
+ Rotates the orientation to match the player's yaw smoothly using `transform.Rotate`.

------CONS----------------->
- Raycast length (`rayLength`) may need to be tweaked per terrain; otherwise can miss ground.
- No fallback when raycast fails (e.g., falling or jumping might break orientation).
- Rotation logic could be more robust (e.g., using `Quaternion.RotateTowards` for smoother transitions).
- `transform.Rotate` can accumulate imprecision over time — needs damping or constraints.

[PlayerMovement6] Script:
=========================
------PROS----------------->
+ Clean structure separating Update (input) and FixedUpdate (physics).
+ Supports multi-jump through `jumpsLeft`.
+ Excellent slope detection and handling via `SlopeAngle()` and `OnFloor()`.
+ Extra gravity logic helps create tight, responsive feel after jumps/falls.
+ Smart friction logic: slows player only when grounded and idle.
+ Limits horizontal velocity while allowing natural vertical momentum.
+ `FeetPosition()` and `SphereCast` for accurate grounded check based on character height and radius.
+ Debug tools (Gizmos, Debug.DrawLine) for visualization and testing.

------CONS----------------->
- Lacks animation state hooks (e.g., running, jumping) — integration needed.
- Movement input is raw; no smoothing or acceleration curves.
- Jump resets only on ground — potential for abuse in specific edge cases (e.g., ledge drops).
- Code repetition between clamping for grounded and air movement — can be abstracted.
- `walkForce` and `walkSpeed` values can fight each other if mismatched.
- No sprint functionality built-in; could be a nice addition.
- Not beginner-friendly — several systems packed into one script.
