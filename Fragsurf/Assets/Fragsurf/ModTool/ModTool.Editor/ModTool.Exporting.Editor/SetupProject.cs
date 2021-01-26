using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager;
using System.Threading.Tasks;
using UnityEngine.Rendering;
using System;
using System.Collections.Generic;
using UnityEditor.PackageManager.Requests;
using System.Linq;

namespace ModTool.Editor 
{
    [InitializeOnLoad]
    public class SetupProject
    {

        private static bool _initializing;
        private static ListRequest _listRequest;
        private static AddRequest _addRequest;
        private static List<string> _funFacts = new List<string>()
        {
            "The heads on Easter Island have bodies",
            "The moon has moonquakes",
            "Goosebumps are meant to ward off predators",
            "Pineapple works as a natural meat tenderizer",
            "Humans are the only animals that blush",
            "The feeling of getting lost inside a mall is known as the Gruen transfer",
            "The wood frog can hold its pee for up to eight months",
            "The hottest spot on the planet is in Libya",
            "You lose up to 30 percent of your taste buds during flight",
            "Your nostrils work one at a time",
            "Only two mammals like spicy food: humans and the tree shrew",
            "Rabbits can't puke",
            "The \"M's\" in M&Ms stand for \"Mars\" and \"Murrie.\"",
            "Cotton candy was invented by a dentist",
            "The English word with the most definitions is \"set.\"",
            "Creedence Clearwater Revival has the most No. 2 Billboard hits—without ever hitting No. 1",
            "Pigeons can tell the difference between a painting by Monet and Picasso",
            "The dot over the lower case \"i\" or \"j\" is known as a \"tittle.\"",
            "Chewing gum boosts concentration",
            "Superman didn't always fly",
            "The first computer was invented in the 1940s",
            "Space smells like seared steak",
            "The longest wedding veil was the same length as 63.5 football fields",
            "The unicorn is the national animal of Scotland",
            "Bees sometimes sting other bees",
            "Kids ask 300 questions a day",
            "The total weight of ants on earth once equaled the total weight of people",
            "\"E\" is the most common letter and appears in 11 percent of all english words",
            "A dozen skeletons were once found in Benjamin Franklin's basement",
            "The healthiest place in the world is in Panama",
            "A pharaoh once lathered his slaves in honey to keep bugs away from him",
            "Some people have an extra bone in their knee (and it's getting more common)",
            "Pringles aren't actually potato chips",
            "There's a giant fish with a transparent head",
            "There's a decorated war hero dog"
        };

        static SetupProject()
        {
            Initialize();
        }

        [MenuItem("Fragsurf/Setup Project", priority = 30)]
        public static void InitializeProject()
        {
            Initialize();
        }

        private static async void Initialize()
        {
            if (_initializing)
            {
                return;
            }

            _initializing = true;

            var list = Client.List();
            while (!list.IsCompleted)
            {
                EditorUtility.DisplayProgressBar("Setting Up", "Checking for URP package", UnityEngine.Random.Range(0, 1f));
                await Task.Delay(100);
            }

            if(list.Status == StatusCode.Failure)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogError("Failed to add Universal Render Pipeline package, navigate to Window -> Package Manager and add it manually!");
                _initializing = false;
                return;
            }

            var urp = list.Result.FirstOrDefault(x => x.name == "com.unity.render-pipelines.universal");
            if(urp == null)
            {
                var add = Client.Add("com.unity.render-pipelines.universal@10.2.2");
                while (!add.IsCompleted)
                {
                    EditorUtility.DisplayProgressBar("Setting Up", "Adding URP package", UnityEngine.Random.Range(0, 1f));
                    await Task.Delay(100);
                }
                if(add.Status == StatusCode.Failure)
                {
                    EditorUtility.ClearProgressBar();
                    _initializing = false;
                    Debug.LogError("Failed to add Universal Render Pipeline package, navigate to Window -> Package Manager and add it manually!");
                    return;
                }
                await Task.Delay(500);
            }

            if(GraphicsSettings.renderPipelineAsset == null)
            {
                EditorUtility.DisplayProgressBar("Setting Up", "Assigning render pipeline asset", UnityEngine.Random.Range(0, 1f));

                var urpAsset = Resources.Load<RenderPipelineAsset>("UniversalRenderPipelineAsset");
                if (!urpAsset)
                {
                    EditorUtility.ClearProgressBar();
                    _initializing = false;
                    Debug.LogError("Couldn't find Universal Render Pipeline asset.  You need to create one manually!");
                    return;
                }

                GraphicsSettings.renderPipelineAsset = urpAsset;
            }

            EditorUtility.ClearProgressBar();
            Debug.ClearDeveloperConsole();

            //EditorUtility.DisplayDialog("Random Fact", _funFacts[UnityEngine.Random.Range(0, _funFacts.Count)], "Ok");

            _initializing = false;
        }

    }
}
