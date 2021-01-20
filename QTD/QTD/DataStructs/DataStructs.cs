using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNALib;

namespace QTD
{
    public static class DataStructs
    {
        public static List<TowerStruct> Towers = new List<TowerStruct>();
        public static List<RunnerStruct> Runners = new List<RunnerStruct>();
        public static List<DefenderStruct> Defenders = new List<DefenderStruct>();
        public static List<TowerCategoryStruct> TowerCategories = new List<TowerCategoryStruct>();

        public static void LoadTowers()
        {
            Towers.Clear();
            string[] towerXmls = Directory.GetFiles("Data/Towers", "*.xml");

            foreach (string path in towerXmls)
            {
                XDocument doc = XDocument.Load(path);

                #region Required
                XElement RequiredNode = doc.Root.Element("Required");
                if (RequiredNode == null)
                    throw new Exception("Required node missing. " + path);

                TowerStruct ts = new TowerStruct(int.Parse(RequiredNode.Element("ID").Value));
                XElement required = RequiredNode.Element("Name");
                if (required != null)
                    ts.Name = required.Value;
                else
                    throw new Exception("Name missing." + path);
                required = null;

                required = RequiredNode.Element("Description");
                if (required != null)
                    ts.Desc = required.Value;
                else
                    throw new Exception("Description missing." + path);
                required = null;

                required = RequiredNode.Element("Cost");
                if (required != null)
                    ts.Cost = int.Parse(required.Value);
                else
                    throw new Exception("Cost missing." + path);
                required = null;

                required = RequiredNode.Element("Icon");
                if (required != null)
                    ts.Icon = Common.str2Tex("Icons/" + required.Value);
                else
                    throw new Exception("Icon missing." + path);
                required = null;

                required = RequiredNode.Element("Animation");
                if (required != null)
                    ts.AnimationType = (eAnimation)Enum.Parse(typeof(eAnimation), required.Value);
                else
                    throw new Exception("Animation missing." + path);
                required = null;
                #endregion

                #region Basic
                XElement basicNode = doc.Root.Element("Basic");
                if (basicNode != null)
                {
                    XElement basic;

                    basic = basicNode.Element("BuildSize");
                    if (basic != null)
                        ts.BuildSize = Common.Str2Point(basic.Value);
                    basic = null;

                    basic = basicNode.Element("BuildTime");
                    if (basic != null)
                        ts.BuildTimeInMS = int.Parse(basic.Value);
                    basic = null;

                    basic = basicNode.Element("Supply");
                    if (basic != null)
                        ts.SupplyCost = int.Parse(basic.Value);
                    basic = null;

                    basic = basicNode.Element("UpgradeTime");
                    if (basic != null)
                        ts.UpgTimeInMS = int.Parse(basic.Value);
                    basic = null;
                }
                #endregion

                #region Advanced
                XElement advancedNode = doc.Root.Element("Advanced");
                if (advancedNode != null)
                {
                    XElement advanced;

                    advanced = advancedNode.Element("IncomePerTick");
                    if (advanced != null)
                        ts.IncomePerTick = int.Parse(advanced.Value);
                    advanced = null;

                    advanced = advancedNode.Element("IncomeTickDelay");
                    if (advanced != null)
                        ts.IncomeTickDelayInMS = int.Parse(advanced.Value);
                    advanced = null;

                    advanced = advancedNode.Element("IncomePerWave");
                    if (advanced != null)
                        ts.IncomePerWave = int.Parse(advanced.Value);
                    advanced = null;
                }
                #endregion

                #region Weapons
                XElement wpnNode = doc.Root.Element("Weapons");
                if (wpnNode != null)
                {
                    foreach (XElement wpn in wpnNode.Elements())
                    {
                        XElement wpnRange = wpn.Element("Range");
                        if (wpnRange == null)
                            throw new Exception("Weapon has no range. " + path);

                        XElement wpnRoF = wpn.Element("RoF");
                        if (wpnRoF == null)
                            throw new Exception("Weapon has no RoF. " + path);

                        XElement wpnDmg = wpn.Element("Damage");
                        if (wpnDmg == null)
                            throw new Exception("Weapon has no Damage. " + path);

                        XElement wpnProjectileType = wpn.Element("Projectile");
                        if (wpnProjectileType == null)
                            throw new Exception("Weapon has no Projectile. " + path);

                        XElement wpnSplash = wpn.Element("Splash");
                        int wpnSplashValue;
                        if (wpnSplash == null)
                            wpnSplashValue = 0;
                        else
                            wpnSplashValue = int.Parse(wpnSplash.Value);

                        XElement wpnSplashDmgPercentage = wpn.Element("SplashDmgPercentage");
                        int wpnSplashDmgPercentageValue;
                        if (wpnSplashDmgPercentage == null)
                            wpnSplashDmgPercentageValue = 0;
                        else
                            wpnSplashDmgPercentageValue = int.Parse(wpnSplashDmgPercentage.Value);

                        XElement wpnAttackableTargets = wpn.Element("AttackableTargets");
                        eAttackableTargets wpnAttackableTargetsValue;
                        if (wpnAttackableTargets == null)
                            wpnAttackableTargetsValue = eAttackableTargets.Ground;
                        else
                            wpnAttackableTargetsValue = (eAttackableTargets)Enum.Parse(typeof(eAttackableTargets), required.Value);

                        WeaponStruct ws = new WeaponStruct(int.Parse(wpnRange.Value), int.Parse(wpnRoF.Value), int.Parse(wpnDmg.Value), (eProjectile)Enum.Parse(typeof(eProjectile), wpnProjectileType.Value), wpnSplashValue, wpnSplashDmgPercentageValue, wpnAttackableTargetsValue);

                        XElement wpnProjectileSpawnOffset = wpn.Element("ProjectileSpawnOffset");
                        if (wpnProjectileSpawnOffset != null)
                            ws.ProjectileSpawnOffset = Common.Str2Vector(wpnProjectileSpawnOffset.Value);

                        XElement wpnCritical;
                        wpnCritical = wpn.Element("CritRate");
                        if (wpnCritical != null)
                            ws.CritChance = int.Parse(wpnCritical.Value);

                        XElement wpnCritMultiplier;
                        wpnCritMultiplier = wpn.Element("CritMultiplier");
                        if (wpnCritMultiplier != null)
                            ws.CritMultiplier = int.Parse(wpnCritMultiplier.Value);

                        XElement shootFXNode = wpn.Element("ShootFX");
                        if (shootFXNode != null)
                            ws.ShootFX = int.Parse(shootFXNode.Value);
                        XElement impactFXNode = wpn.Element("ImpactFX");
                        if (impactFXNode != null)
                            ws.ImpactFX = int.Parse(impactFXNode.Value);

                        #region Modifiers
                        XElement modifierNode = wpn.Element("Stunchance");
                        if (modifierNode != null)
                            ws.WeaponModifiers.StunChance = int.Parse(modifierNode.Value);
                        modifierNode = null;
                        modifierNode = wpn.Element("Slows");
                        if (modifierNode != null)
                            ws.WeaponModifiers.Slows = bool.Parse(modifierNode.Value);
                        modifierNode = null;
                        modifierNode = wpn.Element("RootChance");
                        if (modifierNode != null)
                            ws.WeaponModifiers.RootChance = int.Parse(modifierNode.Value);
                        modifierNode = null;
                        modifierNode = wpn.Element("ArmorReductionValue");
                        if (modifierNode != null)
                            ws.WeaponModifiers.ArmorReductionValue = int.Parse(modifierNode.Value);
                        modifierNode = null;
                        #endregion

                        ts.Weapons.Add(ws);
                    }
                }
                #endregion

                #region Spawn
                XElement spawnMainNode = doc.Root.Element("Spawns");
                if (spawnMainNode != null)
                {
                    ts.MaxRallyPointRange = int.Parse(spawnMainNode.Attribute("maxRallyPointRange").Value);
                    ts.RallyPointPersuitRange = int.Parse(spawnMainNode.Attribute("persuitRange").Value);
                    foreach (XElement spawnNode in spawnMainNode.Elements())
                    {
                        ts.Spawns = true;
                        int defID = int.Parse(spawnNode.Element("ID").Value);
                        DefenderSpawnStruct ds = new DefenderSpawnStruct(defID);

                        XElement initialAmountNode = spawnNode.Element("InitialAmount");
                        if (initialAmountNode != null)
                            ds.InitialAmount = int.Parse(initialAmountNode.Value);

                        XElement spawnUpgCost = spawnNode.Element("UpgCntCost");
                        if (spawnUpgCost != null)
                            ds.UpgCntCost = int.Parse(spawnUpgCost.Value);

                        XElement maxNode = spawnNode.Element("Max");
                        if (maxNode != null)
                            ds.Max = int.Parse(maxNode.Value);

                        ts.Defenders.Add(ds);
                    }
                }
                #endregion

                #region Upgrades (to another tower)
                XElement towerUpgradeMainNode = doc.Root.Element("Upgrades");
                if (towerUpgradeMainNode != null)
                {
                    foreach (XElement towerUpgNode in towerUpgradeMainNode.Elements())
                    {
                        ts.Upgrades.Add(new TowerUpgrade(int.Parse(towerUpgNode.Element("NewTowerID").Value),
                                                         int.Parse(towerUpgNode.Element("Cost").Value),
                                                         ts.Icon
                                                        )
                                       );
                    }
                }
                #endregion

                #region Categories
                XElement catMainNode = doc.Root.Element("Categories");
                if (catMainNode != null)
                {
                    foreach (XElement catIDNode in catMainNode.Elements())
                        ts.Categories.Add(int.Parse(catIDNode.Value));
                }
                #endregion

                #region info
                XElement infoMainNode = doc.Root.Element("Info");
                if (infoMainNode != null)
                {
                    foreach (XElement infoNode in infoMainNode.Elements())
                        ts.Info.Add(new StringBuilder(infoNode.Value));
                }
                #endregion

                #region Requirements
                XElement requiredMainNode = doc.Root.Element("Requirements");
                if (requiredMainNode != null)
                {
                    foreach (XElement requiredOrNode in requiredMainNode.Elements())
                    {
                        List<int> requirementsAND = new List<int>();
                        foreach (XElement requiredAndNode in requiredOrNode.Elements())
                            requirementsAND.Add(int.Parse(requiredAndNode.Value));
                        ts.Requirements.Add(requirementsAND);
                    }
                }
                #endregion

                Towers.Add(ts);
            }
        }

