using ProjectT.Core;

namespace ProjectT.Systems.Scene
{
    public class SceneController : Singleton<SceneController>
    {
        public string SceneTransitionName { get; private set; }
        public void SetTransitionName(string sceneTransitionName)
        {
            this.SceneTransitionName = sceneTransitionName;
        }
    }
}
