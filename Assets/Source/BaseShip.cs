using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public enum ShipState : int
{
    Orbiting,
    Moving,
    Attacking,
    Exploding,
    Dead
}

public class BaseShip : MonoBehaviour
{
    public string shipName = "LeShip";
    public float maxHP = 100.0f;
    public float damage = 5.0f;
    protected float hp = 100.0f;

    protected SpriteRenderer spRenderer;
    protected Collider2D shipCollider;

    protected BasePlanet rootPlanet;

    protected ShipState shipState = ShipState.Orbiting;

    protected const float kRadiusMinOffset = 0.1f;
    protected const float kRadiusMaxOffset = 0.3f;

    BasePlanet targetPlanet;
    float targetOrientation;
    Vector2 velocity;
    //BaseShip enemyShip;

    public bool IsDead
    {
        get
        {
            return shipState == ShipState.Dead;
        }
    }


    Sequence blinkSeq = null;
    protected float orbitDistance;
    protected float degree;
    public float angularSpeed = 2; // rad/s

    public List<float> multipliers = new List<float>();

    // Use this for initialization
    void Awake () {
        spRenderer = GetComponent<SpriteRenderer>();
        shipCollider = GetComponent<Collider2D>();
	}

    public void SetHQ(BasePlanet hq)
    {
        rootPlanet = hq;
        transform.SetParent(rootPlanet.transform.parent);
        orbitDistance = rootPlanet.defaultScale * (1.0f + UnityEngine.Random.Range(kRadiusMinOffset, kRadiusMaxOffset)) / 2.0f;
        degree = UnityEngine.Random.value * 2 * Mathf.PI;
        Debug.Log("Angle: " + degree + ", degrees: " + Mathf.Rad2Deg * degree);
        Vector2 polarPos = (Vector2)rootPlanet.transform.position + new Vector2(orbitDistance * Mathf.Cos(degree), orbitDistance * Mathf.Sin(degree));
        transform.rotation = Quaternion.AngleAxis(-90 + Mathf.Rad2Deg * degree, Vector3.forward);
        transform.position = polarPos;        
    }

    virtual protected void Start()
    {
        hp = maxHP;

    }
	
	// Update is called once per frame
    
	void Update ()
    {
        if(shipState != ShipState.Orbiting && shipState != ShipState.Moving)
        {
            return;
        }

        if (shipState == ShipState.Orbiting)
        {
            degree += angularSpeed * Time.deltaTime;
            Debug.Log("Angle: " + degree + ", degrees: " + Mathf.Rad2Deg * degree);
            Vector2 polarPos = (Vector2)rootPlanet.transform.position + new Vector2(orbitDistance * Mathf.Cos(degree), orbitDistance * Mathf.Sin(degree));
            transform.rotation = Quaternion.AngleAxis(-90 + Mathf.Rad2Deg * degree, Vector3.forward);
            transform.position = polarPos;
        }
        else
        {
            //Vector3 axis;
            //float deg;
            //transform.rotation.ToAngleAxis(out deg, out axis);
            //if (Mathf.Abs(targetOrientation - deg ) < 2.0f)
            //{
                velocity = (targetPlanet.transform.position - transform.position).normalized;
                transform.position += (Vector3)((3.0f * Time.deltaTime) * velocity);

                if (Vector2.Distance(transform.position, targetPlanet.transform.position) < 1)
                {
                    float multiplierBonus = 1f;
                    multipliers.ForEach((value) => multiplierBonus += value);
                    targetPlanet.Hit(damage * multiplierBonus);
                    multipliers.Clear();
                    Hit(hp);
                }
            //}
            //else
            //{
            //    Vector2 targetVector = targetPlanet.transform.position - transform.position;
            //    transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.AngleAxis(targetOrientation, Vector3.forward), Time.deltaTime * 5.0f);
            //}
        }

    }

    public bool Hit(float damage)
    {
        hp = Mathf.Max(0, hp - damage);
        if (Mathf.Approximately(hp, 0))
        {
            shipState = ShipState.Dead;
            return true;
        }
        else
        {
            if (blinkSeq == null)
            {
                blinkSeq = DOTween.Sequence();
                Color oldColour = spRenderer.color;
                blinkSeq.Append(spRenderer.DOFade(0.0f, 0.05f));
                blinkSeq.Join(spRenderer.DOBlendableColor(Color.red, 0.05f));
                blinkSeq.Append(spRenderer.DOFade(1.0f, 0.05f));
                blinkSeq.Join(spRenderer.DOBlendableColor(oldColour, 0.05f));
                blinkSeq.SetLoops(4);
                blinkSeq.AppendCallback(() => { spRenderer.color = oldColour; blinkSeq = null; });
            }
        }
        return false;
    }

    public void SetTarget(BasePlanet target)
    {
        targetPlanet = target;
        shipState = ShipState.Moving;
        Vector2 targetVector = target.transform.position - transform.position;

        targetOrientation = -90 + Mathf.Atan2(targetVector.y, targetVector.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(targetOrientation, Vector3.forward);
    }

    public void ApplyDamageBonus(float bonusMultiplier)
    {
        multipliers.Add(bonusMultiplier);
    }
}
