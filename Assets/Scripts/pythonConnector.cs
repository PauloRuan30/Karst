using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;
using System;
using System.Diagnostics;
using System.IO;
using System.Collections;
using UnityEditor;


public class pythonConnector : MonoBehaviour
{
    // Start is called before the first frame update
    Thread mThread;
    public string connectionIP = "127.0.0.1";
    public int connectionPort = 25001;
    IPAddress localAdd;
    TcpListener listener;
    TcpClient client;
    string receivedMsg;
    bool rightJoystickButtonPressed;
    bool running;
    static byte[] myWriteBuffer;
    Process pythonProcess;
    public static string modelName;
    [SerializeField] int fadeTime;
    [SerializeField] TMPro.TMP_Text startLabel;
    [Header("Debug")]
    [SerializeField] bool UseDefaultModel = false; //in order to be able to work directly from scene 2
    [SerializeField] bool UsePythonCode = true; //use .py instead of exe for development process


    public delegate void DataReceivedEventHandler(string data);
    public static event DataReceivedEventHandler OnDataReceived;
    private static bool modelIsLoaded;
    private bool drawingEnabled;

    private void Start()
    {
        modelIsLoaded = false;
        drawingEnabled = false;
        if (UseDefaultModel)
            modelName = "debug_model";
        gameObject.GetComponent<Draw>().enabled = false;
        ThreadStart ts = new ThreadStart(GetInfo);
        mThread = new Thread(ts);
        mThread.Start();
        CreatePythonProcess();
        GameObject.Find("LoadingMessage").SetActive(true);
        SetDataToSend(Encoding.UTF8.GetBytes(modelName));

        //player.PlayerUpdateMana(-1000);
        //player.PlayerUpdateManaRegenerationSpeed(0);

        //StartCoroutine(PauseForSeconds(fadeTime));
    }

    private void Update()
    {
        if (modelIsLoaded && !drawingEnabled)
        {
            drawingEnabled = true;
            GameObject.Find("LoadingMessage").SetActive(false);
            gameObject.GetComponent<Draw>().enabled = true;
        }
    }
    private IEnumerator PauseForSeconds(float seconds)
    {

        Time.timeScale = 0;
        gameObject.GetComponent<Draw>().enabled = false;
        gameObject.GetComponent<InGameMenuScript>().enabled = false;
        yield return new WaitForSecondsRealtime(seconds);
        gameObject.GetComponent<Draw>().enabled = true;
        gameObject.GetComponent<InGameMenuScript>().enabled = true;
        Time.timeScale = 1;

        if (startLabel != null)
            startLabel.enabled = false;

        UnityEngine.Debug.Log(modelName + "sent");
    }
    private void CreatePythonProcess()
    {
        ProcessStartInfo startInfo = new ProcessStartInfo();

        if (UsePythonCode)
        {
            startInfo.FileName = "python";
            startInfo.Arguments = "\"" + Path.Combine(Application.dataPath, "Scripts", "UnityPython", "UnityPython.py") + "\"";
            startInfo.UseShellExecute = true;

        }
        else
        {
            startInfo.FileName = Path.Combine(Application.dataPath, "Scripts", "UnityPython", "UnityPython.exe");
            startInfo.UseShellExecute = false;
        }

        startInfo.WorkingDirectory = Path.Combine(Application.dataPath);
        //startInfo.FileName = "python";
        //startInfo.Arguments = "\"" + Path.Combine(Application.dataPath, "Scripts", "UnityPython", "UnityPython.py") + "\"";
        startInfo.CreateNoWindow = true;
        pythonProcess = Process.Start(startInfo);
    }

    private void OnApplicationQuit()
    {
        running = false;
        listener?.Stop();
        client?.Close();
        mThread?.Abort();
        pythonProcess?.Kill();
        KillProcessByName("UnityPython");
    }

    private void OnDestroy()
    {
        // Stop the listener when the object is destroyed
        running = false; // Make sure the loop in GetInfo stops
        listener?.Stop(); // Close the listener if it exists
        client?.Close();  // Close the client connection if it's open
        mThread?.Abort(); // Stop the background thread
        pythonProcess?.Kill();
        KillProcessByName("UnityPython"); // Kill the Python process
    }

    void GetInfo()
    {
        localAdd = IPAddress.Parse(connectionIP);
        listener = new TcpListener(IPAddress.Any, connectionPort);
        listener.Start();

        client = listener.AcceptTcpClient();

        running = true;
        while (running)
        {
            if (myWriteBuffer != null)
            { //if there is data to send
                NetworkStream nwStream = client.GetStream();
                byte[] buffer = new byte[client.ReceiveBufferSize];
                nwStream.Write(myWriteBuffer, 0, myWriteBuffer.Length); //Sending the data in Bytes to Python

                int bytesRead = 0;
                try
                {
                    bytesRead = nwStream.Read(buffer, 0, client.ReceiveBufferSize); //Getting data in Bytes from Python
                }
                catch { }
                if (bytesRead > 0)
                {
                    string dataReceived = Encoding.UTF8.GetString(buffer, 0, bytesRead); //Converting byte data to string

                    if (dataReceived != null)
                    {
                        if (dataReceived == "Model loaded")
                        {
                            modelIsLoaded = true;
                            UnityEngine.Debug.Log("Model loaded");
                        }
                        else if (dataReceived == "Model updated")
                        { //if model updated
                            UnityEngine.Debug.Log("Model updated");
                        }
                        else //prediction result
                        {
                            OnDataReceived?.Invoke(dataReceived);
                        }
                    }
                }
                myWriteBuffer = null;
            }
        }
        listener.Stop();
    }

    public static void SetDataToSend(byte[] image)
    {
        myWriteBuffer = image;
    }

    public static void KillProcessByName(string processName)
    {
        try
        {
            // Get all processes with the specified name
            var processes = Process.GetProcessesByName(processName);

            // Iterate through the list and kill each process
            foreach (var process in processes)
            {
                process.Kill();
                process.WaitForExit(); // Ensures the process is fully terminated
                process.Dispose(); // Clean up resources
            }

            if (processes.Length > 0)
            {
                UnityEngine.Debug.Log($"{processes.Length} process(es) named {processName} were killed.");
            }
            else
            {
                UnityEngine.Debug.Log($"No processes named {processName} found.");
            }
        }
        catch (System.Exception ex)
        {
            UnityEngine.Debug.LogError($"An error occurred while trying to kill process {processName}: {ex.Message}");
        }
    }
}