        public static void LoadRunners()
        {
            Runners.Clear();
            string[] enemyXmls = Directory.GetFiles("Data/Enemies", "*.xml");

            foreach (string path in enemyXmls)
            {
                XDocument doc = XDocument.Load(path);

                // ID
                RunnerStruct rs = new RunnerStruct(int.Parse(doc.Root.Element("ID").Value));

                #region Basic
                XElement BasicNode = doc.Root.Element("Basic");
                if (BasicNode != null)
                {
                    XElement basic;

                    basic = BasicNode.Element("HP");
                    if (basic != null)
                        rs.HP = int.Parse(basic.Value);
                    basic = null;

                    basic = BasicNode.Element("HPRegen");
                    if (basic != null)
                        rs.HPRegen = int.Parse(basic.Value);
                    basic = null;

                    basic = BasicNode.Element("Bounty");
                    if (basic != null)
                        rs.Bounty = int.Parse(basic.Value);
                    basic = null;

                    basic = BasicNode.Element("Velocity");
                    if (basic != null)
                        rs.Velocity = float.Parse(basic.Value);
                    basic = null;

                    basic = BasicNode.Element("IsMelee");
                    if (basic != null)
                        rs.IsMelee = bool.Parse(basic.Value);
                    basic = null;

                    basic = BasicNode.Element("IsGround");
                    if (basic != null)
                        rs.IsGround = bool.Parse(basic.Value);
                    basic = null;

                    basic = BasicNode.Element("Recycles");
                    if (basic != null)
                        rs.Recycles = bool.Parse(basic.Value);
                    basic = null;

                    basic = BasicNode.Element("Armor");
                    if (basic != null)
                        rs.Armor = (eArmorType)Enum.Parse(typeof(eArmorType), basic.Value);
                    basic = null;

                    basic = BasicNode.Element("Name");
                    if (basic != null)
                        rs.Name = basic.Value;
                    else
                        throw new NullReferenceException("The name MUST be supplied. " + path);
                    basic = null;

                    basic = BasicNode.Element("Animation");
                    if (basic != null)
                        rs.AnimationType = (eAnimation)Enum.Parse(typeof(eAnimation), basic.Value);
                    else
                        throw new NullReferenceException("The animation MUST be supplied. " + path);
                    basic = null;
                }
                else
                    throw new NullReferenceException("The Basic Node MUST be supplied. " + path);
                #endregion

                #region Immunities
                XElement ImmunitiesNode = doc.Root.Element("Immunities");
                if (ImmunitiesNode != null)
                {
                    XElement immunity;

                    immunity = ImmunitiesNode.Element("Stun");
                    if (immunity != null)
                        rs.ImmuneStun = bool.Parse(immunity.Value);
                    immunity = null;

                    immunity = ImmunitiesNode.Element("Slow");
                    if (immunity != null)
                        rs.ImmuneSlow = bool.Parse(immunity.Value);
                    immunity = null;

                    immunity = ImmunitiesNode.Element("Rooted");
                    if (immunity != null)
                        rs.ImmuneRooted = bool.Parse(immunity.Value);
                    immunity = null;

                    immunity = ImmunitiesNode.Element("ArmorReduction");
                    if (immunity != null)
                        rs.ImmuneArmorReduction = bool.Parse(immunity.Value);
                    immunity = null;
                }
                #endregion

                #region Weapons
                XElement wpnMainNode = doc.Root.Element("Weapons");
                if (wpnMainNode != null)
                {
                    foreach (XElement wpnNode in wpnMainNode.Elements())
                    {
                        int splashValue = 0;
                        XElement splash = wpnNode.Element("Splash");
                        if (splash != null)
                            splashValue = int.Parse(splash.Value);

                        int splashDmgPercValue = 0;
                        XElement splashDmgPerc = wpnNode.Element("SplashDmgPercentage");
                        if (splashDmgPerc != null)
                            splashDmgPercValue = int.Parse(splash.Value);

                        int critRateValue = 0;
                        XElement critRate = wpnNode.Element("CritRate");
                        if (critRate != null)
                            critRateValue = int.Parse(splash.Value);

                        eDamageType dmgTypeValue = eDamageType.Normal;
                        XElement dmgType = wpnNode.Element("DamageType");
                        if (dmgType != null)
                            dmgTypeValue = (eDamageType)Enum.Parse(typeof(eDamageType), dmgType.Value);

                        int critMultiplierValue = 0;
                        XElement critMultiplier = wpnNode.Element("CritMultiplier");
                        if (critMultiplier != null)
                            critMultiplierValue = int.Parse(splash.Value);

                        XElement projectileNode = wpnNode.Element("Projectile");
                        eProjectile projectileType;
                        if (projectileNode != null)
                            projectileType = (eProjectile)Enum.Parse(typeof(eProjectile), projectileNode.Value);
                        else
                            projectileType = eProjectile.None;

                        WeaponStruct ws;
                        if (projectileType != eProjectile.None)
                            ws = new WeaponStruct(int.Parse(wpnNode.Element("Range").Value), int.Parse(wpnNode.Element("RoF").Value), float.Parse(wpnNode.Element("Damage").Value), projectileType, splashValue, splashDmgPercValue, eAttackableTargets.All) { DamageType = dmgTypeValue };
                        else
                            ws = new WeaponStruct(int.Parse(wpnNode.Element("Range").Value), int.Parse(wpnNode.Element("RoF").Value), float.Parse(wpnNode.Element("Damage").Value), splashValue, splashDmgPercValue, eAttackableTargets.All) { DamageType = dmgTypeValue };

                        XElement shootFXNode = wpnNode.Element("ShootFX");
                        if (shootFXNode != null)
                            ws.ShootFX = int.Parse(shootFXNode.Value);
                        XElement impactFXNode = wpnNode.Element("ImpactFX");
                        if (impactFXNode != null)
                            ws.ImpactFX = int.Parse(impactFXNode.Value);

                        #region Modifiers
                        XElement modifierNode = wpnNode.Element("Stunchance");
                        if (modifierNode != null)
                            ws.WeaponModifiers.StunChance = int.Parse(modifierNode.Value);
                        modifierNode = null;
                        modifierNode = wpnNode.Element("Slows");
                        if (modifierNode != null)
                            ws.WeaponModifiers.Slows = bool.Parse(modifierNode.Value);
                        modifierNode = null;
                        modifierNode = wpnNode.Element("RootChance");
                        if (modifierNode != null)
                            ws.WeaponModifiers.RootChance = int.Parse(modifierNode.Value);
                        modifierNode = null;
                        modifierNode = wpnNode.Element("ArmorReductionValue");
                        if (modifierNode != null)
                            ws.WeaponModifiers.ArmorReductionValue = int.Parse(modifierNode.Value);
                        modifierNode = null;
                        #endregion

                        rs.Weapons.Add(ws);
                    }
                }
                #endregion

                Runners.Add(rs);
            }
        }

