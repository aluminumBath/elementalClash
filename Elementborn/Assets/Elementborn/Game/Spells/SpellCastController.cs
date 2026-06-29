using UnityEngine;
using UnityEngine.Events;

namespace Elementborn.Game
{
    public sealed class SpellCastController : MonoBehaviour
    {
        [SerializeField] private SpellResourcePool focusPool;
        [SerializeField] private StaminaResource stamina;
        [SerializeField] private SimpleCombatHealth health;
        [SerializeField] private Transform castPoint;
        [SerializeField] private LayerMask groundMask = ~0;
        [SerializeField] private bool allowQueuedCast = true;
        [SerializeField] private UnityEvent onCastStarted;
        [SerializeField] private UnityEvent onCastCompleted;
        [SerializeField] private UnityEvent onCastInterrupted;

        private SpellCastState state = SpellCastState.Idle;
        private SpellCastRequest currentRequest;
        private SpellCastRequest queuedRequest;
        private bool hasQueuedRequest;
        private float castStartedAt;
        private float castEndsAt;

        public SpellCastState State => state;
        public SpellCastDefinition CurrentSpell => currentRequest.Spell;
        public float CastProgress01
        {
            get
            {
                if (state != SpellCastState.Casting || CurrentSpell == null)
                {
                    return 0f;
                }

                float duration = Mathf.Max(0.001f, castEndsAt - castStartedAt);
                return Mathf.Clamp01((Time.unscaledTime - castStartedAt) / duration);
            }
        }

        private void Awake()
        {
            if (focusPool == null) focusPool = GetComponent<SpellResourcePool>();
            if (stamina == null) stamina = GetComponent<StaminaResource>();
            if (health == null) health = GetComponent<SimpleCombatHealth>();
            if (castPoint == null) castPoint = transform;
        }

        private void Update()
        {
            if (state == SpellCastState.Casting && Time.unscaledTime >= castEndsAt)
            {
                CompleteCast();
            }
        }

        public bool BeginCast(SpellCastDefinition spell)
        {
            return BeginCast(spell, null, GetDefaultGroundPoint(spell));
        }

        public bool BeginCast(SpellCastDefinition spell, GameObject target, Vector3 groundPoint)
        {
            if (spell == null)
            {
                NotificationFeed.Post("No spell selected.", NotificationType.Warning);
                return false;
            }

            if (state == SpellCastState.Casting)
            {
                if (allowQueuedCast && spell.Queueable)
                {
                    queuedRequest = new SpellCastRequest(spell, target, groundPoint);
                    hasQueuedRequest = true;
                    state = SpellCastState.Queued;
                    NotificationFeed.Post($"Queued {spell.DisplayName}.", NotificationType.Info);
                    state = SpellCastState.Casting;
                    return true;
                }

                NotificationFeed.Post("Already casting.", NotificationType.Warning);
                return false;
            }

            if (!CanCast(spell, out string reason))
            {
                NotificationFeed.Post(reason, NotificationType.Warning);
                return false;
            }

            currentRequest = new SpellCastRequest(spell, target, groundPoint);
            SpendResource(spell);

            castStartedAt = Time.unscaledTime;
            castEndsAt = Time.unscaledTime + spell.CastTimeSeconds;
            state = SpellCastState.Casting;
            onCastStarted?.Invoke();

            if (spell.CastTimeSeconds <= 0f)
            {
                CompleteCast();
            }
            else
            {
                NotificationFeed.Post($"Casting {spell.DisplayName}...", NotificationType.Combat);
            }

            return true;
        }

        public void Interrupt()
        {
            if (state != SpellCastState.Casting || CurrentSpell == null || !CurrentSpell.Interruptible)
            {
                return;
            }

            state = SpellCastState.Interrupted;
            onCastInterrupted?.Invoke();
            NotificationFeed.Post($"{CurrentSpell.DisplayName} interrupted.", NotificationType.Warning);
            currentRequest = default;
            TryStartQueued();
        }

        public bool CanCast(SpellCastDefinition spell, out string reason)
        {
            reason = "";

            if (spell == null)
            {
                reason = "No spell selected.";
                return false;
            }

            if (spell.Ability != null && !PlayerAbilityTracker.HasUnlocked(spell.Ability.AbilityId))
            {
                reason = $"Requires ability: {spell.Ability.DisplayName}.";
                return false;
            }

            if (!SpellCooldownTracker.IsReady(spell.SpellId))
            {
                reason = $"{spell.DisplayName} cooldown: {SpellCooldownTracker.Remaining(spell.SpellId):0.0}s.";
                return false;
            }

            if (!HasResource(spell))
            {
                reason = $"Not enough {spell.ResourceType}.";
                return false;
            }

            return true;
        }

