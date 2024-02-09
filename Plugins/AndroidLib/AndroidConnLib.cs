using RegawMOD.Android;

namespace AndroidLib.Unity
{
    public sealed class AndroidConnLib
    {
        public static AndroidConnLib Instance { get; private set; } = new AndroidConnLib();

        public readonly AndroidConnectionManager androidConnectionManager; 
        public AndroidController androidController { get; private set; }

        private AndroidConnLib()
        {
            androidConnectionManager = new AndroidConnectionManager();
            androidController = AndroidController.Instance;
        }

    }
}