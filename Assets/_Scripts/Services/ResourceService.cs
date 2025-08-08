using System.Collections.Generic;
using UnityEngine;

namespace MakeupGame.Services
{
    public class ResourceService : IResourceService
    {
        private readonly Dictionary<string, Sprite> _spriteCache = new();

        public void PreloadSprite(string path) => 
            LoadSprite(path);

        public Sprite LoadSprite(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("[ResourceService] Path is null or empty");
                return null;
            }

            if (_spriteCache.TryGetValue(path, out Sprite cachedSprite))
                return cachedSprite;

            var sprite = Resources.Load<Sprite>(path);
            
#if UNITY_EDITOR
            if (sprite == null) 
                Debug.LogWarning($"[ResourceService] Sprite not found at path: {path}");
#endif

            if (sprite != null) 
                _spriteCache[path] = sprite;

            return sprite;
        }

        public void UnloadSprite(string path)
        {
            if (_spriteCache.TryGetValue(path, out Sprite sprite))
            {
                if (sprite != null) 
                    Resources.UnloadAsset(sprite);
                _spriteCache.Remove(path);
            }
        }

        public void ClearCache()
        {
            foreach (var kvp in _spriteCache)
                if (kvp.Value != null) 
                    Resources.UnloadAsset(kvp.Value);
            _spriteCache.Clear();
        }
    }
}