using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.InputSystem;

public class DebugMenu : MonoBehaviour
{
    public GameObject menuObject;

    public TMP_InputField consoleInput;
    public TextMeshProUGUI consoleLogText;

    public bool consoleActive;

    public PlayerInputActions inputs;

    [HideInInspector] public InputAction m_openConsole;
    [HideInInspector] public InputAction m_commandSubmit;
    bool consoleOpenPress {get{return m_openConsole.triggered;}}
    bool commandSubmit {get{return m_commandSubmit.triggered;}}

    public List<string> consoleLog = new List<string>();
    public int maxConsoleLength = 10;

    public DebugSpawnObjects_SO debugSpawnObjects;

    void OnEnable()
    {
        if (inputs == null) inputs = new PlayerInputActions();
        inputs.Debug.Enable();
        SetupInput();
    }
    void OnDisable()
    {
        inputs.Debug.Disable();
    }

    void SetupInput()
    {
        m_openConsole = inputs.Debug.OpenConsole;
        m_commandSubmit = inputs.Debug.CommandSubmit;
    }

    // Update is called once per frame
    void Update()
    {
        if (consoleOpenPress)
        {
            if (consoleActive) {
                CloseConsole();
            } else {
                OpenConsole();
            }
        }

        if (consoleActive)
        {
            if (commandSubmit)
            {
                ConsoleCommand(consoleInput.text);
                consoleInput.text = "";
            }
        }
    }

    void OpenConsole()
    {
        consoleActive = true;

        menuObject.SetActive(true);
        consoleInput.ActivateInputField();

        UpdateConsole();

        PlayerInput.disableInput = true;
    }

    void CloseConsole()
    {
        consoleActive = false;

        menuObject.SetActive(false);
        consoleInput.DeactivateInputField();

        PlayerInput.disableInput = false;
    }

    void WriteToConsole(string line)
    {
        WriteToConsole(new string[] {line});
    }
    void WriteToConsole(string[] lines)
    {
        for (int i = 0; i < lines.Length; i++)
        {
            consoleLog.Add(lines[i]);
            if (consoleLog.Count > maxConsoleLength) consoleLog.RemoveAt(0);
        }

        UpdateConsole();
    }

    void UpdateConsole()
    {
        string newText = "";
        for (int i = 0; i < consoleLog.Count; i++)
        {
            newText += consoleLog[i] + "\n";
        }

        consoleLogText.text = newText;
    }

    //console commands
    void ConsoleCommand(string command)
    {
        List<string> parse = UFunc.ParseByCharacter(command, " ", true);
        ConsoleCommand(parse);
    }

    void ConsoleCommand(List<string> command)
    {
        if (command.Count == 0) return;

        switch (command[0])
        {
            case "help":
                ConsoleHelp();
                break;
            case "set":
                ConsoleSet(command);
                break;
            case "spawnRB":
                ConsoleSpawnRB(command);
                break;
            default:
                WriteToConsole("command dont exist");
                break;
        }
    }

    void ConsoleHelp()
    {
        string[] write = {
            "--- COMMAND LIST ---",
            "teleport {object name} {x} {y} {z} {w}",
            "velocity {object name} {x} {y} {z} {w}",
            "angularVelocity {object name} {x} {y} {z}",
            "set {object name} position {x} {y} {z} {w}",
            "spawnRB {object type} {object name} {posX} {posY} {posZ} {posW} {scale}"
        };

        WriteToConsole(write);
    }

    void ConsoleSet(List<string> command)
    {
        if (command.Count < 3) {
            WriteToConsole("error: not enough parameters");
            return;
        }

        GameObject targetObj = GameObject.Find(command[1]);
        if (targetObj == null) {
            WriteToConsole("error: target object not found");
            return;
        }

        string commandType = command[2];

        command.RemoveAt(0);
        command.RemoveAt(0);

        switch (commandType)
        {
            case "position":
                SetPosition(targetObj, command);
                break;
            case "velocity":
                SetVelocity(targetObj, command);
                break;
            default:
                WriteToConsole("error: set command dont exist");
                break;
        }
    }

