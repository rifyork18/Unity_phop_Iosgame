Thank you for support our asset!

*IMPORTANT* This asset requires Unity 5.2.0 or higher.

If you have any question about how it works or if you are experiencing any trouble, 
feel free to email us at: inv3ctor@gmail.com
Please do not Upload or share this asset as a Package without permission.

If you downloaded this asset illegally for studies or prototype purposes, 
please reconsider purchase if you want to publish your work, you can buy on the AssetStore 
or send us a email and we can figure something out, you can even post your work on our Forum, 
will be happy to help and advertise your game.

ASSETSTORE: https://www.assetstore.unity3d.com/en/#!/content/44227
FORUM: http://forum.unity3d.com/threads/third-person-controller-template-by-invector.349124/
YOUTUBE: https://www.youtube.com/channel/UCSEoY03WFn7D0m1uMi6DxZQ
WEBSITE: http://inv3ctor.wix.com/invector

Invector Team - 2015

Changelog v1.3d MELEE COMBAT HOTFIX 16/06/2016

Fixed:
- GameController Spawning method getting errors or not spawning the Player prefab
- ThirdPersonCamera spinning around the target if too close while in LockOn 
- Lock the player extra movement when attacking on strafing or free movement
- Layer 'Player' not auto-assigning on the Character childrens, making the character auto-crouch when add a Ragdoll
- Wrong stamina consuption at higher FPS 
- Nullref error when creating a Defense Weapon 

Changes:
- CameraInput method being call on the FixedUpdate instead of LateUpdate, much smoothier behaviour

Add:
- TurnOnSpot animations when strafing 
- AllowMovementAt variable add on the vMeleeAttackBehaviour, allow the player to rotate during the attacking, more smooth combat gameplay

-----------------------------------------------------------------------------------------------------

Changelog v1.3c MELEE COMBAT SMALL UPDATE 30/05/2016

Fixed:
- LockOn Script now automatically set up the Layer Default for Obstacles
- Error when trying to manually add a TPCamera into a Camera
- Wrong stamina consuption in some cases when rolling, jumping and sprint

Changes:
- Player can now do a quickturn while blocking 
- Hitbox collision improved
- Layers fields removed from the vCharacter and add into the vMotor of the Player & AI
- Controller, Animator & Motor scripts revised with new commends and regions for both Player & AI, easier to use and modify

Add:
- Damage Multiplayer option on the Melee Attack Behaviour (Animation State)
- Draggable Boxes 
- Dynamic Trigger Action to know if there is a obstacle above or ahead
- Puzzle Boxes Scene showing the new feature

-----------------------------------------------------------------------------------------------------

Changelog v1.3b MELEE COMBAT SMALL UPDATE 15/05/2016

Fixed:
- AI Min Detect Distance not working
- AI Min/Max Time to Attack not working
- Lock-on not working when the Players Locomotion Type is set to StrafeOnly
- Character not droping weapons when dying by ragdoll
- HitRecoil & HitReaction cancels magic attacks

Changes:
- Overhall performance improvements 
- Change enemies model for the mobile version
- Sprint unlock for strafe locomotion, just add the animations into the blendtree
- AI detect Magic Attacks Attacker and change the state to Chase

-----------------------------------------------------------------------------------------------------

Changelog v1.3a MELEE COMBAT SMALL UPDATE 11/05/2016

Fixed:
- SphereSensors being created when select the AI prefab
- Lock-on Disable on the V-bot prefab

Changes:
- vDamage has more options
- Update beat'n up scene
- Huge AI improvements on Performance 

Add:
- Option to disable turnOnSpot & quickStop animations
- MagicWeapons that instantiate particles
- New weapon magic attack animation

-----------------------------------------------------------------------------------------------------

Changelog v1.3 MELEE COMBAT BIG UPDATE 04/05/2016

Fixed: 
- Footstep not working on Unity Terrain

Changes:
- HitRecoil & HitReaction improvements, now AttackStates can trigger a specific hit ID to match animations
- Action detection improved on the AI, now you can select what action the AI can do with the NavMesh Agent AreaMask
- All scripts have the prefix 'v' to avoid conflict with other scripts
- AI Distance to Attack is set up by the weapon now 
- Several changes/improvements on the main structure of the Controller/Motor/vCharacter

