﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public static float enemySpeed; 

    private GameObject target;
    private NavMeshAgent navmesh;
    private Animator anim;
    private SkinnedMeshRenderer enemyMesh;
    private bool isAttacking = false;
    private bool isDead = false;

    [SerializeField] private int health;
    public static int staticEnemyHealth;
    public static int pointsPerKill;
    public int thisPointsPerKill;

    void Start()
    {
        navmesh = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        target = FindObjectOfType<Player>().gameObject;
        enemyMesh = transform.Find("EnemyDroid").transform.Find("Starwars_Droid").GetComponent<SkinnedMeshRenderer>();
        GetComponent<NavMeshAgent>().speed = enemySpeed;
        health = staticEnemyHealth;
        thisPointsPerKill = pointsPerKill;
    }

    void Update()
    {
        if (GameManager.instance.state == GameManager.GameStates.GameOn)
        {
            navmesh.SetDestination(new Vector3(target.transform.position.x, transform.position.y, target.transform.position.z));

            if (Input.GetKeyDown(KeyCode.Space))
            {
                anim.Play("Hit");
            }
            if (Input.GetKeyUp(KeyCode.Space))
            {
                anim.Play("Walk");
            }

            if (EnemyInRadius() && !isAttacking && !isDead)
            {
                isAttacking = true;
                StartCoroutine("DamagePlayerCoroutine");
            }
        }
    }

    private void OnTriggerEnter(Collider col)
    {
        if (col.tag == "Bullet") {
            TakeHit(Bullet.damage);
        }
    }

    [SerializeField] private ParticleSystem gotHitEffect;
    [SerializeField] private AudioClip enemyHitSound;
    void TakeHit(int damage) {
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
        else {
            FindObjectOfType<Player>().gameObject.GetComponent<AudioSource>().PlayOneShot(enemyHitSound, 0.3f);
            ParticleSystem gotHitPS = Instantiate(gotHitEffect, transform.position + new Vector3(0, 0.65f, 0), Quaternion.identity) as ParticleSystem;
            gotHitPS.Play();
            Destroy(gotHitPS.gameObject, 1);
        }
    }

    [SerializeField] private AudioClip deathSound;
    void Die() {
        GameManager.instance.totalEnemiesOnMap--;
        isDead = true;
        enemyMesh.enabled = false;
        StartCoroutine("DeathEffect");
        GetComponent<BoxCollider>().enabled = false;
        GameManager.instance.UpdateScore(this);
    }

    [SerializeField] private ParticleSystem deathEffect;
    IEnumerator DeathEffect() {
        ParticleSystem dieParticleSystem = Instantiate(deathEffect, transform.position + new Vector3(0, 0.65f, 0), Quaternion.identity) as ParticleSystem;
        dieParticleSystem.Play();
        FindObjectOfType<Player>().gameObject.GetComponent<AudioSource>().PlayOneShot(deathSound, 0.5f);
        yield return new WaitForSeconds(1);
        Destroy(gameObject);
        Destroy(dieParticleSystem.gameObject);
    }

    [SerializeField] private float hitsPerSecond;
    IEnumerator DamagePlayerCoroutine() {
        if (!EnemyInRadius() || isDead)
        {
            StopCoroutine("DamagePlayerCoroutine");
            isAttacking = false;
        }
        else
        {
            yield return new WaitForSeconds(1 / hitsPerSecond);
            DamagePlayer(damageDealtOnHit);
            StartCoroutine(DamagePlayerCoroutine());
        }
    }

    [SerializeField] private int damageDealtOnHit;
    void DamagePlayer(int damage) {
        target.GetComponent<Player>().TakeHit(damage);
    }

    bool EnemyInRadius() {
        Vector3 enemyToPlayer = transform.position - target.transform.position;
        return Mathf.Abs(enemyToPlayer.magnitude) < navmesh.stoppingDistance; //Damage player if enemy is held at its stopping distance
    }

}

/* Cool piece of code
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit)) {
                navmesh.SetDestination(hit.point);
                Debug.Log(hit.point);
                Vector3 cameraPos = Camera.main.transform.position;
                Debug.DrawRay(cameraPos, (hit.point - cameraPos), Color.green, 7f);
            }
        }
        */
