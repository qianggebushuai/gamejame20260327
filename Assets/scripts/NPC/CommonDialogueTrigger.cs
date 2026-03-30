using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommonDialogueTrigger : MonoBehaviour
{
    [Header("NPC±Í ∂")]
    public string npcId;
    public int max = 2;
    public int now = 1;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.tag== "Player")
        {
            if (now < max)
            {
                TriggerDialogue();
                now++;
            }

        }

    }
    public void TriggerDialogue()
    {
        if (DialogueManager.Instance.IsDialogueActive)
            return;

        DialogueManager.Instance.StartDialogueWithNPC(npcId);
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
