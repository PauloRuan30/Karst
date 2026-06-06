using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEngine.Windows;

public class CastSystem : MonoBehaviour
{
    [SerializeField] GameObject FireBallPrefab;
    public static int FireBoltManaCost = 5;
    [SerializeField] GameObject FrostBeamPrefab;
    public static int FrostBeamManaCost = 15;
    [SerializeField] GameObject MeteorPrefab;
    public static int MeteorManaCost = 50;
    [SerializeField] GameObject HealPrefab;
    public static int HealManaCost = 30;
    [SerializeField] GameObject ShieldPrefab;
    public static int ShieldManaCost = 20;
    [SerializeField] GameObject SummonPrefab;
    public static int SummonManaCost = 30;
    [SerializeField] GameObject PortalEnterPrefab;
    public static int PortalManaCost = 40;
    public float teleportDistance = 15f;

    [SerializeField] GameObject PortalExitPrefab;
    [SerializeField] float teleportOffset = 5f;
    [SerializeField] GameObject Tip;
    [SerializeField] GameObject PlayerModel;
    [SerializeField] GameObject XRorigin;


    [SerializeField] LayerMask groundLayer;// Layer mask to filter ground objects
    [SerializeField] LayerMask worldLayer;
    [SerializeField] GameObject PredictionMenuCanvas;
    [SerializeField] GameObject rayInteractor;

    [Header("Debug info")]
    [SerializeField] bool debug = false;
    [SerializeField] TMP_Text PredictionTextField;

    [Header("Skills Text")]
    [SerializeField] TMP_Text fireballText;
    [SerializeField] TMP_Text frostbeamText;
    [SerializeField] TMP_Text teleportText;
    [SerializeField] TMP_Text meteorText;
    [SerializeField] TMP_Text healText;
    [SerializeField] TMP_Text summonText;
    [SerializeField] TMP_Text shieldText;

    [SerializeField] AudioSource notEnoughManaSpeech;
    
    GameObject currentSkill;
    Player playerScript;
    bool isPaused = false;
    //// Update is called once per frame

    void Update()
    {

    }

    private void Start()
    {
        if (notEnoughManaSpeech) notEnoughManaSpeech.enabled = true;
        playerScript = GameObject.Find("PlayerModel").GetComponent<Player>();
        if (debug && PredictionTextField != null) {
            PredictionTextField.text = "['fireball 0.00%', 'frostbeam 0.00%', 'heal 0.00%', 'meteor 0.00%', 'others 0.00%', 'shield 0.00%', 'summon 0.00%', 'teleport 0.00%']";
        }
    }

    public void PrepareSkill(string PredictionArray)
    {
        Draw.syncContext.Post(_ => // This code here will run on the main thread
        {
            Debug.Log(PredictionArray);
            if(PredictionTextField && debug) PredictionTextField.text = PredictionArray;
            Dictionary<string, float> skillDict  = ParseSkillString(PredictionArray);
            var maxSkill = skillDict.Aggregate((l, r) => l.Value > r.Value ? l : r);
            var maxValue = maxSkill.Value;
            var maxSkillName = maxSkill.Key;
            if (maxValue > 90)
            {
                Cast(maxSkillName);
            }
            else 
            {
                fireballText.text = " fireball" + ": " + skillDict["fireball"].ToString() + "%";
                frostbeamText.text = " frostbeam" + ": " + skillDict["frostbeam"].ToString() + "%";
                teleportText.text = " teleport" + ": " + skillDict["teleport"].ToString() + "%";
                meteorText.text = " meteor" + ": " + skillDict["meteor"].ToString() + "%";
                healText.text = " heal" + ": " + skillDict["heal"].ToString() + "%";
                summonText.text = " summon" + ": " + skillDict["summon"].ToString() + "%";
                shieldText.text = " shield" + ": " + skillDict["shield"].ToString() + "%";
                Pause();
            }

        }, null);
    
    }

    public void Resume()
    {
        rayInteractor.SetActive(false);
        PredictionMenuCanvas.SetActive(false); // Hide the custom menu
        Draw drawScript = gameObject.GetComponent<Draw>();
        drawScript.enabled = true;
        Time.timeScale = 1f; // Resume the game
        isPaused = false;
    }

