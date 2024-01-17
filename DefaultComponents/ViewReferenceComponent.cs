using HECSFramework.Core;
using HECSFramework.Unity;
using HECSFramework.Unity.Helpers;
using System;
using Commands;

namespace Components
{
    [Serializable, BluePrint]
    public partial class ViewReferenceComponent : BaseComponent
    {
        public ActorViewReference ViewReference;

        public override void BeforeDispose()
        {
            base.BeforeDispose();
            if (!EntityManager.Default.TryGetSingleComponent(out OnApplicationQuitTagComponent onApplicationQuitTagComponent))
            {
                EntityManager.Default.Command(new ActorViewDisposedCommand { Actor = Owner.AsActor() });
            }
        }
    }

    [Serializable]
    public class ActorViewReference : ComponentReference<Actor>
    {
        public ActorViewReference(string guid) : base(guid)
        {
        }
    }
}