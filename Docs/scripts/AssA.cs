using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IArena.AssA
{
    // Control
    public class ControlAssa : ControlBase {
        // Variables :
        public struct Order {
            public string orderName;
            public Vector3 orderPosition;
        }

        // Méthodes :
        protected override void OnHit(Vector3 direction) {
            List<TargetInformations> targets = GetTargets();

            if (targets.Count > 0) {
                foreach (TargetInformations target in targets) {
                    if (target.team != Team) {
                        Order order = new Order();
                        order.orderName = "Help - Control";
                        order.orderPosition = target.position;

                        // Envoyer l'ordre :
                        foreach(InterceptorAssa i in GetAgentsOfType<InterceptorAssa>()) {
                            SendInformationTo(i, JsonUtility.ToJson(order));
                        }

                        foreach(DestructorAssa i in GetAgentsOfType<DestructorAssa>()) {
                            SendInformationTo(i, JsonUtility.ToJson(order));
                        }
                    }
                }
            } else {
                Order order = new Order();
                order.orderName = "Help - Control";
                order.orderPosition = transform.position;

                // Envoyer l'ordre :
                foreach(InterceptorAssa i in GetAgentsOfType<InterceptorAssa>()) {
                    SendInformationTo(i, JsonUtility.ToJson(order));
                }

                foreach(DestructorAssa i in GetAgentsOfType<DestructorAssa>()) {
                    SendInformationTo(i, JsonUtility.ToJson(order));
                }
            }
        }

        protected override void OnInformationsReceived(string data) {
            TargetInformations informations = JsonUtility.FromJson<TargetInformations>(data);
            Order order = new Order();

            // Vérification du type d'ennemi :
            switch (informations.kind) {
                case EntityKind.Control:
                    order.orderName = "Objectif";
                    order.orderPosition = informations.position;

                    // Envoyer l'ordre :
                    foreach(InterceptorAssa i in GetAgentsOfType<InterceptorAssa>()) {
                        SendInformationTo(i, JsonUtility.ToJson(order));
                    }

                    foreach(DestructorAssa i in GetAgentsOfType<DestructorAssa>()) {
                        SendInformationTo(i, JsonUtility.ToJson(order));
                    }
                    break;
                case EntityKind.Interceptor:
                    order.orderName = "Help - Interceptor";
                    order.orderPosition = informations.position;

                    // Envoyer l'ordre :
                    foreach(InterceptorAssa i in GetAgentsOfType<InterceptorAssa>()) {
                        SendInformationTo(i, JsonUtility.ToJson(order));
                    }

                    foreach(DestructorAssa i in GetAgentsOfType<DestructorAssa>()) {
                        SendInformationTo(i, JsonUtility.ToJson(order));
                    }
                    break;
                case EntityKind.Destructor:
                    order.orderName = "Help - Destructor";
                    order.orderPosition = informations.position;

                    // Envoyer l'ordre :
                    foreach(DestructorAssa i in GetAgentsOfType<DestructorAssa>()) {
                        SendInformationTo(i, JsonUtility.ToJson(order));
                    }
                    break;
            }
        }
    }

    // Destructor

    public class DestructorAssa : DestructorBase {
        // Variables :
        private Vector3 targetPosition;
        private Vector3 middlePosition;
        private bool middleReached = false;
        private bool continuousShooting = !ArenaManager.Instance.Hardcore;

        private Vector3 controlPosition;
        private Vector3 controlEnnemyPosition;
        private Vector3 objectivePosition;
        private bool controlAttacked = false;
        private bool controlEnnemyLocated;

        private float timeResearch = 5.0f;
        private float timeOnHit = 10.0f;
        private bool attacking = false;

        private float totalLife;
        private bool leakAuthorized = true;

        public class Order {
            public string orderName;
            public Vector3 orderPosition;
        }

        // États :
        public static class DestructorAssaState {
            public const string PATROL = "Patrol";
            public const string OBJECTIVE = "Objective";
            public const string HELP = "Help";
            public const string ATTACK = "Attack";
            public const string LEAK = "Leak";
        }

        // Machine à état :
        protected override void InitializeAgent() {
            State = DestructorAssaState.PATROL;
            controlPosition = Control.transform.position;
            totalLife = Life;
        }

        protected override void OnEnterState(string state) {
            switch (State) {
                case DestructorAssaState.PATROL:
                    Log("Enter to state : Patrol");
                    // Go to the middle of the arena :
                    // GoTowards(transform.position, -1.0f);

                    // Go to the opposite side of the arena :
                    GoTo(-transform.position);
                    break;
                case DestructorAssaState.OBJECTIVE:
                    Log("Enter to state : Objective");
                    break;
                case DestructorAssaState.HELP:
                    Log("Enter to state : Help");
                    break;
                case DestructorAssaState.ATTACK:
                    Log("Enter to state : Attack");
                    break;
                case DestructorAssaState.LEAK:
                    Log("Enter to state : Leak");
                    break;
            }
        }

        protected override void OnExitState(string state) {
            switch (State) {
                case DestructorAssaState.PATROL:
                    Log("Exit from state : Patrol");
                    break;
                case DestructorAssaState.OBJECTIVE:
                    Log("Exit from state : Objective");
                    break;
                case DestructorAssaState.HELP:
                    Log("Exit from state : Help");
                    break;
                case DestructorAssaState.ATTACK:
                    Log("Exit from state : Attack");
                    break;
                case DestructorAssaState.LEAK:
                    Log("Exit from state : Leak");
                    break;
            }
        }

        protected override void OnStateUpdate() {
            switch (State) {
                case DestructorAssaState.PATROL:
                    Patrol();
                    break;
                case DestructorAssaState.OBJECTIVE:
                    Objective();
                    break;
                case DestructorAssaState.HELP:
                    Help();
                    break;
                case DestructorAssaState.ATTACK:
                    Attack();
                    break;
                case DestructorAssaState.LEAK:
                    Leak();
                    break;
            }
        }

        // Méthodes :
        // Transmissions des informations :
        protected override void OnInformationsReceived(string data) {
            Order informations = JsonUtility.FromJson<Order>(data);

            // Vérification de si il attaque ou non :
            if (attacking) {
                return;
            }

            switch (informations.orderName) {
                case "Objectif":
                    controlEnnemyLocated = true;
                    controlEnnemyPosition = informations.orderPosition;
                    State = DestructorAssaState.OBJECTIVE;
                    break;
                case "Help - Control":
                    objectivePosition = informations.orderPosition;
                    State = DestructorAssaState.HELP;
                    break;
                case "Help - Interceptor":
                    objectivePosition = informations.orderPosition;
                    State = DestructorAssaState.HELP;
                    break;
                case "Help - Destructor":
                    objectivePosition = informations.orderPosition;
                    State = DestructorAssaState.HELP;
                    break;
            }
        }

        private void InformationsTransmissions() {
            TargetInformations closestTargetsInformations = GetClosestTarget();

            if (closestTargetsInformations != null && closestTargetsInformations.team != Team) {
                SendInformationTo(Control, JsonUtility.ToJson(closestTargetsInformations));
            }
        }

        // Déplacements :
        private void SearchRandomDestination() {
            bool randomPositionFounded = false;
            while (!randomPositionFounded) {
                targetPosition = GetRandomPositionOnNavMesh(transform.position, Random.Range(1.0f, 50.0f));
                randomPositionFounded = GoTo(targetPosition);
            }
            middleReached = true;
        }

        // Attaqué :
        protected override void OnHit(Vector3 direction) {
            if (CanView(controlEnnemyPosition)) {
                return;
            }

            if ((Life <= totalLife / 2.5f) && (leakAuthorized)) {
                State = DestructorAssaState.LEAK;
                totalLife = totalLife / 2.5f;
            } else {
                transform.LookAt(direction);
                InformationsTransmissions();
            }
        }

        // Tir continu si le mode hardcore n'est pas activé :
        private void ContinuousShooting() {
            if (continuousShooting) {
                Shoot(transform.forward);
            }
        }

        // Différents états :
        private void Patrol() {
            // Vérification de la présence d'ennemis :
            List<TargetInformations> targets = GetTargets();

            if (targets.Count > 0) {
                // On détecte le premier ennemi et on l'attaque :
                targetPosition = targets[0].position;
                Stop();
                transform.LookAt(targetPosition);
                State = DestructorAssaState.ATTACK;
                return;
            }

            // Debbuger pour "middleReached" :
            if (!middleReached) {
                if (Time.time - Time.deltaTime > 10.0f) {
                    middleReached = true;
                    middlePosition = transform.position;
                    SearchRandomDestination();
                }
            }

            ContinuousShooting();

            if (controlEnnemyLocated) {
                State = DestructorAssaState.OBJECTIVE;
                return;
            }

            // Vérification de la présence d'un mur :
            if ((IsThereAWall(transform.forward / 1.25f)) && (middleReached)) {
                SearchRandomDestination();
            }

            // Déplacements aléatoirement :
            if (((Vector3.Distance(transform.position, targetPosition) <= 1.0f) || (Time.time - Time.deltaTime > 1.0f)) && (!controlEnnemyLocated) && (middleReached)) {
                SearchRandomDestination();
            }
        }

        // Objectif :
        private void Objective() {
            ContinuousShooting();

            if (CanView(controlEnnemyPosition)) {
                Stop();
                transform.LookAt(controlEnnemyPosition);
                Shoot((controlEnnemyPosition - transform.position).normalized);
                return;
            } else {
                GoTo(controlEnnemyPosition);
            }
        }

        // Aide :
        private void Help() {
            ContinuousShooting();
            GoTo(objectivePosition);

            if (Vector3.Distance(transform.position, objectivePosition) <= 12.5f) {
                State = DestructorAssaState.ATTACK;
            }
        }

        private void Attack() {
            // Vérification de la présence d'ennemis :
            List<TargetInformations> targets = GetTargets();

            if (targets.Count > 0) {
                attacking = true;
                InformationsTransmissions();

                TargetInformations closestTargets = GetClosestTarget();
                targetPosition = closestTargets.position;
                timeResearch = 5.0f;

                // On s'arrête pour tirer :
                Stop();
                transform.LookAt(targetPosition);
                Shoot((targetPosition - transform.position).normalized);
            }

            // S'il n'y a plus d'ennemis:
            if (targets.Count == 0) {
                GoTowards(transform.forward, 10.0f);
                ContinuousShooting();
                timeResearch--;

                if (timeResearch <= 0f) {
                    State = DestructorAssaState.PATROL;
                    timeResearch = 5.0f;
                    attacking = false;
                    return;
                }
            }
        }

        private void Leak() {
            GoTo(controlPosition);
            timeOnHit--;

            if ((Vector3.Distance(transform.position, controlPosition) <= 15.0f) || (timeOnHit <= 0f)) {
                State = DestructorAssaState.PATROL;
                timeOnHit = 10.0f;
                return;
            }
        }
    }


    // Interceptor
    public class InterceptorAssa : InterceptorBase {
        // Variables :
        private Vector3 targetPosition;
        private Vector3 middlePosition;
        private bool middleReached = false;
        private bool continuousShooting = !ArenaManager.Instance.Hardcore;

        private Vector3 controlPosition;
        private Vector3 controlEnnemyPosition;
        private Vector3 objectivePosition;
        private bool controlAttacked = false;
        private bool controlEnnemyLocated;

        private float timeResearch = 5.0f;
        private float timeOnHit = 10.0f;
        private bool attacking = false;

        private float totalLife;
        private bool leakAuthorized = true;

        public class Order {
            public string orderName;
            public Vector3 orderPosition;
        }

        // États :
        public static class InterceptorAssaState {
            public const string PATROL = "Patrol";
            public const string OBJECTIVE = "Objective";
            public const string HELP = "Help";
            public const string ATTACK = "Attack";
            public const string LEAK = "Leak";
        }

        // Machine à état :
        protected override void InitializeAgent() {
            State = InterceptorAssaState.PATROL;
            controlPosition = Control.transform.position;
            totalLife = Life;
        }

        protected override void OnEnterState(string state) {
            switch (State) {
                case InterceptorAssaState.PATROL:
                    Log("Enter to state : Patrol");
                    // Go to the middle of the arena :
                    // GoTowards(transform.position, -1.0f);

                    // Go to the opposite side of the arena :
                    GoTo(-transform.position);
                    break;
                case InterceptorAssaState.OBJECTIVE:
                    Log("Enter to state : Objective");
                    break;
                case InterceptorAssaState.HELP:
                    Log("Enter to state : Help");
                    break;
                case InterceptorAssaState.ATTACK:
                    Log("Enter to state : Attack");
                    break;
                case InterceptorAssaState.LEAK:
                    Log("Enter to state : Leak");
                    break;
            }
        }

        protected override void OnExitState(string state) {
            switch (State) {
                case InterceptorAssaState.PATROL:
                    Log("Exit from state : Patrol");
                    break;
                case InterceptorAssaState.OBJECTIVE:
                    Log("Exit from state : Objective");
                    break;
                case InterceptorAssaState.HELP:
                    Log("Exit from state : Help");
                    break;
                case InterceptorAssaState.ATTACK:
                    Log("Exit from state : Attack");
                    break;
                case InterceptorAssaState.LEAK:
                    Log("Exit from state : Leak");
                    break;
            }
        }

        protected override void OnStateUpdate() {
            switch (State) {
                case InterceptorAssaState.PATROL:
                    Patrol();
                    break;
                case InterceptorAssaState.OBJECTIVE:
                    Objective();
                    break;
                case InterceptorAssaState.HELP:
                    Help();
                    break;
                case InterceptorAssaState.ATTACK:
                    Attack();
                    break;
                case InterceptorAssaState.LEAK:
                    Leak();
                    break;
            }
        }

        // Méthodes :
        // Transmissions des informations :
        protected override void OnInformationsReceived(string data) {
            Order informations = JsonUtility.FromJson<Order>(data);

            // Vérification de si il attaque ou non :
            if (attacking) {
                return;
            }

            switch (informations.orderName) {
                case "Objectif":
                    controlEnnemyLocated = true;
                    controlEnnemyPosition = informations.orderPosition;
                    State = InterceptorAssaState.OBJECTIVE;
                    break;
                case "Help - Control":
                    objectivePosition = informations.orderPosition;
                    State = InterceptorAssaState.HELP;
                    break;
                case "Help - Interceptor":
                    objectivePosition = informations.orderPosition;
                    State = InterceptorAssaState.HELP;
                    break;
            }
        }

        private void InformationsTransmissions() {
            TargetInformations closestTargetsInformations = GetClosestTarget();

            if (closestTargetsInformations != null && closestTargetsInformations.team != Team) {
                SendInformationTo(Control, JsonUtility.ToJson(closestTargetsInformations));
            }
        }

        // Déplacements :
        private void SearchRandomDestination() {
            bool randomPositionFounded = false;
            while (!randomPositionFounded) {
                targetPosition = GetRandomPositionOnNavMesh(transform.position, Random.Range(1.0f, 50.0f));
                randomPositionFounded = GoTo(targetPosition);
            }
            middleReached = true;
        }

        // Attaqué :
        protected override void OnHit(Vector3 direction) {
            if ((Life <= totalLife / 2) && (leakAuthorized)) {
                State = InterceptorAssaState.LEAK;
                totalLife = totalLife / 2;
            } else {
                transform.LookAt(direction);
                InformationsTransmissions();
            }
        }

        // Tir continu si le mode hardcore n'est pas activé :
        private void ContinuousShooting() {
            if (continuousShooting) {
                Shoot(transform.forward);
            }
        }

        // Différents états :
        private void Patrol() {
            // Vérification de la présence d'ennemis :
            List<TargetInformations> targets = GetTargets();

            if (targets.Count > 0) {
                // On détecte le premier ennemi et on l'attaque :
                targetPosition = targets[0].position;
                Stop();
                transform.LookAt(targetPosition);
                State = InterceptorAssaState.ATTACK;
                return;
            }

            // Debbuger pour "middleReached" :
            if (!middleReached) {
                if (Time.time - Time.deltaTime > 10.0f) {
                    middleReached = true;
                    middlePosition = transform.position;
                    SearchRandomDestination();
                }
            }

            ContinuousShooting();

            if (controlEnnemyLocated) {
                State = InterceptorAssaState.OBJECTIVE;
                return;
            }

            // Vérification de la présence d'un mur :
            if ((IsThereAWall(transform.forward / 2.25f)) && (middleReached)) {
                SearchRandomDestination();
            }

            // Déplacements aléatoirement :
            if (((Vector3.Distance(transform.position, targetPosition) <= 1.0f) || (Time.time - Time.deltaTime > 1.0f)) && (!controlEnnemyLocated) && (middleReached)) {
                SearchRandomDestination();
            }
        }

        // Objectif :
        private void Objective() {
            ContinuousShooting();

            if (CanView(controlEnnemyPosition)) {
                Stop();
                transform.LookAt(controlEnnemyPosition);
                Shoot((controlEnnemyPosition - transform.position).normalized);
                return;
            } else {
                GoTo(controlEnnemyPosition);
            }
        }

        // Aide :
        private void Help() {
            ContinuousShooting();
            GoTo(objectivePosition);

            if (Vector3.Distance(transform.position, objectivePosition) <= 20.0f) {
                State = InterceptorAssaState.ATTACK;
            }
        }

        private void Attack() {
            // Vérification de la présence d'ennemis :
            List<TargetInformations> targets = GetTargets();

            if (targets.Count > 0) {
                attacking = true;
                InformationsTransmissions();

                TargetInformations closestTargets = GetClosestTarget();
                targetPosition = closestTargets.position;
                timeResearch = 5.0f;

                // On s'arrête pour tirer :
                Stop();
                transform.LookAt(targetPosition);
                Shoot((targetPosition - transform.position).normalized);

                // Si un "Destructor" est trop proche, on fuit :
                float distance = Vector3.Distance(transform.position, closestTargets.position);

                if ((distance <= 12.5f) && (closestTargets.kind == EntityKind.Destructor)) {
                    GoTowards(transform.position, -1.0f);
                }
            }

            // S'il n'y a plus d'ennemis:
            if (targets.Count == 0) {
                GoTowards(transform.forward, 10.0f);
                ContinuousShooting();
                timeResearch--;

                if (timeResearch <= 0f) {
                    State = InterceptorAssaState.PATROL;
                    timeResearch = 5.0f;
                    attacking = false;
                    return;
                }
            }
        }

        private void Leak() {
            GoTo(controlPosition);
            timeOnHit--;

            if ((Vector3.Distance(transform.position, controlPosition) <= 15.0f) || (timeOnHit <= 0f)) {
                State = InterceptorAssaState.PATROL;
                timeOnHit = 10.0f;
                return;
            }
        }
    }
}