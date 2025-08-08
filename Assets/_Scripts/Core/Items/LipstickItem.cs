using UnityEngine;

namespace MakeupGame
{
    public class LipstickItem : ItemBase
    {
        [Header("Lipstick Specific")]
        [SerializeField] private int colorIndex;

        private const string PathToLipsticks = "_Sprites/Face/Lips/lip_";
        
        public override ItemType Type => ItemType.Lipstick;
        public override int ColorIndex => colorIndex;

        protected override void Awake()
        {
            base.Awake();
            
            if (colorIndex < 0)
            {
                Debug.LogWarning($"[{GetType().Name}] Color index should be non-negative", this);
                colorIndex = 0;
            }
        }

        protected override void OnInitializeComplete()
        {
            base.OnInitializeComplete();
            
            PreloadResources();
        }

        private void PreloadResources()
        {
            if (ResourceService != null) 
                ResourceService.PreloadSprite($"{PathToLipsticks}{colorIndex}");
        }
    }
}