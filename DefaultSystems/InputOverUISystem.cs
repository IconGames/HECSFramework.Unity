using System;
using System.Collections.Generic;
using Components;
using HECSFramework.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Systems
{
    [Serializable][Documentation(Doc.HECS, Doc.Input, "this system update InputOverUIComponent")]
    public sealed class InputOverUISystem : BaseSystem, IPriorityUpdatable 
    {
        [Required]
        public InputOverUIComponent InputOverUIComponent;

        private PointerEventData pointerEventData;
        private List<RaycastResult> raycastResults = new List<RaycastResult>(3);
        private InputAction touchAction;

        public int Priority { get; } = -50;

        public override void InitSystem()
        {
            pointerEventData  = new PointerEventData(EventSystem.current);
            EntityManager.Default.GetSingleComponent<InputActionsComponent>()
                .TryGetInputAction(InputOverUIComponent.TouchPositionInputIdentifier.name, out touchAction);
        }

        public void PriorityUpdateLocal()
        {
            pointerEventData.position = touchAction.ReadValue<Vector2>();
            EventSystem.current.RaycastAll(pointerEventData, raycastResults);
            InputOverUIComponent.InputOverUI = raycastResults.Count > 0;
        }
    }
}