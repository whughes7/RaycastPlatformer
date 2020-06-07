# RaycastPlatformer

### Switch between different parts of Sebastian Lague's tutorial by switching branches
## Git Branch/Checkout Example
```
git checkout 01_Setup_Raycast_Basics
```
### Available branches
- 01_Setup_Raycast_Basics  
- 02_Collision_Detection
- 03_Jump_Physics_Euler_Delta_Time
- 03_Jump_Physics_Euler_Fixed_Delta_Time
- 03_Jump_Physics_Velocity_Verlet
- 03_Jump_Physics_Velocity_Verlet_Faster_Falling
- 04_Climbing_Slopes

## Episode 03 Notes (Jumping)
Sebastian Lague uses Euler integration for jumping physics as mentioned by JackKell100 there is an underlying issue with Euler's method. "The character controller should use Verlet integration instead of Euler integration as the error created form the Euler integration prevents it from reaching the full desired jump height." - JackKell100. I suggest reading his comment to fully understand this issue.

I've created several branches under 03_Jump_Physics_* to showcase Euler's error regarding max jump height. Please check the console logs to view for yourself the error where the actual height is not the desired jump height.

I've also created alternative methods for jump physics such as the Verlet method (specifically Velocity Verlet). With Velocity Verlet I have removed the error regarding precision for the desired jump height. Credit goes to JackKell https://gist.github.com/JackKell/e11610d89c78b5b2d4046612d3f59af4 where they had an almost perfect Velocity Verlet implementation. The only issue was that JackKell was using fixedDeltaTime inside the Update method and it needed to be in FixedUpdate method instead. The Update method only supports deltaTime while FixedUpdate only supports fixedDeltaTime. This causes JackKell's implementation to have a bug where there were inconsistencies when the game was run on different screen sizes/devices.

## Episode 03_Jump_Physics_Velocity_Verlet_Faster_Falling - OPTIONAL FEATURE: Faster Falling
I have decided to continue Sebastian Lague's series with my own jumping implementation described above. ALL following branches will be using the 03_Jump_Physics_Velocity_Verlet_Faster_Falling implementation of jumping where I have already implemented the feature described in episode 10, faster falling.
