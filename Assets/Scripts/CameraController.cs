using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class CameraController: MonoBehaviour
{

    public List<Camera> cameras;

    private int nextCam = -1;
    private InputAction camSwitchAction; 
    void Start()
    {
        //cameras = new List<Camera>();
        //cameras.AddRange(Resources.FindObjectsOfTypeAll(typeof(Camera)));
        //for (int i = 0; i < cameras.Count; i++)
        //{
        //    Debug.Log(cameras[i].name);
        //    if (cameras[i].GetInstanceID() == defaultCam.GetInstanceID())
        //        { nextCam = i-1; }
        //}

        camSwitchAction = InputSystem.actions.FindAction("SwitchCam");
    }

    public void Update()
    {
        if (camSwitchAction.WasReleasedThisFrame()) OnCamSwitch();
    }

    public void OnCamSwitch()
    {

        nextCam += 1;
        if (nextCam >= cameras.Count) { nextCam = 0; }

        foreach (Camera c in cameras) { c.enabled = false; } // we don't have that many cameras anyways
        cameras[nextCam].enabled = true;
    }
}

