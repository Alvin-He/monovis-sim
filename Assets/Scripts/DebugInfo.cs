using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;


public class DebugInfo : MonoBehaviour
{
    public GameEventHandler global;
    public Toggle debugToggle;
    public GameObject panel;
    public TextMeshProUGUI positionField;
    public TextMeshProUGUI rotationField;

    public GameObject robot;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        panel.SetActive(false);
        debugToggle.SetIsOnWithoutNotify(false);
        debugToggle.onValueChanged.AddListener(OnToggle);
    }
    
    public void OnToggle(bool toggle)
    {
        panel.SetActive(toggle);
    }

    public void UpdatePosition()
    {
        var pos = robot.transform.position;
        var text = $"X: {pos.x:F4}; Y: {pos.z:F4}; Z: {pos.y:F4}";
        positionField.SetText(text); 
    }

    public void UpdateRotation()
    {
        var rot = robot.transform.rotation;
        var text = $"{-rot.eulerAngles.y + 360f,3:F4} degrees"; 
        rotationField.SetText(text);
    }

    private struct SimulationPosePacket { 
        public float x, y, r; 
        public SimulationPosePacket(float x, float y, float r)
        {
            this.x = x; this.y = y; this.r = r;
        }
    }
    private float timeSinceLastUpload = 0f;
    private UnityWebRequestAsyncOperation lastUploadOp = null; 
    public async void UploadCurrentSimulationPose()
    {
        if (timeSinceLastUpload < 1f/50f)
        {
            timeSinceLastUpload += Time.deltaTime;
            return;
        }
        timeSinceLastUpload = 0f;
        if (lastUploadOp != null) await lastUploadOp;

        var packet = new SimulationPosePacket(
            robot.transform.position.x, 
            robot.transform.position.z, 
            -robot.transform.rotation.eulerAngles.y
            );

        var req = new UnityWebRequest($"http://{global.apiServerAddress}/api/simulation-pose", UnityWebRequest.kHttpVerbPUT);
        req.uploadHandler = new UploadHandlerRaw(new UTF8Encoding().GetBytes(JsonUtility.ToJson(packet)));
        req.SetRequestHeader("Content-Type", "application/json");
        req.timeout = 1;

        lastUploadOp = req.SendWebRequest();
    }
    // Update is called once per frame
    void Update()
    {
        UploadCurrentSimulationPose();
        if (debugToggle.isOn)
        {
            UpdatePosition();
            UpdateRotation();
        }; 


    }
}
