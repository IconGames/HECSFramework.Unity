﻿using HECSFramework.Core.Generator;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace HECSFramework.Unity.Generator
{
    public partial class UnityProcessGeneration
    {
        public const string BluePrint = "BluePrint";

        private const string DefaultPath = "/Scripts/HECSGenerated/";
        private const string ComponentsBluePrintsPath = "/Scripts/BluePrints/ComponentsBluePrints/";
        private const string SystemsBluePrintsPath = "/Scripts/BluePrints/SystemsBluePrint/";
        private string dataPath = Application.dataPath;

        private const string TypeProvider = "TypeProvider.cs";
        private const string MaskProvider = "MaskProvider.cs";
        private const string HecsMasks = "HECSMasks.cs";
        private const string SystemBindings = "SystemBindings.cs";
        private const string ComponentContext = "ComponentContext.cs";
        private const string BluePrintsProvider = "BluePrintsProvider.cs";
        private const string Documentation = "Documentation.cs";
        private const string MapResolver = "MapResolver.cs";


        [MenuItem("HECS Options/HECS Codogen")]
        public static void UnityCodogen()
        {
            var generator = new CodeGenerator();
            var unityProcessGeneration = new UnityProcessGeneration();
            generator.GatherAssembly();
            unityProcessGeneration.SaveToFile(TypeProvider, generator.GenerateTypesMap());
            unityProcessGeneration.SaveToFile(MaskProvider, generator.GenerateMaskProvider());
            unityProcessGeneration.SaveToFile(SystemBindings, generator.GetSystemBinds());
            unityProcessGeneration.SaveToFile(ComponentContext, generator.GetComponentContext());
            unityProcessGeneration.SaveToFile(HecsMasks, generator.GenerateHecsMasks());
            unityProcessGeneration.SaveToFile(Documentation, generator.GetDocumentation());


            //generate blue prints
            var componetsBPFiles = generator.GenerateComponentsBluePrints();
            var systemsBPFiles = generator.GenerateSystemsBluePrints();
            var resolvers = generator.GetSerializationResolvers();
            unityProcessGeneration.SaveToFile(MapResolver, generator.GetResolverMap());

            foreach (var c in componetsBPFiles)
                unityProcessGeneration.SaveToFile(c.name, c.classBody, ComponentsBluePrintsPath);

            foreach (var c in systemsBPFiles)
                unityProcessGeneration.SaveToFile(c.name, c.classBody, SystemsBluePrintsPath);

            foreach (var c in resolvers)
                unityProcessGeneration.SaveToFile(c.name, c.content, DefaultPath + "Resolvers/");

            unityProcessGeneration.SaveToFile(BluePrintsProvider, generator.GetBluePrintsProvider(), needToImport: true);
            unityProcessGeneration.GenerateNetworkStaff();
        }

        partial void GenerateNetworkStaff();

        private void SaveToFile(string name, string data, string pathToDirectory = DefaultPath, bool needToImport = false)
        {
            var path = dataPath + pathToDirectory + name;

            try
            {
                if (!Directory.Exists(Application.dataPath + pathToDirectory))
                    Directory.CreateDirectory(Application.dataPath + pathToDirectory);

                File.WriteAllText(path, data);

                var sourceFile2 = path.Replace(Application.dataPath, "Assets");

                if (needToImport)
                    AssetDatabase.ImportAsset(sourceFile2);
            }
            catch
            {
                Debug.LogError("не смогли ослить " + pathToDirectory);
            }
        }
    }
}
