using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

public class PortalManager : MonoBehaviourManager<PortalManager>
{
    public event Action OnEnterSameScenePortalEvent;
    public event Action OnExitSameScenePortalEvent;

    private Dictionary<string, List<Portal>> PortalDict = 
        new Dictionary<string, List<Portal>>();

    public void PortalManagerInit()
    {
        PortalDict.Clear();
        OnEnterSameScenePortalEvent = null;
        OnExitSameScenePortalEvent = null;

        foreach (var portal in FindObjectsOfType<Portal>())
        {
            if (PortalDict.ContainsKey(portal.SceneName))
                PortalDict[portal.SceneName].Add(portal);
            else
                PortalDict.Add(portal.SceneName, new List<Portal> { portal });

            portal.PortalInit();
        }
    }

    public override void Init()
    {
        base.Init();
        if (IsNotSubManagerInit) return;

        PortalManagerInit();
    }

    public void PortalEnter(string targetSceneName, int targetPortalId)
    {
        if (targetSceneName.Equals(SceneManager.GetActiveScene().name))
        {
            PortalEnterSameScene(targetSceneName, targetPortalId);
        }
        else
        {
            LoadSceneManager.Instance.LoadSceneAsync(targetSceneName, 
                () => PortalEnterSameScene(targetSceneName, targetPortalId));
        }
    }

    public void PortalExit()
    {
        OnExitSameScenePortalEvent?.Invoke();
    }

    private void PortalEnterSameScene(string targetSceneName, int targetPortalId)
    {
        if (!PortalDict.ContainsKey(targetSceneName)) return;

        Portal targetPortal = PortalDict[targetSceneName].Find(
            portal => portal.PortalId == targetPortalId);

        if (targetPortal == null) return;

        OnEnterSameScenePortalEvent?.Invoke();

        PlayerManager.Instance.gameObject.transform.SetPositionAndRotation(
            targetPortal.PortalPoint.transform.position,
            targetPortal.PortalPoint.transform.rotation
        );

        OnExitSameScenePortalEvent?.Invoke();
    }
}
