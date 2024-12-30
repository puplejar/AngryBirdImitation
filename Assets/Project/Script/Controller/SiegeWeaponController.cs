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

    //��ġ : �ڵ�� ĳ���ϱ⿣ �������� �������Ƿ� ����Ƽ ������ �Ұ��� ����
    //���⸶�� �������̽��� ����� ������ �ߴ�.
    public GameObject headRotate;
    public GameObject CameraPos;
    public GameObject bulletPos;
    public GameObject bowString;
    public GameObject trajectory;
    public LineRenderer lenderFirst;
    public LineRenderer lenderSecond;
    private Quaternion headRotateDefault; //�ʱ� ����������� �����̼��� ������
    private CameraController camCon;

    //ź��
    private GameObject activatedBullet;
    private int selectedBullet = 0;
    public List<GameObject> bullets = new List<GameObject>();

    //��ǲ�ý����� ��ǲ�ý����� ��Ʈ�ѷ����� ���������� �ϳ��� ��ũ��Ʈ���� �����ϴ°� ������ �ϴ�. //��ǲ �ڵ鷯
    private InputAction shootInput;
    private InputAction deboardInput;
    private InputAction powerUpInput;
    private InputAction powerDownInput;
    private InputAction number1;
    private InputAction number2;
    private InputAction number3;
    private InputAction number4;
    private UnityEngine.InputSystem.PlayerInput input;

    [Space] //���콺 ���� ����
    public float horizonWidth = 10.0f;
    public float verticalWidth = 10.0f;
    public float pullSensetive = 1.0f;
    private Vector3 mouseStartPos;
    private Vector3 mousePullPos;
    private Vector3 pullDir;
    private Vector3 mouseAngleStartPos;
    private Vector3 mouseAnglePullPos;
    private Vector3 mouseAngleDir;

    [Space] // ź�� �� ����
    public float chargeMinPower = 1.0f;
    public float chargeMaxPower = 1.0f;

    public float bulletSpeed = 1.0f;
    public float coolDownTime = 2.0f;

    private Vector3 lenderFirstVec;
    private Vector3 lenderSeconVec;

    public float gravity = 0.1f;

    [Space]//���� (���� : ���峲)
    public int parabolaCount = 20;
    public GameObject parabolaOrigin;
    public List<GameObject> parabolas = new List<GameObject>();

    [Space]//�����̽� �߻� 1234�� ź�౳ü
    public SkillManager skillUI;
    private List<InputAction> numbers = new List<InputAction>();

    //����
    SoundController soundController;

    //��Ÿ��
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

        //���⸶�� ���� ���� ���η������� �ٸ��� ������, ���� �ϳ��� ������ �ΰ��� ������ ��������
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
    //Init����
    public void OnComponents(GameObject player)
    {
        //�÷��̾� ��ǲ �ý������� ���ϰ� ���������� �޾���
        this.player = player;

        isBulletCoolDown = true;

        //ī�޶� ��ȯ ���콺 ���� ����
        camCon.EventCameraChanger(Defines.CamType.SiegeMode, CameraPos);
        //camCon.AngleLimit(CameraPos, horizonWidth, verticalWidth);

        input.enabled = true;

        StartCoroutine(ShootingCoolDown());

        //FindChild�Լ��� Util���� �� ������� �˰ڴ�. //true���¿��ٰ� �����Ѵ�.
        GameObject canvas = GameObject.Find("Canvas");
        skillUI = canvas.GetComponentInChildren<SkillManager>();
    }
    public void KeyNumbers()
    {
        //Ű�ٲٴµ� �����̰� ���� ��?
        //���ڸ�ŭ�� ī��Ʈ�� �ƴ� ����ִ� �Ѿ��� ī��Ʈ��ŭ �ؾ� �����÷ο찡 �߻����� ����
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

    //ó�� ���콺 ��ġ
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

            //�߻�
            mousePullPos = Input.mousePosition;
            pullDir = (mousePullPos - mouseStartPos) * Time.fixedDeltaTime * pullSensetive;
            if (chargeMaxPower < pullDir.magnitude) pullDir = pullDir.normalized * chargeMaxPower;
            UpdateLenderPosition(pullDir.magnitude);

            //������
            Rigidbody bulletRg = activatedBullet.GetComponent<Rigidbody>();
            DrawTrajectory(bulletPos.transform.forward * pullDir.magnitude,bulletRg.mass);

            //����
            if (!isSound)
            {
                isSound = true;
                soundController.PlayAudio("Bow_PullString_1");
            }
        }
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            // ���� ����
            mouseAnglePullPos = Input.mousePosition;
            mouseAngleDir = (mouseAnglePullPos - mouseAngleStartPos) * Time.fixedDeltaTime * pullSensetive;

            // ���� ����
            float clampedVertical = Mathf.Clamp(mouseAngleDir.y, -verticalWidth, verticalWidth);
            float clampedHorizontal = Mathf.Clamp(mouseAngleDir.x, -horizonWidth, horizonWidth);

            // ȸ�� ����
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
        // ���콺 �¿�Ŭ�� ������ ��
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

    //���� ���־� �ֽ�ȭ
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

    //���� ���־� �ʱ�ȭ
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
        //����
        isSound = false;
    }

    //������
    private void DrawTrajectory(Vector3 Gvelocity, float mass)
    {
        float timestep = 5f; // �� ���� timestep �� ���
        Vector3 adGravity = Physics.gravity; // �߷� ���ӵ�

        for (int i = 0; i < parabolaCount; i++)
        {
            float time = timestep * i;
            GameObject parabola;

            // ���� �� ���� �Ǵ� ��Ȱ��
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

            // ���� �� ����
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

    //ž������
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
