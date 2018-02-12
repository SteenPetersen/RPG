using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New Equipment", menuName = "Inventory/Equipment")]
public class Equipment : Item {

    public EquipmentSlot equipSlot;
    public EquipmentType equipType;
    public Sprite characterVisibleSprite;

    public int armorModifier;
    public int damageModifier;

    public int rangedProjectile;

    public override void Use()
    {
        base.Use();
        // equip the items
        bool equipIt = EquipmentManager.instance.Equip(this);
        //remove it from inventory if it managed to get equipped
        if (equipIt)
        {
            RemoveFromInventory();
        }
    }


}

public enum EquipmentSlot { Head, Chest, Legs, MainHand, OffHand, FrontFoot, BackFoot, GauntletLeft, GauntletRight, Shoulder, Ring1, Ring2, Neck }
public enum EquipmentType { Melee, Ranged, Armor, Key, Light}

//[CustomEditor(typeof(Equipment))]
//public class EquipmentEditor : Editor
//{
//    public override void OnInspectorGUI()
//    {
//        base.OnInspectorGUI();

//        var equipmentScript = target as Equipment;

//        bool isRanged = 

//        equipmentScript.equipType = EditorGUILayout.Toggle(equipmentScript.equipType, "Ranged");

//        if (equipmentScript.ranged)
//        {
//            Debug.Log("set to ranged");
//        }
//    }
//}
