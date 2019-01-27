using UnityEngine;

namespace Game
{
    public class CritterModel : PlayerBehaviour
    {
        public Transform Model;

        private ICritterController controller;
        private Quaternion rotationDifference;

        public void Start()
        {
            Model.gameObject.SetActive(!Player.NetworkPlayer.IsSelf);
            controller = GetComponentInParent<ICritterController>();

            //rotationDifference = controller.Mover.NeckBone.transform.rotation * Quaternion.Inverse(controller.Mover.Head.transform.rotation);
        }

        public void Update()
        {
            Vector3 eulerAngles = controller.Mover.GetHead().transform.eulerAngles;
            eulerAngles = new Vector3(0, eulerAngles.y, 0);
            transform.rotation = Quaternion.Euler(eulerAngles);
        }

        //public void LateUpdate()
        //{
            //controller.Mover.NeckBone.transform.rotation = controller.Mover.Head.transform.rotation * rotationDifference;
        //}
    }
}