Add:
- Add Animator Layers with AvatarMasks (left arm, right arm, upperbody, fullbody)
- Hand to Hand Combat 
- Two hand moveset option add (drop the current left melee weapon)
- MeleeWeapon now can be Attack, Defense or Both with option to trigger a Defense animation with a Sword
- Add HitboxFrom on the MeleeAttackBehaviour, you can now select from where your attack is comming > LeftArm, RightLeg, BothLegs, etc
- Automatically create Hitbox for hand to hand combat to Humanoid Characters
- HitDamage Particle component to instantiate particles when take damage
- MeleeWeapons can now instantiate a Recoil Particle when hit a wall
- Audio Slots for attacks, defense and recoil on the MeleeWeapon
- TriggerAudio by Animation State
- AI agressive bool option, the AI will chase at first sight or stay passive until you attack him
- SphereSensor on the AI, now you can choose what targets the AI can persue and the max distance to loose the target
- Who the AI can see and attack are separated, ex: Now you can have a Boss that apply damage to his minions but only chase the Player
- Action Layer for action triggers, automatically set up when create a new character
- Enemy VS Enemy 
- Companion AI with basic commands (follow, attack, stay, go there)
- Tags & Layers are automatically add into the project 
- Health Item example to recover health
- New options for HeadTrack
- Option to mirror animation for Defense 
- Add Punching Bag as a example to use the vCharacter with non-Invector Third Person Characters
- Add new Beat'n Up Demo Scene (thanks supneo for the suggestion)

-----------------------------------------------------------------------------------------------------

Changelog v1.2b MELEE COMBAT SMALL UPDATE 02/04/2016

Fixed:
- Nullreference error when start a game with a AI without the MeleeManager Component
- Improve AI behaviour when not using the MeleeManager Component
- Errors in the Mobile demo scene about the footstep (old footstep components attached needs to be removed)
- Fixed hitReaction ID on the EnemyAI

Changes:
- Fixed mobile demo scene errors and update gameobjects
- Add tag "Weapon" into the List Ignore Tags at the Ragdoll component
- Animator State OneHandedSword name changed to BasicAttack

Add:
- Mobile material for weapons

-----------------------------------------------------------------------------------------------------

Changelog v1.2a MELEE COMBAT UPDATE 30/03/2016

Fixed: 
- Stop Move method on the Character Controller 
- AI fix Max Attack Count and improve Random Attack
- Fixed ChangeScenes buttons on Unity 5.3 or higher
- Fixed Pick up weapons on Mobile

Changes: 
- AI Strafe improved
- AI Improve performance and behaviour on actions
- Defense Range improved on the Shield 

Add:
- Customizable Hitbox for Attack Weapons
- AI Strafe options 
- Default Layers set up automatically when the character is created
- AutoCrouch layer is now separeted from the Ground Layer
- NEW FootStep System, new method that instantiate audiosources individually and include footstep mark and particles 

-----------------------------------------------------------------------------------------------------

Changelog v1.2 MELEE COMBAT UPDATE 15/03/2016

Fixed: 
- Character does not go down the stairs
- Mobile Input with delay to move
- Slope limit not working without stopmove layer
- Fixed alerts about loadlevel on Unity 5.3 or higher

Changes: 
- Major changes on most of scripts 
- Custom editor window with warnings if you don't set up the Layers
- Character Creator window now can create a Character Controller or a AI
- Ragdoll improved with option to die and leave the body, or turn off the rigidbody physics
- Slope limit now make the character slide down the ramp

Add:
- Advanded Enemy AI with Melee Combat and Actions
- Advanced Waypoint System and patrol points
- Advanced Weapon Attack Hitbox with visual assistence
- Collectable Weapons with pick-up/drop option
- Weapon Manager component with easy Handler creator to assist the set up of new weapons
- Weapon Creator window with option to Attack or Defense
- Health Recovery for the Controller and AI
- Optional Health bar HUD for the AI 
- Stamina consumption by action 
- Action Controller add, you can choose what action your character can make and what input will trigger
- Lock-On target system with switch between target
- GameController with Respawn system, timer to respawn the character or scene and option to destroy the body
- Die animation included with option to turn the Ragdoll on at the end of the animation
- HitReaction & HitRecoil with 3 levels of damage
- Menu window with Helpers like link to Tutorials, FAQ, Release Notes and Updates.

-----------------------------------------------------------------------------------------------------

Changelog v1.1d HOTFIX 17/01/2016
Fixed:
- Character Template missing Animator
 
-----------------------------------------------------------------------------------------------------
 
Changelog v1.1c 12/01/2016
 
Fixed:
- 2 Actions been activate at the same time, causing the Player to glitch between states
 
