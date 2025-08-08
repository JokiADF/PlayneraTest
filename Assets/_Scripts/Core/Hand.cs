using System;
using DG.Tweening;
using UnityEngine;

namespace MakeupGame.Core
{
    public class Hand : MonoBehaviour
    {
        [Header("Hand Setup")]
        [SerializeField] private RectTransform handRect;
        [SerializeField] private RectTransform pointForItem;
        [SerializeField] private RectTransform pointForShowHand;
        
        [Header("Animation Settings")]
        [SerializeField] private float moveDuration = 0.5f;
        [SerializeField] private Ease moveEase = Ease.OutQuart;

        private ItemBase _heldItem;
        private Face _face;
        
        private Vector2 _hiddenPosition;
        private Vector3 _positionDifference;
        
        private bool _isAnimating;

        public bool HasItem => _heldItem != null;

        #region Unity Methods
        
        private void Awake() => 
            ValidateComponents();

        private void OnDestroy() => 
            handRect.DOKill();

        private void ValidateComponents()
        {
            if (handRect == null)
                Debug.LogError("[Hand] Hand RectTransform is missing", this);
            
            if (pointForItem == null)
                Debug.LogError("[Hand] Point for item RectTransform is missing", this);
            
            if (pointForShowHand == null)
                Debug.LogWarning("[Hand] Point for show hand RectTransform is missing", this);
        }

        #endregion

        public void Initialize(Face face)
        {
            _face = face ?? throw new ArgumentNullException(nameof(face));
            
            if (handRect != null)
            {
                _hiddenPosition = handRect.anchoredPosition;
                CalculatePositionDifference();
                handRect.gameObject.SetActive(false);
            }
        }

        public void HandleItemClick(ItemBase item)
        {
            if (item == null)
            {
                Debug.LogWarning("[Hand] Attempted to handle click on null item");
                return;
            }

            if (_isAnimating || HasItem)
                return;

            if (handRect != null) 
                handRect.gameObject.SetActive(true);
            
            TakeNewItem(item);
        }

        public void HandleZoneClick()
        {
            if (!HasItem || _isAnimating)
                return;

            MoveToFace();
        }

        private void TakeNewItem(ItemBase item)
        {
            if (item == null || handRect == null) return;

            _heldItem = item;
            _isAnimating = true;

            var itemPos = _heldItem.OriginalPosition + _positionDifference;
            
            switch (_heldItem.Type)
            {
                case ItemType.ShadowEye:
                    AnimateEyeShadowTaking(itemPos);
                    break;
                    
                case ItemType.Lipstick:
                    AnimateLipstickTaking(itemPos);
                    break;
                    
                case ItemType.Cream:
                    AnimateCreamTaking(itemPos);
                    break;
                    
                default:
                    AnimateDefaultTaking(itemPos);
                    break;
            }
        }

        private void HideHand() => 
            PutItemBack(HideImmediately);

        private void CalculatePositionDifference()
        {
            if (handRect != null && pointForItem != null)
            {
                _positionDifference = handRect.position - new Vector3(
                    pointForItem.position.x, 
                    pointForItem.position.y - pointForItem.rect.height * pointForItem.lossyScale.y / 2, 
                    pointForItem.position.z
                );
            }
        }

        #region Animations
        
        private void AnimateEyeShadowTaking(Vector3 itemPos)
        {
            var palettePos = _heldItem.transform.position + _positionDifference + Vector3.down * 50;
            var targetPos = Vector3.zero + _positionDifference;

            handRect.DOKill();
            DOTween.Sequence()
                .Append(handRect.DOMove(itemPos, moveDuration).SetEase(moveEase)
                    .OnComplete(() => _heldItem.Take(pointForItem)))
                .Append(handRect.DOMove(palettePos, moveDuration).SetEase(moveEase))
                .AppendInterval(moveDuration / 3)
                .Append(handRect.DOMove(palettePos + Vector3.left * 50, moveDuration / 6).SetEase(Ease.Linear))
                .Append(handRect.DOMove(palettePos + Vector3.right * 50, moveDuration / 6).SetEase(Ease.Linear).SetLoops(6, LoopType.Yoyo))
                .Append(handRect.DOMove(palettePos, moveDuration / 6)
                    .OnComplete(() => _heldItem.OnTakeComplete()))
                .AppendInterval(moveDuration / 3)
                .Append(handRect.DOScale(Vector3.one, moveDuration / 6))
                .Append(handRect.DOAnchorPos(targetPos, moveDuration).SetEase(moveEase)
                    .OnComplete(() => _isAnimating = false));
        }

