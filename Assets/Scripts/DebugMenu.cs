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
            case "teleport":
                ConsoleTeleport(command);
                break;
            case "velocity":
                ConsoleVelocity(command);
                break;
            case "angularVelocity":
                ConsoleAngularVelocity(command);
                break;
        }
    }

    void ConsoleHelp()
    {
        string[] write = {
            "--- COMMAND LIST ---",
            "teleport {object name} {x} {y} {z} {w}",
            "velocity {object name} {x} {y} {z} {w}",
            "angularVelocity {object name} {x} {y} {z}"
        };

        WriteToConsole(write);
    }

    void ConsoleTeleport(List<string> command)
    {
        if (command.Count < 6) {
            return;
        }

        GameObject targetObj = GameObject.Find(command[1]);

        if (targetObj == null) {
            WriteToConsole("error: teleport object not found");
            return;
        }

        Transform4D targetTransform = targetObj.GetComponent<Transform4D>();

        if (targetTransform == null) {
            WriteToConsole("error: teleport object does not have transform");
            return;
        }

        Vector4 newPos = new Vector4();

        for (int i = 0; i < 4; i++)
        {
            if (float.TryParse(command[i+2], out float result)) {
                newPos[i] = result;
            } else {
                WriteToConsole("error: invalid position input (cannot parse to float)");
                return;
            }
        }
        
        if (newPos == Vector4.zero) {
            WriteToConsole("error: invalid position input (zero vector)");
            return;
        }

        newPos = newPos.normalized;

        Rotor moveRotor = new Rotor(targetTransform.positionNorm, newPos);
        targetTransform.MoveRotor(moveRotor);

        WriteToConsole("teleported "+command[1]+" to "+newPos);
    }

    void ConsoleVelocity(List<string> command)
    {
        if (command.Count < 6) {
            return;
        }

        GameObject targetObj = GameObject.Find(command[1]);

        if (targetObj == null) {
            WriteToConsole("error: velocity object not found");
            return;
        }

        Rigidbody4D targetRb = targetObj.GetComponent<Rigidbody4D>();

        if (targetRb == null) {
            WriteToConsole("error: velocity object does not have rigibody");
            return;
        }

        Vector4 newVel = new Vector4();

        for (int i = 0; i < 4; i++)
        {
            if (float.TryParse(command[i+2], out float result)) {
                newVel[i] = result;
            } else {
                WriteToConsole("error: invalid velocity input (cannot parse to float)");
                return;
            }
        }

        float mag = newVel.magnitude;
        newVel = UFunc.ProjectToVectorNormal(newVel, targetRb.transform4.positionNorm);

        newVel = newVel.normalized * mag;

        targetRb.SetVelocity(newVel);

        WriteToConsole("set "+command[1]+" velocity to "+newVel);
    }

    void ConsoleAngularVelocity(List<string> command)
    {
        if (command.Count < 5) {
            return;
        }

        GameObject targetObj = GameObject.Find(command[1]);

        if (targetObj == null) {
            WriteToConsole("error: angularVelocity object not found");
            return;
        }

        Rigidbody4D targetRb = targetObj.GetComponent<Rigidbody4D>();

        if (targetRb == null) {
            WriteToConsole("error: angularVelocity object does not have rigibody");
            return;
        }

        Vector3 newAngular = new Vector3();

        for (int i = 0; i < 3; i++)
        {
            if (float.TryParse(command[i+2], out float result)) {
                newAngular[i] = result;
            } else {
                WriteToConsole("error: invalid angularVelocity input (cannot parse to float)");
                return;
            }
        }

        targetRb.SetAngularVelocity(newAngular);

        WriteToConsole("set "+command[1]+" angularVelocity to "+newAngular);
    }
}
