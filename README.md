# GrandGeoGrass-Public
Geometry grass shader for unity.

Contact me on twitter for some more gifs or if you have any questions. [@TheRensei](https://twitter.com/TheRensei)

GGG is a geometry shader I was working on for quite a while. I started learning shader with that and just kept adding stuff.

# Shader Features :
- Lighting:
  - There is a simple 2 color gradient along the uv.y with added wind color contribution if enabled.
  - The shader is using AlphaToMask and a mipmap magic to make is sharper but still smooth.
  - The lighting itself is based on lighting from here (https://en.wikibooks.org/wiki/Cg_Programming/Unity) plus whatever I changed/added and it worked.
  - Point Lights and shadow casting support with additional passes.
  - 
 ![ ](https://github.com/TheRensei/GrandGeoGrass-Public/blob/main/Screenshots/GGG_Point%20Lights.gif)
 
- 3 Stage LOD:
  - New vertices are not created when the base vertex position is higher than max render distance value.
  - Billboards are drawn afterwards, they have wind and random rotation applied, but not interaction or view bending.
  - Then 2 intersecting quads are drawn with all features enabled.
  - The full model of 3 intersecing quads is drawn the closest to camera, all features applied.
- Wind:
  - A world space noise texture pan.
  - Global settings for speed, strength and directions.
  - The green color channel of the vertex holds wind texture length, which is then used for applying the wind color.

- View Bending:
  - Pushes the top vertices away from the camera when looking down.
 
- Render Texture bending (Interaction)
  - Ortographic camera is rendering to a Render Texture on a specific layer.
  - The RT and Camera data required are set globally so it could be used for other stuff as well.
  - The RT is then mapped in world space and sampled.
  - RGB of the camera corresponds to xyz of the top vertex.
  - RT should allow for values at least -1 to 1 for bending in all directions That will require an additonal shader for the particle system (or whatever you wanna use), which will output negative colours as well.
  - The strength falls off with the distance so the displacement is smooth on the edges of the RT.
 
![ ](https://github.com/TheRensei/GrandGeoGrass-Public/blob/main/Screenshots/ggg_gif0.gif)
 
![ ](https://github.com/TheRensei/GrandGeoGrass-Public/blob/main/Screenshots/GGG_Shader.PNG)
 
 !! The vertex color R and B are used for height and width and are set in the painter scripts. If you don't use those, make sure vertex colors are set to 1, or just remove the vertex color multiplication from the shader.
 
 # Point Cloud Paiting :
- GGG_AreaPainter.cs 
  - Uses a Job system to cast rays down on a collider bellow and creates a point mesh with vertices where rays hit something.
  - The set amount of rays is generated randomly inside the entire area.
  - An alternative is to use a density map (in local or world space). If that is used the rays are still shot randomly, but the process is repeated until a sufficient number of vertices was created.
  - The height and width random range can be uniform or set separetely. 
  - There is an option to set the seed to remove repetition.
  - An option to save or clear the mesh.

 ![ ](https://github.com/TheRensei/GrandGeoGrass-Public/blob/main/Screenshots/GGG_AreaPainter.PNG)


# References, tutorials and everything I used to learn:

Point Cloud + Geometry Shader
https://www.youtube.com/watch?v=b2AlyCNbYmY

Grass shape
https://developer.nvidia.com/gpugems/gpugems/part-i-natural-effects/chapter-7-rendering-countless-blades-waving-grass

Tangent space + rotations + shadow bands fix
https://roystan.net/articles/grass-shader.html

Lighting bits + Shader feature toggles
https://halisavakis.com/my-take-on-shaders-grass-shader-part-ii/

Lighting basics
https://en.wikibooks.org/wiki/Cg_Programming/Unity

Point Lights (I had some silly problems with this and [@minionsart](https://twitter.com/minionsart) helped ;-;
https://www.patreon.com/posts/grass-geometry-1-40090373

Interaction
Simple vertex displacement based on position
https://www.patreon.com/posts/19844414

Render Texture + Particles
https://forum.unity.com/threads/limitations-of-render-texture-particle-system-method-for-interactive-shaders.841735/
https://twitter.com/Ed_dV/status/1072458319590240257
https://www.bruteforce-games.com/post/grass-shader-devblog-04
https://twitter.com/cayou66/status/1073667581276508161?s=20

View angle Bending
Camera view angle
https://twitter.com/ryangreen8/status/634096321335590912

The idea of view bending
https://www.gdcvault.com/play/1025530/Between-Tech-and-Art-The

Rotation matrix that rotates around the provided axis
https://gist.github.com/keijiro/ee439d5e7388f3aafc5296005c8c3f33

Alpa to coverage mip map magic
From: https://medium.com/@bgolus/anti-aliased-alpha-test-the-esoteric-alpha-to-coverage-8b177335ae4f
https://stackoverflow.com/questions/24388346/how-to-access-automatic-mipmap-level-in-glsl-fragment-shader-texture

Custom culling
https://gamedev.stackexchange.com/questions/125462/check-if-vertex-is-visible-in-shader 
Dot product culling
https://answers.unity.com/questions/1010169/how-to-know-if-an-object-is-looking-at-an-other.html
