using System;
using System.Collections.Generic;
using HECSFramework.Core;
using HECSFramework.Unity;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Components
{
    [Serializable][Documentation(Doc.HECS, Doc.Input, "this component holds bool  - on this moment we over ui or not")]
    public sealed partial class InputOverUIComponent : BaseComponent, IWorldSingleComponent, IInitable
    {
        public InputIdentifier TouchPositionInputIdentifier;
        
        private PointerEventData pointerEventData;
        private List<RaycastResult> raycastResults = new List<RaycastResult>(3);
        private InputAction touchAction;
        private bool inputOverUI;
        private int frameCount;
        public void Init()
        {
            pointerEventData  = new PointerEventData(EventSystem.current);
            EntityManager.Default.GetSingleComponent<InputActionsComponent>()
                .TryGetInputAction(TouchPositionInputIdentifier.name, out touchAction);
        }
        
        public bool IsOverUI()
        {
            if (Time.frameCount == frameCount)
                return inputOverUI;
            
            pointerEventData.position = touchAction.ReadValue<Vector2>();
            EventSystem.current.RaycastAll(pointerEventData, raycastResults);
            inputOverUI = raycastResults.Count > 0;
            frameCount = Time.frameCount;
            return inputOverUI;
        }
    }
}