        public static void LoadDefenders()
        {
            Defenders.Clear();
            string[] defenderXmls = Directory.GetFiles("Data/Defenders", "*.xml");

            foreach (string path in defenderXmls)
            {
                XDocument doc = XDocument.Load(path);
                DefenderStruct ds;

                XElement IDNode = doc.Root.Element("ID");
                if (IDNode != null)
                    ds = new DefenderStruct(int.Parse(IDNode.Value));
                else
                    throw new NullReferenceException("ID node missing. " + path);

                #region General
                XElement generalNode;

                generalNode = doc.Root.Element("Name");
                if (generalNode != null)
                    ds.Name = generalNode.Value;
                else
                    throw new NullReferenceException("Name node missing. " + path);
                generalNode = null;

                generalNode = doc.Root.Element("Description");
                if (generalNode != null)
                    ds.Desc = generalNode.Value;
                else
                    throw new NullReferenceException("Desc node missing. " + path);
                generalNode = null;

                generalNode = doc.Root.Element("HP");
                if (generalNode != null)
                    ds.HP = int.Parse(generalNode.Value);
                else
                    throw new NullReferenceException("HP node missing. " + path);
                generalNode = null;

                generalNode = doc.Root.Element("IsMelee");
                if (generalNode != null)
                    ds.IsMelee = bool.Parse(generalNode.Value);
                generalNode = null;

                generalNode = doc.Root.Element("MeleeSightRange");
                if (generalNode != null)
                    ds.MeleeSightRange = int.Parse(generalNode.Value);
                generalNode = null;

                generalNode = doc.Root.Element("SpawnDelay");
                if (generalNode != null)
                    ds.SpawnDelay = int.Parse(generalNode.Value);
                generalNode = null;

                generalNode = doc.Root.Element("Armor");
                if (generalNode != null)
                    ds.ArmorType = (eArmorType)Enum.Parse(typeof(eArmorType), generalNode.Value);
                generalNode = null;

                generalNode = doc.Root.Element("Animation");
                if (generalNode != null)
                    ds.AnimationType = (eAnimation)Enum.Parse(typeof(eAnimation), generalNode.Value);
                else
                    throw new NullReferenceException("Animation node missing. " + path);
                generalNode = null;

                generalNode = doc.Root.Element("Velocity");
                if (generalNode != null)
                    ds.Velocity = float.Parse(generalNode.Value);
                generalNode = null;

                generalNode = doc.Root.Element("IsGround");
                if (generalNode != null)
                    ds.IsGround = bool.Parse(generalNode.Value);
                generalNode = null;

                generalNode = doc.Root.Element("HPRegen");
                if (generalNode != null)
                    ds.HPRegen = int.Parse(generalNode.Value);
                generalNode = null;

                #endregion

                #region Weapons
                XElement weaponsMainNode = doc.Root.Element("Weapons");
                if (weaponsMainNode != null)
                {
                    foreach (XElement wpnNode in weaponsMainNode.Elements())
                    {
                        XElement wpnRange = wpnNode.Element("Range");
                        int wpnRangeValue = 0;
                        if (wpnRange != null)
                            wpnRangeValue = int.Parse(wpnRange.Value);

                        XElement wpnRoF = wpnNode.Element("RoF");
                        int wpnRoFValue = 0;
                        if (wpnRoF != null)
                            wpnRoFValue = int.Parse(wpnRoF.Value);

                        XElement wpnDmg = wpnNode.Element("Damage");
                        int wpnDmgValue = 0;
                        if (wpnDmg != null)
                            wpnDmgValue = int.Parse(wpnDmg.Value);

                        XElement wpnSplash = wpnNode.Element("Splash");
                        int wpnSplashValue = 0;
                        if (wpnSplash != null)
                            wpnSplashValue = int.Parse(wpnSplash.Value);

                        XElement wpnSplashDmgPerc = wpnNode.Element("SplashDmgPercentage");
                        int wpnSplashDmgPercValue = 0;
                        if (wpnSplashDmgPerc != null)
                            wpnSplashDmgPercValue = int.Parse(wpnDmg.Value);

                        XElement wpnAttackableTargets = wpnNode.Element("AttackableTargets");
                        eAttackableTargets wpnAttackableTargetsValue;
                        if (wpnAttackableTargets == null)
                            wpnAttackableTargetsValue = eAttackableTargets.Ground;
                        else
                            wpnAttackableTargetsValue = (eAttackableTargets)Enum.Parse(typeof(eAttackableTargets), wpnAttackableTargets.Value);

                        XElement dmgType = wpnNode.Element("DamageType");
                        eDamageType dmgTypeValue = eDamageType.Normal;
                        if (dmgType != null)
                            dmgTypeValue = (eDamageType)Enum.Parse(typeof(eDamageType), dmgType.Value);

                        WeaponStruct ws = new WeaponStruct(wpnRangeValue, wpnRoFValue, wpnDmgValue, wpnSplashValue, wpnSplashDmgPercValue, wpnAttackableTargetsValue) { DamageType = dmgTypeValue };

                        XElement shootFXNode = wpnNode.Element("ShootFX");
                        if (shootFXNode != null)
                            ws.ShootFX = int.Parse(shootFXNode.Value);
                        XElement impactFXNode = wpnNode.Element("ImpactFX");
                        if (impactFXNode != null)
                            ws.ImpactFX = int.Parse(impactFXNode.Value);

                        #region Modifiers
                        XElement modifierNode = wpnNode.Element("Stunchance");
                        if (modifierNode != null)
                            ws.WeaponModifiers.StunChance = int.Parse(modifierNode.Value);
                        modifierNode = null;
                        modifierNode = wpnNode.Element("Slows");
                        if (modifierNode != null)
                            ws.WeaponModifiers.Slows = bool.Parse(modifierNode.Value);
                        modifierNode = null;
                        modifierNode = wpnNode.Element("RootChance");
                        if (modifierNode != null)
                            ws.WeaponModifiers.RootChance = int.Parse(modifierNode.Value);
                        modifierNode = null;
                        modifierNode = wpnNode.Element("ArmorReductionValue");
                        if (modifierNode != null)
                            ws.WeaponModifiers.ArmorReductionValue = int.Parse(modifierNode.Value);
                        modifierNode = null;
                        #endregion

                        ds.Weapons.Add(ws);
                    }
                }
                #endregion

                Defenders.Add(ds);
            }
        }

