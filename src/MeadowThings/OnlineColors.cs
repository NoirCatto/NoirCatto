using RainMeadow;
using UnityEngine;

namespace NoirCatto;

internal static partial class MeadowThings
{
    public static void GetMeadowColors(PlayerGraphics self, ref Color? eyeColor, ref Color? bodyColor, ref Color? fluffColor, ref Color? noseColor, ref Color? pawsColor)
    {
        if (RainMeadow.RainMeadow.creatureCustomizations.TryGetValue(self.player, out var avatarData) && avatarData is SlugcatCustomization customization)
        {
            eyeColor = customization.currentColors[1];
            bodyColor = customization.currentColors[0];
            fluffColor = customization.currentColors[2];
            noseColor = customization.currentColors[3];
            pawsColor = customization.currentColors[4];
        }
    }
    
    public static string GetOnlineObjectId(AbstractPhysicalObject abstractPhysicalObject)
    {
        var onlineObject = abstractPhysicalObject.GetOnlineObject();
        return $"{onlineObject.id.originalOwner}{onlineObject.id.id}";
    }
}