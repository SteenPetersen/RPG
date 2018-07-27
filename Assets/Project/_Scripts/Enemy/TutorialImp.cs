using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialImp : EnemyAI {

	protected override void Start () {

        base.Start();

	}

    protected override void Update () {

        if (StoryManager.tutorialConversation == 3)
        {
            //Debug.Log("running update " + StoryManager.tutorialConversation);
            if (GameDetails.ripostes == 0 ||
                GameDetails.blocks == 0 ||
                GameDetails.hits == 0 ||
                GameDetails.fullChargeHits == 0)
            {
                base.Update();
            }
            else
            {
                setter.targetASTAR = null;
                canMove = false;
                anim.SetBool("Scared", true);
                myStats.shielded = true;
            }
        }
    }
}
