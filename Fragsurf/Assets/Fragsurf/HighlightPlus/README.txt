**************************************
*          HIGHLIGHT PLUS            *
* Created by Ramiro Oliva (Kronnect) * 
*            README FILE             *
**************************************


Notice about Universal Rendering Pipeline
-----------------------------------------
This package is designed for URP.
It requires Unity 2019.3 and URP 7.1.6 or later
To install the plugin correctly:

1) Make sure you have Universal Rendering Pipeline asset installed (from Package Manager).
2) Go to Project Settings / Graphics.
3) Double click the Universal Rendering Pipeline asset.
4) Double click the Forward Renderer asset.
5) Click "+" to add the Highlight Plus Renderer Feature to the list of the Forward Renderer Features.

You can also find a HighlightPlusForwardRenderer asset in the Highlight Plus / Pipelines / URP folder.
Make sure the Highlight Plus Scriptable Renderer Feature is listed in the Renderer Features of the  Forward Renderer in the pipeline asset.


Quick help: how to use this asset?
----------------------------------

1) Highlighting specific objects: add HighlightEffect.cs script to any GameObject. Customize the appearance options.
  In the Highlight Effect inspector, you can specify which objects, in addition to this one, are also affected by the effect:
    a) Only this object
    b) This object and its children
    c) All objects from the root to the children
    d) All objects belonging to a layer

2) Control highlight effect when mouse is over:
  Add HighlightTrigger.cs script to the GameObject. It will activate highlight on the gameobject when mouse pass over it.

3) Highlighting any object in the scene:
  Select top menu GameObject -> Effects -> Highlight Plus -> Create Manager.
  Customize appearance and behaviour of Highlight Manager. Those settings are default settings for all objects. If you want different settings for certain objects just add another HighlightEffect script to each different object. The manager will use those settings.

4) Make transparent shaders compatible with See-Through effect:
  If you want the See-Through effect be seen through other transparent objects, they need to be modified so they write to depth buffer (by default transparent objects do not write to z-buffer).
  To do so, select top menu GameObject -> Effects -> Highlight Plus -> Add Depth To Transparent Object.

5) Static batching:
  Objects marked as "static" need a MeshCollider in order to be highlighted. This is because Unity combines the meshes of static objects so it's not possible to highlight individual objects if their meshes are combined.
  To allow highlighting static objects make sure they have a MeshCollider attached (the MeshCollider can be disabled).




Help & Support Forum
--------------------

Check the Documentation (PDF) for detailed instructions:

Have any question or issue?
* Email: contact@kronnect.com
* Support Forum: http://kronnect.com
* Twitter: @KronnectGames

If you like Highlight Plus, please rate it on the Asset Store. It encourages us to keep improving it! Thanks!




Future updates
--------------

All our assets follow an incremental development process by which a few beta releases are published on our support forum (kronnect.com).
We encourage you to signup and engage our forum. The forum is the primary support and feature discussions medium.

Of course, all updates of Highlight Plus will be eventually available on the Asset Store.



More Cool Assets!
-----------------
Check out our other assets here:
https://assetstore.unity.com/publishers/15018



Version history
---------------

Version 5.3.4 22/01/2021
- Optimizations to material setters
- [Fix] Fixed outline color issue with quality level set to medium

Version 5.3.3
- Effects now reflect object transform changes when combines meshes option is enabled

Version 5.3
- Added "Combine Meshes" option to profile
- Optimizations and fixes

Version 5.2
- Added "Object Name Filter" option to profile

Version 5.1
- Added "Border When Hidden" effect (outline when see-through triggers)

Version 5.0.1
- Added support for Unity 2020.2 beta

Version 5.0
- API: added "TargetFX" method to programmatically start the target effect  
- Added support for double-sided shader effects

Version 4.9
- Added "Medium" quality level

Version 4.8.2
- [Fix] Fixed issue with outline set to fastest and glow using highest in latest URP version

Version 4.8.1
- [Fix] Fixed issue with outline/glow when overlay cameras are present on the stack

Version 4.8
- Added "Outer Glow Blend Passes" option
- [Fix] Fixed outline & glow issue with alpha cutout when using non-highest quality mode

Version 4.7
- Added "Normals Option" with Smooth, Preserve and Reorient variants to improve results
- Target effect now only renders once per gameobject if a specific target transform is specified
- API: added OnTargetAnimates. Allows you to override center, rotation and scale of target effect on a per-frame basis.

Version 4.6
- Added "SubMesh Mask" which allows to exclude certain submeshes
- [Fix] Fixed shader compilation issue with Single Pass Instanced mode enabled

Version 4.4
- Exposed "Smooth Normals" option in inspector.

Version 4.3.2
- Added HitFX effect
- Improvements to SeeThrough Occluder when Detection Mode is set to RayCast

Version 4.3.1
- [Fix] Fixed issue with Highlight Effect Occluder script

Version 4.3
- Added GPU instancing support for outline / outer glow effects

Version 4.2.2
- [Fix] Fixed effect being rendered when object is outside of frustum camera

Version 4.2.1
- Profile: added "Constant Width" property
- Enabled HDR color picker to Color properties
- [Fix] Fixed missing outline with flat surfaces like quads under certain angles

Version 4.2
- Glow/Outline downsampling option added to profiles
- [Fix] Removed VR API usage console warning

Version 4.1
- Added Outline Independent option
- [Fix] Fixed error when highlight script is added to an empty gameobject

Version 4.0
- Support for URP Scriptable Rendering Feature
