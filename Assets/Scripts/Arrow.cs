using UnityEngine;

public class Arrow : MonoBehaviour
{
    public Villager hunter;
    public Animal target;
    public float timer;

    // Update is called once per frame
    void Update()
    {
        if (timer >= 1) return;
        timer += Time.deltaTime * 2;
        Vector3 position = Vector3.Lerp(hunter.transform.position, target.transform.position, timer);
        position.y += 1 - Mathf.Pow((2 * timer) - 1, 2);
        Vector3 direction = position - transform.position;
        transform.position = position;
        transform.rotation = Quaternion.LookRotation(direction);
        transform.Rotate(new Vector3(-90, 0, 0), Space.Self);
        if (timer >= 1)
        {
            target.Damage(1);
            Destroy(gameObject);
        }
    }
}
