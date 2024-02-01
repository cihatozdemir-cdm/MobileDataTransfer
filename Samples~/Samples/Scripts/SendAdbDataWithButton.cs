using RegawMOD.Android;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cdm.MobileDataTransfer.Samples
{
    public class SendAdbDataWithButton : MonoBehaviour
    {
        [SerializeField] private TMP_InputField commandInputField;
        [SerializeField] private Button sendCommandButton;
        private void Start()
        {
            sendCommandButton.onClick.AddListener(SendAdbData);
        }

        private void SendAdbData()
        {
            var output = Adb.ExecuteAdbCommand(Adb.FormAdbCommand(commandInputField.text));
            Debug.Log(output);
        }
    }
}
