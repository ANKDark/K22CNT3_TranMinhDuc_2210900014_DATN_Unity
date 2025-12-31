using UnityEngine;

[CreateAssetMenu(fileName = "New Skill", menuName = "Skills/Skill Data")]
public class SkillData : ScriptableObject
{
    [Header("Skill Info")]
    public string skillName;
    public Sprite skillIcon;
    [TextArea] public string skillDescription;

    [Header("Stats")]
    public float cooldownTime = 5f;
    public float damageMultiplier = 1f;
    public float duration = 0f;
    public float range = 0f;
    public int manaCost = 10;

    [Header("VFX & SFX")]
    public GameObject vfxPrefab;
    public float spawnDelay = 0.3f;
    public AudioClip sfxSound;

}