        public static void LoadTowerCategories()
        {
            TowerCategories.Clear();
            XDocument doc = XDocument.Load("Data/Misc/TowerCategories.xml");

            foreach (XElement catNode in doc.Root.Elements())
            {
                TowerCategoryStruct tcs = new TowerCategoryStruct(int.Parse(catNode.Element("ID").Value), catNode.Element("Text").Value, catNode.Element("Icon").Value);
                foreach (TowerStruct ts in Towers)
                {
                    if (ts.Categories.Contains(tcs.ID))
                        tcs.TowersInThisCat.Add(ts);
                }

                #region info
                XElement infoMainNode = doc.Root.Element("Info");
                if (infoMainNode != null)
                {
                    foreach (XElement infoNode in infoMainNode.Elements())
                        tcs.Info.Add(new StringBuilder(infoNode.Value));
                }
                #endregion

                TowerCategories.Add(tcs);
            }
        }
    }

    public struct TowerCategoryStruct
    {
        public int ID;
        public StringBuilder Text;
        public Texture2D Icon;
        public List<TowerStruct> TowersInThisCat;
        public List<StringBuilder> Info;

        public TowerCategoryStruct(int id, string text, string icon)
        {
            ID = id;
            Text = new StringBuilder(text, text.Length);
            Icon = Common.str2Tex("Icons/" + icon);
            TowersInThisCat = new List<TowerStruct>();
            Info = new List<StringBuilder>();
        }
    }

