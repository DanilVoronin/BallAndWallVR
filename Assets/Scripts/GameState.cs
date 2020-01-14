using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameState : MonoBehaviour {

    public static GameState GameStateObj;   //Статическая ссылка к этому объекту

    public float Acceleration = 0;  //Метры в секунду за секунду
    public float V_0 = 0;           //Начальная скорость

    public float V = 0;             //Скорость движения
    public float StartT  = 0;       //Время игры

    public Rigidbody ObjectMove;    //Игровая сфера
    public Transform Arm_L;         //Левая рука
    public Transform Arm_R;         //Правая рука
    public GameObject Box;          //Элемент стены
    public List<GameObject> BoxList = new List<GameObject>();

    public int _State = 0;          //Состояние автомата 

    //Вектора и ограничители
    private Vector3 s_0 = new Vector3(-5,0,10);
    private Vector3 s_1 = new Vector3(5, 6, 10);

    private Vector3 e_0 = new Vector3(-0.5f, 1, -1);
    private Vector3 e_1 = new Vector3(0.5f, 1.5f, -1);

    private Vector3 p_0 = Vector3.zero;
    private Vector3 p_1 = Vector3.zero;

    IEnumerator CreateBox(float time, Vector3 pos)
    {
        yield return new WaitForSeconds(time);
        BoxList.Add(Instantiate(Box, pos, Quaternion.identity));
    }
    IEnumerator StartGame(float time)
    {
        yield return new WaitForSeconds(time);
        _State = 2;
    }

    public AudioClip Contact;
    public AudioClip GameOver;

    AudioSource _AudioSource;

    public AudioSource _Fon;
    public TextMesh _Text;
    public TextMesh _Number;
    int _NumberInt = 0;

    void Start()
    {
        GameStateObj = this;
        _AudioSource = gameObject.AddComponent<AudioSource>();
        _AudioSource.loop = false;
        _AudioSource.playOnAwake = false;
        _AudioSource.volume = 0.75f;

        SteamVR_ControllerManager steam = GameObject.FindObjectOfType<SteamVR_ControllerManager>();
        if (steam != null)
        {
            steam.UpdateTargets();
            steam.left.SetActive(true);
            steam.right.SetActive(true);
        }
    }
    void LateUpdate ()
    {
        switch (_State)
        {
            case 0:
                //-------------------------------------------
                //Ожидаем запуска игры
                //Из этого состояним выход в 1
                //-------------------------------------------
                //Необходимо взвести _State = 1
                //-------------------------------------------
                break;
            case 1:
                //-------------------------------------------
                //Событие старт игры
                //Корректируем данные, запускаем анимации
                _State = -1;
                _Text.gameObject.SetActive(false);
                _NumberInt = 0; 
                _Number.text = _NumberInt.ToString();
                _Fon.Play();

                foreach (GameObject g in BoxList)
                    Destroy(g);
                BoxList.Clear();


                float x;
                int x_max = (int)s_1.x;

                float y;
                int y_max = (int)s_1.y;

                float time = 0;

                for (y = s_0.y + 0.5f; y <= y_max; y++)
                {
                    for (x = s_0.x; x <= x_max; x++)
                    {
                        StartCoroutine(CreateBox(time, new Vector3(x, y, 10.5f)));
                        time += 0.03f;
                    }
                }

                V = V_0;
                StartT = Time.time;
                //-------------------------------------------
                //Запускаем игру
                StartCoroutine(StartGame(time));
                break;
            case 2:
                //-------------------------------------------
                //Выбираем вектор направления p_0
                //-------------------------------------------
                p_0 = SetVector(s_0, s_1, ObjectMove);
                _State = 3;
                _AudioSource.clip = Contact;
                _AudioSource.Play();
                //-------------------------------------------
                break;
            case 3:
                //-------------------------------------------
                //Перемещаем шар к позиции s
                //-------------------------------------------
                AccelerationMove(ObjectMove, p_0);
                //Достигли позиции s
                if (ObjectMove.transform.position.z > s_0.z)
                {
                    //ObjectMove.transform.position = new Vector3(ObjectMove.transform.position.x, ObjectMove.transform.position.y, s_0.z);
                    _State = 4;
                }
                //-------------------------------------------
                break;
            case 4:
                //-------------------------------------------
                //Выбираем вектор направления p_1
                //-------------------------------------------
                p_1 = SetVector(e_0, e_1, ObjectMove);
                _State = 5;
                _AudioSource.clip = Contact;
                _AudioSource.Play();

                _NumberInt += 10;
                _Number.text = _NumberInt.ToString();
                //-------------------------------------------
                break;
            case 5:
                //-------------------------------------------
                //Перемещаем шар к позиции e
                //-------------------------------------------
                AccelerationMove(ObjectMove, p_1);
                //В этом состоянии необходимо сбрасывать _State = 2
                //Проверяем руку игрока (Попадает ли она в зону) (пока без проверки на вылет)

                //-------------------------------------------
                //Проверка 1
                //-------------------------------------------
                //Достигли позиции e (Проверка попадания в зону)3
                Bounds BoundsBita = new Bounds(Arm_L.position, Arm_L.localScale + new Vector3(0.2f,0.2f,0.2f));
                if (BoundsBita.Contains(ObjectMove.position))
                    _State = 2;

                BoundsBita = new Bounds(Arm_R.position, Arm_R.localScale + new Vector3(0.2f, 0.2f, 0.2f));
                if (BoundsBita.Contains(ObjectMove.position))
                    _State = 2;
                //-------------------------------------------
                //Проверка 2
                //-------------------------------------------
                float AccelerationFrame = Acceleration / (1 / Time.deltaTime);
                float NextV = V + (AccelerationFrame * (Time.time - StartT));

                RaycastHit hit;
                if (Physics.Raycast(ObjectMove.transform.position, p_1, out hit, 50))
                {
                    //Если попали в контроллеры
                    if (hit.collider.name == "SphereL" || hit.collider.name == "SphereR")
                    {
                        if (NextV > Vector3.Distance(ObjectMove.transform.position, hit.point))
                        {
                            //Лучше сделать тригер и переключать в следующем кадре, но ...
                            ObjectMove.transform.position = hit.point;
                            _State = 2;
                        }
                    }
                }
                //-------------------------------------------
                //Проверка проиргыша
                //-------------------------------------------
                if (ObjectMove.transform.position.z < e_0.z)
                {
                    ObjectMove.transform.position = new Vector3(ObjectMove.transform.position.x, ObjectMove.transform.position.y, e_0.z);
                    _State = 6;
                }
                //-------------------------------------------
                break;
            case 6:
                //-------------------------------------------
                //Конец игры
                //-------------------------------------------
                _AudioSource.clip = GameOver;
                _AudioSource.Play();
                _Text.gameObject.SetActive(true);
                _Fon.Stop();

                _State = 0;
                //-------------------------------------------
                break;
            default:
                break;
        }
    }

    void AccelerationMove(Rigidbody move, Vector3 direction)
    {
        //Переводим ускорение
        float AccelerationFrame = Acceleration / (1 / Time.deltaTime);
        V += (AccelerationFrame * (Time.time - StartT));

        //Проверка прохождения объекта через сферу

        move.MovePosition(move.position + direction * V);

        move.transform.Rotate(Vector3.up, (Time.time - StartT) * V);
    }
    Vector3 SetVector(Vector3 angle_0, Vector3 angle_1, Rigidbody obj)
    {
        float x = Random.Range(angle_0.x, angle_1.x);
        float y = Random.Range(angle_0.y, angle_1.y);
        return (new Vector3(x, y, angle_0.z) - obj.position).normalized;
    }
}
