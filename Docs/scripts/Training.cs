using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IArena.Training
{
    // Control
    public class ControlTraining : ControlBase
    {
        protected override void OnInformationsReceived(string data)
        {
            //nothing to do here, because I'm only here to be destroyed :) 
        }
    }

    // Destructor
    public class DestructorTraining : DestructorBase
    {
        public static class DestructorTrainingState {
            public const string IDLE = "Idle";
            public const string PATROL = "Patrol";
        }

        private Vector3 targetPosition;

        protected override void InitializeAgent()
        {
            State = DestructorTrainingState.IDLE;
        }

        protected override void OnEnterState(string state)
        {
            switch (State){
                case DestructorTrainingState.IDLE:
                    Log("Enter to state : Idle");
                    //Search for a destination to go
                    SearchDestination();
                    //Then let's put our interceptor in patrol
                    State = DestructorTrainingState.PATROL;
                break;
                case DestructorTrainingState.PATROL:
                    Log("Enter to state : Patrol");
                break;
            }
        }

        protected override void OnExitState(string state)
        {
            switch (State){
                case DestructorTrainingState.IDLE:
                    Log("Exit from state : Idle");
                break;
                case DestructorTrainingState.PATROL:
                    Log("Exit from state : Patrol");
                break;
            }
        }

        protected override void OnInformationsReceived(string data)
        {
            //Here you may receive some useful informations from Control
        }

        protected override void OnStateUpdate()
        {
            switch (State){
                case DestructorTrainingState.IDLE:
                    //Do nothing here :) 
                break;
                case DestructorTrainingState.PATROL:
                    Patrol();
                break;
            }
        }

        protected override void OnHit(Vector3 direction)
        {
            //I'm under attack !!!
            //Vector3 direction given in parameter will give me the direction from where I'm attack
            //Maybe I should run away in opposite direction? Or die, or attack, or nothing...
        }

        private void Patrol(){
            //Check if we are arrived at position desired
            //Let's say that if distance is under or equal to 1 we are close enough to our target
            if (Vector3.Distance(transform.position, targetPosition) <= 1f){
                SearchDestination();
            }
        }

        private void SearchDestination(){
            bool foundGoal = false;
            while (!foundGoal){
                targetPosition = GetRandomPositionOnNavMesh(transform.position, Random.Range(1f,50f));
                foundGoal = GoTo(targetPosition);
            }        
        }
    }

    // Interceptor
    public class InterceptorTraining : InterceptorBase
    {
        public static class InterceptorTrainingState {
            public const string IDLE = "Idle";
            public const string PATROL = "Patrol";
            public const string ATTACK = "Attack";
        }

        private Vector3 targetPosition;

        protected override void InitializeAgent()
        {
            State = InterceptorTrainingState.IDLE;
        }

        protected override void OnEnterState(string state)
        {
            switch (State){
                case InterceptorTrainingState.IDLE:
                    Log("Enter to state : Idle");
                    //Search for a destination to go
                    SearchDestination();
                    //Then let's put our interceptor in patrol
                    State = InterceptorTrainingState.PATROL;
                break;
                case InterceptorTrainingState.PATROL:
                    Log("Enter to state : Patrol");
                break;
            }
        }

        protected override void OnExitState(string state)
        {
            switch (State){
                case InterceptorTrainingState.IDLE:
                    Log("Exit from state : Idle");
                break;
                case InterceptorTrainingState.PATROL:
                    Log("Exit from state : Patrol");
                break;
            }
        }

        protected override void OnInformationsReceived(string data)
        {
            //Here you may receive some useful informations from Control
        }

        protected override void OnStateUpdate()
        {
            switch (State){
                case InterceptorTrainingState.IDLE:
                    //Do nothing here :) 
                break;
                case InterceptorTrainingState.PATROL:
                    Patrol();
                break;
                case InterceptorTrainingState.ATTACK:
                    AttackUpdate();
                break;
            }
        }

        protected override void OnHit(Vector3 direction)
        {
            if (IsAlive){
                bool foundGoal = false;
                float distance = 8f;
                while (!foundGoal && distance > 0f){
                    distance--;
                    foundGoal = GoTowards(-direction.normalized, distance);
                }
            }
        }

        private void Patrol(){
            //Check if we have a target to shoot
            List<TargetInformations> targets = GetTargets();
            if (targets.Count > 0){
                //Let's take our first target and let's attack
                targetPosition = targets[0].position;
                State = InterceptorTrainingState.ATTACK;
                return;
            }

            //Check if we are arrived at position desired
            //Let's say that if distance is under or equal to 1 we are close enough to our target
            if (Vector3.Distance(transform.position, targetPosition) <= 1f){
                SearchDestination();
            }
        }

        private void SearchDestination(){
            bool foundGoal = false;
            while (!foundGoal){
                targetPosition = GetRandomPositionOnNavMesh(transform.position, Random.Range(1f,50f));
                foundGoal = GoTo(targetPosition);
            }        
        }
        
        private void AttackUpdate(){
            Shoot(targetPosition);
        }
    }
}