    void SetPosition(GameObject targetObj, List<string> command)
    {
        if (command.Count < 4) {
            WriteToConsole("error: not enough parameters");
            return;
        }

        Transform4D targetTransform = targetObj.GetComponent<Transform4D>();
        if (targetTransform == null) {
            WriteToConsole("error: object does not have transform");
            return;
        }

        Vector4 newPos = ParseToVector4(command[0],command[1],command[2],command[3]);

        if (newPos == Vector4.zero) newPos = new Vector4(0,0,0,1);
        newPos = newPos.normalized;

        targetTransform.MoveTo(newPos);

        WriteToConsole("set "+targetObj.name+" position to "+newPos);
    }

    void SetVelocity(GameObject targetObj, List<string> command)
    {
        if (command.Count < 4) {
            WriteToConsole("error: not enough parameters");
            return;
        }

        Rigidbody4D targetRB = targetObj.GetComponent<Rigidbody4D>();
        if (targetRB == null) {
            WriteToConsole("error: object does not have rigidbody");
            return;
        }

        Vector4 newVel = ParseToVector4(command[0],command[1],command[2],command[3]);

        if (newVel != Vector4.zero) {
            float velMag = newVel.magnitude;
            newVel = UFunc.ProjectToVectorNormal(newVel, targetRB.transform4.positionNorm);
            newVel = newVel * velMag/newVel.magnitude;
        }

        targetRB.SetVelocity(newVel);

        WriteToConsole("set "+targetObj.name+" velocity to "+newVel);
    }

    void ConsoleSpawnRB(List<string> command)
    {
        if (command.Count < 8) {
            return;
        }

        string spawnType = command[1];

        Rigidbody4D spawnPrefab = debugSpawnObjects.GetRigidbody(spawnType);

        if (!spawnPrefab) {
            WriteToConsole("error: spawn object dont exist");
            return;
        }

        Vector4 spawnPos = new Vector4();
        for (int i = 0; i < 4; i++)
        {
            if (float.TryParse(command[i+3], out float result)) {
                spawnPos[i] = result;
            } else {
                WriteToConsole("error: invalid position input (cannot parse to float)");
                return;
            }
        }

        if (spawnPos == Vector4.zero) spawnPos = new Vector4(0,0,0,1);
        spawnPos = spawnPos.normalized;

        Rigidbody4D spawnRb = Instantiate(spawnPrefab);
        Transform4D spawnTransform = spawnRb.transform4;

        spawnRb.gameObject.name = command[2];

        spawnTransform.MoveTo(spawnPos);
        
        float scale = 1;
        if (float.TryParse(command[7], out float result2)) {
            scale = result2;
        }
        spawnTransform.scale = scale;

        WriteToConsole("spawned "+command[1]+" \""+command[2]+"\" at "+spawnPos);
    }

    Vector4 ParseToVector4(string s0, string s1, string s2, string s3)
    {
        string[] str = new string[] {s0,s1,s2,s3};
        Vector4 outV = new Vector4();

        for (int i = 0; i < 4; i++)
        {
            outV[i] = ParseNum(str[i]);
        }

        return outV;
    }
    float ParseNum(string str)
    {
        if (str.Length > 4 && str[0] == '$' && str[1] == 'R' && str[2] == '[' && str[^1] == ']')
        {
            string randStr1 = "";
            string randStr2 = "";
            bool add2 = false;

            for (int i = 3; i < str.Length; i++)
            {
                if (str[i] == ',') {
                    add2 = true;
                } else {
                    if (!add2) {
                        randStr1 += str[i];
                    } else {
                        randStr2 += str[i];
                    }
                }
            }

            float rand1 = 0;
            float rand2 = 0;

            if (float.TryParse(randStr1, out float result1)) {
                rand1 = result1;
            }
            if (float.TryParse(randStr1, out float result2)) {
                rand2 = result2;
            }

            float randOut = Random.Range(rand1,rand2);
            return randOut;
        }

        float outF = 0;
        if (float.TryParse(str, out float resultF)) {
            outF = resultF;
        }
        return outF;
    }
}
