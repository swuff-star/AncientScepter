using Mono.Cecil.Cil;
using MonoMod.Cil;
using EntityStates.TitanMonster;
using R2API;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using System;
using UnityEngine;
using static AncientScepter.SkillUtil;

namespace AncientScepter.ScepterSkillsMonster
{
    public class AurelioniteEyeLaser2 : ScepterSkill
    {
        public override SkillDef myDef { get; protected set; }

        public override string oldDescToken { get; protected set; }
        public override string newDescToken { get; protected set; }
        public override string overrideStr => "\n<color=#d299ff>SCEPTER: Deals an extra 35% wall-piercing damage.</color>";

        public override string targetBody => "TitanGoldBody";
        public override SkillSlot targetSlot => SkillSlot.Special;
        public override int targetVariantIndex => 0;

        internal override void SetupAttributes()
        {
            var oldDef = LegacyResourcesAPI.Load<SkillDef>("SkillDefs/TitanGoldBody/ChargeGoldLaser");
            myDef = CloneSkillDef(oldDef);

            var nametoken = "ANCIENTSCEPTER_TITANGOLD_MEGALASERNAME";
            newDescToken = "ANCIENTSCEPTER_TITANGOLD_MEGALASERDESC";
            oldDescToken = oldDef.skillDescriptionToken;
            var namestr = "Piercing Eye Laser";
            LanguageAPI.Add(nametoken, namestr);

            myDef.skillName = $"{oldDef.skillName}Scepter";
            (myDef as ScriptableObject).name = myDef.skillName;
            myDef.skillNameToken = nametoken;
            myDef.skillDescriptionToken = newDescToken;
            myDef.icon = Assets.mainAssetBundle.LoadAsset<Sprite>("texArtiR1");

            ContentAddition.AddSkillDef(myDef);
        }

        internal override void LoadBehavior()
        {
            if (AncientScepterItem.enableMonsterSkills)
                On.EntityStates.TitanMonster.FireMegaLaser.FireBullet += FireMegaLaser_FireBullet;
        }

        internal override void UnloadBehavior()
        {
            On.EntityStates.TitanMonster.FireMegaLaser.FireBullet -= FireMegaLaser_FireBullet;
        }

        private void FireMegaLaser_FireBullet(On.EntityStates.TitanMonster.FireMegaLaser.orig_FireBullet orig, FireMegaLaser self, Transform modelTransform, Ray aimRay, string targetMuzzle, float maxDistance)
        {
            if (self is FireGoldMegaLaser && AncientScepterItem.instance.GetCountEffective(self.outer.commonComponents.characterBody) > 0)
            {
                if (self.isAuthority && self.lockedOnHurtBox && self.lockedOnHurtBox.healthComponent)
                {
                    var damageInfo = new DamageInfo
                    {
                        attacker = self.gameObject,
                        crit = self.RollCrit(),
                        damage = FireMegaLaser.damageCoefficient * self.damageStat / FireMegaLaser.fireFrequency * 0.35f,
                        inflictor = self.gameObject,
                        position = self.lockedOnHurtBox.transform.position,
                        procCoefficient = 0f,
                        procChainMask = default,
                    };
                    self.lockedOnHurtBox.healthComponent.TakeDamage(damageInfo);
                }
            }
            orig(self, modelTransform, aimRay, targetMuzzle, maxDistance);
        }
    }
}