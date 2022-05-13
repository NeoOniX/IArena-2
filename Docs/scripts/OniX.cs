using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IArena.OniX
{
    // Control
    public class ControlOniX : ControlBase
    {
        private List<Vector3> knownEnnemies = new List<Vector3>();
        private Vector3 controlLoc;

        protected override void Start()
        {
            if (transform != null)
            {
                Message msg = new Message(Type.GoTo, Vector3.zero - transform.position);

                foreach (Agent agent in Agents)
                {
                    SendInformationTo(agent, JsonUtility.ToJson(msg));
                }
            }

            base.Start();
        }

        protected override void OnHit(Vector3 direction)
        {
            Message msg = new Message(Type.Defend, transform.position);

            foreach (Agent agent in Agents)
            {
                SendInformationTo(agent, JsonUtility.ToJson(msg));
            }
        }

        protected override void OnInformationsReceived(string data)
        {
            Message msg = JsonUtility.FromJson<Message>(data);

            Message toSend;

            switch (msg.type)
            {
                case Type.ControlFound:
                    controlLoc = msg.location;
                    toSend = msg;
                    toSend.origin = this;
                    foreach (Agent agent in Agents.FindAll((match) => match.GetComponent<Entity>() != msg.origin))
                    {
                        SendInformationTo(agent, JsonUtility.ToJson(msg));
                    }
                    break;
                case Type.Attacking:
                    // Add ennemy location to list of known ennemies locations
                    knownEnnemies.Add(msg.location);
                    // Remove the coordinates after 5 seconds
                    StartCoroutine(RemoveAfterDelay(msg.location));
                    break;
                case Type.AskForOrder:
                    if (knownEnnemies.Count > 0)
                    {
                        // Find nearest ennemy
                        Vector3 dest = knownEnnemies[0];

                        foreach (Vector3 ennemy in knownEnnemies)
                        {
                            if (Vector3.Distance(msg.location, ennemy) > Vector3.Distance(msg.location, dest))
                            {
                                dest = ennemy;
                            }
                        }

                        toSend = new Message(Type.GoTo, dest, this);
                    } else
                    {
                        toSend = new Message(Type.Explore, msg.location, this);
                    }
                    SendInformationTo(msg.origin, JsonUtility.ToJson(toSend));
                    break;
                default:
                    break;
            }
        }

        private IEnumerator RemoveAfterDelay(Vector3 toDelete)
        {
            yield return new WaitForSeconds(5);
            knownEnnemies.Remove(toDelete);
        }
    }

    // Destructor
    public class DestructorOniX : DestructorBase
    {
        private Vector3 _loc;
        private Vector3 _shoot;

        protected override void InitializeAgent()
        {

        }

        protected override void OnEnterState(string state)
        {
            switch (State)
            {
                case UnitState.Going:
                case UnitState.Control:
                    GoTo(_loc);
                    break;
                case UnitState.Base:
                    GoTo(Control.transform.position);
                    break;
                case UnitState.Explore:
                case UnitState.Attacking:
                    GoTo(transform.position);
                    break;
                case UnitState.Waiting:
                    GoTo(transform.position);
                    Message msg = new Message(Type.AskForOrder, transform.position, this);
                    SendInformationTo(Control, JsonUtility.ToJson(msg));
                    break;
                default:
                    break;
            }
        }

        protected override void OnExitState(string state)
        {

        }

        protected override void OnStateUpdate()
        {
            Message msg;
            List<TargetInformations> ennemies;

            switch (State)
            {
                case UnitState.Control:
                    ennemies = GetTargets().FindAll((match) => match.team != Team && match.kind == EntityKind.Control);
                    if (ennemies.Count > 0)
                    {
                        _shoot = (ennemies[0].position - transform.position).normalized;
                        State = UnitState.Attacking;
                    }
                    break;
                case UnitState.Going:
                    ennemies = GetTargets().FindAll((match) => match.team != Team);
                    if (ennemies.Count > 0)
                    {
                        TargetInformations control = ennemies.Find(match => match.kind == EntityKind.Control);
                        if (control != null)
                        {
                            msg = new Message(Type.ControlFound, control.position, this);
                            State = UnitState.Attacking;
                        }
                        else
                        {
                            msg = new Message(Type.Attacking, ennemies[0].position, this);
                            State = UnitState.Attacking;
                        }
                        SendInformationTo(Control, JsonUtility.ToJson(msg));
                    }
                    else if (Vector3.Distance(transform.position, _loc) < 1f)
                    {
                        State = UnitState.Waiting;
                    }
                    break;
                case UnitState.Attacking:
                    ennemies = GetTargets().FindAll((match) => match.team != Team);
                    if (ennemies.Count > 0)
                    {
                        _shoot = (ennemies[0].position - transform.position).normalized;
                        transform.rotation = Quaternion.LookRotation(_shoot, Vector3.up);
                        Shoot(_shoot);
                        msg = new Message(Type.Attacking, ennemies[0].position, this);
                        SendInformationTo(Control, JsonUtility.ToJson(msg));
                    }
                    else
                    {
                        if (_loc != null)
                        {
                            State = UnitState.Going;
                        }
                        else
                        {
                            State = UnitState.Waiting;
                        }
                    }
                    break;
                case UnitState.Explore:
                    while (!GoTo(GetRandomPositionOnNavMesh(transform.position, 10f))) ;
                    ennemies = GetTargets().FindAll((match) => match.team != Team);
                    if (ennemies.Count > 0)
                    {
                        TargetInformations control = ennemies.Find(match => match.kind == EntityKind.Control);
                        if (control != null)
                        {
                            msg = new Message(Type.ControlFound, control.position, this);
                            State = UnitState.Attacking;
                        }
                        else
                        {
                            msg = new Message(Type.Attacking, ennemies[0].position, this);
                            State = UnitState.Attacking;
                        }
                        SendInformationTo(Control, JsonUtility.ToJson(msg));
                    }
                    break;
                default:
                    break;
            }
        }

        protected override void OnInformationsReceived(string data)
        {
            Message msg = JsonUtility.FromJson<Message>(data);

            switch (msg.type)
            {
                case Type.GoTo:
                    _loc = msg.location;
                    State = UnitState.Going;
                    break;
                case Type.ControlFound:
                    _loc = msg.location;
                    State = UnitState.Control;
                    break;
                case Type.Defend:
                    State = UnitState.Base;
                    break;
                case Type.Explore:
                    State = UnitState.Explore;
                    break;
                default:
                    break;
            }
        }
    }

    // Interceptor
    public class InterceptorOniX : InterceptorBase
    {
        private Vector3 _loc;
        private Vector3 _shoot;

        protected override void InitializeAgent()
        {

        }

        protected override void OnEnterState(string state)
        {
            switch (State)
            {
                case UnitState.Going:
                case UnitState.Control:
                    GoTo(_loc);
                    break;
                case UnitState.Base:
                    GoTo(Control.transform.position);
                    break;
                case UnitState.Explore:
                case UnitState.Attacking:
                    GoTo(transform.position);
                    break;
                case UnitState.Waiting:
                    GoTo(transform.position);
                    Message msg = new Message(Type.AskForOrder, transform.position, this);
                    SendInformationTo(Control, JsonUtility.ToJson(msg));
                    break;
                default:
                    break;
            }
        }

        protected override void OnExitState(string state)
        {

        }

        protected override void OnStateUpdate()
        {
            Message msg;
            List<TargetInformations> ennemies;

            switch (State)
            {
                case UnitState.Control:
                    ennemies = GetTargets().FindAll((match) => match.team != Team && match.kind == EntityKind.Control);
                    if (ennemies.Count > 0)
                    {
                        _shoot = (ennemies[0].position - transform.position).normalized;
                        State = UnitState.Attacking;
                    }
                    break;
                case UnitState.Going:
                    ennemies = GetTargets().FindAll((match) => match.team != Team);
                    if (ennemies.Count > 0)
                    {
                        TargetInformations control = ennemies.Find(match => match.kind == EntityKind.Control);
                        if (control != null)
                        {
                            msg = new Message(Type.ControlFound, control.position, this);
                            State = UnitState.Attacking;
                        }
                        else
                        {
                            msg = new Message(Type.Attacking, ennemies[0].position, this);
                            State = UnitState.Attacking;
                        }
                        SendInformationTo(Control, JsonUtility.ToJson(msg));
                    } else if (Vector3.Distance(transform.position, _loc) < 1f)
                    {
                        State = UnitState.Waiting;
                    }
                    break;
                case UnitState.Attacking:
                    ennemies = GetTargets().FindAll((match) => match.team != Team);
                    if (ennemies.Count > 0)
                    {
                        _shoot = (ennemies[0].position - transform.position).normalized;
                        transform.rotation = Quaternion.LookRotation(_shoot, Vector3.up);
                        Shoot(_shoot);
                        msg = new Message(Type.Attacking, ennemies[0].position, this);
                        SendInformationTo(Control, JsonUtility.ToJson(msg));
                    }
                    else
                    {
                        if (_loc != null)
                        {
                            State = UnitState.Going;
                        } else
                        {
                            State = UnitState.Waiting;
                        }
                    }
                    break;
                case UnitState.Explore:
                    while (!GoTo(GetRandomPositionOnNavMesh(transform.position, 10f)));
                    ennemies = GetTargets().FindAll((match) => match.team != Team);
                    if (ennemies.Count > 0)
                    {
                        TargetInformations control = ennemies.Find(match => match.kind == EntityKind.Control);
                        if (control != null)
                        {
                            msg = new Message(Type.ControlFound, control.position, this);
                            State = UnitState.Attacking;
                        }
                        else
                        {
                            msg = new Message(Type.Attacking, ennemies[0].position, this);
                            State = UnitState.Attacking;
                        }
                        SendInformationTo(Control, JsonUtility.ToJson(msg));
                    }
                    break;
                default:
                    break;
            }
        }

        protected override void OnInformationsReceived(string data)
        {
            Message msg = JsonUtility.FromJson<Message>(data);

            switch (msg.type)
            {
                case Type.GoTo:
                    _loc = msg.location;
                    State = UnitState.Going;
                    break;
                case Type.ControlFound:
                    _loc = msg.location;
                    State = UnitState.Control;
                    break;
                case Type.Defend:
                    State = UnitState.Base;
                    break;
                case Type.Explore:
                    State = UnitState.Explore;
                    break;
                default:
                    break;
            }
        }
    }

    // Utility classes

    public static class UnitState
    {
        public const string Explore = "Explore";
        public const string Waiting = "Waiting";
        public const string Attacking = "Attacking";
        public const string Going = "Going";
        public const string Base = "Base";
        public const string Control = "Control";
    }

    public enum Type
    {
        AskForOrder,
        GoTo,
        Explore,
        ControlFound,
        Attacking,
        Defend,
    }

    public class Message
    {
        public Type type;
        public Vector3 location;
        public Entity origin;

        public Message() { }

        public Message(Type type, Vector3 location)
        {
            this.type = type;
            this.location = location;
        }

        public Message(Type type, Vector3 location, Entity origin)
        {
            this.type = type;
            this.location = location;
            this.origin = origin;
        }
    }
}