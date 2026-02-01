using UnityEngine;
using ProjectT.Systems.GameMode;

namespace ProjectT.Systems.Scene
{
    public class MainMenu : MonoBehaviour
    {
        const string TOWN_TEXT = "GameScene1";

        public void Play()
        {
            Debug.Log("Play Button Clicked");
            SceneTransitionExecution.Instance?.Request(
                new SceneTransitionRequest(TOWN_TEXT, targetGameMode: GameModeList.Town));
        }
        public void Quit()
        {
            Debug.Log("Quit Button Clicked");
            Application.Quit();
        }
    }
}
