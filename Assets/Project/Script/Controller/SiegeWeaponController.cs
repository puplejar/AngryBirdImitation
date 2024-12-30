using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class SiegeWeaponController : MonoBehaviour, IInteracts , IDragHandler ,IPointerUpHandler , IPointerDownHandler
{
    private Defines.Interacts m_interact = Defines.Interacts.SiegeWeapon;
    public Defines.Interacts interactType { get { return m_interact; } }
    private GameObject player;

    //위치 : 코드로 캐싱하기엔 가독성이 떨어지므로 유니티 툴에서 할것을 권장
    //무기마다 인터페이스를 만들어 줬었어야 했다.
    public GameObject headRotate;
    public GameObject CameraPos;
    public GameObject bulletPos;
    public GameObject bowString;
    public GameObject trajectory;
    public LineRenderer lenderFirst;
    public LineRenderer lenderSecond;
    private Quaternion headRotateDefault; //초기 헤드포지션의 로테이션을 저장함
    private CameraController camCon;

    //탄약
    private GameObject activatedBullet;
    private int selectedBullet = 0;
    public List<GameObject> bullets = new List<GameObject>();

    //인풋시스템즈 인풋시스템은 컨트롤러마다 나누지말고 하나의 스크립트에서 관리하는게 좋을듯 하다. //인풋 핸들러
    private InputAction shootInput;
    private InputAction deboardInput;
    private InputAction powerUpInput;
    private InputAction powerDownInput;
    private InputAction number1;
    private InputAction number2;
    private InputAction number3;
    private InputAction number4;
    private UnityEngine.InputSystem.PlayerInput input;

    [Space] //마우스 범위 제한
    public float horizonWidth = 10.0f;
    public float verticalWidth = 10.0f;
    public float pullSensetive = 1.0f;
    private Vector3 mouseStartPos;
    private Vector3 mousePullPos;
    private Vector3 pullDir;
    private Vector3 mouseAngleStartPos;
    private Vector3 mouseAnglePullPos;
    private Vector3 mouseAngleDir;

    [Space] // 탄알 힘 조절
    public float chargeMinPower = 1.0f;
    public float chargeMaxPower = 1.0f;

    public float bulletSpeed = 1.0f;
    public float coolDownTime = 2.0f;

    private Vector3 lenderFirstVec;
    private Vector3 lenderSeconVec;

    public float gravity = 0.1f;

    [Space]//궤적 (주의 : 고장남)
    public int parabolaCount = 20;
    public GameObject parabolaOrigin;
    public List<GameObject> parabolas = new List<GameObject>();

    [Space]//스페이스 발사 1234는 탄약교체
    public SkillManager skillUI;
    private List<InputAction> numbers = new List<InputAction>();

    //사운드
    SoundController soundController;

    //쿨타임
    private bool isSound = false;
    private bool isBulletChange = false;
    private bool isBulletCoolDown = true;
    public float bulletChangeCool = 1.0f;

    void Awake()
    {
        camCon = Camera.main.gameObject.GetComponent<CameraController>();
        soundController = gameObject.GetComponent<SoundController>();
    }

    private void Start()
    {
        input = GetComponent<UnityEngine.InputSystem.PlayerInput>();
        shootInput = input.actions["Shoot"];
        deboardInput = input.actions["Deboard"];
        powerUpInput = input.actions["LeftClick"];
        powerDownInput = input.actions["RightClick"];
        number1 = input.actions["1"];
        number2 = input.actions["2"];
        number3 = input.actions["3"];
        number4 = input.actions["4"];
        numbers.Add(number1);
        numbers.Add(number2);
        numbers.Add(number3);
        numbers.Add(number4);

        headRotateDefault = headRotate.transform.localRotation;
        CameraPos.transform.rotation = gameObject.transform.rotation;

        //무기마다 줄을 당기는 라인렌더러가 다르기 때문에, 라인 하나만 쓸건지 두개를 쓸건지 지정해줌
        switch (gameObject.name)
        {
            case "Ballista":
                lenderFirstVec = lenderFirst.GetPosition(1);
                lenderSeconVec = lenderSecond.GetPosition(1);
                ResetLenderPositions();
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        KeyNumbers();
        if (deboardInput.triggered) DeBoard();
    }
    //Init역할
    public void OnComponents(GameObject player)
    {
        //플레이어 인풋 시스템을을 편하게 가져오려고 받아줌
        this.player = player;

        isBulletCoolDown = true;

        //카메라 전환 마우스 범위 제한
        camCon.EventCameraChanger(Defines.CamType.SiegeMode, CameraPos);
        //camCon.AngleLimit(CameraPos, horizonWidth, verticalWidth);

        input.enabled = true;

        StartCoroutine(ShootingCoolDown());

        //FindChild함수를 Util에서 왜 만드는지 알겠다. //true상태였다가 꺼야한다.
        GameObject canvas = GameObject.Find("Canvas");
        skillUI = canvas.GetComponentInChildren<SkillManager>();
    }
    public void KeyNumbers()
    {
        //키바꾸는데 딜레이가 있음 왜?
        //숫자만큼의 카운트가 아닌 들고있는 총알의 카운트만큼 해야 오버플로우가 발생하지 않음
        for (int i = 0; i < bullets.Count; i++)
        {
            if (numbers[i].triggered && !isBulletChange)
            {
                isBulletChange = true;
                selectedBullet = i;
                StartCoroutine(BulletChange());
            }
        }
    }

    //처음 마우스 위치
    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            mouseStartPos = Input.mousePosition;
        }
        if (eventData.button == PointerEventData.InputButton.Right) { mouseAngleStartPos = Input.mousePosition; }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left && !isBulletCoolDown)
        {

            //발사
            mousePullPos = Input.mousePosition;
            pullDir = (mousePullPos - mouseStartPos) * Time.fixedDeltaTime * pullSensetive;
            if (chargeMaxPower < pullDir.magnitude) pullDir = pullDir.normalized * chargeMaxPower;
            UpdateLenderPosition(pullDir.magnitude);

            //포물선
            Rigidbody bulletRg = activatedBullet.GetComponent<Rigidbody>();
            DrawTrajectory(bulletPos.transform.forward * pullDir.magnitude,bulletRg.mass);

            //사운드
            if (!isSound)
            {
                isSound = true;
                soundController.PlayAudio("Bow_PullString_1");
            }
        }
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            // 각도 조절
            mouseAnglePullPos = Input.mousePosition;
            mouseAngleDir = (mouseAnglePullPos - mouseAngleStartPos) * Time.fixedDeltaTime * pullSensetive;

            // 각도 제한
            float clampedVertical = Mathf.Clamp(mouseAngleDir.y, -verticalWidth, verticalWidth);
            float clampedHorizontal = Mathf.Clamp(mouseAngleDir.x, -horizonWidth, horizonWidth);

            // 회전 적용
            headRotate.transform.rotation = Quaternion.Euler(new Vector3(-clampedVertical, clampedHorizontal, 0)) * headRotateDefault;

        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (chargeMinPower < pullDir.magnitude && !isBulletCoolDown) { Shoot(); }
            else { ResetLenderPositions(); }
        }
    }

    void Shoot()
    {
        // 마우스 좌우클릭 힘조절 용
        //if (powerUpInput.IsPressed() && chargeCurrentPower <= chargeMaxPower && activatedBullet)
        //{
        //    chargeCurrentPower += Time.deltaTime;
        //    UpdateLenderPosition(chargeCurrentPower);
        //}
        //else if (powerDownInput.IsPressed() && chargeMinPower <= chargeCurrentPower && activatedBullet)
        //{
        //    chargeCurrentPower -= Time.deltaTime;
        //    UpdateLenderPosition(chargeCurrentPower);
        //}

        isBulletCoolDown = true;

        activatedBullet.transform.parent = null;
        BulletSkills bulletskill = activatedBullet.GetComponent<BulletSkills>();
        bulletskill.Shoot(bulletPos.transform.forward * pullDir.magnitude * bulletSpeed, ForceMode.Impulse);

        soundController.PlayAudio("Arrow_Impact");

        StartCoroutine(ShootingCoolDown());
        ResetLenderPositions();
        bowString.SetActive(true);
        ClearTrafectory();
    }

    //무기 비주얼 최신화
    void UpdateLenderPosition(float x)
    {
        if (gameObject.name == "Ballista")
        {
            lenderFirst.SetPosition(1, new Vector3(x, 0, lenderFirstVec.z));
            lenderSecond.SetPosition(1, new Vector3(x, 0, -lenderFirstVec.z));
            float distance = Mathf.Sqrt(x * x + lenderFirstVec.z * lenderFirstVec.z);
            activatedBullet.transform.localPosition = new Vector3(0, 0, -x/distance);
        }
    }

    //무기 비주얼 초기화
    void ResetLenderPositions()
    {
        if (gameObject.name == "Ballista")
        {
            lenderFirst.SetPosition(1, new Vector3(0, 0, lenderFirstVec.z));
            lenderSecond.SetPosition(1, new Vector3(0, 0, lenderSeconVec.z));
            lenderFirst.enabled = false;
            lenderSecond.enabled = false;
        }
    }

    IEnumerator ShootingCoolDown()
    {
        yield return new WaitForSeconds(coolDownTime);
        isBulletCoolDown = false;

        BulletInstate();

        bowString.SetActive(false);

        switch (gameObject.name)
        {
            case "Ballista":
                lenderFirst.enabled = true;
                lenderSecond.enabled = true;
                break;
        }
        //사운드
        isSound = false;
    }

    //포물선
    private void DrawTrajectory(Vector3 Gvelocity, float mass)
    {
        float timestep = 5f; // 더 작은 timestep 값 사용
        Vector3 adGravity = Physics.gravity; // 중력 가속도

        for (int i = 0; i < parabolaCount; i++)
        {
            float time = timestep * i;
            GameObject parabola;

            // 궤적 점 생성 또는 재활용
            if (i >= parabolas.Count)
            {
                parabola = Managers.Pool.Instantiate(parabolaOrigin, true);
                parabolas.Add(parabola);
            }
            else
            {
                parabola = parabolas[i];
            }
            parabola.SetActive(true);

            // 궤적 점 설정
            parabola.transform.parent = trajectory.transform;
            parabola.transform.rotation = Quaternion.Euler(Vector3.zero);
            parabola.transform.position = bulletPos.transform.position + Gvelocity * time + gravity * adGravity * time * time;
        }
    }

    private void ClearTrafectory()
    {
        int count = trajectory.transform.childCount;
        for (int i=0; i< count; i++)
        {
            Managers.Pool.Destroy(trajectory.transform.GetChild(0).gameObject);
        }
    }

    IEnumerator BulletChange()
    {
        yield return new WaitForSeconds(bulletChangeCool);

        if (activatedBullet) Managers.Pool.Destroy(activatedBullet);
        skillUI.SeletedSlot(selectedBullet);
        StopCoroutine(ShootingCoolDown());
        StartCoroutine(ShootingCoolDown());
        isBulletChange = false;
    }
    public void BulletInstate()
    {
        activatedBullet = Managers.Pool.Instantiate(bullets[selectedBullet], true);
        activatedBullet.transform.parent = bulletPos.transform;
        activatedBullet.transform.rotation = bulletPos.transform.rotation;
        activatedBullet.transform.localScale = new Vector3(3, 3, 3);
        activatedBullet.transform.localPosition = Vector3.zero;
    }

    //탑승해제
    public void DeBoard()
    {
        input.enabled = false;
        if(activatedBullet) Managers.Pool.Destroy(activatedBullet);
        UnityEngine.InputSystem.PlayerInput inputs = player.GetComponent<UnityEngine.InputSystem.PlayerInput>();
        inputs.enabled = true;

        ResetLenderPositions();
        headRotate.transform.rotation = headRotateDefault;

        camCon.camType = Defines.CamType.ThirdPerson;
    }
}
