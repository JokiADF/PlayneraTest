using UnityEngine;
using System.Collections.Generic;
using MakeupGame.Core;
using MakeupGame.Services;
using MakeupGame.UI;

namespace MakeupGame
{
    public class GameBootstrapper : MonoBehaviour
    {
        [Header("Core Components")]
        [SerializeField] private Hand hand;
        [SerializeField] private Face face;
        [SerializeField] private Book book;
        
        [Header("Items")]
        [SerializeField] private List<ItemBase> items;

        private IResourceService _resourceService;
        private bool _isInitialized;

        #region Unity Methods
        
        private void Awake()
        {
            Application.targetFrameRate = 60;
            
            if (!ValidateComponents())
            {
                Debug.LogError("[GameBootstrapper] Component validation failed");
                return;
            }

            InitializeServices();
            InitializeComponents();
            _isInitialized = true;
        }

        private void OnDestroy()
        {
            if (_resourceService is ResourceService resourceService) 
                resourceService.ClearCache();
        }
        
        #endregion

        private void InitializeServices() => 
            _resourceService = new ResourceService();

        private void InitializeComponents()
        {
            face.Initialize(OnFaceClicked, _resourceService);
            hand.Initialize(face);
            book.Initialize(hand, _resourceService);

            foreach (var item in items)
                if (item != null)
                    item.Initialize(OnItemClicked, _resourceService);
        }

        private void OnItemClicked(ItemBase item)
        {
            if (!_isInitialized || item == null)
                return;

            hand.HandleItemClick(item);
        }

        private void OnFaceClicked()
        {
            if (!_isInitialized)
                return;

            hand.HandleZoneClick();
        }

        private bool ValidateComponents()
        {
            if (hand == null)
            {
                Debug.LogError("[GameBootstrapper] Hand component is missing");
                return false;
            }

            if (face == null)
            {
                Debug.LogError("[GameBootstrapper] Face component is missing");
                return false;
            }

            if (items == null || items.Count == 0) 
                Debug.LogWarning("[GameBootstrapper] No items assigned");

            return true;
        }
    }
}
