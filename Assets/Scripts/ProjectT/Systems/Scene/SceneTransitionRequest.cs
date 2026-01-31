using ProjectT.Systems.GameMode;

namespace ProjectT.Systems.Scene
{
    public struct SceneTransitionRequest
    {
        public string sceneName;
        public string entryId;
        public GameMode targetGameMode;
        public bool useFade;
        public bool resetTimeScale;

        public SceneTransitionRequest(string sceneName, string entryId = null, GameMode targetGameMode = GameMode.Gameplay,
            bool useFade = true, bool resetTimeScale = true)
        {
            this.sceneName = sceneName;
            this.entryId = entryId;
            this.targetGameMode = targetGameMode;
            this.useFade = useFade;
            this.resetTimeScale = resetTimeScale;
        }
    }
}
