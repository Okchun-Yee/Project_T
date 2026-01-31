using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectT.Systems.Scene
{
    public class MainMenu : MonoBehaviour
    {
        const string TOWN_TEXT = "GameScene1";

        public void Play()
        {
            Debug.Log("Play Button Clicked");
            SceneManager.LoadScene(TOWN_TEXT);
        }
        public void Quit()
        {
            Debug.Log("Quit Button Clicked");
            Application.Quit();
        }
    }
}
