using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellsNeededManaCanvas : MonoBehaviour
{
    [SerializeField] TMPro.TMP_Text fireballManaText;
    [SerializeField] TMPro.TMP_Text shieldManaText;
    [SerializeField] TMPro.TMP_Text teleportManaText;
    [SerializeField] TMPro.TMP_Text meteorManaText;
    [SerializeField] TMPro.TMP_Text healManaText;
    [SerializeField] TMPro.TMP_Text frostbeamManaText;
    [SerializeField] TMPro.TMP_Text summonManaText;

    void Start()
    {
        fireballManaText.text = CastSystem.FireBoltManaCost.ToString();
        shieldManaText.text = CastSystem.ShieldManaCost.ToString();
        teleportManaText.text = CastSystem.PortalManaCost.ToString();
        meteorManaText.text = CastSystem.MeteorManaCost.ToString();
        healManaText.text = CastSystem.HealManaCost.ToString();
        frostbeamManaText.text = CastSystem.FrostBeamManaCost.ToString();
        summonManaText.text = CastSystem.SummonManaCost.ToString();
    }

    void Update()
    {
        
    }
}