        private void AnimateLipstickTaking(Vector3 itemPos)
        {
            var targetPos = Vector3.zero + _positionDifference;

            handRect.DOKill();
            DOTween.Sequence()
                .Append(handRect.DOMove(itemPos, moveDuration).SetEase(moveEase)
                    .OnComplete(() => _heldItem.Take(pointForItem)))
                .Append(handRect.DOAnchorPos(targetPos, moveDuration).SetEase(moveEase)
                    .OnComplete(() => _isAnimating = false));
        }

        private void AnimateCreamTaking(Vector3 itemPos)
        {
            var targetPos = (_heldItem.OriginalPosition + _positionDifference + _face.transform.position) / 2;

            handRect.DOKill();
            DOTween.Sequence()
                .Append(handRect.DOMove(itemPos, moveDuration).SetEase(moveEase)
                    .OnComplete(() => _heldItem.Take(pointForItem)))
                .Append(handRect.DOMove(targetPos, moveDuration).SetEase(moveEase)
                    .OnComplete(() => _isAnimating = false));
        }

        private void AnimateDefaultTaking(Vector3 itemPos)
        {
            var targetPos = Vector3.zero + _positionDifference;

            handRect.DOKill();
            DOTween.Sequence()
                .Append(handRect.DOMove(itemPos, moveDuration).SetEase(moveEase)
                    .OnComplete(() => _heldItem.Take(pointForItem)))
                .Append(handRect.DOMove(targetPos, moveDuration).SetEase(moveEase)
                    .OnComplete(() => _isAnimating = false));
        }

        private void MoveToFace()
        {
            if (_face == null || _heldItem == null || handRect == null) return;

            _isAnimating = true;

            var targetTransform = _heldItem.Type == ItemType.Lipstick ? 
                _face.LipsTransform : _face.SkinTransform;

            if (targetTransform == null)
            {
                Debug.LogWarning("[Hand] Target transform is null for face interaction");
                _isAnimating = false;
                return;
            }

            var corners = new Vector3[4];
            targetTransform.GetWorldCorners(corners);
            
            var leftEdge = new Vector3(corners[0].x, targetTransform.position.y + _positionDifference.y, 0);
            var rightEdge = new Vector3(corners[2].x, targetTransform.position.y + _positionDifference.y, 0);

            handRect.DOKill();
            DOTween.Sequence()
                .Append(handRect.DOMove(targetTransform.position + _positionDifference, moveDuration).SetEase(moveEase))
                .Append(handRect.DOMove(leftEdge, moveDuration / 4).SetEase(Ease.Linear))
                .Append(handRect.DOMove(rightEdge, moveDuration / 4).SetEase(Ease.Linear).SetLoops(6, LoopType.Yoyo))
                .OnComplete(() =>
                {
                    _face.OnItemUsed(_heldItem);
                    HideHand();
                });
        }

        private void PutItemBack(Action onComplete)
        {
            if (_heldItem == null || handRect == null)
            {
                onComplete?.Invoke();
                return;
            }

            _isAnimating = true;
            
            handRect.DOKill();
            handRect.DOMove(_heldItem.OriginalPosition + _positionDifference, moveDuration)
                .SetEase(moveEase)
                .OnComplete(() =>
                {
                    _heldItem.PutDown();
                    _heldItem = null;
                    onComplete?.Invoke();
                });
        }

        private void HideImmediately()
        {
            if (handRect == null) return;

            _isAnimating = true;
            
            handRect.DOKill();
            handRect.DOAnchorPos(_hiddenPosition, moveDuration)
                .SetEase(moveEase)
                .OnComplete(() =>
                {
                    handRect.gameObject.SetActive(false);
                    _isAnimating = false;
                });
        }
        
        #endregion
    }
}
