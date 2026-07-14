using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "AllItemsData", menuName = "Game/All Items Data")]
public class AllItemsData : ScriptableObject
{
    public List<ItemData> items;

    private void OnEnable()
    {
        if (items == null || items.Count == 0)
        {
            items = new List<ItemData>();
            FillDefaultItems();
        }
    }

    private void FillDefaultItems()
    {
        // ============================================
        // ОРУЖИЕ (Weapon) — наносят урон
        // ============================================

        // 1. Ржавый меч
        ItemData rustySword = new ItemData();
        rustySword.itemName = "Ржавый меч";
        rustySword.itemType = ItemType.Weapon;
        rustySword.price = 3;
        rustySword.folderName = "RustySword";
        rustySword.loreDescription = "Старый клинок, повидавший сотни битв. Ржавчина въелась глубоко, но сталь всё ещё держит удар.";
        rustySword.cellMask[2 + 1*5] = true;
        rustySword.cellMask[2 + 2*5] = true;
        rustySword.cellMask[2 + 3*5] = true;
        rustySword.damage = 5;
        rustySword.attackSpeed = 0.7f;
        items.Add(rustySword);

        // 2. Клинок берсерка
        ItemData berserkBlade = new ItemData();
        berserkBlade.itemName = "Клинок берсерка";
        berserkBlade.itemType = ItemType.Weapon;
        berserkBlade.price = 8;
        berserkBlade.folderName = "BerserkBlade";
        berserkBlade.loreDescription = "Огромный меч из чёрной стали. Рубит демонов и всё, что встанет на пути.";
        berserkBlade.cellMask[1 + 1*5] = true;
        berserkBlade.cellMask[2 + 1*5] = true;
        berserkBlade.cellMask[2 + 2*5] = true;
        berserkBlade.cellMask[2 + 3*5] = true;
        berserkBlade.cellMask[3 + 1*5] = true;
        berserkBlade.damage = 12;
        berserkBlade.attackSpeed = 1.2f;
        berserkBlade.bleedDamage = 3;
        berserkBlade.bonusHP = -10;
        items.Add(berserkBlade);

        // 3. Кровавый рассвет
        ItemData bloodyDawn = new ItemData();
        bloodyDawn.itemName = "Кровавый рассвет";
        bloodyDawn.itemType = ItemType.Weapon;
        bloodyDawn.price = 12;
        bloodyDawn.folderName = "BloodyDawn";
        bloodyDawn.loreDescription = "Меч с алым лезвием, что пьёт кровь врагов.";
        bloodyDawn.cellMask[2 + 1*5] = true;
        bloodyDawn.cellMask[2 + 2*5] = true;
        bloodyDawn.cellMask[2 + 3*5] = true;
        bloodyDawn.damage = 8;
        bloodyDawn.attackSpeed = 0.7f;
        bloodyDawn.vampirism = 12;
        items.Add(bloodyDawn);

        // 4. Топор дровосека
        ItemData lumberAxe = new ItemData();
        lumberAxe.itemName = "Топор дровосека";
        lumberAxe.itemType = ItemType.Weapon;
        lumberAxe.price = 5;
        lumberAxe.folderName = "LumberAxe";
        lumberAxe.loreDescription = "Тяжёлый топор, которым валили лес. В умелых руках крушит не только деревья.";
        lumberAxe.cellMask[1 + 2*5] = true;
        lumberAxe.cellMask[2 + 1*5] = true;
        lumberAxe.cellMask[2 + 2*5] = true;
        lumberAxe.cellMask[2 + 3*5] = true;
        lumberAxe.damage = 8;
        lumberAxe.attackSpeed = 1.0f;
        items.Add(lumberAxe);

        // 5. Топор палача
        ItemData executionerAxe = new ItemData();
        executionerAxe.itemName = "Топор палача";
        executionerAxe.itemType = ItemType.Weapon;
        executionerAxe.price = 15;
        executionerAxe.folderName = "ExecutionerAxe";
        executionerAxe.loreDescription = "Тяжёлый топор с длинной рукоятью. Один удар — и приговор приведён в исполнение.";
        executionerAxe.cellMask[1 + 2*5] = true;
        executionerAxe.cellMask[2 + 1*5] = true;
        executionerAxe.cellMask[2 + 2*5] = true;
        executionerAxe.cellMask[2 + 3*5] = true;
        executionerAxe.cellMask[3 + 2*5] = true;
        executionerAxe.damage = 14;
        executionerAxe.attackSpeed = 1.4f;
        executionerAxe.executeThreshold = 25;
        items.Add(executionerAxe);

        // 6. Гнилой посох
        ItemData rottenStaff = new ItemData();
        rottenStaff.itemName = "Гнилой посох";
        rottenStaff.itemType = ItemType.Weapon;
        rottenStaff.price = 5;
        rottenStaff.folderName = "RottenStaff";
        rottenStaff.loreDescription = "Трухлявый посох болотного отшельника. Даже лёгкое касание заражает рану.";
        rottenStaff.cellMask[2 + 1*5] = true;
        rottenStaff.cellMask[2 + 2*5] = true;
        rottenStaff.cellMask[2 + 3*5] = true;
        rottenStaff.damage = 2;
        rottenStaff.attackSpeed = 0.8f;
        rottenStaff.poisonDamage = 2;
        rottenStaff.poisonDuration = 2;
        rottenStaff.isMagical = true;
        rottenStaff.manaCostPerAttack = 3;
        items.Add(rottenStaff);

        // 7. Кинжал
        ItemData dagger = new ItemData();
        dagger.itemName = "Кинжал";
        dagger.itemType = ItemType.Weapon;
        dagger.price = 4;
        dagger.folderName = "Dagger";
        dagger.loreDescription = "Маленький, но быстрый. Пока враг замахивается — ты уже ударил трижды.";
        dagger.cellMask[2 + 1*5] = true;
        dagger.cellMask[2 + 2*5] = true;
        dagger.damage = 3;
        dagger.attackSpeed = 0.25f;
        dagger.dodgeChance = 5;
        items.Add(dagger);

        // 8. Лук охотника
        ItemData hunterBow = new ItemData();
        hunterBow.itemName = "Лук охотника";
        hunterBow.itemType = ItemType.Weapon;
        hunterBow.price = 4;
        hunterBow.folderName = "HunterBow";
        hunterBow.loreDescription = "Простой лук из тиса. Охотник знает — важен не лук, а глаз.";
        hunterBow.cellMask[1 + 1*5] = true;
        hunterBow.cellMask[2 + 2*5] = true;
        hunterBow.cellMask[3 + 1*5] = true;
        hunterBow.damage = 4;
        hunterBow.attackSpeed = 0.6f;
        hunterBow.dodgeChance = 8;
        items.Add(hunterBow);

        // ============================================
        // БРОНЯ (Armor) — защита, не атакуют
        // ============================================

        // 9. Кожаный шлем
        ItemData leatherHelmet = new ItemData();
        leatherHelmet.itemName = "Кожаный шлем";
        leatherHelmet.itemType = ItemType.Armor;
        leatherHelmet.price = 3;
        leatherHelmet.folderName = "LeatherHelmet";
        leatherHelmet.loreDescription = "Простой кожаный шлем. Спасёт голову от случайного удара.";
        leatherHelmet.cellMask[2 + 2*5] = true;
        leatherHelmet.bonusArmor = 8;
        items.Add(leatherHelmet);

        // 10. Стёганый доспех
        ItemData quiltedArmor = new ItemData();
        quiltedArmor.itemName = "Стёганый доспех";
        quiltedArmor.itemType = ItemType.Armor;
        quiltedArmor.price = 5;
        quiltedArmor.folderName = "QuiltedArmor";
        quiltedArmor.loreDescription = "Плотно прошитый доспех из слоёв ткани и кожи. Дёшево, но лучше чем ничего.";
        for (int y = 1; y <= 3; y++) for (int x = 1; x <= 2; x++) quiltedArmor.cellMask[x + y*5] = true;
        quiltedArmor.bonusArmor = 15;
        quiltedArmor.damageReduction = 2;
        items.Add(quiltedArmor);

        // 11. Круглый щит
        ItemData roundShield = new ItemData();
        roundShield.itemName = "Круглый щит";
        roundShield.itemType = ItemType.Armor;
        roundShield.price = 4;
        roundShield.folderName = "RoundShield";
        roundShield.loreDescription = "Простой дубовый щит, окованный железом. Тяжёлый, но надёжный — выдержит не один удар.";
        for (int y = 1; y <= 3; y++) for (int x = 1; x <= 2; x++) roundShield.cellMask[x + y*5] = true;
        roundShield.bonusArmor = 20;
        roundShield.blockChance = 8;
        items.Add(roundShield);

        // 12. Сапоги ополченца
        ItemData militiaBoots = new ItemData();
        militiaBoots.itemName = "Сапоги ополченца";
        militiaBoots.itemType = ItemType.Armor;
        militiaBoots.price = 4;
        militiaBoots.folderName = "MilitiaBoots";
        militiaBoots.loreDescription = "Грубые кожаные сапоги. Устойчивая посадка помогает держать равновесие в бою.";
        militiaBoots.cellMask[2 + 1*5] = true;
        militiaBoots.cellMask[2 + 2*5] = true;
        militiaBoots.bonusArmor = 5;
        militiaBoots.dodgeChance = 5;
        items.Add(militiaBoots);

        // ============================================
        // УКРАШЕНИЯ (Jewelry) — баффают владельца, не атакуют
        // ============================================

        // 13. Кольцо боли
        ItemData painRing = new ItemData();
        painRing.itemName = "Кольцо боли";
        painRing.itemType = ItemType.Jewelry;
        painRing.price = 6;
        painRing.folderName = "PainRing";
        painRing.loreDescription = "Кольцо с шипами внутрь. Причиняет боль владельцу, но даёт невероятную силу.";
        painRing.cellMask[2 + 2*5] = true;
        painRing.neighborBonusDamage = 15;
        painRing.bonusHP = -10;
        items.Add(painRing);

        // 14. Перчатки берсерка
        ItemData berserkGloves = new ItemData();
        berserkGloves.itemName = "Перчатки берсерка";
        berserkGloves.itemType = ItemType.Jewelry;
        berserkGloves.price = 7;
        berserkGloves.folderName = "BerserkGloves";
        berserkGloves.loreDescription = "Тяжёлые перчатки из чёрной кожи. Кулаки наливаются силой, когда оружия много.";
        berserkGloves.cellMask[2 + 2*5] = true;
        berserkGloves.damagePerItemInInventory = 1;
        items.Add(berserkGloves);

        // 15. Пояс берсерка
        ItemData berserkBelt = new ItemData();
        berserkBelt.itemName = "Пояс берсерка";
        berserkBelt.itemType = ItemType.Jewelry;
        berserkBelt.price = 6;
        berserkBelt.folderName = "BerserkBelt";
        berserkBelt.loreDescription = "Широкий пояс из чёрной кожи с пряжкой-оскалом медведя. Ярость зверя течёт в жилах.";
        berserkBelt.cellMask[1 + 2*5] = true;
        berserkBelt.cellMask[2 + 2*5] = true;
        berserkBelt.neighborBonusSpeed = 10;
        berserkBelt.bonusHP = -8;
        items.Add(berserkBelt);

        // ============================================
        // АРТЕФАКТЫ (Artifact) — ауры и глобальные эффекты
        // ============================================

        // 16. Кристалл берсерка
        ItemData berserkCrystal = new ItemData();
        berserkCrystal.itemName = "Кристалл берсерка";
        berserkCrystal.itemType = ItemType.Artifact;
        berserkCrystal.price = 8;
        berserkCrystal.folderName = "BerserkCrystal";
        berserkCrystal.loreDescription = "Тёмно-красный кристалл, пульсирующий яростью. Ускоряет союзное оружие ценой здоровья.";
        berserkCrystal.cellMask[2 + 2*5] = true;
        berserkCrystal.hasCategoryBuff = true;
        berserkCrystal.categoryTargetType = ItemType.Weapon;
        berserkCrystal.categoryBonusSpeed = 15;
        berserkCrystal.bonusHP = -10;
        items.Add(berserkCrystal);

        // 17. Сердце тьмы
        ItemData darkHeart = new ItemData();
        darkHeart.itemName = "Сердце тьмы";
        darkHeart.itemType = ItemType.Artifact;
        darkHeart.price = 10;
        darkHeart.folderName = "DarkHeart";
        darkHeart.loreDescription = "Живое сердце из тьмы. Дарует вампиризм и ману ценой здоровья.";
        darkHeart.cellMask[2 + 2*5] = true;
        darkHeart.vampirism = 15;
        darkHeart.bonusMana = 30;
        darkHeart.manaRegen = 2;
        darkHeart.isMagical = true;
        darkHeart.bonusHP = -15;
        darkHeart.phoenixResurrection = true;
        darkHeart.phoenixHPPercent = 1;
        items.Add(darkHeart);

        // ============================================
        // ПРИПАСЫ (Supply) — экономика и расходники
        // ============================================

        // 18. Флакон крови тролля
        ItemData trollBlood = new ItemData();
        trollBlood.itemName = "Флакон крови тролля";
        trollBlood.itemType = ItemType.Supply;
        trollBlood.price = 3;
        trollBlood.folderName = "TrollBlood";
        trollBlood.loreDescription = "Пузырёк с красной жидкостью. Пахнет травами и детством.";
        trollBlood.cellMask[2 + 2*5] = true;
        trollBlood.bonusHP = 30;
        items.Add(trollBlood);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(AllItemsData))]
