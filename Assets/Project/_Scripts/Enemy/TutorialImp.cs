
public class TutorialImp : EnemyAI {

    bool textFadedIn;

	protected override void Start () {

        base.Start();

        distanceToLook = 1.5f;
	}

    protected override void Update ()
    {
        if (setter.targetASTAR != null)
        {
            if (!textFadedIn)
            {
                StartCoroutine(TutorialManager.instance.MyBlock.FadeIn());
                StartCoroutine(TutorialManager.instance.MyRiposte.FadeIn());
                StartCoroutine(TutorialManager.instance.MyHit.FadeIn());
                StartCoroutine(TutorialManager.instance.MyChargedHit.FadeIn());
                textFadedIn = true;
            }
        }

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
            healthbar.gameObject.SetActive(false);
        }
    }
}
