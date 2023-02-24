
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
    public int numberOfFieldsPerLine = 6;
    public Slider worldIDIndex;
    public VRCPortalMarker portalMarker;
    const int worldsPerPage = 8;
    public TMPro.TextMeshProUGUI[] textElements;
    public RectTransform[] panels;
    public Button[] navigationButtons;

    string[][] allWorldsInfo;
    int currentPage;
    int currentPageSize;
    int nPages;

    int fieldWorldID = 0;
    int fieldWorldTitle = 4;
    int fieldWorldAuthor = 3;
    int nFields = 6;
    string[] dummyInfo;

    const int previousButton = 0;
    const int nextButton = 1;

    const int textTitle = 0;
    const int textAuthor = 1;
    const int uiElementsPerElement = 2;
   

    

    GameObject portalMarkerObject;

    private void HidePage()
    {
        foreach (var panel in panels)
        {
            panel.gameObject.SetActive(false);
        }
    }

    private void DisplayElement(int pageElement, int worldIndex)
    {
        int uiElementsIndex = pageElement * uiElementsPerElement;
        string[] worldFields = allWorldsInfo[worldIndex];
        if (worldFields == dummyInfo)
        {
            return;
        }

        var uiTitle = textElements[uiElementsIndex + textTitle];
        var uiAuthor = textElements[uiElementsIndex + textAuthor];

        uiTitle.text = worldFields[fieldWorldTitle];
        uiAuthor.text = worldFields[fieldWorldAuthor];
        panels[pageElement].gameObject.SetActive(true);
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

        Debug.Log($"Display from ({computedStart}) {startWorldIndex} to {endWorldIndex} (Last : {lastWorldIndex})");

        for (
            int pageIndex = 0, currentWorldIndex = startWorldIndex;
            currentWorldIndex < endWorldIndex;
            pageIndex++, currentWorldIndex++)
        {
            DisplayElement(pageIndex, currentWorldIndex);
        }
        currentPageSize = endWorldIndex - startWorldIndex;
    }

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
            enabled = false;
            return;
        }

        if (url == null)
        {
            Debug.LogError($"[{name}] [{this.GetType().Name}] url not set. Disabling.");
            enabled = false;
            return;
        }

        if (worldIDIndex == null)
        {
            Debug.LogError($"[{name}] [{this.GetType().Name}] textWithWorldID not set. Disabling.");
            enabled = false;
            return;
        }

        if (portalMarker == null)
        {
            Debug.LogError($"[{name}] [{this.GetType().Name}] portalMarker not set. Disabling.");
            enabled = false;
            return;
        }

        fieldWorldID = worldIDFieldNumber - 1;
        fieldWorldTitle = worldTitleFieldNumber - 1;
        fieldWorldAuthor = worldAuthorFieldNumber - 1;
        nFields = numberOfFieldsPerLine;
        dummyInfo = new string[nFields];
        for (int i = 0; i < nFields; i++)
        {
            dummyInfo[i] = "";
        }


        portalMarkerObject = portalMarker.gameObject;
        nPages = 0;
        currentPage = 0;
        currentPageSize = 0;
        allWorldsInfo = new string[0][];
        VRCStringDownloader.LoadUrl(url, (IUdonEventReceiver)this);
        //pageElements = new Array[worldsPerPage];
    }

    string GetWorldFromPageElement(int element)
    {
        if (element < 0)
        {
            Debug.LogWarning($"[{name}] [GetWorldFromPageElement] Negative index. ({element})");
            return "";
        }

        if (element > currentPageSize)
        {
            Debug.LogWarning($"[{name}] [GetWorldFromPageElement] Exceeding Page size ! ({element})");
            return "";
        }

        int worldIndex = (currentPage * worldsPerPage) + element;
        return (worldIndex < allWorldsInfo.Length ? allWorldsInfo[worldIndex][fieldWorldID] : "");

    }

    public void OpenPortal()
    {
        portalMarker.roomId = GetWorldFromPageElement((int)worldIDIndex.value);
        portalMarkerObject.SetActive(true);
    }

    public void ClosePortal()
    {
        portalMarkerObject.SetActive(false);
    }

    public void RefreshPageButtons()
    {
        navigationButtons[previousButton].gameObject.SetActive(currentPage > 0);
        Debug.Log($"{currentPage} < {nPages - 1}");
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
            if (fields.Length < 6)
            {
                Debug.Log($"Not enough fields. Expected 6 got {fields.Length}");
                fields = dummyInfo;
            }
            allWorldsInfo[line] = fields;
        }
        currentPage = 0;
        nPages = (nLines > 0 ? (nLines - 1) / worldsPerPage : 0) + 1;

        Debug.Log($"nPages : {nPages}");
        HidePage();
        RefreshPageButtons();
        ShowCurrentPage();
    }

    public override void OnStringLoadError(IVRCStringDownload result)
    {
        Debug.LogError("String loading failed !");
    }
}
