using UnityEngine;

public class ChestThrow : MonoBehaviour
{
    [SerializeField] private AudioClip chestClip;

    public void PlayChestSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.Play3DSFX(chestClip, transform.position, minDistance: 5f);
        }
    }
}
