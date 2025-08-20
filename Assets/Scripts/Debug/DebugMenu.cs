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
                consoleInput.ActivateInputField();
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

    void ClearConsole()
    {
        consoleLog.Clear();
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
                ConsoleHelp(command);
                break;
            case "clear":
                ClearConsole();
                break;
            case "gameRule":
                ConsoleGameRule(command);
                break;
            case "set":
                ConsoleSet(command);
                break;
            case "spawnRB":
                ConsoleSpawnRB(command);
                break;
            case "cloneRB":
                ConsoleCloneRB(command);
                break;
            default:
                WriteToConsole("command dont exist");
                break;
        }
    }

    void ConsoleHelp(List<string> command)
    {
        if (command.Count < 2)
        {
            string[] write = {
                "--- COMMAND LIST --- use \"help {command}\" for more info",
                "clear",
                "gameRule {rule} {state}",
                "set {object name} {property}",
                "spawnRB {object type} {object name} {posX} {posY} {posZ} {posW} {scale}",
                "cloneRB {object name} {number}",
                "--- random = $R[{lower},{upper}] ---"
            };

            WriteToConsole(write);
        }
        else
        {
            switch (command[1])
            {
                case "clear":
                    WriteToConsole(new string[] {
                        "--- COMMAND clear --- clears the console",
                    });
                    break;
                case "gameRule":
                    WriteToConsole(new string[] {
                        "--- COMMAND gameRule ---",
                        "gameRule {rule} {state}",
                        "gameRule disableShadows {state}"
                    });
                    break;
                case "set":
                    WriteToConsole(new string[] {
                        "--- COMMAND set --- sets property of object",
                        "set {object name} {property}",

                        "set {object name} position {x} {y} {z} {w}",
                        "set {object name} velocity {x} {y} {z} {w}",
                        "set {object name} angularVelocity {x} {y} {z}",

                        "set {object name} rigidbody gravityScale {val}",
                        "set {object name} rigidbody gravity {x} {y} {z} {w}",
                        "set {object name} rigidbody mass {val}",
                        "set {object name} rigidbody angularMassMult {val}",
                        "set {object name} rigidbody bounce {val}",
                        "set {object name} rigidbody friction {val}",

                        "set {object name} renderer color {r} {g} {b}",
                        "set {object name} renderer lit {bool}",
                        "set {object name} renderer doubleSideLit {bool}",
                        "set {object name} renderer castShadows {bool}",

                        "set {object name} light color {r} {g} {b}",
                        "set {object name} light intensity {val}"
                    });
                    break;
                case "spawnRB":
                    WriteToConsole(new string[] {
                        "--- COMMAND spawnRB --- spawns a dynamic physics object",
                        "spawnRB {object type} {object name} {posX} {posY} {posZ} {posW} {scale}",
                        "object types --- ball"
                    });
                    break;
                case "cloneRB":
                    WriteToConsole(new string[] {
                        "--- COMMAND cloneRB --- creates copies of a object at a slight position offset",
                        "cloneRB {object name}",
                        "cloneRB {object name} {number}"
                    });
                    break;
            }
        }
    }

    void ConsoleGameRule(List<string> command)
    {
        if (command.Count < 3) {
            WriteToConsole("error: not enough parameters");
            return;
        } 

        string rule = command[1];

        bool ruleState = ParseToBool(command[2]);

        command.RemoveAt(0);
        command.RemoveAt(0);
        command.RemoveAt(0);

        switch (rule)
        {
            case "disableShadows":
                SetDisableShadows();
                break;
            default:
                WriteToConsole("error: gameRule dont exist");
                break;
        }

        void SetDisableShadows()
        {
            LightHandlerS lightHandler = FindObjectOfType<LightHandlerS>();

            if (lightHandler == null) {
                WriteToConsole("error: lightHandler dont exist");
                return;
            }

            lightHandler.disableShadows = ruleState;

            WriteToConsole("set gameRule disableShadows to "+ruleState);
        }
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
        command.RemoveAt(0);

        switch (commandType)
        {
            case "position":
                SetPosition(targetObj, command);
                break;
            case "velocity":
                SetVelocity(targetObj, command);
                break;
            case "angularVelocity":
                SetAngularVelocity(targetObj, command);
                break;
            case "rigidbody":
                SetRigidbodyProperty(targetObj, command);
                break;
            case "renderer":
                SetRendererProperty(targetObj, command);
                break;
            case "light":
                SetLightProperty(targetObj, command);
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

    void SetAngularVelocity(GameObject targetObj, List<string> command)
    {
        if (command.Count < 3) {
            WriteToConsole("error: not enough parameters");
            return;
        }

        Rigidbody4D targetRB = targetObj.GetComponent<Rigidbody4D>();
        if (targetRB == null) {
            WriteToConsole("error: object does not have rigidbody");
            return;
        }

        Vector3 newAngVel = ParseToVector3(command[0],command[1],command[2]);

        targetRB.SetAngularVelocity(newAngVel);

        WriteToConsole("set "+targetObj.name+" angularVelocity to "+newAngVel);
    }

    void SetRigidbodyProperty(GameObject targetObj, List<string> command)
    {
        if (command.Count < 1) {
            WriteToConsole("error: not enough parameters");
            return;
        }

        Rigidbody4D targetRB = targetObj.GetComponent<Rigidbody4D>();
        if (targetRB == null) {
            WriteToConsole("error: object does not have rigidbody");
            return;
        }

        string property = command[0];
        command.RemoveAt(0);

        switch (property)
        {
            case "gravityScale":
                SetGravityScale(command);
                break;
            case "gravity":
                SetGravity(command);
                break;
            case "mass":
                SetMass(command);
                break;
            case "angularMassMult":
                SetAngularMassMult(command);
                break;
            case "bounce":
                SetBounce(command);
                break;
            case "friction":
                SetFriction(command);
                break;
            default:
                WriteToConsole("error: rigidbody property dont exist");
                break;
        }

        void SetGravityScale(List<string> command)
        {
            if (command.Count < 1) {
                WriteToConsole("error: not enough parameters");
                return;
            }

            float newVal = ParseNum(command[0]);
            targetRB.gravityScale = newVal;

            WriteToConsole("set "+targetObj.name+" rigidbody gravityScale to "+newVal);
        }
        void SetGravity(List<string> command)
        {
            if (command.Count < 4) {
                WriteToConsole("error: not enough parameters");
                return;
            }

            Vector4 newVal = ParseToVector4(command[0],command[1],command[2],command[3]);
            targetRB.gravity = newVal;

            WriteToConsole("set "+targetObj.name+" rigidbody gravity to "+newVal);
        }
        void SetMass(List<string> command)
        {
            if (command.Count < 1) {
                WriteToConsole("error: not enough parameters");
                return;
            }

            float newVal = ParseNum(command[0]);
            targetRB.mass = newVal;

            WriteToConsole("set "+targetObj.name+" rigidbody mass to "+newVal);
        }
        void SetAngularMassMult(List<string> command)
        {
            if (command.Count < 1) {
                WriteToConsole("error: not enough parameters");
                return;
            }

            float newVal = ParseNum(command[0]);
            targetRB.angularMassMult = newVal;

            WriteToConsole("set "+targetObj.name+" rigidbody angularMassMult to "+newVal);
        }
        void SetBounce(List<string> command)
        {
            if (command.Count < 1) {
                WriteToConsole("error: not enough parameters");
                return;
            }

            float newVal = ParseNum(command[0]);
            targetRB.bounce = newVal;

            WriteToConsole("set "+targetObj.name+" rigidbody bounce to "+newVal);
        }
        void SetFriction(List<string> command)
        {
            if (command.Count < 1) {
                WriteToConsole("error: not enough parameters");
                return;
            }

            float newVal = ParseNum(command[0]);
            targetRB.friction = newVal;

            WriteToConsole("set "+targetObj.name+" rigidbody friction to "+newVal);
        }
    }

    void SetRendererProperty(GameObject targetObj, List<string> command)
    {
        if (command.Count < 1) {
            WriteToConsole("error: not enough parameters");
            return;
        }

        Renderer4D targetRenderer = targetObj.GetComponent<Renderer4D>();
        if (targetRenderer == null) {
            WriteToConsole("error: object does not have renderer");
            return;
        }

        string property = command[0];
        command.RemoveAt(0);

        switch (property)
        {
            case "color":
                SetColor(command);
                break;
            case "lit":
                SetLit(command);
                break;
            case "doubleSideLit":
                SetDoubleSideLit(command);
                break;
            case "castShadows":
                SetCastShadows(command);
                break;
            default:
                WriteToConsole("error: renderer property dont exist");
                break;
        }

        void SetColor(List<string> command)
        {
            if (command.Count < 3) {
                WriteToConsole("error: not enough parameters");
                return;
            }

            Vector3 newColor = ParseToVector3(command[0],command[1],command[2]);
            targetRenderer.meshColor = new Color(newColor.x,newColor.y,newColor.z);

            WriteToConsole("set "+targetObj.name+" render color to "+newColor);
        }
        void SetLit(List<string> command)
        {
            if (command.Count < 1) {
                WriteToConsole("error: not enough parameters");
                return;
            }

            bool state = ParseToBool(command[0]);
            targetRenderer.lit = state;

            WriteToConsole("set "+targetObj.name+" render lit to "+state);
        }
        void SetDoubleSideLit(List<string> command)
        {
            if (command.Count < 1) {
                WriteToConsole("error: not enough parameters");
                return;
            }

            bool state = ParseToBool(command[0]);
            targetRenderer.doubleSideLit = state;

            WriteToConsole("set "+targetObj.name+" render doubleSideLit to "+state);
        }
        void SetCastShadows(List<string> command)
        {
            if (command.Count < 1) {
                WriteToConsole("error: not enough parameters");
                return;
            }

            bool state = ParseToBool(command[0]);
            targetRenderer.castShadows = state;

            WriteToConsole("set "+targetObj.name+" render castShadows to "+state);
        }
    }

    void SetLightProperty(GameObject targetObj, List<string> command)
    {
        if (command.Count < 1) {
            WriteToConsole("error: not enough parameters");
            return;
        }

        LightS targetLight = targetObj.GetComponent<LightS>();
        if (targetLight == null) {
            WriteToConsole("error: object does not have light");
            return;
        }

        string property = command[0];
        command.RemoveAt(0);

        switch (property)
        {
            case "color":
                SetColor(command);
                break;
            case "intensity":
                SetIntensity(command);
                break;
            default:
                WriteToConsole("error: light property dont exist");
                break;
        }

        void SetColor(List<string> command)
        {
            if (command.Count < 3) {
                WriteToConsole("error: not enough parameters");
                return;
            }

            Vector3 newColor = ParseToVector3(command[0],command[1],command[2]);
            targetLight.color = new Color(newColor.x,newColor.y,newColor.z);

            WriteToConsole("set "+targetObj.name+" light color to "+newColor);
        }
        void SetIntensity(List<string> command)
        {
            if (command.Count < 1) {
                WriteToConsole("error: not enough parameters");
                return;
            }

            float newIntensity = ParseNum(command[0]);
            targetLight.intensity = newIntensity;

            WriteToConsole("set "+targetObj.name+" light intensity to "+newIntensity);
        }
    }

    void ConsoleSpawnRB(List<string> command)
    {
        if (command.Count < 8) {
            WriteToConsole("error: not enough parameters");
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

    void ConsoleCloneRB(List<string> command)
    {
        if (command.Count < 2) {
            WriteToConsole("error: not enough parameters");
            return;
        }

        GameObject targetObj = GameObject.Find(command[1]);
        if (targetObj == null) {
            WriteToConsole("error: target object not found");
            return;
        }
        Rigidbody4D targetRB = targetObj.GetComponent<Rigidbody4D>();
        if (targetRB == null) {
            WriteToConsole("error: object does not have rigidbody");
            return;
        }

        int copies = 1;
        if (command.Count > 1) {
            copies = Mathf.FloorToInt(ParseNum(command[2]));
        }

        for (int i = 0; i < copies; i++)
        {
            Rigidbody4D newRB = Instantiate(targetRB);
            Transform4D newTransform = newRB.transform4;
            newTransform.MoveRelative(new Vector3(newTransform.scale,0,0)*(i+1));
            newTransform.gameObject.name = targetRB.gameObject.name + "_Clone" + (i+1).ToString();
        }

        WriteToConsole("cloned "+command[1]+" "+copies+" times");
    }

    Vector3 ParseToVector3(string s0, string s1, string s2)
    {
        string[] str = new string[] {s0,s1,s2};
        Vector3 outV = new Vector3();

        for (int i = 0; i < 3; i++)
        {
            outV[i] = ParseNum(str[i]);
        }

        return outV;
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

            for (int i = 3; i < str.Length-1; i++)
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
            if (float.TryParse(randStr2, out float result2)) {
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
    bool ParseToBool(string str)
    {
        if (str == "true") return true;
        return false;
    }
}
