using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

namespace AssetsManagement.Containers
{
    public class AssetRefContainer<TRef, TObject> : IAssetContainer<TObject>
        where TRef : AssetReference
        where TObject : Object
    {
        protected readonly TObject asset;
        protected readonly TRef reference;

        private readonly List<GameObject> instances = new();

        public int RefsCount => instances.Count;
        public TObject Asset => asset;
        public TRef Reference => reference;

        public async UniTask<GameObject> CreateInstance(Vector3 pos, Quaternion rot, Transform parent = null, CancellationToken token = default)
        {
            var instanceHandle = Addressables.InstantiateAsync(reference, pos, rot, parent);
            var instance = await instanceHandle.WithCancellation(token);
            instances.Add(instance);
            return instance;
        }

        public async UniTask<TComponent> CreateInstanceForComponent<TComponent>(Vector3 pos = default,
            Quaternion rot = default,
            Transform parent = null,
            CancellationToken token = default) where TComponent : Component
        {
            var instance = await CreateInstance(pos, rot, parent, token);
            return instance != null ? instance.GetComponent<TComponent>() : null;
        }

        public void ReleaseInstance(GameObject instance)
        {
            if (instances.Remove(instance))
                Addressables.ReleaseInstance(instance);
        }


        public AssetRefContainer(TRef loadedReference)
        {
            asset = loadedReference.Asset as TObject;
            reference = loadedReference;
        }
    }
}