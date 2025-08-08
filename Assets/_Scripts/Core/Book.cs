using System;
using MakeupGame.Core;
using UnityEngine;
using UnityEngine.UI;
using MakeupGame.Services;

namespace MakeupGame.UI
{
    public class Book : MonoBehaviour
    {
        [Header("Tab Buttons")]
        [SerializeField] private Button tabEyeShadows;
        [SerializeField] private Button tabLipsticks;
        
        [Header("Content Panels")]
        [SerializeField] private GameObject eyeShadows;
        [SerializeField] private GameObject lipsticks;

        private const string PathEyeShadowsEnable = "_Sprites/Tabs/EyeShadowsEnable";
        private const string PathEyeShadowsDisable = "_Sprites/Tabs/EyeShadowsDisable";
        private const string PathLipsticksEnable = "_Sprites/Tabs/LipsticksEnable";
        private const string PathLipsticksDisable = "_Sprites/Tabs/LipsticksDisable";

        private Hand _hand;
        private IResourceService _resourceService;
        
        private bool _isEyeShadowActive;
        private bool _isLipsticksActive;

        #region Unity Methods
        
        private void Awake() => 
            ValidateComponents();

        private void OnDestroy()
        {
            if (tabEyeShadows != null)
                tabEyeShadows.onClick.RemoveAllListeners();
            
            if (tabLipsticks != null)
                tabLipsticks.onClick.RemoveAllListeners();
        }

        private void ValidateComponents()
        {
            if (tabEyeShadows == null)
                Debug.LogError("[Book] Eye shadows tab button is missing", this);
            
            if (tabLipsticks == null)
                Debug.LogError("[Book] Lipsticks tab button is missing", this);
            
            if (eyeShadows == null)
                Debug.LogError("[Book] Eye shadows panel is missing", this);
            
            if (lipsticks == null)
                Debug.LogError("[Book] Lipsticks panel is missing", this);
        }

        #endregion

        public void Initialize(Hand hand, IResourceService resourceService)
        {
            _hand = hand;
            _resourceService = resourceService;
            
            SetupTabs();
            PreloadSprites();
            
            SetActiveTab(TabType.EyeShadows);
        }

        private void SetupTabs()
        {
            SetupTabButton(tabEyeShadows, () => SetActiveTab(TabType.EyeShadows));
            SetupTabButton(tabLipsticks, () => SetActiveTab(TabType.Lipsticks));
        }
        
        private void SetupTabButton(Button tabButton, Action onClickAction)
        {
            if (tabButton == null) return;

            tabButton.onClick.RemoveAllListeners();
            tabButton.onClick.AddListener(() =>
            {
                if (!_hand.HasItem) onClickAction?.Invoke();
            });
        }

        private void PreloadSprites()
        {
            _resourceService?.PreloadSprite(PathEyeShadowsEnable);
            _resourceService?.PreloadSprite(PathEyeShadowsDisable);
            _resourceService?.PreloadSprite(PathLipsticksEnable);
            _resourceService?.PreloadSprite(PathLipsticksDisable);
        }

        private void SetActiveTab(TabType tabType)
        {
            switch (tabType)
            {
                case TabType.EyeShadows:
                    ActivateEyeShadowsTab();
                    break;
                    
                case TabType.Lipsticks:
                    ActivateLipsticksTab();
                    break;
            }
        }

        private void ActivateEyeShadowsTab()
        {
            if (_isEyeShadowActive) return;

            _isEyeShadowActive = true;
            _isLipsticksActive = false;

            SetPanelActive(eyeShadows, true);
            SetPanelActive(lipsticks, false);

            UpdateTabSprite(tabEyeShadows, PathEyeShadowsEnable);
            UpdateTabSprite(tabLipsticks, PathLipsticksDisable);
        }

        private void ActivateLipsticksTab()
        {
            if (_isLipsticksActive) return;

            _isLipsticksActive = true;
            _isEyeShadowActive = false;

            SetPanelActive(eyeShadows, false);
            SetPanelActive(lipsticks, true);

            UpdateTabSprite(tabEyeShadows, PathEyeShadowsDisable);
            UpdateTabSprite(tabLipsticks, PathLipsticksEnable);
        }

        private void SetPanelActive(GameObject panel, bool active)
        {
            if (panel != null) 
                panel.SetActive(active);
        }

        private void UpdateTabSprite(Button tabButton, string spritePath)
        {
            if (tabButton?.image == null || _resourceService == null) return;

            var sprite = _resourceService.LoadSprite(spritePath);
            if (sprite != null)
                tabButton.image.sprite = sprite;
            else
                Debug.LogWarning($"[Book] Failed to load sprite: {spritePath}");
        }

        private enum TabType
        {
            EyeShadows,
            Lipsticks
        }
    }
}
