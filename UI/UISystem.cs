using System;
using Commands;
using Components;
using System.Linq;
using UnityEngine;
using HECSFramework.Core;
using HECSFramework.Unity;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Systems
{
    [Serializable, BluePrint]
    [Documentation(Doc.UI, Doc.HECS, "This system default for operating ui at hecs, this system have command for show and hide ui plus show or hide ui groups, this system still in progress")]
    public class UISystem : BaseSystem, IUISystem, IGlobalStart
    {
        public const string UIBluePrints = "UIBluePrints";

        private Queue<ShowUICommand> commandsQueue = new Queue<ShowUICommand>();

        private EntitiesFilter uiCurrents;
        private EntitiesFilter additionalCanvases;

        private UnityTransformComponent mainCanvasTransform;
        private List<UIBluePrint> uIBluePrints = new List<UIBluePrint>();
        private PoolingSystem poolingSystem;

        private bool isReady;
        private bool isLoaded;

        private List<UIBluePrint> spawnInProgress = new List<UIBluePrint>();

        public override void InitSystem()
        {
            uiCurrents = Owner.World.GetFilter<UITagComponent>();
            additionalCanvases = Owner.World.GetFilter<AdditionalCanvasTagComponent>();
            Addressables.LoadAssetsAsync<UIBluePrint>(UIBluePrints, null).Completed += LoadReact;
        }

        public void GlobalStart()
        {
            poolingSystem = Owner.World.GetSingleSystem<PoolingSystem>();

            if (Owner.World.TryGetSingleComponent(out MainCanvasTagComponent mainCanvasTagComponent))
            {
                isReady = true;
                mainCanvasTransform = mainCanvasTagComponent.Owner.GetOrAddComponent<UnityTransformComponent>();
            }
        }

        private void LoadReact(AsyncOperationHandle<IList<UIBluePrint>> obj)
        {
            foreach (var bp in obj.Result)
                uIBluePrints.Add(bp);

            isLoaded = true;
        }

        public async void CommandGlobalReact(ShowUICommand command)
        {
            if (!isLoaded || !isReady)
                await UniTask.WaitUntil(() => isReady && isLoaded, PlayerLoopTiming.LastEarlyUpdate);

            if (!command.MultyView)
            {
                foreach (var ui in uiCurrents)
                {
                    if (ui == null || !ui.IsAlive)
                        continue;

                    var uiTag = ui.GetComponent<UITagComponent>();

                    if (uiTag.ViewType.Id == command.UIViewType)
                    {
                        uiTag.Owner.Command(command);
                        command.OnUILoad?.Invoke(uiTag.Owner);
                        return;
                    }
                }
            }

            var spawn = uIBluePrints.FirstOrDefault(x => x.UIType.Id == command.UIViewType);

            if (spawn == null)
            {
                Debug.LogAssertion("Cannot find UIBluePrint for " + command.UIViewType);
                return;
            }

            SpawnUIFromBluePrint(spawn, command.OnUILoad, mainCanvasTransform.Transform);
        }

        private void SpawnUIFromBluePrint(UIBluePrint spawn, Action<Entity> action, Transform mainTransform)
        {
            if (spawn.AdditionalCanvasIdentifier != null)
            {
                var neededCanvas = Owner.World.GetFilter<AdditionalCanvasTagComponent>()
                     .FirstOrDefault(x => x.GetComponent<AdditionalCanvasTagComponent>()
                         .AdditionalCanvasIdentifier.Id == spawn.AdditionalCanvasIdentifier.Id);

                if (neededCanvas != null)
                    mainTransform = neededCanvas.GetOrAddComponent<UnityTransformComponent>().Transform;
            }

            Addressables.InstantiateAsync(spawn.UIActor, mainTransform).Completed += a => LoadUI(a, action);
        }

        public async UniTask<Entity> ShowUI(int uiType, bool isMultiple = false, int additionalCanvas = 0, bool needInit = true, bool ispoolable = false)
        {
            if (!isLoaded || !isReady)
                await UniTask.WaitUntil(() => isReady && isLoaded, PlayerLoopTiming.LastEarlyUpdate);

            if (!isMultiple)
            {
                if (TryGetFromCurrentUI(uiType, out var ui))
                    return ui;
            }

            Transform canvas = null;

            if (additionalCanvas == 0)
                canvas = mainCanvasTransform.Transform;
            else
            {
                var needCanvas = additionalCanvases
                    .FirstOrDefault(x => x.GetComponent<AdditionalCanvasTagComponent>().AdditionalCanvasIdentifier.Id == additionalCanvas);

                if (needCanvas == null)
                    throw new Exception("We dont have additional canvas " + additionalCanvas);

                canvas = needCanvas.GetComponent<UnityTransformComponent>().Transform;
            }

            var bluePrint = GetUIBluePrint(uiType);

            if (bluePrint == null)
                throw new Exception("we dont have blue print for this ui " + uiType);

            if (ispoolable)
            {
                var container = await poolingSystem.GetEntityContainerFromPool(bluePrint.Container);
                var uiActorFromPool = await poolingSystem.GetActorFromPool<UIActor>(container);
                uiActorFromPool.Entity.Init();
                uiActorFromPool.GetHECSComponent<UnityTransformComponent>().Transform.SetParent(canvas);
                return uiActorFromPool.Entity;
            }

            var newUIactorPrfb = await Addressables.LoadAssetAsync<GameObject>(bluePrint.UIActor).Task;
            var newUiActor = MonoBehaviour.Instantiate(newUIactorPrfb, canvas).GetComponent<UIActor>();

            if (needInit)
                newUiActor.Init();
            else
                newUiActor.InitActorWithoutEntity();

            newUiActor.ActorContainer.Init(newUiActor.Entity);

            newUiActor.transform.SetParent(canvas);
            return newUiActor.Entity;
        }

        private UIBluePrint GetUIBluePrint(int uiType)
        {
            return uIBluePrints.FirstOrDefault(x => x.UIType.Id == uiType);
        }

        private bool TryGetFromCurrentUI(int uiType, out Entity uiEntity)
        {
            uiEntity = null;

            foreach (var ui in uiCurrents)
            {
                if (ui == null || !ui.IsAlive)
                    continue;

                var uiTag = ui.GetComponent<UITagComponent>();

                if (uiTag.ViewType.Id == uiType)
                {
                    uiEntity = uiTag.Owner;
                    return true;
                }
            }

            return false;
        }

        private void LoadUI(AsyncOperationHandle<GameObject> obj, Action<Entity> onUILoad)
        {
            if (obj.Result.TryGetComponent<UIActor>(out var actor))
            {
                actor.InitWithContainer();
                actor.Command(new ShowUICommand());
                onUILoad?.Invoke(actor.Entity);
            }
            else
                Debug.LogAssertion("this is not UIActor " + obj.Result.name);
        }

        public async void CommandGlobalReact(HideUICommand command)
        {
            if (!isLoaded || !isReady)
                await UniTask.WaitUntil(() => isReady && isLoaded, PlayerLoopTiming.LastEarlyUpdate);

            uiCurrents.ForceUpdateFilter();
            foreach (var ui in uiCurrents)
            {
                if (ui == null || !ui.IsAlive)
                    continue;

                var uiTag = ui.GetComponent<UITagComponent>();

                if (uiTag.ViewType.Id == command.UIViewType)
                {
                    uiTag.Owner.Command(command);
                    return;
                }
            }
        }

        public void CommandGlobalReact(CanvasReadyCommand command)
        {
            if (EntityManager.TryGetSingleComponent<MainCanvasTagComponent>(Owner.WorldId, out var canvas))
            {
                if (!canvas.Owner.TryGetComponent(out mainCanvasTransform))
                    Debug.LogAssertion("we dont have unity transform on main canvas");
            }
            else
                Debug.LogAssertion("we dont have main canvas");

            isReady = true;

        }

        private void HideGroup(int groupID)
        {
            uiCurrents.ForceUpdateFilter();

            foreach (var ui in uiCurrents)
            {
                if (ui.TryGetComponent(out UIGroupTagComponent uIGroupTagComponent))
                {
                    if (uIGroupTagComponent.IsHaveGroupIndex(groupID))
                        uIGroupTagComponent.Owner.Command(new HideUICommand());
                }
            }
        }

        private void ShowGroup(int groupID)
        {
            uiCurrents.ForceUpdateFilter();

            foreach (var ui in uiCurrents)
            {
                if (ui.TryGetComponent(out UIGroupTagComponent uIGroupTagComponent))
                {
                    if (uIGroupTagComponent.IsHaveGroupIndex(groupID))
                        continue;
                    else
                        ui.Command(new HideUICommand());
                }
                else
                    ui.Command(new HideUICommand());
            }
        }

        private bool IsCurrentUIContainsId(int id)
        {
            foreach (var ui in uiCurrents)
            {
                if (ui.GetComponent<UITagComponent>().ViewType.Id == id)
                    return true;
            }

            return false;
        }
    }

    public interface IUISystem : ISystem,
        IReactGlobalCommand<ShowUICommand>,
        IReactGlobalCommand<HideUICommand>,
        IReactGlobalCommand<CanvasReadyCommand>
    { }
}