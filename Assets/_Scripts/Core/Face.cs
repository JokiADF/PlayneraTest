using System;
using DG.Tweening;
using MakeupGame.Services;
using UnityEngine;
using UnityEngine.UI;

namespace MakeupGame.Core
{
    public class Face : MonoBehaviour
    {
        [Header("Face Images")]
        [SerializeField] private Image skin;
        [SerializeField] private Image lips;
        [SerializeField] private Image eyes;
        
        [Header("Transform References")]
        [SerializeField] private RectTransform lipsTransform;
        
        [Header("Interaction")]
        [SerializeField] private Button button;

        private const string LipFormatPath = "_Sprites/Face/Lips/lip_";
        private const string EyeFormatPath = "_Sprites/Face/Eyes/eye_";

        private Action _onClickCallback;
        private IResourceService _resourceService;

        public RectTransform SkinTransform => skin != null ? skin.rectTransform : null;
        public RectTransform LipsTransform => lipsTransform;

        #region Unity Methods
        
        private void Awake() => 
            ValidateComponents();

        private void OnDestroy()
        {
            if (button != null) 
                button.onClick.RemoveAllListeners();
        }

        private void ValidateComponents()
        {
            if (skin == null)
                Debug.LogWarning("[Face] Skin image is missing", this);
            
            if (lips == null)
                Debug.LogWarning("[Face] Lips image is missing", this);
            
            if (eyes == null)
                Debug.LogWarning("[Face] Eyes image is missing", this);
            
            if (button == null)
                Debug.LogWarning("[Face] Button component is missing", this);

            if (lipsTransform == null && lips != null)
                lipsTransform = lips.rectTransform;
        }
        
        #endregion

        public void Initialize(Action onClick, IResourceService resourceService)
        {
            _onClickCallback = onClick ?? throw new ArgumentNullException(nameof(onClick));
            _resourceService = resourceService ?? throw new ArgumentNullException(nameof(resourceService));

            SetupButton();
            InitializeFaceState();
        }

        public void OnItemUsed(ItemBase heldItem)
        {
            if (heldItem == null)
            {
                Debug.LogWarning("[Face] Attempted to use null item");
                return;
            }

            switch (heldItem.Type)
            {
                case ItemType.Cream:
                    ApplyCream();
                    break;
                    
                case ItemType.Lipstick:
                    ApplyLipstick(heldItem.ColorIndex);
                    break;
                    
                case ItemType.ShadowEye:
                    ApplyEyeShadow(heldItem.ColorIndex);
                    break;
                    
                default:
                    Debug.LogWarning($"[Face] Unknown item type: {heldItem.Type}");
                    break;
            }
        }

        private void SetupButton()
        {
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(NotifyExternalClick);
            }
        }

        private void InitializeFaceState()
        {
            if (skin != null)
            {
                skin.color = new Color(skin.color.r, skin.color.g, skin.color.b, 1);
                skin.enabled = true;
            }

            if (lips != null)
            {
                lips.color = new Color(lips.color.r, lips.color.g, lips.color.b, 0);
                lips.enabled = false;
            }

            if (eyes != null)
            {
                eyes.color = new Color(eyes.color.r, eyes.color.g, eyes.color.b, 0);
                eyes.enabled = false;
            }
        }

        private void ApplyCream()
        {
            if (skin != null)
            {
                skin?.DOKill();
                skin.DOFade(0f, 0.6f).OnComplete(() =>
                    {
                        skin.enabled = false;
                    });
            }
        }

        private void ApplyLipstick(int colorIndex)
        {
            if (lips == null || _resourceService == null) return;

            var lipSprite = _resourceService.LoadSprite($"{LipFormatPath}{colorIndex}");
            if (lipSprite != null)
            {
                lips.sprite = lipSprite;
                lips.enabled = true;
                
                lips?.DOKill();
                lips.DOFade(1f, 0.5f);
            }
            else
                Debug.LogWarning($"[Face] Failed to load lipstick sprite for color index {colorIndex}");
        }

        private void ApplyEyeShadow(int colorIndex)
        {
            if (eyes == null || _resourceService == null) return;

            var eyeSprite = _resourceService.LoadSprite($"{EyeFormatPath}{colorIndex}");
            if (eyeSprite != null)
            {
                eyes.sprite = eyeSprite;
                eyes.enabled = true;
                
                eyes?.DOKill();
                eyes.DOFade(1f, 0.5f);
            }
            else
                Debug.LogWarning($"[Face] Failed to load eye shadow sprite for color index {colorIndex}");
        }

        private void NotifyExternalClick() => 
            _onClickCallback?.Invoke();
    }
}
