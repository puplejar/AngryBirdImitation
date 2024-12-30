using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
public class CameraController : MonoBehaviour
{

    public Camera mainCam;
    public Transform playerPivot;
    public CinemachineBrain cinemachineBrain;

    public Transform cameraGroup;
    public Dictionary<string, CinemachineVirtualCamera> camDic = new Dictionary<string, CinemachineVirtualCamera>();
    private Defines.CamType _camType = Defines.CamType.ThirdPerson;

    private float verticalAxis = 0;
    private float horizonAxis = 0;

    public Defines.CamType camType { set { _camType = value; ActivatingCamera(); } }


    private void Awake()
    {
        mainCam = Camera.main;
        cinemachineBrain = gameObject.GetComponent<CinemachineBrain>();
    }
    // Start is called before the first frame update
    void Start()
    {
        Init();
    }



    public void Init()
    {
        

        if (!playerPivot)
        {
            playerPivot = GameObject.Find("PlayerPivot").transform;
        }
        if (!cameraGroup)
        {
            cameraGroup = GameObject.Find("CameraGroup").transform;
        }

        //ī�޶� �׷� ĳ��
        if(cameraGroup.name == "CameraGroup")
        {
            foreach(Transform go in cameraGroup)
            {
                CinemachineVirtualCamera cam = go.GetComponent<CinemachineVirtualCamera>();
                if (cam)
                {
                    camDic.Add(cam.name, cam);
                    go.gameObject.SetActive(false);
                }
            }
            cameraGroup.GetChild(0).gameObject.SetActive(true);
        }
        else
        {
            Debug.Log("CameraGroup Can't be found or Dosen't exist");
        }
    }

    void ActivatingCamera()
    {
        switch (_camType)
        {
            case Defines.CamType.ThirdPerson:
                CameraChanger(playerPivot, playerPivot, Defines.CamType.ThirdPerson);
                break;
            case Defines.CamType.OnePerson:
                CameraChanger(playerPivot, null, Defines.CamType.OnePerson);
                break;
            case Defines.CamType.SiegeMode:
                break;
        }
    }

    void CameraChanger(Transform target,Transform lookAt = null, Defines.CamType camType = Defines.CamType.ThirdPerson)
    {

        ICinemachineCamera currentCamera = cinemachineBrain.ActiveVirtualCamera;
        if (currentCamera == null) return;
        
        if (currentCamera.VirtualCameraGameObject) 
        {
            //ī�޶� ���� ����
            CinemachineVirtualCamera precam = currentCamera.VirtualCameraGameObject.GetComponent<CinemachineVirtualCamera>();
            CinemachinePOV precamPov = precam.GetCinemachineComponent<CinemachinePOV>();
            if (precamPov)
            {
                verticalAxis = precamPov.m_VerticalAxis.Value;
                horizonAxis = precamPov.m_HorizontalAxis.Value;
            }

            currentCamera.VirtualCameraGameObject.SetActive(false); 
        }

        //�ٲ� ī�޶� Ȱ��ȭ
        CinemachineVirtualCamera changeCamera;
        camDic.TryGetValue(camType.ToString(),out changeCamera);
        changeCamera.gameObject.SetActive(true);

        //���� ī�޶���� �ҷ�����
        CinemachinePOV cam = changeCamera.GetCinemachineComponent<CinemachinePOV>();
        cam.m_VerticalAxis.Value = verticalAxis;
        cam.m_HorizontalAxis.Value = horizonAxis;

        //Ÿ�� ����
        changeCamera.Follow = target;
        changeCamera.LookAt = lookAt;

        //ī�޶�Ÿ��
        _camType = camType;
    }

    public void EventCameraChanger(Defines.CamType camType, GameObject camera)
    {
        ICinemachineCamera currentCamera = cinemachineBrain.ActiveVirtualCamera;

        if (currentCamera == null) { return; }

        currentCamera.VirtualCameraGameObject.SetActive(false);
        camera.SetActive(true);
        
        _camType = camType;
    }

    public void AngleLimit(GameObject go,float horizon,float vertical)
    {
        CinemachineVirtualCamera cam = go.GetComponent<CinemachineVirtualCamera>();
        CinemachinePOV pov = cam.GetCinemachineComponent<CinemachinePOV>();

        pov.m_HorizontalAxis.m_MaxValue = horizon;
        pov.m_HorizontalAxis.m_MinValue = -horizon;

        pov.m_VerticalAxis.m_MaxValue = vertical;
        pov.m_VerticalAxis.m_MinValue = -vertical;
    }
}
