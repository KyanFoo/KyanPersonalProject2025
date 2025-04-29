Unity Project: KyanPersonalProject2025
Custom Shader: Dissolve Shader (Shader Graph - URP)
======================================================================

1. Dissolve#1: DISSOLVE using Unity Shader Graph (Brackeys)
=======================================================================
- Time Nodes, animated dissolve effect, simulating a "camouflage" transition.

2. Dissolve#2: Dissolve Effect in Unity Shader Graph (Daniel Ilett)
=======================================================================
- World-Space Cutoff, Supports axis-based dissolve (X, Y, Z).

3. Dissolve#3: How to Make a Dissolve Shader Unity (Updated 2023) (Rigor Mortis Tortoise)
=======================================================================
- Basic dissolve effect, lacks advanced features.

4. Dissolve#4: Easy Dissolve Shader for Unity Tutorial (supports Emission maps) (The Game Dev Cave)
=======================================================================
- Texture2D Node, place the material textures onto the GameObject.
- Dissolve Edge, more refine concept that previous shader.

5. Dissolve#5: Unity Shader graph - Dissolve Effect Tutorial (Unity Magic)
=======================================================================
- Time Nodes, animated dissolve effect, simulating a "camouflage" transition.

6. Dissolve#6: Dissolve effect in Shader Graph (PabloMakes)
=======================================================================
- Texture2D Node, place the material textures onto the GameObject.

7. How to Scale Texture in Unity using Shader Graph (Vikings Devlogs)
=======================================================================
- Tiling and Offset, Integrate different type of noise texture for custom scaling.

8. Unity Shader Graph - Liquid Effect Tutorial (Gabriel Aguiar Prod.)
=======================================================================
- Remap Node, adjusting the value of the dissolve effect of the GameObject.
(X = fully visible, Y = fully dissolved)

======================================================================

Shader Graph Nodes:
----------------- Textures -----------------
Albedo Texture (Texture2D):
- Applying the Albedo Texture to the GameObject.
- Base color texture of the GameObject.

Normal Texture (Texture2D):
- Applying the Normal Texture to the GameObject.
- Adds surface detail and depth.

Metallic Texture (Texture2D):
- Applying the Metallic Texture to the GameObject.
- Controls how "metal-like" the surface appears.

Emission Texture (Texture2D):
- Applying the Emission Texture to the GameObject.
- Adds glowing or light-emitting areas.

Smoothness (Float):
- Controls the reflectivity and glossiness of the surface.

----------------- Noise Textures -----------------
Base Color (Color):
- Control the main color applied to the material of the GameObject.

Edge Color (Color):
- Control the color of the glowing edges of the GameObject during dissolve.

Edge Width (Float):
- Control the Thickness of the dissolve edge.

Threshold (Float):
- Controls dissolve progression of the GameObject.
(0 = fully visible, 1 = fully dissolved).

Noise Scale (Float):
- Control the scale of the Simple Noise node used for dissolve pattern.

Emission Power (Float):
- Controls intensity of glow from emission texture.

Tilling & Offset (Vector2):
- Adjusts how Simple Noise node is positioned and repeated.

----------------- Noise Controls -----------------
IsTextureOn (Boolean):
- Toggles between using Simple Noise or Noise Texture.

Noise Texture (Texture2D):
- Optional noise map for unique dissolve.

Texture Scale (Float):
- Controls scale of Noise Texture.

Texture Offset (Vector2):
- Adjusts how Noise Texture is positioned and repeated.

----------------- Adjustments -----------------
IsEmissionTexture (Boolean):
- Toggle between whether an emissive material is used.

Threshold Control (Vector2):
- X and Y values that regulate dissolve based on object-specific logic.

Reverse (Boolean):
- Toggles the direction of the dissolve.
(e.g., top-down vs. bottom-up)

False Threshold Control (Vector2):
- Used when Reverse is false; controls dissolve based on object-specific logic. (X = fully visible, Y = fully dissolved)

True Threshold Control (Vector2):
- Used when Reverse is true; controls dissolve based on object-specific logic. (X = fully visible, Y = fully dissolved)

======================================================================