using System;
using System.IO;
using System.Net;
using System.Net.Http;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Networking;
using UnityEngine.Rendering;
using UnityEngine.VFX;

public class RealCamprocesing : MonoBehaviour
{
    public GameEventHandler global;
    public Camera referenceCamera = null;
    public Camera targetCamera;
    public float uploadFrequencyHz = 10f;
    public float fx;
    public float cx;
    public float fy;
    public float cy;

    public RenderTexture captureTexture;

    private int width, height;
    private Texture2D destTexture;
    private Rect readRegion;

    private bool uploading = false;
    private float timePassed = 0f;

    private UnityWebRequestAsyncOperation uploadCurOP; 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        width = captureTexture.width; height = captureTexture.height;

        targetCamera.enabled = false;
        targetCamera.usePhysicalProperties = true;
        configureCameraToRealMatrix(targetCamera);       

        if (referenceCamera != null)
        {
            referenceCamera.usePhysicalProperties = true;
            configureCameraToRealMatrix(referenceCamera);
        }

        //RenderPipelineManager.endCameraRendering += OnPostRenderHook;
        targetCamera.targetTexture = captureTexture;
        destTexture = new Texture2D(width, height, TextureFormat.RGB24, false);
        readRegion = new Rect(0, 0, width, height);
    }

    //from https://discussions.unity.com/t/how-to-use-opencv-camera-calibration-to-set-physical-camera-parameters/749237/2
    void configureCameraToRealMatrix(Camera camera)
    {
        float f = camera.focalLength;

        float sizeX = f * width / fx;
        float sizeY = f * height / fy;

        camera.sensorSize = new Vector2(sizeX, sizeY);

        float shiftX = -(cx - width / 2.0f) / width;
        float shiftY = (cy - height / 2.0f) / height;
        camera.lensShift = new Vector2(shiftX, shiftY);
    }

    // Update is called once per frame
    async void Update()
    {
        if (uploading) 
        {
            timePassed += Time.deltaTime;
            if ((timePassed > 1/uploadFrequencyHz) &&
                (uploadCurOP == null || uploadCurOP.isDone))
            {
                if (uploadCurOP != null) Debug.Log(uploadCurOP.webRequest.result);

                startUploadCapture(await CaptureFrame());
                timePassed = 0;
            } 
        }
    }

    void startUploadCapture(byte[] byteArr)
    {
        if (byteArr == null ||  byteArr.Length == 0) return;

        var req = UnityWebRequest.Put($"http://{global.apiServerAddress}/api/cam/upload", byteArr);
        req.timeout = 1;

        uploadCurOP = req.SendWebRequest();
    }

    void saveCapture(byte[] byteArr)
    {
        var path = Path.GetFullPath(global.cameraCaptureSaveFile);
        var f = File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        f.Write(byteArr, 0, byteArr.Length);
        f.Flush();
        f.Close();


        Debug.Log($"Saved to {path}");
    }

    public async Awaitable<byte[]> CaptureFrame()
    {
        await Awaitable.EndOfFrameAsync();

        var currentRt = RenderTexture.active;
        targetCamera.Render();
        RenderTexture.active = captureTexture;

        destTexture.ReadPixels(readRegion, 0, 0);
        RenderTexture.active = currentRt;


        byte[] pngArr = destTexture.EncodeToPNG();

        return pngArr;
    }

    public void ToggleUpload()
    {
        uploading = !uploading;
        timePassed = 0;
    }

    public async void CaptureAndSave()
    {
        saveCapture(await CaptureFrame());
    }
}
