using UnityEngine;

namespace MakeupGame.Services
{
    public interface IResourceService
    {
        void PreloadSprite(string path);
        Sprite LoadSprite(string path);
        void UnloadSprite(string path);
    }
}