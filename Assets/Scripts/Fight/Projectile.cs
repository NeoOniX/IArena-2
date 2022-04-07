using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public GameObject laserGo;
    public ParticleSystem impactFX;

    private Entity thrower;
    private Vector3 shootDirection;
    private float startSpeed = 20f;
    private float speed;
    private float startLifetime = 2f;
    private float lifetime;
    private float damage = 0f;

    public void Setup(Entity thrower, Vector3 dir, float damage){
        this.shootDirection = dir;
        this.damage = damage;
        this.thrower = thrower;
        transform.rotation = Quaternion.LookRotation(dir);
        lifetime = startLifetime;
        speed = startSpeed;
    }
    
    void Update()
    {
        transform.position = transform.position + shootDirection * speed * Time.deltaTime;
        
        lifetime -= Time.deltaTime;
        if (lifetime <= 0){
            BackToPool();
        }
    }

    void OnTriggerEnter(Collider collider){
        if (collider.gameObject != null && thrower != null && collider.gameObject == thrower.gameObject){
            //Ignore ourself
            return;
        }
        if (speed > 0f && lifetime > 0){
            Entity hitEntity = collider.gameObject.GetComponentInParent<Entity>();
            if (hitEntity != null){
                if (ArenaManager.Instance.Hardcore || (!ArenaManager.Instance.Hardcore && thrower.Team != hitEntity.Team)){
                    //Only apply damage if we are in hardcore mode with friendly fire enable or otherwise if it's an enemy
                    ApplyDamage(hitEntity);
                }
            }
            speed = 0;
            Explode();
        }
    }

    private void ApplyDamage(Entity entity){
        if (entity != null){
            Vector3 hitDirection = (transform.position - entity.transform.position).normalized;
            hitDirection.y = 0;
            entity.ReceiveHit(damage,hitDirection);
        }
    }

    void Explode(){
        if (!impactFX.isPlaying){
            laserGo.SetActive(false);
            impactFX.Play();
        }
    }

    public void OnFxEnd(){
        BackToPool();
    }

    private void BackToPool(){
        gameObject.SetActive(false);
        laserGo.SetActive(true);
    }
}
