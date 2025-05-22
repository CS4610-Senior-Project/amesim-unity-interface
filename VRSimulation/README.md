# VR Drone Simulation

## Scene Setup
- Drag the scene from "/Scenes/Drone_Setup" to the Project Hierarchy
- Main Camera already has a script that follows the drone from a set height and distance. These properties can be edited.
- The Drone has many properties that can also be edited, more detail later
- THe Level Manager has all the lighting, geographical features and graphical and audio effects
- Geo_GRP has all the terrain, runway, hangar and trees

## Basic Movement Controls
**"W"**: pitch down  
**"S"**: pitch up  
**"A"**: roll left  
**"D"**: roll right  

**"Up arrow &uarr;"**: throttle up  
**"Down arrow &darr;"**: throttle down  
**"Left arrow &larr;"**: turn left  
**"Right arrow &rarr;"**: turn right
Note: For throttle, the plane uses sticky throttle, so you do not have to hold the throttle button upjust hold it till the value reaches 1 and then you can let go  

**"Space"**: Brake 
Note: In the scene, if you click "Drone" and go to the inspector, you can change the default brake key in the Drone_Base_Input Script Component

**"F"**: Increase flap input  
**"G"**: Decrease flap input
Note: Increasing flap will increase the drag significantly and cause your plane to slow down, useful for landing. You can change the flap Drag factor in the inspector for the drone

**"C"**: Switch camera from third person to first person view inside the cockpit


## Concepts of plane physics
- Ground Effect: 
    - Ground effect is the increased lift(force) and decreased aerodynamic drag that an aircraft's wings generate when they are close to a fixed surface.
    - When landing, ground effect can give the pilot feeling that the aircraft is "floating"
    - When taking off, ground effect may temporarily reduce the stall speed. 
        - The pilot can then fly just above the runway while the aircraft accelerates in ground effect until a safe climb speed is reached
    - Script for ground effect is located in the features folder of scripts, called Drone_GroundEffect
## Editing the Drone Properties
- If you click on the Drone in the project hierarchy you can see that it has a graphics group, collision group, center of gravity, engine, and control surfaces.  
- The graphics group contains all the graphical components from the plane asset that I used from IndiePixel Airplanes  
- The Collision group has all the colliders for the plane
- The control surfaces has all the properties of the plane that get transformed when you control the plane. Ex. when you pitch up or down, the elevator component of the plane should be changed graphically to reflect your movement
- The Center of gravity allows you to tell the rigid body where the center of mass is. 
- The engine has a max force, a power curve, a max RPM, a power curve, and is assigned a propellor. These properties are set to a working value but can be edited to users liking

