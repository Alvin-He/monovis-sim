using UnityEngine;

public class GameEventHandler : MonoBehaviour
{
    public string apiServerAddress = "127.0.0.1:8081";
    public string cameraCaptureSaveFile = "./capture";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        apiServerAddress = apiServerAddress.Trim();
        cameraCaptureSaveFile = cameraCaptureSaveFile.Trim();
    }

    public void OnExit()
    {
        Debug.Log("exiting");
        Application.Quit();
    }

    public void UpdateAPIServerAddress(string n) { apiServerAddress = n.Trim(); }
    public void UpdateCameraCaptureSaveFile(string n) { cameraCaptureSaveFile = n.Trim(); }

}
