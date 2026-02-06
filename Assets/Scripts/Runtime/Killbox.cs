using UnityEngine;

namespace Game.Runtime
{
    public class Killbox : MonoBehaviour
    {
        [SerializeField] private Vector3 _respawnPosition = new(0f, 1f, 0f);

        private void OnTriggerEnter(Collider other)
        {
            var characterController = other.GetComponent<CharacterController>();

            if (characterController == null)
                return;

            characterController.enabled = false;
            characterController.transform.position = _respawnPosition;
            characterController.enabled = true;
        }
    }
}
