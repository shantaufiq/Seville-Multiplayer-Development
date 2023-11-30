using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Seville
{
    public class EnvNavigation : MonoBehaviour
    {
        public void OnClickChangeEnvirontment(int index)
        {
            EnvironmentManager.Instance.ChangeAreaByIndex(index);
        }

        public void ChangeScene(int sceneId)
        {
            SceneManager.LoadScene(sceneId);
        }
    }
}