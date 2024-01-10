namespace AndroidLib.Unity
{
    public class AndroidConnLib
    {
        public static AndroidConnLib Instance { get; private set; } = new AndroidConnLib();

        public AndroidConnectionManager androidConnectionManager; 

        public AndroidConnLib()
        {
            androidConnectionManager = new AndroidConnectionManager();
        }
    }
}