    public struct DefenderStruct
    {
        public int ID;
        public string Name;
        public string Desc;
        public int HP;
        public bool IsMelee;
        public int MeleeSightRange;
        public int SpawnDelay;
        public eArmorType ArmorType;
        public float Velocity;
        public int HPRegen;
        public bool IsGround;

        public List<WeaponStruct> Weapons;

        public eAnimation AnimationType;
        public Color DrawColor;

        public DefenderStruct(int id)
        {
            ID = id;
            Name = "<NAME NOT SET>";
            Desc = "<DESC NOT SET>";
            HP = 0;
            IsMelee = true;
            MeleeSightRange = 48;
            SpawnDelay = 1500;
            ArmorType = eArmorType.Medium;
            Velocity = 1.5f;
            HPRegen = 0;
            IsGround = true;

            Weapons = new List<WeaponStruct>();

            AnimationType = eAnimation.None;
            DrawColor = Color.White;
        }
    }

    public struct TowerUpgrade
    {
        public int NewTowerID; // ID of the tower to upgrade too
        public int Cost; // Cost to upgrade
        public Texture2D Icon; // The upgrade icon

        public TowerUpgrade(int newTowerID, int cost, Texture2D icon)
        {
            NewTowerID = newTowerID;
            Cost = cost;
            Icon = icon;
        }
    }

