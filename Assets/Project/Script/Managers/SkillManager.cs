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

    //자신을 담은 무기의 불렛정보를 불러와 이미지와 슬롯이미지를 결합시킴
    //위의 슬롯카운터도 마찬가지 불렛의 갯수를 불러와야함
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

    //슬롯에서 해줘야하는데 작은 규모의 게임이라 그냥 여기서 했음
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

    //스킬 드래그드롭 할당 등등의 함수...
}
