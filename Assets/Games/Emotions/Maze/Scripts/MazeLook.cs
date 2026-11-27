using UnityEngine;

namespace GameHub.Maze
{
    public sealed class MazeLook
    {
        public const float FloorTile = 4f;
        public const float WallTile = 2f;
        public const float CeilingTile = 2f;

        private const string LampShader = "Sprites/Default";
        private const string SmoothnessProperty = "_Smoothness";
        private static readonly int BaseMapTiling = Shader.PropertyToID("_BaseMap_ST");
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        private static readonly Color LampColor = new(1f, 0.95f, 0.85f, 1f);

        private readonly Texture2D floorTexture;
        private readonly Texture2D wallTexture;
        private readonly Texture2D ceilingTexture;
        private readonly MaterialPropertyBlock block = new();

        public MazeLook(Material floorBase, Material wallBase, Material ceilingBase)
        {
            floorTexture = MazeTextures.Floor();
            wallTexture = MazeTextures.Wall();
            ceilingTexture = MazeTextures.Ceiling();
            Floor = Build(floorBase, floorTexture, 0.35f);
            Wall = Build(wallBase, wallTexture, 0.12f);
            Ceiling = Build(ceilingBase, ceilingTexture, 0.05f);
            Lamp = new Material(Shader.Find(LampShader)) { color = LampColor };
        }

        public Material Floor { get; }

        public Material Wall { get; }

        public Material Ceiling { get; }

        public Material Lamp { get; }

        public void Paint(Renderer renderer, Material material, Vector2 tiling, Color tint)
        {
            renderer.sharedMaterial = material;
            renderer.GetPropertyBlock(block);
            block.SetVector(BaseMapTiling, new Vector4(tiling.x, tiling.y, 0f, 0f));
            block.SetColor(BaseColor, tint);
            renderer.SetPropertyBlock(block);
        }

        public void Release()
        {
            Object.Destroy(Floor);
            Object.Destroy(Wall);
            Object.Destroy(Ceiling);
            Object.Destroy(Lamp);
            Object.Destroy(floorTexture);
            Object.Destroy(wallTexture);
            Object.Destroy(ceilingTexture);
        }

        private static Material Build(Material source, Texture2D texture, float smoothness)
        {
            Material material = new(source)
            {
                mainTexture = texture,
                mainTextureScale = Vector2.one,
                color = Color.white,
            };
            if (material.HasProperty(SmoothnessProperty))
                material.SetFloat(SmoothnessProperty, smoothness);
            return material;
        }
    }
}