    void Pause()
    {
        Draw drawScript = gameObject.GetComponent<Draw>();
        drawScript.enabled = false;
        rayInteractor.SetActive(true);
        PredictionMenuCanvas.SetActive(true); // Show the custom menu
        Time.timeScale = 0f; // Pause the game
        isPaused = true;
        //PredictionsDropdown.Show();

    }
    public void IsFireball() 
    {
        pythonConnector.SetDataToSend(Encoding.UTF8.GetBytes("fireball"));
        Resume();
        Cast("fireball");
    }
    public void IsFrostbeam()
    {
        pythonConnector.SetDataToSend(Encoding.UTF8.GetBytes("frostbeam"));
        Resume();
        Cast("frostbeam");
    }
    public void IsTeleport()
    {
        pythonConnector.SetDataToSend(Encoding.UTF8.GetBytes("teleport"));
        Resume();
        Cast("teleport");
    }
    public void IsHeal()
    {
        pythonConnector.SetDataToSend(Encoding.UTF8.GetBytes("heal"));
        Resume();
        Cast("heal");
    }
    public void IsShield()
    {
        pythonConnector.SetDataToSend(Encoding.UTF8.GetBytes("shield"));
        Resume();
        Cast("shield"); 
    }
    public void IsSummon()
    {
        pythonConnector.SetDataToSend(Encoding.UTF8.GetBytes("summon"));
        Resume();
        Cast("summon");
    }
    public void IsMeteor()
    {
        pythonConnector.SetDataToSend(Encoding.UTF8.GetBytes("meteor"));
        Resume();
        Cast("meteor");
    }
    public void closeButtonHandler() 
    {
        Resume();
    }

