
using JetBrains.Annotations;
using System;
using System.Linq;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Components;
using VRC.SDK3.StringLoading;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

public class RemoteWorldListLoader : UdonSharpBehaviour
{
    public VRCUrl url;
    public int worldIDFieldNumber = 1;
    public int worldTitleFieldNumber = 5;
    public int worldAuthorFieldNumber = 4;
    public int worldDescriptionFieldNumber = 0;
    public int worldSizeFieldNumber = 0;
    public int numberOfFieldsPerLine = 6;
    public VRCPortalMarker portalMarker;
    int worldsPerPage = 0;
    public Button[] navigationButtons;

    string[][] allWorldsInfo;
    int currentPage;
    int currentPageSize;
    int nPages;

    object[] uiFields;

    string[] dummyInfo;

    const int previousButton = 0;
    const int nextButton = 1;

    public WorldPanelElement[] uiElements;
    
    GameObject portalMarkerObject;

    string lastError;
    int lastErrorCode;
    public TMPro.TextMeshProUGUI errorOutput;

    private void ShowWorld(int uiElementIndex, int worldIndex)
    {
        string[] worldInfo = allWorldsInfo[worldIndex];
        var uiElement = uiElements[uiElementIndex];
        int worldInfoLength = worldInfo.Length;

        foreach (object uiFieldArray in uiFields)
        {
            object[] uiField = (object[]) uiFieldArray;

            int tsvFieldIndex = (int)uiField[0];
            string panelVariable = (string)uiField[1];
            string panelUIMethod = (string)uiField[2];

            if ((tsvFieldIndex >= 0) & (tsvFieldIndex < worldInfoLength))
            {
                
                string worldField = worldInfo[tsvFieldIndex];
                uiElement.SetProgramVariable(panelVariable, worldField);
                uiElement.SendCustomEvent(panelUIMethod);
            }
        }
    }

    private void HidePage()
    {
        foreach (var uiElement in uiElements)
        {
            uiElement.gameObject.SetActive(false);
        }
    }

    private void DisplayElement(int pageElement, int worldIndex)
    {
        Debug.Log($"Showing {worldIndex} in {pageElement}");
        string[] worldFields = allWorldsInfo[worldIndex];
        if (worldFields == dummyInfo)
        {
            return;
        }

        uiElements[pageElement].gameObject.SetActive(true);
        ShowWorld(pageElement, worldIndex);

    }

    private void ShowCurrentPage()
    {
        if (nPages == 0)
        {
            Debug.LogError("No page to show");
            return;
        }
        /* Handle Wrapping */
        int pageNumber = currentPage;
        pageNumber = pageNumber < nPages ? pageNumber : 0;
        pageNumber = pageNumber >= 0 ? pageNumber : nPages - 1;
        currentPage = pageNumber;

        int lastWorldIndex = allWorldsInfo.Length - 1;
        int computedStart = currentPage * worldsPerPage;
        int startWorldIndex = Mathf.Clamp(computedStart, 0, lastWorldIndex);
        int endWorldIndex = Mathf.Min(startWorldIndex + worldsPerPage, lastWorldIndex);

        for (
            int pageIndex = 0, currentWorldIndex = startWorldIndex;
            currentWorldIndex < endWorldIndex;
            pageIndex++, currentWorldIndex++)
        {
            DisplayElement(pageIndex, currentWorldIndex);
        }
        currentPageSize = endWorldIndex - startWorldIndex;
    }

    bool checkingForErrors = false;

