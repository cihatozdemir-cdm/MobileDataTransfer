using System.Threading;
using RegawMOD.Android;

namespace AndroidLib.Unity
{
    public class AndroidConnLib
    {
        public static AndroidConnLib Instance { get; private set; } = new AndroidConnLib();

        public AndroidConnectionManager androidConnectionManager; 
        public AndroidController androidController { get; private set; }
        //Check libraries setup is completed
        public bool IsReady { get; private set; }

        private AndroidConnLib()
        {
            IsReady = false;
            
            androidConnectionManager = new AndroidConnectionManager();
            var thread = new Thread(StartInThread);
            thread.Start();
        }

        private void StartInThread()
        {
            androidController = AndroidController.Instance;

            IsReady = true;
        }
    }
}