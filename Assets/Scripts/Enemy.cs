using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IE.RSB
{
    public class Enemy : MonoBehaviour
    {
        public static event Action<Enemy, bool> OnAnyEnemyHit;

        public class TransformStamp
        {
            public Transform m_transform;
            public Vector3 m_position;
            public Quaternion m_rotation;
        }

        [SerializeField] private Animator m_animator = null;
        [SerializeField] public int animalNo = 0;

        // Private class members.
        private CapsuleCollider[] m_ragdollBodies;
        private BulletTimeTarget[] m_btTargets;
        private List<TransformStamp> m_transformStamps = new List<TransformStamp>();
        private bool m_isDead = false;

        private GameManager m_gameManager = null;

        private bool moveFlg = false;
        private float speed = 0.2f;
        public bool isDead => m_isDead;
        [HideInInspector] public bool isRayHit = false;
        [SerializeField] int MaxHP = 1;
        [SerializeField] int currHP = 1;
        [SerializeField] private Transform targetAttack;
        private float stopDistanceToTarget = 5f;
        [SerializeField] private Slider healthSlider;
        public Transform pointer;
        [SerializeField] private GameObject coinEffect;
        [SerializeField] private TextMeshProUGUI bonusText;
        [SerializeField] private GameObject hitEffectPrefab;
        private TextMeshProUGUI assignedDamageText;
        private DamageIndicatorManager dmgManager;
        public float HpTime = 1f;

        [SerializeField] private Image healthFillImage;
        [SerializeField] private Color normalColor = Color.green;
        [SerializeField] private Color hitColor = Color.red;
        public bool IsGoldRabbit = false;
        public int AnimalNo => animalNo;

        public static bool IsBonusAnimal(int animalId) => animalId >= 8 && animalId <= 12;

        public static bool IsBonusAnimalIndex(int zeroBasedIndex) => IsBonusAnimal(zeroBasedIndex + 1);
        private bool isPlayingAnimWait = false;
        EnemyPatrol patrol;
        [SerializeField] private Slider damageSlider; // Thanh vàng
        public bool IsRunningAnimal { get; set; } = false;

        void Awake()
        {
            patrol = GetComponent<EnemyPatrol>();
            m_gameManager = GameObject.Find("GameManager").gameObject.GetComponent<GameManager>();
            m_ragdollBodies = GetComponentsInChildren<CapsuleCollider>();
            m_btTargets = GetComponentsInChildren<BulletTimeTarget>();


            // Saves the current pose, meaning all the children transforms' position & rotation will be saved in a list of TransformStamps.
            SaveCurrentPose();

            //currHP = MaxHP;
            //if (healthSlider != null)
            //{
            //    healthSlider.gameObject.SetActive(true);
            //    healthSlider.value = 1f;
            //}


            currHP = MaxHP;
            if (healthSlider != null)
            {
                healthSlider.gameObject.SetActive(true);
                healthSlider.value = 1f;
            }

            if (damageSlider != null)
            {
                damageSlider.gameObject.SetActive(true);
                damageSlider.value = 1f;
            }

            if (IsBonusAnimal(animalNo))
                IsGoldRabbit = true;
        }

        public void SetTargetAttack(Transform target, float stopDistanceToTarget, float delay = 0)
        {
            this.stopDistanceToTarget = stopDistanceToTarget;
            targetAttack = target;
            DOVirtual.DelayedCall(4 + delay, () =>
            {
                if (targetAttack != null && !isDead)
                {
                    PrepareAttack();
                }
            });
        }

        private void PrepareAttack()
        {
            moveFlg = true;
            transform.LookAt(targetAttack);
            m_animator.SetTrigger("RunToAttack");
        }

        private void OnEnable()
        {
            SniperAndBallisticsSystem.EAnyHit += OnAnyHit;
            OnAnyEnemyHit += OnGlobalEnemyHit;
        }


        private void OnDisable()
        {
            SniperAndBallisticsSystem.EAnyHit -= OnAnyHit;
            OnAnyEnemyHit -= OnGlobalEnemyHit;
            CancelInvoke("RestorePose");
            RestorePose();
        }


        private void ShowDamageEffect(float damageAmount, bool isHeadshot, bool isHeartshot)
        {
            if (healthSlider == null || IsBonusAnimal(animalNo)) return;

            int percentLost;

            if (isHeadshot || isHeartshot)
            {
                percentLost = 200;
            }
            else
            {
                percentLost = 100;
            }

            var dmgManager = FindObjectOfType<DamageIndicatorManager>();
            if (dmgManager != null)
            {
                dmgManager.ShowDamage(this, percentLost);
            }
        }

        public void OnAnyHit(BulletPoint point)
        {
            if (m_isDead) return;

            // Check if the bullet hit any of the ragdoll bodies.
            for (int i = 0; i < m_ragdollBodies.Length; i++)
            {
                // If yes, enable all ragdolls and set dead flag.
                if (point.m_hitTransform == m_ragdollBodies[i].transform)
                {
                    bool isHeadshot = point.m_hitTransform.name.ToLower().Contains("head");
                    bool isHeartshot = point.m_hitTransform.name.ToLower().Contains("heartshot");

                    UIManager uiManager = FindObjectOfType<UIManager>();

                    if (uiManager != null && !IsBonusAnimal(animalNo))
                    {
                        //uiManager.ShowHitIndicator(isHeadshot);
                        if (isHeartshot)
                            uiManager.ShowHeartshot(true);


                        else if (isHeadshot)
                            uiManager.ShowHitIndicator(true);
                    }

                    if (MissionManager.Instance != null)
                    {
                        if (isHeadshot)
                        {
                            MissionManager.Instance.OnBrainHit();
                        }
                        else if (isHeartshot)
                        {
                            MissionManager.Instance.OnHeartHit();
                        }
                    }


                    //HP
                    if (isHeadshot || isHeartshot) currHP -= 2;
                    else currHP--;
                    currHP = Mathf.Clamp(currHP, 0, MaxHP);
                    ShowDamageEffect(isHeadshot || isHeartshot ? 2 : 1, isHeadshot, isHeartshot);
                    if (hitEffectPrefab != null)
                    {
                        GameObject bloodEffect = Instantiate(
                            hitEffectPrefab,
                            point.m_hitTransform.position,
                            Quaternion.LookRotation(point.m_hitNormal)
                        );
                        Destroy(bloodEffect, 2f);
                    }

                    if (healthSlider != null)
                    {
                        float hpRatio = (float)currHP / MaxHP;
                        healthSlider.value = hpRatio;
                        if (damageSlider != null)
                        {
                            DOTween.Kill(damageSlider);
                            damageSlider.DOValue(hpRatio, 0.5f).SetEase(Ease.OutCubic).SetDelay(0.3f);
                        }

                        if (currHP <= 0 && !m_isDead)
                        {
                            Death();
                        }
                    }

                    if (currHP > 0 && targetAttack == null)
                    {
                        m_animator.SetTrigger("GetHit");
                    }

                    BulletTimeTargetsActivation(false);
                    OnAnyEnemyHit?.Invoke(this, true);

                    break;
                }
            }
        }

        private void OnGlobalEnemyHit(Enemy sender, bool info)
        {
            if (sender == this) return; // only other enemies receive
            Debug.Log("Other enemies run...");
            if(!isDead && targetAttack == null) m_animator.SetTrigger("GetHit");
        }

        void Update()
        {
            if (moveFlg)
            {
                if (targetAttack != null)
                {
                    if (m_gameManager != null && m_gameManager.isFailed)
                    {
                        m_animator.speed = 0;
                        return;
                    }

                    if (m_animator.speed <= 0)
                    {
                        m_animator.speed = 1;
                    }


                    if (!IsGoldRabbit && Vector3.Distance(transform.position, targetAttack.position) <
                        stopDistanceToTarget)
                    {
                        m_animator.SetTrigger("Attack");
                        Debug.Log("Attack: " + gameObject.name);
                        moveFlg = false;
                        if (targetAttack.GetComponent<Animator>() != null)
                            targetAttack.GetComponent<Animator>().SetTrigger("Death");
                        DOVirtual.DelayedCall(2, () => { m_gameManager.Faild(); });
                    }
                }
                else
                {
                    transform.position += transform.forward * speed * Time.deltaTime;
                }
            }
        }

        void RestorePose()
        {
            Debug.Log("ここ入っている");
            // Make bodies kinematic & enable bullet time target components in the body parts again so that we can trigger bullet time again.
            //RagdollBodiesIsKinematic(true);
            BulletTimeTargetsActivation(true);

            for (int i = 0; i < m_transformStamps.Count; i++)
            {
                m_transformStamps[i].m_transform.localPosition = m_transformStamps[i].m_position;
                m_transformStamps[i].m_transform.localRotation = m_transformStamps[i].m_rotation;
            }

            m_animator.enabled = true;
            m_isDead = false;
        }


        private void SaveCurrentPose()
        {
            m_transformStamps.Clear();
            Transform[] allTransforms = GetComponentsInChildren<Transform>();

            for (int i = 0; i < allTransforms.Length; i++)
            {
                TransformStamp stamp = new TransformStamp();
                stamp.m_transform = allTransforms[i];
                stamp.m_position = allTransforms[i].localPosition;
                stamp.m_rotation = allTransforms[i].localRotation;
                m_transformStamps.Add(stamp);
            }
        }

        private void BulletTimeTargetsActivation(bool activate)
        {
            for (int i = 0; i < m_btTargets.Length; i++)
                m_btTargets[i].SetActivation(activate);
        }


        public void DoMoveAnimal(int animationNo)
        {
            m_animator.SetInteger("AnimationNo", animationNo);
            switch (animationNo)
            {
                case 1:
                    //Idle
                    break;
                case 2:
                    //Move
                    if (patrol != null) patrol.StartMove();
                    break;
                case 3:
                    //Attack
                    break;
            }
        }


        private void Death()
        {
            if (m_isDead) return;

            if (IsRunningAnimal)
            {
                MissionManager.Instance.OnRunningAnimalKilled();
                Debug.Log("Mission progress: Killed running animal");
            }
            else
            {
                MissionManager.Instance.OnAnimalKilled();
                Debug.Log("Mission progress: Killed animal");
            }

            if (patrol != null) patrol.StopMove();
            m_animator.SetTrigger("Death");
            if (moveFlg) moveFlg = false;
            m_isDead = true;

            if (healthSlider != null)
            {
                if (healthSlider.fillRect != null)
                    healthSlider.fillRect.gameObject.SetActive(false);
            }

            if (damageSlider != null)
            {
                DOTween.Kill(damageSlider);
                damageSlider.DOValue(0, 0.5f).SetEase(Ease.OutCubic).OnComplete(() =>
                {
                    damageSlider.gameObject.SetActive(false);
                });
            }


            if (m_gameManager)
            {
                DOVirtual.DelayedCall(1f, () => { m_gameManager.EnemyDown(animalNo); });
            }

            if (IsBonusAnimal(animalNo))
            {
                if (coinEffect != null)
                {
                    coinEffect.SetActive(true);
                    ParticleSystem ps = coinEffect.GetComponent<ParticleSystem>();
                    if (ps != null)
                    {
                        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                        ps.Play();
                    }
                }
            }

            if (bonusText != null)
            {
                bonusText.gameObject.SetActive(true);
                bonusText.color = new Color(1, 1, 0);
                bonusText.transform.localScale = Vector3.zero;
                bonusText.transform
                    .DOScale(Vector3.one * 1.5f, 0.4f)
                    .SetEase(Ease.OutBack)
                    .OnComplete(() =>
                    {
                        bonusText.DOFade(0, 0.5f).SetDelay(0.5f).OnComplete(() =>
                        {
                            bonusText.gameObject.SetActive(false);
                            bonusText.color = new Color(1, 1, 0, 1);
                            bonusText.transform.localScale = Vector3.zero;
                        });
                    });

                SaveDataManager.Instance.UpdateCoins(500);
                MissionManager.Instance.OnCoinsCollected(500);
            }
        }

        public void HuntingDogAttacked(Transform vfxtrans)
        {
            if (hitEffectPrefab != null)
            {
                GameObject bloodEffect = Instantiate(
                    hitEffectPrefab,
                    vfxtrans.position,
                    Quaternion.LookRotation(Vector3.up)
                );
                bloodEffect.transform.localScale = Vector3.one * 1.5f;
                Destroy(bloodEffect, 2f);
            }

            Death();
        }
    }
}