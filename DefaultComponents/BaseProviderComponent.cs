using HECSFramework.Core;
using HECSFramework.Unity;
using System;
using UnityEngine;

namespace Components
{
    [Serializable]
    [Documentation(Doc.Provider, "its base component for providing monobehaviours to ecs")]
    public abstract class BaseProviderComponent<T> : BaseComponent, IHaveActor, IAfterEntityInit, IInitAferView where T : MonoBehaviour
    {
        public T Get;

        public Actor Actor { get; set; }

        public void AfterEntityInit()
        {
            if (Owner.ContainsMask<ViewReferenceGameObjectComponent>()
                && !Owner.ContainsMask<ViewReadyTagComponent>())
                return;

            SetGet();
        }

        public void InitAferView()
        {
            SetGet();
        }

        private void SetGet()
        {
            Actor.TryGetComponent(out Get, true);
        }
    }
}