        public Vector3 GetDefaultGroundPoint(SpellCastDefinition spell)
        {
            float range = spell != null ? spell.Range : 12f;
            Vector3 origin = castPoint != null ? castPoint.position : transform.position;
            Vector3 forward = castPoint != null ? castPoint.forward : transform.forward;
            Vector3 point = origin + forward * range;

            if (Physics.Raycast(origin, forward, out RaycastHit hit, range, groundMask, QueryTriggerInteraction.Ignore))
            {
                point = hit.point;
            }

            return point;
        }

        private void CompleteCast()
        {
            SpellCastDefinition spell = CurrentSpell;
            if (spell == null)
            {
                state = SpellCastState.Idle;
                return;
            }

            ExecuteSpell(currentRequest);
            SpellCooldownTracker.StartCooldown(spell.SpellId, GetModifiedCooldown(spell));
            GrantFirstCastReward(spell);

            state = SpellCastState.Complete;
            onCastCompleted?.Invoke();
            NotificationFeed.Post($"Cast {spell.DisplayName}.", NotificationType.Combat);

            currentRequest = default;
            state = SpellCastState.Idle;
            TryStartQueued();
        }

        private void ExecuteSpell(SpellCastRequest request)
        {
            SpellCastDefinition spell = request.Spell;
            if (spell == null)
            {
                return;
            }

            if (spell.SelfStatus != null)
            {
                StatusEffectController status = GetComponent<StatusEffectController>();
                if (status == null) status = gameObject.AddComponent<StatusEffectController>();
                status.Apply(spell.SelfStatus);
            }

            switch (spell.TargetingMode)
            {
                case SpellTargetingMode.Self:
                    ExecuteSelf(spell);
                    break;
                case SpellTargetingMode.TargetedUnit:
                    ExecuteTargeted(spell, request.Target);
                    break;
                case SpellTargetingMode.GroundAoe:
                    ExecuteAoe(spell, request.GroundPoint);
                    break;
                case SpellTargetingMode.BoatAssist:
                    ExecuteBoatAssist(spell);
                    break;
                case SpellTargetingMode.Cone:
                    ExecuteCone(spell);
                    break;
                default:
                    ExecuteProjectile(spell);
                    break;
            }
        }

        private void ExecuteSelf(SpellCastDefinition spell)
        {
            if (spell.Attack != null)
            {
                var context = BuildContext(spell);
                CombatDamageUtility.ApplyHit(gameObject, context);
            }
        }

        private void ExecuteTargeted(SpellCastDefinition spell, GameObject target)
        {
            if (target == null)
            {
                ExecuteProjectile(spell);
                return;
            }

            var context = BuildContext(spell);
            CombatDamageUtility.ApplyHit(target, context);
        }

        private void ExecuteProjectile(SpellCastDefinition spell)
        {
            Transform origin = castPoint != null ? castPoint : transform;

            if (spell.ProjectilePrefab != null)
            {
                ProjectileCombatEmitter projectile = Instantiate(spell.ProjectilePrefab, origin.position, origin.rotation);
                projectile.SetOwner(gameObject);
                return;
            }

            if (spell.Attack == null)
            {
                return;
            }

            if (Physics.Raycast(origin.position, origin.forward, out RaycastHit hit, spell.Range, spell.TargetMask, QueryTriggerInteraction.Ignore))
            {
                GameObject target = hit.rigidbody != null ? hit.rigidbody.gameObject : hit.collider.gameObject;
                CombatDamageUtility.ApplyHit(target, BuildContext(spell));
            }
        }

        private void ExecuteAoe(SpellCastDefinition spell, Vector3 point)
        {
            if (spell.AoeVisualPrefab != null)
            {
                Instantiate(spell.AoeVisualPrefab, point, Quaternion.identity);
            }

            Collider[] hits = Physics.OverlapSphere(point, spell.Radius, spell.TargetMask, QueryTriggerInteraction.Ignore);
            foreach (var hit in hits)
            {
                if (hit == null)
                {
                    continue;
                }

                GameObject target = hit.attachedRigidbody != null ? hit.attachedRigidbody.gameObject : hit.gameObject;
                if (target.transform.root == transform.root)
                {
                    continue;
                }

                CombatDamageUtility.ApplyHit(target, BuildContext(spell));
            }
        }

