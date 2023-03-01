
using System.Net;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class WorldPanelElement : UdonSharpBehaviour
{
    public RemoteWorldListLoader remoteWorldLoader;

    public bool desactivateFieldsOnReset = true;

    public GameObject mainPanelObject;

    public TMPro.TextMeshProUGUI uiWorldID;
    public TMPro.TextMeshProUGUI uiAuthor;
    public TMPro.TextMeshProUGUI uiTitle;
    public TMPro.TextMeshProUGUI uiDescription;
    public TMPro.TextMeshProUGUI uiSize;

    /* FIXME : This is not tested at the moment */
    [HideInInspector]
    public TMPro.TextMeshProUGUI[] uiTags;

    public void ResetElement(string field, TMPro.TextMeshProUGUI element)
    {
        if (element != null)
        {
            element.text = "";
            element.gameObject.SetActive(!desactivateFieldsOnReset);
        }
        field = null;
    }

    private void OnEnable()
    {
        /* Get us out of the Update loop, we don't need it */
        enabled = false;
    }

    public void UIReset()
    {
        ResetElement(worldID, uiWorldID);
        ResetElement(worldAuthor, uiAuthor);
        ResetElement(worldTitle, uiTitle);
        ResetElement(worldDescription, uiDescription);
        ResetElement(worldSize, uiSize);

        foreach (var uiTag in uiTags)
        {
            if (uiTag != null)
            {
                uiTag.text = "";
                uiTag.gameObject.SetActive(!desactivateFieldsOnReset);
            }
        }
    }

    void SetField(TMPro.TextMeshProUGUI field, string value)
    {
        if ((field != null) & (value != null))
        {
            field.gameObject.SetActive(true);
            field.text = value;
        }
    }

    [HideInInspector]
    public string worldID;
    public void WorldID()
    {
        Debug.Log($"[PanelElement] ID : {worldID}");
        SetField(uiWorldID, worldID);
    }

    [HideInInspector]
    public string worldAuthor;
    public void WorldAuthor() => SetField(uiAuthor, worldAuthor);

    [HideInInspector]
    public string worldTitle;
    public void WorldTitle() => SetField(uiTitle, worldTitle);

    [HideInInspector]
    public string worldDescription;
    public void WorldDescription() => SetField(uiDescription, worldDescription);

    [HideInInspector]
    public string worldSize;
    public void WorldSize() => SetField(uiSize, worldSize);

    [HideInInspector]
    public string[] tags;
    public void WorldTags()
    {
        if ((uiTags == null) | (tags == null))
        {
            return;
        }

        int minSize = Mathf.Min(uiTags.Length, tags.Length);

        /* Let's be nice here.
         * If you have a null tag element in your list, we'll skip to the next element.
         * If you have a null tag in your list, we'll skip to the next tag.
         */
        for (int uiIndex = 0, tagIndex = 0; (uiIndex < minSize) & (tagIndex < minSize);)
        {
            var uiTag = uiTags[uiIndex];
            if (uiTag == null)
            {
                uiIndex++;
                continue;
            }

            var tag = tags[tagIndex];
            if (tag == null)
            {
                tagIndex++;
                continue;
            }

            uiTag.text = tag;
            uiIndex++;
            tagIndex++;
        }
    }

    public override void Interact()
    {
        if (remoteWorldLoader == null)
        {
            Debug.LogWarning($"[{name}] [{this.GetType().Name}] BUG : Not opening the portal. Remote World Loader isn't set");
            return;
        }

        if (worldID == null)
        {
            Debug.LogWarning($"[{name}] [{this.GetType().Name}] Not opening the portal. The world ID isn't set.");
            return;
        }

        remoteWorldLoader.ActivePortal(worldID);
    }


}