Changes:
- QuickStop and QuickTurn now are actions and are located in the Action State on the Animator
- Grounded State remade to handle Strafe animations
- Minor changes at the TPMotor, TPAnimator and TPController to prepare the AI on the next update
- Better transition from Strafe to Free locomotion, and the a smoother Quickturn
- ICharacter Interface created to handle the Ragdoll behaviour for both Player and AI
- Improve the verification for quickStop and quickTurn Add:
- Locomotion Type, now you can choose between Strafe Only, Free Only or Free with Strafe
- Crouch Strafe locomotion
- New Layer for Melee 3 Attack Combo, this layers comes without animations at this version,
but we will add some example animations on the next version,
along with the AI
 
-----------------------------------------------------------------------------------------------------

Changelog v1.1b 30/12/2015

Fixed:
- Message displaying change of input between mobile/pc when clicking 
- Fixed the error about bounds on the FootStep when walking between Terrain and Mesh 

Changes:
- TPCamera & HUD isoleted from the controller, now this components are modular and work individually
- Controller now works as a Prefab, can be Instantiated and will automatically find a Camera or the HUD component on the scene
- Culling fade now goes into the character instead of the Camera, also modular just like the Ragdoll of Footstep component.
- Strafe animations re-configured with better transitions
- Several TPCamera improvements of CameraState transitions and Culling 
- Improvements and changes into the TPAnimator and TPMotor, now we separated on methods that controls animations and locomotion behaviour from each other

Add: 
- GameController with SpawnPoint system 
- Non Root-motion movement add, with separated Directional Movement Speed and Strafing Speed
- Rotate by World bool add into the Controller for better locomotion when playing on Isometric Mode
- List of Ignore Tags for the Ragdoll Component, keep Weapons or Acessories that are children of the Player with the correctly rotation
- New Camera Feature - Fixed Point or Multiple Points (Resident Evil oldschool camera's style)
- Trigger System to change CameraPoints
- New Demo Scene with the V Mansion showing the new CameraMode Fixed Point
- Simple Door example

-----------------------------------------------------------------------------------------------------

Changelog HOTFIX v1.1a 10/11/2015

Fix Bugs:
- On the MobileScene the gameObject MobileControls need to be below the HUD gameObject on the Hierarchy (otherwise it will stop respond after the ragdoll turn on)
- Fix the Jump behaviour if you jump right on the edge, the character falls and jump again (mecanim issue)
- Fix the trajectory of the jump in case of high value of height and force forward 
- v1.1 shipped with a aditional XInput plugin, we removed and now you can export Mobile builds normally

Add: 
- The character now can jump while Aiming (just a condition add in the Animator)
- Add 'quick change scene' buttons on the demo scenes
- Add a Camera Fade effect to hide the character when is too close
- Add Clip Plane Margin for better clipping planes to the Camera
- Add InvectorJoystick.cs is just a quickfix to the Square touch area from the CrossPlataformInput to a Circular touch area
- Add the original Standard Assets CrossPlataformInput to improve compatibility with other assets and avoid errors of scripts duplicity
- Add back the AntiAliasing in the Camera

-----------------------------------------------------------------------------------------------------

Changelog v1.1 25/10/2015

Fix Bugs:
- Fix the vibrating upperbody animation when in Aiming mode after the ragdoll was activated (bug found by Chrisb3D, thanks!)
- Fix lockPlayer after roll through the Crouch Area and rarely lock the player on the exit (bug found by tmmandk, thank you sir!)
- Fix minor bug holding shift and enter on Aim mode still drains stamina (reported by Steel Grin, thanks!)

Changes:
- Script "FindTarget" changed to "TriggerAction" and add AutoActions bool
- Major changes on TPCamera script
- Update XInput plugins to the last version with x86 and x86_x64 support
- change CheckForwardAction() and create a CheckActionObject on ThirdPersonMotor
- remove CheckAutoCrouch from the ThirdPersonController and put on ThirdPersonMotor
- improved Culling of the camera 
- improved Ragdoll transition when back to player 
- improved footstep system syncronization
- improved Ground Detection
- now users need to set up layers manually to improve compatibility with others projects

Add:
- Add float spine curvature to set up how much the spine will curve while on Aiming mode
- Add support to realtime change input to Mobile (thanks to Xander Davis)
- Add support to MFi iOS gamepad (thanks to Xander Davis)
- Add Aiming button on Mobile Touch Controls
- Add AutoActions for TriggerActions automatically do an Action without the need of input
- Add Tag for AutoCrouch (use on a simple trigger object)
- Add FootStep support to Play a specific material on objects with multiple materials
- Add feature Jump 
- Add feature Camera Scroll Zoom (Mouse only)
- Add feature FixedAngle on CameraState
- Add DrawGizmos for Debug to verify the Raycasts on Motor (head, actions, stepup, stopmove)
- Add 2.5D Scene Demo
- Add Topdown Scene Demo
- Add Isometric Point&Click Scene Demo


