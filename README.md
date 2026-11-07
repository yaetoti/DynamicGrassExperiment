# Dynamic Instanced Grass Rendering (Experiment)

### Features
- Instanced rendering using Graphics.RenderMeshPrimitives (1 million blades = ~180 FPS on RTX 5070 / R5 3600)
- You can generate grass over any mesh. Generation is hapening in a compute shader only once, the buffer stays on GPU. Surface basis is taken into account (grass blades are always directed towards the surface normal)
- You can use any grass mesh made by an artist (bending depends on the V texture coordinate)
- Random wind sway
- Grass is bending under player (made using RenderTexture + orthographic camera)

### How it works

1. CPU takes the surface mesh, calculates area of every triangle and the total area to build a cummulative distribution. This is essential to create a uniform grass distribution over the whole mesh, because smaller triangles should have less blades placed on them
2. Triangles' data (positions, normals, tangents, CDF) is transfered to the GPU to be used in the InitGrass compute shader
3. The compute shader is being executed only once during Start. It generates a random float [0, 1], selects a triangle using CDF and binary search, generates another 2 random floats to create barycentric coordinates. Then it interpolates position, normal and tangent. All of them are transformed from the surface mesh space to the world space. The tangent is rotated around the normal so that all blades have random rotation
4. The resulting buffer stays on GPU, so no time is wasted on transfer
5. During update grass blades are rendered using Graphics.RenderMeshPrimitives. I use an unlit shader graph with color interpolation. For vertex preprocessing I use a custom HLSL function that bends blades in model space, then rotates and positions all the vertices.
6. For bending I use an orthographic camera placed above the whole map. It renders objects marked as GrassBender to a 1024x1024 R8_UNORM RenderTexture, which I then sample in Grass.hlsl vertex shader.
7. Unity does not have a native option to avoid using ClearColor before rendering (meh), so I created a RendererFeature that sets camera content to RendererTexture before drawing new content.

### Limitations & Space for improvement

- Currently grass bending does not take into account character altitude, so grass bends even if the character is in the air
- I wanna try adding grass cutting and particles. This will most likely require using another compute shader to identify the newly mowed grass and fill the position buffer. Since there is a lot of grass, I could randomly select like every tenth blade
- I saw people doing wind direction field using perlin noise. Would be interesting to figure out how that works.