    public class WeaponModifiers
    {
        public int StunChance;
        public int RootChance;
        public int ArmorReductionValue;
        public bool Slows;

        public WeaponModifiers()
        {
            StunChance = 0;
            RootChance = 0;
            ArmorReductionValue = 0;
            Slows = false;
        }
    }

    public struct WeaponStruct
    {
        public int Range;
        public int AttackDelay;
        public int CritChance; // 0-100
        public int CritMultiplier;
        public float Damage;
        public eProjectile ProjectileType;
        public int Splash;
        public int SplashDmgPerc;
        public bool IsMelee;
        public eAttackableTargets AttackableTargets;
        public Vector2 ProjectileSpawnOffset;
        public eDamageType DamageType;
        public int ShootFX;
        public int ImpactFX;

        public WeaponModifiers WeaponModifiers;

        /// <summary>
        /// Projectile constructor
        /// </summary>
        /// <param name="range"></param>
        /// <param name="attackDelay"></param>
        /// <param name="damage"></param>
        /// <param name="projectileType"></param>
        /// <param name="splash"></param>
        /// <param name="splashDmgPerc"></param>
        public WeaponStruct(int range, int attackDelay, float damage, eProjectile projectileType, int splash, int splashDmgPerc, eAttackableTargets attackableTargets)
        {
            Range = range;
            AttackDelay = attackDelay;
            CritChance = 0;
            CritMultiplier = 2;
            Damage = damage;
            ProjectileType = projectileType;
            Splash = splash;
            SplashDmgPerc = splashDmgPerc;
            IsMelee = false;
            AttackableTargets = attackableTargets;
            ProjectileSpawnOffset = Vector2.Zero;
            DamageType = eDamageType.Normal;
            ShootFX = ImpactFX = -1;
            WeaponModifiers = new WeaponModifiers();
        }

