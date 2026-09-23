using System;
using UnityEngine;

public class DesitionTreeNPC : MonoBehaviour
{
    public Transform enemy;
    public Transform home;
    public float enemyDangerDistance;
    public int hunger;
    public int energy;
    public int food;

    public float speed;

    public delegate void MiDelegateVoid();
    public delegate int MiDelegateParams(string texto);

    bool hasWeapon;

    public MiDelegateVoid ejecutable;
    private MiDelegateVoid AlClickear;

    private IDecisionNode rootNode;

    void Start()
    {
        /*//ejecutable = Huir;
        //ejecutable += Sleep;
        //
        //ejecutable.Invoke();
        //
        //AlClickear = Roll;

        //GameState.Instance.ExcecuteDelegate(ejecutable);*/

        SetTree();
    }

    // Update is called once per frame
    void Update()
    {
        rootNode.Execute();

        /*//if (Input.GetKeyDown(KeyCode.Mouse0))
        //{
        //    if (hasWeapon) Attack();
        //    else Roll();
        //}
        //AlClickear.Invoke();*/

        /*if(Vector3.Distance(enemy.position, transform.position) <= enemyDangerDistance)
        {
            Huir();
        }
        else
        {
            if (GameState.Instance.isDayTime)
            {
                if (hunger > 30)
                {
                    if (food > 0)
                    {
                        Eat();
                    }
                    else
                    {
                        Harvest();
                    }
                }
                else
                {
                    ChopWood();
                }
            }
            else
            {
                if (energy > 0)
                {
                    PlayGames();
                }
                else
                {
                    Sleep();
                }
            }
        }*/
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, enemyDangerDistance);
    }

    private void SetTree()
    {
        var comer = new DecisionActionNode(Eat);
        var huir = new DecisionActionNode(Flee);
        var cosechar = new DecisionActionNode(Harvest);
        var talar = new DecisionActionNode(ChopWood);
        var viciar = new DecisionActionNode(PlayGames);
        var dormir = new DecisionActionNode(Sleep);

        //Aca uso "() => GameState.Instance.isDayTime" que es un metodo anonimo o Lambda
        //los parentesis son para parametros y en este caso no recibe ninguno "()"
        // => se conoce como "Goes to" e indica el comienzo de la/las lineas de ejecución
        // no usamos return porque la única linea que existe define el tipo de retorno

        var tengoComida = new DecisionQuestionNode(() => food > 0, comer, cosechar);
        var tengoEnergia = new DecisionQuestionNode(() => energy > 0, viciar, dormir);
        var tengoHambre = new DecisionQuestionNode(() => hunger > 30, tengoComida, talar);
        var esDeDia = new DecisionQuestionNode(() => GameState.Instance.isDayTime, tengoHambre, tengoEnergia);
        var peligroCerca = new DecisionQuestionNode(IsDangerClose, huir, esDeDia);

        rootNode = peligroCerca;
    }

    //() => GameState.Instance.isDayTime;
    bool IsDangerClose()
    {
        return Vector3.Distance(enemy.position, transform.position) <= enemyDangerDistance;
    }

    public void ObtainWeapon()
    {
        hasWeapon = true;
        AlClickear = Attack;
    }
    private void Roll()
    {
        throw new NotImplementedException();
    }
    private void Attack()
    {
        throw new NotImplementedException();
    }
    private void Sleep()
    {
        MoveTowards(home);
    }
    private void PlayGames()
    {
        MoveTowards(home);
    }
    private void ChopWood()
    {
        Debug.Log("Talo");
    }
    private void Harvest()
    {
        Debug.Log("Cosecho");
    }
    private void Eat()
    {
        MoveTowards(home);
    }
    private void Flee()
    {
        MoveInDirection(transform.position - enemy.position);
    }
    private void MoveTowards(Transform point) => MoveTowards(point.position);
    private void MoveTowards(Vector3 point)
    {
        var dir = point - transform.position;
        MoveInDirection(dir);
        
    }
    private void MoveInDirection(Vector3 dir)
    {
        if (dir.sqrMagnitude < 0.1f) return;
        transform.position += dir.normalized * speed * Time.deltaTime;
    }
}
