using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Enemy : MonoBehaviour
{
    [Header("Характеристики")]
    public string enemyName;
    public float maxHP;
    public float damage;
    public float attackSpeed;
    
    [Header("Ссылки")]
    public SpriteRenderer spriteRenderer;
    
    [Header("Визуал удара")]
    public SpriteRenderer slashRenderer;
    
    [Header("Смерть")]
    public float deathDuration = 0.5f;
    
    [Header("Появление")]
    public float spawnFallHeight = 5f;
    public float spawnBounceHeight = 0.2f;
    public float spawnDuration = 0.35f;
    
    [Header("Оглушение")]
    public float stunDuration = 2f;
    
    private float currentHP;
    private bool isDead = false;
    private bool isStunned = false;
    private float stunEndTime;
    private Coroutine hitAnimationRoutine;
    private Coroutine attackAnimationRoutine;
    private Coroutine slashRoutine;
    private Coroutine stunRoutine;
    private Vector3 originalLocalPos;
    private Vector3 originalScale;
    private Color originalColor;
    
    public System.Action<Enemy> OnEnemyDeath;
    public System.Action OnEnemyDamaged;
    
    public float CurrentHP => currentHP;
    public float MaxHP => maxHP;
    public bool IsStunned => isStunned;
    
    private List<ActiveEffect> activeEffects = new List<ActiveEffect>();
    private float slowMultiplier = 1f;
    private float bleedStacks = 0f;
    
    private class ActiveEffect
    {
        public enum Type { Poison, Burn }
        public Type type;
        public float damagePerSecond;
        public float remainingTime;
    }
    
    void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        
        originalLocalPos = transform.localPosition;
        originalScale = transform.localScale;
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
        
        if (slashRenderer != null)
            slashRenderer.gameObject.SetActive(false);
    }
    
    public void Init(string name, float hp, float dmg, float spd, Sprite spr)
    {
        enemyName = name;
        maxHP = hp;
        currentHP = hp;
        damage = dmg;
        attackSpeed = spd;
        isDead = false;
        isStunned = false;
        
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = spr;
            originalColor = spriteRenderer.color;
        }
        
        originalLocalPos = transform.localPosition;
        activeEffects.Clear();
        slowMultiplier = 1f;
        bleedStacks = 0f;
    }
    
    public void ApplyStun()
    {
        if (isDead) return;
        stunEndTime = Time.time + stunDuration;
        if (!isStunned)
        {
            isStunned = true;
            stunRoutine = StartCoroutine(StunRoutine());
        }
    }
    
    IEnumerator StunRoutine()
    {
        float height = spriteRenderer.sprite.bounds.size.y * originalScale.y;
        float angle = 0f;
        
        while (Time.time < stunEndTime)
        {
            if (this == null || isDead) yield break;
            angle += Time.deltaTime * 3f;
            
            float tilt = Mathf.Sin(angle) * 3f;
            transform.localRotation = Quaternion.Euler(0, 0, tilt);
            float rad = tilt * Mathf.Deg2Rad;
            float offsetX = Mathf.Sin(rad) * height * 0.2f;
            float offsetY = (1 - Mathf.Cos(rad)) * height * 0.2f;
            
            float scaleY = 1f + Mathf.Cos(angle) * 0.04f;
            float scaleDelta = (scaleY - 1f) * height * 0.5f;
            
            transform.localScale = new Vector3(originalScale.x, originalScale.y * scaleY, 1f);
            transform.localPosition = originalLocalPos + new Vector3(-offsetX, -offsetY + scaleDelta, 0);
            yield return null;
        }
        
        isStunned = false;
        transform.localRotation = Quaternion.identity;
        transform.localPosition = originalLocalPos;
        transform.localScale = originalScale;
    }
    
    public IEnumerator SpawnAnimation()
    {
        Vector3 targetPos = transform.localPosition;
        float startY = targetPos.y + spawnFallHeight;
        
        transform.localPosition = new Vector3(targetPos.x, startY, targetPos.z);
        
        float elapsed = 0f;
        while (elapsed < spawnDuration)
        {
            if (this == null || isDead) yield break;
            elapsed += Time.deltaTime;
            float t = elapsed / spawnDuration;
            t = t * t * t;
            float y = Mathf.Lerp(startY, targetPos.y, t);
            transform.localPosition = new Vector3(targetPos.x, y, targetPos.z);
            yield return null;
        }
        
        if (this == null || isDead) yield break;
        transform.localPosition = targetPos;
        
        float shakeAmount = 0.05f;
        float shakeDuration = 0.06f;
        elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            if (this == null || isDead) yield break;
            elapsed += Time.deltaTime;
            float decay = 1f - (elapsed / shakeDuration);
            transform.localPosition = targetPos + new Vector3(Random.Range(-1f, 1f) * shakeAmount * decay, 0f, 0f);
            yield return null;
        }
        if (this == null || isDead) yield break;
        transform.localPosition = targetPos;
        
        Vector3 slightCompress = new Vector3(originalScale.x * 1.05f, originalScale.y * 0.92f, 1f);
        elapsed = 0f;
        float compressDuration = 0.04f;
        while (elapsed < compressDuration)
        {
            if (this == null || isDead) yield break;
            elapsed += Time.deltaTime;
            float t = elapsed / compressDuration;
            transform.localScale = Vector3.Lerp(originalScale, slightCompress, t);
            yield return null;
        }
        
        elapsed = 0f;
        float bounceDuration = 0.15f;
        while (elapsed < bounceDuration)
        {
            if (this == null || isDead) yield break;
            elapsed += Time.deltaTime;
            float t = elapsed / bounceDuration;
            float y = Mathf.Sin(t * Mathf.PI) * spawnBounceHeight;
            transform.localPosition = new Vector3(targetPos.x, targetPos.y + y, targetPos.z);
            transform.localScale = Vector3.Lerp(slightCompress, originalScale, t);
            yield return null;
        }
        
        if (this == null || isDead) yield break;
        transform.localPosition = targetPos;
        transform.localScale = originalScale;
        originalLocalPos = targetPos;
    }
    
    void Update()
    {
        if (isDead) return;
        float dt = Time.deltaTime;
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            var effect = activeEffects[i];
            effect.remainingTime -= dt;
            if (effect.remainingTime <= 0)
            {
                activeEffects.RemoveAt(i);
                continue;
            }
            TakeDamage(effect.damagePerSecond * dt, false);
        }
    }
    
    public void ApplyPoison(float damagePerSecond, float duration)
    {
        var existing = activeEffects.Find(e => e.type == ActiveEffect.Type.Poison);
        if (existing != null)
        {
            existing.damagePerSecond = Mathf.Max(existing.damagePerSecond, damagePerSecond);
            existing.remainingTime = Mathf.Max(existing.remainingTime, duration);
        }
        else
        {
            activeEffects.Add(new ActiveEffect
            {
                type = ActiveEffect.Type.Poison,
                damagePerSecond = damagePerSecond,
                remainingTime = duration
            });
        }
    }
    
    public void ApplyBurn(float damagePerSecond, float duration)
    {
        var existing = activeEffects.Find(e => e.type == ActiveEffect.Type.Burn);
        if (existing != null)
        {
            existing.damagePerSecond = Mathf.Max(existing.damagePerSecond, damagePerSecond);
            existing.remainingTime = Mathf.Max(existing.remainingTime, duration);
        }
        else
        {
            activeEffects.Add(new ActiveEffect
            {
                type = ActiveEffect.Type.Burn,
                damagePerSecond = damagePerSecond,
                remainingTime = duration
            });
        }
    }
    
    public void ApplySlow(float percent) { slowMultiplier = 1f + percent / 100f; }
    public void AddBleedStack(float damage) { bleedStacks += damage; }
    public float GetBleedDamage() { return bleedStacks; }
    public float GetModifiedAttackSpeed() { return attackSpeed * slowMultiplier; }
    public void ResetSlow() { slowMultiplier = 1f; }
    
    public void ResetEffects()
    {
        activeEffects.Clear();
        slowMultiplier = 1f;
        bleedStacks = 0f;
    }
    
    public void TakeDamage(float dmg) { TakeDamage(dmg, true); }
    
    private void TakeDamage(float dmg, bool triggerEvents)
    {
        if (isDead) return;
        currentHP -= dmg;
        if (currentHP < 0) currentHP = 0;
        
        if (triggerEvents)
        {
            if (hitAnimationRoutine != null)
            {
                StopCoroutine(hitAnimationRoutine);
                if (spriteRenderer != null) spriteRenderer.color = originalColor;
            }
            hitAnimationRoutine = StartCoroutine(HitFlash());
            OnEnemyDamaged?.Invoke();
        }
        
        if (currentHP <= 0 && !isDead)
            Die();
    }
    
    public float GetDamage()
    {
        if (isStunned) return 0f;
        
        if (attackAnimationRoutine != null) StopCoroutine(attackAnimationRoutine);
        attackAnimationRoutine = StartCoroutine(AttackAnimation());
        
        if (slashRenderer != null)
        {
            if (slashRoutine != null) StopCoroutine(slashRoutine);
            slashRoutine = StartCoroutine(Slash());
        }
        return damage;
    }
    
    public bool IsAlive() => currentHP > 0 && !isDead;
    
    void Die()
    {
        if (isDead) return;
        isDead = true;
        
        OnEnemyDeath?.Invoke(this);
        
        if (hitAnimationRoutine != null) StopCoroutine(hitAnimationRoutine);
        if (attackAnimationRoutine != null) StopCoroutine(attackAnimationRoutine);
        if (slashRoutine != null) StopCoroutine(slashRoutine);
        if (stunRoutine != null) StopCoroutine(stunRoutine);
        
        if (spriteRenderer != null) spriteRenderer.color = originalColor;
        if (slashRenderer != null) slashRenderer.gameObject.SetActive(false);
        
        StartCoroutine(DeathAnimation());
    }
    
    IEnumerator Slash()
    {
        slashRenderer.gameObject.SetActive(true);
        Transform t = slashRenderer.transform;
        Vector3 origScale = t.localScale;
        t.localRotation = Quaternion.identity;
        t.localScale = Vector3.zero;
        Color c = slashRenderer.color;
        c.a = 1f;
        slashRenderer.color = c;
        
        float elapsed = 0f;
        float duration = 0.2f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float p = elapsed / duration;
            t.localScale = origScale * p;
            if (p > 0.7f) c.a = 1f - (p - 0.7f) / 0.3f;
            slashRenderer.color = c;
            yield return null;
        }
        slashRenderer.gameObject.SetActive(false);
        t.localScale = origScale;
    }
    
    IEnumerator HitFlash()
    {
        if (spriteRenderer == null) yield break;
        spriteRenderer.color = Color.red;
        float shakeAmount = 0.1f;
        float elapsed = 0f;
        float duration = 0.15f;
        Vector3 basePos = isStunned ? transform.localPosition : originalLocalPos;
        while (elapsed < duration)
        {
            float decay = 1f - (elapsed / duration);
            Vector2 shake = Random.insideUnitCircle * shakeAmount * decay;
            transform.localPosition = basePos + (Vector3)shake;
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = basePos;
        spriteRenderer.color = originalColor;
    }
    
    IEnumerator AttackAnimation()
    {
        Vector3 backPos = originalLocalPos + new Vector3(0.15f, -0.05f, 0f);
        Vector3 attackPos = originalLocalPos + new Vector3(-0.5f, 0f, 0f);
        float elapsed = 0f;
        float duration = 0.2f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            if (t < 0.25f)
            {
                float p = t / 0.25f;
                transform.localPosition = Vector3.Lerp(originalLocalPos, backPos, p);
                float sx = 1f - p * 0.1f;
                float sy = 1f + p * 0.15f;
                transform.localScale = new Vector3(originalScale.x * sx, originalScale.y * sy, 1f);
            }
            else if (t < 0.5f)
            {
                float p = (t - 0.25f) / 0.25f;
                transform.localPosition = Vector3.Lerp(backPos, attackPos, p * p);
                float sx = 1f + 0.2f * Mathf.Sin(p * Mathf.PI);
                float sy = 1f - 0.15f * Mathf.Sin(p * Mathf.PI);
                transform.localScale = new Vector3(originalScale.x * sx, originalScale.y * sy, 1f);
            }
            else
            {
                float p = (t - 0.5f) / 0.5f;
                p = 1f - (1f - p) * (1f - p);
                transform.localPosition = Vector3.Lerp(attackPos, originalLocalPos, p);
                transform.localScale = Vector3.Lerp(new Vector3(originalScale.x, originalScale.y, 1f), originalScale, p);
            }
            yield return null;
        }
        transform.localPosition = originalLocalPos;
        transform.localScale = originalScale;
    }
    
    IEnumerator DeathAnimation()
    {
        float elapsed = 0f;
        Vector3 startPos = transform.localPosition;
        Vector3 startScale = transform.localScale;
        float pivotY = 0.17f;
        float spriteHeight = spriteRenderer.sprite.bounds.size.y * startScale.y;
        float pivotOffset = (0.5f - pivotY) * spriteHeight;
        
        while (elapsed < deathDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / deathDuration;
            float currentHeight = Mathf.Lerp(startScale.y, 0f, t);
            transform.localScale = new Vector3(startScale.x, currentHeight, 1f);
            float heightDiff = startScale.y - currentHeight;
            transform.localPosition = startPos - new Vector3(0f, heightDiff * pivotOffset, 0f);
            Color c = Color.Lerp(originalColor, Color.black, Mathf.Clamp01(t * 1.5f));
            c.a = Mathf.Lerp(1f, 0f, Mathf.Clamp01((t - 0.3f) * 2f));
            spriteRenderer.color = c;
            yield return null;
        }
        Destroy(gameObject);
    }
}