        private void ExecuteCone(SpellCastDefinition spell)
        {
            Transform origin = castPoint != null ? castPoint : transform;
            Collider[] hits = Physics.OverlapSphere(origin.position, spell.Range, spell.TargetMask, QueryTriggerInteraction.Ignore);
            foreach (var hit in hits)
            {
                GameObject target = hit.attachedRigidbody != null ? hit.attachedRigidbody.gameObject : hit.gameObject;
                Vector3 direction = (target.transform.position - origin.position).normalized;
                if (Vector3.Dot(origin.forward, direction) < 0.45f)
                {
                    continue;
                }

                CombatDamageUtility.ApplyHit(target, BuildContext(spell));
            }
        }

        private void ExecuteBoatAssist(SpellCastDefinition spell)
        {
            BoatCombatHook boat = GetComponentInParent<BoatCombatHook>();
            if (boat == null)
            {
                NotificationFeed.Post("Boat spell cast without boat hook; applying self effect only.", NotificationType.Info);
                ExecuteSelf(spell);
                return;
            }

            NotificationFeed.Post($"{spell.DisplayName} empowered the boat.", NotificationType.Combat);
        }

        private CombatHitContext BuildContext(SpellCastDefinition spell)
        {
            CombatAttackDefinition attack = spell.Attack;
            return new CombatHitContext
            {
                Source = gameObject,
                AttackDefinition = attack,
                BaseDamage = attack != null ? attack.BaseDamage : 0f,
                Element = attack != null ? attack.Element : spell.Ability != null ? spell.Ability.Element : AbilityElementType.Neutral,
                CritChance = attack != null ? attack.CritChance : 0.03f,
                CritMultiplier = attack != null ? attack.CritMultiplier : 1.5f,
                KnockbackForce = attack != null ? attack.KnockbackForce : 0f,
                UseEquipmentBonuses = attack == null || attack.UseEquipmentBonuses,
                OriginType = spell.TargetingMode == SpellTargetingMode.BoatAssist ? AttackOriginType.Boat : AttackOriginType.OnFoot,
                StatusToApply = attack != null ? attack.StatusToApply : null,
                AttackName = spell.DisplayName
            };
        }

        private bool HasResource(SpellCastDefinition spell)
        {
            if (spell.ResourceType == SpellResourceType.None || spell.ResourceCost <= 0f)
            {
                return true;
            }

            if (spell.ResourceType == SpellResourceType.Stamina)
            {
                return stamina != null && stamina.CanSpend(spell.ResourceCost);
            }

            if (spell.ResourceType == SpellResourceType.Health)
            {
                return health != null && health.CurrentHealth > spell.ResourceCost;
            }

            return focusPool != null && focusPool.CanSpend(spell.ResourceCost);
        }

        private void SpendResource(SpellCastDefinition spell)
        {
            if (spell.ResourceType == SpellResourceType.None || spell.ResourceCost <= 0f)
            {
                return;
            }

            if (spell.ResourceType == SpellResourceType.Stamina && stamina != null)
            {
                stamina.TrySpend(spell.ResourceCost);
                return;
            }

            if (spell.ResourceType == SpellResourceType.Health && health != null)
            {
                health.ApplyDamage(spell.ResourceCost);
                return;
            }

            if (focusPool != null)
            {
                focusPool.TrySpend(spell.ResourceCost);
            }
        }

        private float GetModifiedCooldown(SpellCastDefinition spell)
        {
            float cooldown = spell.CooldownSeconds;
            float reduction = PlayerEquipmentTracker.GetPercentBonus(GearStatType.CooldownReduction);
            cooldown *= 1f - Mathf.Clamp01(reduction / 100f);
            return Mathf.Max(0f, cooldown);
        }

        private void GrantFirstCastReward(SpellCastDefinition spell)
        {
            if (spell.SkillPointRewardOnFirstCast <= 0 || SpellCooldownTracker.FirstCastRewardGranted(spell.SpellId))
            {
                return;
            }

            SpellCooldownTracker.MarkFirstCastRewardGranted(spell.SpellId);
            PlayerAbilityTracker.AddSkillPoints(spell.SkillPointRewardOnFirstCast, $"First cast: {spell.DisplayName}");
        }

        private void TryStartQueued()
        {
            if (!hasQueuedRequest)
            {
                return;
            }

            SpellCastRequest next = queuedRequest;
            hasQueuedRequest = false;
            queuedRequest = default;

            if (next.Spell != null)
            {
                BeginCast(next.Spell, next.Target, next.GroundPoint);
            }
        }
    }
}
