using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using MakeupGame.Services;

namespace MakeupGame
{
    [RequireComponent(typeof(Button))]
    public abstract class ItemBase : MonoBehaviour
    {
        [Header("Item Setup")]
        [SerializeField] protected RectTransform forHand;
        [SerializeField] private Button button;
        
        private Transform _originalParent;
        private Action<ItemBase> _onClickCallback;
        protected IResourceService ResourceService;

        public Vector3 OriginalPosition { get; private set; }
        public abstract ItemType Type { get; }
        public virtual int ColorIndex => 0;

        #region Unity Methods
        
        protected virtual void Awake() => 
            ValidateSetup();

        protected void Start() => 
            StoreOriginalState();

        protected virtual void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
            }
        }
        
        #endregion

        private void ValidateSetup()
        {
            if (forHand == null) 
                Debug.LogError($"[{GetType().Name}] forHand RectTransform is missing", this);
            if (button == null) 
                Debug.LogError($"[{GetType().Name}] Button component is missing", this);
        }

        private void StoreOriginalState()
        {
            if (forHand != null)
            {
                _originalParent = forHand.parent;
                OriginalPosition = forHand.position;
            }
            
            Debug.Log(forHand.position);
            Debug.Log(OriginalPosition);
        }

        public virtual void Initialize(Action<ItemBase> onClick, IResourceService resourceService)
        {
            _onClickCallback = onClick ?? throw new ArgumentNullException(nameof(onClick));
            ResourceService = resourceService ?? throw new ArgumentNullException(nameof(resourceService));

            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(NotifyExternalClick);
            }

            OnInitializeComplete();
        }

        protected virtual void OnInitializeComplete() { }

        public virtual void Take(Transform handParent)
        {
            Debug.Log(forHand.position);
            Debug.Log(OriginalPosition);

            if (forHand == null || handParent == null)
            {
                Debug.LogWarning($"[{GetType().Name}] Cannot take item - missing components");
                return;
            }

            forHand.SetParent(handParent);
            OnTaken();
        }

        protected virtual void OnTaken() { }

        public virtual void PutDown()
        {
            if (forHand == null || _originalParent == null)
            {
                Debug.LogWarning($"[{GetType().Name}] Cannot put down item - missing components");
                return;
            }

            forHand.SetParent(_originalParent);
            forHand.position = OriginalPosition;
            
            OnPutDown();
        }

        protected virtual void OnPutDown() { }

        public virtual void OnTakeComplete() { }

        private void NotifyExternalClick() => 
            _onClickCallback?.Invoke(this);
    }
}
