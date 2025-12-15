using BepInEx.Configuration;
using R2API;
using RoR2;
using RoR2.Skills;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static AncientScepter.ItemHelpers;
using static AncientScepter.MiscUtil;
using static AncientScepter.SkillUtil;
using AncientScepter.ScepterSkillsMonster;
using System.Runtime.CompilerServices;
using RoR2.Modding;
using UnityEngine.AddressableAssets;

namespace AncientScepter
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public abstract class ScepterSkill
    {
        public abstract SkillDef myDef { get; protected set; }
        public abstract string oldDescToken { get; protected set; }
        public abstract string newDescToken { get; protected set; }
        public abstract string overrideStr { get; }

        internal abstract void SetupAttributes();

        internal virtual void LoadBehavior()
        {
        }

        internal virtual void UnloadBehavior()
        {
        }

        public abstract string targetBody { get; }
        public abstract SkillSlot targetSlot { get; }
        public abstract int targetVariantIndex { get; }
    }

    public class AncientScepterItem : ItemBase<AncientScepterItem>
    {
        public static RerollMode rerollMode;
        public static UnusedMode unusedMode;
        public static bool enableMonsterSkills;
        //public static bool enableBrotherEffects;
        public static StridesInteractionMode stridesInteractionMode;
        public static bool altModel;
        public static bool removeClassicItemsScepterFromPool;
        public static bool enableSOTVTransforms;

        // Artificer
        public static bool artiFlamePerformanceMode;

        // Bandit

        // Captain
        public static bool captainNukeFriendlyFire;

        // Commando
        //public static bool enableCommandoAutoaim;

        // Croco

        // Engi
        public static bool engiTurretAdjustCooldown;
        public static bool engiWalkerAdjustCooldown;
        public static bool turretBlacklist;

        // Heretic

        // Huntress

        // Loader

        // Merc

        // Railgunner

        // Toolbot

        // Treebot

        // Void Fiend

        //TODO: test w/ stage changes ?

        //Mithrix
        //public static bool mithrixEnableScepter;

        public enum StridesInteractionMode
        {
            HeresyTakesPrecedence, ScepterTakesPrecedence, ScepterRerolls
        }

        public enum RerollMode
        {
            Disabled, 
            Random, 
            Scrap
        }
        public enum UnusedMode
        {
            Keep,
            Reroll,
            Metamorphosis,
        }

        public override string ItemName => "Ancient Scepter";

        public override string ItemLangTokenName => "ANCIENT_SCEPTER";

        public override string ItemPickupDesc => "Upgrades one of your skills.";

        public override string ItemFullDescription =>
            $"Upgrade one of your <style=cIsUtility>skills</style>. " + $"{(rerollMode != RerollMode.Disabled ? "<style=cStack>(Unique per survivor)</style>." : "Reduce skill cooldown by <style=cIsUtility>0%</style> <style=cStack>(+30% per stack. Unique per survivor)</style>.")}"
                        + $"\n<style=cStack>{(rerollMode != RerollMode.Disabled ? "Extra/Unusable" : "Unusable")} pickups will reroll into {(rerollMode == RerollMode.Scrap ? "Item Scrap, Red" : "other Legendary items.")}</style>";

        public override string ItemLore => "Perfected energies. <He> holds it before us. The crystal of foreign elements is not attached physically, yet it does not falter from the staff's structure.\n\nOverwhelming strength. We watch as <His> might splits the ground asunder with a single strike.\n\nWondrous possibilities. <His> knowledge unlocks further pathways of development. We are enlightened by <Him>.\n\nExcellent results. From <His> hands, [Nanga] takes hold. It is as <He> said: The weak are culled.\n\nRisking everything. The crystal destabilizies. [Nanga] is gone, and <He> is forced to wield it once again.\n\nPower comes at a cost. <He> is willing to pay.";

        public override ItemTier Tier => ItemTier.Tier3;
        public override ItemTag[] ItemTags => EvaluateItemTags();

        private string AssetName => altModel ? "AghanimScepter" : "AncientScepter";

        private GameObject itemModel;
        public override GameObject ItemModel {
            get {
                if (itemModel == null) {
                    itemModel = Assets.mainAssetBundle.LoadAsset<GameObject>($"mdl{AssetName}Pickup");
                }
                return itemModel;
            }
        }

        private GameObject itemDisplay;
        public override GameObject ItemDisplay
        {
            get
            {
                if (itemDisplay == null)
                {
                    itemDisplay = Assets.mainAssetBundle.LoadAsset<GameObject>($"mdl{AssetName}Display");
                }
                return itemDisplay;
            }
        }

        public override Sprite ItemIcon => Assets.mainAssetBundle.LoadAsset<Sprite>($"tex{AssetName}Icon");
        public override bool TILER2_MimicBlacklisted => true;

        public override bool AIBlacklisted => true;

        public static GameObject ItemBodyModelPrefab;
        public static GameObject ancientWispPrefab;
        public static Material purpleFireMaterial;

        public override void Init(ConfigFile config)
        {
            ancientWispPrefab = LegacyResourcesAPI.Load<GameObject>("prefabs/characterbodies/AncientWispBody");
            RegisterSkills();
            CreateConfig(config);
            CreateLang();
            CreateItem();
            Hooks();
            Install();
            CreateCraftableDef();
            //InstallLanguage();
            On.RoR2.UI.MainMenu.MainMenuController.Start += MainMenuController_Start;
        }

        private void MainMenuController_Start(On.RoR2.UI.MainMenu.MainMenuController.orig_Start orig, RoR2.UI.MainMenu.MainMenuController self)
        {
            orig(self);
            On.RoR2.UI.MainMenu.MainMenuController.Start -= MainMenuController_Start;
        }

        private ItemTag[] EvaluateItemTags()
        {
            List<ItemTag> availableTags = new List<ItemTag>()
            {
                ItemTag.Utility,
                ItemTag.AIBlacklist,
                ItemTag.CanBeTemporary,
                //ItemTag.AllowedForUseAsCraftingIngredient, // re-enable when it has recipes implemented proper
            };
            if (turretBlacklist)
            {
                availableTags.Add(ItemTag.CannotCopy);
            }
            return availableTags.ToArray();
        }

        public override void CreateConfig(ConfigFile config)
        {
            string configCategory = "Item: " + ItemName;

            engiTurretAdjustCooldown = 
                config.Bind("Engineer", 
                            "TR12-C Gauss Compact Faster Recharge", 
                            false, 
                            "If true, TR12-C Gauss Compact will recharge faster to match the additional stock.").Value;
            engiWalkerAdjustCooldown = 
                config.Bind("Engineer", 
                            "TR58-C Carbonizer Mini Faster Recharge", 
                            false, 
                            "If true, TR58-C Carbonizer Mini will recharge faster to match the additional stock.").Value;
            turretBlacklist = 
                config.Bind("Engineer", 
                            "Blacklist Turrets", 
                            false, 
                            "If true, turrets will be blacklisted from getting the Ancient Scepter." +
                            "\nIf false, they will get the scepter and will get rerolled depending on the reroll mode.").Value;
            artiFlamePerformanceMode = 
                config.Bind("Artificer", 
                            "ArtiFlamePerformance",
                            false, 
                            "If true, Dragon's Breath will use significantly lighter particle effects and no dynamic lighting.").Value;
            captainNukeFriendlyFire = 
                config.Bind("Captain", 
                            "Captain Nuke Friendly Fire", 
                            false, 
                            "If true, then Captain's Scepter Nuke will also inflict blight on allies.").Value;
            rerollMode = 
                config.Bind(configCategory, 
                            "Reroll on pickup mode", 
                            RerollMode.Disabled, 
                            "If \"Disabled\", additional stacks will not be rerolled. Gaining extra stacks reduces skill cooldown." +
                            "\nIf \"Random\", any stacks picked up past the first will reroll to other red items." +
                            "\nIf \"Scrap\", any stacks picked up past the first will reroll into red scrap.").Value;
            unusedMode =
                config.Bind(configCategory,
                            "Unused mode",
                            UnusedMode.Metamorphosis,
                            "If \"Keep\", Characters which cannot benefit from the item will still keep it." +
                            "\nIf \"Reroll\", Characters without any scepter upgrades will reroll according to above pickup mode." +
                            "\nIf \"Metamorphosis\", Characters without scepter upgrades will only reroll if Artifact of Metamorphosis is not active.").Value;
            stridesInteractionMode = 
                config.Bind(configCategory, 
                            "Strides pickup mode", 
                            StridesInteractionMode.ScepterRerolls, 
                            "Changes what happens when a character whose skill is affected by Ancient Scepter has both Ancient Scepter and the corresponding heretic skill replacements (Visions/Hooks/Strides/Essence) at the same time.").Value; //defer until next stage
            enableMonsterSkills = 
                config.Bind(configCategory, 
                            "Enable skills for monsters", 
                            true, 
                            "If true, certain monsters get the effects of the Ancient Scepter.").Value;
            /*mithrixEnableScepter =
                config.Bind(configCategory,
                            "Enable Mithrix Lines",
                            true,
                            "If true, Mithrix will have additional dialogue when acquiring the Ancient Scepter. Only applies on Commencement. Requires Enable skills for monsters to be enabled.").Value;*/
            //enableBrotherEffects = config.Bind(configCategory, "Enable Mithrix Lines", true, "If true, Mithrix will have additional dialogue when acquiring the Ancient Scepter.").Value;
            //enableCommandoAutoaim = config.Bind(configCategory, "Enable Commando Autoaim", true, "This may break compatibiltiy with skills.").Value;
            altModel =
                config.Bind(configCategory,
                            "Alt Model",
                            false,
                            "Changes the model as a reference to a certain other scepter that upgrades abilities.").Value;
            removeClassicItemsScepterFromPool =
                config.Bind(configCategory,
                            "CLASSICITEMS: Remove Classic Items Ancient Scepter From Droplist If Installed",
                            true,
                            "If true, then the Ancient Scepter from Classic Items will be removed from the drop pool to prevent complications.").Value;

            var engiSkill = skills.First(x => x is EngiTurret2);
            engiSkill.myDef.baseRechargeInterval = EngiTurret2.oldDef.baseRechargeInterval * (engiTurretAdjustCooldown ? 2f / 3f : 1f);
            GlobalUpdateSkillDef(engiSkill.myDef);

            var engiSkill2 = skills.First(x => x is EngiWalker2);
            engiSkill2.myDef.baseRechargeInterval = EngiWalker2.oldDef.baseRechargeInterval / (engiWalkerAdjustCooldown ? 2f : 1f);
            GlobalUpdateSkillDef(engiSkill2.myDef);

            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.ThinkInvisible.ClassicItems") && removeClassicItemsScepterFromPool)
            {
                Run.onRunStartGlobal += RemoveClassicItemsScepter;
            }

            enableSOTVTransforms =
                config.Bind(configCategory,
                "Transformation Notification",
                true,
                "If true, then when scepters are re-rolled, then it will be accompanied by a transformation notification like other items.").Value;
        }

        protected override void CreateCraftableDef()
        {
            CraftableDef fromBottleAndBandolier = ScriptableObject.CreateInstance<CraftableDef>();
            fromBottleAndBandolier.pickup = ItemDef;
            fromBottleAndBandolier.recipes = new Recipe[]
            {
                new Recipe()
                {
                    amountToDrop = 1,
                    ingredients = new RecipeIngredient[]
                    {
                        new RecipeIngredient()
                        {
                            // bottled chaos
                            pickup = Addressables.LoadAssetAsync<ItemDef>("b4d935cc376fab14a9e1b3e0aa772536").WaitForCompletion()
                        },
                        new RecipeIngredient()
                        {
                            // bandolier
                            pickup = Addressables.LoadAssetAsync<ItemDef>("c2701e84e1d92a246a9adaffd41b56a5").WaitForCompletion()
                        }
                    }
                }
            };

            // and then it has to get registered to a contentpack and stuff not set up here and ugh nvm another time

            // other ideas:
            // Sonorous Whispers + Crowbar (desirable red + white 'staff' = desirable red staff)
            // Alien Head + ?? (skill-related red + ?? = scepter)
            // Scepter + Item Scrap, White = 4x Focus Crystal (break the scepter)
        }


        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void RemoveClassicItemsScepter(Run run)
        {
            if (ThinkInvisible.ClassicItems.Scepter.instance.itemDef?.itemIndex > ItemIndex.None)
                Run.instance.DisableItemDrop(ThinkInvisible.ClassicItems.Scepter.instance.itemDef.itemIndex);
        }

        public override ItemDisplayRuleDict CreateDisplayRules()
        {
            SetupMaterials(ItemModel);
            ItemDisplay disp = ItemModel.AddComponent<ItemDisplay>();
            disp.rendererInfos = Assets.SetupRendererInfos(ItemModel);


            displayPrefab = ItemDisplay;
            SetupMaterials(displayPrefab);
            disp = displayPrefab.AddComponent<ItemDisplay>();
            disp.rendererInfos = Assets.SetupRendererInfos(displayPrefab);

            ItemDisplayRuleDict rules = new ItemDisplayRuleDict(
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(0.1473F, -0.073F, -0.0935F),
                    localAngles = new Vector3(333.2843F, 198.8161F, 165.1177F),
                    localScale = new Vector3(0.2235F, 0.2235F, 0.2235F)
                });

            rules.Add("mdlHuntress", new ItemDisplayRule
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = displayPrefab,
                childName = "Pelvis",
                localPos = new Vector3(0F, 0.0638F, 0.0973F),
                localAngles = new Vector3(76.6907F, 0F, 0F),
                localScale = new Vector3(0.2812F, 0.2812F, 0.2812F)
            });

            rules.Add("mdlMage", new ItemDisplayRule
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = displayPrefab,
                childName = "HandR",
                localPos = new Vector3(-0.0021F, 0.1183F, 0.063F),
                localAngles = new Vector3(0F, 34.1F, 90F),
                localScale = new Vector3(0.4416F, 0.4416F, 0.4416F)
            });

            rules.Add("mdlEngi", new ItemDisplayRule
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = displayPrefab,
                childName = "CannonHeadR",
                localPos = new Vector3(0.0186F, 0.3435F, 0.2246F),
                localAngles = new Vector3(0F, 0F, 0F),
                localScale = new Vector3(0.5614F, 0.5614F, 0.5614F)
            });

            rules.Add("mdlMerc", new ItemDisplayRule
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = displayPrefab,
                childName = "Pelvis",
                localPos = new Vector3(0.1712F, 0F, 0F),
                localAngles = new Vector3(69.8111F, 180F, 180F),
                localScale = new Vector3(0.2679F, 0.2679F, 0.2679F)
            });

            rules.Add("mdlLoader", new ItemDisplayRule
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = displayPrefab,
                childName = "MechLowerArmR",
                localPos = new Vector3(0.0813F, 0.4165F, -0.0212F),
                localAngles = new Vector3(0F, 180F, 180F),
                localScale = new Vector3(0.4063F, 0.4063F, 0.4063F)
            });

            rules.Add("mdlCaptain", new ItemDisplayRule
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = displayPrefab,
                childName = "Chest",
                localPos = new Vector3(-0.0046F, 0.0099F, -0.286F),
                localAngles = new Vector3(10.4706F, 1.6895F, 24.8468F),
                localScale = new Vector3(0.4928F, 0.4928F, 0.4928F)
            });

            rules.Add("mdlToolbot", new ItemDisplayRule
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = displayPrefab,
                childName = "Chest",
                localPos = new Vector3(1.1191F, 0.358F, -1.6717F),
                localAngles = new Vector3(0F, 0F, 270F),
                localScale = new Vector3(2.4696F, 2.4696F, 2.4696F)
            });

            rules.Add("mdlTreebot", new ItemDisplayRule
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = displayPrefab,
                childName = "CalfFrontL",
                localPos = new Vector3(0F, 0.8376F, -0.1766F),
                localAngles = new Vector3(0F, 0F, 0F),
                localScale = new Vector3(0.8037F, 0.8037F, 0.8037F)
            });

            rules.Add("mdlCroco", new ItemDisplayRule
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = displayPrefab,
                childName = "MouthMuzzle",
                localPos = new Vector3(0F, 2.1215F, 2.9939F),
                localAngles = new Vector3(0F, 0F, 270F),
                localScale = new Vector3(5.2969F, 5.2969F, 5.2969F)
            });

            rules.Add("mdlBandit", new ItemDisplayRule
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = displayPrefab,
                childName = "Pelvis",
                localPos = new Vector3(-0.1152f, -0.1278f, 0.2056f),
                localAngles = new Vector3(20F, 285F, 10F),
                localScale = new Vector3(0.2235F, 0.2235F, 0.2235F)
            });

            rules.Add("mdlEnforcer",
                      ItemHelpers.CreateDisplayRule(displayPrefab,
                                                    "Pelvis",
                                                    new Vector3(-0.08448F, 0.00357F, -0.35566F),
                                                    new Vector3(43.57039F, 347.6845F, 69.64303F),
                                                    new Vector3(0.31291F, 0.31291F, 0.31291F)
                                                    )
                      );

            rules.Add("mdlNemforcer",
                      ItemHelpers.CreateDisplayRule(displayPrefab,
                                                    "Minigun",
                                                    new Vector3(0.00287F, -0.00305F, -0.03029F),
                                                    new Vector3(358.9499F, 89.5545F, 180.8908F),
                                                    new Vector3(0.00837F, 0.00837F, 0.00837F)
                                                    )
                      );

            rules.Add("mdlVoidSurvivor", new ItemDisplayRule
            {
                childName = "Hand",
                followerPrefab = displayPrefab,
                localPos = new Vector3(-0.02335F, 0.11837F, 0.11306F),
                localAngles = new Vector3(55.42191F, 299.1461F, 266.1845F),
                localScale = new Vector3(0.56092F, 0.56276F, 0.56092F)
            });

            rules.Add("mdlRailGunner", new ItemDisplayRule
            {
                childName = "ThighR",
                followerPrefab = displayPrefab,
                localPos = new Vector3(-0.11836F, 0.17205F, 0.0282F),
                localAngles = new Vector3(353.4687F, 184.4017F, 177.4758F),
                localScale = new Vector3(0.2235F, 0.2235F, 0.2235F)
            });
            
            rules.Add("mdlNemmando", new ItemDisplayRule
            {
                childName = "Sword",
                followerPrefab = displayPrefab,
                localPos = new Vector3(-0.00005576489F, 0.001674413F, -0.00002617424F),
                localAngles = new Vector3(1.114511F, 204.2958F, 177.8329F),
                localScale = new Vector3(0.0026F, 0.0026F, 0.0026F)
            });
            
            rules.Add("mdlHeretic", new ItemDisplayRule
            {
                childName = "ThighL",
                followerPrefab = displayPrefab,
                localPos = new Vector3(0.49264F, -0.16267F, -0.14486F),
                localAngles = new Vector3(9.97009F, 351.3801F, 100.2498F),
                localScale = new Vector3(0.5F, 0.5F, 0.5F)
            });

            rules.Add("mdlBrother", new ItemDisplayRule
            {
                childName = "HandL",
                followerPrefab = displayPrefab,
                localPos = new Vector3(-0.05066F, 0.13436F, 0.0282F),
                localAngles = new Vector3(79.95749F, 180F, 230.595F),
                localScale = new Vector3(0.4F, 0.4F, 0.4F)
            });
            return rules;
        }

        protected override void SetupMaterials(GameObject modelPrefab)
        {
            purpleFireMaterial = ancientWispPrefab.transform.Find("Model Base?/mdlAncientWisp/AncientWispArmature/chest/Fire, Main").GetComponent<ParticleSystemRenderer>().material;
            modelPrefab.GetComponentInChildren<Renderer>().material = Assets.CreateMaterial($"mat{AssetName}", 1, Color.white, 1);
            foreach (var psr in modelPrefab.GetComponentsInChildren<ParticleSystemRenderer>())
            {
                psr.material = purpleFireMaterial;
            }
        }

        internal List<ScepterSkill> skills = new List<ScepterSkill>();
        
        public AncientScepterItem()
        {
            skills.Add(new ArtificerFlamethrower2());
            skills.Add(new ArtificerFlyUp2());
            skills.Add(new Bandit2ResetRevolver2());
            skills.Add(new Bandit2SkullRevolver2());
            skills.Add(new CaptainAirstrike2());
            skills.Add(new CaptainAirstrikeAlt2());
            skills.Add(new CommandoBarrage2());
            skills.Add(new CommandoGrenade2());
            skills.Add(new CrocoDisease2());
            skills.Add(new EngiTurret2());
            skills.Add(new EngiWalker2());
            skills.Add(new HuntressBallista2());
            skills.Add(new HuntressRain2());
            skills.Add(new LoaderChargeFist2());
            skills.Add(new LoaderChargeZapFist2());
            skills.Add(new MercEvis2());
            skills.Add(new MercEvisProjectile2());
            skills.Add(new ToolbotDash2());
            skills.Add(new TreebotFlower2_2());
            skills.Add(new TreebotFireFruitSeed2());

            skills.Add(new RailgunnerSuper2());
            skills.Add(new RailgunnerCryo2());

            skills.Add(new VoidFiendCrush());

            skills.Add(new SeekerMeditate2());
            skills.Add(new SeekerPalmBlast2());
            skills.Add(new ChefGlaze2());
            skills.Add(new ChefYesChef2());

            skills.Add(new DrifterSalvage2());
            skills.Add(new DrifterTinker2());

            // Monster
            if (enableMonsterSkills)
            {
                skills.Add(new AurelioniteEyeLaser2());
                skills.Add(new VultureWindblade2());
                /*if (mithrixEnableScepter)
                {
                    skills.Add(new BrotherFistSlam2());
                }*/
            }
        }

        public void RegisterSkills()
        {
            foreach (var skill in skills)
            {
                skill.SetupAttributes();
                RegisterScepterSkill(skill.myDef, skill.targetBody, skill.targetSlot, skill.targetVariantIndex);
            }
            RegisterScepterSkill(VoidFiendCrush.myCtxDef,"VoidSurvivorBody",VoidFiendCrush.dirtySkillDef);
            var Nevermore = new HereticNevermore2();
            Nevermore.SetupAttributes();
            RegisterScepterSkill( Nevermore.myDef,Nevermore.targetBody,UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Heretic/HereticDefaultAbility.asset").WaitForCompletion());
            skills.Add(Nevermore);
        }

        public void Install()
        {
            CharacterBody.onBodyInventoryChangedGlobal += On_CBInventoryChangedGlobal;
            On.RoR2.CharacterMaster.GetDeployableSameSlotLimit += On_CMGetDeployableSameSlotLimit;
            On.RoR2.GenericSkill.UnsetSkillOverride += On_GSUnsetSkillOverride;
            RoR2.Run.onRunStartGlobal += On_RunStartGlobal;
            R2API.RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;

            foreach (var skill in skills)
            {
                skill.LoadBehavior();
            }

            foreach (var cm in AliveList())
            {
                if (!cm.hasBody) continue;
                var body = cm.GetBody();
                HandleScepterSkill(body);
            }
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            SkillSlot slot = SkillSlot.None;
            // scepter stacking
            if (sender.inventory != null && sender.inventory.GetItemCountEffective(ItemDef) > 0 && sender.skillLocator != null && TryGetScepterSlot(sender, out slot))
            {
                switch (slot)
                {
                    default:
                        break;
                    case SkillSlot.None:
                        break;
                    case SkillSlot.Primary:
                        args.primarySkill.cooldownReductionMultAdd += (sender.inventory.GetItemCountEffective(ItemDef) - 1f) * 0.3f;
                        break; 
                    case SkillSlot.Secondary:
                        args.secondarySkill.cooldownReductionMultAdd += (sender.inventory.GetItemCountEffective(ItemDef) - 1f) * 0.3f;
                        break;
                    case SkillSlot.Utility:
                        args.utilitySkill.cooldownReductionMultAdd += (sender.inventory.GetItemCountEffective(ItemDef) - 1f) * 0.3f;
                        break;
                    case SkillSlot.Special:
                        args.specialSkill.cooldownReductionMultAdd += (sender.inventory.GetItemCountEffective(ItemDef) - 1f) * 0.3f;
                        break;
                }
            }

            // chef super weakness debuff
            if (sender.HasBuff(AncientScepterMain.chefSuperWeakDebuff))
            {
                args.armorAdd -= 30f * sender.GetBuffCount(AncientScepterMain.chefSuperWeakDebuff);
                args.damageTotalMult *= 0.6f;
                args.moveSpeedTotalMult *= 0.6f;
            }
        }

        private bool TryGetScepterSlot(CharacterBody body, out SkillSlot result)
        {
            result = SkillSlot.None;

            if (body.skillLocator == null || body.master == null || body.master.loadout == null)
                return false;

            string bodyName = BodyCatalog.GetBodyName(body.bodyIndex);

            var repls = scepterReplacers.Where(x => x.bodyName == bodyName).ToList();
            if (repls.Count == 0)
                return false;

            foreach (var replacement in repls)
            {
                if (!replacement.trgtDef)
                    continue;

                var skill = body.skillLocator.allSkills.FirstOrDefault(s => s && (s.skillDef == replacement.trgtDef || s.baseSkill == replacement.trgtDef));

                if (!skill)
                    continue;

                var slot = replacement.slotIndex != SkillSlot.None ? replacement.slotIndex : body.skillLocator.FindSkillSlot(skill);

                result = slot;
                return true;
            }

            return false;
        }

        public void InstallLanguage()
        {
            foreach (var skill in skills)
            {
                if (skill.oldDescToken == null)
                {
                    continue;
                }
                languageOverlays.Add(LanguageAPI.AddOverlay(skill.newDescToken, Language.GetString(skill.oldDescToken) + skill.overrideStr));
            }
        }

        private bool handlingOverride = false;

        private void On_GSUnsetSkillOverride(On.RoR2.GenericSkill.orig_UnsetSkillOverride orig, GenericSkill self,object source , SkillDef skillDef,GenericSkill.SkillOverridePriority priority)
        {
         /*   bool skillDefIsNotHeresy()
            {
                return (skillDef != CharacterBody.CommonAssets.lunarPrimaryReplacementSkillDef
                    || skillDef != CharacterBody.CommonAssets.lunarSecondaryReplacementSkillDef
                    || skillDef != CharacterBody.CommonAssets.lunarUtilityReplacementSkillDef
                    || skillDef != CharacterBody.CommonAssets.lunarSpecialReplacementSkillDef);
            }
            var body = self.characterBody;

            if ((stridesInteractionMode != StridesInteractionMode.ScepterTakesPrecedence
                && !skillDefIsNotHeresy())
                || body?.inventory.GetItemCount(ItemDef) < 1
                || handlingOverride)
                orig(self, skillDef);
            else
            {
                handlingOverride = true;
                HandleScepterSkill(body);
                handlingOverride = false;
            }*/
            orig(self,source,skillDef,priority);
            if(!handlingOverride && self.characterBody){
                handlingOverride = true;
                CleanScepter(self.characterBody,self);
                HandleScepterSkill(self.characterBody);
                handlingOverride = false;
            }
        }

        private int On_CMGetDeployableSameSlotLimit(On.RoR2.CharacterMaster.orig_GetDeployableSameSlotLimit orig, CharacterMaster self, DeployableSlot slot)
        {
            var retv = orig(self, slot);
            if (slot != DeployableSlot.EngiTurret) return retv;
            var sp = self.GetBody()?.skillLocator?.special;
            if (!sp) return retv;
            if (sp.skillDef == skills.First(x => x is EngiTurret2).myDef)
                return retv + 1;
            if (sp.skillDef == skills.First(x => x is EngiWalker2).myDef)
                return retv + 2;
            return retv;
        }

        private class ScepterReplacer
        {
            public string bodyName;
            public SkillSlot slotIndex;
            public int variantIndex;
            public SkillDef replDef;
            public SkillDef trgtDef;
            public int registryEpoch;
        }

        private readonly List<ScepterReplacer> scepterReplacers = new List<ScepterReplacer>();
        private readonly Dictionary<SkillSlot,SkillDef> heresyDefs = new Dictionary<SkillSlot,SkillDef>();
        public bool RegisterScepterSkill(SkillDef replacingDef, string targetBodyName, SkillSlot targetSlot, int targetVariant)
        {
            if (targetVariant < 0)
            {
                AncientScepterMain._logger.LogError("Can't register a scepter skill to negative variant index");
                return false;
            }
            ScepterReplacer firstMatch = scepterReplacers.FirstOrDefault(x => x.bodyName == targetBodyName && (x.slotIndex == targetSlot && x.variantIndex == targetVariant));
            if (firstMatch != null)
            {
                AncientScepterMain._logger.LogMessage($"Replacing scepter skill for \"{targetBodyName}\" ({firstMatch.replDef.skillName}) with ({replacingDef.skillName})");
                scepterReplacers.Remove(firstMatch);
                firstMatch = null;
            }
            firstMatch = scepterReplacers.FirstOrDefault( x => x.bodyName == targetBodyName && (x.trgtDef != null));
            AncientScepterMain._logger.LogMessage($"Adding scepter skill for \"{targetBodyName}\" ({replacingDef.skillName})");
            scepterReplacers.Add(new ScepterReplacer { bodyName = targetBodyName, slotIndex = targetSlot, variantIndex = targetVariant, replDef = replacingDef,registryEpoch = (firstMatch != null) ? firstMatch.registryEpoch + 1 : 0 });
            return true;
        }

        public bool RegisterScepterSkill(SkillDef replacingDef, string targetBodyName, SkillDef targetDef)
        {
            ScepterReplacer firstMatch = scepterReplacers.FirstOrDefault(x => x.bodyName == targetBodyName && (x.trgtDef == targetDef));
            if (firstMatch != null)
            {
                AncientScepterMain._logger.LogMessage($"Replacing scepter skill for \"{targetBodyName}\" ({firstMatch.replDef.skillName}) with ({replacingDef.skillName})");
                scepterReplacers.Remove(firstMatch);
                firstMatch = null;
            }
            firstMatch = scepterReplacers.FirstOrDefault( x => x.bodyName == targetBodyName && (x.trgtDef == null));
            AncientScepterMain._logger.LogMessage($"Adding scepter skill for \"{targetBodyName}\" ({replacingDef.skillName})");
            scepterReplacers.Add(new ScepterReplacer { bodyName = targetBodyName, slotIndex = SkillSlot.None, variantIndex = -1, replDef = replacingDef,trgtDef = targetDef,registryEpoch = (firstMatch != null) ? firstMatch.registryEpoch + 1 : 0 });
            return true;
        }

        private void On_RunStartGlobal(Run run){
            heresyDefs.Add(SkillSlot.Primary,CharacterBody.CommonAssets.lunarPrimaryReplacementSkillDef);
            heresyDefs.Add(SkillSlot.Secondary,CharacterBody.CommonAssets.lunarSecondaryReplacementSkillDef);
            heresyDefs.Add(SkillSlot.Utility,CharacterBody.CommonAssets.lunarUtilityReplacementSkillDef);
            heresyDefs.Add(SkillSlot.Special,CharacterBody.CommonAssets.lunarSpecialReplacementSkillDef);
            foreach(ScepterReplacer repdef in scepterReplacers.Where(x => x.trgtDef == null).Reverse()){
                var prefab = BodyCatalog.FindBodyPrefab(repdef.bodyName);
                var locator = prefab?.GetComponent<SkillLocator>();
                if(locator && repdef.slotIndex != SkillSlot.None && repdef.variantIndex >= 0){
                  var variants = locator.GetSkill(repdef.slotIndex).skillFamily.variants;
                  if(variants.Length <= repdef.variantIndex){
                    AncientScepterMain._logger.LogError($"Invalid Scepter Replacement for body:{repdef.bodyName},slot:{repdef.slotIndex},with skill:{repdef.replDef.skillNameToken}");
                    repdef.trgtDef = null;
                    continue;
                  }
                  ScepterReplacer firstMatch = scepterReplacers.FirstOrDefault( x => x.trgtDef == variants[repdef.variantIndex].skillDef && x.registryEpoch >= repdef.registryEpoch);
                  if(firstMatch != null){
                    AncientScepterMain._logger.LogMessage($"Replacing scepter skill for \"{repdef.bodyName}\" ({repdef.replDef.skillName}) with ({firstMatch.replDef.skillName})");
                    repdef.trgtDef = null;
                    continue; 
                  }
                  repdef.trgtDef = variants[repdef.variantIndex].skillDef;
                }
            }
            Run.onRunStartGlobal -= On_RunStartGlobal;
        }

        private bool handlingInventory = false;

        private void On_CBInventoryChangedGlobal(CharacterBody body){
          if(!handlingInventory){
            handlingInventory = true;
            if (!HandleScepterSkill(body) && RerollUnused())
            {
                if (GetCountPermanent(body) > 0)
                {
                    Reroll(body, GetCountPermanent(body));
                }

                else if (GetCountTemporary(body) > 0)
                {
                    Reroll(body, GetCountTemporary(body), true);
                }
            }

            else if (GetCountPermanent(body) > 1 && rerollMode != RerollMode.Disabled)
            {
                Reroll(body, GetCountPermanent(body) - 1);
            }

            // should still refresh duration? thats fine by me
            else if (GetCountEffective(body) > 1 && GetCountTemporary(body) > 0 && rerollMode != RerollMode.Disabled)
            {
                Reroll(body, GetCountTemporary(body) - 1, true);
            } 
            handlingInventory = false;
          }
        }

        private bool RerollUnused()
        {
            switch (unusedMode)
            {
                default:
                case UnusedMode.Reroll:
                    return true;
                case UnusedMode.Keep:
                    return false;
                case UnusedMode.Metamorphosis:
                    return !RunArtifactManager.instance.IsArtifactEnabled(RoR2Content.Artifacts.randomSurvivorOnRespawnArtifactDef);
            }
        }

        private void Reroll(CharacterBody self, int count)
        {
            Reroll(self, count, false);
        }

        private void Reroll(CharacterBody self, int count, bool isTemp)
        {
            if (count <= 0) return;
            switch (rerollMode)
            {
                case RerollMode.Disabled:
                    break;
                case RerollMode.Random:
                    var list = Run.instance.availableTier3DropList.Except(new[] { PickupCatalog.FindPickupIndex(ItemDef.itemIndex) }).ToList(); //todo optimize
                    var notifylist = new List<ItemIndex>();
                    for (var i = 0; i < count; i++)
                    {
                        var newItem = PickupCatalog.GetPickupDef(list[UnityEngine.Random.Range(0, list.Count)]).itemIndex;
                        if (isTemp)
                        {
                            self.inventory.RemoveItemTemp(ItemDef.itemIndex, 1);
                            self.inventory.GiveItemTemp(newItem);
                        }
                        else
                        {
                            self.inventory.RemoveItemPermanent(ItemDef, 1);
                            self.inventory.GiveItemPermanent(newItem);
                        }
                        notifylist.Add(newItem);
                    }
                    if (enableSOTVTransforms){
                        foreach(var newItem in notifylist.Distinct()){
                            CharacterMasterNotificationQueue.SendTransformNotification(self.master, ItemDef.itemIndex, newItem, CharacterMasterNotificationQueue.TransformationType.Default);
                        }
                    }
                    break;
                case RerollMode.Scrap:
                    for (var i = 0; i < count; i++)
                    {
                        if (isTemp)
                        {
                            self.inventory.RemoveItemTemp(ItemDef.itemIndex, 1);
                            self.inventory.GiveItemTemp(RoR2Content.Items.ScrapRed.itemIndex); // >>>>>>>>>>>>>>>>>>>>>>>>>>>>
                        }
                        else
                        {
                            self.inventory.RemoveItemPermanent(ItemDef, 1);
                            self.inventory.GiveItemPermanent(RoR2Content.Items.ScrapRed);
                        }
                    }
                    if (enableSOTVTransforms)
                        CharacterMasterNotificationQueue.SendTransformNotification(self.master, ItemDef.itemIndex, RoR2Content.Items.ScrapRed.itemIndex, CharacterMasterNotificationQueue.TransformationType.Default);
                    break;
            }
        }

        private void CleanScepter(CharacterBody self,GenericSkill slot = null,bool force = false){
           var bodyName = BodyCatalog.GetBodyName(self.bodyIndex);
           var repl = scepterReplacers.FindAll(x => x.bodyName == bodyName);
           if(slot){
               if(repl.Any(r => r.replDef == slot.skillDef && (force || (slot.baseSkill != r.trgtDef && !slot.skillOverrides.Any(s => r.trgtDef == s.skillDef))))){
                 slot.UnsetSkillOverride(self,slot.skillDef,GenericSkill.SkillOverridePriority.Upgrade);
               }
           }
           else{
               foreach(var skill in self.skillLocator.allSkills){
               if(repl.Any(r => r.replDef == slot.skillDef && (force || (slot.baseSkill != r.trgtDef && !slot.skillOverrides.Any(s => r.trgtDef == s.skillDef))))){
                    skill.UnsetSkillOverride(self,skill.skillDef,GenericSkill.SkillOverridePriority.Upgrade);
                  }
               }
           }
        }

        private bool HandleScepterSkill(CharacterBody self, bool forceOff = false)
        {
            bool hasHeresyForSlot(SkillSlot skillSlot)
            {
                switch (skillSlot)
                {
                    case SkillSlot.Primary:
                        return self.inventory.GetItemCountEffective(RoR2Content.Items.LunarPrimaryReplacement) > 0;
                    case SkillSlot.Secondary:
                        return self.inventory.GetItemCountEffective(RoR2Content.Items.LunarSecondaryReplacement) > 0;
                    case SkillSlot.Utility:
                        return self.inventory.GetItemCountEffective(RoR2Content.Items.LunarUtilityReplacement) > 0;
                    case SkillSlot.Special:
                        return self.inventory.GetItemCountEffective(RoR2Content.Items.LunarSpecialReplacement) > 0;
                }
                return false;
            }
            if (self.skillLocator && self.master?.loadout != null)
            {
                var bodyName = BodyCatalog.GetBodyName(self.bodyIndex);

                var repl = scepterReplacers.FindAll(x => x.bodyName == bodyName);
                if (repl.Count > 0)
                {
                    if(repl.Select(x => x.replDef).Intersect(self.skillLocator.allSkills.Select(x => x.skillDef)).Any() && GetCountEffective(self) > 0){return true;}
                    GenericSkill targetSkill = null;
                    SkillSlot targetSlot = SkillSlot.None;
                    ScepterReplacer replVar = null;
                    bool heresyExists = false;
                    foreach(var replacement in repl){
                      foreach(var skill in self.skillLocator.allSkills.Reverse().ToList().FindAll((s) => s.skillDef == replacement.trgtDef || s.baseSkill == replacement.trgtDef)){
                          targetSkill = skill;
                          targetSlot = (replacement.slotIndex == SkillSlot.None)? self.skillLocator.FindSkillSlot(targetSkill) : replacement.slotIndex;
                          if((stridesInteractionMode != StridesInteractionMode.ScepterTakesPrecedence || bodyName == "HereticBody" ) && hasHeresyForSlot(targetSlot) && replacement.trgtDef != heresyDefs[targetSlot]){
                            heresyExists = true;
                            continue;
                          }
                          replVar = replacement;
                          break;
                      }
                      if(replVar != null && targetSkill && replVar.trgtDef == targetSkill.skillDef){
                        break;
                      }
                    }
                    if (replVar == null){ return heresyExists ? stridesInteractionMode != StridesInteractionMode.ScepterRerolls : false;}
                    var outerOverride = handlingOverride;
                    handlingOverride = true;
                    if (!forceOff && GetCountEffective(self) > 0)
                    {
                        if (stridesInteractionMode == StridesInteractionMode.ScepterTakesPrecedence && hasHeresyForSlot(targetSlot))
                        {
                            self.skillLocator.GetSkill(targetSlot).UnsetSkillOverride(self, heresyDefs[targetSlot], GenericSkill.SkillOverridePriority.Replacement);
                        }
                        targetSkill.SetSkillOverride(self, replVar.replDef, GenericSkill.SkillOverridePriority.Upgrade);
                        targetSkill.onSkillChanged += UnsetOverrideLater;
                    }
                    else
                    {
                        targetSkill.UnsetSkillOverride(self, replVar.replDef, GenericSkill.SkillOverridePriority.Upgrade);
                        if (stridesInteractionMode == StridesInteractionMode.ScepterTakesPrecedence && hasHeresyForSlot(targetSlot))
                        {
                            self.skillLocator.GetSkill(targetSlot).SetSkillOverride(self, heresyDefs[targetSlot], GenericSkill.SkillOverridePriority.Replacement);
                        }
                    }
                    handlingOverride = outerOverride;
                    return true;
                    void UnsetOverrideLater(GenericSkill skill){
                       skill.onSkillChanged -= UnsetOverrideLater;
                       skill.UnsetSkillOverride(self, replVar.replDef,GenericSkill.SkillOverridePriority.Upgrade);
                    }
                }
            }
            return false;

        }
    }
}
