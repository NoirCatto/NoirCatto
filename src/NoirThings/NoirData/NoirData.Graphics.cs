using System.Linq;
using UnityEngine;

namespace NoirCatto;

public partial class NoirCatto
{
    public partial class NoirData
    {
        #region Graphics Variables
        public const int NewSprites = 2;
        public readonly int[] EarSpr = new int[2];
        public int TotalSprites;

        public readonly TailSegment[][] Ears =
        [
            new TailSegment[2],
            new TailSegment[2]
        ];
        public readonly int[] EarsFlip = [1, 1];
        public Vector2 LastHeadRotation;
        public bool CallingAddToContainerFromOrigInitiateSprites;
        #endregion

        public FAtlasElement ElementFromTexture(Texture2D texture, bool forceRedraw = false)
        {
            var name = texture.name + "_" + Cat.playerState.playerNumber;
            if (forceRedraw)
            {
                var oldAtlas = Futile.atlasManager._atlases.FirstOrDefault(x => x.name == name);
                if (oldAtlas != null)
                {
                    Futile.atlasManager._allElementsByName.Remove(oldAtlas.name);
                    oldAtlas.Unload();
                    Object.Destroy(oldAtlas.texture);
                    Futile.atlasManager._atlases.Remove(oldAtlas);
                }
            }
            var atlas = Futile.atlasManager.LoadAtlasFromTexture(name, texture, false);
            return atlas.elements[0];
        }
    }
}