    private void OnEnable()
    {
        /*int worldTitleFieldNumber = 5;
        int worldDescriptionFieldNumber = 6;
        int numberOfFieldsPerLine = 6;*/
        bool fieldsNumbersDontMatch =
            (worldIDFieldNumber > numberOfFieldsPerLine)
            | (worldTitleFieldNumber > numberOfFieldsPerLine)
            | (worldAuthorFieldNumber > numberOfFieldsPerLine);
        if (fieldsNumbersDontMatch)
        {
            Debug.LogError($"[{name}] [{this.GetType().Name}] The number of fields don't match.");
            Debug.LogError($"[{name}] [{this.GetType().Name}] Max field is {numberOfFieldsPerLine}");
            Debug.LogError($"[{name}] [{this.GetType().Name}] One of the field number exceed this number");
            gameObject.SetActive(false);
            return;
        }

        if (url == null)
        {
            Debug.LogError($"[{name}] [{this.GetType().Name}] url not set. Disabling.");
            gameObject.SetActive(false);
            return;
        }

        if (numberOfFieldsPerLine < 1)
        {
            Debug.LogError($"[{name}] [{this.GetType().Name}] Invalid nFields value. Disabling.");
            gameObject.SetActive(false);
            return;
        }

        if (worldIDFieldNumber < 1)
        {
            Debug.LogError($"[{name}] [{this.GetType().Name}] worldIDFieldNumber is invalid. Disabling.");
            gameObject.SetActive(false);
            return;
        }

        if (worldIDFieldNumber > numberOfFieldsPerLine)
        {
            Debug.LogError($"[{name}] [{this.GetType().Name}] The World ID field number is higher than the number of fields.");
            gameObject.SetActive(false);
            return;
        }

        if (portalMarker == null)
        {
            Debug.LogError($"[{name}] [{this.GetType().Name}] portalMarker not set. Disabling.");
            gameObject.SetActive(false);
            return;
        }

        uiFields = new object[]
        {
            new object[]
            {
                    worldIDFieldNumber - 1, nameof(WorldPanelElement.worldID), nameof(WorldPanelElement.WorldID)
            },
            new object[]
            {
                    worldTitleFieldNumber - 1, nameof(WorldPanelElement.worldTitle), nameof(WorldPanelElement.WorldTitle)
            },
            new object[]
            {
                    worldAuthorFieldNumber - 1, nameof(WorldPanelElement.worldAuthor), nameof(WorldPanelElement.WorldAuthor)
            },
            new object[]
            {
                    worldDescriptionFieldNumber - 1, nameof(WorldPanelElement.worldDescription), nameof(WorldPanelElement.WorldDescription)
            },
            new object[]
            {
                    worldSizeFieldNumber - 1, nameof(WorldPanelElement.worldSize), nameof(WorldPanelElement.WorldSize)
            }
        };

        
        dummyInfo = new string[numberOfFieldsPerLine];
        for (int i = 0; i < numberOfFieldsPerLine; i++)
        {
            dummyInfo[i] = "";
        }

        portalMarkerObject = portalMarker.gameObject;
        nPages = 0;
        currentPage = 0;
        currentPageSize = 0;
        allWorldsInfo = new string[0][];

        worldsPerPage = uiElements.Length;
        foreach (var uiElement in uiElements)
        {
            uiElement.remoteWorldLoader = this;
        }

        VRCStringDownloader.LoadUrl(url, (IUdonEventReceiver)this);

        if ((errorOutput != null) & (checkingForErrors == false))
        {
            checkingForErrors = true;
            SendCustomEvent(nameof(CheckForError));
        }

    }

    [HideInInspector]
    [UdonSynced]
    public string selectedWorldID;

    void OpenPortal()
    {
        if ((selectedWorldID != null) & (selectedWorldID != ""))
        {
            portalMarker.roomId = selectedWorldID;
            portalMarkerObject.SetActive(true);
        }

    }

    public override void OnDeserialization()
    {
        Debug.Log($"<color=orange>DESERIALIZED : Selected world ID {selectedWorldID}</color>");
        OpenPortal();
    }

    public override void OnOwnershipTransferred(VRCPlayerApi player)
    {
        RequestSerialization();
    }

    public void ActivePortal(string worldID)
    {
        selectedWorldID = worldID;
        OpenPortal();
        if (Networking.GetOwner(gameObject) != Networking.LocalPlayer)
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }
        else
        {
            RequestSerialization();
        }
    }

    public void ClosePortal()
    {
        portalMarkerObject.SetActive(false);
    }

    public void RefreshPageButtons()
    {
        navigationButtons[previousButton].gameObject.SetActive(currentPage > 0);
        navigationButtons[nextButton].gameObject.SetActive(currentPage < (nPages - 1));
    }

    public void NextPage()
    {
        HidePage();
        currentPage += 1;
        RefreshPageButtons();
        ShowCurrentPage();
    }

    public void PreviousPage()
    {
        HidePage();
        currentPage -= 1;
        RefreshPageButtons();
        ShowCurrentPage();
    }

    public override void OnStringLoadSuccess(IVRCStringDownload result)
    {
        string resultString = result.Result;

        var lines = resultString.Split('\n');
        var nLines = lines.Length;

        allWorldsInfo = new string[nLines][];

        for (int line = 0; line < nLines; line++)
        {
            var fields = lines[line].Split('\t');
            if (fields.Length < numberOfFieldsPerLine)
            {
                Debug.Log($"Not enough fields. Expected {numberOfFieldsPerLine} got {fields.Length}");
                fields = dummyInfo;
            }
            allWorldsInfo[line] = fields;
        }
        currentPage = 0;
        nPages = (nLines > 0 ? (nLines - 1) / worldsPerPage : 0) + 1;

        HidePage();
        RefreshPageButtons();
        ShowCurrentPage();
    }



    public override void OnStringLoadError(IVRCStringDownload result)
    {
        lastError = result.Error;
        lastErrorCode = result.ErrorCode;
    }

    void CheckForError()
    {

        if (lastError != null)
        {
            errorOutput.text =
                $"Failed to load {url}\nError code : {lastErrorCode}\nError : {lastError}\n";
            lastError = null;
        }
        SendCustomEventDelayedSeconds(nameof(CheckForError), 2);

    }
}
