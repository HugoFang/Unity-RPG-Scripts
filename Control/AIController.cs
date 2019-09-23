﻿using RPG.Attributes;
using RPG.Combat;
using RPG.Core;
using RPG.Movement;
using RPG.Utility;
using UnityEngine;
using static RPG.Utility.Utility;

namespace RPG.Control {

    public class AIController : MonoBehaviour {
        [SerializeField] private float chaseDistance = 5f;
        [SerializeField] private float suspicionTime = 5f;
        [SerializeField] private float aggroCooldown = 5f;
        [SerializeField] private float shoutDistance = 5f;
        [SerializeField] private float dwellTime = 3f;
        [SerializeField] private float wayPointTolerance = 1f;
        [Range(0, 1)][SerializeField] private float patrolSpeedFraction = 0.2f;
        [SerializeField] private PatrolPath patrolPath;

        private int currentWaypointIndex = 0;
        private float timeSinceLastSawPlayer = Mathf.Infinity;
        private float timeSinceLastWaypoint = Mathf.Infinity;
        private float timeSinceAggrevated = Mathf.Infinity;
        private bool hasNotAggrevated = true;
        private GameObject player;
        private Health playerHealth;
        private Health AIHealth;
        private Fighter fighter;
        private Mover mover;
        private ActionScheduler actionScheduler;
        private LazyValue<Vector3> guardPosition;

        public PatrolPath PatrolPath {
            get {
                return patrolPath;
            }
        }

        private void Awake() {
            player = GameObject.FindWithTag("Player");
            playerHealth = player.GetComponent<Health>();
            AIHealth = GetComponent<Health>();
            fighter = GetComponent<Fighter>();
            mover = GetComponent<Mover>();
            actionScheduler = GetComponent<ActionScheduler>();
            guardPosition = new LazyValue<Vector3>(GetGuardPosition);
        }

        private void Start() {
            guardPosition.ForceInit();
        }

        private void Update() {
            if (AIHealth.IsDead) {
                return;
            }
            if (IsAggrevated()) {
                timeSinceLastSawPlayer = 0f;
                AttackBehaviour();
            } else if (timeSinceLastSawPlayer < suspicionTime) {
                SuspicionBehaviour();
            } else if (transform.position != guardPosition.value) {
                hasNotAggrevated = true;
                PatrolBehaviour();
            }

            UpdateTimers();
        }

        private void UpdateTimers() {
            timeSinceLastSawPlayer += Time.deltaTime;
            timeSinceLastWaypoint += Time.deltaTime;
            timeSinceAggrevated += Time.deltaTime;
        }

        public void Aggrevate() {
            timeSinceAggrevated = 0;
        }

        private Vector3 GetGuardPosition() {
            return transform.position;
        }

        private void AttackBehaviour() {
            fighter.Attack(player);
            if (hasNotAggrevated) {
                hasNotAggrevated = false;
                Aggrevate();
                AggrevateNearbyEnemies();
            }
        }

        private void AggrevateNearbyEnemies() {
            RaycastHit[] hits = Physics.SphereCastAll(transform.position, shoutDistance, Vector3.up, 0);
            foreach (RaycastHit hit in hits) {
                AIController ai = hit.collider.GetComponent<AIController>();

                if (ai == null) {
                    continue;
                }
                ai.Aggrevate();

            }
        }

        private void SuspicionBehaviour() {
            actionScheduler.CancelCurrentAction();
        }

        private void PatrolBehaviour() {
            Vector3 nextPosition = guardPosition.value;

            if (patrolPath != null) {
                if (AtWayPoint()) {
                    if (dwellTime < timeSinceLastWaypoint) {
                        timeSinceLastWaypoint = 0f;
                        CycleWaypoint();
                    }
                }

                nextPosition = GetCurrentWaypoint().position;
            }

            mover.StartMovement(nextPosition, patrolSpeedFraction);
        }

        private bool AtWayPoint() {
            return IsTargetInRange(transform, GetCurrentWaypoint(), wayPointTolerance);
        }

        private void CycleWaypoint() {
            currentWaypointIndex = patrolPath.GetNextIndex(currentWaypointIndex);
        }

        private Transform GetCurrentWaypoint() {
            return patrolPath.GetWayPoint(currentWaypointIndex);
        }

        private bool IsAggrevated() {
            return !playerHealth.IsDead && IsTargetInRange(transform, player.transform, chaseDistance) || timeSinceAggrevated < aggroCooldown;
        }

        private void OnDrawGizmosSelected() {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, chaseDistance);
        }
    }
}