    private void Cast(string maxSkillName)
    {
        switch (maxSkillName)
        {
            case "fireball":
                //currentSkill = Instantiate(FireBallPrefab, Tip.transform.position, Tip.transform.rotation);
                if (TryUseMana(FireBoltManaCost))
                {
                    Instantiate(FireBallPrefab, Tip.transform.position, Tip.transform.rotation);
                }
                break;
            case "frostbeam":
                //currentSkill = Instantiate(FrostBallPrefab, Tip.transform.position, Tip.transform.rotation);
                if (TryUseMana(FrostBeamManaCost))
                {
                    Instantiate(FrostBeamPrefab, Tip.transform.position, Tip.transform.rotation);
                }
                break;
            case "meteor":
                //currentSkill = Instantiate(MeteorPrefab, new Vector3(Player.transform.position.x,Player.transform.position.y+1,Player.transform.position.z), Player.transform.rotation);
                GameObject spawnPoint = GameObject.Find("MeteorSpawnPoint");
                if (TryUseMana(MeteorManaCost))
                {
                    GameObject meteor = Instantiate(MeteorPrefab, spawnPoint.transform.position, new Quaternion(0, 0, 0, 0));
                    meteor.transform.position += new Vector3(0, 3, 0);
                }
                break;
            case "heal":
                if (TryUseMana(HealManaCost))
                {
                    Instantiate(HealPrefab, new Vector3(PlayerModel.transform.position.x, PlayerModel.transform.position.y - 2 * PlayerModel.transform.localScale.y, PlayerModel.transform.position.z), PlayerModel.transform.rotation);
                }
                break;
            case "shield":
                if (TryUseMana(ShieldManaCost))
                {
                    Instantiate(ShieldPrefab, new Vector3(PlayerModel.transform.position.x, PlayerModel.transform.position.y - 2 * PlayerModel.transform.localScale.y, PlayerModel.transform.position.z), PlayerModel.transform.rotation);
                }
                break;
            case "summon":
                spawnPoint = GameObject.Find("MeteorSpawnPoint");
                float raycastHeight = 100f; // Height from which to cast the ray
                                            // Start the raycast from high above the ground
                Vector3 rayOrigin = new Vector3(spawnPoint.transform.position.x, raycastHeight, spawnPoint.transform.position.z);
                //GameObject meteor = Instantiate(MeteorPrefab, spawnPoint.transform.position, new Quaternion(0, 0, 0, 0));
                // Cast the ray downwards
                if (!Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, Mathf.Infinity, worldLayer))
                {
                    if (Physics.Raycast(rayOrigin, Vector3.down, out hit, Mathf.Infinity, groundLayer))
                    {
                        if (TryUseMana(SummonManaCost))
                        {
                            // Get the position where the ray hit the ground
                            Vector3 spawnPosition = hit.point;
                            // Instantiate the object at the hit point
                            Instantiate(SummonPrefab, spawnPosition, Quaternion.identity);
                        }
                    }
                }
                else
                {
                    Debug.Log("summon" + "No ground found at position: " + spawnPoint.transform.position);
                }
                break;
            case "teleport":
                TryTeleport();
                break;
            default:
                break;
        }
    }

    private void TryTeleport()
    {
        var spherecastRadius = 0.5f;
        var raycastHeight = 200f;
        // Calculate the target position based on the Tip's forward direction
        var playerModel = GameObject.Find("PlayerModel");
        Vector3 targetPosition = playerModel.transform.position + playerModel.transform.forward.normalized * teleportDistance;

        // Start the spherecast from above the target position
        Vector3 spherecastStart = new Vector3(targetPosition.x, raycastHeight, targetPosition.z);

        // Check for valid teleport area first
        if (Physics.SphereCast(spherecastStart, spherecastRadius, Vector3.down, out RaycastHit teleportHit, Mathf.Infinity, LayerMask.GetMask("Teleport")))
        {
            if (! Physics.SphereCast(teleportHit.point, spherecastRadius, Vector3.down, out RaycastHit worldHit, Mathf.Infinity, LayerMask.GetMask("World")))
            {
                // Now check for the ground position
                {
                    if(TryUseMana(PortalManaCost)){
                        // Valid teleportation spot found
                        Vector3 teleportPosition = new Vector3(teleportHit.point.x, teleportHit.point.y, teleportHit.point.z);
                        PlaceTeleport(teleportPosition); 
                    }
                }
            }
            else
            {
                Debug.Log("Ground not found below teleport area.");
            }
        }
        else
        {
            Debug.Log("No valid teleportation surface found.");
        }
    }

    private void PlaceTeleport(Vector3 position)
    {
        GameObject playerModel = GameObject.Find("PlayerModel");

        if (GameObject.Find("PortalEnter") != null && GameObject.Find("PortalExit")!=null) {
            Destroy(GameObject.Find("PortalExit"));
            Destroy(GameObject.Find("PortalEnter"));
        }
        GameObject greenPortal = Instantiate(PortalEnterPrefab,playerModel.transform.position + playerModel.transform.forward*1.2f, playerModel.transform.rotation);
        greenPortal.name = "PortalEnter";
        GameObject redPortal = Instantiate(PortalExitPrefab, position + 10f * Vector3.up, playerModel.transform.rotation);
        redPortal.name = "PortalExit";
    }
    private bool TryUseMana(int amount) {
        if (playerScript.GetCurrentMana() - amount < 0) {
            if (notEnoughManaSpeech && !notEnoughManaSpeech.isPlaying) {
                notEnoughManaSpeech.Play();
            }
            return false;
        }
        playerScript.PlayerUpdateMana(-amount);
        return true;
    }
    public static Dictionary<string, float> ParseSkillString(string skillString)
    {
        // Remove the outer brackets and single quotes
        skillString = skillString.Trim('[', ']', '\'');

        // Split the string into individual skill entries
        string[] skillEntries = skillString.Split(new string[] { "', '" }, StringSplitOptions.RemoveEmptyEntries);

        // Create a dictionary to store the parsed skills and their percentages
        Dictionary<string, float> skillDict = new Dictionary<string, float>();

        // Regex pattern to extract the skill name and percentage
        string pattern = @"^(?<name>\w+)\s(?<value>\d+\.\d+)%$";
        Regex regex = new Regex(pattern);

        foreach (string entry in skillEntries)
        {
            Match match = regex.Match(entry);
            if (match.Success)
            {
                string name = match.Groups["name"].Value;
                float value = float.Parse(match.Groups["value"].Value);
                skillDict[name] = value;
            }
            else
            {
                Console.WriteLine($"Failed to parse entry: {entry}");
            }
        }

        return skillDict;
    }

}
