using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillManager : MonoBehaviour
{
    public int slotCount = 4;
    private int activatedSlot = 0;

    public GameObject slot;
    public List<Transform> SkillSlots = new List<Transform>();

    //�ڽ��� ���� ������ �ҷ������� �ҷ��� �̹����� �����̹����� ���ս�Ŵ
    //���� ����ī���͵� �������� �ҷ��� ������ �ҷ��;���
    public IInteracts weponCheck;

    private void Start()
    {
        //foreach(Transform child in transform)
        //{
        //    if (child.name == "Slot") SkillSlots.Add(child);
        //}

        MakeSlot(slotCount);
        SeletedSlot(activatedSlot);
    }
    public void MakeSlot(int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject prefab = Managers.Pool.Instantiate(slot);
            prefab.transform.SetParent(transform, false);
            SkillSlots.Add(prefab.transform);
            SkillController con = prefab.GetComponent<SkillController>();
            con.thisNumber = i;
        }
    }

    //���Կ��� ������ϴµ� ���� �Ը��� �����̶� �׳� ���⼭ ����
    public bool SeletedSlot(int index)
    {
        Transform NextChild = SkillSlots[index].GetChild(0);
        Image Nextimage = NextChild.GetComponent<Image>();
        if (Nextimage.sprite == null) return false;
        Nextimage.color = new Color(Nextimage.color.r, Nextimage.color.g, Nextimage.color.b, 0.2f);

        Transform PreChild = SkillSlots[activatedSlot].GetChild(0);
        Image Preimage = PreChild.GetComponent<Image>();
        Preimage.color = new Color(Preimage.color.r, Preimage.color.g, Preimage.color.b, 1f);

        activatedSlot = index;

        return true;
    }

    //��ų �巡�׵�� �Ҵ� ����� �Լ�...
}