public class AllItemsDataEditor : Editor
{
    private SerializedProperty itemsProp;
    private Dictionary<int, bool> foldoutStats = new Dictionary<int, bool>();
    private Dictionary<int, bool> editEffectMask = new Dictionary<int, bool>();

    void OnEnable() { itemsProp = serializedObject.FindProperty("items"); }

    void DrawMaskGrid(SerializedProperty maskProp, string label, Color activeColor)
    {
        EditorGUILayout.LabelField(label);
        EditorGUILayout.BeginVertical(GUI.skin.box);
        for (int y = 0; y < 5; y++)
        {
            EditorGUILayout.BeginHorizontal();
            for (int x = 0; x < 5; x++)
            {
                int index = x + y * 5;
                bool active = maskProp.GetArrayElementAtIndex(index).boolValue;
                Color bg = active ? activeColor : new Color(0.3f, 0.3f, 0.3f, 1f);
                GUI.backgroundColor = bg;
                if (GUILayout.Button($"{x},{y}", GUILayout.Width(40), GUILayout.Height(40)))
                    maskProp.GetArrayElementAtIndex(index).boolValue = !active;
                GUI.backgroundColor = Color.white;
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        if (GUILayout.Button("Добавить предмет", GUILayout.Height(30)))
            (target as AllItemsData).items.Add(new ItemData());

        EditorGUILayout.Space(10);

        for (int i = 0; i < itemsProp.arraySize; i++)
        {
            SerializedProperty itemProp = itemsProp.GetArrayElementAtIndex(i);
            SerializedProperty nameProp = itemProp.FindPropertyRelative("itemName");
            SerializedProperty loreDescProp = itemProp.FindPropertyRelative("loreDescription");
            SerializedProperty folderProp = itemProp.FindPropertyRelative("folderName");
            SerializedProperty priceProp = itemProp.FindPropertyRelative("price");
            SerializedProperty typeProp = itemProp.FindPropertyRelative("itemType");
            SerializedProperty cellMaskProp = itemProp.FindPropertyRelative("cellMask");
            SerializedProperty effectMaskProp = itemProp.FindPropertyRelative("effectMask");
            SerializedProperty hasCategoryBuffProp = itemProp.FindPropertyRelative("hasCategoryBuff");
            SerializedProperty categoryTargetTypeProp = itemProp.FindPropertyRelative("categoryTargetType");

            EditorGUILayout.BeginVertical(GUI.skin.box);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"#{i + 1} — {nameProp.stringValue}", EditorStyles.boldLabel);
            if (GUILayout.Button("X", GUILayout.Width(25))) { (target as AllItemsData).items.RemoveAt(i); serializedObject.ApplyModifiedProperties(); break; }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(nameProp, new GUIContent("Название"));
            EditorGUILayout.PropertyField(loreDescProp, new GUIContent("Лор-описание"));
            EditorGUILayout.PropertyField(folderProp, new GUIContent("Папка"));
            EditorGUILayout.PropertyField(typeProp, new GUIContent("Тип"));
            EditorGUILayout.PropertyField(priceProp, new GUIContent("Цена"));

            DrawMaskGrid(cellMaskProp, "Форма предмета (зелёные):", new Color(0.3f, 0.8f, 0.3f));

            EditorGUILayout.LabelField("Мана:", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(itemProp.FindPropertyRelative("isMagical"), new GUIContent("Магический?"));
            EditorGUILayout.PropertyField(itemProp.FindPropertyRelative("bonusMana"), new GUIContent("Макс. мана"));
            EditorGUILayout.PropertyField(itemProp.FindPropertyRelative("manaRegen"), new GUIContent("Реген маны/сек"));
            EditorGUILayout.PropertyField(itemProp.FindPropertyRelative("manaCostPerSecond"), new GUIContent("Расход маны/сек"));
            EditorGUILayout.PropertyField(itemProp.FindPropertyRelative("manaCostPerAttack"), new GUIContent("Расход маны/атака"));

            EditorGUILayout.PropertyField(hasCategoryBuffProp, new GUIContent("Бафф по категории?"));
            if (hasCategoryBuffProp.boolValue)
            {
                EditorGUILayout.PropertyField(categoryTargetTypeProp, new GUIContent("Тип цели"));
                EditorGUILayout.PropertyField(itemProp.FindPropertyRelative("categoryBonusDamage"), new GUIContent("Бонус к урону (%)"));
                EditorGUILayout.PropertyField(itemProp.FindPropertyRelative("categoryBonusSpeed"), new GUIContent("Бонус к скорости (%)"));
                EditorGUILayout.PropertyField(itemProp.FindPropertyRelative("categoryBonusCrit"), new GUIContent("Бонус к криту (%)"));
            }

            EditorGUILayout.Space(3);

            if (!editEffectMask.ContainsKey(i)) editEffectMask[i] = false;
            editEffectMask[i] = EditorGUILayout.Foldout(editEffectMask[i], "Аура (синие клетки)");
            if (editEffectMask[i]) DrawMaskGrid(effectMaskProp, "Клетки воздействия:", new Color(0.3f, 0.5f, 1f));

            EditorGUILayout.Space(5);

            if (!foldoutStats.ContainsKey(i)) foldoutStats[i] = false;
            foldoutStats[i] = EditorGUILayout.Foldout(foldoutStats[i], "Характеристики");
            if (foldoutStats[i])
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField("Боевые:", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(itemProp.FindPropertyRelative("damage"));
                EditorGUILayout.PropertyField(itemProp.FindPropertyRelative("attackSpeed"));
                EditorGUILayout.PropertyField(itemProp.FindPropertyRelative("critChance"));
                EditorGUILayout.PropertyField(itemProp.FindPropertyRelative("critDamage"));
                EditorGUILayout.PropertyField(itemProp.FindPropertyRelative("dodgeChance"));
                EditorGUILayout.PropertyField(itemProp.FindPropertyRelative("vampirism"));
                EditorGUILayout.PropertyField(itemProp.FindPropertyRelative("poisonDamage"));
                EditorGUILayout.PropertyField(itemProp.FindPropertyRelative("poisonDuration"));
                EditorGUILayout.PropertyField(itemProp.FindPropertyRelative("burnDamage"));
                EditorGUILayout.PropertyField(itemProp.FindPropertyRelative("burnDuration"));
                EditorGUILayout.PropertyField(itemProp.FindPropertyRelative("slowPercent"));
                EditorGUILayout.PropertyField(itemProp.FindPropertyRelative("stunChance"));
                EditorGUILayout.PropertyField(itemProp.FindPropertyRelative("thornsDamage"));
                EditorGUILayout.PropertyField(itemProp.FindPropertyRelative("bleedDamage"));
                EditorGUILayout.PropertyField(itemProp.FindPropertyRelative("executeThreshold"));
                EditorGUILayout.PropertyField(itemProp.FindPropertyRelative("extraProjectileChance"));

                EditorGUILayout.LabelField("Защитные:", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(itemProp.FindPropertyRelative("bonusHP"));
                EditorGUILayout.PropertyField(itemProp.FindPropertyRelative("bonusArmor"));
                EditorGUILayout.PropertyField(itemProp.FindPropertyRelative("hpRegen"));
                EditorGUILayout.PropertyField(itemProp.FindPropertyRelative("armorRegen"));
                EditorGUILayout.PropertyField(itemProp.FindPropertyRelative("blockChance"));
                EditorGUILayout.PropertyField(itemProp.FindPropertyRelative("poisonResist"));
                EditorGUILayout.PropertyField(itemProp.FindPropertyRelative("fireResist"));
                EditorGUILayout.PropertyField(itemProp.FindPropertyRelative("damageReduction"));
                EditorGUILayout.PropertyField(itemProp.FindPropertyRelative("damageReductionPercent"));
                EditorGUILayout.PropertyField(itemProp.FindPropertyRelative("invulnerabilityDuration"));

                EditorGUILayout.LabelField("Ауры:", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(itemProp.FindPropertyRelative("neighborBonusDamage"));
                EditorGUILayout.PropertyField(itemProp.FindPropertyRelative("neighborBonusSpeed"));
                EditorGUILayout.PropertyField(itemProp.FindPropertyRelative("neighborBonusCrit"));
                EditorGUILayout.PropertyField(itemProp.FindPropertyRelative("neighborBonusVampirism"));
                EditorGUILayout.PropertyField(itemProp.FindPropertyRelative("neighborBonusArmor"));
                EditorGUILayout.PropertyField(itemProp.FindPropertyRelative("neighborBonusHP"));
                EditorGUILayout.PropertyField(itemProp.FindPropertyRelative("bonusPerEmptyCell"));

                EditorGUILayout.LabelField("Экономические:", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(itemProp.FindPropertyRelative("coinsPerLevel"));
                EditorGUILayout.PropertyField(itemProp.FindPropertyRelative("coinsPerKill"));
                EditorGUILayout.PropertyField(itemProp.FindPropertyRelative("shopDiscount"));
                EditorGUILayout.PropertyField(itemProp.FindPropertyRelative("freeRerollChance"));
                EditorGUILayout.PropertyField(itemProp.FindPropertyRelative("extraShopCards"));
                EditorGUILayout.PropertyField(itemProp.FindPropertyRelative("fullPriceSell"));

                EditorGUILayout.LabelField("Растущие:", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(itemProp.FindPropertyRelative("damagePerKillInBattle"));
                EditorGUILayout.PropertyField(itemProp.FindPropertyRelative("damagePerItemInInventory"));

                EditorGUILayout.LabelField("Особые:", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(itemProp.FindPropertyRelative("phoenixResurrection"));
                EditorGUILayout.PropertyField(itemProp.FindPropertyRelative("phoenixHPPercent"));
                EditorGUILayout.PropertyField(itemProp.FindPropertyRelative("mirrorNeighbor"));
                EditorGUILayout.PropertyField(itemProp.FindPropertyRelative("dummy"));
                EditorGUILayout.PropertyField(itemProp.FindPropertyRelative("timeCompression"));
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }

        serializedObject.ApplyModifiedProperties();

        if (GUILayout.Button("Сохранить", GUILayout.Height(30)))
        {
            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();
        }
    }
}
#endif