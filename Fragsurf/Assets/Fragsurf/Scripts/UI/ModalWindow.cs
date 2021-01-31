using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Fragsurf.UI
{
    public class ModalWindow : MonoBehaviour
    {

        [SerializeField]
        private TMP_Text _titleText;
        [SerializeField]
        private Button _closeButton;

        private void Start()
        {
            var modal = GetComponentInParent<UGuiModal>();
            if (!modal)
            {
                return;
            }

            _titleText.text = modal.Name;

            if (!_closeButton)
            {
                return;
            }

            _closeButton.onClick.AddListener(() =>
            {
                modal.Close();
            });
        }

    }
}

