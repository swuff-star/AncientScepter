using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace AncientScepter
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public abstract class ItemBase<T> : ItemBase where T : ItemBase<T>
    {
        public static T instance { get; private set; }

        public ItemBase()
        {
            if (instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting ItemBoilerplate/Item was instantiated twice");
            instance = this as T;
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public abstract class ItemBase
    {
        public abstract string ItemName { get; }
        public abstract string ItemLangTokenName { get; }
        public abstract string ItemPickupDesc { get; }
        public abstract string ItemFullDescription { get; }
        public abstract string ItemLore { get; }

        public abstract ItemTier Tier { get; }
        public virtual ItemTag[] ItemTags { get; set; } = new ItemTag[] { };

        public abstract GameObject ItemModel { get; }
        public abstract Sprite ItemIcon { get; }
        public abstract GameObject ItemDisplay { get; }

        public ItemDef ItemDef { get; set; }

        public virtual bool CanRemove { get; } = true;

        public virtual bool AIBlacklisted { get; set; } = false;

        public virtual bool TILER2_MimicBlacklisted { get; set; } = false;

        public static ItemDef myDef { get; set; }

        /// <summary>
        /// This method structures your code execution of this class. An example implementation inside of it would be:
        /// <para>CreateConfig(config);</para>
        /// <para>CreateLang();</para>
        /// <para>CreateItem();</para>
        /// <para>Hooks();</para>
        /// <para>This ensures that these execute in this order, one after another, and is useful for having things available to be used in later methods.</para>
        /// <para>P.S. CreateItemDisplayRules(); does not have to be called in this, as it already gets called in CreateItem();</para>
        /// </summary>
        /// <param name="config">The config file that will be passed into this from the main class.</param>
        public abstract void Init(ConfigFile config);

        public static GameObject displayPrefab;

        public virtual void CreateConfig(ConfigFile config)
        {
        }

        protected void CreateLang()
        {
            LanguageAPI.Add("ITEM_" + ItemLangTokenName + "_NAME", ItemName);
            LanguageAPI.Add("ITEM_" + ItemLangTokenName + "_PICKUP", ItemPickupDesc);
            LanguageAPI.Add("ITEM_" + ItemLangTokenName + "_DESCRIPTION", ItemFullDescription);
            LanguageAPI.Add("ITEM_" + ItemLangTokenName + "_LORE", ItemLore);
        }

        public virtual ItemDisplayRuleDict CreateDisplayRules()
        {
            return new ItemDisplayRuleDict(new ItemDisplayRule[0]);
        }

        protected void CreateItem()
        {
            if (AIBlacklisted)
            {
                ItemTags = new List<ItemTag>(ItemTags) { ItemTag.AIBlacklist }.ToArray();
            }
            ItemDef = ScriptableObject.CreateInstance<ItemDef>();
            myDef = ItemDef;
            ItemDef.name = "ITEM_" + ItemLangTokenName;
            ItemDef.nameToken = "ITEM_" + ItemLangTokenName + "_NAME";
            ItemDef.pickupToken = "ITEM_" + ItemLangTokenName + "_PICKUP";
            ItemDef.descriptionToken = "ITEM_" + ItemLangTokenName + "_DESCRIPTION";
            ItemDef.loreToken = "ITEM_" + ItemLangTokenName + "_LORE";
            ItemDef.pickupModelPrefab = ItemModel;
            ItemDef.pickupIconSprite = ItemIcon;
            ItemDef.hidden = false;
            ItemDef.tags = ItemTags;
            ItemDef.canRemove = CanRemove;
            //ItemDef.tier = Tier;
            ItemDef.deprecatedTier = Tier;
            if (ItemTags.Length > 0) { ItemDef.tags = ItemTags; }

            
            if (TILER2_MimicBlacklisted)
            {
                if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.ThinkInvisible.TILER2"))
                {
                    TILER2_BlacklistItem(ItemDef);
                }
            }

            ItemAPI.Add(new CustomItem(ItemDef, CreateDisplayRules()));
        }

        protected virtual void CreateCraftableDef()
        { }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void TILER2_BlacklistItem(ItemDef itemDef)
        {
            TILER2.FakeInventory.blacklist.Add(itemDef);
        }

        public virtual void Hooks()
        {
            
        }

        protected virtual void SetupMaterials(GameObject modelPrefab)
        {
            Renderer[] children = modelPrefab.GetComponentsInChildren<Renderer>();

            for (int i = 0; i < children.Length; i++)
            {
                children[i].material.shader = Assets.hotpoo;
            }
        }

        //Based on ThinkInvis' methods
        public int GetCountPermanent(CharacterBody body)
        {
            if (!body || !body.inventory) { return 0; }

            return body.inventory.GetItemCountPermanent(ItemDef);
        }

        public int GetCountPermanent(CharacterMaster master)
        {
            if (!master || !master.inventory) { return 0; }

            return master.inventory.GetItemCountPermanent(ItemDef);
        }

        public int GetCountSpecificPermanent(CharacterBody body, ItemDef itemDef)
        {
            if (!body || !body.inventory) { return 0; }

            return body.inventory.GetItemCountPermanent(itemDef);
        }

        public int GetCountTemporary(CharacterBody body)
        {
            if (!body || !body.inventory) { return 0; }

            return body.inventory.GetItemCountTemp(ItemDef);
        }

        public int GetCountTemporary(CharacterMaster master)
        {
            if (!master || !master.inventory) { return 0; }

            return master.inventory.GetItemCountTemp(ItemDef);
        }

        public int GetCountSpecificTemporary(CharacterBody body, ItemDef itemDef)
        {
            if (!body || !body.inventory) { return 0; }

            return body.inventory.GetItemCountTemp(itemDef);
        }

        public int GetCountEffective(CharacterBody body)
        {
            if (!body || !body.inventory) { return 0; }

            return body.inventory.GetItemCountEffective(ItemDef);
        }

        public int GetCountEffective(CharacterMaster master)
        {
            if (!master || !master.inventory) { return 0; }

            return master.inventory.GetItemCountEffective(ItemDef);
        }

        public int GetCountSpecificEffective(CharacterBody body, ItemDef itemDef)
        {
            if (!body || !body.inventory) { return 0; }

            return body.inventory.GetItemCountEffective(itemDef);
        }

        ///<summary>A server-only rng instance based on the current run's seed.</summary>
        public Xoroshiro128Plus rng { get; internal set; }

        protected readonly List<LanguageAPI.LanguageOverlay> languageOverlays = new List<LanguageAPI.LanguageOverlay>();
        protected readonly Dictionary<string, string> genericLanguageTokens = new Dictionary<string, string>();
        protected readonly Dictionary<string, Dictionary<string, string>> specificLanguageTokens = new Dictionary<string, Dictionary<string, string>>();
        public bool languageInstalled { get; private set; } = false;
    }
}