        /// <summary>
        /// Melee constructor
        /// </summary>
        /// <param name="range"></param>
        /// <param name="attackDelay"></param>
        /// <param name="damage"></param>
        /// <param name="projectileType"></param>
        /// <param name="splash"></param>
        /// <param name="splashDmgPerc"></param>
        public WeaponStruct(int range, int attackDelay, float damage, int splash, int splashDmgPerc, eAttackableTargets attackableTargets)
        {
            Range = range;
            AttackDelay = attackDelay;
            CritChance = 0;
            CritMultiplier = 2;
            Damage = damage;
            ProjectileType = eProjectile.None;
            Splash = splash;
            SplashDmgPerc = splashDmgPerc;
            IsMelee = true;
            AttackableTargets = attackableTargets;
            ProjectileSpawnOffset = Common.InvalidVector2;
            DamageType = eDamageType.Normal;
            ShootFX = ImpactFX = -1;
            WeaponModifiers = new WeaponModifiers();
        }
    }

    public struct DefenderSpawnStruct
    {
        public int ID;
        public int InitialAmount;
        public int Max;
        public int UpgCntCost;

        public DefenderSpawnStruct(int id)
        {
            ID = id;
            InitialAmount = 1;
            Max = 1;
            UpgCntCost = 0;
        }
    }

   

