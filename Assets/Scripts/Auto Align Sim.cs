using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

public class AutoAlignSim : MonoBehaviour
{
   [Serializable]
   public struct PossibleTestRange
    {
        public int tagId;
        public GameObject tagPosObj;
        public float targetR;
        public float xLower;
        public float xUpper;
        public float yLower;
        public float yUpper;
        public float rLower;
        public float rUpper;
    }

    public GameEventHandler global;
    public Movement movement;
    public GameObject robot;
    public GameObject posReferencePoint; // rerference point for robot x, z (note: rotation is still directly claculated from robot)
    public List<PossibleTestRange> testRanges;

    private int currentTickId = -1;
    private int currentTargetID = -1;
    private float currentTargetX = 0;
    private float currentTargetY = 0;
    private float currentTargetR = 0;


    private struct TickInput
    {
        public int tick_id;
        public bool reset;
        public float forward;
        public float strafe;
        public float rotation;
    }
    private struct TickResult
    {
        public int tick_id;
        public float xOff;
        public float yOff;
        public float rOff;
        public float robotYaw;
    }
    private struct ResetFinish
    {
        public int tick_id;
        public int targetID;
    }

    private float AngleCR(float a)
    {
        a = a % 360;
        if (a < 0)
        {
            if (a < -180) return a + 360;
        } else
        {
            if (a > 180) return a - 360;
        }
        return a;
    }

    private async Awaitable UploadTickResult()
    {
        TickResult result = new()
        {
            tick_id = currentTickId,
            robotYaw = -robot.transform.rotation.eulerAngles.y + 360f,
            xOff = currentTargetX - posReferencePoint.transform.position.x,
            yOff = currentTargetY - posReferencePoint.transform.position.z,
            rOff = AngleCR(currentTargetR - (-robot.transform.rotation.eulerAngles.y + 360f))
        };
        var req = UnityWebRequest.Post($"http://{global.apiServerAddress}/api/sim/tick_result", JsonUtility.ToJson(result), "application/json");
        await req.SendWebRequest();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (posReferencePoint == null) posReferencePoint = robot;
    }

    private async Awaitable SignalResetFinish()
    {
        // upload resetted starting position
        await UploadTickResult();

        ResetFinish res = new ResetFinish()
        {
            tick_id = currentTickId,
            targetID = currentTargetID
        };
        var req = UnityWebRequest.Post($"http://{global.apiServerAddress}/api/sim/reset_finish", JsonUtility.ToJson(res), "application/json");
        await req.SendWebRequest();
    }

    public void ActivateOrDisableSim(Single state)
    {
        if (state == 1f)
        {
            movement.disableControllerInput = true;
        }
        else
        {
            movement.disableControllerInput = false;
        }
    }

    public async Awaitable ResetSim()
    {
        // reset to random tag and position
        PossibleTestRange tag = testRanges[Random.Range(0, testRanges.Count)];
        robot.transform.position = new Vector3(
            Random.Range(tag.xLower, tag.xUpper),
            0,
            Random.Range(tag.yLower, tag.yUpper)
            );
        robot.transform.rotation = Quaternion.Euler(0, -Random.Range(tag.rLower, tag.rUpper), 0);

        currentTargetID = tag.tagId;
        currentTargetX = tag.tagPosObj.transform.position.x;
        currentTargetY = tag.tagPosObj.transform.position.z;
        currentTargetR = tag.targetR;

        await SignalResetFinish();
    }
    public async void QuickResetSim()
    {
        await ResetSim();
    }

    public void Move(float forward, float strafe, float rotation)
    {
        movement.RobotRelativeMove(forward, strafe, rotation);   
    }

    // Update is called once per frame
    async void FixedUpdate()
    {
        var req = UnityWebRequest.Get($"http://{global.apiServerAddress}/api/sim/tick_update");
        await req.SendWebRequest();
        if (req.responseCode != 200) return; 
        var resObj = JsonUtility.FromJson<TickInput>(req.downloadHandler.text);

        // check if new tick
        if (currentTickId == resObj.tick_id)
        {
            //Move(0f, 0f, 0f); // needed untill physics sim is figured out so we can sim coasting
            return;
        }; 
        currentTickId = resObj.tick_id;
        //print($"Tick: {currentTickId}, with input: {JsonUtility.ToJson(resObj)}");

        if (resObj.reset)
        {
            await ResetSim();
            return;
        }
        Move(resObj.forward, resObj.strafe, resObj.rotation);
    
        await UploadTickResult();
    }
}
