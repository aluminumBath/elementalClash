using System;
using UnityEngine;
using UnityEngine.Events;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Scene-side driver for character creation. Element buttons roll a loadout; weapon buttons
    /// make a non-channeler and equip a starter weapon (wood) via <see cref="WeaponHolder"/> — more
    /// materials are found on the map. Results go to the player and broadcast on <see cref="OnResult"/>.
    /// </summary>
    public sealed class CharacterCreationController : MonoBehaviour
    {
        [Serializable] public class ResultEvent : UnityEvent<CharacterCreationResult> { }

        [SerializeField] private PlayerCombatController player;
        [SerializeField] private WeaponHolder weaponHolder;
        [Tooltip("0 = unseeded random; set a value for reproducible demos.")]
        [SerializeField] private int rollSeed = 0;

        [Header("Drop rates")]
        [SerializeField] private float confluenceChance = 0.001f;
        [SerializeField] private float subArtChance = 0.05f;
        [SerializeField] private bool confluenceIncludesSubArts = true;

        [Header("Starter weapon material")]
        [SerializeField] private WeaponMaterial starterMaterial = WeaponMaterial.Wood;

        public ResultEvent OnResult = new ResultEvent();
        public CharacterCreationResult? LastResult { get; private set; }

        private IRandomSource _random;

        private void Awake() =>
            _random = rollSeed != 0 ? new SystemRandomSource(rollSeed) : new SystemRandomSource();

        /// <summary>Injects the runtime player + weapon holder. Used by the flow controller when it
        /// builds creation at runtime instead of having it wired in a scene.</summary>
        public void Bind(PlayerCombatController playerTarget, WeaponHolder holder)
        {
            player = playerTarget;
            weaponHolder = holder;
        }

        private GachaConfig Config() => new GachaConfig
        {
            ConfluenceChance = confluenceChance,
            SubArtChance = subArtChance,
            ConfluenceIncludesSubArts = confluenceIncludesSubArts
        };

        // Elements
        public void ChooseFire()  => ChooseElement(Element.Fire);
        public void ChooseWater() => ChooseElement(Element.Water);
        public void ChooseEarth() => ChooseElement(Element.Earth);
        public void ChooseAir()   => ChooseElement(Element.Air);

        // Weapons (start in wood; find metal / ice on the map)
        public void ChooseWeaponHammer()  => ChooseWeapon(WeaponType.Hammer);
        public void ChooseWeaponSword()   => ChooseWeapon(WeaponType.Sword);
        public void ChooseWeaponLongBow() => ChooseWeapon(WeaponType.LongBow);
        public void ChooseWeaponShield()  => ChooseWeapon(WeaponType.Shield);
        public void ChooseWeaponDagger()  => ChooseWeapon(WeaponType.Dagger);
        public void ChooseWeaponSai()     => ChooseWeapon(WeaponType.Sai);

        public void ChooseElement(Element element) =>
            Apply(CharacterCreationService.CreateChanneler(element, _random, Config()));

        public void ChooseWeapon(WeaponType weapon)
        {
            if (weaponHolder != null) weaponHolder.Equip(new WeaponInstance(weapon, starterMaterial));
            Apply(CharacterCreationService.CreateWeaponUser(weapon));
        }

        private void Apply(CharacterCreationResult result)
        {
            LastResult = result;
            if (player != null) player.SetLoadout(result.Loadout);
            OnResult.Invoke(result);
            Debug.Log($"[Elementborn] Created: tier={result.Tier}, elements={result.Loadout.Elements.Count}, weapon={result.Loadout.Weapon}");
        }
    }
}
