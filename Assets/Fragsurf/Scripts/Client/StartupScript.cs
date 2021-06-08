using Fragsurf.Shared;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fragsurf.Client
{
    public class StartupScript : MonoBehaviour
    {

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            if (Structure.DedicatedServer)
            {
                SceneManager.LoadScene(GameData.Instance.MainServer.ScenePath);
            }
            else
            {
                if (!GameObject.FindObjectOfType<PlayTest>())
                {
                    SceneManager.LoadScene(GameData.Instance.MainMenu.ScenePath);
                }
            }
        }

    }
}

