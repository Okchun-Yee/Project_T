using Cinemachine;
using ProjectT.Core;
namespace ProjectT.Systems.Camera
{
    public class ScreenShakeManager : Singleton<ScreenShakeManager>
    {
        private CinemachineImpulseSource source;
        protected override void Awake()
        {
            base.Awake();
            source = GetComponent<CinemachineImpulseSource>();
        }
        public void ShakeScreen()
        {
            source.GenerateImpulse();
        }
    }
}
