using UnityEngine;
using UnityEngine.Events;

public class ParticleSystemCallback : MonoBehaviour
{
    public UnityEvent onParticleSystemStopped;
    
    public void OnParticleSystemStopped(){
        if (onParticleSystemStopped != null){
            onParticleSystemStopped.Invoke();
        }
    }
}
