using UnityEngine;
using TMPro;
using Mirror;
using Steamworks;

public class PlayerStats : NetworkBehaviour
{
    [Header("Energy Stats")]
    public float maxEnergy = 100f;
    public float currentEnergy;
    public float energyDrainJump = 20f;
    public float energyDrainRun = 10f; // Per second
    public float energyRegenRate = 15f; // Per second when idle

    [SerializeField] private float regenDelay = 2f; // Seconds to wait before regen starts
    private float regenCooldownTimer = 0f;

    [Header("Health Stats")]
    public float maxHealth = 100f;
    public float currentHealth;

    [SerializeField] private float minFallSpeedForDamage = 10f;
    [SerializeField] private float maxFallSpeedForMaxDamage = 30f;
    [SerializeField] private float maxFallDamage = 100f; // Full health loss


    [Header("UI")]
    public TextMeshProUGUI energyText;
    public TextMeshProUGUI healthText;

    private void Start()
    {
        currentEnergy = maxEnergy;
        currentHealth = maxHealth;
    }

    private void Update()
    {
        if (regenCooldownTimer > 0)
        {
            regenCooldownTimer -= Time.deltaTime;
        }
        else
        {
            RegenerateEnergy();
        }

        UpdateUI(); // Always update the UI
    }


    public bool CanJump() => currentEnergy >= energyDrainJump;
    public bool CanRun() => currentEnergy >= energyDrainRun * Time.deltaTime;

    public void ConsumeEnergyForJump()
    {
        currentEnergy = Mathf.Max(currentEnergy - energyDrainJump, 0f);
        regenCooldownTimer = regenDelay;
    }

    public void ConsumeEnergyForRun()
    {
        currentEnergy = Mathf.Max(currentEnergy - energyDrainRun * Time.deltaTime, 0f);
        regenCooldownTimer = regenDelay;
    }

    private void RegenerateEnergy()
    {
        if (!Input.GetKey(KeyCode.Space) && !Input.GetKey(KeyCode.LeftShift))
        {
            currentEnergy = Mathf.Min(currentEnergy + energyRegenRate * Time.deltaTime, maxEnergy);
        }
    }

    private void UpdateUI()
    {
        if (energyText != null)
            energyText.text = $"Energy: {Mathf.RoundToInt(currentEnergy)}";
        if (healthText != null)
            healthText.text = $"Health: {Mathf.RoundToInt(currentHealth)}";
    }
    public void TakeFallDamage(float fallSpeed)
    {
        if (fallSpeed < minFallSpeedForDamage) return;

        float fallRatio = Mathf.InverseLerp(minFallSpeedForDamage, maxFallSpeedForMaxDamage, fallSpeed);
        float damage = fallRatio * maxFallDamage;

        currentHealth = Mathf.Max(currentHealth - damage, 0f);
        Debug.Log($"Ouch! Fall speed: {fallSpeed}, damage: {damage}");
    }

}
