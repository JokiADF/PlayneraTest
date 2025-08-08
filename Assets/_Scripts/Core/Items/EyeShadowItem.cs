using System;
using UnityEngine;
using UnityEngine.UI;

namespace MakeupGame
{
    public class EyeShadowItem : ItemBase
    {
        [Header("EyeShadow Specific")]
        [SerializeField] private Image brushColor;
        [SerializeField] private int colorIndex;

        private const string PathToBrushColor = "_Sprites/Brush/EyeShadowColor";
        private const string PathToEyeShadows = "_Sprites/Eyes/eyes_";

        public override ItemType Type => ItemType.ShadowEye;
        public override int ColorIndex => colorIndex;

        protected override void Awake()
        {
            base.Awake();
            ValidateEyeShadowComponents();
        }

        private void ValidateEyeShadowComponents()
        {
            if (brushColor == null) 
                Debug.LogWarning($"[{GetType().Name}] Brush color image is missing", this);
            if (colorIndex < 0) 
                Debug.LogWarning($"[{GetType().Name}] Color index should be non-negative", this);
        }

        protected override void OnInitializeComplete()
        {
            base.OnInitializeComplete();
            
            if (brushColor != null)
            {
                brushColor.enabled = false;
                brushColor.sprite = null;
            }
            
            PreloadResources();
        }

        private void PreloadResources()
        {
            if (ResourceService != null) 
                ResourceService.PreloadSprite($"{PathToEyeShadows}{colorIndex}");
        }

        public override void OnTakeComplete()
        {
            if (brushColor != null && ResourceService != null)
            {
                var sprite = ResourceService.LoadSprite($"{PathToBrushColor}{colorIndex}");
                if (sprite != null)
                {
                    brushColor.enabled = true;
                    brushColor.sprite = sprite;
                }
                else
                    Debug.LogWarning($"[{GetType().Name}] Failed to load brush color sprite for index {colorIndex}");
            }
        }

        protected override void OnPutDown()
        {
            base.OnPutDown();
            
            if (brushColor != null)
            {
                brushColor.enabled = false;
                brushColor.sprite = null;
            }
        }
    }
}