    public struct TowerStruct
    {
        public int ID;
        public string Name;
        public string Desc;
        public int Cost;
        public eAnimation AnimationType;
        public Color DrawColor;
        public Point BuildSize; // in grid squares
        public int BuildTimeInMS;
        public int UpgTimeInMS;
        public int MaxRallyPointRange;
        public int RallyPointPersuitRange;
        public int SupplyCost;
        public Texture2D Icon;
        public List<int> Categories;

        public List<StringBuilder> Info;
        public List<WeaponStruct> Weapons;
        public List<TowerUpgrade> Upgrades;
        public List<List<int>> Requirements;

        // Income
        public int IncomePerWave;
        public int IncomePerTick;
        public int IncomeTickDelayInMS;

        // Spawn
        public bool Spawns;
        public int SpawnRange;
        public List<DefenderSpawnStruct> Defenders;

        public TowerStruct(int id)
        {
            ID = id;
            Name = "<NAME NOT SET>";
            Desc = "<DESC NOT SET>";

            BuildTimeInMS = 0;
            UpgTimeInMS = 0;
            BuildSize = new Point(2, 2);
            Cost = 0;
            SupplyCost = 1;

            Spawns = false;
            SpawnRange = 0;
            Defenders = new List<DefenderSpawnStruct>();
            Upgrades = new List<TowerUpgrade>();
            MaxRallyPointRange = 0;
            RallyPointPersuitRange = 0;
            Icon = null;

            IncomePerWave = 0;
            IncomePerTick = 0;
            IncomeTickDelayInMS = 0;

            AnimationType = eAnimation.None;
            DrawColor = Color.White;

            Weapons = new List<WeaponStruct>();
            Categories = new List<int>();
            Info = new List<StringBuilder>();
            Requirements = new List<List<int>>();
        }
    }

    public struct RunnerStruct
    {
        public int ID;

        public string Name;
        public int HP;
        public float HPRegen;
        public float Velocity;
        public bool IsMelee;
        public bool IsGround;
        public bool Recycles;
        public int Bounty;
        public eArmorType Armor;
        /// <summary>
        /// The damage that this runner causes when it reaches the finish.
        /// </summary>
        public int FinishDamage;

        public List<WeaponStruct> Weapons;

        #region Immunities
        public bool ImmuneStun;
        public bool ImmuneSlow;
        public bool ImmuneArmorReduction;
        public bool ImmuneRooted;
        #endregion

        #region Animation
        public eAnimation AnimationType;
        public Color DrawColor;
        #endregion

        public RunnerStruct(int id)
        {
            ID = id;

            Name = "<NAME NOT SET>";
            HP = 0;
            HPRegen = 0;
            Velocity = 0f;
            IsMelee = true;
            IsGround = true;
            Bounty = 0;
            Recycles = false;
            FinishDamage = 1;
            Armor = eArmorType.Medium;

            ImmuneStun = ImmuneSlow = ImmuneRooted = ImmuneArmorReduction = false;
            Weapons = new List<WeaponStruct>();

            AnimationType = eAnimation.None;
            DrawColor = Color.White;
        }
    }
}